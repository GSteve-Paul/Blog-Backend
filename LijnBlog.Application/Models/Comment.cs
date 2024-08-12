namespace LijnBlog.Application.Models;

public class Comment
{
    public Guid CommentId { get; set; }
    
    public string? Text { get; set; }
    
    public Guid AuthorId { get; set; }
    
    public Guid ReplyToId { get; set; }
    
    public Guid BlogId { get; set; }
    
    public DateTime CreateTime { get; set; }
    
    public DateTime UpdateTime { get; set; }
}