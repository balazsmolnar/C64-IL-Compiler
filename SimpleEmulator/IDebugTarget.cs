using System.Collections.Generic;

namespace SimpleEmulator
{
    // Read-only view of a machine's memory -- all the value/object
    // inspection code needs. Implemented by Emulator and, for real-VICE
    // debugging, by a remote-monitor client.
    public interface IMemoryReader
    {
        byte GetMemory(int address);
    }

    // What a debugger needs from something that runs 6502 code: read memory
    // and registers, run until one of a set of addresses is reached, and
    // single-step. Emulator implements it directly (its members already
    // have these exact signatures).
    public interface IDebugTarget : IMemoryReader
    {
        int ProgramCounter { get; }
        byte HardwareStackPointer { get; }
        EmulatorRegisters Registers { get; }

        RunResult RunUntil(HashSet<int> breakpointAddresses, long maxSteps, out int stoppedAtAddress, out long stepsExecuted);

        // Executes exactly one instruction; false if the machine halted.
        bool StepOne();

        // Pokes raw bytes directly into memory -- an uncontrolled, no-undo
        // write, unlike running real compiled code. Used by the debugger's
        // own "set"/setVariable (edit-a-value-in-the-Watch-panel) feature;
        // nothing else needs a target to ever write to itself.
        void SetMemory(int address, params byte[] value);
    }
}
