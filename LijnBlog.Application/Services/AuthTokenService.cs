using IdentityModel;
using LijnBlog.Application.Constants;
using LijnBlog.Application.Database;
using LijnBlog.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace LijnBlog.Application.Services;

public class AuthTokenService : IAuthTokenService
{
    private readonly IConfiguration _config;

    private readonly RsaSecurityKey rsaPrivateKey;

    private readonly IGarnetStringClient _cache;

    public AuthTokenService(IConfiguration configuration, IGarnetStringClient garnetClient)
    {
        _config = configuration;
        rsaPrivateKey = new(JsonConvert.DeserializeObject<RSAParameters>(_config["Jwt:PrivateKey"]!));
        _cache = garnetClient;
    }

    private static string AccessTokenTag(Client client) => AuthTokenConstant.Cache.AccessTokenTag + client.ClientId;

    private static string RefreshTokenTag(Client client) => AuthTokenConstant.Cache.RefreshTokenTag + client.ClientId;

    public async Task<string> CreateAccessTokenAsync(Client client, Guid refreshTokenId)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new (JwtClaimTypes.Id,client.ClientId.ToString()),
                new (JwtClaimTypes.Name, client.Name),
                new (AuthTokenConstant.JwtClaimTypes.RefreshTokenIdClaimType, refreshTokenId.ToString())
                ]
            ),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:AccessTokenExpireTime"]!)),
            SigningCredentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha256Signature)
        };
        JwtSecurityTokenHandler handler = new();
        var token = handler.CreateJwtSecurityToken(tokenDescriptor);
        string strToken = handler.WriteToken(token);

        await _cache.Set(AccessTokenTag(client), strToken);

        return handler.WriteToken(token);
    }

    public async Task<(Guid refreshTokenId, string refreshToken)> CreateRefreshTokenAsync(Client client)
    {
        var tokenId = Guid.NewGuid();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new (JwtClaimTypes.Id,client.ClientId.ToString()),
                new (JwtClaimTypes.Name, client.Name),
                ]
            ),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:RefreshTokenExpireTime"]!)),
            SigningCredentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha256Signature)
        };
        JwtSecurityTokenHandler handler = new();
        var token = handler.CreateJwtSecurityToken(tokenDescriptor);
        string strToken = handler.WriteToken(token);

        await _cache.Set(RefreshTokenTag(client), strToken);

        return (tokenId, strToken);
    }

    public async Task<AuthTokenDto> CreateAuthTokenAsync(Client client)
    {
        (Guid refreshTokenId, string refreshToken) = await CreateRefreshTokenAsync(client);
        string accessToken = await CreateAccessTokenAsync(client, refreshTokenId);

        AuthTokenDto tokenDto = new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
        return tokenDto;
    }

    public Task<AuthTokenDto> RefreshAuthTokenAsync(Client client, )
    {
        RemoveAuthTokenAsync(client);
        
    }

    public void RemoveAuthTokenAsync(Client client)
    {
        Task.WaitAll(_cache.Delete(RefreshTokenTag(client)), _cache.Delete(AccessTokenTag(client)));
    }
}
