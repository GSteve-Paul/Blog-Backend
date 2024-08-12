namespace LijnBlog.Application.Models;

public class Image
{
    public Guid ImageId { get; set; }
    
    public string FilePath { get; set; }
    
    public string ZipFilePath { get; set; }
}