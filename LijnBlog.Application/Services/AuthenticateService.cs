using IdentityModel;
using LijnBlog.Application.Cache;
using LijnBlog.Application.Constants;
using LijnBlog.Application.Models;
using LijnBlog.Application.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace LijnBlog.Application.Services;

public class AuthenticateService : IAuthenticateService
{
    private readonly IConfiguration _config;

    private readonly RsaSecurityKey rsaPrivateKey;

    private readonly IDatabase _cache;

    public AuthenticateService(IConfiguration configuration, ICacheConnectionFactory cacheConnectionfactory)
    {
        _config = configuration;
        rsaPrivateKey = new(JsonSerializer.Deserialize<RSAParameters>(_config["Jwt:PrivateKey"]!));
        _cache = cacheConnectionfactory.CreateConnectionAsync();
    }

    private static string AccessTokenCacheKey(Client client) => AuthenticateConstant.Cache.AccessTokenTag + client.ClientId;

    private static string RefreshTokenCacheKey(Client client) => AuthenticateConstant.Cache.RefreshTokenTag + client.ClientId;

    private async Task<string> CreateAccessTokenAsync(Client client, Guid refreshTokenId)
    {
        int expireHours = int.Parse(_config["Jwt:AccessTokenExpireTime"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new (JwtClaimTypes.Id, client.ClientId.ToString()),
                new (JwtClaimTypes.Name, client.Name),
                new (JwtClaimTypes.Role, client.Role.ToString()),
                new (AuthenticateConstant.JwtClaimTypes.BannedClaimType, client.Banned.ToString()),
                new (AuthenticateConstant.JwtClaimTypes.RefreshTokenIdClaimType, refreshTokenId.ToString())
                ]
            ),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(expireHours),
            SigningCredentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha256Signature)
        };
        JwtSecurityTokenHandler handler = new();
        var token = handler.CreateJwtSecurityToken(tokenDescriptor);
        string strToken = handler.WriteToken(token);

        await _cache.StringSetAsync(AccessTokenCacheKey(client), strToken, new TimeSpan(hours: expireHours, 0, 0));

        return handler.WriteToken(token);
    }

    private async Task<(Guid refreshTokenId, string refreshToken)> CreateRefreshTokenAsync(Client client)
    {
        var tokenId = Guid.NewGuid();
        int expireHours = int.Parse(_config["Jwt:RefreshTokenExpireTime"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new (JwtClaimTypes.Id,client.ClientId.ToString()),
                new (JwtClaimTypes.Name, client.Name),
                new (JwtClaimTypes.Role, client.Role.ToString())
                ]
            ),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(expireHours),
            SigningCredentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha256Signature)
        };
        JwtSecurityTokenHandler handler = new();
        var token = handler.CreateJwtSecurityToken(tokenDescriptor);
        string strToken = handler.WriteToken(token);

        await _cache.StringSetAsync(RefreshTokenCacheKey(client), strToken, new TimeSpan(hours: expireHours, 0, 0);

        return (tokenId, strToken);
    }

    public async Task<AuthTokenDto> CreateAuthTokenAsync(Client client)
    {
        (Guid refreshTokenId, string refreshToken) = await CreateRefreshTokenAsync(client);
        string accessToken = await CreateAccessTokenAsync(client, refreshTokenId);

        return new AuthTokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ClientId = client.ClientId
        };
    }

    public async Task<AuthTokenDto> RefreshAuthTokenAsync(Client client)
    {
        await RemoveAuthTokenAsync(client);
        (Guid refreshTokenId, string refreshToken) = await CreateRefreshTokenAsync(client);
        string newAccessToken = await CreateAccessTokenAsync(client, refreshTokenId);

        return new AuthTokenDto
        {
            AccessToken = newAccessToken,
            RefreshToken = refreshToken,
            ClientId = client.ClientId
        };
    }

    public async Task RemoveAuthTokenAsync(Client client)
    {
        Task delRefreshToken = _cache.StringGetDeleteAsync(RefreshTokenCacheKey(client));
        Task delAccessToken = _cache.StringGetDeleteAsync(AccessTokenCacheKey(client));
        await Task.WhenAll(delRefreshToken, delAccessToken);
    }

    public async Task<bool> CheckTokenCacheValidation(string token, string tokenType, Client client)
    {
        string key = tokenType switch
        {
            AuthenticateConstant.Cache.AccessTokenTag => AccessTokenCacheKey(client),
            AuthenticateConstant.Cache.RefreshTokenTag => RefreshTokenCacheKey(client),
            _ => throw new ArgumentException("Token type is invalid")
        };

        string? localToken = await _cache.StringGetAsync(token);
        return localToken is not null && localToken == token;
    }
}
