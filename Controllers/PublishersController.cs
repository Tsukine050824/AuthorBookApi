using AuthorBookApi.Data;
using AuthorBookApi.Dtos;
using AuthorBookApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthorBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublishersController : ControllerBase
{
    private readonly AppDbContext _db;
    public PublishersController(AppDbContext db) => _db = db;

    // POST: api/publishers
    [HttpPost]
    public async Task<ActionResult<Publisher>> Create([FromBody] CreatePublisherDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        var p = new Publisher { Name = dto.Name.Trim(), Country = dto.Country };
        _db.Publishers.Add(p);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = p.PublisherId }, p);
    }

    // GET: api/publishers/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PublisherDto>> GetById(int id)
    {
        var pub = await _db.Publishers
            .Include(p => p.Books)
            .FirstOrDefaultAsync(p => p.PublisherId == id);

        if (pub is null) return NotFound();

        var dto = new PublisherDto
        {
            PublisherId = pub.PublisherId,
            Name = pub.Name,
            Country = pub.Country,
            Books = pub.Books.Select(b => new BookMiniDto { BookId = b.BookId, Title = b.Title }).ToList()
        };
        return Ok(dto);
    }

    // GET: api/publishers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PublisherDto>>> GetAll()
    {
        var data = await _db.Publishers
            .Include(p => p.Books)
            .Select(pub => new PublisherDto
            {
                PublisherId = pub.PublisherId,
                Name = pub.Name,
                Country = pub.Country,
                Books = pub.Books.Select(b => new BookMiniDto { BookId = b.BookId, Title = b.Title }).ToList()
            })
            .ToListAsync();

        return Ok(data);
    }
}
