using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using Compiler;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Compiler.Ops;

internal class OpBase
{
    public OpBase(int parameterSize)
    {
        ParameterSize = parameterSize;
    }

    public OpBase(int parameterSize, string command) : this(parameterSize)
    {
        Command = command;
    }

    public virtual string Command { get; }
    public int ParameterSize { get; }

    public virtual string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var command = Command + SizeSuffix(context, operation);
        if (operation.RawParameter == null)
            return command;
        return $"{command} {operation.RawParameter}";
    }

    // Pulled out of Emit so a subclass (see OpArithmetic1/OpArithmetic2/
    // OpCompare) can insert a float branch ahead of the existing binary
    // 8-vs-16 one, without touching every other OpBase subclass's behavior.
    protected virtual string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (!Is16BitSupported)
            return "";
        return Is16Bit(context, operation) ? "16" : "8";
    }

    public virtual object ConvertParameter(CompilerMethodContext context, ILOperation operation) => null;

    public virtual void SetNextInstructions(CompilerMethodContext context, ILOperation operation, ILOperation nextOperation)
    {
        operation.NextInstructions.Add(nextOperation);
    }

    public virtual void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {

    }

    public virtual bool Is16BitSupported => false;

    public virtual bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return operation.StackContent.Last().GetStorageBytes() == 2;
    }
}

class OpLdstr : OpBase
{
    public OpLdstr() : base(4, "#stack_push_pointer")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        if (!context.CompilerContext.StringValues.ContainsKey((int)operation.RawParameter))
            context.CompilerContext.StringValues.Add((int)operation.RawParameter, context.CompilerContext.Assembly.ManifestModule.ResolveString((int)operation.RawParameter));
        return $"string_{operation.RawParameter}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(typeof(string));
    }
}

class OpLdftn : OpBase
{
    public OpLdftn() : base(4, "#stack_push_pointer")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)operation.RawParameter).GetLabel();
    }
    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(typeof(object));
    }
}

class OpCall : OpBase
{
    public OpCall() : base(4, "jsr")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)operation.RawParameter).GetLabel();
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var method = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)operation.OriginalParameter) as MethodBase;
        var parameterNum = method.GetParameters().Length + (method.IsStatic ? 0 : 1);
        operation.StackContent.RemoveLast(parameterNum);

        if (method is MethodInfo)
        {
            if (((MethodInfo)method).ReturnType != typeof(void))
                operation.StackContent.Add(((MethodInfo)method).ReturnType);
        }
    }
}

class OpCallVirt : OpCall
{
    // Keyed by (call site's static receiver type, method name) -- this compiler's
    // own vtable (TypeExtensions.GetVirtualMethodIndex/GetVirtualMethods) already
    // resolves overrides by name only, not full signature, so this check matches
    // that same (name-only) notion of "override" for consistency, rather than a
    // stricter one that could disagree with it.
    private readonly Dictionary<(Type, string), bool> _overriddenAnywhereCache = new();

    public OpCallVirt()
    {
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        bool normalCall = false;
        var methodInfo = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)operation.OriginalParameter);
        // for Func<>
        if (!methodInfo.IsVirtual || methodInfo.ReflectedType.Name.StartsWith("Func"))
            normalCall = true;
        else if (!methodInfo.IsAbstract && !IsOverriddenAnywhere(context.CompilerContext.Assembly, methodInfo.ReflectedType, methodInfo.Name))
            // Closed-world devirtualization: a general JIT has to stay conservative
            // here, since code can still load dynamically after it decides. This
            // compiler never has that problem -- the entire program is one already-
            // fully-loaded assembly by the time any of it is compiled, so "nothing
            // anywhere overrides this" is a permanent fact, not just true so far.
            // (methodInfo.IsAbstract is excluded because it never has a body of its
            // own to jump to -- true regardless of overrides, and if it somehow had
            // none, the vtable slot it'd otherwise use would be equally meaningless.)
            normalCall = true;

        if (normalCall)
            return base.Emit(context, operation);

        var index = methodInfo.ReflectedType.GetVirtualMethodIndex(methodInfo);
        var stackPosition = methodInfo.GetParameters().Select(x => x.ParameterType.GetStorageBytes()).Sum() + 1;
        if (index == -1)
            throw new InvalidOperationException($"Virtual method not found: {methodInfo.Name}. Type: {methodInfo.ReflectedType.Name}");
        return $"#callVirt {index}, {stackPosition}";
    }

    private bool IsOverriddenAnywhere(Assembly assembly, Type declaringType, string methodName)
    {
        var key = (declaringType, methodName);
        if (_overriddenAnywhereCache.TryGetValue(key, out var cached))
            return cached;

        bool overridden = false;
        foreach (var candidateType in assembly.GetTypes())
        {
            if (candidateType == declaringType || !declaringType.IsAssignableFrom(candidateType))
                continue;

            var methods = candidateType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (methods.Any(m => m.IsVirtual && m.Name == methodName))
            {
                overridden = true;
                break;
            }
        }

        _overriddenAnywhereCache[key] = overridden;
        return overridden;
    }
}

