using System;
using TsSoft.Dapper.QueryBuilder.Models.Enumerations;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class WhereAttribute : Attribute
    {
        public WhereAttribute(string field) : this()
        {
            Field = field;
        }

        public WhereAttribute()
        {
            WhereType = WhereType.Eq;
        }

        public string Field { get; private set; }

        public string Expression { get; set; }

        public WhereType WhereType { get; set; }

        public string TableName { get; set; }
    }
}