using TsSoft.Dapper.QueryBuilder.Metadata;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    public interface IJoinClauseCreator
    {
        JoinClause Create(JoinAttribute joinAttribute);
        JoinClause CreateNotJoin(JoinAttribute joinAttribute);
    }
}