using LoviBackend.Data;
using LoviBackend.Models.DbSets;
using LoviBackend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoviBackend.Models.Dtos.Pagination;
using System.Reflection;

namespace LoviBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EBooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public EBooksController(ApplicationDbContext context, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/e-books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EBookDto>>> Get()
        {
            var eBooks = await _context.EBooks
                .Include(e => e.Writers)
                    .ThenInclude(w => w.Creator)
                .ToListAsync();

            var dtos = eBooks.Select(e => new EBookDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                CoverImageUrl = e.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "EBooks", new { id = e.Id }, Request.Scheme) : null,
                CoverImagePreviewUrl = e.CoverImagePreviewPath != null ? Url.Action(nameof(GetCoverImage), "EBooks", new { id = e.Id, isPreview = true }, Request.Scheme) : null,
                FileUrl = e.FilePath != null ? Url.Action(nameof(GetFile), "EBooks", new { id = e.Id }, Request.Scheme) : null,
                Writers = e.Writers.Select(w => new CreatorDto
                {
                    Id = w.Creator.Id,
                    Nickname = w.Creator.Nickname,
                    Name = w.Creator.Name,
                    Surname = w.Creator.Surname
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        // GET: api/e-books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EBookDto>> Get(int id)
        {
            var eBook = await _context.EBooks
                .Include(e => e.Writers)
                    .ThenInclude(w => w.Creator)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eBook == null) return NotFound();

            var dto = new EBookDto
            {
                Id = eBook.Id,
                Name = eBook.Name,
                Description = eBook.Description,
                DataUrl = Url.Action(nameof(Get), "EBooks", new { id = eBook.Id }, Request.Scheme),
                CoverImageUrl = eBook.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "EBooks", new { id = eBook.Id }, Request.Scheme) : null,
                CoverImagePreviewUrl = eBook.CoverImagePreviewPath != null ? Url.Action(nameof(GetCoverImage), "EBooks", new { id = eBook.Id, isPreview = true }, Request.Scheme) : null,
                FileUrl = eBook.FilePath != null ? Url.Action(nameof(GetFile), "EBooks", new { id = eBook.Id }, Request.Scheme) : null,
                Writers = eBook.Writers.Select(w => new CreatorDto
                {
                    Id = w.Creator.Id,
                    Nickname = w.Creator.Nickname,
                    Name = w.Creator.Name,
                    Surname = w.Creator.Surname
                }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/e-books/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<EBookDto>>> GetPaged([FromQuery] PagedQuery query)
        {
            var eBooksQuery = _context.EBooks
                .Include(e => e.Writers)
                    .ThenInclude(w => w.Creator)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search;
                eBooksQuery = eBooksQuery.Where(e =>
                    EF.Functions.Like(e.Name, $"%{search}%") ||
                    (e.Description != null && EF.Functions.Like(e.Description, $"%{search}%")) ||
                    e.Writers.Any(w => EF.Functions.Like(w.Creator.Nickname, $"%{search}%"))
                );
            }

            if (string.IsNullOrEmpty(query.SortBy)) query.SortBy = "Id";
            var property = typeof(EBook).GetProperty(query.SortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;
            eBooksQuery = query.SortOrder.ToLower() == "desc"
                ? eBooksQuery.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : eBooksQuery.OrderBy(e => EF.Property<object>(e, property.Name));

            var totalCount = await eBooksQuery.CountAsync();

            var items = await eBooksQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new EBookDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    DataUrl = Url.Action(nameof(Get), "EBooks", new { id = e.Id }, Request.Scheme),
                    CoverImageUrl = e.CoverImagePath != null ? Url.Action(nameof(GetCoverImage), "EBooks", new { id = e.Id }, Request.Scheme) : null,
                    CoverImagePreviewUrl = e.CoverImagePreviewPath != null ? Url.Action(nameof(GetCoverImage), "EBooks", new { id = e.Id, isPreview = true }, Request.Scheme) : null,
                    FileUrl = e.FilePath != null ? Url.Action(nameof(GetFile), "EBooks", new { id = e.Id }, Request.Scheme) : null,
                    Description = e.Description,
                    Writers = e.Writers.Select(w => new CreatorDto
                    {
                        Id = w.Creator.Id,
                        Nickname = w.Creator.Nickname,
                        Name = w.Creator.Name,
                        Surname = w.Creator.Surname
                    }).ToList()
                })
                .ToListAsync();

            var result = new PagedResult<EBookDto>
            {
                PagedQuery = query,
                Items = items,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        // GET: api/e-books/5/cover
        [HttpGet("{id}/cover")]
        public IActionResult GetCoverImage(int id, bool? isPreview)
        {
            var eBook = _context.EBooks.Find(id);
            if (eBook == null) return NotFound();

            var path = isPreview == true ? eBook.CoverImagePreviewPath : eBook.CoverImagePath;
            if (string.IsNullOrEmpty(path)) return NotFound();

            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!, path);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var contentType = "image/jpeg";
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fs, contentType, enableRangeProcessing: true);
        }

        // GET: api/e-books/5/file
        [HttpGet("{id}/file")]
        public IActionResult GetFile(int id)
        {
            var eBook = _context.EBooks.Find(id);
            if (eBook == null || string.IsNullOrEmpty(eBook.FilePath)) return NotFound();

            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!, eBook.FilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var contentType = "application/epub+zip"; // common for EPUB; detecting by extension could be improved
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fs, contentType, enableRangeProcessing: true);
        }

        // POST: api/e-books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(500_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
        public async Task<ActionResult<EBookDto>> Create([FromForm] EBookDto dto)
        {
            var eBook = new EBook
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
            };

            _context.EBooks.Add(eBook);
            await _context.SaveChangesAsync();

            // handle images
            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var eBookPath = Path.Combine("e-books", eBook.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, eBookPath));

            if (dto.CoverImage != null)
            {
                var fileName = $"cover{Path.GetExtension(dto.CoverImage.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, eBookPath, fileName), FileMode.Create);
                await dto.CoverImage.CopyToAsync(stream);
                eBook.CoverImagePath = Path.Combine(eBookPath, fileName);
                _context.EBooks.Update(eBook);
                await _context.SaveChangesAsync();
            }

            if (dto.CoverImagePreview != null)
            {
                var fileName = $"cover-preview{Path.GetExtension(dto.CoverImagePreview.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, eBookPath, fileName), FileMode.Create);
                await dto.CoverImagePreview.CopyToAsync(stream);
                eBook.CoverImagePreviewPath = Path.Combine(eBookPath, fileName);
                _context.EBooks.Update(eBook);
                await _context.SaveChangesAsync();
            }

            // handle ebook file (e.g. .epub)
            if (dto.File != null)
            {
                var fileName = $"file{Path.GetExtension(dto.File.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadPath, eBookPath, fileName), FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                eBook.FilePath = Path.Combine(eBookPath, fileName);
                _context.EBooks.Update(eBook);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(Get), new { id = eBook.Id }, new EBookDto { 
                Id = eBook.Id, 
                Name = eBook.Name, 
                Description = eBook.Description 
            });
        }

        // PUT: api/e-books/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(500_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] EBookDto dto)
        {
            if (id != dto.Id) return BadRequest();
            var eBook = await _context.EBooks.FindAsync(id);
            if (eBook == null) return NotFound();

            eBook.UpdatedAt = DateTime.UtcNow;
            eBook.Name = dto.Name;
            eBook.Description = dto.Description;

            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var eBookPath = Path.Combine("e-books", eBook.Id.ToString());
            Directory.CreateDirectory(Path.Combine(uploadPath, eBookPath));

            if (dto.CoverImageUrl == null && eBook.CoverImagePath != null)
            {
                var oldFile = Path.Combine(uploadPath, eBook.CoverImagePath);
                if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                eBook.CoverImagePath = null;
            }
            if (dto.CoverImage != null)
            {
                var fileName = $"cover{Path.GetExtension(dto.CoverImage.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, eBookPath, fileName), FileMode.Create);
                await dto.CoverImage.CopyToAsync(stream);
                eBook.CoverImagePath = Path.Combine(eBookPath, fileName);
            }

            if (dto.CoverImagePreviewUrl == null && eBook.CoverImagePreviewPath != null)
            {
                var oldFile = Path.Combine(uploadPath, eBook.CoverImagePreviewPath);
                if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
                eBook.CoverImagePreviewPath = null;
            }
            if (dto.CoverImagePreview != null)
            {
                var fileName = $"cover-preview{Path.GetExtension(dto.CoverImagePreview.FileName)}";
                using var stream = new FileStream(Path.Combine(uploadPath, eBookPath, fileName), FileMode.Create);
                await dto.CoverImagePreview.CopyToAsync(stream);
                eBook.CoverImagePreviewPath = Path.Combine(eBookPath, fileName);
            }

            // handle file replacement via form field `File` if provided
            if (dto.FileUrl == null && eBook.FilePath != null)
            {
                // Delete Existing File
                var oldFilePath = Path.Combine(uploadPath, eBook.FilePath);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update the path in the database model
                eBook.FilePath = null;
            }
            if (dto.File != null)
            {
                // Save the new file
                var fileName = $"file{Path.GetExtension(dto.File.FileName)}";
                using (var stream = new FileStream(Path.Combine(uploadPath, eBookPath, fileName), FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                // Update the path in the database model
                eBook.FilePath = Path.Combine(eBookPath, fileName);
            }

            _context.EBooks.Update(eBook);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/e-books/5/writers/2
        [HttpPost("{id}/writers/{writerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddWriter(int id, int writerId)
        {
            var eBook = await _context.EBooks.FindAsync(id);
            if (eBook == null) return NotFound();

            var exists = await _context.EBookWriters.AnyAsync(ew => ew.EBookId == id && ew.CreatorId == writerId);
            if (exists) return NoContent();

            var ew = new EBookWriter { EBookId = id, CreatorId = writerId };
            _context.EBookWriters.Add(ew);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/e-books/5/writers/2
        [HttpDelete("{id}/writers/{writerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveWriter(int id, int writerId)
        {
            var ew = await _context.EBookWriters.FirstOrDefaultAsync(x => x.EBookId == id && x.CreatorId == writerId);
            if (ew == null) return NotFound();

            _context.EBookWriters.Remove(ew);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/e-books/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var eBook = await _context.EBooks.FindAsync(id);
            if (eBook == null) return NotFound();

            _context.EBooks.Remove(eBook);
            await _context.SaveChangesAsync();

            var uploadPath = Path.Combine(_hostingEnvironment.ContentRootPath, _configuration["UploadsPath"]!);
            var eBookPath = Path.Combine("e-books", eBook.Id.ToString());
            if (Directory.Exists(Path.Combine(uploadPath, eBookPath))) Directory.Delete(Path.Combine(uploadPath, eBookPath), true);

            return NoContent();
        }

        // GET: api/e-books/exists/5
        [HttpGet("exists/{id}")]
        public async Task<IActionResult> Exists(int id)
        {
            var eBookExists = await _context.EBooks.AnyAsync(e => e.Id == id);

            if (eBookExists)
            {
                return Ok(true); // Return 200 OK with a true value
            }

            return NotFound(false); // Return 404 Not Found with a false value
        }
    }
}