class OpNewObj : OpBase
{
    public OpNewObj() : base(4, "#newObj")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var method = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)operation.RawParameter);
        var t = method.ReflectedType;

        // Delegates OTHER than Func<T> (which already has its own working
        // heap-allocated ctor/Invoke pair, Func_1_x_ctor/Func_1_Invoke in
        // asm/system.asm -- see the Name.StartsWith("Func") check this
        // matches, e.g. OpCallVirt above) are special-cased entirely in
        // Emit below: no #newObj heap allocation at all. This return value
        // is unused for them, and skipping the computation below avoids
        // reflecting a real BCL delegate's CLR-internal fields (_target/
        // _methodPtr/_methodPtrAux/...), which was never meaningful input
        // for this compiler anyway.
        if (typeof(Delegate).IsAssignableFrom(t) && !t.Name.StartsWith("Func"))
            return null;

        var size = 0;
        var referenceFields = 0;
        foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            size += f.FieldType.GetStorageBytes();
            if (f.FieldType.IsReferenceCounted())
                referenceFields++;
        }
        //var label = context.CompilerContext.Assembly.ManifestModule.ResolveMethod(operation.RawParameter).GetLabel();


        var vtable = t.IsGenericType ? "0" : $"{t.FullName.ToValidName()}_VTable";
        // var ctor = $"{t.Name}_x_ctor";
        var ctor = "0";

        if (t.Assembly == typeof(Func<object>).Assembly)
            ctor = $"{t.Name}_x_ctor".ToValidName();
        return $"{size}, {referenceFields}, {vtable}, {ctor}";
    }

    // Static-method delegate construction (`ldnull; ldftn M; newobj
    // SomeDelegate::.ctor(object, native int)`) needs no heap object at all:
    // this compiler's only consumer (C64.Interrupt's single subscriber, see
    // C64Lib/C64.cs and asm/C64.asm's C64_add_Interrupt/OnInterrupt) just
    // wants the raw method pointer Ldftn already pushed. #newObj would have
    // sized the allocation from reflecting the delegate's real CLR-internal
    // fields -- meaningless here, and the "_x_ctor" label a plain #newObj
    // would reference is never defined for any delegate type other than the
    // hand-written Func_1_x_ctor (Func<T> itself is excluded below and
    // keeps going through the ordinary #newObj/Func_1_x_ctor/Func_1_Invoke
    // path already proven by Test/FuncTest.cs). Instance-bound delegates (a
    // non-null target) aren't supported -- #stack_construct_static_delegate
    // discards whatever's under the method pointer unconditionally.
    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var method = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)operation.OriginalParameter) as MethodBase;
        var t = method.ReflectedType;
        if (typeof(Delegate).IsAssignableFrom(t) && !t.Name.StartsWith("Func"))
            return "#stack_construct_static_delegate";
        return base.Emit(context, operation);
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var method = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)operation.OriginalParameter) as MethodBase;
        var parameterNum = method.GetParameters().Length;
        operation.StackContent.RemoveLast(parameterNum);

        operation.StackContent.Add(typeof(object));
    }
}

class OpNewArr : OpBase
{
    public OpNewArr() : base(4)
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.RemoveLast(1);
        var type = context.CompilerContext.Assembly.ManifestModule.ResolveType((int)operation.OriginalParameter);
        var dummy = Array.CreateInstance(type, 0);
        operation.StackContent.Add(dummy.GetType());
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var arrayType = operation.StackContent.Last().GetElementType();

