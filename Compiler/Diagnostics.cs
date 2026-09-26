using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Compiler;

// Error codes. Stable, so a message can be searched for and documented.
static class DiagnosticCodes
{
    public const string UnsupportedInstruction = "C64001"; // a C# construct that compiles to an IL instruction we can't translate
    public const string UnsupportedCall = "C64002";        // a call into the .NET class library (or similar) with no C64 implementation
    public const string UnsupportedType = "C64003";        // a type we can't represent
    public const string UnsupportedDeclaration = "C64004"; // e.g. generics, interface dispatch
    public const string InternalError = "C64900";          // the compiler itself failed on this method
}

// Thrown wherever the compiler meets a C# construct it cannot compile. Caught
// per method by ILCodePass, which records it (with the source line) and goes
// on to the next method, so one build reports every problem, not just the
// first.
class UnsupportedFeatureException : Exception
{
    public string Code { get; }
    public string Hint { get; }

    public UnsupportedFeatureException(string code, string message, string hint = null) : base(message)
    {
        Code = code;
        Hint = hint;
    }
}

class CompilerDiagnostic
{
    public string Code { get; init; }
    public string Message { get; init; }
    public string Hint { get; init; }
    public string File { get; init; }
    public int Line { get; init; }
    public string Method { get; init; }

    // MSBuild/VS Code problem-matcher shape: "file(line): error CODE: text".
    public override string ToString()
    {
        var location = File != null ? $"{File}({Line})" : (Method ?? "compiler");
        var text = $"{location}: error {Code}: {Message}";
        if (File != null && Method != null)
            text += $" (in {Method})";
        if (!string.IsNullOrEmpty(Hint))
            text += $"\n    hint: {Hint}";
        return text;
    }
}

class CompilerDiagnostics
{
    private readonly List<CompilerDiagnostic> _items = new();

    public IReadOnlyList<CompilerDiagnostic> Items => _items;
    public bool HasErrors => _items.Count > 0;

    public void Add(CompilerDiagnostic diagnostic)
    {
        // The same construct can be reported by two passes; keep one.
        if (!_items.Any(d => d.ToString() == diagnostic.ToString()))
            _items.Add(diagnostic);
    }

    public void ThrowIfErrors()
    {
        if (HasErrors)
            throw new CompilationFailedException(this);
    }
}

class CompilationFailedException : Exception
{
    public CompilerDiagnostics Diagnostics { get; }

    public CompilationFailedException(CompilerDiagnostics diagnostics)
        : base($"Compilation failed with {diagnostics.Items.Count} error(s).")
    {
        Diagnostics = diagnostics;
    }
}

// Maps (method, IL offset) back to a C# source line through the assembly's
// portable PDB -- the same file the debugger support reads. Without a PDB
// (or for a compiler-generated method) it just returns null and the message
// names the method instead.
class SourceLocator
{
    private readonly MetadataReader _reader;

    public SourceLocator(Assembly assembly)
    {
        try
        {
            var pdbPath = Path.ChangeExtension(assembly.Location, ".pdb");
            if (File.Exists(pdbPath))
            {
                var provider = MetadataReaderProvider.FromPortablePdbStream(File.OpenRead(pdbPath));
                _reader = provider.GetMetadataReader();
            }
        }
        catch (Exception)
        {
            // Diagnostics must never fail the build on their own.
            _reader = null;
        }
    }

    public (string file, int line)? Locate(MethodBase method, int? ilOffset)
    {
        if (_reader == null || method == null)
            return null;
        try
        {
            var handle = MetadataTokens.EntityHandle(method.MetadataToken);
            if (handle.Kind != HandleKind.MethodDefinition)
                return null;
            var info = _reader.GetMethodDebugInformation(((MethodDefinitionHandle)handle).ToDebugInformationHandle());

            SequencePoint? best = null;
            SequencePoint? first = null;
            foreach (var sp in info.GetSequencePoints())
            {
                if (sp.IsHidden)
                    continue;
                first ??= sp;
                if (ilOffset.HasValue && sp.Offset <= ilOffset.Value)
                    best = sp;
            }

            var chosen = best ?? first;
            if (chosen == null)
                return null;
            var doc = _reader.GetDocument(chosen.Value.Document);
            return (Path.GetFullPath(_reader.GetString(doc.Name)), chosen.Value.StartLine);
        }
        catch (Exception)
        {
            return null;
        }
    }
}

