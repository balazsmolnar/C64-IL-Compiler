using System;
using System.Reflection;
using Compiler.Ops;
using NUnit.Framework;

namespace Compiler.UnitTests;

// Host-side tests for the two compile-time guards in
// Compiler/Operands/OperandBase.cs -- neither needs the emulator, only the
// reflection metadata of some sample types.
[TestFixture]
public class CompileTimeCheckTests
{
    // --- OpNewObj.ConstructorHasUnsupportedBody -------------------------
    //
    // #newObj never calls an ordinary class's constructor (ctor is
    // hardcoded to "0"), so a constructor doing real work would be silently
    // skipped. Only a constructor that does nothing but forward to a base
    // constructor is safe to skip.

    class NoExplicitCtor
    {
        public float A;
    }

    class EmptyCtor
    {
        public float A;
        public EmptyCtor() { }
    }

    class Base
    {
        public Base(int x) { }
    }

    class ForwardsArgsToBase : Base
    {
        public ForwardsArgsToBase(int x) : base(x) { }
    }

    class AssignsField
    {
        public float A;
        public AssignsField() { A = 1f; }
    }

    class AssignsFieldFromParameter
    {
        public float A;
        public AssignsFieldFromParameter(float a) { A = a; }
    }

    class FieldInitializer
    {
        public float A = 1f;
    }

    class CallsAnotherMethod
    {
        static void Helper() { }
        public CallsAnotherMethod() { Helper(); }
    }

    static bool Unsupported(Type t) =>
        OpNewObj.ConstructorHasUnsupportedBody(t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0]);

    [TestCase(typeof(NoExplicitCtor))]
    [TestCase(typeof(EmptyCtor))]
    [TestCase(typeof(ForwardsArgsToBase))]
    public void Constructor_ThatDoesNothingButCallBase_IsSafeToSkip(Type t)
    {
        Assert.That(Unsupported(t), Is.False);
    }

    [TestCase(typeof(AssignsField))]
    [TestCase(typeof(AssignsFieldFromParameter))]
    [TestCase(typeof(FieldInitializer))]
    [TestCase(typeof(CallsAnotherMethod))]
    public void Constructor_DoingRealWork_IsRejected(Type t)
    {
        Assert.That(Unsupported(t), Is.True);
    }

    // --- OpCall.IsResolvable --------------------------------------------

    static readonly Assembly ThisAssembly = typeof(CompileTimeCheckTests).Assembly;

    static bool Resolvable(MethodBase m) => OpCall.IsResolvable(m, ThisAssembly);

    [Test]
    public void Console_WriteLine_String_IsResolvable()
    {
        Assert.That(Resolvable(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })), Is.True);
    }

    [Test]
    public void Console_Write_String_IsResolvable()
    {
        Assert.That(Resolvable(typeof(Console).GetMethod("Write", new[] { typeof(string) })), Is.True);
    }

    // asm/system.asm only implements the single-string form.
    [TestCase("WriteLine", typeof(int))]
    [TestCase("Write", typeof(int))]
    [TestCase("WriteLine", typeof(char))]
    [TestCase("WriteLine", typeof(object))]
    public void Console_NonStringOverloads_AreRejected(string name, Type parameter)
    {
        Assert.That(Resolvable(typeof(Console).GetMethod(name, new[] { parameter })), Is.False);
    }

    [Test]
    public void Console_WriteLine_WithNoArguments_IsRejected()
    {
        Assert.That(Resolvable(typeof(Console).GetMethod("WriteLine", Type.EmptyTypes)), Is.False);
    }

    [Test]
    public void Console_WriteLine_WithFormatArguments_IsRejected()
    {
        Assert.That(Resolvable(typeof(Console).GetMethod("WriteLine", new[] { typeof(string), typeof(object) })), Is.False);
    }

    [Test]
    public void Console_OtherMembers_AreRejected()
    {
        Assert.That(Resolvable(typeof(Console).GetMethod("ReadLine")), Is.False);
    }

    [Test]
    public void Method_DefinedInCompiledAssembly_IsResolvable()
    {
        Assert.That(Resolvable(typeof(CompileTimeCheckTests).GetMethod(nameof(Method_DefinedInCompiledAssembly_IsResolvable))), Is.True);
    }

    [Test]
    public void GC_Collect_IsResolvable()
    {
        Assert.That(Resolvable(typeof(GC).GetMethod("Collect", Type.EmptyTypes)), Is.True);
    }

    [TestCase("Sin")]
    [TestCase("Cos")]
    [TestCase("Sqrt")]
    public void MathF_SupportedMethods_AreResolvable(string name)
    {
        Assert.That(Resolvable(typeof(MathF).GetMethod(name, new[] { typeof(float) })), Is.True);
    }

    [Test]
    public void MathF_UnsupportedMethod_IsRejected()
    {
        Assert.That(Resolvable(typeof(MathF).GetMethod("Tan", new[] { typeof(float) })), Is.False);
    }

    [Test]
    public void ArbitraryBclMethod_IsRejected()
    {
        Assert.That(Resolvable(typeof(System.IO.Path).GetMethod("Combine", new[] { typeof(string), typeof(string) })), Is.False);
    }
}
