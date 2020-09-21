using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Ejector.Services
{
    public class RedisService
    {

        private readonly ConnectionMultiplexer redis;
        
        public RedisService(IConfiguration _config)
        {
            redis = ConnectionMultiplexer.Connect(_config["RedisServers"]);
        }

        public IDatabase GetRedisDb()
        {
            return redis.GetDatabase();
        }
    }
}