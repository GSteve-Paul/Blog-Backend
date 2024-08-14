using StackExchange.Redis;

namespace LijnBlog.Application.Cache;

public class CacheConnectionFactory : ICacheConnectionFactory
{
    private readonly ConnectionMultiplexer _redis;

    private readonly ConfigurationOptions _options;

    private readonly int _dbId;

    public CacheConnectionFactory(string host, int port, string user, string password, int dbId)
    {
        string serviceName = System.Reflection.Assembly.GetCallingAssembly().GetName().Name!;
        _options = new()
        {
            EndPoints = { { host, port } },
            ServiceName = serviceName,
            User = user,
            Password = password
        };
        _dbId = dbId;
        _redis = ConnectionMultiplexer.Connect(_options);
    }

    public IDatabase CreateConnectionAsync()
    {
        return _redis.GetDatabase(_dbId);
    }
}
