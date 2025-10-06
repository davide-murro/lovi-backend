using LoviBackend.Data;
using LoviBackend.Models.DbSets;
using LoviBackend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LoviBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrariesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LibrariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/libraries
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LibraryDto>>> Get()
        {
            var libraries = await _context.Libraries
                .Include(l => l.User)
                .Include(l => l.Podcast)
                .Include(l => l.PodcastEpisode)
                .Include(l => l.AudioBook)
                .ToListAsync();

            var libraryDtos = libraries.Select(l => new LibraryDto
            {
                Id = l.Id,
                User = new UserDto
                {
                    Id = l.User.Id,
                    Name = l.User.Name
                },
                Podcast = l.Podcast != null ? new PodcastDto
                {
                    Id = l.Podcast.Id,
                    Name = l.Podcast.Name,
                    CoverImageUrl = l.Podcast.CoverImagePath != null ? Url.Action(nameof(PodcastsController.GetCoverImage), "Podcasts", new { id = l.Podcast.Id }, Request.Scheme) : null,
                    Description = l.Podcast.Description
                } : null,
                PodcastEpisode = l.PodcastEpisode != null ? new PodcastEpisodeDto
                {
                    Id = l.PodcastEpisode.Id,
                    Name = l.PodcastEpisode.Name,
                    CoverImageUrl = l.PodcastEpisode.CoverImagePath != null ? Url.Action(nameof(PodcastsController.GetEpisodeCoverImage), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id }, Request.Scheme) : null,
                    Description = l.PodcastEpisode.Description
                } : null,
                AudioBook = l.AudioBook != null ? new AudioBookDto
                {
                    Id = l.AudioBook.Id,
                    Name = l.AudioBook.Name,
                    Description = l.AudioBook.Description
                } : null
            }).ToList();

            return Ok(libraryDtos);
        }

        // GET: api/libraries/me
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<LibraryDto>>> GetMe()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var libraries = await _context.Libraries
                .Include(l => l.User)
                .Include(l => l.Podcast)
                .Include(l => l.PodcastEpisode)
                .Include(l => l.AudioBook)
                .Where(l => l.UserId == userId)
                .ToListAsync();

            var libraryDtos = libraries.Select(l => new LibraryDto
            {
                Id = l.Id,
                User = new UserDto
                {
                    Id = l.User.Id,
                    Name = l.User.Name
                },
                Podcast = l.Podcast != null ? new PodcastDto
                {
                    Id = l.Podcast.Id,
                    Name = l.Podcast.Name,
                    CoverImageUrl = l.Podcast.CoverImagePath != null ? Url.Action(nameof(PodcastsController.GetCoverImage), "Podcasts", new { id = l.Podcast.Id }, Request.Scheme) : null,
                    Description = l.Podcast.Description
                } : null,
                PodcastEpisode = l.PodcastEpisode != null ? new PodcastEpisodeDto
                {
                    Id = l.PodcastEpisode.Id,
                    Name = l.PodcastEpisode.Name,
                    CoverImageUrl = l.PodcastEpisode.CoverImagePath != null ? Url.Action(nameof(PodcastsController.GetEpisodeCoverImage), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id }, Request.Scheme) : null,
                    Description = l.PodcastEpisode.Description,
                    AudioUrl = Url.Action(nameof(PodcastsController.GetEpisodeAudio), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id }, Request.Scheme)!,
                } : null,
                AudioBook = l.AudioBook != null ? new AudioBookDto
                {
                    Id = l.AudioBook.Id,
                    Name = l.AudioBook.Name,
                    Description = l.AudioBook.Description
                } : null
            }).ToList();

            return Ok(libraryDtos);
        }

        // POST: api/libraries/me
        [Authorize]
        [HttpPost("me")]
        public async Task<ActionResult<LibraryDto>> CreateMe(ManageLibraryDto manageLibraryDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var library = new Library
            {
                UserId = userId,
                PodcastId = manageLibraryDto.PodcastId,
                PodcastEpisodeId = manageLibraryDto.PodcastEpisodeId,
                AudioBookId = manageLibraryDto.AudioBookId,
            };
            _context.Libraries.Add(library);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMe), library);
        }

        // POST: api/libraries/me/list
        [Authorize]
        [HttpPost("me/list")]
        public async Task<ActionResult<IEnumerable<LibraryDto>>> CreateMe([FromBody] List<ManageLibraryDto> manageLibraryDtos)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (manageLibraryDtos == null || !manageLibraryDtos.Any())
            {
                return BadRequest("No items provided");
            }

            var libraries = manageLibraryDtos.Select(dto => new Library
            {
                UserId = userId,
                PodcastId = dto.PodcastId,
                PodcastEpisodeId = dto.PodcastEpisodeId,
                AudioBookId = dto.AudioBookId,
            }).ToList();

            _context.Libraries.AddRange(libraries);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMe), new { }, libraries);
        }

        // DELETE: api/libraries/me
        [Authorize]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var libraries = await _context.Libraries.Where(l => l.UserId == userId).ToListAsync();
            _context.Libraries.RemoveRange(libraries);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/libraries/me/2
        [Authorize]
        [HttpDelete("me/{id}")]
        public async Task<IActionResult> DeleteMe(int id)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var library = await _context.Libraries.FirstOrDefaultAsync(p => p.UserId == userId && p.Id == id);

            if (library == null)
            {
                return NotFound();
            }

            _context.Libraries.Remove(library);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/libraries/me/list
        [Authorize]
        [HttpDelete("me/list")]
        public async Task<IActionResult> DeleteMe([FromBody] List<int> ids)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var libraries = await _context.Libraries
                .Where(l => ids.Contains(l.Id))
                .ToListAsync();

            if (libraries == null || !libraries.Any())
            {
                return NotFound();
            }

            _context.Libraries.RemoveRange(libraries);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
