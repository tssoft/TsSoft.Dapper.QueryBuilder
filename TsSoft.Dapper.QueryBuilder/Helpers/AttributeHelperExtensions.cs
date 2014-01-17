using System;
using System.Collections.Generic;
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

        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(T), false).Select(x => (T)x);
        }

        public static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            var res = GetCustomAttributes<T>(memberInfo);
            return res != null && res.Any();
        }

        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            return value.GetType().GetField(value.ToString()).GetCustomAttribute<T>();
        }
    }
}