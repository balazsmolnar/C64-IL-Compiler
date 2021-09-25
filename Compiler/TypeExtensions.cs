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

        public static int GetVirtualMethodIndex(this Type type2, MethodBase methodInfo)
        {

            int index = 0;
            var types = GetBaseTypes(type2);

            foreach (var t in types)
            {
                var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(m => m.DeclaringType == t);
                foreach (var method in methods)
                {
                    if (!method.IsVirtual)
                        continue;

                    if (methodInfo.Name == method.Name)
                        return index;
                    index++;
                }

            }

            return index;
        }

        public static List<MethodInfo> GetVirtualMethods(this Type type2)
        {

            var types = GetBaseTypes(type2);
            var typesRev = types.Reverse<Type>().ToList();

            List<MethodInfo> result = new();

            foreach (var t in types)
            {
                var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(m => m.DeclaringType == t);
                foreach (var method in methods)
                {
                    if (!method.IsVirtual)
                        continue;

                    if (result.Any(x => x.Name == method.Name))
                        continue;

                    foreach (var t2 in typesRev)
                    {
                        var m = t2.GetMethod(method.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                        if (m.DeclaringType == t2)
                        {
                            result.Add(m);
                            break;
                        }
                    }
                }

            }

            return result;
        }

        private static List<Type> GetBaseTypes(Type type)
        {
            List<Type> types = new();
            var type2 = type;
            while (type2 != typeof(object) && type2 != null)
            {
                types.Add(type2);
                type2 = type2.BaseType;
            }
            types.Reverse();

            return types;
        }
    }
}