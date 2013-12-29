using System;
using TsSoft.Dapper.QueryBuilder.Formatters;

namespace TsSoft.Dapper.QueryBuilder.Metadata
{
    public class FormatAttribute : Attribute
    {
        public readonly Type FormatterType;

        public FormatAttribute(Type formatter)
        {
            if (!formatter.IsAssignableFrom(typeof (IFormatter)))
            {
                throw new ArgumentException("Type must implement IFormatter");
            }
            FormatterType = formatter;
        }
    }
}