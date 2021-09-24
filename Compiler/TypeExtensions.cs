using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Compiler
{
    static class TypeExtension
    {
        static public bool IsReferenceCounted(this Type type)
        {
            if (type.IsValueType)
                return false;
            if (type == typeof(string))
                return false;
            if (type.FullName.StartsWith("C64Lib."))
                return false;
            return true;
        }

        static public int GetStorageBytes(this Type type)
        {
            if (type == typeof(string) || type == typeof(long) || type == typeof(ulong))
                return 2;
            return 1;
        }

        static public void CheckCompatible(this Type type, Type otherType)
        {
            if (type.IsValueType)
            {
                if (type.GetStorageBytes() != otherType.GetStorageBytes())
                    throw new InvalidOperationException($"Incompatible types: {type.Name}, {otherType.Name}");
            }
            else if (type == typeof(string))
            {
                if (otherType != typeof(string))
                    throw new InvalidOperationException($"Incompatible types: {type.Name}, {otherType.Name}");
            }
            else
            {
                if (otherType.IsValueType)
                    throw new InvalidOperationException($"Incompatible types: {type.Name}, {otherType.Name}");
            }
        }

        public static List<MethodInfo> VirtualMethods(this Type type2)
        {
            List<MethodInfo> result = new();
            List<Type> types = new();
            var @type = type2;
            while (@type != typeof(object) && @type != null)
            {
                types.Add(@type);
                @type = @type.BaseType;
            }
            types.Reverse();

            foreach (var t in types)
            {
                var methods = type2.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(m => m.DeclaringType == t);
                foreach (var method in methods)
                {
                    if (!method.IsVirtual)
                        continue;

                    result.Add(method);
                }

            }

            return result;
        }

    }
}