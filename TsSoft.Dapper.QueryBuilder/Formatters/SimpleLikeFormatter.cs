namespace TsSoft.Dapper.QueryBuilder.Formatters
{
    public class SimpleLikeFormatter : IFormatter
    {
        public object Format(object input)
        {
            return string.Format("%{0}%", input);
        }
    }
}