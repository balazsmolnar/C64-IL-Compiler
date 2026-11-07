using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler;

class ILObjectInitializerOptimizer : ICompilerMethodPass
{
    // #newObj 3, 0 ; Newobj
    // #stack_duplicate8 ; Dup
    // #stack_push_int16 3288 ; Ldc_i4
    // ; conv ; Conv_i8
    // #stfld16 0 ; Stfld
    // #stack_duplicate8 ; Dup
    // #stack_push_int8 10 ; Ldc_i4_s
    // #stfld8 2 ; Stfld
    public void Execute(CompilerMethodContext context)
    {

        if (!context.TypeContext.CompilerContext.Optimize)
            return;

        var lines = context.Lines;
        for (int i = 0; i < lines.Count; i++)
        {
            if (!lines[i].Optimized &&
                lines[i].Operation is OpNewObj &&
                // OpNewObj's delegate special case (see OperandBase.cs)
                // returns a null RawParameter -- a delegate newobj is
                // never followed by Dup+field-stores (no object-initializer
                // syntax for delegates), so it can never match this
                // pattern anyway; skip it explicitly rather than crashing
                // GetIntParam on a null ToString().
                lines[i].RawParameter != null &&
                this.GetStringParam(lines[i].RawParameter, 3).Trim() == "0" &&
                (lines[i + 1].Operation is OpDup || IsConstantInit(context, lines[i])))
            {
                // A constructor of constant field stores (field initializers,
                // ConstructorSupport) gives the starting bytes; the object
                // initializer's stores come after it, as in C#.
                bool constantInit = IsConstantInit(context, lines[i]);
                lines[i].Optimized = true;

                var size = this.GetIntParam(lines[i].RawParameter, 0);
                // Reference fields come first in the layout and start out
                // null (0) here: a constant (ldc/ldstr -- strings are not
                // reference counted) can only ever be stored into a value
                // field, so the folded stores never touch them. A reference
                // field set in the same initializer (to a non-constant)
                // ends the folded run and keeps its own Stfld after
                // #newObjInit. newObjLInit takes the count, like newObjL.
                var referenceFields = this.GetIntParam(lines[i].RawParameter, 1);
                var vtable = this.GetStringParam(lines[i].RawParameter, 2);

                string[] mem = constantInit
                    ? ConstructorSupport.InitialBytes(context.CompilerContext, ResolveCtor(context, lines[i]), size)
                    : Enumerable.Repeat("0", size).ToArray();

                int index = i + 1;
                while (
                    lines[index].Operation is OpDup &&
                    (lines[index + 1].Operation is OpLdConst || lines[index + 1].Operation is OpLdstr) &&
                    (lines[index + 2].Operation is OpStfld ||
                     ((lines[index + 2].OpCode is ILOpCode.Conv_i8 or ILOpCode.Conv_u8) && lines[index + 3].Operation is OpStfld)))
                {
                    // there is a conversion between ldc and setfld -- only
                    // when the line after the ldc is not already the Stfld.
                    // Deciding it from "lines[index + 3] is a Stfld" alone
                    // misread `dup; ldc; stfld A; stfld B` (the last
                    // constant of an object that is itself stored into a
                    // field, `x.B = new T { A = 1 }` or a nested
                    // initializer): it folded the OUTER store B into this
                    // object too, writing the constant at B's offset and
                    // dropping the real store.
                    var conv = !(lines[index + 2].Operation is OpStfld);
                    var opPush = lines[index + 1].Operation as OpLdConst;
                    var opLdstr = lines[index + 1].Operation as OpLdstr;
                    var is16 = false;

                    if (opPush != null)
                    {
                        is16 = opPush.Is16Bit(context, lines[index + 1]);
                    }
                    else if (opLdstr != null)
                    {
                        is16 = true; // ldstr;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid operation.");
                    }
                    var value = GetStringParam(lines[index + 1].RawParameter, 0);
                    var pos = GetIntParam(lines[index + (conv ? 3 : 2)].RawParameter, 0);
                    mem[pos] = $"<{value}";
                    if (is16)
                    {
                        mem[pos + 1] = $">{value}";
                    }
                    lines[index].Optimized = true;
                    lines[index + 1].Optimized = true;
                    lines[index + 2].Optimized = true;
                    if (conv)
                    {
                        lines[index + 3].Optimized = true;
                    }

                    index += conv ? 4 : 3;
                }

                // Nothing constant to fold (every store in the initializer
                // is computed): #newObjInit would only be a bigger #newObj
                // plus an all-zero data row.
                if (index == i + 1 && !constantInit)
                {
                    lines[i].Optimized = false;
                    continue;
                }

                ILOperation newOperation = new ILOperation
                {
                    Operation = new OpNewObjInit(mem, size, referenceFields, vtable),
                };
                newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
                newOperation.StackContent = lines[i].StackContent;
                lines.Insert(index, newOperation);
            }
        }
    }

    private static System.Reflection.MethodBase ResolveCtor(CompilerMethodContext context, ILOperation line) =>
        context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)line.OriginalParameter);

    private static bool IsConstantInit(CompilerMethodContext context, ILOperation line) =>
        ConstructorSupport.Kind(ResolveCtor(context, line)) == ConstructorKind.ConstantInit;

    private int GetIntParam(object parameter, int index)
    {
        return int.Parse(parameter.ToString().Split(',')[index]);
    }

    private string GetStringParam(object parameter, int index)
    {
        return parameter.ToString().Split(',')[index];
    }

}