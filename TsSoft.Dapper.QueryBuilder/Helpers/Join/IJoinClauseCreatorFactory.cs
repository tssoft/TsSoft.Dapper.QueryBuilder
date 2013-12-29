using System;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    public interface IJoinClauseCreatorFactory
    {
        IJoinClauseCreator Get(Type joinAttributeType);
    }
}