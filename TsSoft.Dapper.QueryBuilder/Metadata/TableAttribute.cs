using System;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public class TableAttribute : Attribute
    {
        public TableAttribute()
        {
            
        }

        public TableAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}