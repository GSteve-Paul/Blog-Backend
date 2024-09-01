namespace LijnBlog.Api.Contracts.Responses;

public class UnauthorizedResponse
{
    public string Msg { get; init; } = "Token is invalid.";
}
