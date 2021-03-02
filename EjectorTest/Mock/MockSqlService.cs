using System.Collections.Generic;
using System.Data;
using System.IO;
using Dapper;
using Ejector.Services;
using Ejector.Utils.Calender;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace EjectorTest.Mock
{
    public class MockSqlService : ISqlService
    {
        private readonly string _dbPath;
        public MockSqlService(string dbPath)
        {
            _dbPath = dbPath;
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Execute(@"CREATE TABLE TermConfig (
  ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  ""Year"" integer NOT NULL,
  ""Term"" integer NOT NULL,
  ""Begin"" integer NOT NULL,
  ""End"" integer NOT NULL,
  ""FirstWeekNo"" integer NOT NULL
);");
            connection.Execute(@"CREATE TABLE Tweak (
  ""Id"" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  ""TweakType"" integer NOT NULL,
  ""Description"" TEXT NOT NULL,
  ""From"" integer NOT NULL,
  ""To"" integer NOT NULL
);");
            connection.Execute(@"CREATE TABLE ClassParseSettings (
  ""Id"" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  ""Year"" integer NOT NULL,
  ""Term"" integer NOT NULL
);");
            connection.Execute(@"CREATE TABLE ExamParseSettings (
  ""Id"" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  ""Year"" integer NOT NULL,
  ""Term"" integer NOT NULL
);");
            SqlMapper.AddTypeHandler(new DateHandler());

            var termConfigs = new List<TermConfig>(new []
            {
                new TermConfig { Year="2020-2021", Term=ClassTerm.Autumn, Begin=Date.Parse(20200914), End=Date.Parse(20201115), FirstWeekNo = 1}, 
                new TermConfig { Year="2020-2021", Term=ClassTerm.Winter, Begin=Date.Parse(20201123), End=Date.Parse(20210119), FirstWeekNo = 1}, 
                new TermConfig { Year="2020-2021", Term=ClassTerm.Spring, Begin=Date.Parse(20210301), End=Date.Parse(20210426), FirstWeekNo = 1}, 
                new TermConfig { Year="2020-2021", Term=ClassTerm.Summer, Begin=Date.Parse(20210503), End=Date.Parse(20210629), FirstWeekNo = 1}, 
            });
            var tweaks = new List<Tweak>(new[]
            {
                new Tweak{TweakType=TweakType.Clear, Description="国庆节、中秋节放假", From=Date.Parse(20201001), To=Date.Parse(20201008)}, 
                new Tweak{TweakType=TweakType.Exchange, Description="国庆中秋调休（9月27日与10月7日对调）", From=Date.Parse(20200927), To=Date.Parse(20201007)}, 
                new Tweak{TweakType=TweakType.Exchange, Description="国庆中秋调休（10月10日与10月8日对调）", From=Date.Parse(20201010), To=Date.Parse(20201008)}, 
                new Tweak{TweakType=TweakType.Copy, Description="国庆中秋调休（补10月3日课）", To=Date.Parse(20201006), From=Date.Parse(20201003)}, 
                new Tweak{TweakType=TweakType.Clear, Description="秋季校运动会放假", From=Date.Parse(20201023), To=Date.Parse(20201025)}, 
                new Tweak{TweakType=TweakType.Copy, Description="秋季校运动会调休（补10月23日课）", From=Date.Parse(20201023), To=Date.Parse(20201113)}, 
                new Tweak{TweakType=TweakType.Copy, Description="国庆中秋调休（补10月1日课）", From=Date.Parse(20201001), To=Date.Parse(20201109)}, 
                new Tweak{TweakType=TweakType.Copy, Description="国庆中秋调休（补10月2日课）", From=Date.Parse(20201002), To=Date.Parse(20201110)}, 
                new Tweak{TweakType=TweakType.Copy, Description="国庆中秋调休（补10月5日课）", From=Date.Parse(20201005), To=Date.Parse(20201111)}, 
                new Tweak{TweakType=TweakType.Copy, Description="国庆中秋调休（补10月6日课）", From=Date.Parse(20201006), To=Date.Parse(20201112)}, 
            });

            foreach (var termConfig in termConfigs)
            {
                connection.Execute(
                    "INSERT INTO TermConfig (\"Year\", \"Term\", \"Begin\", \"End\", \"FirstWeekNo\") VALUES (@Year, @Term, @Begin, @End, @FirstWeekNo)", termConfig);
            }

            foreach (var tweak in tweaks)
            {
                connection.Execute(
                    "INSERT INTO Tweak (\"TweakType\", \"Description\", \"From\", \"To\") VALUES (@TweakType, @Description, @From, @To)", tweak);
            }
        }
        
        public IDbConnection GetSqlConnection()
        {
            return new SqliteConnection($"Data Source={_dbPath}");
        }
    }
}