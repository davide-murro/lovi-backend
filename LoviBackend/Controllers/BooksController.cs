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
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BooksController(ApplicationDbContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> Get()
        {
            var books = await _context.Books
                .Include(b => b.Readers).ThenInclude(r => r.Creator)
                .Include(b => b.Writers).ThenInclude(w => w.Creator)
                .ToListAsync();

            var dtos = books.Select(b => new BookDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Readers = b.Readers.Select(r => new CreatorDto
                {
                    Id = r.Creator.Id,
                    Nickname = r.Creator.Nickname,
                    Name = r.Creator.Name,
                    Surname = r.Creator.Surname
                }).ToList(),
                Writers = b.Writers.Select(w => new CreatorDto
                {
                    Id = w.Creator.Id,
                    Nickname = w.Creator.Nickname,
                    Name = w.Creator.Name,
                    Surname = w.Creator.Surname
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        // GET: api/books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> Get(int id)
        {
            var b = await _context.Books
                .Include(bk => bk.Readers).ThenInclude(r => r.Creator)
                .Include(bk => bk.Writers).ThenInclude(w => w.Creator)
                .FirstOrDefaultAsync(bk => bk.Id == id);
            if (b == null) return NotFound();

            var dto = new BookDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                DataUrl = Url.Action(nameof(Get), "Books", new { id = b.Id }, Request.Scheme),
                CoverImageUrl = b.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "Books", new { id = b.Id }, Request.Scheme) : null,
                CoverImagePreviewUrl = b.CoverImagePreviewPath != null ? Url.Action(nameof(GetCoverImage), "Books", new { id = b.Id, isPreview = true }, Request.Scheme) : null,
                AudioUrl = b.AudioPath != null ? Url.Action(nameof(GetAudio), "Books", new { id = b.Id }, Request.Scheme) : null,
                FileUrl = b.FilePath != null ? Url.Action(nameof(GetFile), "Books", new { id = b.Id }, Request.Scheme) : null,
                Readers = b.Readers.Select(r => new CreatorDto
                {
                    Id = r.Creator.Id,
                    Nickname = r.Creator.Nickname,
                    Name = r.Creator.Name,
                    Surname = r.Creator.Surname
                }).ToList(),
                Writers = b.Writers.Select(w => new CreatorDto
                {
                    Id = w.Creator.Id,
                    Nickname = w.Creator.Nickname,
                    Name = w.Creator.Name,
                    Surname = w.Creator.Surname
                }).ToList()
            };
            return Ok(dto);
        }

        // GET: api/books/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<BookDto>>> GetPaged([FromQuery] PagedQuery query)
        {
            // Base query from EF
            var booksQuery = _context.Books
                .Include(b => b.Readers).ThenInclude(r => r.Creator)
                .Include(b => b.Writers).ThenInclude(w => w.Creator)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search;
                booksQuery = booksQuery.Where(b =>
                    EF.Functions.Like(b.Name, $"%{search}%") ||
                    (b.Description != null && EF.Functions.Like(b.Description, $"%{search}%")) ||
                    b.Readers.Any(r => 
                        EF.Functions.Like(r.Creator.Nickname, $"%{search}%") ||
                        (r.Creator.Name != null && EF.Functions.Like(r.Creator.Name, $"%{search}%")) ||
                        (r.Creator.Surname != null && EF.Functions.Like(r.Creator.Surname, $"%{search}%"))
                    ) ||
                    b.Writers.Any(
                        w => EF.Functions.Like(w.Creator.Nickname, $"%{search}%") ||
                        (w.Creator.Name != null && EF.Functions.Like(w.Creator.Name, $"%{search}%")) ||
                        (w.Creator.Surname != null && EF.Functions.Like(w.Creator.Surname, $"%{search}%"))
                    )
                );
            }

            // Sorting
            if (string.IsNullOrEmpty(query.SortBy)) query.SortBy = "Id";
            var property = typeof(Book).GetProperty(query.SortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;
            booksQuery = query.SortOrder.ToLower() == "desc"
                ? booksQuery.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : booksQuery.OrderBy(e => EF.Property<object>(e, property.Name));

            // Total count (before pagination)
            var totalCount = await booksQuery.CountAsync();
            // Apply pagination and project to DTO
            var items = await booksQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    DataUrl = Url.Action(nameof(Get), "Books", new { id = b.Id }, Request.Scheme),
                    CoverImageUrl = b.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "Books", new { id = b.Id }, Request.Scheme) : null,
                    CoverImagePreviewUrl = b.CoverImagePreviewPath != null ? Url.Action(nameof(GetCoverImage), "Books", new { id = b.Id, isPreview = true }, Request.Scheme) : null,
                    Description = b.Description,
                    AudioUrl = b.AudioPath != null ? Url.Action(nameof(GetAudio), "Books", new { id = b.Id }, Request.Scheme) : null,
                    FileUrl = b.FilePath != null ? Url.Action(nameof(GetFile), "Books", new { id = b.Id }, Request.Scheme) : null,
                    Readers = b.Readers.Select(r => new CreatorDto
                    {
                        Id = r.Creator.Id,
                        Nickname = r.Creator.Nickname,
                        Name = r.Creator.Name,
                        Surname = r.Creator.Surname
                    }).ToList(),
                    Writers = b.Writers.Select(w => new CreatorDto
                    {
                        Id = w.Creator.Id,
                        Nickname = w.Creator.Nickname,
                        Name = w.Creator.Name,
                        Surname = w.Creator.Surname
                    }).ToList()
                })
                .ToListAsync();

            var result = new PagedResult<BookDto>
            {
                PagedQuery = query,
                Items = items,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        // GET: api/books/{id}/cover
        [HttpGet("{id}/cover")]
        public IActionResult GetCoverImage(int id, bool? isPreview)
        {
            var book = _context.Books.Find(id);
            if (book == null) return NotFound();

            var path = isPreview == true ? book.CoverImagePreviewPath : book.CoverImagePath;
            if (string.IsNullOrEmpty(path)) return NotFound();

            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!, path);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var contentType = "image/jpeg";
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fs, contentType, enableRangeProcessing: true);
        }

        // GET: api/books/{id}/audio
        [HttpGet("{id}/audio")]
        public IActionResult GetAudio(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null || string.IsNullOrEmpty(book.AudioPath)) return NotFound();

            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!, book.AudioPath);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var contentType = "audio/mpeg";
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fs, contentType, enableRangeProcessing: true);
        }

        // GET: api/books/{id}/file
        [HttpGet("{id}/file")]
        public IActionResult GetFile(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null || string.IsNullOrEmpty(book.FilePath)) return NotFound();

            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!, book.FilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var contentType = "application/epub+zip";
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fs, contentType, enableRangeProcessing: true);
        }

        // POST: api/books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(500_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
        public async Task<ActionResult<BookDto>> Create([FromForm] BookDto dto)
        {
            var book = new Book
            {
                Name = dto.Name,
                Description = dto.Description,
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var bookPath = Path.Combine("books", book.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, bookPath));

            if (dto.CoverImage != null)
            {
                var fileName = $"cover{Path.GetExtension(dto.CoverImage.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, bookPath, fileName), FileMode.Create);
                await dto.CoverImage.CopyToAsync(stream);
                book.CoverImagePath = Path.Combine(bookPath, fileName);
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
            if (dto.CoverImagePreview != null)
            {
                var fileName = $"cover-preview{Path.GetExtension(dto.CoverImagePreview.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, bookPath, fileName), FileMode.Create);
                await dto.CoverImagePreview.CopyToAsync(stream);
                book.CoverImagePreviewPath = Path.Combine(bookPath, fileName);
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
            if (dto.Audio != null)
            {
                var fileName = $"audio{Path.GetExtension(dto.Audio.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, bookPath, fileName), FileMode.Create);
                await dto.Audio.CopyToAsync(stream);
                book.AudioPath = Path.Combine(bookPath, fileName);
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
            if (dto.File != null)
            {
                var fileName = $"file{Path.GetExtension(dto.File.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, bookPath, fileName), FileMode.Create);
                await dto.File.CopyToAsync(stream);
                book.FilePath = Path.Combine(bookPath, fileName);
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
            }
            return CreatedAtAction(nameof(Get), new { id = book.Id }, new BookDto { Id = book.Id, Name = book.Name, Description = book.Description });
        }

        // PUT: api/books/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(500_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] BookDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            // edit audioBook
            book.UpdatedAt = DateTime.UtcNow;
            book.Name = dto.Name;
            book.Description = dto.Description;

            // Handle file upload
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var bookPath = Path.Combine("books", book.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, bookPath));

            if (dto.CoverImageUrl == null && book.CoverImagePath != null)
            {
                // Delete Existing File
                var oldFile = Path.Combine(uploadPath, book.CoverImagePath);
                if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                // Update the path in the database model
                book.CoverImagePath = null;
            }
            if (dto.CoverImage != null)
            {
                // Save the new file
                var fileName = $"cover{Path.GetExtension(dto.CoverImage.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, bookPath, fileName), FileMode.Create);
                await dto.CoverImage.CopyToAsync(stream);
                // Update the path in the database model
                book.CoverImagePath = Path.Combine(bookPath, fileName);
            }
            if (dto.CoverImagePreviewUrl == null && book.CoverImagePreviewPath != null)
            {
                var oldFile = Path.Combine(uploadPath, book.CoverImagePreviewPath);
                if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                book.CoverImagePreviewPath = null;
            }
            if (dto.CoverImagePreview != null)
            {
                var fileName = $"cover-preview{Path.GetExtension(dto.CoverImagePreview.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, bookPath, fileName), FileMode.Create);
                await dto.CoverImagePreview.CopyToAsync(stream);
                book.CoverImagePreviewPath = Path.Combine(bookPath, fileName);
            }
            if (dto.AudioUrl == null && book.AudioPath != null)
            {
                var oldFile = Path.Combine(uploadPath, book.AudioPath);
                if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                book.AudioPath = null;
            }
            if (dto.Audio != null)
            {
                var fileName = $"audio{Path.GetExtension(dto.Audio.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, bookPath, fileName), FileMode.Create);
                await dto.Audio.CopyToAsync(stream);
                book.AudioPath = Path.Combine(bookPath, fileName);
            }
            if (dto.FileUrl == null && book.FilePath != null)
            {
                var oldFile = Path.Combine(uploadPath, book.FilePath);
                if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                book.FilePath = null;
            }
            if (dto.File != null)
            {
                var fileName = $"file{Path.GetExtension(dto.File.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, bookPath, fileName), FileMode.Create);
                await dto.File.CopyToAsync(stream);
                book.FilePath = Path.Combine(bookPath, fileName);
            }
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/books/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            // handle files
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var bookPath = Path.Combine("books", book.Id.ToString());
            if (Directory.Exists(Path.Combine(uploadPath, bookPath))) Directory.Delete(Path.Combine(uploadPath, bookPath), true);
            
            return NoContent();
        }

        // GET: api/books/exists/{id}
        [HttpGet("exists/{id}")]
        public async Task<IActionResult> Exists(int id)
        {
            var exists = await _context.Books.AnyAsync(b => b.Id == id);
            if (exists) return Ok(true);
            return NotFound(false);
        }

        // POST: api/books/5/readers/2
        [HttpPost("{id}/readers/{readerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddReader(int id, int readerId)
        {
            var bookReader = new BookReader
            {
                BookId = id,
                CreatorId = readerId,
            };
            _context.BookReaders.Add(bookReader);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/books/5/readers/2
        [HttpDelete("{id}/readers/{readerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveReader(int id, int readerId)
        {
            var bookReader = await _context.BookReaders.FirstOrDefaultAsync(abr => abr.CreatorId == readerId);
            if (bookReader == null)
            {
                return NotFound();
            }

            _context.BookReaders.Remove(bookReader);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/books/5/writers/2
        [HttpPost("{id}/writers/{writerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddWriter(int id, int writerId)
        {
            var bookWriter = new BookWriter
            {
                BookId = id,
                CreatorId = writerId,
            };
            _context.BookWriters.Add(bookWriter);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/books/5/writers/2
        [HttpDelete("{id}/writers/{writerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveWriter(int id, int writerId)
        {
            var bookWriter = await _context.BookWriters.FirstOrDefaultAsync(abr => abr.CreatorId == writerId);
            if (bookWriter == null)
            {
                return NotFound();
            }

            _context.BookWriters.Remove(bookWriter);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
