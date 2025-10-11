using LoviBackend.Data;
using LoviBackend.Models.DbSets;
using LoviBackend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                Nickname = creator.Nickname
            };

            return creatorDto;
        }

        // PUT: api/creators/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreatorDto creatorDto)
        {
            if (id != creatorDto.Id)
            {
                return BadRequest();
            }

            _context.Entry(creatorDto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Creators.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/creators
        [HttpPost]
        public async Task<ActionResult<CreatorDto>> Create(CreatorDto creatorDto)
        {
            var creator = new Creator
            {
                Id = creatorDto.Id,
                Nickname = creatorDto.Nickname,
                Name = creatorDto.Name,
                Surname = creatorDto.Surname
            };
            _context.Creators.Add(creator);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = creatorDto.Id }, creatorDto);
        }

        // DELETE: api/creators/5
        [HttpDelete("{id}")]
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
    }
}