        string command;
        if (arrayType == typeof(string))
            command = "#newArr16";
        else if (!arrayType.IsValueType)
            command = "#newArrRef";
        else if (arrayType == typeof(float))
            command = "#newArrflt";
        else if (arrayType.GetStorageBytes() == 2)
            command = "#newArr16";
        else
            command = "#newArr";
        return $"{command}";
    }
}

class OpStfld : OpBase
{
    public OpStfld() : base(4, "#stfld")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.CompilerContext.Assembly.ManifestModule.ResolveField((int)operation.RawParameter);
        var pos = context.CompilerContext.GetFieldPosition(field); return $"{pos}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var last = operation.StackContent.Last();
        var field = context.CompilerContext.Assembly.ManifestModule.ResolveField((int)operation.OriginalParameter);
        last.CheckCompatible(field.FieldType);
        operation.StackContent.RemoveLast(2);
    }
    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return operation.PreviousInstructions[0].StackContent.Last().GetStorageBytes() == 2;
    }

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (operation.PreviousInstructions[0].StackContent.Last() == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}

class OpStElem : OpBase
{
    public OpStElem() : base(0)
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.RawParameter = operation.StackContent.Last(2).GetElementType();
        operation.StackContent.RemoveLast(3);
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var arrayType = (Type)operation.RawParameter;

        string command;
        if (arrayType == typeof(string))
            command = "#stelem16";
        else if (!arrayType.IsValueType)
            command = "#stelemRef";
        else if (arrayType == typeof(float))
            command = "#stelemflt";
        else if (arrayType.GetStorageBytes() == 2)
            command = "#stelem16";
        else
            command = "#stelem";
        return $"{command}";
    }

}

class OpLdElem : OpBase
{
    public OpLdElem() : base(0)
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var elemType = operation.StackContent.Last(1).GetElementType();
        operation.RawParameter = elemType;

        operation.StackContent.RemoveLast(2);
        operation.StackContent.Add(elemType);
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var arrayType = (Type)operation.RawParameter;

        string command;
        if (arrayType == typeof(string))
            command = "#ldelem16";
        else if (!arrayType.IsValueType)
            command = "#ldelemRef";
        else if (arrayType == typeof(float))
            command = "#ldelemflt";
        else if (arrayType.GetStorageBytes() == 2)
            command = "#ldelem16";
        else
            command = "#ldelem";
        return $"{command}";
    }

}

class OpLdLen : OpBase
{
    public OpLdLen() : base(0, "#ldlen")
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var elemType = operation.StackContent.Last().GetElementType();
        operation.RawParameter = elemType;

        operation.StackContent.RemoveLast(1);
        operation.StackContent.Add(typeof(int));
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var arrayType = (Type)operation.RawParameter;

        string command;
        if (arrayType == typeof(float))
            command = "#ldlenflt";
        else if (arrayType.GetStorageBytes() == 2)
            command = "#ldlen16";
        else
            command = "#ldlen";
        return $"{command}";
    }

}

class OpLdfld : OpBase
{
    public OpLdfld() : base(4, "#ldfld")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.CompilerContext.Assembly.ManifestModule.ResolveField((int)operation.RawParameter);
        var pos = context.CompilerContext.GetFieldPosition(field);
        return $"{pos}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.RemoveLast(1);
        var field = context.CompilerContext.Assembly.ManifestModule.ResolveField((int)operation.OriginalParameter);
        operation.StackContent.Add(field.FieldType);
    }

    public override bool Is16BitSupported => true;

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (operation.StackContent.Last() == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}

class OpIncfld : OpBase
{
    private readonly string thisVar;
    private readonly string pos;

    // Unlike Setfld/Pushfld, the "incfld" asm macro (asm/helper/optimized.asm)
    // has no 16-bit variant -- only a single byte-sized "incfld". Is16BitSupported
    // is deliberately false (not true, despite the underlying field access being
    // width-dependent in general): ILFieldIncrementOptimizer only ever builds one
    // of these for a confirmed-8-bit field (see its WithGuard check), so there's
    // no 16-bit case to size for here, and emitting "incfld8"/"incfld16" would
    // just be calling an undefined macro.
    public OpIncfld(string thisVar, string pos) : base(0, "#incfld")
    {
        this.thisVar = thisVar;
        this.pos = pos;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return $"{pos}";
    }
}

class OpSetfld : OpBase
{
    private readonly string pos;
    private readonly bool _is16Bit;
    private readonly string objRelPos;
    private readonly string valueRelPos;

