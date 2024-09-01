using IdentityModel;
using LijnBlog.Api.Contracts.Responses;
using LijnBlog.Application;
using LijnBlog.Application.Constants;
using LijnBlog.Application.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

/*
 * service add database
 */
builder.Services.AddDatabase($"User ID={config["Database:User ID"]};" +
                             $"Password={config["Database:Password"]};" +
                             $"Host={config["Database:Host"]};" +
                             $"Port={config["Database:Port"]};" +
                             $"Database={config["Database:Database"]};" +
                             $"Pooling={config["Database:Pooling"]};" +
                             $"Min Pool Size={config["Database:Min Pool Size"]};" +
                             $"Max Pool Size={config["Database:Max Pool Size"]};" +
                             $"Connection Lifetime={config["Database:Connection Lifetime"]};");
/*
 * service add cache
 */
builder.Services.AddCache(config["Cache:Host"]!, int.Parse(config["Cache:Port"]!), config["Cache:User"]!,
    config["Cache:Password"]!, int.Parse(config["Cache:Database"]!));

/*
 * service add jwt
 */
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{
    var rsaPublicKey = new RsaSecurityKey(JsonSerializer.Deserialize<RSAParameters>(config["Jwt:PublicKey"]!));
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidTypes = [JwtConstants.HeaderType],
        ValidAlgorithms = [SecurityAlgorithms.RsaSha256Signature],

        IssuerSigningKey = rsaPublicKey,
        ValidateIssuerSigningKey = true,

        ValidIssuer = config["Jwt:Issuer"]!,
        ValidateIssuer = true,

        ValidAudience = config["Jwt:Audience"],
        ValidateAudience = true,

        ValidateLifetime = true,
        RequireSignedTokens = true,
        RequireExpirationTime = true,

        NameClaimType = JwtClaimTypes.Name,
        RoleClaimType = JwtClaimTypes.Role,

        ClockSkew = TimeSpan.FromSeconds(int.Parse(config["Jwt:Audience"]!))
    };

    o.SaveToken = false;
    o.TokenHandlers.Clear();
    o.TokenHandlers.Add(new JwtSecurityTokenHandler());

    o.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            if (context.AuthenticateFailure is SecurityTokenExpiredException)
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy(AuthorizationConstant.Policy.AdminPolicy, p =>
    {
        p.RequireClaim(JwtClaimTypes.Role, ((int)RoleEnum.Admin).ToString());
        p.RequireClaim(AuthenticateConstant.JwtClaimTypes.BannedClaimType, false.ToString());
        p.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                claim.Type == AuthenticateConstant.JwtClaimTypes.RefreshTokenIdClaimType));
    });

    o.AddPolicy(AuthorizationConstant.Policy.UserPolicy, p =>
    {
        p.RequireClaim(JwtClaimTypes.Role, ((int)RoleEnum.User).ToString());
        p.RequireClaim(AuthenticateConstant.JwtClaimTypes.BannedClaimType, false.ToString());
        p.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                claim.Type == AuthenticateConstant.JwtClaimTypes.RefreshTokenIdClaimType));
    });

    o.AddPolicy(AuthorizationConstant.Policy.RefreshPolicy, p =>
    {
        p.RequireAssertion(context => 
            !context.User.HasClaim(claim => claim.Type == AuthenticateConstant.JwtClaimTypes.RefreshTokenIdClaimType)            
        );
    });

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();