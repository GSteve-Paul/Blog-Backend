using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using IdentityModel;
using LijnBlog.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("DbConnection.json", optional: true, reloadOnChange: true);

var config = builder.Configuration;

string dbSchema = builder.Environment.EnvironmentName switch
{
    "Development" => "Database:dev",
    "Production" => "Database:pro",
    _ => throw new InvalidEnumArgumentException($"Project environment {builder.Environment.EnvironmentName} is invalid for deciding database schema")
};

/*
 * service add database
 */
builder.Services.AddDatabase($"User ID={config[dbSchema + "User ID"]};" +
                             $"Password={config[dbSchema + "Password"]};" +
                             $"Host={config[dbSchema + "Host"]};" +
                             $"Port={config[dbSchema + "Port"]};" +
                             $"Database={config[dbSchema + "Database"]};" +
                             $"Pooling={config[dbSchema + "Pooling"]};" +
                             $"Min Pool Size={config[dbSchema + "Min Pool Size"]};" +
                             $"Max Pool Size={config[dbSchema + "Max Pool Size"]};" +
                             $"Connection Lifetime={config[dbSchema + "Connection Lifetime"]};");

/*
 * service add jwt
 */
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{
    var rsaValidateSigningKey = new RsaSecurityKey(JsonConvert.DeserializeObject<RSAParameters>(
        config["Jwt:PublicKey"]!));
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidTypes = new[] {JwtConstants.HeaderType},
        
        IssuerSigningKey = rsaValidateSigningKey,
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

    //o.EventsType = typeof();
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

app.Run();