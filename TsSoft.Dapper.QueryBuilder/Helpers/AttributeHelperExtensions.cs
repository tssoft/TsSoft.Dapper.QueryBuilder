using System;
using System.Linq;
using System.Reflection;

namespace TsSoft.Dapper.QueryBuilder.Helpers
{
    public static class AttributeHelperExtensions
    {
        public static T GetCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return (T) memberInfo.GetCustomAttributes(typeof (T), false).SingleOrDefault();
        }


        public static T Getattribute<T>(this Enum value) where T : Attribute
        {
            return value.GetType().GetField(value.ToString()).GetCustomAttribute<T>();
        }
    }
}