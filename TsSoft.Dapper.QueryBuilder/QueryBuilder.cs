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
        private const string SimpleSqlTemplate =
            @"Select /**select**/ from {0} /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**orderby**/";

        private const string ExistsSqlTemplate = "Select 1 from {0} /**innerjoin**/ /**leftjoin**/ /**where**/";
        private const string CountSqlTemplate = "Select count(1) from {0} /**innerjoin**/ /**leftjoin**/ /**where**/";

        private const string PaginateSqlTemplate =
            @"Select /**select**/ from {0} /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**orderby**/ OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

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

        protected virtual string TableName
        {
            get { return _table.Name; }
        }

        protected virtual string GetSimpleSql()
        {
            return
                string.Format(
                    SimpleSqlTemplate,
                    TableName);
        }

        protected virtual string GetPaginateSql()
        {
            return
                string.Format(
                    PaginateSqlTemplate,
                    TableName);
        }

        protected virtual string GetCountSql()
        {
            return string.Format(CountSqlTemplate, TableName);
        }

        protected virtual string GetExistsSql()
        {
            return string.Format(ExistsSqlTemplate, TableName);
        }

        protected virtual void Select(TCriteria criteria)
        {
            var selects = SelectClauseManager.Get(criteria, TableName);
            foreach (var selectClause in selects)
            {
                Builder.Select(
                    selectClause.IsExpression
                        ? selectClause.Select
                        : string.Format("{0}.{1}", selectClause.Table, selectClause.Select));
            }
        }

        protected virtual void Join(TCriteria criteria)
        {
            IEnumerable<JoinClause> joinClauses =
                JoinClauseManager.Get(criteria, TableName)
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

        protected virtual void Where(TCriteria criteria)
        {
            var whereClauses = WhereClauseManager.Get(criteria, TableName);
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

        protected virtual void GroupBy(TCriteria criteria)
        {
        }

        protected virtual void OrderBy(TCriteria criteria)
        {
            if (criteria.HasOrder)
            {
                foreach (var order in criteria.Order)
                {
                    Builder.OrderBy(string.Format("{0} {1}", order.Key, order.Value));
                }
            }
        }

        protected SqlBuilder.Template GetTemplate(TCriteria criteria)
        {
            switch (criteria.QueryType)
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

        private void Init()
        {
            Builder = new SqlBuilder();

            SimplyTemplate = Builder.AddTemplate(GetSimpleSql());
            PaginateTemplate = Builder.AddTemplate(GetPaginateSql(), new {Criteria.Skip, Criteria.Take});
            CountTemplate = Builder.AddTemplate(GetCountSql());
            ExistsTemplate = Builder.AddTemplate(GetExistsSql());
        }

        public virtual Query Build()
        {
            Init();
            Select(Criteria);
            Join(Criteria);
            Where(Criteria);
            GroupBy(Criteria);
            OrderBy(Criteria);
            var template = GetTemplate(Criteria);
            var query = new Query {Sql = template.RawSql, Parameters = template.Parameters, SplitOn = SplitOnString};
            return query;
        }
    }
}