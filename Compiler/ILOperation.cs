using System;
using System.Collections.Generic;
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
    public List<ILOperation> NextInstructions { get; } = new List<ILOperation>();
    public List<ILOperation> PreviousInstructions { get; } = new List<ILOperation>();

    public List<Type> StackContent { get; set; } = null;

    public override string ToString()
    {
        return $"{Label?.ToString() ?? "" } {OpCode.ToString()} {RawParameter?.ToString() ?? ""} {StackContent?.Count}";
    }
}