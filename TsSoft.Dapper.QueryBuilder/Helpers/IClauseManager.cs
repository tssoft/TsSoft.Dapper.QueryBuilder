using System.Collections.Generic;
using TsSoft.Dapper.QueryBuilder.Models;

namespace TsSoft.Dapper.QueryBuilder.Helpers
{
    public interface IClauseManager<out T>
    {
        IEnumerable<T> Get(Criteria criteria, string tableName);
    }
}