    public OpSetfld(string objRelPos, string valueRelPos, string pos, bool Is16Bit) : base(0, "#setfld")
    {
        this.objRelPos = objRelPos;
        this.valueRelPos = valueRelPos;
        this.pos = pos;
        _is16Bit = Is16Bit;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return $"{objRelPos}, {valueRelPos}, {pos}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.RemoveLast(2);
    }

    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation) => _is16Bit;
}

class OpNewObjInit : OpBase
{
    string[] _mem;
    int _size;
    int _referenceFields;

    string _vtable;

    public OpNewObjInit(string[] mem, int size, int referenceFields, string vtable) : base(0, "#newObjInit")
    {
        _mem = mem;
        _size = size;
        _referenceFields = referenceFields;
        _vtable = vtable;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var memLabel = context.CompilerContext.GetInitValueLabel(string.Join(',', _mem));

        // var ctor = $"{context.TypeContext.Type.Name}_x_ctor";
        var ctor = 0;
        return $"{_size}, {_referenceFields}, {_vtable}, {memLabel}, {ctor}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var method = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)operation.OriginalParameter) as MethodBase;
        var parameterNum = method.GetParameters().Length;
        operation.StackContent.RemoveLast(parameterNum);

        operation.StackContent.Add(typeof(object));
    }

    public override bool Is16BitSupported => false;
}

// Synthesized by ILStaticArrayInitializerPass, replacing Newarr;Dup;
// Ldtoken;Call(RuntimeHelpers.InitializeArray) -- allocates an array
// (like #newArr) and copies its constant contents from a baked .byte data
// label in one step (like #newObjInit, but simpler: arrays of primitives
// have no vtable/reference fields). Leaves the Stsfld that follows it
// untouched, same as ILObjectInitializerOptimizer leaves the final store
// after #newObjInit -- it just consumes the handle this operation pushes.
class OpNewArrInit : OpBase
{
    private readonly int _size;
    private readonly string _initValuesLabel;

    public OpNewArrInit(int size, string initValuesLabel) : base(0, "#newArrInit")
    {
        _size = size;
        _initValuesLabel = initValuesLabel;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return $"{_size}, {_initValuesLabel}";
    }
}

class OpPushFld : OpBase
{
    private readonly string thisVar;
    private readonly string pos;
    private readonly bool is16Bit;

    public OpPushFld(string thisVar, string pos, bool is16Bit) : base(0, "#pushfld")
    {
        this.thisVar = thisVar;
        this.pos = pos;
        this.is16Bit = is16Bit;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return $"{pos}";
    }

    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation) => is16Bit;

}

class OpDup : OpBase
{
    public OpDup() : base(0, "#stack_duplicate")
    {

    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var last = operation.StackContent.Last();
        operation.StackContent.Add(last);
    }

    public override bool Is16BitSupported => true;
}

class OpPop : OpBase
{
    public OpPop() : base(0, "#stack_pop")
    {

    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.RemoveLast(1);
    }
}

class OpPushBase : OpBase
{
    public OpPushBase(int parameterSize) : base(parameterSize, "#stack_push_int")
    {

    }

    public OpPushBase(int parameterSize, string command) : base(parameterSize, command) { }
}

class OpLdConst : OpPushBase
{
    public OpLdConst(int parameterSize) : base(parameterSize)
    {

    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        if (operation.NextInstructions[0].OpCode == ILOpCode.Conv_i8 || operation.NextInstructions[0].OpCode == ILOpCode.Conv_u8)
            operation.StackContent.Add(typeof(long));
        else
            operation.StackContent.Add(typeof(int));
    }

    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return operation.NextInstructions[0].OpCode == ILOpCode.Conv_i8 || operation.NextInstructions[0].OpCode == ILOpCode.Conv_u8;
    }

}

class OpLdc_i4_const : OpLdConst
{
    private int _value;
    public OpLdc_i4_const(int value) : base(0)
    {
        _value = value.ToByte();
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return _value;
    }

}

class OpLdc_i4 : OpLdConst
{
    public OpLdc_i4() : base(4)
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return ((int)(operation.RawParameter));
    }
}

class OpLdc_i4_s : OpLdConst
{
    public OpLdc_i4_s() : base(1)
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return operation.RawParameter;
    }
}

