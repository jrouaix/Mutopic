using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopic
{
    public static class ReflectionExtensions
    {
        public static Type[] GetAllInheritedTypes(this Type type, bool returnSelf = false)
        {
            return InternalGetAllInheritedTypes(type, returnSelf).Distinct().ToArray();
        }

        static IEnumerable<Type> InternalGetAllInheritedTypes(this Type type, bool returnSelf)
        {
            if (returnSelf) yield return type;

            if (type.BaseType != null)
                foreach (var inherited in InternalGetAllInheritedTypes(type.BaseType, true))
                    yield return inherited;

            foreach (var @interface in type.GetInterfaces())
                foreach (var inherited in InternalGetAllInheritedTypes(@interface, true))
                    yield return inherited;
        }

        public static IEnumerable<Type> GetAllInheritedInterfaces(this Type type, bool returnSelf = false)
        {
            return InternalGetAllInheritedTypes(type, returnSelf)
                .Where(t => t.IsInterface)
                .Distinct();
        }
    }
}
