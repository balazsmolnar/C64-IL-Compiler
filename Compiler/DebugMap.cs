namespace Compiler;

// Accumulated by ILMethodDebugLabelPass (only when CompilerContext.EmitDebugInfo
// is set), written out once by ILDebugMapPass. See debugmap.txt's format
// comment on ILDebugMapPass for how these are serialized.
record DebugSequencePoint(string MethodLabel, string Label, string SourceFile, int Line);
record DebugLocal(string MethodLabel, int Index, string Name);
