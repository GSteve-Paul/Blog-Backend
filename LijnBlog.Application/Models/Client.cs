namespace LijnBlog.Application.Models;

public class Client
{
    public Guid ClientId { get; set; }
    
    public string Name { get; set; }
    
    public string Password { get; set; }
    
    public string Link { get; set; }
    
    public int Role { get; set; }
    
    public DateTime banned { get; set; }
    
    public DateTime CreateTime { get; set; }
    
    public DateTime UpdateTime { get; set; }
}