// Ldc_r4 (a float literal). Deliberately not an OpLdConst subclass -- that
// base's SetStackContent always adds int/long, never float, and its
// Is16BitSupported/Is16Bit machinery (8-vs-16 suffix) doesn't apply here at
// all: the command name itself ("#stack_push_mflpt_const", a fixed macro
// with no width variants) already fully encodes what this pushes.
//
// The IL payload for Ldc_r4 is 4 raw bytes, read generically by
// ILMethodCodePass as a little-endian int (unchecked overflow reconstructs
// the exact 32-bit pattern regardless of sign) -- reinterpreting those bits
// as a float and converting to the C64's 5-byte MFLPT format (Mflpt.ToBytes)
// happens once, here, at compile time, not in any hand-written 6502.
class OpLdc_r4 : OpPushBase
{
    public OpLdc_r4() : base(4, "#stack_push_mflpt_const")
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(typeof(float));
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        int bits = (int)operation.RawParameter;
        float value = BitConverter.Int32BitsToSingle(bits);
        var mflpt = Mflpt.ToBytes(value);
        return string.Join(",", mflpt);
    }
}

class OpLdloc : OpPushBase
{
    public int VarIndex;
    public OpLdloc(int varIndex) : base(0, "#locals_push_value")
    {
        VarIndex = varIndex;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {

        int relPos = context.GetLocalVariableReferencePosition(VarIndex);
        return $"{relPos}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(context.GetLocalVariableType(VarIndex));
    }

    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return context.GetLocalVariableType(VarIndex).GetStorageBytes() == 2;
    }

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (context.GetLocalVariableType(VarIndex) == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}

class OpLdloc_s : OpPushBase
{
    public OpLdloc_s() : base(1, "#locals_push_value")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {

        int relPos = context.GetLocalVariableReferencePosition((int)operation.RawParameter);
        return $"{relPos}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(context.GetLocalVariableType((int)operation.OriginalParameter));
    }

    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return context.GetLocalVariableType((int)operation.OriginalParameter).GetStorageBytes() == 2;
    }

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (context.GetLocalVariableType((int)operation.OriginalParameter) == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}

class OpLdarg : OpPushBase
{
    private int _argIndex;
    public OpLdarg(int argIndex) : base(0, "#locals_push_value")
    {
        _argIndex = argIndex;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        int relPos = context.GetParameterReferencePosition(_argIndex);
        return $"{relPos}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(context.GetParameterType((int)_argIndex));
    }


    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return context.GetParameterSize(_argIndex) == 2;
    }

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (context.GetParameterType(_argIndex) == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}

// Ldarg_s -- same as OpLdarg above, but for a method's 5th+ parameter
// (Ldarg_0..Ldarg_3 bake the index into the opcode itself; beyond that,
// Roslyn emits Ldarg_s with the index as a real 1-byte IL operand
// instead). Reads the index from operation.OriginalParameter, not
// RawParameter -- by the time SetStackContent/Is16Bit/SizeSuffix run,
// RawParameter has already been overwritten with ConvertParameter's own
// return value (see ILMethodCodePass.cs), same reason OpNewObj's Emit/
// SetStackContent use OriginalParameter for the same kind of lookup.
class OpLdarg_s : OpPushBase
{
    public OpLdarg_s() : base(1, "#locals_push_value")
    {
    }

    private static int ArgIndex(ILOperation operation) => (int)operation.OriginalParameter;

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        int relPos = context.GetParameterReferencePosition(ArgIndex(operation));
        return $"{relPos}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(context.GetParameterType(ArgIndex(operation)));
    }

    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return context.GetParameterSize(ArgIndex(operation)) == 2;
    }

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (context.GetParameterType(ArgIndex(operation)) == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}

