namespace LijnBlog.Application.Models;

public partial class Blog
{
    public Guid BlogId { get; set; }

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public Guid AuthorId { get; set; }

    public Guid ImageId { get; set; }

    public long ClickCount { get; set; }

    public long LikeCount { get; set; }

    public bool Private { get; set; }

    public List<Category> Categories { get; set; } = new();

    public DateTime CreateTime { get; set; }

    public DateTime UpdateTime { get; set; }
}