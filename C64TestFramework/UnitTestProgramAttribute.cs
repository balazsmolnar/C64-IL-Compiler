using System;

namespace C64TestFramework;

// Assembly-level: names the compiled 6502 program (prg/<name>.prg and
// prg/<name>.labels) that RunInEmulatorAspect runs this assembly's tests
// in. Defaults to "unittest" -- Test/'s program -- when absent. A second
// test project needs its own, since all of an assembly's tests are compiled
// into one program (see Test.Runtime/).
[AttributeUsage(AttributeTargets.Assembly)]
public class UnitTestProgramAttribute : Attribute
{
    public string Name { get; }

    public UnitTestProgramAttribute(string name)
    {
        Name = name;
    }
}