class OpLdsld : OpPushBase
{
    public OpLdsld() : base(4, "#stack_push_var")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.Method.ReflectedType.Module.ResolveField((int)operation.RawParameter);
        return $"{field.DeclaringType.FullName.ToValidName()}_field_{field.Name.ToValidName()}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.Method.ReflectedType.Module.ResolveField((int)operation.OriginalParameter);
        operation.StackContent.Add(field.FieldType);
    }

    // #stack_push_var (unsuffixed) is stack_push_var8 -- correct only for
    // int/uint/bool/reference-typed fields (all 1 byte in this compiler's
    // storage model). A wider field (ulong/long: 2 bytes, float: 5) was
    // silently pushing only its first byte regardless -- invisible for a
    // self-contained "read the field, do something, write it back"
    // sequence (nothing else disagrees about the width), but corrupts the
    // evaluation stack's actual byte count the moment that value is
    // consumed by something that expects the real width (found via a
    // `ulong` field passed to Sprite.X from inside a compiled interrupt
    // handler: the callee pulled one byte more than this pushed, eating
    // into OnInterrupt's own saved state and permanently wedging future
    // interrupt delivery -- see asm/C64.asm).
    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.Method.ReflectedType.Module.ResolveField((int)operation.OriginalParameter);
        var address = $"{field.DeclaringType.FullName.ToValidName()}_field_{field.Name.ToValidName()}";
        if (field.FieldType == typeof(float))
            return $"#stack_push_var_mflpt {address}";
        if (field.FieldType.GetStorageBytes() == 2)
            return $"#stack_push_var16 {address}";
        return base.Emit(context, operation);
    }
}

class OpLdnull : OpPushBase
{
    public OpLdnull() : base(0)
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation) => 0;

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(typeof(object));
    }

}

class OpPullBase : OpBase
{
    public OpPullBase(int parameterSize) : base(parameterSize, "#stack_pull_int")
    {

    }
}

class OpStloc : OpBase
{
    public int VarIndex { get; }
    public OpStloc(int varIndex) : base(0, "#locals_pull_value")
    {
        VarIndex = varIndex;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var body = context.Method.GetMethodBody();
        var variables = body.LocalVariables;
        var index = context.GetLocalVariableReferencePosition(VarIndex);
        var isRef = variables[VarIndex].LocalType.IsReferenceCounted() ? "1" : "0";
        return $"{index}, {isRef}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var last = operation.StackContent.Last();
        last.CheckCompatible(context.GetLocalVariableType(VarIndex));
        operation.StackContent.RemoveLast(1);
    }

    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return context.GetLocalVariableType(VarIndex).GetStorageBytes() == 2;
    }

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (context.GetLocalVariableType(VarIndex) == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}

class OpStsfld : OpBase
{
    public OpStsfld() : base(4, "#stack_pull_int_ref")
    {
    }
    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.Method.ReflectedType.Module.ResolveField((int)operation.RawParameter);
        var address = $"{field.ReflectedType.FullName.ToValidName()}_field_{field.Name.ToValidName()}";
        string isRef = field.FieldType.IsReferenceCounted() ? "1" : "0";
        return $"{address}, {isRef}";
    }

    // #stack_pull_int_ref is 1-byte-wide (plus the reference-counting
    // dance, which only ever applies to 1-byte object handles in this
    // compiler's object model) -- see OpLdsld's identical issue for the
    // full story. A wider field (ulong/long, float) never needs the ref
    // half at all (IsReferenceCounted() is false for both), so it's not
    // just a width suffix here: it's a genuinely different macro, with no
    // ref parameter.
    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.Method.ReflectedType.Module.ResolveField((int)operation.OriginalParameter);
        var address = $"{field.ReflectedType.FullName.ToValidName()}_field_{field.Name.ToValidName()}";
        if (field.FieldType == typeof(float))
            return $"#stack_pull_mflpt {address}";
        if (field.FieldType.GetStorageBytes() == 2)
            return $"#stack_pull_int16 {address}";
        return base.Emit(context, operation);
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.Method.ReflectedType.Module.ResolveField((int)operation.OriginalParameter);
        var last = operation.StackContent.Last();
        last.CheckCompatible(field.FieldType);
        operation.StackContent.RemoveLast(1);
    }

}

class OpStloc_s : OpBase
{
    public OpStloc_s() : base(1, "#locals_pull_value")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var body = context.Method.GetMethodBody();
        var variables = body.LocalVariables;
        var index = context.GetLocalVariableReferencePosition((int)operation.RawParameter);
        var isRef = variables[(int)operation.RawParameter].LocalType.IsReferenceCounted() ? "1" : "0";
        return $"{index}, {isRef}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.RemoveLast(1);
    }

    public override bool Is16BitSupported => true;

    public override bool Is16Bit(CompilerMethodContext context, ILOperation operation)
    {
        return context.GetLocalVariableType((int)operation.OriginalParameter).GetStorageBytes() == 2;
    }

    protected override string SizeSuffix(CompilerMethodContext context, ILOperation operation)
    {
        if (context.GetLocalVariableType((int)operation.OriginalParameter) == typeof(float))
            return "flt";
        return base.SizeSuffix(context, operation);
    }
}


