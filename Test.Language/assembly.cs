using C64TestFramework;
using PostSharp.Extensibility;

[assembly: UnitTestProgram("unittest_language")]
[assembly: RunInEmulatorAspect(AttributeTargetTypes = "Compiler.LanguageTest.*", AttributeTargetElements = MulticastTargets.Method)]
