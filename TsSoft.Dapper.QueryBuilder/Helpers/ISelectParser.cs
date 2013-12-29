using System.Collections.Generic;

namespace TsSoft.Dapper.QueryBuilder.Helpers
{
    public interface ISelectParser
    {
        IDictionary<string, ICollection<string>> Parse(string str);
    }
}