using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Helpers
{
    public interface IWhereAttributeManager
    {
        bool IsWithoutValue(WhereType whereType);
        string GetExpression(WhereType whereType, string paramName);
        string GetSelector(WhereType whereType);
    }
}