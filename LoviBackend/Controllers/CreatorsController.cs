using LoviBackend.Data;
using LoviBackend.Models.DbSets;
using LoviBackend.Models.Dtos;
using LoviBackend.Models.Dtos.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LoviBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreatorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CreatorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/creators
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreatorDto>>> Get()
        {
            var creators = await _context.Creators.ToListAsync();

            var creatorDtos = creators.Select(creator => new CreatorDto
            {
                Id = creator.Id,
                Nickname = creator.Nickname,
                Name = creator.Name,
                Surname = creator.Surname,
            }).ToList();

            return Ok(creatorDtos);
        }

        // GET: api/creators/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreatorDto>> Get(int id)
        {
            var creator = await _context.Creators.FindAsync(id);

            if (creator == null)
            {
                return NotFound();
            }

            var creatorDto = new CreatorDto
            {
                Id = creator.Id,
                Nickname = creator.Nickname,
                Name = creator.Name,
                Surname = creator.Surname
            };

            return creatorDto;
        }

        // PUT: api/creators/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, CreatorDto creatorDto)
        {
            if (id != creatorDto.Id)
            {
                return BadRequest();
            }

            var creator = await _context.Creators.FindAsync(id);
            if (creator == null)
            {
                return NotFound();
            }

            // edit podcast
            creator.UpdatedAt = DateTime.UtcNow;
            creator.Nickname = creatorDto.Nickname;
            creator.Name = creatorDto.Name;
            creator.Surname = creatorDto.Surname;

            _context.Creators.Update(creator);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/creators
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreatorDto>> Create(CreatorDto creatorDto)
        {
            var creator = new Creator
            {
                Nickname = creatorDto.Nickname,
                Name = creatorDto.Name,
                Surname = creatorDto.Surname
            };
            _context.Creators.Add(creator);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = creator.Id }, new CreatorDto
            {
                Id = creator.Id,
                Nickname = creator.Nickname,
                Name = creator.Name,
                Surname = creator.Surname
            });
        }

        // DELETE: api/creators/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var creator = await _context.Creators.FindAsync(id);
            if (creator == null)
            {
                return NotFound();
            }

            _context.Creators.Remove(creator);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/creators/exists/5
        [HttpGet("exists/{id}")]
        public async Task<IActionResult> Exists(int id)
        {
            var creatorExists = await _context.Creators.AnyAsync(e => e.Id == id);

            if (creatorExists)
            {
                return Ok(true);
            }

            return NotFound(false);
        }

        // GET: api/creators/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<CreatorDto>>> GetPaged([FromQuery] PagedQuery query)
        {
            // Base query from EF
            var creatorsQuery = _context.Creators.AsNoTracking();

            // Sorting
            if (string.IsNullOrEmpty(query.SortBy)) query.SortBy = "Id";
            var property = typeof(Creator).GetProperty(query.SortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;
            creatorsQuery = query.SortOrder.ToLower() == "desc"
                ? creatorsQuery.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : creatorsQuery.OrderBy(e => EF.Property<object>(e, property.Name));

            // Total count (before pagination)
            var totalCount = await creatorsQuery.CountAsync();

            // Apply pagination and project to DTO
            var items = await creatorsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new CreatorDto
                {
                    Id = p.Id,
                    Nickname = p.Nickname,
                    Name = p.Name,
                    Surname = p.Surname
                })
                .ToListAsync();

            // Wrap result
            var result = new PagedResult<CreatorDto>
            {
                PagedQuery = query,
                Items = items,
                TotalCount = totalCount
            };

            return Ok(result);
        }
    }
}
