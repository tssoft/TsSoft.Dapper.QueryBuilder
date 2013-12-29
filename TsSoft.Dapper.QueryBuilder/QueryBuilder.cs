using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TsSoft.Dapper.QueryBuilder.Helpers;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder
{
    public class QueryBuilder<TCriteria> where TCriteria : Criteria
    {
        private static readonly WhereClauseManager WhereClauseManager = new WhereClauseManager(new WhereAttributeManager());
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
                throw new NullReferenceException(string.Format("Not exists table from criteria {0}",
                                                               criteria.GetType()));
            }
            Criteria = criteria;
            
            Builder = new SqlBuilder();
            
            SimplyTemplate = Builder.AddTemplate(SimplySql);
            PaginateTemplate = Builder.AddTemplate(PaginateSql, new {Criteria.Skip, Criteria.Take});
            CountTemplate = Builder.AddTemplate(CountSql);
            ExistsTemplate = Builder.AddTemplate(ExistsSql);

            SplitOn = new List<string>();
        }

        protected string SplitOnString
        {
            get { return SplitOn.All(x => x.Equals("Id")) ? "Id" : string.Join(",", SplitOn); }
        }

        public TCriteria Criteria { get; private set; }

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
            IEnumerable<WhereClauseManager.WhereClauseModel> whereClauses = WhereClauseManager.Get(Criteria, TableName);
            var dbArgs = new DynamicParameters();


            foreach (WhereClauseManager.WhereClauseModel whereClause in whereClauses)
            {
                if (!whereClause.WithoutValue)
                {
                    dbArgs.Add(whereClause.ParameterName, whereClause.ParameterValue);
                }
                Builder.Where(whereClause.Sql);
            }

            Builder.AddParameters(dbArgs);
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