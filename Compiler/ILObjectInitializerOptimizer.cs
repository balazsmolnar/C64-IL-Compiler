using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;
using Compiler.Ops;

namespace Compiler
{
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
                if (lines[i].Operation is OpNewObj &&
                    this.GetIntParam(lines[i].RawParameter, 1) == 0 &&
                    lines[i + 1].Operation is OpDup)
                {
                    lines[i].Optimized = true;

                    var size = this.GetIntParam(lines[i].RawParameter, 0);
                    var vtable = this.GetStringParam(lines[i].RawParameter, 2);

                    byte[] mem = new byte[size];

                    int index = i + 1;
                    while (
                        lines[index].Operation is OpDup &&
                        lines[index + 1].Operation is OpLdConst &&
                        (lines[index + 2].Operation is OpStfld || lines[index + 3].Operation is OpStfld))
                    {
                        // there is a conversion between ldc and setfld

                        var conv = lines[index + 3].Operation is OpStfld;
                        var opPush = lines[index + 1].Operation as OpLdConst;
                        var is16 = opPush.Is16Bit(context, lines[index + 1]);
                        var value = GetIntParam(lines[index + 1].RawParameter, 0);
                        var pos = GetIntParam(lines[index + (conv ? 3 : 2)].RawParameter, 0);
                        mem[pos] = (byte)(value % 256);
                        if (is16)
                        {
                            mem[pos + 1] = (byte)(value / 256);
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

                    ILOperation newOperation = new ILOperation
                    {
                        Operation = new OpNewObjInit(mem, size, 0, vtable),
                    };
                    newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
                    newOperation.StackContent = lines[i].StackContent;
                    lines.Insert(index, newOperation);
                }
            }
        }

        private int GetIntParam(object parameter, int index)
        {
            return int.Parse(parameter.ToString().Split(',')[index]);
        }

        private string GetStringParam(object parameter, int index)
        {
            return parameter.ToString().Split(',')[index];
        }

    }
}
