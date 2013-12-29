﻿using System.Collections.Generic;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Helpers.Join
{
    public class JoinClause
    {
        public string Splitter { get; set; }

        public IEnumerable<string> JoinSqls { get; set; }

        public string SelectSql { get; set; }

        public JoinType JoinType { get; set; }

        public bool HasJoin { get; set; }
    }
}