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
        var command = Command;
        if (Is16BitSupported)
        {
            if (Is16Bit(context, operation))
                command += "16";
            else
                command += "8";
        }
        if (operation.RawParameter == null)
            return command;
        return $"{command} {operation.RawParameter}";
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

        if (normalCall)
            return base.Emit(context, operation);

        var index = methodInfo.ReflectedType.GetVirtualMethodIndex(methodInfo);
        var stackPosition = methodInfo.GetParameters().Select(x => x.ParameterType.GetStorageBytes()).Sum() + 1;
        if (index == -1)
            throw new InvalidOperationException($"Virtual method not found: {methodInfo.Name}. Type: {methodInfo.ReflectedType.Name}");
        return $"#callVirt {index}, {stackPosition}";
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
        var size = 0;
        var referenceFields = 0;
        foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            size += f.FieldType.GetStorageBytes();
            if (f.FieldType.IsReferenceCounted())
                referenceFields++;
        }
        //var label = context.CompilerContext.Assembly.ManifestModule.ResolveMethod(operation.RawParameter).GetLabel();


        var vtable = t.IsGenericType ? "0" : $"{t.Name.ToValidName()}_VTable";
        // var ctor = $"{t.Name}_x_ctor";
        var ctor = "0";

        if (t.Assembly == typeof(Func<object>).Assembly)
            ctor = $"{t.Name}_x_ctor".ToValidName();
        return $"{size}, {referenceFields}, {vtable}, {ctor}";
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
        if (arrayType.GetStorageBytes() == 2)
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
}

class OpIncfld : OpBase
{
    private readonly string thisVar;
    private readonly string pos;

    public OpIncfld(string thisVar, string pos) : base(0, "#incfld")
    {
        this.thisVar = thisVar;
        this.pos = pos;
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        return $"{pos}";
    }
    public override bool Is16BitSupported => true;

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

}

class OpLdsld : OpPushBase
{
    public OpLdsld() : base(4, "#stack_push_var")
    {
    }

    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.Method.ReflectedType.Module.ResolveField((int)operation.RawParameter);
        return $"{field.DeclaringType.Name.ToValidName()}_field_{field.Name.ToValidName()}";
    }

    public override void SetStackContent(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.Method.ReflectedType.Module.ResolveField((int)operation.OriginalParameter);
        operation.StackContent.Add(field.FieldType);
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
}

class OpStsfld : OpBase
{
    public OpStsfld() : base(4, "#stack_pull_int_ref")
    {
    }
    public override object ConvertParameter(CompilerMethodContext context, ILOperation operation)
    {
        var field = context.Method.ReflectedType.Module.ResolveField((int)operation.RawParameter);
        var address = $"{field.ReflectedType.Name.ToValidName()}_field_{field.Name.ToValidName()}";
        string isRef = field.FieldType.IsReferenceCounted() ? "1" : "0";
        return $"{address}, {isRef}";
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