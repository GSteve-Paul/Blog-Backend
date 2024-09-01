namespace LijnBlog.Application.Models.DTOs;

public class AuthTokenDto
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required Guid ClientId { get; init; }
}
