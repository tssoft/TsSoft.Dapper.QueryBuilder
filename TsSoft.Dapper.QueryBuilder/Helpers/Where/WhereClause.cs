namespace TsSoft.Dapper.QueryBuilder.Helpers.Where
{
    public class WhereClause
    {
        public string Sql { get; set; }

        public string ParameterName { get; set; }

        public object ParameterValue { get; set; }

        public bool WithoutValue { get; set; }
    }
}