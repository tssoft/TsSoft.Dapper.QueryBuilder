using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using TsSoft.Dapper.QueryBuilder.Helpers;
using TsSoft.Dapper.QueryBuilder.Helpers.Join;
using TsSoft.Dapper.QueryBuilder.Helpers.Select;
using TsSoft.Dapper.QueryBuilder.Helpers.Where;
using TsSoft.Dapper.QueryBuilder.Metadata;
using TsSoft.Dapper.QueryBuilder.Models;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder
{
    public class QueryBuilder<TCriteria> where TCriteria : Criteria
    {
        private static readonly TableAttribute _table;
        protected SqlBuilder Builder;
        protected SqlBuilder.Template CountTemplate;
        protected SqlBuilder.Template ExistsTemplate;
        protected SqlBuilder.Template PaginateTemplate;
        protected SqlBuilder.Template SimplyTemplate;
        protected ICollection<string> SplitOn;

        static QueryBuilder()
        {
            WhereClauseManager = new WhereClauseManager(new WhereAttributeManager());
            JoinClauseManager = new JoinClauseManager(new JoinClauseCreatorFactory());
            SelectClauseManager = new SelectClauseManager();
            _table =
                (TableAttribute) typeof (TCriteria).GetCustomAttributes(typeof (TableAttribute), false).FirstOrDefault();
        }

        public QueryBuilder(TCriteria criteria)
        {
            if (_table == null)
            {
                throw new NullReferenceException(string.Format("Not exists table from criteria {0}",
                    criteria.GetType()));
            }
            Criteria = criteria;

            Builder = new SqlBuilder();

            SimplyTemplate = Builder.AddTemplate(SimpleSql);
            PaginateTemplate = Builder.AddTemplate(PaginateSql, new {Criteria.Skip, Criteria.Take});
            CountTemplate = Builder.AddTemplate(CountSql);
            ExistsTemplate = Builder.AddTemplate(ExistsSql);

            SplitOn = new List<string>();
        }

        public static IClauseManager<WhereClause> WhereClauseManager { protected get; set; }
        public static IClauseManager<JoinClause> JoinClauseManager { protected get; set; }
        public static IClauseManager<SelectClause> SelectClauseManager { protected get; set; }

        protected string SplitOnString
        {
            get { return SplitOn.All(x => x.Equals("Id")) ? "Id" : string.Join(",", SplitOn); }
        }

        public TCriteria Criteria { get; private set; }

        protected string SimpleSql
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
            get
            {
                return string.Format("Select count(1) from {0} /**innerjoin**/ /**leftjoin**/ /**where**/", TableName);
            }
        }

        protected string ExistsSql
        {
            get { return string.Format("Select 1 from {0} /**innerjoin**/ /**leftjoin**/ /**where**/", TableName); }
        }

        protected virtual string TableName
        {
            get { return _table.Name; }
        }

        protected virtual void Select()
        {
            var selects = SelectClauseManager.Get(Criteria, TableName);
            foreach (var selectClause in selects)
            {
                Builder.Select(
                    selectClause.IsExpression
                        ? selectClause.Select
                        : string.Format("{0}.{1}", selectClause.Table, selectClause.Select));
            }
        }

        protected virtual void Join()
        {
            IEnumerable<JoinClause> joinClauses =
                JoinClauseManager.Get(Criteria, TableName)
                    .OrderBy(x => x.Order == 0 ? int.MaxValue : x.Order);
            foreach (var joinClause in joinClauses)
            {
                Builder.Select(string.Format("0 as {0}", joinClause.Splitter));
                if (joinClause.HasJoin)
                {
                    foreach (var joinSql in joinClause.JoinSqls)
                    {
                        switch (joinClause.JoinType)
                        {
                            case JoinType.Inner:
                                Builder.InnerJoin(joinSql);
                                break;
                            case JoinType.Left:
                                Builder.LeftJoin(joinSql);
                                break;
                            case JoinType.Right:
                                Builder.RightJoin(joinSql);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    foreach (var selectSql in joinClause.SelectsSql)
                    {
                        Builder.Select(selectSql);
                    }
                }
                SplitOn.Add(joinClause.Splitter);
            }
        }

        protected virtual void Where()
        {
            var whereClauses = WhereClauseManager.Get(Criteria, TableName);
            var dbArgs = new DynamicParameters();


            foreach (var whereClause in whereClauses)
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
            Join();
            Where();
            GroupBy();
            OrderBy();
            var template = GetTemplate();
            var query = new Query {Sql = template.RawSql, Parameters = template.Parameters, SplitOn = SplitOnString};
            return query;
        }
    }
}