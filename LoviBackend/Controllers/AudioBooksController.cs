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
    public class AudioBooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AudioBooksController(
            ApplicationDbContext context,
            IConfiguration configuration,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/audio-books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AudioBookDto>>> Get()
        {
            var audioBooks = await _context.AudioBooks.ToListAsync();

            var audioBookDtos = audioBooks.Select(audioBook => new AudioBookDto
            {
                Id = audioBook.Id,
                Name = audioBook.Name,
            }).ToList();

            return Ok(audioBookDtos);
        }

        // GET: api/audio-books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AudioBookDto>> Get(int id)
        {
            var audioBook = await _context.AudioBooks.FindAsync(id);

            if (audioBook == null)
            {
                return NotFound();
            }

            var audioBookDto = new AudioBookDto
            {
                Id = audioBook.Id,
                Name = audioBook.Name,
                Description = audioBook.Description,
                CoverImageUrl = audioBook.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "AudioBooks", new { id = audioBook.Id }, Request.Scheme) : null,
                AudioUrl = Url.Action(nameof(GetAudio), "AudioBooks", new { id = audioBook.Id }, Request.Scheme)!,
            };

            return audioBookDto;
        }

        // PUT: api/audio-books/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, AudioBookDto audioBookDto)
        {
            if (id != audioBookDto.Id)
            {
                return BadRequest();
            }

            _context.Entry(audioBookDto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.AudioBooks.Any(e => e.Id == id))
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

        // POST: api/audio-books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AudioBookDto>> Create(AudioBookDto audioBookDto)
        {
            var audioBook = new AudioBook
            {
                Id = audioBookDto.Id,
                Name = audioBookDto.Name,
                Description = audioBookDto.Description,
            };
            _context.AudioBooks.Add(audioBook);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAudioBook", new { id = audioBook.Id }, audioBook);
        }

        // DELETE: api/audio-books/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var audioBook = await _context.AudioBooks.FindAsync(id);
            if (audioBook == null)
            {
                return NotFound();
            }

            _context.AudioBooks.Remove(audioBook);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/audio-books/exists/5
        [HttpGet("exists/{id}")]
        public async Task<IActionResult> Exists(int id)
        {
            var audioBookExists = await _context.AudioBooks.AnyAsync(e => e.Id == id);

            if (audioBookExists)
            {
                return Ok(true);
            }

            return NotFound(false);
        }


        // GET: api/audio-books/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<AudioBookDto>>> GetPaged([FromQuery] PagedQuery query)
        {
            // Base query from EF
            var audioBooksQuery = _context.AudioBooks.AsNoTracking();

            // Sorting
            if (string.IsNullOrEmpty(query.SortBy)) query.SortBy = "Id";
            var property = typeof(AudioBook).GetProperty(query.SortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;
            audioBooksQuery = query.SortOrder.ToLower() == "desc"
                ? audioBooksQuery.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : audioBooksQuery.OrderBy(e => EF.Property<object>(e, property.Name));

            // Total count (before pagination)
            var totalCount = await audioBooksQuery.CountAsync();

            // Apply pagination and project to DTO
            var items = await audioBooksQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(ab => new AudioBookDto
                {
                    Id = ab.Id,
                    Name = ab.Name,
                    CoverImageUrl = ab.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "AudioBooks", new { id = ab.Id }, Request.Scheme) : null,
                    AudioUrl = Url.Action(nameof(GetAudio), "AudioBooks", new { id = ab.Id }, Request.Scheme)!,
                })
                .ToListAsync();

            // Wrap result
            var result = new PagedResult<AudioBookDto>
            {
                PagedQuery = query,
                Items = items,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        // GET: api/audio-books/5/cover
        [HttpGet("{id}/cover")]
        public IActionResult GetCoverImage(int id)
        {
            var audioBook = _context.AudioBooks.Find(id);
            if (audioBook == null || string.IsNullOrEmpty(audioBook.CoverImagePath))
                return NotFound();

            var filePath = Path.Combine(
                _hostingEnvironment.ContentRootPath,
                _configuration["UploadsPath"]!,
                audioBook.CoverImagePath
            );
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "image/jpeg"; // optionally detect by extension
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return File(fileStream, contentType, enableRangeProcessing: true);
        }

        // GET: api/audio-books/1/audio
        [HttpGet("{id}/audio")]
        public IActionResult GetAudio(int id)
        {
            var audioBook = _context.AudioBooks.Find(id);
            if (audioBook == null || string.IsNullOrEmpty(audioBook.AudioPath))
                return NotFound();

            var filePath = Path.Combine(
                _hostingEnvironment.ContentRootPath,
                _configuration["UploadsPath"]!,
                audioBook.AudioPath
            );
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "audio/mpeg";
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return File(fileStream, contentType, enableRangeProcessing: true);
        }

    }
}