// The plain-language description of every IL instruction this compiler can't
// translate, in terms of the C# the user wrote. Anything not listed here (and
// not in CommandMap) still gets a generic message, never a raw exception.
static class UnsupportedInstructions
{
    private const string NoExceptions = "Exceptions are not supported: check for the error condition explicitly and handle it (or return a value).";

    public static UnsupportedFeatureException For(ILOpCode opCode)
    {
        var (feature, hint) = Describe(opCode);
        return new UnsupportedFeatureException(DiagnosticCodes.UnsupportedInstruction,
            $"{feature} is not supported by the C64 compiler.", hint);
    }

    private static (string feature, string hint) Describe(ILOpCode opCode)
    {
        switch (opCode)
        {
            case ILOpCode.Isinst:
                return ("A type test ('is' or 'as')", "Use a virtual method, or an enum/tag field on the base class, to tell the types apart.");
            case ILOpCode.Castclass:
                return ("A cast between class types", "Keep the value in a variable of the derived type, or call a virtual method on the base type.");
            case ILOpCode.Box:
            case ILOpCode.Unbox:
            case ILOpCode.Unbox_any:
                return ("Boxing a value type (converting it to object)", "Avoid APIs that take object, such as string.Format(...) or Console.WriteLine(int); convert with ToString() or string interpolation first.");
            case ILOpCode.Initobj:
            case ILOpCode.Ldobj:
            case ILOpCode.Stobj:
            case ILOpCode.Cpobj:
            case ILOpCode.Ldelem:
            case ILOpCode.Stelem:
                return ("A struct (user-defined value type)", "Use a class instead; objects live on the heap on this platform.");
            case ILOpCode.Ldelema:
                return ("Taking the address of an array element", "Copy the element to a local variable first, and write it back afterwards.");
            case ILOpCode.Leave:
            case ILOpCode.Leave_s:
            case ILOpCode.Endfinally:
            case ILOpCode.Endfilter:
                return ("try/catch/finally", NoExceptions);
            case ILOpCode.Throw:
            case ILOpCode.Rethrow:
                return ("The 'throw' statement", NoExceptions);
            case ILOpCode.Ldind_i:
            case ILOpCode.Ldind_i1:
            case ILOpCode.Ldind_i2:
            case ILOpCode.Ldind_i4:
            case ILOpCode.Ldind_i8:
            case ILOpCode.Ldind_r4:
            case ILOpCode.Ldind_r8:
            case ILOpCode.Ldind_ref:
            case ILOpCode.Ldind_u1:
            case ILOpCode.Ldind_u2:
            case ILOpCode.Ldind_u4:
            case ILOpCode.Stind_i:
            case ILOpCode.Stind_i1:
            case ILOpCode.Stind_i2:
            case ILOpCode.Stind_i4:
            case ILOpCode.Stind_i8:
            case ILOpCode.Stind_r4:
            case ILOpCode.Stind_r8:
            case ILOpCode.Stind_ref:
                return ("A ref or out parameter (or another by-reference variable)", "Return the value, or keep it in a field, instead of passing it by reference.");
            case ILOpCode.Ldc_r8:
            case ILOpCode.Conv_r8:
                return ("double-precision floating point (double)", "Use float, the C64's native 5-byte format.");
            case ILOpCode.Ldc_i8:
                return ("A 64-bit integer literal", "long and ulong are 16 bits wide on this platform; the literal has to fit in 16 bits.");
            case ILOpCode.Conv_i2:
            case ILOpCode.Conv_u2:
                return ("A conversion to short, ushort or char", "Use byte or uint (8-bit) or ulong or long (16-bit); short and ushort are not supported.");
            case ILOpCode.Ldelem_i2:
            case ILOpCode.Ldelem_u2:
            case ILOpCode.Stelem_i2:
                return ("An array of short, ushort or char", "Use a byte or uint array (8-bit elements) or a ulong or long array (16-bit elements).");
            case ILOpCode.Ldelem_r8:
            case ILOpCode.Stelem_r8:
                return ("An array of double", "Use a float array.");
            case ILOpCode.Add_ovf:
            case ILOpCode.Add_ovf_un:
            case ILOpCode.Sub_ovf:
            case ILOpCode.Sub_ovf_un:
            case ILOpCode.Mul_ovf:
            case ILOpCode.Mul_ovf_un:
            case ILOpCode.Conv_ovf_i1:
            case ILOpCode.Conv_ovf_i2:
            case ILOpCode.Conv_ovf_i4:
            case ILOpCode.Conv_ovf_i8:
            case ILOpCode.Conv_ovf_u1:
            case ILOpCode.Conv_ovf_u2:
            case ILOpCode.Conv_ovf_u4:
            case ILOpCode.Conv_ovf_u8:
                return ("Checked arithmetic (checked { } or an overflow-checking conversion)", "Use unchecked arithmetic, which is the default. Values wrap around.");
            case ILOpCode.Calli:
            case ILOpCode.Ldvirtftn:
                return ("Calling through a function pointer or a virtual method group", "Call the method directly.");
            case ILOpCode.Localloc:
                return ("stackalloc", "Use a normal array.");
            case ILOpCode.Sizeof:
                return ("sizeof", "Use a constant for the size.");
            default:
                return ($"The IL instruction '{opCode}'", "This C# construct has no C64 implementation.");
        }
    }
}

