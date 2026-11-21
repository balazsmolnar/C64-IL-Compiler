using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Compiler.Ops;

namespace Compiler;

// Table-driven array-of-objects initializers. Runs right after
// ILObjectInitializerOptimizer, which has already turned every
// `new T { A = 1, B = 2 }` with constant fields into one #newObjInit. An
// array literal of such objects then compiles to, per element:
//
//   #stack_duplicate8          ; Dup         (the array)
//   #stack_push_int8 i         ; Ldc         (the index)
//   #newObjInit size, refs, vtable, Init_Values_n, 0
//   #stelemRef                 ; Stelem_ref
//
// about 42 bytes of code per element (Hunchback's title tune has 64 of
// them). A run of two or more such elements with consecutive indices and the
// same type becomes one #newObjArrayInit whose descriptor holds the object
// layout, the count, the first index and every element's initial bytes back
// to back; asm/helper/object.asm's NewObjArrayInitL allocates each object
// with newObjLInit (same heap/object-table protocol as #newObjInit) and
// stores its handle into the array exactly like #stelemRef (a plain byte
// store: references held by heap objects are traced by the GC, not counted).
// The array stays on the stack, as it did after the last Stelem_ref.
//
// A plain `new T()` element (no initializer, so still a #newObj) joins a run
// too, as all-zero bytes; so does a static method group stored as a Func<T>
// (TryParseStaticFunc: about 55 bytes of code each before, a slide list of
// method groups being the typical case).
//
// Elements that don't fit (a non-constant field, a nested `new`, a different
// type, a gap in the indices, a branch target inside) end the run and keep
// their ordinary code, so a mixed initializer gets one table per maximal run.
// A run of one is left alone: the routine (about 60 bytes, linked only when
// some table is emitted) would not pay for itself.
class ILObjectArrayInitializerOptimizer : ICompilerMethodPass
{
    private const int MinimumRun = 2;

    private sealed class Element
    {
        public int Dup;
        public int Index;
        public int InitLine;
        public int Store;
        public int Value;
        public int Size;
        public int ReferenceFields;
        public string VTable;
        public IReadOnlyList<string> Mem;
    }

    public void Execute(CompilerMethodContext context)
    {
        if (!context.TypeContext.CompilerContext.Optimize)
            return;

        var lines = context.Lines;
        for (int i = 0; i < lines.Count; i++)
        {
            var first = TryParse(lines, i);
            if (first == null)
                continue;

            var run = new List<Element> { first };
            while (run.Count < 255)
            {
                var last = run[run.Count - 1];
                var next = TryParse(lines, last.Store + 1);
                if (next == null ||
                    next.Value != last.Value + 1 ||
                    lines[next.Dup].Label != null ||
                    !SameLayout(next, first))
                    break;
                run.Add(next);
            }

            if (run.Count < MinimumRun || first.Value + run.Count > 255)
            {
                i = run[run.Count - 1].Store;
                continue;
            }

            var descriptor = new List<string>
            {
                first.Size.ToString(CultureInfo.InvariantCulture),
                first.ReferenceFields.ToString(CultureInfo.InvariantCulture),
                run.Count.ToString(CultureInfo.InvariantCulture),
                first.Value.ToString(CultureInfo.InvariantCulture),
                $"<{first.VTable}",
                $">{first.VTable}",
            };
            foreach (var element in run)
                descriptor.AddRange(element.Mem);

            var start = first.Dup;
            var end = run[run.Count - 1].Store;
            for (int k = start; k <= end; k++)
                lines[k].Optimized = true;

            var newOperation = new ILOperation
            {
                Operation = new OpNewObjArrayInit(descriptor.ToArray()),
                Position = lines[start].Position,
                StackContent = lines[end].StackContent,
            };
            lines.Insert(end + 1, newOperation);
            i = end + 1;
        }
    }

    private static bool SameLayout(Element a, Element b) =>
        a.Size == b.Size && a.ReferenceFields == b.ReferenceFields && a.VTable == b.VTable;

