using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            @"Select /**select**/ from {0} /**simplesql**/ /**where**/ /**groupby**/ /**orderby**/";

        private const string ExistsSqlTemplate = "Select 1 from {0} /**simplesql**/ /**where**/";
        private const string CountSqlTemplate = "Select count(1) from {0} /**simplesql**/ /**where**/";

        private const string PaginateSqlTemplate =
            @"Select /**select**/ from {0} /**simplesql**/ /**where**/ /**groupby**/ /**orderby**/ OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

        private static readonly TableAttribute _table;
        private static readonly Type CriteriaType = typeof (TCriteria);
        protected SqlBuilder Builder;
        protected SqlBuilder.Template CountTemplate;
        protected SqlBuilder.Template ExistsTemplate;
        protected SqlBuilder.Template PaginateTemplate;
        protected SqlBuilder.Template SimpleTemplate;
        protected ICollection<string> SplitOn;
        private string _tableName;

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
                    CriteriaType));
            }
            Builder = new SqlBuilder();
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

        protected virtual string GetTableName()
        {
            if (string.IsNullOrWhiteSpace(_tableName))
            {
                _tableName = _table.Name;
            }
            return _tableName;
        }

        protected virtual string GetSimpleSql()
        {
            return
                string.Format(
                    SimpleSqlTemplate,
                    GetTableName());
        }

        protected virtual string GetPaginateSql()
        {
            return
                string.Format(
                    PaginateSqlTemplate,
                    GetTableName());
        }

        protected virtual string GetCountSql()
        {
            return string.Format(CountSqlTemplate, GetTableName());
        }

        protected virtual string GetExistsSql()
        {
            return string.Format(ExistsSqlTemplate, GetTableName());
        }

        protected virtual void Select(TCriteria criteria)
        {
            var selects = SelectClauseManager.Get(criteria, GetTableName());
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
                JoinClauseManager.Get(criteria, GetTableName())
                    .OrderBy(x => x.Order == 0 ? int.MaxValue : x.Order);
            foreach (var joinClause in joinClauses)
            {
                if (!string.IsNullOrWhiteSpace(joinClause.Splitter))
                {
                    Builder.Select(string.Format("0 as {0}", joinClause.Splitter));                    
                }
                if (joinClause.HasJoin)
                {
                    var sb = new StringBuilder();
                    foreach (var joinSql in joinClause.JoinSqls)
                    {
                        sb.Clear();
                        sb.AppendLine();
                        switch (joinClause.JoinType)
                        {
                            case JoinType.Inner:
                                sb.AppendLine("INNER JOIN");
                                break;
                            case JoinType.Left:
                                sb.AppendLine("LEFT JOIN");
                                break;
                            case JoinType.Right:
                                sb.AppendLine("RIGHT JOIN");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        sb.AppendLine(joinSql);
                        Builder.SimpleSql(sb.ToString());
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
            var whereClauses = WhereClauseManager.Get(criteria, GetTableName());
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
                    return SimpleTemplate;
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
            Builder.Clear();
            SimpleTemplate = Builder.AddTemplate(GetSimpleSql());
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