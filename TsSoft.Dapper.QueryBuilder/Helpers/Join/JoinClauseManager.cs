using System;
using System.Collections.Generic;
using System.Linq;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    public class JoinClauseManager : IClauseManager<JoinClause>
    {
        private readonly IJoinClauseCreatorFactory _joinClauseCreatorFactory;

        public JoinClauseManager(IJoinClauseCreatorFactory joinClauseCreatorFactory)
        {
            _joinClauseCreatorFactory = joinClauseCreatorFactory;
        }

        public IEnumerable<JoinClause> Get(Criteria criteria, string criteriaTableName)
        {
            var type = criteria.GetType();
            var props = type.GetProperties().Where(pi => pi.HasAttribute<JoinAttribute>());
            var joinClauses = new List<JoinClause>();
            foreach (var propertyInfo in props)
            {
                if (propertyInfo.PropertyType != typeof (bool))
                {
                    throw new NotImplementedException("Join implemented to only bool properties");
                }
                var joinAttribute = propertyInfo.GetCustomAttribute<JoinAttribute>();
                var joiner = _joinClauseCreatorFactory.Get(joinAttribute.GetType());
                if (!(bool) propertyInfo.GetValue(criteria, null))
                {
                    joinClauses.Add(joiner.CreateNotJoin(joinAttribute));
                    continue;
                }

                joinAttribute.CurrentTable = string.IsNullOrWhiteSpace(joinAttribute.CurrentTable)
                    ? criteriaTableName
                    : joinAttribute.CurrentTable;
                joinAttribute.CurrentTableField = string.IsNullOrWhiteSpace(joinAttribute.CurrentTableField)
                    ? propertyInfo.Name
                    : joinAttribute.CurrentTableField;
                joinClauses.Add(joiner.Create(joinAttribute));
            }
            return joinClauses;
        }
    }
}