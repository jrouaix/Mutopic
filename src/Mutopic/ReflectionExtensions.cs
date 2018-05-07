using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mutopic
{
    /// <summary>
    /// Some reflection helpers
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Get topic name from a type name.
        /// Currently it's just a factorized way to call type.Name
        /// </summary>
        /// <param name="type">Type of the message</param>
        /// <returns>Returns a topic name</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTopicName(this Type type) => type.Name;

        internal static Type[] GetAllInheritedTypes(this Type type, bool returnSelf = false)
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

        internal static IEnumerable<Type> GetAllInheritedInterfaces(this Type type, bool returnSelf = false)
        {
            return InternalGetAllInheritedTypes(type, returnSelf)
                .Where(t => t.IsInterface)
                .Distinct();
        }
    }
}
