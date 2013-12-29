using System;
using TsSoft.Dapper.QueryBuilder.Metadata;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    public class JoinClauseCreatorFactory : IJoinClauseCreatorFactory
    {
        public IJoinClauseCreator Get(Type joinAttributeType)
        {
            if (!typeof (JoinAttribute).IsAssignableFrom(joinAttributeType))
            {
                throw new ArgumentException("joinAttributeType should inherit JoinAttribute");
            }
            if (joinAttributeType == typeof(SimpleJoinAttribute))
            {
                return new SimpleJoinClauseCreator();
            }
            throw new ArgumentOutOfRangeException("joinAttributeType");
        }
    }
}