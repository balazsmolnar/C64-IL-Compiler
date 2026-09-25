using System;

namespace C64TestFramework;

// Runtime faults the compiled program can raise (asm/helper/fault.asm). The
// numbers are the FAULT_* codes there -- keep them in sync.
public enum RuntimeFault
{
    OutOfMemory = 0,
    TooManyObjects = 1,
    Overflow = 2,
    DivisionByZero = 3,
    IllegalQuantity = 4,
    FloatError = 5,
}

// Marks a test that must end in the given runtime fault: the test passes if
// the compiled method faults with exactly that code, and fails if it faults
// with another one or completes normally. Without this attribute a fault
// fails the test, naming the fault. See RunInEmulatorAspect.
[AttributeUsage(AttributeTargets.Method)]
public class ExpectFaultAttribute : Attribute
{
    public RuntimeFault Fault { get; }

    public ExpectFaultAttribute(RuntimeFault fault)
    {
        Fault = fault;
    }
}
