using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Ejector.Services
{
    public class SqlService : ISqlService
    {
        private enum SqlType
        {
            SQLite,
            MySQL
        }

        private readonly SqlType _sqlType;
        private readonly string _sqlConnectionString;
        
        public SqlService(IConfiguration _configuration)
        {
            _sqlType = _configuration["SqlType"].ToLowerInvariant() switch
            {
                "sqlite" => SqlType.SQLite,
                "mysql" => SqlType.MySQL,
                _ => throw new Exception($"SQL Type {_configuration["SqlType"]} is not supported.")
            };
            
            _sqlConnectionString = _configuration["ConnectionString"];
        }

        public IDbConnection GetSqlConnection()
        {
            return _sqlType switch
            {
                SqlType.SQLite => new SqliteConnection(_sqlConnectionString),
                SqlType.MySQL => new MySqlConnection(_sqlConnectionString),
            };
        }
    }
}