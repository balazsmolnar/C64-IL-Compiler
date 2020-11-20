using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
namespace Compiler
{
    internal static class CommandMap 
    {
        private static Dictionary<ILOpCode, string> map
            = new Dictionary<ILOpCode, string>
            {
                { ILOpCode.Ldstr, "+stack_push_pointer" },
                { ILOpCode.Call, "jsr"},
                { ILOpCode.Ldc_i4_0, "+stack_push_int 0"},
                { ILOpCode.Ldc_i4_1, "+stack_push_int 1"},
                { ILOpCode.Ldc_i4_2, "+stack_push_int 2"},
                { ILOpCode.Ldc_i4_3, "+stack_push_int 3"},
                { ILOpCode.Ldc_i4_4, "+stack_push_int 4"},
                { ILOpCode.Ldc_i4_m1, "+stack_push_int $FFFF"},
                { ILOpCode.Ldc_i4_s, "+stack_push_int"},
                { ILOpCode.Ldc_i4, "+stack_push_int"},
                { ILOpCode.Stloc_0, "+stack_pull_int"},
                { ILOpCode.Stloc_1, "+stack_pull_int"},
                { ILOpCode.Stloc_2, "+stack_pull_int"},
                { ILOpCode.Stloc_3, "+stack_pull_int"},
                { ILOpCode.Stloc_s, "+stack_pull_int"},
                { ILOpCode.Br_s, "jmp"},
                { ILOpCode.Brtrue_s, "+branch_true"},
                { ILOpCode.Brfalse_s, "+branch_false"},
                { ILOpCode.Nop, "nop"},
                { ILOpCode.Ldloc_0, "+stack_push_var"},
                { ILOpCode.Ldloc_1, "+stack_push_var"},
                { ILOpCode.Ldloc_2, "+stack_push_var"},
                { ILOpCode.Ldloc_3, "+stack_push_var"},
                { ILOpCode.Ldloc_s, "+stack_push_var"},
                { ILOpCode.Ldarg_0, "+stack_push_var"},
                { ILOpCode.Ldarg_1, "+stack_push_var"},
                { ILOpCode.Ldarg_2, "+stack_push_var"},
                { ILOpCode.Add, "+add16"},
                { ILOpCode.Ret, "rts"},
                { ILOpCode.Clt, "+compareLess16"},
                { ILOpCode.Ceq, "+compareEqual16"},
            };
        
        public static bool Supported(ILOpCode code) {
            return map.ContainsKey(code);
        }

        public static string Get(ILOpCode opCode) {
                return map[opCode];
        }
    }
}