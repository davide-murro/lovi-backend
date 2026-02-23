using LoviBackend.Data;
using LoviBackend.Models.DbSets;
using LoviBackend.Models.Dtos.Auth;
using LoviBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LoviBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IConfiguration configuration,
            IWebHostEnvironment hostingEnvironment,
            IHttpClientFactory httpClientFactory
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // save user
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name
            };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Generate confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Build callback URL (frontend route)
            var callbackUrl = $"{_hostingEnvironment.ContentRootPath}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            /*
            // Send confirmation newEmail
            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your LOVI account",
                $"Welcome, {user.DisplayName}!<br><br>" +
                $"Please confirm your newEmail by clicking this link:<br>" +
                $"<a href='{callbackUrl}'>Confirm Email</a>"
            );
            */

            return NoContent();
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Token))
                return BadRequest("Invalid confirmation parameters.");

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound("User not found.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpPost("resend-confirm-email")]
        public async Task<IActionResult> ResendConfirmEmail(ResendConfirmEmailDto model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return BadRequest("Invalid parameters.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound("User not found.");

            // Generate confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Build callback URL (frontend route)
            var callbackUrl = $"{_hostingEnvironment.ContentRootPath}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            /*
            // Send confirmation newEmail
            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your LOVI account",
                $"Welcome, {user.DisplayName}!<br><br>" +
                $"Please confirm your newEmail by clicking this link:<br>" +
                $"<a href='{callbackUrl}'>Confirm Email</a>"
            );
            */

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, [FromHeader(Name = "X-DeviceId")] string deviceId)
        {
            // check if the DeviceId header is present
            if (string.IsNullOrEmpty(deviceId))
            {
                return BadRequest("Device ID is required.");
            }

            // check user
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null || (await _userManager.CheckPasswordAsync(user, dto.Password)) == false)
            {
                return Unauthorized("Invalid login");
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                List<IdentityError> errors = new List<IdentityError>();
                errors.Add(new IdentityError
                {
                    Code = "EmailNotConfirmed",
                    Description = "The email is not confirmed."
                });
                return BadRequest(errors);
            }

            // save login
            user.LoggedInAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // creating the necessary claims
            List<Claim> authClaims = [
                new (ClaimTypes.Name, user.UserName!),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // unique id for token
                new (ClaimTypes.NameIdentifier, user.Id)    // Add the user's ID to the claims
            ];

            // adding roles to the claims. So that we can get the user role from the token.
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            // generating access token
            var token = _tokenService.GenerateAccessToken(authClaims);
            string refreshToken = _tokenService.GenerateRefreshToken();

            // save refreshToken with exp date in the database
            var tokenInfo = _context.TokenInfos.FirstOrDefault(t => t.UserName == user.UserName && t.DeviceId == deviceId);

            // If tokenInfo is null for the user, create a new one
            var expireAt = DateTime.UtcNow.AddMonths(int.Parse(_configuration["Jwt:ExpireAfterInMonths"]!));
            if (tokenInfo == null)
            {
                tokenInfo = new TokenInfo
                {
                    UserName = user.UserName!,
                    RefreshToken = refreshToken,
                    ExpiredAt = expireAt,
                    DeviceId = deviceId
                };
                _context.TokenInfos.Add(tokenInfo);
            }
            // Else, update the refresh token and expiration
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.ExpiredAt = expireAt;
            }

            await _context.SaveChangesAsync();

            // set refresh token cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = _hostingEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict,
                Path = "/",
                Expires = tokenInfo.ExpiredAt
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            return Ok(new TokenDto
            {
                AccessToken = token
            });
        }

        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto dto, [FromHeader(Name = "X-DeviceId")] string deviceId)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Provider) || string.IsNullOrEmpty(dto.AccessToken))
                return BadRequest("Provider and access token are required.");

            if (string.IsNullOrEmpty(deviceId))
                return BadRequest("Device ID is required.");

            var provider = dto.Provider.ToLowerInvariant();
            string? email = null;
            string? name = null;
            string? providerUserId = null;

            var client = _httpClientFactory.CreateClient();

            try
            {
                switch (provider)
                {
                    case "google":
                    {
                        var req = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
                        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", dto.AccessToken);
                        var res = await client.SendAsync(req);
                        if (!res.IsSuccessStatusCode)
                            return BadRequest("Invalid Google token.");

                        var payload = await res.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(payload);
                        var root = doc.RootElement;
                        email = root.TryGetProperty("email", out var e) ? e.GetString() : null;
                        name = root.TryGetProperty("name", out var n) ? n.GetString() : null;
                        providerUserId = root.TryGetProperty("sub", out var s) ? s.GetString() : null;
                        break;
                    }
                    case "spotify":
                    {
                        var req = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
                        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", dto.AccessToken);
                        var res = await client.SendAsync(req);
                        if (!res.IsSuccessStatusCode)
                            return BadRequest("Invalid Spotify token.");

                        var payload = await res.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(payload);
                        var root = doc.RootElement;
                        providerUserId = root.TryGetProperty("id", out var spid) ? spid.GetString() : null;
                        name = root.TryGetProperty("display_name", out var d) ? d.GetString() : null;
                        email = root.TryGetProperty("email", out var em) ? em.GetString() : null;
                        break;
                    }
                    case "facebook":
                    {
                        var fbUrl = $"https://graph.facebook.com/me?fields=id,name,email&access_token={Uri.EscapeDataString(dto.AccessToken)}";
                        var res = await client.GetAsync(fbUrl);
                        if (!res.IsSuccessStatusCode)
                            return BadRequest("Invalid Facebook token.");

                        var payload = await res.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(payload);
                        var root = doc.RootElement;
                        providerUserId = root.TryGetProperty("id", out var idp) ? idp.GetString() : null;
                        name = root.TryGetProperty("name", out var nn) ? nn.GetString() : null;
                        email = root.TryGetProperty("email", out var ee) ? ee.GetString() : null;
                        break;
                    }
                    case "instagram":
                    {
                        // Instagram Graph API (Basic Display) does not return email. Require email for account creation.
                        var igUrl = $"https://graph.instagram.com/me?fields=id,username&access_token={Uri.EscapeDataString(dto.AccessToken)}";
                        var res = await client.GetAsync(igUrl);
                        if (!res.IsSuccessStatusCode)
                            return BadRequest("Invalid Instagram token.");

                        var payload = await res.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(payload);
                        var root = doc.RootElement;
                        providerUserId = root.TryGetProperty("id", out var igid) ? igid.GetString() : null;
                        name = root.TryGetProperty("username", out var igu) ? igu.GetString() : null;

                        // Instagram won't provide email via this API. Require client to provide email if needed.
                        return BadRequest("Instagram login requires an email which is not provided by Instagram Basic Display API.");
                    }
                    default:
                        return BadRequest("Unsupported external provider.");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (string.IsNullOrEmpty(email))
                return BadRequest("External provider did not return an email address.");

            if (string.IsNullOrEmpty(providerUserId))
                return BadRequest("External provider did not return a provider user id.");

            // Find or create local user
            var user = await _userManager.FindByEmailAsync(email!);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = name ?? "",
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return BadRequest(createResult.Errors);
            }

            // Link external login
            var userExisting = await _userManager.FindByLoginAsync(dto.Provider, providerUserId); 
            if (userExisting != null && userExisting.Id != user.Id)
                return BadRequest("This external account is already linked to a different user.");
            if (userExisting == null)
            {
                var loginInfo = new UserLoginInfo(dto.Provider, providerUserId, dto.Provider);
                var addLogin = await _userManager.AddLoginAsync(user, loginInfo);
                if (!addLogin.Succeeded)
                    return BadRequest(addLogin.Errors);
            }

            // Update last login
            user.LoggedInAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // create claims and tokens (same as login)
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // persist refresh token
            var tokenInfo = _context.TokenInfos.FirstOrDefault(t => t.UserName == user.UserName && t.DeviceId == deviceId);
            var expireAt = DateTime.UtcNow.AddMonths(int.Parse(_configuration["Jwt:ExpireAfterInMonths"]!));
            if (tokenInfo == null)
            {
                tokenInfo = new TokenInfo
                {
                    UserName = user.UserName!,
                    RefreshToken = refreshToken,
                    ExpiredAt = expireAt,
                    DeviceId = deviceId
                };
                _context.TokenInfos.Add(tokenInfo);
            }
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.ExpiredAt = expireAt;
            }
            await _context.SaveChangesAsync();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = _hostingEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict,
                Path = "/",
                Expires = tokenInfo.ExpiredAt
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            return Ok(new TokenDto { AccessToken = accessToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromHeader(Name = "X-DeviceId")] string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                    return BadRequest("Device ID is required.");

                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                    return BadRequest("Missing refresh token.");

                var tokenInfo = await _context.TokenInfos
                    .SingleOrDefaultAsync(t =>
                        t.RefreshToken == refreshToken &&
                        t.DeviceId == deviceId);

                if (tokenInfo == null || tokenInfo.ExpiredAt <= DateTime.UtcNow)
                    return Unauthorized("Invalid refresh token.");

                // Get user
                var user = await _userManager.FindByNameAsync(tokenInfo.UserName);
                if (user == null)
                    return Unauthorized();

                // Create fresh claims
                var roles = await _userManager.GetRolesAsync(user);

                // create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

                // generate tokens
                var newAccessToken = _tokenService.GenerateAccessToken(claims);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                tokenInfo.RefreshToken = newRefreshToken; // rotating the refresh token
                tokenInfo.RefreshedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // set refresh token cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = _hostingEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict,
                    Path = "/",
                    Expires = tokenInfo.ExpiredAt
                };
                Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

                return Ok(new TokenDto
                {
                    AccessToken = newAccessToken
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromHeader(Name = "X-DeviceId")] string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                    return BadRequest("Device ID is required.");

                var userName = User.Identity!.Name;

                var tokenInfo = await _context.TokenInfos.SingleOrDefaultAsync(t => t.UserName == userName && t.DeviceId == deviceId);
                if (tokenInfo == null)
                {
                    return BadRequest("Token not found for this device.");
                }

                _context.TokenInfos.Remove(tokenInfo);
                await _context.SaveChangesAsync();

                // Delete the refresh token cookie so browser no longer sends it
                var deleteCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = _hostingEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(-1)
                };
                Response.Cookies.Delete("refreshToken", deleteCookieOptions);

                return Ok(true);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // Revoke all refresh tokens for the current user (all devices)
        [HttpPost("revoke-all")]
        [Authorize]
        public async Task<IActionResult> RevokeAll()
        {
            try
            {
                var userName = User.Identity!.Name;

                var tokenInfos = await _context.TokenInfos.Where(u => u.UserName == userName).ToListAsync();
                if (tokenInfos.Count == 0)
                {
                    return BadRequest("Tokens not found for this user.");
                }

                _context.TokenInfos.RemoveRange(tokenInfos);
                await _context.SaveChangesAsync();

                // Delete the refresh token cookie so browser no longer sends it
                var deleteCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = _hostingEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(-1)
                };
                Response.Cookies.Delete("refreshToken", deleteCookieOptions);

                return Ok(true);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        // POST: api/auth/change-email
        [HttpPost("change-email")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto model)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.CheckPasswordAsync(user, model.Password) == false)
            {
                return Unauthorized("Invalid password");
            }

            var currentEmail = user.Email;
            var newEmail = model.NewEmail;

            if (string.Equals(currentEmail, newEmail, StringComparison.OrdinalIgnoreCase))
            {

                List<IdentityError> errors = new List<IdentityError>();
                errors.Add(new IdentityError
                {
                    Code = "ChangeSameMail",
                    Description = "The new email is the same as the current one."
                });
                return BadRequest(errors);
            }

            if (await _userManager.FindByEmailAsync(newEmail) != null)
            {
                List<IdentityError> errors = new List<IdentityError>();
                errors.Add(new IdentityError
                {
                    Code = "DuplicateEmail",
                    Description = "Email is already taken."
                });
                return BadRequest(errors);
            }

            // Update the newEmail and security stamp using UserManager
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var callbackUrl = $"{_hostingEnvironment.ContentRootPath}/auth/confirm-change-email?userId={id}&newEmail={newEmail}&token={encodedToken}";

            /*
            await _emailService.SendEmailAsync(
                currentEmail,
                "Email Address Changing",
                $"Your newEmail address on LOVI it s about to change from {currentEmail} to {newEmail}. If this wasn't you, please contact support."
            );
            await _emailService.SendEmailAsync(
                newEmail,
                "Confirm Your New Email",
                $"Please confirm your new email address by clicking here: <a href='{callbackUrl}'>Confirm Email</a>"
            );
            */

            return NoContent();
        }

        // POST: api/auth/confirm-change-email
        [HttpPost("confirm-change-email")]
        public async Task<IActionResult> ConfirmChangeEmail(ConfirmChangeEmailDto model)
        {

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            string currentEmail = user.Email!;
            string newEmail = model.NewEmail;
            string token = model.Token;

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ChangeEmailAsync(user, newEmail, decodedToken);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.SetUserNameAsync(user, newEmail);
            await _userManager.UpdateSecurityStampAsync(user);

            /*
            await _emailService.SendEmailAsync(
                currentEmail,
                "Email Address Changed",
                $"Your newEmail address on LOVI has been changed from {currentEmail} to {newEmail}. If this wasn't you, please contact support."
            );
            await _emailService.SendEmailAsync(
                newEmail,
                "Email Address Changed",
                $"Your newEmail address on LOVI has been changed from {currentEmail} to {newEmail}. If this wasn't you, please contact support."
            );
            */

            return NoContent();
        }

        // POST: api/auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // 1. Core Identity method: Verifies old password and hashes the new one
            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                // Handle common errors like "Incorrect password" or password policy violations
                return BadRequest(result.Errors);
            }

            // 2. Update security stamp (optional, but good practice to invalidate old tokens)
            await _userManager.UpdateSecurityStampAsync(user);

            /*// 3. Send confirmation newEmail
            await _emailService.SendEmailAsync(
                user.Email,
                "Password Changed Successfully",
                "Your account password has been successfully updated. If you did not make this change, please contact support immediately."
            );*/

            return NoContent();
        }


        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var email = model.Email;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _userManager.FindByEmailAsync(email);

            // SECURITY NOTE: Always return 204/200 OK even if the user is not found.
            // This prevents an attacker from enumerating valid user emails.
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Log the attempt (optional)
                return NoContent();
            }

            // 1. Generate the Password Reset Token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 2. Encode the necessary parameters for the URL
            // Token must be URL-encoded because it contains special characters
            var encodedToken = UrlEncoder.Default.Encode(token);
            var encodedEmail = UrlEncoder.Default.Encode(user.Email!); // Use newEmail for simplicity in routing

            // 3. Construct the Reset URL pointing to your Angular app
            // The Angular route should look something like: /auth/forgot-password?email=...&token=...
            var callbackUrl = $"{_hostingEnvironment.ContentRootPath}/auth/forgot-password?email={encodedEmail}&token={encodedToken}";

            /*// 4. Send the Email
            var subject = "Password Reset Request";
            var body = $"Please reset your password by clicking here: <a href='{callbackUrl}'>Reset Password</a>";

            await _emailService.SendEmailAsync(
                user.Email,
                subject,
                body
            );*/

            // 5. Return success (or NoContent)
            return NoContent();
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            // 1. Basic input validation
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword))
            {
                return BadRequest("All fields (newEmail, token, new password) are required.");
            }

            // 2. Find the user
            var user = await _userManager.FindByEmailAsync(model.Email);

            // SECURITY NOTE: If the user doesn't exist, return success/NoContent 
            // to avoid confirming user existence based on the response.
            if (user == null)
            {
                return NoContent();
            }

            // 3. Reset the password using the token
            // The token MUST be decoded before passing it to ResetPasswordAsync
            var decodedToken = Uri.UnescapeDataString(model.Token);

            // This method validates the token, checks for expiration, and updates the password
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (result.Succeeded)
            {
                /*
                // Optional: Send confirmation newEmail
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Password Reset Confirmed",
                    "Your password has been successfully reset. You can now log in with your new password."
                );*/
                return NoContent();
            }

            // 4. Handle Identity errors (e.g., weak password, invalid token)
            // Convert to List for reliable JSON serialization
            return BadRequest(result.Errors);
        }

        // POST: api/auth/delete-account
        [HttpPost("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount(DeleteAccountDto model)
        {
            // 1. Get the current user's ID from the JWT claim
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            // 2. Load the user object
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.CheckPasswordAsync(user, model.Password) == false)
            {
                return Unauthorized("Invalid password");
            }

            // 3. Delete the user
            // Note: You may need to manually delete associated data in other tables here.
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // Note: After deletion, the client must also log out (clear the JWT).
                // This is handled on the Angular side.
                return NoContent(); // HTTP 204: Success, no content to return
            }

            // 4. Handle errors (e.g., database constraints failed)
            return BadRequest(result.Errors);
        }
    }
}
