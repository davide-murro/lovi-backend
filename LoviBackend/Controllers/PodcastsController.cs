using LoviBackend.Data;
using LoviBackend.Models.DbSets;
using LoviBackend.Models.Dtos;
using LoviBackend.Models.Dtos.Pagination;
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
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PodcastsController(
            ApplicationDbContext context,
            IConfiguration configuration,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/podcasts
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

        // GET: api/podcasts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PodcastDto>> Get(int id)
        {
            var podcast = await _context.Podcasts.Include(p => p.Episodes).FirstOrDefaultAsync(p => p.Id == id);

            if (podcast == null)
            {
                return NotFound();
            }

            var podcastDto = new PodcastDto
            {
                Id = podcast.Id,
                Name = podcast.Name,
                Description = podcast.Description,
                CoverImageUrl = podcast.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "Podcasts", new { id = podcast.Id }, Request.Scheme) : null,
                Episodes = podcast.Episodes.OrderBy(pe => pe.Number).Select(pe => new PodcastEpisodeDto
                {
                    Id = pe.Id,
                    Number = pe.Number,
                    Name = pe.Name,
                    Description = pe.Description,
                    CoverImageUrl = pe.CoverImagePath != null ? Url.Action(nameof(GetEpisodeCoverImage), "Podcasts", new { id = pe.PodcastId, episodeId = pe.Id }, Request.Scheme) : null,
                    AudioUrl = Url.Action(nameof(GetEpisodeAudio), "Podcasts", new { id = pe.PodcastId, episodeId = pe.Id }, Request.Scheme)!,
                }).ToList(),
            };

            return Ok(podcastDto);
        }

        // PUT: api/podcasts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
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
                if (!_context.Podcasts.Any(e => e.Id == id))
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

        // POST: api/podcasts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PodcastDto>> Create(PodcastDto podcastDto)
        {
            var podcast = new Podcast
            {
                Id = podcastDto.Id,
                Name = podcastDto.Name,
                Description = podcastDto.Description,
            };
            _context.Podcasts.Add(podcast);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = podcast.Id }, podcast);
        }

        // DELETE: api/podcasts/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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

        // GET: api/podcasts/exists/5
        [HttpGet("exists/{id}")]
        public async Task<IActionResult> Exists(int id)
        {
            var podcastExists = await _context.Podcasts.AnyAsync(e => e.Id == id);

            if (podcastExists)
            {
                return Ok(true); // Return 200 OK with a true value
            }

            return NotFound(false); // Return 404 Not Found with a false value
        }

        // GET: api/podcasts/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<PodcastDto>>> GetPaged([FromQuery] PagedQuery query)
        {
            // Base query from EF
            var podcastsQuery = _context.Podcasts.AsNoTracking();

            // Sorting
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                var property = typeof(Podcast).GetProperty(query.SortBy);
                if (property != null)
                {
                    podcastsQuery = query.SortOrder.ToLower() == "desc"
                        ? podcastsQuery.OrderByDescending(e => EF.Property<object>(e, property.Name))
                        : podcastsQuery.OrderBy(e => EF.Property<object>(e, property.Name));
                }
            }

            // Total count (before pagination)
            var totalCount = await podcastsQuery.CountAsync();

            // Apply pagination and project to DTO
            var items = await podcastsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PodcastDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CoverImageUrl = p.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "Podcasts", new { id = p.Id }, Request.Scheme) : null,
                })
                .ToListAsync();

            // Wrap result
            var result = new PagedResult<PodcastDto>
            {
                PagedQuery = query,
                Items = items,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        // GET: api/podcasts/5/cover
        [HttpGet("{id}/cover")]
        public IActionResult GetCoverImage(int id)
        {
            var podcast = _context.Podcasts.Find(id);
            if (podcast == null || string.IsNullOrEmpty(podcast.CoverImagePath))
                return NotFound();

            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!, podcast.CoverImagePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "image/jpeg"; // optionally detect by extension
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return File(fileStream, contentType, enableRangeProcessing: true);
        }

        // GET: api/podcasts/5/episodes/1
        [HttpGet("{id}/episodes/{episodeId}")]
        public async Task<ActionResult<PodcastEpisodeDto>> GetEpisode(int id, int episodeId)
        {
            var podcastEpisode = await _context.PodcastEpisodes.Include(pe => pe.Podcast).ThenInclude(p => p.Episodes).FirstOrDefaultAsync(p => p.Id == episodeId);
            if (podcastEpisode == null)
                return NotFound();

            var podcastEpisodeDto = new PodcastEpisodeDto
            {
                Id = podcastEpisode.Id,
                Number = podcastEpisode.Number,
                Name = podcastEpisode.Name,
                Description = podcastEpisode.Description,
                CoverImageUrl = podcastEpisode.CoverImagePath != null ? Url.Action(nameof(GetEpisodeCoverImage), "Podcasts", new { id = podcastEpisode.PodcastId, episodeId = podcastEpisode.Id }, Request.Scheme) : null,
                AudioUrl = Url.Action(nameof(GetEpisodeAudio), "Podcasts", new { id = podcastEpisode.PodcastId, episodeId = podcastEpisode.Id }, Request.Scheme)!,
                Podcast = new PodcastDto
                {
                    Id = podcastEpisode.Podcast.Id,
                    Name = podcastEpisode.Podcast.Name,
                    Description = podcastEpisode.Podcast.Description,
                    Episodes = podcastEpisode.Podcast.Episodes.OrderBy(pe => pe.Number).Select(pe => new PodcastEpisodeDto
                    {
                        Id = pe.Id,
                        Number = pe.Number,
                        Name = pe.Name,
                        Description = pe.Description,
                        CoverImageUrl = pe.CoverImagePath != null ? Url.Action(nameof(GetEpisodeCoverImage), "Podcasts", new { id = pe.PodcastId, episodeId = pe.Id }, Request.Scheme) : null,
                        AudioUrl = Url.Action(nameof(GetEpisodeAudio), "Podcasts", new { id = pe.PodcastId, episodeId = pe.Id }, Request.Scheme)!
                    }).ToList(),
                },
            };

            return Ok(podcastEpisodeDto);
        }

        // GET: api/podcasts/5/episodes/1/cover
        [HttpGet("{id}/episodes/{episodeId}/cover")]
        public IActionResult GetEpisodeCoverImage(int id, int episodeId)
        {
            var podcastEpisode = _context.PodcastEpisodes.Find(episodeId);
            if (podcastEpisode == null || string.IsNullOrEmpty(podcastEpisode.CoverImagePath))
                return NotFound();

            var filePath = Path.Combine(
                _hostingEnvironment.ContentRootPath, 
                _configuration["UploadsPath"]!, 
                podcastEpisode.CoverImagePath
            );
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "image/jpeg"; // optionally detect by extension
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return File(fileStream, contentType, enableRangeProcessing: true);
        }

        // GET: api/podcasts/5/episodes/1/audio
        [HttpGet("{id}/episodes/{episodeId}/audio")]
        public IActionResult GetEpisodeAudio(int id, int episodeId)
        {
            var podcastEpisode = _context.PodcastEpisodes.Find(episodeId);
            if (podcastEpisode == null || string.IsNullOrEmpty(podcastEpisode.AudioPath))
                return NotFound();

            var filePath = Path.Combine(
                _hostingEnvironment.ContentRootPath,
                _configuration["UploadsPath"]!,
                podcastEpisode.AudioPath
            );
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "audio/mpeg"; 
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return File(fileStream, contentType, enableRangeProcessing: true);
        }

    }
}
