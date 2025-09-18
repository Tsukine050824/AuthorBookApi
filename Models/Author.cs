namespace AuthorBookApi.Models;

public class Author
{
    public int AuthorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? BirthYear { get; set; }   // thêm BirthYear (có thể null)

    public ICollection<Book> Books { get; set; } = new List<Book>();
}