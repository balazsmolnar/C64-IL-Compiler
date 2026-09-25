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
    SessionStop Step();
    SessionStop StepInstruction();
}
