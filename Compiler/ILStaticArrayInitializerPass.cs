using System;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

// Recognizes Roslyn's codegen for a `<primitive>[] = { lit, lit, ... }`
// array literal, wherever it's assigned -- a static field (Stsfld), an
// instance field (Stfld), or a local (Stloc/Stloc_s):
//   ldc.i4 <length>; newarr; dup; ldtoken <PrivateImplementationDetails blob field>;
//   call RuntimeHelpers.InitializeArray(Array, RuntimeFieldHandle);
//   st(s)fld|stloc(_s) <target>
//
// Originally Stsfld-only, reading the target field's value back via
// reflection (FieldInfo.GetValue(null) on a static field, after letting
// the CLR run the type's real static initializer in the compiler's own
// process) to get the populated array with no blob-parsing needed. That
// trick fundamentally can't generalize to Stfld/Stloc: there's no already-
// materialized instance to read an instance field or local back from at
// compile time (the compiler only ever reflects over static metadata, it
// never actually runs the target program). Generalized instead by not
// depending on the target at all: the blob field Ldtoken references is
// ALWAYS static regardless of what the array itself gets assigned to, so
// calling System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray
// directly -- the exact same BCL method Roslyn's own generated call
// invokes -- against a throwaway array built from Newarr's own element
// type and the constant length pushed just before it, populates that
// array with the real literal data with no dependency on the target field
// at all. Subsumes the original Stsfld case too (same mechanism now
// handles all three target kinds uniformly), so the old field-read path is
// gone entirely rather than kept as a special case.
//
// Collapses the whole 4-op window (Newarr..Call) into one #newArrInit call
// referencing a baked .byte data label -- same technique
// ILObjectInitializerOptimizer already uses for object-initializer chains
// via #newObjInit. The store that follows is deliberately left untouched
// (same as that pass leaves its final store alone): #newArrInit pushes the
// new array's handle exactly like Newarr did, so the existing, unmodified
// Stsfld/Stfld/Stloc codegen just consumes it normally.
class ILStaticArrayInitializerPass : ICompilerMethodPass
{
    public void Execute(CompilerMethodContext context)
    {
        if (!context.CompilerContext.Optimize)
            return;

        var lines = context.Lines;
        for (int i = 1; i + 4 < lines.Count; i++)
        {
            if (lines[i].Optimized ||
                !(lines[i].Operation is OpNewArr) ||
                !(lines[i + 1].Operation is OpDup) ||
                lines[i + 2].OpCode != ILOpCode.Ldtoken ||
                !(lines[i + 3].Operation is OpCall) ||
                !(lines[i - 1].Operation is OpLdConst) ||
                !(lines[i + 4].Operation is OpStsfld or OpStfld or OpStloc or OpStloc_s))
                continue;

            var callTarget = context.CompilerContext.Assembly.ManifestModule.ResolveMethod((int)lines[i + 3].OriginalParameter);
            if (callTarget.DeclaringType != typeof(System.Runtime.CompilerServices.RuntimeHelpers) ||
                callTarget.Name != "InitializeArray")
                continue;

            var elementType = context.CompilerContext.Assembly.ManifestModule.ResolveType((int)lines[i].OriginalParameter);
            var length = (int)lines[i - 1].RawParameter;
            var blobField = context.Method.ReflectedType.Module.ResolveField((int)lines[i + 2].OriginalParameter);

            var array = Array.CreateInstance(elementType, length);
            System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray(array, blobField.FieldHandle);

            // #newArrInit's "size" is a raw BYTE count copied straight into
            // the new heap object (heap.asm's newObjLInit does a literal
            // byte-for-byte copy, no per-element width awareness at all) --
            // for a multi-byte element type (long/ulong, 2 bytes each in
            // this compiler; TypeExtensions.GetStorageBytes) that's
            // array.Length * elementBytes, not array.Length itself, and
            // each element needs to contribute that many LOW-byte-first
            // bytes to the blob (matching heap.asm's ldelem16/stelem16,
            // which read/write low then high at byte offset index*2) --
            // not one decimal value per element the way the 1-byte case
            // works. Missing this originally corrupted every 2-byte-element
            // array literal silently: e.g. Hunchback's ulong[]
            // ScoreMultipliers table only had half its real byte count
            // copied in, so Ldelem16 read adjacent PAIRS of small values as
            // one garbled 16-bit multiplier (a completion bonus in the tens
            // of thousands instead of 10-80) -- 1-byte element types
            // (int[]/uint[], TickThresholds alongside it in that same
            // table) were never affected, which is why this went unnoticed
            // until a 2-byte array literal was actually exercised.
            var elementBytes = elementType.GetStorageBytes();
            var mem = new string[array.Length * elementBytes];
            for (int k = 0; k < array.Length; k++)
            {
                var value = Convert.ToInt64(array.GetValue(k));
                for (int b = 0; b < elementBytes; b++)
                    mem[k * elementBytes + b] = ((value >> (8 * b)) & 0xFF).ToString();
            }

            var memLabel = context.CompilerContext.GetInitValueLabel(string.Join(',', mem));

            var newOperation = new ILOperation
            {
                Operation = new OpNewArrInit(array.Length * elementBytes, memLabel),
            };
            newOperation.RawParameter = newOperation.Operation.ConvertParameter(context, null);
            newOperation.StackContent = lines[i].StackContent;

            lines.Insert(i + 4, newOperation);
            // lines[i-1] (the length push) is dead once #newArrInit exists --
            // it takes size as a compile-time macro argument (see
            // OpNewArrInit.ConvertParameter/heap.asm's newArrInit), never
            // pulling it off the runtime stack the way the real Newarr it
            // replaces would have. Leaving it un-Optimized (as this pass
            // always did, pre-dating the Stfld/Stloc support above) left a
            // stray value sitting on the hardware stack underneath the new
            // array handle -- harmless for Stsfld/Stloc's target-store
            // macros (#stack_pull_int_ref/#locals_pull_value8 only ever pull
            // ONE value, so the leftover just sits there unread), but
            // corrupts Stfld's #stfld8/#stfld16 (which pulls TWO: the field
            // value, then separately the "this" object reference) -- the
            // second pull silently got the stray length constant instead of
            // the real object reference. Found via
            // Test/ArrayTest.cs's Instance_Field_Array_Literal.
            lines[i - 1].Optimized = true;
            lines[i].Optimized = true;
            lines[i + 1].Optimized = true;
            lines[i + 2].Optimized = true;
            lines[i + 3].Optimized = true;
        }
    }
}
