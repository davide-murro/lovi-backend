using LoviBackend.Data;
using LoviBackend.Models.DbSets;
using LoviBackend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoviBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PodcastsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PodcastsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Podcasts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PodcastDto>>> Get()
        {
            var podcasts = await _context.Podcasts.ToListAsync();

            var podcastDtos = podcasts.Select(podcast => new PodcastDto
            {
                Id = podcast.Id,
                Name = podcast.Name,
            }).ToList();

            return Ok(podcastDtos);
        }

        // GET: api/Podcasts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PodcastDto>> Get(int id)
        {
            var podcast = await _context.Podcasts.FindAsync(id);

            if (podcast == null)
            {
                return NotFound();
            }

            var podcastDto = new PodcastDto
            {
                Id = podcast.Id,
                Name = podcast.Name,
            };

            return podcastDto;
        }

        // PUT: api/Podcasts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PodcastDto podcastDto)
        {
            if (id != podcastDto.Id)
            {
                return BadRequest();
            }

            _context.Entry(podcastDto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(id))
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

        // POST: api/Podcasts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<PodcastDto>> Create(PodcastDto podcastDto)
        {
            var podcast = new Podcast
            {
                Id = podcastDto.Id,
                Name = podcastDto.Name,
            };
            _context.Podcasts.Add(podcast);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = podcast.Id }, podcast);
        }

        // DELETE: api/Podcasts/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var podcast = await _context.Podcasts.FindAsync(id);
            if (podcast == null)
            {
                return NotFound();
            }

            _context.Podcasts.Remove(podcast);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool Exists(int id)
        {
            return _context.Podcasts.Any(e => e.Id == id);
        }
    }
}
