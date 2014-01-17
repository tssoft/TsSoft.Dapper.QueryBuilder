using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TsSoft.Dapper.QueryBuilder.Formatters;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Where
{
    public class WhereClauseManager : IClauseManager<WhereClause>
    {
        private readonly IWhereAttributeManager _whereAttributeManager;

        public WhereClauseManager(IWhereAttributeManager whereAttributeManager)
        {
            _whereAttributeManager = whereAttributeManager;
        }

        public IEnumerable<WhereClause> Get(Criteria criteria, string criteriaTableName)
        {
            var type = criteria.GetType();
            var propertyInfos = type.GetProperties()
                .Where(pi => pi.HasAttribute<WhereAttribute>());
            var whereClauses = new List<WhereClause>();
            foreach (var propertyInfo in propertyInfos)
            {
                object value;
                var whereAttribute = propertyInfo.GetCustomAttribute<WhereAttribute>();
                if ((value = propertyInfo.GetValue(criteria, null)) == null)
                {
                    continue;
                }
                if (propertyInfo.PropertyType == typeof (bool) && !(bool)value)
                {
                    continue;
                }
                var tableName = !string.IsNullOrWhiteSpace(whereAttribute.TableName)
                    ? whereAttribute.TableName
                    : criteriaTableName;
                var paramName = string.Format("@{0}{1}", tableName, propertyInfo.Name);
                var str = GeWheretSting(whereAttribute, propertyInfo, tableName, paramName, ref value);
                whereClauses.Add(new WhereClause
                {
                    ParameterName = paramName,
                    ParameterValue = value,
                    Sql = str,
                    WithoutValue = _whereAttributeManager.IsWithoutValue(whereAttribute.WhereType)
                });
            }
            return whereClauses;
        }


        private static string GetWhereStringByExpression(WhereAttribute whereAttribute, string tableName,
            string fieldName, string compareOperation, string paramName)
        {
            return whereAttribute.Expression
                .Replace(GetNameForReplace("TableName"), tableName)
                .Replace(GetNameForReplace("FieldName"), fieldName)
                .Replace(GetNameForReplace("CompareOperation"), compareOperation)
                .Replace(GetNameForReplace("Parameter"), paramName);
        }

        private static string GetNameForReplace(string replaced)
        {
            return string.Format("/**{0}**/", replaced);
        }

        private static void SetValueByWhereType(WhereType whereType, ref object value, IFormatter formatter = null)
        {
            if (formatter == null)
            {
                formatter = GetFormatter(whereType);
            }
            formatter.Format(ref value);
        }

        private static IFormatter GetFormatter(WhereType whereType)
        {
            if (whereType == WhereType.Like)
            {
                return new SimpleLikeFormatter();
            }
            return new DummyFormatter();
        }

        private string GeWheretSting(WhereAttribute whereAttribute, PropertyInfo propertyInfo, string tableName,
            string paramName, ref object value)
        {
            var fieldName = !string.IsNullOrWhiteSpace(whereAttribute.Field)
                ? whereAttribute.Field
                : propertyInfo.Name;
            string str;
            var formatAttr = propertyInfo.GetCustomAttribute<FormatAttribute>();
            IFormatter formatter = null;
            if (formatAttr != null)
            {
                formatter = (IFormatter) Activator.CreateInstance(formatAttr.FormatterType);
            }
            SetValueByWhereType(whereAttribute.WhereType, ref value, formatter);
            if (string.IsNullOrWhiteSpace(whereAttribute.Expression))
            {
                str = string.Format("{0}.{1} {2} ", tableName, fieldName
                    , _whereAttributeManager.GetExpression(whereAttribute.WhereType, paramName));
            }
            else
            {
                var whereString = GetWhereStringByExpression(whereAttribute, tableName, fieldName,
                    _whereAttributeManager.GetSelector(
                        whereAttribute.WhereType),
                    paramName);
                str = string.Format("({0})", whereString);
            }
            return str;
        }
    }
}