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
    // --- ConstructorSupport.Kind -----------------------------------------
    //
    // A constructor that only forwards to an empty base constructor is
    // never called; one that only stores constants becomes the object's
    // initial bytes; anything else is compiled and called by newobj.

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

    static ConstructorKind Kind(Type t)
    {
        // Only the program's own constructors are analysed.
        var saved = MethodBaseExtension.ProgramAssembly;
        MethodBaseExtension.ProgramAssembly = t.Assembly;
        try
        {
            return ConstructorSupport.Kind(t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0]);
        }
        finally
        {
            MethodBaseExtension.ProgramAssembly = saved;
        }
    }

    [TestCase(typeof(NoExplicitCtor))]
    [TestCase(typeof(EmptyCtor))]
    public void Constructor_ThatDoesNothingButCallBase_IsTrivial(Type t)
    {
        Assert.That(Kind(t), Is.EqualTo(ConstructorKind.Trivial));
    }

    [TestCase(typeof(AssignsField))]
    [TestCase(typeof(FieldInitializer))]
    public void Constructor_StoringConstants_IsConstantInit(Type t)
    {
        Assert.That(Kind(t), Is.EqualTo(ConstructorKind.ConstantInit));
    }

    [TestCase(typeof(ForwardsArgsToBase))]
    [TestCase(typeof(AssignsFieldFromParameter))]
    [TestCase(typeof(CallsAnotherMethod))]
    public void Constructor_DoingRealWork_IsCalled(Type t)
    {
        Assert.That(Kind(t), Is.EqualTo(ConstructorKind.Call));
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
