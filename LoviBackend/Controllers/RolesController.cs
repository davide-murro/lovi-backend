using LoviBackend.Models.Dtos.Auth;
using LoviBackend.Models.Dtos.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection;

namespace LoviBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> Get()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            var roleDtos = roles.Select(role => new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
            }).ToList();

            return Ok(roleDtos);
        }

        // GET: api/roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> Get(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
            };

            return roleDto;
        }

        // PUT: api/roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, RoleDto roleDto)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            role.Id = roleDto.Id;
            role.Name = roleDto.Name;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        // POST: api/roles
        [HttpPost]
        public async Task<ActionResult<RoleDto>> Create(RoleDto roleDto)
        {
            var role = new IdentityRole
            {
                Id = roleDto.Id,
                Name = roleDto.Name,
                NormalizedName = roleDto.Name.ToUpper()
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return CreatedAtAction(nameof(Get), new { id = role.Id }, new RoleDto
            {
                Id = role.Id,
                Name = role.Name
            });
        }

        // DELETE: api/roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        // GET: api/roles/exists/5
        [HttpGet("exists/{id}")]
        public async Task<IActionResult> Exists(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                return Ok(true);
            }

            return NotFound(false);
        }

        // GET: api/roles/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<RoleDto>>> GetPaged([FromQuery] PagedQuery query)
        {
            // Base query from EF
            var rolesQuery = _roleManager.Roles.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.ToLower();

                rolesQuery = rolesQuery.Where(p =>
                    p.Name == null || p.Name.ToLower().Contains(search)
                );
            }

            // Sorting
            if (string.IsNullOrEmpty(query.SortBy)) query.SortBy = "Id";
            var property = typeof(IdentityRole).GetProperty(query.SortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;
            rolesQuery = query.SortOrder.ToLower() == "desc"
                ? rolesQuery.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : rolesQuery.OrderBy(e => EF.Property<object>(e, property.Name));

            // Total count (before pagination)
            var totalCount = await rolesQuery.CountAsync();

            // Apply pagination and project to DTO
            var items = await rolesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new RoleDto
                {
                    Id = p.Id,
                    Name = p.Name!,
                })
                .ToListAsync();

            // Wrap result
            var result = new PagedResult<RoleDto>
            {
                PagedQuery = query,
                Items = items,
                TotalCount = totalCount
            };

            return Ok(result);
        }
    }
}
