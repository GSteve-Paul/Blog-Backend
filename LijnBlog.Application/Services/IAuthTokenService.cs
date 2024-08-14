using LijnBlog.Application.Models;

namespace LijnBlog.Application.Services;

public interface IAuthTokenService
{
    Task<AuthTokenDto> CreateAuthTokenAsync(Client client);

    Task<AuthTokenDto> RefreshAuthTokenAsync(Client client);

    void RemoveAuthTokenAsync(Client client);
}
