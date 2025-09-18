using AuthorBookApi.Data;
using AuthorBookApi.Dtos;
using AuthorBookApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthorBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthorsController(AppDbContext db) => _db = db;

    // POST: api/authors
    [HttpPost]
    public async Task<ActionResult<Author>> Create([FromBody] CreateAuthorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        var author = new Author { Name = dto.Name.Trim(), BirthYear = dto.BirthYear };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = author.AuthorId }, author);
    }

    // GET: api/authors/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Author>> GetById(int id)
    {
        var author = await _db.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.AuthorId == id);
        return author is null ? NotFound() : Ok(author);
    }

    // GET: api/authors
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAll()
    {
        var data = await _db.Authors
            .Include(a => a.Books)
            .Select(a => new AuthorDto
            {
                AuthorId = a.AuthorId,
                Name = a.Name,
                BirthYear = a.BirthYear,
                Books = a.Books.Select(b => new BookMiniDto { BookId = b.BookId, Title = b.Title }).ToList()
            })
            .ToListAsync();

        return Ok(data);
    }
}
