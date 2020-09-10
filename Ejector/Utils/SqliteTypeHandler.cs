using System;
using System.Data;
using Dapper;
using Ejector.Services;

namespace Ejector.Utils.Calender
{
    abstract public class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        public override void SetValue(IDbDataParameter parameter, T value) => parameter.Value = value;
    }

    public class DateHandler : SqliteTypeHandler<Date>
    {
        public override Date Parse(object value)
            => Date.Parse(Convert.ToInt32(value));

        public override void SetValue(IDbDataParameter parameter, Date value)
            => parameter.Value = value.ToInt32();
    }
    
}