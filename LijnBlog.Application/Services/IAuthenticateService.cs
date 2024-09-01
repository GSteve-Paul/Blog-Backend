using LijnBlog.Application.Models;
using LijnBlog.Application.Models.DTOs;

namespace LijnBlog.Application.Services;

public interface IAuthenticateService
{
    Task<AuthTokenDto> CreateAuthTokenAsync(Client client);

    Task<AuthTokenDto> RefreshAuthTokenAsync(Client client);

    Task RemoveAuthTokenAsync(Client client);

    Task<bool> CheckTokenCacheValidation(string token, string tokenType, Client client);
}
