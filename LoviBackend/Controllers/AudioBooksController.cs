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
            var audioBooks = await _context.AudioBooks
                .Include(ab => ab.Readers)
                    .ThenInclude(r => r.Creator)
                .ToListAsync();

            var audioBookDtos = audioBooks.Select(audioBook => new AudioBookDto
            {
                Id = audioBook.Id,
                Name = audioBook.Name,
                Description = audioBook.Description,
                Readers = audioBook.Readers.Select(v => new CreatorDto
                {
                    Id = v.Creator.Id,
                    Nickname = v.Creator.Nickname,
                    Name = v.Creator.Name,
                    Surname = v.Creator.Surname
                }).ToList()
            }).ToList();

            return Ok(audioBookDtos);
        }

        // GET: api/audio-books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AudioBookDto>> Get(int id)
        {
            var audioBook = await _context.AudioBooks
                .Include(ab => ab.Readers)
                    .ThenInclude(r => r.Creator)
                .FirstOrDefaultAsync(ab => ab.Id == id);

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
                AudioUrl = audioBook.AudioPath != null ? Url.Action(nameof(GetAudio), "AudioBooks", new { id = audioBook.Id }, Request.Scheme) : null,
                Readers = audioBook.Readers.Select(v => new CreatorDto
                {
                    Id = v.Creator.Id,
                    Nickname = v.Creator.Nickname,
                    Name = v.Creator.Name,
                    Surname = v.Creator.Surname
                }).ToList()
            };

            return audioBookDto;
        }

        // PUT: api/audio-books/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(500_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] AudioBookDto audioBookDto)
        {
            if (id != audioBookDto.Id)
            {
                return BadRequest();
            }

            var audioBook = await _context.AudioBooks.FirstOrDefaultAsync((ab) => ab.Id == id);
            if (audioBook == null)
            {
                return NotFound();
            }

            // edit audioBook
            audioBook.UpdatedAt = DateTime.UtcNow;
            audioBook.Name = audioBookDto.Name;
            audioBook.Description = audioBookDto.Description;

            // Handle file upload
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var audioBookPath = Path.Combine("audio-books", audioBook.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, audioBookPath));

            if (audioBookDto.CoverImageUrl == null && audioBook.CoverImagePath != null)
            {
                // Delete Existing File
                var oldFilePath = Path.Combine(uploadPath, audioBook.CoverImagePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update the path in the database model
                audioBook.CoverImagePath = null;
            }
            if (audioBookDto.CoverImage != null)
            {
                // Save the new file
                var fileName = $"cover{Path.GetExtension(audioBookDto.CoverImage.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadPath, audioBookPath, fileName), FileMode.Create))
                {
                    await audioBookDto.CoverImage.CopyToAsync(stream);
                }

                // Update the path in the database model
                audioBook.CoverImagePath = Path.Combine(audioBookPath, fileName);
            }

            if (audioBookDto.AudioUrl == null && audioBook.AudioPath != null)
            {
                // Delete Existing File
                var oldFilePath = Path.Combine(uploadPath, audioBook.AudioPath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update the path in the database model
                audioBook.AudioPath = null;
            }
            if (audioBookDto.Audio != null)
            {
                // Save the new file
                var fileName = $"cover{Path.GetExtension(audioBookDto.Audio.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadPath, audioBookPath, fileName), FileMode.Create))
                {
                    await audioBookDto.Audio.CopyToAsync(stream);
                }

                // Update the path in the database model
                audioBook.AudioPath = Path.Combine(audioBookPath, fileName);
            }

            _context.AudioBooks.Update(audioBook);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/audio-books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(500_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
        public async Task<ActionResult<AudioBookDto>> Create([FromForm] AudioBookDto audioBookDto)
        {
            var audioBook = new AudioBook
            {
                Id = audioBookDto.Id,
                Name = audioBookDto.Name,
                Description = audioBookDto.Description,
            };

            _context.AudioBooks.Add(audioBook);
            await _context.SaveChangesAsync();

            // Handle file upload
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var audioBookPath = Path.Combine("audio-books", audioBook.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, audioBookPath));

            if (audioBookDto.CoverImage != null)
            {
                var fileName = $"cover{Path.GetExtension(audioBookDto.CoverImage.FileName)}";

                using (var stream = new FileStream(Path.Combine(uploadPath, audioBookPath, fileName), FileMode.Create))
                {
                    await audioBookDto.CoverImage.CopyToAsync(stream);
                }

                audioBook.CoverImagePath = Path.Combine(audioBookPath, fileName);

                _context.AudioBooks.Update(audioBook);
                await _context.SaveChangesAsync();
            }
            if (audioBookDto.Audio != null)
            {
                var fileName = $"audio{Path.GetExtension(audioBookDto.Audio.FileName)}";

                using (var stream = new FileStream(Path.Combine(uploadPath, audioBookPath, fileName), FileMode.Create))
                {
                    await audioBookDto.Audio.CopyToAsync(stream);
                }

                audioBook.AudioPath = Path.Combine(audioBookPath, fileName);

                _context.AudioBooks.Update(audioBook);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(Get), new { id = audioBook.Id }, new PodcastEpisodeDto
            {
                Id = audioBook.Id,
                Name = audioBook.Name,
                Description = audioBook.Description,
                CoverImageUrl = audioBook.CoverImagePath,
                AudioUrl = audioBook.AudioPath
            });
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
            var audioBooksQuery = _context.AudioBooks
                .Include(ab => ab.Readers)
                    .ThenInclude(r => r.Creator)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.ToLower();

                audioBooksQuery = audioBooksQuery.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Description == null || p.Description.ToLower().Contains(search) ||
                    p.Readers.Any(v =>
                        v.Creator.Nickname.ToLower().Contains(search) ||
                        v.Creator.Name == null || v.Creator.Name.ToLower().Contains(search) ||
                        v.Creator.Surname == null || v.Creator.Surname.ToLower().Contains(search)
                    )
                );
            }

            // Sorting
            if (string.IsNullOrEmpty(query.SortBy)) query.SortBy = "Id";
            var property = typeof(AudioBook).GetProperty(query.SortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;
            audioBooksQuery = query.SortOrder.ToLower() == "desc"
                ? audioBooksQuery.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : audioBooksQuery.OrderBy(e => EF.Property<object>(e, property.Name));

            // Total count (before pagination)
            var totalCount = await audioBooksQuery.CountAsync();
            var aa = await audioBooksQuery.ToListAsync();

            // Apply pagination and project to DTO
            var items = await audioBooksQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(ab => new AudioBookDto
                {
                    Id = ab.Id,
                    Name = ab.Name,
                    CoverImageUrl = ab.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "AudioBooks", new { id = ab.Id }, Request.Scheme) : null,
                    Description = ab.Description,
                    AudioUrl = ab.AudioPath != null ? Url.Action(nameof(GetAudio), "AudioBooks", new { id = ab.Id }, Request.Scheme) : null,
                    Readers = ab.Readers.Select(v => new CreatorDto
                    {
                        Id = v.Creator.Id,
                        Nickname = v.Creator.Nickname,
                        Name = v.Creator.Name,
                        Surname = v.Creator.Surname
                    }).ToList()
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

        // GET: api/audio-books/5/audio
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

        // POST: api/audio-books/5/readers/2
        [HttpPost("{id}/readers/{readerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddVoicer(int id, int readerId)
        {
            var audioBookReader = new AudioBookReader
            {
                AudioBookId = id,
                CreatorId = readerId,
            };
            _context.AudioBookReaders.Add(audioBookReader);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/audio-books/5/readers/2
        [HttpDelete("{id}/readers/{readerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveVoicer(int id, int readerId)
        {
            var audioBookReader = await _context.AudioBookReaders.FirstOrDefaultAsync(abr => abr.CreatorId == readerId);
            if (audioBookReader == null)
            {
                return NotFound();
            }

            _context.AudioBookReaders.Remove(audioBookReader);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
