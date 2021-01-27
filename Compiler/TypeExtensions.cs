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
    }
}