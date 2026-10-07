using System.Collections.Generic;
using System.Reflection;
using SimpleEmulator;

namespace TestDebugger;

// What DapServer needs from a running debug session, whether it's one unit
// test in SimpleEmulator (TestSession) or a whole program on VICE
// (ProgramSession).
interface IDebugSession
{
    IDebugTarget Target { get; }

    // The method the target is currently stopped in, or null if that can't
    // be determined (e.g. inside a ROM routine).
    MethodBase CurrentMethod { get; }

    // Inspector for the current method's locals; null when there is none.
    LocalVariableInspector Locals { get; }

    // The address the source location shown to the user comes from. Normally
    // the PC; after a runtime fault it's where the faulting code was called
    // from, since the PC is inside the fault routine.
    int SourcePc { get; }

    void ReplaceBreakpoints(IEnumerable<int> addresses);

    SessionStop Continue();

    // Steps to the next source line ANYWHERE, including descending into a
    // called method -- DAP "stepIn" (F11)'s own semantics.
    SessionStop Step();

    // Steps to the next source line WITHOUT descending into a called
    // method -- DAP "next" (F10)'s own semantics. A call made from the
    // stepped-over line still runs to completion; only its own internal
    // stops are skipped.
    SessionStop StepOver();

    // Runs until the current method returns to its caller -- DAP
    // "stepOut" (Shift+F11)'s own semantics.
    SessionStop StepOut();

    SessionStop StepInstruction();
}
