using LijnBlog.Application.Cache;
using LijnBlog.Application.Database;
using Microsoft.Extensions.DependencyInjection;

namespace LijnBlog.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));
        return services;
    }

    public static IServiceCollection AddCache(this IServiceCollection services, string host, int port,
        string user, string password, int dbId)
    {
        services.AddSingleton<ICacheConnectionFactory>(_ =>
        new CacheConnectionFactory(host, port, user, password, dbId));
        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}