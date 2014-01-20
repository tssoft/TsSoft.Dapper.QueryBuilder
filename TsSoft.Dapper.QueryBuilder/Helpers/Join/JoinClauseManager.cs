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
                var isNullable = propertyInfo.PropertyType == typeof(string) ||
                                 propertyInfo.PropertyType.IsGenericType &&
                                 propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>);
                var isBool = propertyInfo.PropertyType == typeof (bool);
                if (!isBool && !isNullable)
                {
                    throw new NotImplementedException("Join implemented to only bool or nullable properties");
                }
                var joinAttributes = propertyInfo.GetCustomAttributes<JoinAttribute>();
                var value = propertyInfo.GetValue(criteria, null);
                foreach (var joinAttribute in joinAttributes)
                {
                    var joiner = _joinClauseCreatorFactory.Get(joinAttribute.GetType());
                    if ((isBool && !(bool) value) || (isNullable && value == null))
                    {
                        var clause = joiner.CreateNotJoin(joinAttribute);
                        if (joinClauses.All(x => x.Splitter != clause.Splitter))
                        {
                            joinClauses.Add(clause);
                        }
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(joinAttribute.Including))
                    {
                        var includedProp = props.SingleOrDefault(x => x.Name == joinAttribute.Including);
                        if (includedProp == null)
                        {
                            throw new Exception(string.Format("Property {0} not found", joinAttribute.Including));
                        }
                        var includedPropIsNullable = includedProp.PropertyType == typeof(string) ||
                                                     includedProp.PropertyType.IsGenericType &&
                                                     propertyInfo.PropertyType.GetGenericTypeDefinition() ==
                                                     typeof (Nullable<>);
                        var includedPropIsBool = includedProp.PropertyType == typeof (bool);

                        if (!includedPropIsNullable && !includedPropIsBool)
                        {
                            throw new NotImplementedException(
                                "Including implemented to only bool or nullable properties");
                        }
                        var includedPropValue = includedProp.GetValue(criteria, null);
                        if ((includedPropIsBool && (bool) includedPropValue) ||
                            (includedPropIsNullable && includedPropValue != null))
                        {
                            continue;
                        }
                    }

                    joinAttribute.CurrentTable = string.IsNullOrWhiteSpace(joinAttribute.CurrentTable)
                                                     ? criteriaTableName
                                                     : joinAttribute.CurrentTable;
                    joinAttribute.CurrentTableField = string.IsNullOrWhiteSpace(joinAttribute.CurrentTableField)
                                                          ? propertyInfo.Name
                                                          : joinAttribute.CurrentTableField;
                    joinClauses.Add(joiner.Create(joinAttribute));
                }
            }
            return joinClauses;
        }
    }
}