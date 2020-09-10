using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Ejector.Services
{
    public class SqlService : ISqlService
    {
        private readonly string _sqlConnectionString;
        
        public SqlService(IConfiguration _configuration)
        {
            _sqlConnectionString = _configuration["ConnectionString"];
        }

        public IDbConnection GetSqlConnection()
        {
            return new SqliteConnection(_sqlConnectionString);
        }
    }
}