namespace AuthorBookApi.Dtos;

public record CreatePublisherDto(string Name, string? Country);

public class PublisherDto
{
    public int PublisherId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public List<BookMiniDto> Books { get; set; } = new();
}
