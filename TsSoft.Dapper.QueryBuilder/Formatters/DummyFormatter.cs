namespace TsSoft.Dapper.QueryBuilder.Formatters
{
    public class DummyFormatter : IFormatter
    {
        public object Format(object input)
        {
            return input;
        }
    }
}