// TestDebugger reuses CompilerMethodContext.GetLocalVariableReferencePosition/
// GetLocalVariableType (internal) instead of duplicating the local-variable
// address formula.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TestDebugger")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Compiler.UnitTests")]
