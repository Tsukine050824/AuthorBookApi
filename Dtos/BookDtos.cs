namespace AuthorBookApi.Dtos;

public record CreateBookDto(string Title, int? PublishedYear);

public class BookDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? PublishedYear { get; set; }

    public PublisherMiniDto? Publisher { get; set; }
    public List<AuthorMiniDto> Authors { get; set; } = new();
}

public class AuthorMiniDto
{
    public int AuthorId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PublisherMiniDto
{
    public int PublisherId { get; set; }
    public string Name { get; set; } = string.Empty;
}
