using LoviBackend.Data;
using LoviBackend.Models;
using LoviBackend.Models.Dtos;
using LoviBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        public AuthController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IConfiguration configuration
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, Name = dto.Name };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
                return Ok("User created successfully");

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

            // creating the necessary claims
            List<Claim> authClaims = [
                new (ClaimTypes.Name, user.UserName!),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // unique id for token
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

                var tokenInfo = _context.TokenInfos.SingleOrDefault(u => u.UserName == userName && u.DeviceId == deviceId);
                if (tokenInfo == null)
                {
                    return BadRequest("Token not found for this user and device.");
                }

                tokenInfo.RefreshToken = string.Empty;
                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