// Types the compiler has no representation for. Checked on every field,
// parameter, return type and local of the assembly being compiled.
static class UnsupportedTypes
{
    // Returns null when the type is fine.
    public static UnsupportedFeatureException Check(Type type, Assembly compiledAssembly, string what)
    {
        if (type == null || type.IsGenericParameter)
            return null;

        if (type.IsByRef || type.IsPointer)
            return Fail($"{what} of type '{Name(type)}' (a by-reference or pointer type)",
                "Return the value, or keep it in a field, instead of passing it by reference.");

        if (type.IsArray)
            return Check(type.GetElementType(), compiledAssembly, what);

        if (type == typeof(short) || type == typeof(ushort))
            return Fail($"{what} of type '{Name(type)}'", "Use byte or uint (8-bit) or ulong or long (16-bit); short and ushort are not supported.");
        if (type == typeof(double))
            return Fail($"{what} of type 'double'", "Use float, the C64's native 5-byte format.");
        if (type == typeof(decimal))
            return Fail($"{what} of type 'decimal'", "Use ulong or long, or float.");
        if (type == typeof(IntPtr) || type == typeof(UIntPtr))
            return Fail($"{what} of type '{Name(type)}'", "Native-size integers are not supported; use ulong or long.");

        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(object) || type == typeof(void))
            return null;

        // The C# compiler's string-interpolation helper: ILStringInterpolationPass
        // rewrites the whole $"..." pattern away, so it never reaches the output.
        if (type.FullName == "System.Runtime.CompilerServices.DefaultInterpolatedStringHandler")
            return null;

        if (type.IsValueType)
            return Fail($"{what} of type '{Name(type)}' (a struct)", "Use a class instead; objects live on the heap on this platform.");

        // Reference types: the assembly being compiled, C64Lib, and the few
        // BCL types the compiler special-cases.
        if (type.Assembly == compiledAssembly)
            return null;
        if (type.FullName != null && type.FullName.StartsWith("C64Lib."))
            return null;
        if (type.FullName != null && type.FullName.StartsWith("C64TestFramework."))
            return null;
        if (typeof(Delegate).IsAssignableFrom(type))
            return CheckDelegate(type, what);
        if (type == typeof(Array) || type == typeof(Delegate) || type == typeof(MulticastDelegate))
            return null;

        return Fail($"{what} of type '{Name(type)}' from the .NET class library",
            "Only your own classes, C64Lib types, string, arrays and Func<T> are available on the C64.");
    }

    // Only Func<TResult> can be invoked; any delegate type can be handed to
    // C64.Interrupt (a static handler), which is all the rest are good for.
    private static UnsupportedFeatureException CheckDelegate(Type type, string what)
    {
        if (type.IsGenericType && type.Name.StartsWith("Func") && type.GetGenericArguments().Length > 1)
            return Fail($"{what} of type '{Name(type)}'", "Only Func<TResult> (no parameters) is supported. Pass values through fields or captured variables instead.");
        return null;
    }

    // The C# keyword where there is one, so the message says 'short', not 'Int16'.
    private static string Name(Type type)
    {
        if (type == typeof(short)) return "short";
        if (type == typeof(ushort)) return "ushort";
        if (type == typeof(int)) return "int";
        if (type == typeof(uint)) return "uint";
        if (type == typeof(long)) return "long";
        if (type == typeof(ulong)) return "ulong";
        if (type == typeof(double)) return "double";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(sbyte)) return "sbyte";
        return type.IsGenericType ? type.Name.Split('`')[0] + "<...>" : type.Name;
    }

    private static UnsupportedFeatureException Fail(string message, string hint) =>
        new(DiagnosticCodes.UnsupportedType, $"{char.ToUpper(message[0])}{message.Substring(1)} is not supported.", hint);
}