    // One element at lines[p]: live Dup, live constant index, any number of
    // already-Optimized lines (the object initializer's own field stores),
    // the live #newObjInit, live Stelem_ref. Only the Dup may be a branch
    // target (the caller additionally rejects that for every element after
    // the first, since the folded run is emitted after its last element).
    private static Element TryParse(List<ILOperation> lines, int p)
    {
        if (p + 3 >= lines.Count)
            return null;
        if (lines[p].Optimized || !(lines[p].Operation is OpDup))
            return null;

        var index = p + 1;
        if (lines[index].Optimized || lines[index].Label != null || !(lines[index].Operation is OpLdConst))
            return null;
        if (lines[index].RawParameter == null ||
            !int.TryParse(lines[index].RawParameter.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ||
            value < 0 || value > 254)
            return null;
        if (lines[index].NextInstructions.Count == 1 &&
            lines[index].NextInstructions[0].OpCode is System.Reflection.Metadata.ILOpCode.Conv_i8 or System.Reflection.Metadata.ILOpCode.Conv_u8)
            return null;

        if (TryParseStaticFunc(lines, index) is { } func)
        {
            func.Dup = p;
            func.Index = index;
            func.Value = value;
            return func;
        }

        var k = index + 1;
        while (k < lines.Count && lines[k].Optimized)
        {
            if (lines[k].Label != null)
                return null;
            k++;
        }
        if (k + 1 >= lines.Count || lines[k].Optimized || lines[k].Label != null)
            return null;

        Element element;
        if (lines[k].Operation is OpNewObjInit init)
        {
            element = new Element { Size = init.Size, ReferenceFields = init.ReferenceFields, VTable = init.VTable.Trim(), Mem = init.Mem };
        }
        else if (lines[k].Operation is OpNewObj && k == index + 1 && lines[k].RawParameter != null)
        {
            // A plain `new T()` (no initializer, no constructor call to make):
            // the same allocation with all-zero initial bytes.
            var parts = lines[k].RawParameter.ToString().Split(',').Select(x => x.Trim()).ToArray();
            if (parts.Length != 4 || parts[3] != "0" ||
                ConstructorSupport.Kind(MethodBaseExtension.ProgramAssembly.ManifestModule.ResolveMethod((int)lines[k].OriginalParameter)) != ConstructorKind.Trivial ||
                !int.TryParse(parts[0], out var size) || !int.TryParse(parts[1], out var refs))
                return null;
            element = new Element { Size = size, ReferenceFields = refs, VTable = parts[2], Mem = Enumerable.Repeat("0", size).ToArray() };
        }
        else
            return null;
        // newObjLInit copies until its byte counter wraps back to the size:
        // a 0-byte object would copy 256.
        if (element.Size == 0)
            return null;

        var store = k + 1;
        if (lines[store].Optimized || lines[store].Label != null ||
            !(lines[store].Operation is OpStElem) ||
            !(lines[store].RawParameter is Type elementType) ||
            elementType.IsValueType || elementType == typeof(string))
            return null;

        element.Dup = p;
        element.Index = index;
        element.InitLine = k;
        element.Store = store;
        element.Value = value;
        return element;
    }

    // A static method group stored as a Func<T> (`Func<Slide>[] s = [A.Create,
    // B.Create]`): `ldnull; ldftn M; newobj Func<T>::.ctor; stelem.ref` after
    // the index. Func_1_x_ctor (asm/system.asm) only stores its two arguments,
    // the target at byte 0 (none: 0) and the method address at bytes 1-2, so
    // the object is constant and joins a run like any `new T { .. }`: the same
    // size, reference count and vtable as the #newObj it replaces.
    private static Element TryParseStaticFunc(List<ILOperation> lines, int index)
    {
        int target = index + 1, method = index + 2, ctor = index + 3, store = index + 4;
        if (store >= lines.Count)
            return null;
        for (int k = target; k <= store; k++)
            if (lines[k].Optimized || lines[k].Label != null)
                return null;
        if (!(lines[target].Operation is OpLdnull) ||
            !(lines[method].Operation is OpLdftn) || lines[method].RawParameter is not string label ||
            !(lines[ctor].Operation is OpNewObj) || lines[ctor].RawParameter == null ||
            !(lines[store].Operation is OpStElem) || !(lines[store].RawParameter is Type elementType) || elementType.IsValueType)
            return null;
        var constructor = MethodBaseExtension.ProgramAssembly.ManifestModule.ResolveMethod((int)lines[ctor].OriginalParameter);
        var type = constructor.DeclaringType;
        if (type == null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Func<>))
            return null;
        var parts = lines[ctor].RawParameter.ToString().Split(',').Select(x => x.Trim()).ToArray();
        if (parts.Length != 4 || !int.TryParse(parts[0], out var size) || !int.TryParse(parts[1], out var refs) || size < 3)
            return null;
        var mem = new string[size];
        Array.Fill(mem, "0");
        mem[1] = $"<{label}";
        mem[2] = $">{label}";
        return new Element { Size = size, ReferenceFields = refs, VTable = parts[2], Mem = mem, InitLine = ctor, Store = store };
    }
}
