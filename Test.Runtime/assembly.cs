using C64TestFramework;
using PostSharp.Extensibility;

[assembly: UnitTestProgram("unittest_runtime")]
[assembly: RunInEmulatorAspect(AttributeTargetTypes = "Compiler.RuntimeTest.*", AttributeTargetElements = MulticastTargets.Method)]