// Plain-language hints for calls into the .NET class library, so that the
// error says what to do instead of just what failed.
static class UnsupportedCalls
{
    public static UnsupportedFeatureException For(MethodBase method)
    {
        var type = method.DeclaringType;
        var typeName = type?.FullName ?? "?";
        var call = $"{type?.Name}.{method.Name}";
        return new UnsupportedFeatureException(DiagnosticCodes.UnsupportedCall,
            $"The call to '{call}' is not supported: {typeName} is not part of the C64 runtime.", HintFor(type, method));
    }

    private static string HintFor(Type type, MethodBase method)
    {
        var full = type?.FullName ?? "";
        var name = method.Name;

        if (type != null && typeof(Exception).IsAssignableFrom(type))
            return "Exceptions are not supported: check for the error condition explicitly and handle it (or return a value).";
        if (type != null && type.IsArray && type.GetArrayRank() > 1)
            return "Multi-dimensional arrays are not supported; use a jagged array (uint[][]) or index a one-dimensional array as y * width + x.";
        if (full == "System.Runtime.CompilerServices.RuntimeHelpers" && name == "InitializeArray")
            return "An array given inline as an argument (params, or a collection expression) is not supported; put it in a variable first: uint[] a = { 1, 2, 3 };";

        if (type == typeof(string))
        {
            return name switch
            {
                "op_Equality" or "op_Inequality" or "Equals" or "Compare" or "CompareOrdinal" =>
                    "String comparison is not implemented yet; compare a Length and the characters yourself, or use an enum/number instead of a string as the key.",
                "get_Chars" => "String indexing is not implemented yet.",
                "Concat" => "Only the two-string overload of string.Concat (a + b) is supported; join longer strings one pair at a time, or use string interpolation.",
                "Format" => "string.Format is not supported; use string interpolation ($\"...\").",
                _ => "The only string members supported are + (two strings), .Length, .PadLeft(int, char) and string interpolation."
            };
        }
        if (full == "System.Math")
            return "System.Math is not supported. Use an expression such as (a > b ? a : b) for Min/Max; MathF.Sin, Cos and Sqrt are supported for float.";
        if (full == "System.Array")
            return "Array.Copy/Clear/Fill/Resize are not supported; use a loop, or C64.CopyMemory and C64.FillMemory for raw memory.";
        if (full.StartsWith("System.Collections") || full.StartsWith("System.Linq"))
            return "Generic collections and LINQ are not supported (there is no generics support yet). Use arrays.";
        if (full.StartsWith("System.Nullable") || full.StartsWith("System.ValueTuple") || full.StartsWith("System.Tuple"))
            return "Nullable value types and tuples are not supported. Use a separate bool, or a small class.";
        if (full == "System.Object")
            return "Object.ToString(), GetType(), GetHashCode() and Equals() are not supported; numeric .ToString() is.";
        if (typeof(Delegate).IsAssignableFrom(type ?? typeof(object)))
            return "Only Func<TResult> (no parameters) can be invoked. Other delegate types can only be assigned to C64.Interrupt.";
        if (full == "System.Console")
            return "Only Console.Write and Console.WriteLine with a single string argument are supported.";
        if (full.StartsWith("System.Text"))
            return "StringBuilder and encodings are not supported; build strings with + and string interpolation.";
        if (full == "System.Random")
            return "Use C64.Random() instead.";
        if (full.StartsWith("System.Threading") || full.StartsWith("System.Threading.Tasks"))
            return "Threads, tasks and async/await are not supported.";
        if (full == "System.Convert" || name == "Parse" || name == "TryParse")
            return "Parsing and conversion helpers are not supported.";
        if (full == "System.MathF")
            return "Only MathF.Sin, Cos and Sqrt are supported.";
        if (full == "System.GC")
            return "Only GC.Collect() is supported.";

        return "Only your own code, C64Lib.*, MathF.Sin/Cos/Sqrt, Console.Write/WriteLine(string), GC.Collect(), Func<T>.Invoke(), numeric ToString() and the string basics (+, Length, PadLeft) are available.";
    }
}
