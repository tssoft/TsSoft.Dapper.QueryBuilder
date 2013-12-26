namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public class Query
    {
        public string Sql { get; set; }

        public object Parameters { get; set; }

        public string SplitOn { get; set; }
    }
}
