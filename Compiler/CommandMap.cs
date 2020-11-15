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
                { ILOpCode.Ldc_i4_s, "+stack_push_int"},
                { ILOpCode.Stloc_0, "+stack_pull_int .var0"},
                { ILOpCode.Stloc_1, "+stack_pull_int .var1"},
                { ILOpCode.Br_s, "jmp"},
                { ILOpCode.Brtrue_s, "beq"},
                { ILOpCode.Nop, "nop"},
                { ILOpCode.Ldloc_0, "+stack_push_var .var0"},
                { ILOpCode.Ldloc_1, "+stack_push_var .var1"},
                { ILOpCode.Add, "+add16"},
                { ILOpCode.Ret, "rts"},
                { ILOpCode.Clt, "+compareLess16"},
            };
        
        public static bool Supported(ILOpCode code) {
            return map.ContainsKey(code);
        }

        public static string Get(ILOpCode opCode) {
                return map[opCode];
        }
    }
}