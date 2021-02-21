using C64TestFramework;
using PostSharp.Extensibility;
[assembly: RunInEmulatorAspect(AttributeTargetTypes = "Compiler.Test.*", AttributeTargetElements = MulticastTargets.Method)]
