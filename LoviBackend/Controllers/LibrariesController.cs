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
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LibraryDto>>> Get()
        {
            var libraries = await _context.Libraries
                .Include(l => l.User)
                .Include(l => l.Podcast)
                    .ThenInclude(p => p!.Voicers)
                        .ThenInclude(v => v.Creator)
                .Include(l => l.PodcastEpisode)
                    .ThenInclude(pe => pe!.Voicers)
                        .ThenInclude(v => v.Creator)
                .Include(l => l.Book)
                    .ThenInclude(b => b!.Readers)
                        .ThenInclude(r => r.Creator)
                .Include(l => l.Book)
                    .ThenInclude(b => b!.Writers)
                        .ThenInclude(w => w.Creator)
                .OrderByDescending(l => l.Id)
                .Select(l => new LibraryDto
                {
                    Id = l.Id,
                    User = new UserProfileDto
                    {
                        Id = l.User.Id,
                        Name = l.User.Name
                    },
                    Podcast = l.Podcast != null ? new PodcastDto
                    {
                        Id = l.Podcast.Id,
                        Name = l.Podcast.Name,
                        DataUrl = Url.Action(nameof(PodcastsController.Get), "Podcasts", new { id = l.Podcast.Id }, Request.Scheme),
                        CoverImageUrl = l.Podcast.CoverImagePath != null ? Url.Action(nameof(PodcastsController.GetCoverImage), "Podcasts", new { id = l.Podcast.Id }, Request.Scheme) : null,
                        CoverImagePreviewUrl = l.Podcast.CoverImagePreviewPath != null ? Url.Action(nameof(PodcastsController.GetCoverImage), "Podcasts", new { id = l.Podcast.Id, isPreview = true }, Request.Scheme) : null,
                        Description = l.Podcast.Description,
                        Voicers = l.Podcast.Voicers.Select(v => new CreatorDto
                        {
                            Id = v.Creator.Id,
                            Nickname = v.Creator.Nickname,
                            Name = v.Creator.Name,
                            Surname = v.Creator.Surname
                        }).ToList()
                    } : null,
                    PodcastEpisode = l.PodcastEpisode != null ? new PodcastEpisodeDto
                    {
                        Id = l.PodcastEpisode.Id,
                        Number = l.PodcastEpisode.Number,
                        Name = l.PodcastEpisode.Name,
                        DataUrl = Url.Action(nameof(PodcastsController.GetEpisode), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id }, Request.Scheme),
                        CoverImageUrl = l.PodcastEpisode.CoverImagePath != null ? Url.Action(nameof(PodcastsController.GetEpisodeCoverImage), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id }, Request.Scheme) : null,
                        CoverImagePreviewUrl = l.PodcastEpisode.CoverImagePreviewPath != null ? Url.Action(nameof(PodcastsController.GetEpisodeCoverImage), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id, isPreview = true }, Request.Scheme) : null,
                        Description = l.PodcastEpisode.Description,
                        Voicers = l.PodcastEpisode.Voicers.Select(v => new CreatorDto
                        {
                            Id = v.Creator.Id,
                            Nickname = v.Creator.Nickname,
                            Name = v.Creator.Name,
                            Surname = v.Creator.Surname
                        }).ToList()
                    } : null,
                    Book = l.Book != null ? new BookDto
                    {
                        Id = l.Book.Id,
                        Name = l.Book.Name,
                        DataUrl = Url.Action(nameof(BooksController.Get), "Books", new { id = l.Book.Id }, Request.Scheme),
                        CoverImageUrl = l.Book.CoverImagePath != null ? Url.Action(nameof(BooksController.GetCoverImage), "Books", new { id = l.Book.Id }, Request.Scheme) : null,
                        CoverImagePreviewUrl = l.Book.CoverImagePreviewPath != null ? Url.Action(nameof(BooksController.GetCoverImage), "Books", new { id = l.Book.Id, isPreview = true }, Request.Scheme) : null,
                        Description = l.Book.Description,
                        AudioUrl = l.Book.AudioPath != null ? Url.Action(nameof(BooksController.GetAudio), "Books", new { id = l.Book.Id }, Request.Scheme) : null,
                        FileUrl = l.Book.FilePath != null ? Url.Action(nameof(BooksController.GetFile), "Books", new { id = l.Book.Id }, Request.Scheme) : null,
                        Readers = l.Book.Readers.Select(r => new CreatorDto
                        {
                            Id = r.Creator.Id,
                            Nickname = r.Creator.Nickname,
                            Name = r.Creator.Name,
                            Surname = r.Creator.Surname
                        }).ToList(),
                        Writers = l.Book.Writers.Select(w => new CreatorDto
                        {
                            Id = w.Creator.Id,
                            Nickname = w.Creator.Nickname,
                            Name = w.Creator.Name,
                            Surname = w.Creator.Surname
                        }).ToList()
                    } : null
                })
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            return Ok(libraries);
        }

        // GET: api/libraries/me
        [HttpGet("me")]
        [Authorize]
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
                    .ThenInclude(p => p!.Voicers)
                        .ThenInclude(v => v.Creator)
                .Include(l => l.PodcastEpisode)
                    .ThenInclude(pe => pe!.Voicers)
                        .ThenInclude(v => v.Creator)
                .Include(l => l.Book)
                    .ThenInclude(b => b!.Readers)
                        .ThenInclude(r => r.Creator)
                .Include(l => l.Book)
                    .ThenInclude(b => b!.Writers)
                        .ThenInclude(w => w.Creator)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Id)
                .Select(l => new LibraryDto
                {
                    Id = l.Id,
                    User = new UserProfileDto
                    {
                        Id = l.User.Id,
                        Name = l.User.Name
                    },
                    Podcast = l.Podcast != null ? new PodcastDto
                    {
                        Id = l.Podcast.Id,
                        Name = l.Podcast.Name,
                        DataUrl = Url.Action(nameof(PodcastsController.Get), "Podcasts", new { id = l.Podcast.Id }, Request.Scheme),
                        CoverImageUrl = l.Podcast.CoverImagePath != null ? Url.Action(nameof(PodcastsController.GetCoverImage), "Podcasts", new { id = l.Podcast.Id }, Request.Scheme) : null,
                        CoverImagePreviewUrl = l.Podcast.CoverImagePreviewPath != null ? Url.Action(nameof(PodcastsController.GetCoverImage), "Podcasts", new { id = l.Podcast.Id, isPreview = true }, Request.Scheme) : null,
                        Description = l.Podcast.Description,
                        Voicers = l.Podcast.Voicers.Select(v => new CreatorDto
                        {
                            Id = v.Creator.Id,
                            Nickname = v.Creator.Nickname,
                            Name = v.Creator.Name,
                            Surname = v.Creator.Surname
                        }).ToList()
                    } : null,
                    PodcastEpisode = l.PodcastEpisode != null ? new PodcastEpisodeDto
                    {
                        Id = l.PodcastEpisode.Id,
                        Number = l.PodcastEpisode.Number,
                        Name = l.PodcastEpisode.Name,
                        DataUrl = Url.Action(nameof(PodcastsController.GetEpisode), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id }, Request.Scheme),
                        CoverImageUrl = l.PodcastEpisode.CoverImagePath != null ? Url.Action(nameof(PodcastsController.GetEpisodeCoverImage), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id }, Request.Scheme) : null,
                        CoverImagePreviewUrl = l.PodcastEpisode.CoverImagePreviewPath != null ? Url.Action(nameof(PodcastsController.GetEpisodeCoverImage), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id, isPreview = true }, Request.Scheme) : null,
                        Description = l.PodcastEpisode.Description,
                        AudioUrl = l.PodcastEpisode.AudioPath != null ? Url.Action(nameof(PodcastsController.GetEpisodeAudio), "Podcasts", new { id = l.PodcastEpisode.PodcastId, episodeId = l.PodcastEpisode.Id }, Request.Scheme) : null,
                        Voicers = l.PodcastEpisode.Voicers.Select(v => new CreatorDto
                        {
                            Id = v.Creator.Id,
                            Nickname = v.Creator.Nickname,
                            Name = v.Creator.Name,
                            Surname = v.Creator.Surname
                        }).ToList()
                    } : null,
                    Book = l.Book != null ? new BookDto
                    {
                        Id = l.Book.Id,
                        Name = l.Book.Name,
                        DataUrl = Url.Action("Get", "Books", new { id = l.Book.Id }, Request.Scheme),
                        CoverImageUrl = l.Book.CoverImagePath != null ? Url.Action(nameof(BooksController.GetCoverImage), "Books", new { id = l.Book.Id }, Request.Scheme) : null,
                        CoverImagePreviewUrl = l.Book.CoverImagePreviewPath != null ? Url.Action(nameof(BooksController.GetCoverImage), "Books", new { id = l.Book.Id, isPreview = true }, Request.Scheme) : null,
                        Description = l.Book.Description,
                        AudioUrl = l.Book.AudioPath != null ? Url.Action(nameof(BooksController.GetAudio), "Books", new { id = l.Book.Id }, Request.Scheme) : null,
                        FileUrl = l.Book.FilePath != null ? Url.Action(nameof(BooksController.GetFile), "Books", new { id = l.Book.Id }, Request.Scheme) : null,
                        Readers = l.Book.Readers.Select(r => new CreatorDto
                        {
                            Id = r.Creator.Id,
                            Nickname = r.Creator.Nickname,
                            Name = r.Creator.Name,
                            Surname = r.Creator.Surname
                        }).ToList(),
                        Writers = l.Book.Writers.Select(w => new CreatorDto
                        {
                            Id = w.Creator.Id,
                            Nickname = w.Creator.Nickname,
                            Name = w.Creator.Name,
                            Surname = w.Creator.Surname
                        }).ToList()
                    } : null
                })
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            return Ok(libraries);
        }

        // POST: api/libraries/me
        [HttpPost("me")]
        [Authorize]
        public async Task<ActionResult<LibraryDto>> CreateMe(ManageLibraryDto manageLibraryDto)
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // check if the same library item already exists for this user
            var exists = await _context.Libraries.AnyAsync(l =>
                l.UserId == userId &&
                l.PodcastId == manageLibraryDto.PodcastId &&
                l.PodcastEpisodeId == manageLibraryDto.PodcastEpisodeId &&
                l.BookId == manageLibraryDto.BookId);

            if (exists) return NoContent();

            var library = new Library
            {
                UserId = userId,
                PodcastId = manageLibraryDto.PodcastId,
                PodcastEpisodeId = manageLibraryDto.PodcastEpisodeId,
                BookId = manageLibraryDto.BookId,
            };
            _context.Libraries.Add(library);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMe), library);
        }

        // POST: api/libraries/me/list
        [HttpPost("me/list")]
        [Authorize]
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

            var libraries = new List<Library>();
            foreach (var dto in manageLibraryDtos)
            {
                var exists = await _context.Libraries.AnyAsync(l =>
                    l.UserId == userId &&
                    l.PodcastId == dto.PodcastId &&
                    l.PodcastEpisodeId == dto.PodcastEpisodeId &&
                    l.BookId == dto.BookId);

                if (exists)
                    continue;

                libraries.Add(new Library
                {
                    UserId = userId,
                    PodcastId = dto.PodcastId,
                    PodcastEpisodeId = dto.PodcastEpisodeId,
                    BookId = dto.BookId,
                });
            }

            _context.Libraries.AddRange(libraries);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMe), new { }, libraries);
        }

        // DELETE: api/libraries/me
        [HttpDelete("me")]
        [Authorize]
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
        [HttpDelete("me/{id}")]
        [Authorize]
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
                return NoContent();
            }

            _context.Libraries.Remove(library);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/libraries/me/list
        [HttpDelete("me/list")]
        [Authorize]
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
                return NoContent();
            }

            _context.Libraries.RemoveRange(libraries);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
