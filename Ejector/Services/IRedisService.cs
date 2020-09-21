using StackExchange.Redis;

namespace Ejector.Services
{
    public interface IRedisService
    {
        IDatabase GetRedisDb();
    }
}