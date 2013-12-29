namespace TsSoft.Dapper.QueryBuilder.Formatters
{
    public interface IFormatter
    {
        void Format(ref object input);
    }
}