namespace AuthorBookApi.Dtos;

public record CreateAuthorDto(string Name, int? BirthYear);

public class AuthorDto
{
    public int AuthorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? BirthYear { get; set; }
    public List<BookMiniDto> Books { get; set; } = new();
}

public class BookMiniDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
}
