using System.Collections.Generic;
using TsSoft.Dapper.QueryBuilder.Helpers.Select;

namespace TsSoft.Dapper.QueryBuilder.Helpers
{
    public interface ISelectParser
    {
        IDictionary<string, ICollection<SelectClause>> Parse(string str, bool strict = true);
    }
}