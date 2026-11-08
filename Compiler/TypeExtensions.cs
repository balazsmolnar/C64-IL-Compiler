using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Compiler;

static class TypeExtension
{
    // The instance fields of an object of `type`, in heap order: the base
    // class's own layout first (so a base class's code finds its fields at
    // the same offsets in every derived object, private ones included --
    // a primary constructor's captured parameter, an auto-property's
    // backing field), then the fields `type` itself declares, references
    // first. Class-library bases (System.Object) contribute nothing.
    //
    // The GC traces an object's first referenceFields bytes as references
    // (asm/helper/object.asm), so the references have to be a prefix of the
    // whole layout: ReferenceFieldCount reports a reference field that is
    // not (a derived class adding one under a base class with value fields).
    static public List<FieldInfo> GetInstanceLayout(this Type type)
    {
        lock (LayoutCache)
        {
            if (LayoutCache.TryGetValue(type, out var cached))
                return cached;
            var layout = new List<FieldInfo>();
            // A class-library type (Func<T>) keeps its flat, references-first
            // layout.
            bool program = type.Assembly == MethodBaseExtension.ProgramAssembly;
            if (program && type.BaseType != null && type.BaseType.Assembly == type.Assembly)
                layout.AddRange(type.BaseType.GetInstanceLayout());
            layout.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | (program ? BindingFlags.DeclaredOnly : 0))
                .Where(f => !f.IsLiteral)
                .OrderBy(f => f.FieldType.IsReferenceCounted() ? 0 : 1));
            LayoutCache[type] = layout;
            return layout;
        }
    }

    private static readonly Dictionary<Type, List<FieldInfo>> LayoutCache = new();

    static public int GetInstanceSize(this Type type) =>
        type.GetInstanceLayout().Sum(f => f.FieldType.GetStorageBytes());

    // How many leading fields are references, or -1 when a reference field
    // comes after a value field (see GetInstanceLayout).
    static public int ReferenceFieldCount(this Type type)
    {
        var layout = type.GetInstanceLayout();
        int prefix = layout.TakeWhile(f => f.FieldType.IsReferenceCounted()).Count();
        return layout.Skip(prefix).Any(f => f.FieldType.IsReferenceCounted()) ? -1 : prefix;
    }

    static public bool IsSameField(this FieldInfo a, FieldInfo b) =>
        a.MetadataToken == b.MetadataToken && a.Module == b.Module;

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
        // Native C64 MFLPT format (see Compiler/Mflpt.cs), not IEEE754's 4 --
        // storing float this way lets runtime arithmetic hand values
        // straight to the BASIC ROM's float routines with no conversion.
        if (type == typeof(float))
            return 5;
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