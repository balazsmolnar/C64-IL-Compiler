using System;
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
    }
}