class OpSwitch : OpBase
{
    public OpSwitch() : base(-1)
    {

    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        return $"#switch {context.Method.GetLabel()}_Jump_{operation.Position}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.RemoveLast(1);
    }

    public override void SetNextInstructions(CompilerMethodContext context, ILOperation operation, ILOperation nextOperation)
    {
        List<int> parameters = (List<int>)operation.RawParameter;
        foreach (var parameter in parameters)
        {
            var target = parameter + operation.Position + parameters.Count * 4 + 5;
            var label = $"{context.Method.GetLabel()}_{target}";
            var jmpInstruction = context.Lines.FirstOrDefault(l => l.Label == label);

            operation.NextInstructions.Add(jmpInstruction);
        }

        base.SetNextInstructions(context, operation, nextOperation);
    }
}

class OpRet : OpBase
{
    public OpRet() : base(0, "#method_exit")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        List<string> refList = new List<string>();
        bool isInstance = !context.Method.IsStatic;

        for (int i = 0; i < context.Method.GetParameters().Length; i++)
        {
            if (context.Method.GetParameters()[i].ParameterType.IsReferenceCounted())
                refList.Add(context.GetParameterReferencePosition(i + (isInstance ? 1 : 0)).ToString());
        }

        var body = context.Method.GetMethodBody();
        var variables = body.LocalVariables;
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].LocalType.IsReferenceCounted())
                refList.Add(context.GetLocalVariableReferencePosition(i).ToString());
        }

        return $"{context.GetLocalStackSize()}, [{string.Join(',', refList)}]";
    }

    public override void SetNextInstructions(CompilerMethodContext context, ILOperation operation, ILOperation nextOperation)
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var method = context.Method as MethodInfo;
        if (method != null && method.ReturnType == typeof(void) && operation.StackContent.Count != 0)
            throw new InvalidOperationException($"Stack should be empty. {context.Method.Name}");

        if (method != null && method.ReturnType != typeof(void))
        {
            if (operation.StackContent.Count != 1)
                throw new InvalidOperationException($"Stack should contain 1 element. {context.Method.Name}");

            operation.StackContent[0].CheckCompatible(method.ReturnType);
        }

    }
}

class OpIncVar : OpBase
{
    private int _varIndex;
    public OpIncVar(int varIndex) : base(0, "#inc_var")
    {
        _varIndex = varIndex;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var refPos = context.GetLocalVariableReferencePosition(_varIndex);
        return $"{refPos}";
    }
}

class OpDecVar : OpBase
{
    private int _varIndex;
    public OpDecVar(int varIndex) : base(0, "#dec_var")
    {
        _varIndex = varIndex;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var refPos = context.GetLocalVariableReferencePosition(_varIndex);
        return $"{refPos}";
    }
}

// "x = ++x;" / "x = --x;" (prefix) or "x = x++;" / "x = x--;" (postfix) used
// as an expression -- Roslyn dups the value being stored back, so it's also
// stored to a second, separate local holding the expression's result, as
// opposed to OpIncVar/OpDecVar's plain statement form ("x++;", result
// discarded). Prefix dups the NEW (post-inc/dec) value; postfix dups the
// OLD value *before* incrementing/decrementing -- different IL shapes (see
// the two PeepholeRules in ILMethodIncOptimizer/ILMethodDecOptimizer that
// build this), hence isPostfix controlling the emit order here: postfix
// must capture the old value into the result local *before* inc_var/dec_var
// overwrites it, since that macro mutates in place with nothing pushed.
// Emits three macro calls instead of trying to force this into the usual
// single-Command-plus-parameter shape, since it doesn't fit that model.
class OpIncOrDecVarExpr : OpBase
{
    private readonly string _incOrDecMacro;
    private readonly int _varIndex;
    private readonly int _resultVarIndex;
    private readonly bool _isPostfix;

