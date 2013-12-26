using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dapper
{
    public class SqlBuilder
    {
        private readonly Dictionary<string, Clauses> data = new Dictionary<string, Clauses>();
        private int seq;


        public Template AddTemplate(string sql, dynamic parameters = null)
        {
            return new Template(this, sql, parameters);
        }

        private void AddClause(string name, string sql, object parameters, string joiner, string prefix = "",
                               string postfix = "")
        {
            Clauses clauses;
            if (!data.TryGetValue(name, out clauses))
            {
                clauses = new Clauses(joiner, prefix, postfix);
                data[name] = clauses;
            }
            clauses.Add(new Clause {Sql = sql, Parameters = parameters});
            seq++;
        }

        public SqlBuilder InnerJoin(string sql, dynamic parameters = null)
        {
            AddClause("innerjoin", sql, parameters, joiner: "\nINNER JOIN ", prefix: "\nINNER JOIN ", postfix: "\n");
            return this;
        }

        public SqlBuilder LeftJoin(string sql, dynamic parameters = null)
        {
            AddClause("leftjoin", sql, parameters, joiner: "\nLEFT JOIN ", prefix: "\nLEFT JOIN ", postfix: "\n");
            return this;
        }

        public SqlBuilder RightJoin(string sql, dynamic parameters = null)
        {
            AddClause("rightjoin", sql, parameters, joiner: "\nRIGHT JOIN ", prefix: "\nRIGHT JOIN ", postfix: "\n");
            return this;
        }

        public SqlBuilder Where(string sql, dynamic parameters = null)
        {
            AddClause("where", sql, parameters, " AND ", prefix: "WHERE ", postfix: "\n");
            return this;
        }

        public SqlBuilder OrderBy(string sql, dynamic parameters = null)
        {
            AddClause("orderby", sql, parameters, " , ", prefix: "ORDER BY ", postfix: "\n");
            return this;
        }

        public SqlBuilder Select(string sql, dynamic parameters = null)
        {
            AddClause("select", sql, parameters, " , ", prefix: "", postfix: "\n");
            return this;
        }

        public SqlBuilder AddParameters(dynamic parameters)
        {
            AddClause("--parameters", "", parameters, "");
            return this;
        }

        public SqlBuilder Join(string sql, dynamic parameters = null)
        {
            AddClause("join", sql, parameters, joiner: "\nJOIN ", prefix: "\nJOIN ", postfix: "\n");
            return this;
        }

        public SqlBuilder GroupBy(string sql, dynamic parameters = null)
        {
            AddClause("groupby", sql, parameters, joiner: " , ", prefix: "\nGROUP BY ", postfix: "\n");
            return this;
        }

        public SqlBuilder Having(string sql, dynamic parameters = null)
        {
            AddClause("having", sql, parameters, joiner: "\nAND ", prefix: "HAVING ", postfix: "\n");
            return this;
        }

        private class Clause
        {
            public string Sql { get; set; }
            public object Parameters { get; set; }
        }

        private class Clauses : List<Clause>
        {
            private readonly string joiner;
            private readonly string postfix;
            private readonly string prefix;

            public Clauses(string joiner, string prefix = "", string postfix = "")
            {
                this.joiner = joiner;
                this.prefix = prefix;
                this.postfix = postfix;
            }

            public string ResolveClauses(DynamicParameters p)
            {
                foreach (Clause item in this)
                {
                    p.AddDynamicParams(item.Parameters);
                }
                return prefix + string.Join(joiner, this.Select(c => c.Sql)) + postfix;
            }
        }

        public class Template
        {
            private static readonly Regex regex =
                new Regex(@"\/\*\*.+\*\*\/", RegexOptions.Compiled | RegexOptions.Multiline);

            private readonly SqlBuilder builder;
            private readonly object initParams;
            private readonly string sql;
            private int dataSeq = -1; // Unresolved

            private object parameters;
            private string rawSql;

            public Template(SqlBuilder builder, string sql, dynamic parameters)
            {
                initParams = parameters;
                this.sql = sql;
                this.builder = builder;
            }

            public string RawSql
            {
                get
                {
                    ResolveSql();
                    return rawSql;
                }
            }

            public object Parameters
            {
                get
                {
                    ResolveSql();
                    return parameters;
                }
            }

            private void ResolveSql()
            {
                if (dataSeq != builder.seq)
                {
                    var p = new DynamicParameters(initParams);

                    rawSql = sql;

                    foreach (var pair in builder.data)
                    {
                        rawSql = rawSql.Replace("/**" + pair.Key + "**/", pair.Value.ResolveClauses(p));
                    }
                    parameters = p;

                    // replace all that is left with empty
                    rawSql = regex.Replace(rawSql, "");

                    dataSeq = builder.seq;
                }
            }
        }
    }
}