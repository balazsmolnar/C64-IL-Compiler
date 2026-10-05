using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using Compiler.Ops;

namespace Compiler;

class ILOperation
{
    public string Label { get; set; }
    public ILOpCode OpCode { get; set; }
    public object OriginalParameter { get; set; }
    public object RawParameter { get; set; }
    public int Position { get; set; }
    public int Size { get; set; }
    public bool Optimized { get; set; }
    public OpBase Operation { get; set; }

    // Set only by ILMethodInliningPass, on every operation it splices in from
    // an inlined callee's own body -- null (the default, for every line of
    // the method actually being compiled) means "VarIndex/OriginalParameter
    // on this line resolves against context.Method, exactly as before
    // inlining existed." A spliced-in line's local/parameter index is only
    // meaningful against the CALLEE's own locals/parameters, never the
    // caller's -- see ILMethodInliningPass's own comment ("the VarIndex
    // hazard") for why this exists and exactly which call sites consult it.
    public MethodBase SourceMethod { get; set; }
    public List<ILOperation> NextInstructions { get; } = new List<ILOperation>();
    public List<ILOperation> PreviousInstructions { get; } = new List<ILOperation>();

    public List<Type> StackContent { get; set; } = null;

    public override string ToString()
    {
        return $"{Label?.ToString() ?? "" } {OpCode.ToString()} {RawParameter?.ToString() ?? ""} {StackContent?.Count}";
    }
}