    public OpIncOrDecVarExpr(string incOrDecMacro, int varIndex, int resultVarIndex, bool isPostfix) : base(0)
    {
        _incOrDecMacro = incOrDecMacro;
        _varIndex = varIndex;
        _resultVarIndex = resultVarIndex;
        _isPostfix = isPostfix;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var refPos = context.GetLocalVariableReferencePosition(_varIndex);
        var resultRefPos = context.GetLocalVariableReferencePosition(_resultVarIndex);
        return _isPostfix
            ? $"#locals_push_value8 {refPos}\n    #locals_pull_value8 {resultRefPos}, 0\n    {_incOrDecMacro} {refPos}"
            : $"{_incOrDecMacro} {refPos}\n    #locals_push_value8 {refPos}\n    #locals_pull_value8 {resultRefPos}, 0";
    }
}

// "arr[x++] = v;" / "Foo(x++)" -- x's postfix result isn't stored to a
// second local at all, it's consumed directly by whatever follows (an array
// index, a method argument, ...), so the dup'd old value is left on the
// (custom) stack rather than pulled into a result local -- see
// OpIncOrDecVarExpr for the "y = x++;" case that does have a result local.
// Just 2 macro calls (push the still-current value, then mutate in place)
// instead of the original 5 (push, dup, push-1, add, pull).
class OpPostIncOrDecVarLeaveOnStack : OpBase
{
    private readonly string _incOrDecMacro;
    private readonly int _varIndex;

    public OpPostIncOrDecVarLeaveOnStack(string incOrDecMacro, int varIndex) : base(0)
    {
        _incOrDecMacro = incOrDecMacro;
        _varIndex = varIndex;
    }

    public override string Emit(CompilerMethodContext context, ILOperation operation)
    {
        var refPos = context.GetLocalVariableReferencePosition(_varIndex);
        return $"#locals_push_value8 {refPos}\n    {_incOrDecMacro} {refPos}";
    }
}

class OpInitVar : OpBase
{
    private int _varIndex;
    private int _value;
    public OpInitVar(int varIndex, int value) : base(0, "#init_var")
    {
        _varIndex = varIndex;
        _value = value;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var refPos = context.GetLocalVariableReferencePosition(_varIndex);
        return $"{refPos}, {_value}";
    }
}

class OpCopyVar : OpBase
{
    private readonly object _srcRelPos;
    private readonly int _dstVarIndex;
    public OpCopyVar(object srcRelPos, int dstVarIndex) : base(0, "#copy_var")
    {
        _srcRelPos = srcRelPos;
        _dstVarIndex = dstVarIndex;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var dstRelPos = context.GetLocalVariableReferencePosition(_dstVarIndex);
        return $"{_srcRelPos}, {dstRelPos}";
    }
}


class OpDeref : OpBase
{
    private int _varIndex;
    public OpDeref(int varIndex) : base(0, "#deref")
    {
        _varIndex = varIndex;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return $"{context.Method.GetLabel()}_var{_varIndex}";
    }
}

class OpDerefParam : OpBase
{
    private string _parameterName;
    public OpDerefParam(string parameterName) : base(0, "#deref")
    {
        _parameterName = parameterName;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return $"{context.Method.GetLabel()}_{_parameterName}";
    }
}

class OpLoadPointerFromLabel : OpBase
{
    public OpLoadPointerFromLabel() : base(4, "#stack_push_pointer")
    {
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(typeof(ulong));
    }
}

// Ldtoken takes a real 4-byte metadata token operand -- ParameterSize
// must be 4 so ILMethodCodePass's raw-byte decoder actually consumes it;
// getting this wrong (e.g. reusing a ParameterSize-0 operand like
// OpLdc_i4_const) desyncs every subsequent opcode read for the rest of
// the method, since the decode loop is a flat byte-offset walk with no
// per-instruction resync. Only real, verified use is via
// ILStaticArrayInitializerPass's rewrite of the Newarr;Dup;Ldtoken;Call
// InitializeArray;Stsfld pattern, which always marks this Optimized
// before ILMethodEmitPass runs -- see CommandMap's registration comment.
class OpLdtoken : OpBase
{
    public OpLdtoken() : base(4, "#stack_push_int")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return 0;
    }

    // Real Ldtoken pushes a RuntimeFieldHandle -- ILMethodBuildEvaluationStackPass
    // (a setup pass, runs before ILStaticArrayInitializerPass's rewrite)
    // simulates the stack over the ORIGINAL, unrewritten IL, so this still
    // needs to push *something* or the immediately-following Call
    // (InitializeArray, 2 real args) pops the stack empty and the Stsfld
    // after it throws indexing a now-empty simulated stack.
    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        operation.StackContent.Add(typeof(System.RuntimeFieldHandle));
    }
}