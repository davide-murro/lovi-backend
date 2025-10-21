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
            var podcasts = await _context.Podcasts
                .Include(p => p.Voicers)
                    .ThenInclude(v => v.Creator)
                .ToListAsync();

            var podcastDtos = podcasts.Select(podcast => new PodcastDto
            {
                Id = podcast.Id,
                Name = podcast.Name,
                Voicers = podcast.Voicers.Select(v => new CreatorDto
                {
                    Id = v.Creator.Id,
                    Nickname = v.Creator.Nickname,
                    Name = v.Creator.Name,
                    Surname = v.Creator.Surname
                }).ToList()
            }).ToList();

            return Ok(podcastDtos);
        }

        // GET: api/podcasts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PodcastDto>> Get(int id)
        {
            var podcast = await _context.Podcasts
                .Include(p => p.Episodes)
                    .ThenInclude(p => p.Voicers)
                        .ThenInclude(v => v.Creator)
                .Include(p => p.Voicers)
                    .ThenInclude(v => v.Creator)
                .FirstOrDefaultAsync(p => p.Id == id);

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
                    AudioUrl = pe.AudioPath != null ? Url.Action(nameof(GetEpisodeAudio), "Podcasts", new { id = pe.PodcastId, episodeId = pe.Id }, Request.Scheme) : null,
                    Voicers = pe.Voicers.Select(v => new CreatorDto
                    {
                        Id = v.Creator.Id,
                        Nickname = v.Creator.Nickname,
                        Name = v.Creator.Name,
                        Surname = v.Creator.Surname
                    }).ToList()
                }).ToList(),
                Voicers = podcast.Voicers.Select(v => new CreatorDto
                {
                    Id = v.Creator.Id,
                    Nickname = v.Creator.Nickname,
                    Name = v.Creator.Name,
                    Surname = v.Creator.Surname
                }).ToList(),
            };

            return Ok(podcastDto);
        }

        // PUT: api/podcasts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PodcastDto>> Update(int id, [FromForm] PodcastDto podcastDto)
        {
            if (id != podcastDto.Id)
            {
                return BadRequest();
            }

            var podcast = await _context.Podcasts.FindAsync(id);
            if (podcast == null)
            {
                return NotFound();
            }

            // edit podcast
            podcast.UpdatedAt = DateTime.UtcNow;
            podcast.Name = podcastDto.Name;
            podcast.Description = podcastDto.Description;

            // Handle file upload
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var podcastPath = Path.Combine("podcasts", podcast.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, podcastPath));

            if (podcastDto.CoverImageUrl == null && podcast.CoverImagePath != null)
            {
                // Delete Existing File
                var oldFilePath = Path.Combine(uploadPath, podcast.CoverImagePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update the path in the database model
                podcast.CoverImagePath = null;
            }

            if (podcastDto.CoverImage != null)
            {
                // Save the new file
                var fileName = $"cover{Path.GetExtension(podcastDto.CoverImage.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadPath, podcastPath, fileName), FileMode.Create))
                {
                    await podcastDto.CoverImage.CopyToAsync(stream);
                }

                // Update the path in the database model
                podcast.CoverImagePath = Path.Combine(podcastPath, fileName);
            }

            _context.Podcasts.Update(podcast);
            await _context.SaveChangesAsync();

            return Ok(new PodcastDto
            {
                Id = podcast.Id,
                Name = podcast.Name,
                Description = podcast.Description,
                CoverImageUrl = podcast.CoverImagePath
            });
        }

        // POST: api/podcasts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PodcastDto>> Create([FromForm] PodcastDto podcastDto)
        {
            var podcast = new Podcast
            {
                Name = podcastDto.Name,
                Description = podcastDto.Description
            };

            // add podcastEpisode
            _context.Podcasts.Add(podcast);
            await _context.SaveChangesAsync();

            // Handle file upload
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var podcastPath = Path.Combine("podcasts", podcast.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, podcastPath));

            if (podcastDto.CoverImage != null)
            {
                var fileName = $"cover{Path.GetExtension(podcastDto.CoverImage.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadPath, podcastPath, fileName), FileMode.Create))
                {
                    await podcastDto.CoverImage.CopyToAsync(stream);
                }

                podcast.CoverImagePath = Path.Combine(podcastPath, fileName);

                _context.Podcasts.Update(podcast);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(Get), new { id = podcast.Id }, new PodcastDto
            {
                Id = podcast.Id,
                Name = podcast.Name,
                Description = podcast.Description,
                CoverImageUrl = podcast.CoverImagePath
            });
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

            // handle files
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var podcastPath = Path.Combine("podcasts", podcast.Id.ToString());

            if (Directory.Exists(Path.Combine(uploadPath, podcastPath)))
            {
                Directory.Delete(Path.Combine(uploadPath, podcastPath), recursive: true);
            }

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
            var podcastsQuery = _context.Podcasts
                .Include(p => p.Voicers)
                    .ThenInclude(v => v.Creator)
                .AsNoTracking();

            // Sorting
            if (string.IsNullOrEmpty(query.SortBy)) query.SortBy = "Id";
            var property = typeof(Podcast).GetProperty(query.SortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;
            podcastsQuery = query.SortOrder.ToLower() == "desc"
                ? podcastsQuery.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : podcastsQuery.OrderBy(e => EF.Property<object>(e, property.Name));

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
                    Voicers = p.Voicers.Select(v => new CreatorDto
                    {
                        Id = v.Creator.Id,
                        Nickname = v.Creator.Nickname,
                        Name = v.Creator.Name,
                        Surname = v.Creator.Surname
                    }).ToList()
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

        // POST: api/podcasts/5/voicers/2
        [HttpPost("{id}/voicers/{voicerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddVoicer(int id, int voicerId)
        {
            var podcastVoicer = new PodcastVoicer
            {
                PodcastId = id,
                CreatorId = voicerId,
            };
            _context.PodcastVoicers.Add(podcastVoicer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/podcasts/5/voicers/2
        [HttpDelete("{id}/voicers/{voicerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveVoicer(int id, int voicerId)
        {
            var podcastVoicer = await _context.PodcastVoicers.FirstOrDefaultAsync(pv => pv.PodcastId == id && pv.CreatorId == voicerId);
            if (podcastVoicer == null)
            {
                return NotFound();
            }

            _context.PodcastVoicers.Remove(podcastVoicer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/podcasts/5/episodes/1
        [HttpGet("{id}/episodes/{episodeId}")]
        public async Task<ActionResult<PodcastEpisodeDto>> GetEpisode(int id, int episodeId)
        {
            var podcastEpisode = await _context.PodcastEpisodes
                .Include(pe => pe.Podcast)
                    .ThenInclude(p => p.Voicers)
                        .ThenInclude(v => v.Creator)
                .Include(pe => pe.Podcast)
                    .ThenInclude(p => p.Episodes)
                        .ThenInclude(pe => pe.Voicers)
                            .ThenInclude(v => v.Creator)
                .Include(pe => pe.Voicers)
                    .ThenInclude(v => v.Creator)
                .FirstOrDefaultAsync(p => p.PodcastId == id && p.Id == episodeId);
            if (podcastEpisode == null)
                return NotFound();

            var podcastEpisodeDto = new PodcastEpisodeDto
            {
                Id = podcastEpisode.Id,
                Number = podcastEpisode.Number,
                Name = podcastEpisode.Name,
                Description = podcastEpisode.Description,
                CoverImageUrl = podcastEpisode.CoverImagePath != null ? Url.Action(nameof(GetEpisodeCoverImage), "Podcasts", new { id = podcastEpisode.PodcastId, episodeId = podcastEpisode.Id }, Request.Scheme) : null,
                AudioUrl = podcastEpisode.AudioPath != null ? Url.Action(nameof(GetEpisodeAudio), "Podcasts", new { id = podcastEpisode.PodcastId, episodeId = podcastEpisode.Id }, Request.Scheme) : null,
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
                        AudioUrl = pe.AudioPath != null ? Url.Action(nameof(GetEpisodeAudio), "Podcasts", new { id = pe.PodcastId, episodeId = pe.Id }, Request.Scheme) : null,
                        Voicers = pe.Voicers.Select(v => new CreatorDto
                        {
                            Id = v.Creator.Id,
                            Nickname = v.Creator.Nickname,
                            Name = v.Creator.Name,
                            Surname = v.Creator.Surname
                        }).ToList()
                    }).ToList(),
                    Voicers = podcastEpisode.Podcast.Voicers.Select(v => new CreatorDto
                    {
                        Id = v.Creator.Id,
                        Nickname = v.Creator.Nickname,
                        Name = v.Creator.Name,
                        Surname = v.Creator.Surname
                    }).ToList()
                },
                Voicers = podcastEpisode.Voicers.Select(v => new CreatorDto
                {
                    Id = v.Creator.Id,
                    Nickname = v.Creator.Nickname,
                    Name = v.Creator.Name,
                    Surname = v.Creator.Surname
                }).ToList()
            };

            return Ok(podcastEpisodeDto);
        }

        // GET: api/podcasts/5/episodes/1/cover
        [HttpGet("{id}/episodes/{episodeId}/cover")]
        public IActionResult GetEpisodeCoverImage(int id, int episodeId)
        {
            var podcastEpisode = _context.PodcastEpisodes.FirstOrDefault((pe) => pe.PodcastId == id && pe.Id == episodeId);
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
            var podcastEpisode = _context.PodcastEpisodes.FirstOrDefault((pe) => pe.PodcastId == id && pe.Id == episodeId);
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

        // PUT: api/podcasts/5/episodes/1
        [HttpPut("{id}/episodes/{episodeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEpisode(int id, int episodeId, [FromForm] PodcastEpisodeDto podcastEpisodeDto)
        {
            if (id != podcastEpisodeDto.PodcastId || episodeId != podcastEpisodeDto.Id)
            {
                return BadRequest();
            }

            var podcastEpisode = await _context.PodcastEpisodes.FirstOrDefaultAsync((pe) => pe.PodcastId == id && pe.Id == episodeId);
            if (podcastEpisode == null)
            {
                return NotFound();
            }

            // edit podcastEpisode
            podcastEpisode.UpdatedAt = DateTime.UtcNow;
            podcastEpisode.Number = podcastEpisodeDto.Number;
            podcastEpisode.Name = podcastEpisodeDto.Name;
            podcastEpisode.Description = podcastEpisodeDto.Description;

            // Handle file upload
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var podcastEpisodePath = Path.Combine("podcasts", podcastEpisode.PodcastId.ToString(), "episodes", podcastEpisode.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, podcastEpisodePath));

            if (podcastEpisodeDto.CoverImageUrl == null && podcastEpisode.CoverImagePath != null)
            {
                // Delete Existing File
                var oldFilePath = Path.Combine(uploadPath, podcastEpisode.CoverImagePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update the path in the database model
                podcastEpisode.CoverImagePath = null;
            }
            if (podcastEpisodeDto.CoverImage != null)
            {
                // Save the new file
                var fileName = $"cover{Path.GetExtension(podcastEpisodeDto.CoverImage.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadPath, podcastEpisodePath, fileName), FileMode.Create))
                {
                    await podcastEpisodeDto.CoverImage.CopyToAsync(stream);
                }

                // Update the path in the database model
                podcastEpisode.CoverImagePath = Path.Combine(podcastEpisodePath, fileName);
            }

            if (podcastEpisodeDto.AudioUrl == null && podcastEpisode.AudioPath != null)
            {
                // Delete Existing File
                var oldFilePath = Path.Combine(uploadPath, podcastEpisode.AudioPath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update the path in the database model
                podcastEpisode.AudioPath = null;
            }
            if (podcastEpisodeDto.Audio != null)
            {
                // Save the new file
                var fileName = $"cover{Path.GetExtension(podcastEpisodeDto.Audio.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadPath, podcastEpisodePath, fileName), FileMode.Create))
                {
                    await podcastEpisodeDto.Audio.CopyToAsync(stream);
                }

                // Update the path in the database model
                podcastEpisode.AudioPath = Path.Combine(podcastEpisodePath, fileName);
            }

            _context.PodcastEpisodes.Update(podcastEpisode);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/podcasts/5/episodes
        [HttpPost("{id}/episodes")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PodcastEpisodeDto>> CreateEpisode(int id, [FromForm] PodcastEpisodeDto podcastEpisodeDto)
        {
            var podcastEpisode = new PodcastEpisode
            {
                Number = podcastEpisodeDto.Number,
                Name = podcastEpisodeDto.Name,
                Description = podcastEpisodeDto.Description,
                PodcastId = id
            };

            // add podcastEpisode
            _context.PodcastEpisodes.Add(podcastEpisode);
            await _context.SaveChangesAsync();

            // Handle file upload
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var podcastEpisodePath = Path.Combine("podcasts", podcastEpisode.PodcastId.ToString(), "episodes", podcastEpisode.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, podcastEpisodePath));

            if (podcastEpisodeDto.CoverImage != null)
            {
                var fileName = $"cover{Path.GetExtension(podcastEpisodeDto.CoverImage.FileName)}";

                using (var stream = new FileStream(Path.Combine(uploadPath, podcastEpisodePath, fileName), FileMode.Create))
                {
                    await podcastEpisodeDto.CoverImage.CopyToAsync(stream);
                }

                podcastEpisode.CoverImagePath = Path.Combine(podcastEpisodePath, fileName);

                _context.PodcastEpisodes.Update(podcastEpisode);
                await _context.SaveChangesAsync();
            }
            if (podcastEpisodeDto.Audio != null)
            {
                var fileName = $"audio{Path.GetExtension(podcastEpisodeDto.Audio.FileName)}";

                using (var stream = new FileStream(Path.Combine(uploadPath, podcastEpisodePath, fileName), FileMode.Create))
                {
                    await podcastEpisodeDto.Audio.CopyToAsync(stream);
                }

                podcastEpisode.AudioPath = Path.Combine(podcastEpisodePath, fileName);

                _context.PodcastEpisodes.Update(podcastEpisode);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetEpisode), new { id = podcastEpisode.PodcastId, episodeId = podcastEpisode.Id }, new PodcastEpisodeDto
            {
                Id = podcastEpisode.Id,
                Name = podcastEpisode.Name,
                Description = podcastEpisode.Description,
                CoverImageUrl = podcastEpisode.CoverImagePath,
                AudioUrl = podcastEpisode.AudioPath,
                PodcastId = podcastEpisode.PodcastId
            });
        }

        // DELETE: api/podcasts/5/episodes/1
        [HttpDelete("{id}/episodes/{episodeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEpisode(int id, int episodeId)
        {
            var podcastEpisode = await _context.PodcastEpisodes.FirstOrDefaultAsync(pe => pe.PodcastId == id && pe.Id == episodeId);
            if (podcastEpisode == null)
            {
                return NotFound();
            }

            _context.PodcastEpisodes.Remove(podcastEpisode);
            await _context.SaveChangesAsync();

            // handle files
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var podcastEpisodePath = Path.Combine("podcasts", podcastEpisode.PodcastId.ToString(), "episodes", podcastEpisode.Id.ToString());

            if (Directory.Exists(Path.Combine(uploadPath, podcastEpisodePath)))
            {
                Directory.Delete(Path.Combine(uploadPath, podcastEpisodePath), recursive: true);
            }

            return NoContent();
        }

        // GET: api/podcasts/5/episodes/exists/1
        [HttpGet("{id}/episodes/{episodeId}/exists/")]
        public async Task<IActionResult> EpisodeExists(int id, int episodeId)
        {
            var podcastEpisodeExists = await _context.PodcastEpisodes.AnyAsync(pe => pe.PodcastId == id && pe.Id == episodeId);

            if (podcastEpisodeExists)
            {
                return Ok(true);
            }

            return NotFound(false);
        }

        // POST: api/podcasts/5/episodes/1/voicers/2
        [HttpPost("{id}/episodes/{episodeId}/voicers/{voicerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddEpisodeVoicer(int id, int episodeId, int voicerId)
        {
            var podcastEpisodeVoicer = new PodcastEpisodeVoicer
            {
                PodcastEpisodeId = episodeId,
                CreatorId = voicerId,
            };
            _context.PodcastEpisodeVoicers.Add(podcastEpisodeVoicer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/podcasts/5/episodes/1/voicers/2
        [HttpDelete("{id}/episodes/{episodeId}/voicers/{voicerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveEpisodeVoicer(int id, int episodeId, int voicerId)
        {
            var podcastEpisodeVoicer = await _context.PodcastEpisodeVoicers.FirstOrDefaultAsync(pev => pev.PodcastEpisodeId == episodeId && pev.CreatorId == voicerId);
            if (podcastEpisodeVoicer == null)
            {
                return NotFound();
            }

            _context.PodcastEpisodeVoicers.Remove(podcastEpisodeVoicer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
