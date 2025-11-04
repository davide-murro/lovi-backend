using LoviBackend.Data;
using LoviBackend.Models.DbSets;
using LoviBackend.Models.Dtos;
using LoviBackend.Models.Dtos.Auth;
using LoviBackend.Models.Dtos.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection;
using System.Security.Claims;

namespace LoviBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> Get()
        {
            var users = await _userManager.Users.ToListAsync();

            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                Name = user.Name
            }).ToList();

            return Ok(userDtos);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roleNames = await _userManager.GetRolesAsync(user);
            var roleDtos = await _roleManager.Roles
                .Where(r => roleNames.Contains(r.Name!))
                .Select((r) => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name!
                })
                .ToListAsync();

            var tokenInfoDtos = await _context.TokenInfos
                .Where(t => t.UserName == user.UserName)
                .Select(t => new TokenInfoDto
                {
                    Id = t.Id,
                    UserName = t.UserName,
                    RefreshToken = t.RefreshToken,
                    ExpiredAt = t.ExpiredAt,
                    RefreshedAt = t.RefreshedAt,
                    DeviceId = t.DeviceId
                })
                .ToListAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                Name = user.Name,
                Roles = roleDtos,
                TokenInfos = tokenInfoDtos,
            };

            return Ok(userDto);
        }

        // PUT: api/users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, UserDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (userDto.NewPassword != null && userDto.NewPassword != "")
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var decodedToken = Uri.UnescapeDataString(token);
                var resultPassword = await _userManager.ResetPasswordAsync(user, decodedToken, userDto.NewPassword);
                if (!resultPassword.Succeeded)
                {
                    return BadRequest(resultPassword.Errors);
                }
            }

            if (userDto.Email != user.Email)
            {
                var emailResult = await _userManager.SetEmailAsync(user, userDto.Email);
                if (!emailResult.Succeeded)
                {
                    return BadRequest(emailResult.Errors);
                }
                emailResult = await _userManager.SetUserNameAsync(user, userDto.Email);
                if (!emailResult.Succeeded)
                {
                    return BadRequest(emailResult.Errors);
                }
            }

            user.UpdatedAt = DateTime.UtcNow;
            user.EmailConfirmed = userDto.EmailConfirmed ?? false;
            user.Name = userDto.Name;


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        // POST: api/users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> Create(UserDto userDto)
        {
            var user = new ApplicationUser
            {
                UserName = userDto.Email,
                Email = userDto.Email,
                EmailConfirmed = userDto.EmailConfirmed ?? false,
                Name = userDto.Name,
            };
            var result = await _userManager.CreateAsync(user, userDto.NewPassword!);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(Get), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Name = user.Name,
            });
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        // GET: api/users/exists/5
        [HttpGet("exists/{id}")]
        public async Task<IActionResult> Exists(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                return Ok(true); // Return 200 OK with a true value
            }

            return NotFound(false); // Return 404 Not Found with a false value
        }

        // GET: api/users/paged
        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetPaged([FromQuery] PagedQuery query)
        {
            // Base query from EF
            var usersQuery = _userManager.Users.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.ToLower();

                usersQuery = usersQuery.Where(p =>
                    p.Name == null || p.Name.ToLower().Contains(search)
                );
            }

            // Sorting
            if (string.IsNullOrEmpty(query.SortBy)) query.SortBy = "Id";
            var property = typeof(ApplicationUser).GetProperty(query.SortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;
            usersQuery = query.SortOrder.ToLower() == "desc"
                ? usersQuery.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : usersQuery.OrderBy(e => EF.Property<object>(e, property.Name));

            // Total count (before pagination)
            var totalCount = await usersQuery.CountAsync();

            // Apply pagination and project to DTO
            var items = await usersQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email!,
                    EmailConfirmed = u.EmailConfirmed,
                    Name = u.Name,
                })
                .ToListAsync();

            // Wrap result
            var result = new PagedResult<UserDto>
            {
                PagedQuery = query,
                Items = items,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        // POST: api/users/5/roles/2
        [HttpPost("{id}/roles/{roleId}")]
        public async Task<IActionResult> AddRole(string id, string roleId)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, role.Name!))
                return BadRequest("User already has role");

            var result = await _userManager.AddToRoleAsync(user, role.Name!);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        // DELETE: api/users/5/roles/2
        [HttpDelete("{id}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRole(string id, string roleId)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound();

            if (!await _userManager.IsInRoleAsync(user, role.Name!))
                return BadRequest("User does not have role");

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }


        // GET: api/users/profiles/me
        [HttpGet("profiles/me")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetProfileMe()
        {
            var id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(id))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
            };

            return Ok(userDto);
        }

        // PUT: api/users/profiles/me
        [HttpPut("profiles/me")]
        [Authorize]
        public async Task<IActionResult> UpdateProfileMe(UserProfileDto userDto)
        {
            // 1. Get the authenticated user's ID from the JWT token
            var id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(id))
            {
                return Unauthorized(); // Token is missing the required ID claim
            }

            // 2. You must ensure the DTO ID matches the authenticated user ID
            if (id != userDto.Id)
            {
                // Return a 403 Forbidden if the user is trying to edit another profile
                return Forbid();
            }

            // 3. Find the existing user in the database
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // 4. Update the user's properties from the DTO
            // This prevents a user from changing their ID or other fields
            user.Name = userDto.Name;

            // 5. Save the changes
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
