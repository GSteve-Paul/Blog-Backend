using IdentityModel;
using LijnBlog.Api.Contracts.Responses;
using LijnBlog.Application.Models;
using LijnBlog.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LijnBlog.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class TokenValidationFilterAttribute : Attribute, IAsyncAuthorizationFilter, IOrderedFilter
{ 
    private readonly IAuthenticateService _authenticateService;

    private readonly string _tokenType;

    public TokenValidationFilterAttribute(IAuthenticateService authenticateService, string tokenType)
    {
        _authenticateService = authenticateService;
        _tokenType = tokenType;
    }

    public int Order => int.MinValue;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        HttpRequest req = context.HttpContext.Request;
        string token = req.Headers["Authorization"].ToString().Replace("Bearer ", "");
        Client client = new()
        {
            ClientId = new Guid(context.HttpContext.User.FindFirst(JwtClaimTypes.Id)!.Value)
        };
        bool res = await _authenticateService.CheckTokenCacheValidation(token, _tokenType, client);
        if(!res)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
