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

    void ReplaceBreakpoints(IEnumerable<int> addresses);

    SessionStop Continue();
    SessionStop Step();
    SessionStop StepInstruction();
}
