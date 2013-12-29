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
    public class WhereClauseManager
    {
        private readonly IWhereAttributeManager whereAttributeManager;

        public WhereClauseManager(IWhereAttributeManager whereAttributeManager)
        {
            this.whereAttributeManager = whereAttributeManager;
        }

        public IEnumerable<WhereClauseModel> Get(Criteria criteria, string criteriaTableName)
        {
            Type type = criteria.GetType();
            IEnumerable<PropertyInfo> propertyInfos = type.GetProperties()
                                                          .Where(pi => pi.HasAttribute<WhereAttribute>());
            var whereClauses = new List<WhereClauseModel>();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                object value;
                var whereAttribute = propertyInfo.GetCustomAttribute<WhereAttribute>();
                if ((value = propertyInfo.GetValue(criteria, null)) == null)
                {
                    continue;
                }
                string tableName = !string.IsNullOrWhiteSpace(whereAttribute.TableName)
                                       ? whereAttribute.TableName
                                       : criteriaTableName;
                string paramName = string.Format("@{0}{1}", tableName, propertyInfo.Name);
                string str = GeWheretSting(whereAttribute, propertyInfo, tableName, paramName, ref value);
                whereClauses.Add(new WhereClauseModel
                    {
                        ParameterName = paramName,
                        ParameterValue = value,
                        Sql = str,
                        WithoutValue = whereAttributeManager.IsWithoutValue(whereAttribute.WhereType)
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
            string fieldName = !string.IsNullOrWhiteSpace(whereAttribute.Field)
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
                                    , whereAttributeManager.GetExpression(whereAttribute.WhereType, paramName));
            }
            else
            {
                string whereString = GetWhereStringByExpression(whereAttribute, tableName, fieldName,
                                                                whereAttributeManager.GetSelector(
                                                                    whereAttribute.WhereType),
                                                                paramName);
                str = string.Format("({0})", whereString);
            }
            return str;
        }


        public class WhereClauseModel
        {
            public string Sql { get; set; }

            public string ParameterName { get; set; }

            public object ParameterValue { get; set; }

            public bool WithoutValue { get; set; }
        }
    }
}