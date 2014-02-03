using System.Collections.Generic;
using System.Linq;
using TsSoft.Dapper.QueryBuilder.Helpers.Select;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Models
{
    public class Criteria
    {
        public Criteria()
        {
            SelectClause = new SelectClause
                {
                    IsExpression = false,
                    Select = "*",
                };
        }

        public int Take { get; set; }

        public int Skip { get; set; }

        public QueryType QueryType { get; set; }

        public IDictionary<string, OrderType> Order { get; set; }

        public SelectClause SelectClause { get; set; }

        public bool HasOrder
        {
            get { return Order != null && Order.Any(); }
        }

        public bool HasGrouping
        {
            get { return GroupBy != null && GroupBy.Any(); }
        }

        public IEnumerable<string> GroupBy { get; set; }
    }
}