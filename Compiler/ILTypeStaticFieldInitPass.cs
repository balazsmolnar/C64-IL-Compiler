using System.Linq;
using System.Reflection;

namespace Compiler;

class ILTypeStaticFieldInitPass : ICompilerTypePass
{
    public void Execute(CompilerTypeContext context)
    {
        var fields = context.Type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
        foreach (var field in fields.Where(f => !f.IsLiteral).OrderBy(f => f.FieldType.IsReferenceCounted() ? 0 : 1))
        {
            // FullName, not Name -- must match OpLdsfld/OpStsfld's own
            // label computation (Operands/OperandBase.cs) and ILCodePass's
            // per-type file naming exactly; see ILCodePass.cs's comment for
            // why short names alone collide (Roslyn's synthesized "<>O").
            //
            // One ".byte 0" per storage byte (GetStorageBytes() -- 5 for
            // float's MFLPT representation, 2 for long/ulong, 1 otherwise),
            // NOT a single ".byte 0" regardless of width: OpLdsld/OpStsfld
            // already read/write the field's full width (#stack_push_var_
            // mflpt/#stack_pull_mflpt for float, #stack_push_var16/
            // #stack_pull_int16 for long/ulong), so under-reserving here
            // let a multi-byte field's write spill into whatever field was
            // declared right after it -- and that field's own later write
            // would then spill back over this field's trailing bytes,
            // corrupting it. Found via Demo/Program.cs's rotating-cube
            // scene (a static float field written a second time read back
            // corrupted) and reproduced directly in
            // Test/StaticFieldLayoutTest.cs. Repeating ".byte 0" (not
            // ".fill N", which doesn't guarantee the assembled .prg
            // actually contains real zero bytes there -- see
            // Compiler/Templates/ProgramEntry.asm's own comment on that)
            // keeps the same "real zero bytes in the file" guarantee the
            // original single-byte case had, just for every byte.
            var byteList = string.Join(",", Enumerable.Repeat("0", field.FieldType.GetStorageBytes()));
            string outputLine = $"{context.Type.FullName.ToValidName()}_field_{field.Name.ToValidName()} .byte {byteList}";
            context.OutputFile.WriteLine(outputLine);
        }
    }
}