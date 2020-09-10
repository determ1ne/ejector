using System.Data;

namespace Ejector.Services
{
    public interface ISqlService
    {
        public IDbConnection GetSqlConnection();
    }
}