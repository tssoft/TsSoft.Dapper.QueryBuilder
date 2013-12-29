using System;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public class TableAttribute : Attribute
    {
        public string Name { get; set; }
    }
}