namespace TsSoft.Dapper.QueryBuilder.Helpers.Select
{
    public class SelectClause
    {
        public SelectClause(string @select)
        {
            Select = @select;
            IsExpression = true;
        }

        public SelectClause(string @select, bool isExpression)
        {
            Select = @select;
            IsExpression = isExpression;
        }

        public SelectClause(string @select, string table, bool isExpression)
        {
            Select = @select;
            Table = table;
            IsExpression = isExpression;
        }

        public SelectClause()
        {
            
        }


        public string Select { get; set; }

        public string Table { get; set; }

        public bool IsExpression { get; set; }

        protected bool Equals(SelectClause other)
        {
            return string.Equals(Select, other.Select) && string.Equals(Table, other.Table) &&
                   IsExpression.Equals(other.IsExpression);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SelectClause) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Select != null ? Select.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Table != null ? Table.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ IsExpression.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SelectClause left, SelectClause right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SelectClause left, SelectClause right)
        {
            return !Equals(left, right);
        }
    }
}