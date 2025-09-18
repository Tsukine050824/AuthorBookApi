namespace AuthorBookApi.Models;

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? PublishedYear { get; set; }  // thêm PublishedYear

    // One-to-Many: Publisher - Book
    public int? PublisherId { get; set; }    // có thể chưa gán publisher
    public Publisher? Publisher { get; set; }

    // Many-to-Many: Author - Book
    public ICollection<Author> Authors { get; set; } = new List<Author>();
}
