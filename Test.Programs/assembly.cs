using C64TestFramework;
using PostSharp.Extensibility;

[assembly: UnitTestProgram("unittest_programs")]
[assembly: RunInEmulatorAspect(AttributeTargetTypes = "Compiler.ProgramTest.*", AttributeTargetElements = MulticastTargets.Method)]

