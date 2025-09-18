using AuthorBookApi.Data;
using AuthorBookApi.Dtos;
using AuthorBookApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthorBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _db;
    public BooksController(AppDbContext db) => _db = db;

    // POST: api/books
    [HttpPost]
    public async Task<ActionResult<Book>> Create([FromBody] CreateBookDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Title is required.");

        var book = new Book { Title = dto.Title.Trim(), PublishedYear = dto.PublishedYear };
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = book.BookId }, book);
    }

    // GET: api/books/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var b = await _db.Books
            .Include(b => b.Authors)
            .Include(b => b.Publisher)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (b is null) return NotFound();

        var dto = new BookDto
        {
            BookId = b.BookId,
            Title = b.Title,
            PublishedYear = b.PublishedYear,
            Publisher = b.Publisher is null ? null : new PublisherMiniDto
            {
                PublisherId = b.Publisher.PublisherId,
                Name = b.Publisher.Name
            },
            Authors = b.Authors.Select(a => new AuthorMiniDto
            {
                AuthorId = a.AuthorId,
                Name = a.Name
            }).ToList()
        };
        return Ok(dto);
    }

    // GET: api/books  (kèm Author & Publisher)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
    {
        var data = await _db.Books
            .Include(b => b.Authors)
            .Include(b => b.Publisher)
            .Select(b => new BookDto
            {
                BookId = b.BookId,
                Title = b.Title,
                PublishedYear = b.PublishedYear,
                Publisher = b.Publisher == null ? null : new PublisherMiniDto
                {
                    PublisherId = b.Publisher.PublisherId,
                    Name = b.Publisher.Name
                },
                Authors = b.Authors.Select(a => new AuthorMiniDto
                {
                    AuthorId = a.AuthorId,
                    Name = a.Name
                }).ToList()
            })
            .ToListAsync();

        return Ok(data);
    }

    // POST: api/books/{bookId}/attach-author/{authorId}
    [HttpPost("{bookId:int}/attach-author/{authorId:int}")]
    public async Task<IActionResult> AttachAuthor(int bookId, int authorId)
    {
        var book = await _db.Books.Include(b => b.Authors)
            .FirstOrDefaultAsync(b => b.BookId == bookId);
        if (book is null) return NotFound($"Book {bookId} not found");

        var author = await _db.Authors.FindAsync(authorId);
        if (author is null) return NotFound($"Author {authorId} not found");

        if (!book.Authors.Any(a => a.AuthorId == authorId))
        {
            book.Authors.Add(author);
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }

    // POST: api/books/{bookId}/set-publisher/{publisherId}
    [HttpPost("{bookId:int}/set-publisher/{publisherId:int}")]
    public async Task<IActionResult> SetPublisher(int bookId, int publisherId)
    {
        var book = await _db.Books.FindAsync(bookId);
        if (book is null) return NotFound($"Book {bookId} not found");

        var publisher = await _db.Publishers.FindAsync(publisherId);
        if (publisher is null) return NotFound($"Publisher {publisherId} not found");

        book.PublisherId = publisherId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ========= LINQ QUERIES =========

    // 1) Tìm tất cả sách của một Author
    // GET: api/books/by-author/{authorId}
    [HttpGet("by-author/{authorId:int}")]
    public async Task<ActionResult<IEnumerable<BookMiniDto>>> GetBooksByAuthor(int authorId)
    {
        var books = await _db.Books
            .Where(b => b.Authors.Any(a => a.AuthorId == authorId))
            .Select(b => new BookMiniDto { BookId = b.BookId, Title = b.Title })
            .ToListAsync();
        return Ok(books);
    }

    // GET: api/books/authors-gt2-groupby
// Mục đích: Tác giả có > 2 sách, dùng GROUP BY + COUNT
[HttpGet("authors-gt2-groupby")]
public async Task<ActionResult<IEnumerable<AuthorMiniDto>>> GetAuthorsWithMoreThanTwoBooks_GroupBy()
{
    var authors = await _db.Authors
        // “nổ” quan hệ Many-to-Many: mỗi (Author, Book) là 1 bản ghi
        .SelectMany(a => a.Books.Select(b => new { a.AuthorId, a.Name }))
        // GroupBy theo tác giả
        .GroupBy(x => new { x.AuthorId, x.Name })
        // Count từng nhóm > 2
        .Where(g => g.Count() > 2)
        // Trả về DTO gọn
        .Select(g => new AuthorMiniDto { AuthorId = g.Key.AuthorId, Name = g.Key.Name })
        .ToListAsync();

    return Ok(authors);
}


    // 3) Tìm các sách xuất bản sau năm X (mặc định 2015)
    // GET: api/books/after-year/{year?}
    [HttpGet("after-year/{year:int?}")]
    public async Task<ActionResult<IEnumerable<BookMiniDto>>> GetBooksAfterYear(int? year = 2015)
    {
        int y = year ?? 2015;
        var books = await _db.Books
            .Where(b => b.PublishedYear != null && b.PublishedYear > y)
            .Select(b => new BookMiniDto { BookId = b.BookId, Title = b.Title })
            .ToListAsync();
        return Ok(books);
    }

    // 4) Liệt kê Publisher có ít nhất n cuốn sách (mặc định 3)
    // GET: api/books/publishers-atleast/{min?}
    [HttpGet("publishers-atleast/{min:int?}")]
    public async Task<ActionResult<IEnumerable<PublisherMiniDto>>> GetPublishersWithAtLeast(int? min = 3)
    {
        int m = min ?? 3;
        var pubs = await _db.Publishers
            .Where(p => p.Books.Count() >= m)
            .Select(p => new PublisherMiniDto { PublisherId = p.PublisherId, Name = p.Name })
            .ToListAsync();
        return Ok(pubs);
    }

    // GET: api/books/join-abp
[HttpGet("join-abp")]
// GET: api/books/join-abp
// Mục đích: Truy vấn dữ liệu kết hợp (Join) giữa Author – Book – Publisher
// Trả về: Danh sách (AuthorName, BookTitle, PublisherName)
// Ví dụ JSON:
/*
[
  { "authorName": "Nguyễn Nhật Ánh", "bookTitle": "Mắt biếc", "publisherName": "NXB Trẻ" },
  { "authorName": "Haruki Murakami", "bookTitle": "Rừng Na Uy", "publisherName": "Vintage" }
]
*/
[HttpGet("join-abp")]
public async Task<ActionResult<IEnumerable<object>>> JoinAuthorBookPublisher()
{
    // 1) Lấy dữ liệu thô từ DB:
    //    - BookTitle
    //    - PublisherName (nếu null => "(No Publisher)")
    //    - Authors (danh sách tên tác giả cho từng Book)
    // EF Core sẽ dịch ra SQL đơn giản (LEFT JOIN), sau đó trả dữ liệu vào bộ nhớ.
    var rows = await _db.Books
        .Select(b => new
        {
            BookTitle = b.Title,
            PublisherName = b.Publisher != null ? b.Publisher.Name : "(No Publisher)",
            Authors = b.Authors.Select(a => a.Name).ToList() // list tên tác giả
        })
        .AsNoTracking() // không cần tracking EF để tối ưu
        .ToListAsync();

    // 2) Flatten dữ liệu trong bộ nhớ (LINQ to Objects):
    //    - Vì mỗi Book có thể có nhiều Authors
    //    - Sử dụng SelectMany để “nổ” từng Author thành 1 dòng riêng
    //    - Nếu sách chưa có tác giả nào -> gán "(No Author)"
    var result = rows
        .SelectMany(
            r => r.Authors.DefaultIfEmpty("(No Author)"),
            (r, authorName) => new
            {
                AuthorName = authorName,
                BookTitle = r.BookTitle,
                PublisherName = r.PublisherName
            })
        .ToList();

    // 3) Trả kết quả về cho client (Swagger/Postman)
    return Ok(result);
}


}
