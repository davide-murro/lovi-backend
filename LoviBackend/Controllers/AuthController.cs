using LoviBackend.Data;
using LoviBackend.Models.DbSets;
using LoviBackend.Models.Dtos;
using LoviBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;

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

        public AuthController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IConfiguration configuration,
            IWebHostEnvironment hostingEnvironment
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, Name = dto.Name };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
                return NoContent();

            return BadRequest(result.Errors);
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
                var ti = new TokenInfo
                {
                    UserName = user.UserName!,
                    RefreshToken = refreshToken,
                    ExpiredAt = expireAt,
                    DeviceId = deviceId
                };
                _context.TokenInfos.Add(ti);
            }
            // Else, update the refresh token and expiration
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.ExpiredAt = expireAt;
            }

            await _context.SaveChangesAsync();

            return Ok(new TokenDto
            {
                AccessToken = token,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenDto dto, [FromHeader(Name = "X-DeviceId")] string deviceId)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
                var userName = principal.Identity!.Name;

                var tokenInfo = _context.TokenInfos.SingleOrDefault(t => t.UserName == userName && t.DeviceId == deviceId);
                if (tokenInfo == null
                    || tokenInfo.RefreshToken != dto.RefreshToken
                    || tokenInfo.ExpiredAt <= DateTime.UtcNow)
                {
                    return BadRequest("Invalid refresh token. Please login again.");
                }

                var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                tokenInfo.RefreshToken = newRefreshToken; // rotating the refresh token
                tokenInfo.ExpiredAt = DateTime.UtcNow.AddMonths(int.Parse(_configuration["Jwt:ExpireAfterInMonths"]!));
                await _context.SaveChangesAsync();

                return Ok(new TokenDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
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
                var userName = User.Identity!.Name;

                var tokenInfos = await _context.TokenInfos.Where(u => u.UserName == userName).ToListAsync();
                if (tokenInfos.Count() == 0)
                {
                    return BadRequest("Tokens not found for this user.");
                }

                _context.TokenInfos.RemoveRange(tokenInfos);
                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        // PUT: api/auth/change-email
        [HttpPut("change-email")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto model)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentEmail = user.Email;
            var newEmail = model.NewEmail;

            // Update the email and security stamp using UserManager
            var result = await _userManager.SetEmailAsync(user, newEmail);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            result = await _userManager.SetUserNameAsync(user, newEmail);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }


            /*
            // Send confirmation emails
            // Send to OLD email
            await _emailService.SendEmailAsync(
                currentEmail,
                "Email Address Changed",
                $"Your email address on LOVI has been changed from {currentEmail} to {newEmail}. If this wasn't you, please contact support."
            );

            // Send to NEW email (often requires a verification step in a real app)
            await _emailService.SendEmailAsync(
                newEmail,
                "Email Address Updated Successfully",
                "Your email address has been successfully updated on LOVI."
            );
            */

            // Note: In a real-world scenario, you would typically send a verification token 
            // to the new email and require confirmation before updating the database.

            return NoContent();
        }

        // PUT: api/auth/change-password
        [HttpPut("change-password")]
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

            /*// 3. Send confirmation email
            await _emailService.SendEmailAsync(
                user.Email,
                "Password Changed Successfully",
                "Your account password has been successfully updated. If you did not make this change, please contact support immediately."
            );*/

            return NoContent();
        }

        // DELETE: api/auth/delete-account
        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            // 1. Get the current user's ID from the JWT claim
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            // 2. Load the user object
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

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
            var encodedEmail = UrlEncoder.Default.Encode(user.Email!); // Use email for simplicity in routing

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
                return BadRequest("All fields (email, token, new password) are required.");
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
                // Optional: Send confirmation email
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Password Reset Confirmed",
                    "Your password has been successfully reset. You can now log in with your new password."
                );*/
                return NoContent();
            }

            // 4. Handle Identity errors (e.g., weak password, invalid token)
            // Convert to List for reliable JSON serialization
            return BadRequest(result.Errors.ToList());
        }
    }
}
