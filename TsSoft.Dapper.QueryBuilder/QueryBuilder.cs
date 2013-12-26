using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Dapper;
using TsSoft.Dapper.QueryBuilder.Helpers;
using TsSoft.Dapper.QueryBuilder.Metadata;

namespace TsSoft.Dapper.QueryBuilder
{
    public abstract class QueryBuilder<TCriteria> where TCriteria : Criteria
    {
        private readonly TableAttribute table;

        protected SqlBuilder Builder;
        protected SqlBuilder.Template CountTemplate;
        protected SqlBuilder.Template ExistsTemplate;
        protected SqlBuilder.Template PaginateTemplate;
        protected SqlBuilder.Template SimplyTemplate;
        protected ICollection<string> SplitOn;

        protected QueryBuilder(TCriteria criteria)
        {
            table =
                (TableAttribute) criteria.GetType().GetCustomAttributes(typeof (TableAttribute), false).FirstOrDefault();
            if (table == null)
            {
                throw new NullReferenceException(string.Format("Не определена таблица у критерии {0}",
                                                               criteria.GetType()));
            }
            Criteria = criteria;
            //Создаем билдер
            Builder = new SqlBuilder();
            //Добавляем шаблоны. Все операции с билдером будут производиться со всеми шаблонами.
            SimplyTemplate = Builder.AddTemplate(SimplySql);
            PaginateTemplate = Builder.AddTemplate(PaginateSql, new {Criteria.Skip, Criteria.Take});
            CountTemplate = Builder.AddTemplate(CountSql);
            ExistsTemplate = Builder.AddTemplate(ExistsSql);
            //Создаем контейнер для разделителей, при join будем добавлять сюда поля
            SplitOn = new List<string>();
        }

        protected string SplitOnString
        {
            get { return SplitOn.All(x => x.Equals("Id")) ? "Id" : string.Join(",", SplitOn); }
        }

        public TCriteria Criteria { get; private set; }

        //Создаем шаблон для sql
        //делаем виртуальным, чтобы переопределить в наследниках, если понадобится
        protected string SimplySql
        {
            get
            {
                return
                    string.Format(
                        @"Select /**select**/ from {0} /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**orderby**/",
                        TableName);
            }
        }

        protected string PaginateSql
        {
            get
            {
                return
                    string.Format(
                        @"Select /**select**/ from {0} /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**orderby**/ OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY",
                        TableName);
            }
        }

        protected string CountSql
        {
            get { return string.Format("Select count(1) from {0} /**innerjoin**/ /**leftjoin**/ /**where**/", TableName); }
        }

        protected string ExistsSql
        {
            get { return string.Format("Select 1 from {0} /**innerjoin**/ /**leftjoin**/ /**where**/", TableName); }
        }

        protected virtual string TableName
        {
            get { return table.Name; }
        }

        //Метод селекта, по умолчанию будет выбираться все у таблицы, привязанной к критерии.
        //Чтобы изменить поведение просто переопределить в наследнике
        protected virtual void Select()
        {
            Builder.Select(string.Format(@"{0}.*", TableName));
        }

        protected virtual void InnerJoin()
        {
        }

        protected virtual void LeftJoin()
        {
        }

        protected virtual void Where()
        {
            IEnumerable<PropertyInfo> props =
                typeof (TCriteria).GetProperties()
                                  .Where(x => x.GetCustomAttributes(typeof(WhereAttribute), false).Count() != 0);
            var dbArgs = new DynamicParameters();
            foreach (PropertyInfo propertyInfo in props)
            {
                object value;
                var whereAttribute = (WhereAttribute) propertyInfo.GetCustomAttributes(typeof(WhereAttribute), false).Single();
                if ((value = propertyInfo.GetValue(Criteria, null)) == null)
                {
                    continue;
                }
                string tableName = !string.IsNullOrWhiteSpace(whereAttribute.TableName)
                                       ? whereAttribute.TableName
                                       : TableName;
                string paramName = string.Format("@{0}{1}", tableName, propertyInfo.Name);

                string str = string.Format("{0}.{1} {2} ", tableName,
                                           !string.IsNullOrWhiteSpace(whereAttribute.Field)
                                               ? whereAttribute.Field
                                               : propertyInfo.Name, GetExpression(whereAttribute.WhereType, paramName, ref value));
                if (!IsWithoutValue(whereAttribute.WhereType))
                {
                    dbArgs.Add(paramName, value);                    
                }
                Builder.Where(str);
            }
            Builder.AddParameters(dbArgs);
        }

        private static bool IsWithoutValue(WhereType whereType)
        {
            switch (whereType)
            {
                case WhereType.Eq:
                case WhereType.NotEq:
                case WhereType.Gt:
                case WhereType.Lt:
                case WhereType.GtEq:
                case WhereType.LtEq:
                case WhereType.Like:
                    return false;
                case WhereType.IsNull:
                case WhereType.IsNotNull:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException("whereType");
            }
        }

        private static string GetExpression(WhereType whereType, string paramName, ref object value)
        {
            switch (whereType)
            {
                case WhereType.Like:
                    value = string.Format("%{0}%", value);
                    return string.Format("{0} {1}", GetSelector(whereType), paramName);
                case WhereType.IsNull:
                case WhereType.IsNotNull:
                    return GetSelector(whereType);
                case WhereType.Eq:
                case WhereType.NotEq:
                case WhereType.Gt:
                case WhereType.Lt:
                case WhereType.GtEq:
                case WhereType.LtEq:
                    return string.Format("{0} {1}", GetSelector(whereType), paramName);
                default:
                    throw new ArgumentOutOfRangeException("whereType");
            }
        }

        private static string GetSelector(WhereType whereType)
        {
            return whereType.Getattribute<DescriptionAttribute>().Description;
        }

        protected virtual void GroupBy()
        {
        }

        protected virtual void OrderBy()
        {
            if (Criteria.HasOrder)
            {
                foreach (var order in Criteria.Order)
                {
                    Builder.OrderBy(string.Format("{0} {1}", order.Key, order.Value));
                }
            }
        }

        /// <summary>
        ///     В зависимости от типа запроса возвращает шаблон
        /// </summary>
        /// <returns></returns>
        protected SqlBuilder.Template GetTemplate()
        {
            switch (Criteria.QueryType)
            {
                case QueryType.Simple:
                    return SimplyTemplate;
                case QueryType.Paginate:
                    return PaginateTemplate;
                case QueryType.OnlyCount:
                    return CountTemplate;
                case QueryType.Exists:
                    return ExistsTemplate;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Возвращает объект запроса
        /// </summary>
        /// <returns></returns>
        public virtual Query Build()
        {
            Select();
            InnerJoin();
            LeftJoin();
            Where();
            GroupBy();
            OrderBy();
            SqlBuilder.Template template = GetTemplate();
            var query = new Query {Sql = template.RawSql, Parameters = template.Parameters, SplitOn = SplitOnString};
            return query;
        }
    }
}