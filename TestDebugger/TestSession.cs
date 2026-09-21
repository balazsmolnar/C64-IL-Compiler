using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using C64TestFramework;
using Compiler;
using SimpleEmulator;

namespace TestDebugger;

class TestCaseSpec
{
    public object[] Arguments = Array.Empty<object>();
    public object ExpectedResult;
    public bool HasExpectedResult;
}

enum SessionStopKind
{
    Breakpoint,
    Step,
    Passed,
    Failed
}

class SessionStop
{
    public SessionStopKind Kind;
    public string SourceFile;
    public int Line;
    public string Message; // set for Failed
    public object ReturnValue; // set for Passed/Failed when decodable
}

// Owns one Emulator instance for one test method run, and drives it via
// Emulator.RunUntil. No call-stack walking -- the method injected by Run()
// is the only frame LocalVariableInspector can see.
class TestSession
{
    private readonly Assembly _testAssembly;
    private readonly DebugMapModel _model;
    private readonly string _prgPath;

    private Emulator _emulator;
    private MethodInfo _method;
    private TestCaseSpec _currentTestCase;
    private HashSet<int> _userBreakpoints = new();
    private bool _halted;

    public MethodInfo CurrentMethod => _method;

    public TestSession(Assembly testAssembly, DebugMapModel model, string prgPath)
    {
        _testAssembly = testAssembly;
        _model = model;
        _prgPath = prgPath;
    }

    public LocalVariableInspector Locals => _method == null ? null : new LocalVariableInspector(_emulator, _model, _method);
    public Emulator Emulator => _emulator;

    public void AddBreakpoint(int address) => _userBreakpoints.Add(address);

    // Every runnable "TestClass.TestMethod[:N]" selector in the loaded test
    // assembly, in declaration order -- used by "run all" (DapServer's
    // run-all mode, and the REPL's `run all` command) to iterate every test
    // one at a time, same shape Resolve() already expects.
    public List<string> DiscoverAllTestSelectors()
    {
        var result = new List<string>();
        foreach (var type in _testAssembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (method.DeclaringType != type)
                    continue;
                var attrs = method.GetCustomAttributesData();
                // [Ignore]/[Explicit] mean the real NUnit run skips this
                // method too (e.g. Uint_Array_Index_Past_255 -- a known,
                // documented limitation, not something "run all" should
                // report as a fresh failure).
                if (attrs.Any(a => a.AttributeType.Name is "IgnoreAttribute" or "ExplicitAttribute"))
                    continue;
                var caseCount = attrs.Count(a => a.AttributeType.Name == "TestCaseAttribute");
                var isPlainTest = attrs.Any(a => a.AttributeType.Name == "TestAttribute");
                if (caseCount == 0 && !isPlainTest)
                    continue;

                if (caseCount == 0)
                    result.Add($"{type.Name}.{method.Name}");
                else
                    for (int i = 0; i < caseCount; i++)
                        result.Add($"{type.Name}.{method.Name}:{i}");
            }
        }
        return result;
    }

    // Replaces the whole breakpoint set at once -- used by DapServer, whose
    // setBreakpoints request semantics are "these are now ALL the
    // breakpoints for this file" (VS Code resends the full set on every
    // gutter-click change, across possibly several files).
    public void ReplaceBreakpoints(IEnumerable<int> addresses) => _userBreakpoints = addresses.ToHashSet();

    public SessionStop Run(string testSelector)
    {
        var (method, testCase) = Resolve(testSelector);
        _method = method;
        _currentTestCase = testCase;
        _halted = false;

        _emulator = new Emulator();
        _emulator.LoadPrg(_prgPath);

        if (!_model.TryResolveMethodAddress(method.GetLabel(), out var methodAddress))
            throw new InvalidOperationException($"No compiled entry point for {method.GetLabel()} (is the method abstract, or not reachable from the debug build?).");

        _emulator.SetMemory(0x09fe, (byte)(methodAddress & 0xFF), (byte)((methodAddress >> 8) & 0xFF));

        var pointer = 0x09e1;
        foreach (var arg in testCase.Arguments)
            EmulatorArgumentMarshaling.WriteArgument(_emulator, ref pointer, arg);
        _emulator.SetMemory(0x09e0, (byte)(pointer - 0x09e1));

        _emulator.SetProgramCounter(0x1000);
        return Advance(_userBreakpoints);
    }

    public SessionStop Continue()
    {
        RequireActiveSession();
        return Advance(_userBreakpoints);
    }

    public SessionStop Step()
    {
        RequireActiveSession();
        return Advance(_model.AllSequencePointAddresses().ToHashSet());
    }

    // Raw single-instruction step (instruction-granularity debugging) --
    // bypasses RunUntil entirely (there's no "run until" happening, just
    // one instruction), unlike Step()/Continue() above.
    public SessionStop StepInstruction()
    {
        RequireActiveSession();
        if (!_emulator.StepOne())
        {
            _halted = true;
            return BuildHaltStop();
        }

        var entry = _model.FindByAddress(_emulator.ProgramCounter);
        return new SessionStop
        {
            Kind = SessionStopKind.Step,
            SourceFile = entry?.SourceFile ?? "?",
            Line = entry?.Line ?? -1, // -1 is expected/common: most raw instructions aren't at a source-line boundary
        };
    }

    private void RequireActiveSession()
    {
        if (_emulator == null)
            throw new InvalidOperationException("No active session -- use `run <TestClass>.<TestMethod>` first.");
        if (_halted)
            throw new InvalidOperationException("Test already finished -- use `run` to start a new session.");
    }

    private (MethodInfo, TestCaseSpec) Resolve(string testSelector)
    {
        int? caseIndex = null;
        var colon = testSelector.LastIndexOf(':');
        var namepart = testSelector;
        if (colon >= 0 && int.TryParse(testSelector.Substring(colon + 1), out var idx))
        {
            caseIndex = idx;
            namepart = testSelector.Substring(0, colon);
        }

        var dot = namepart.LastIndexOf('.');
        if (dot < 0)
            throw new ArgumentException($"Expected TestClass.TestMethod[:N], got \"{testSelector}\"");
        var className = namepart.Substring(0, dot);
        var methodName = namepart.Substring(dot + 1);

        var type = _testAssembly.GetTypes().FirstOrDefault(t => t.FullName == className || t.Name == className);
        if (type == null)
            throw new ArgumentException($"No test class \"{className}\" found.");
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        if (method == null)
            throw new ArgumentException($"No method \"{methodName}\" on {className}.");

        var cases = GetTestCases(method);
        var selectedIndex = caseIndex ?? 0;
        if (selectedIndex < 0 || selectedIndex >= cases.Count)
            throw new ArgumentException($"{className}.{methodName} has {cases.Count} test case(s), index {selectedIndex} is out of range.");

        return (method, cases[selectedIndex]);
    }

    private static List<TestCaseSpec> GetTestCases(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var result = new List<TestCaseSpec>();
        foreach (var attrData in method.GetCustomAttributesData())
        {
            if (attrData.AttributeType.Name != "TestCaseAttribute")
                continue;

            object[] args;
            if (attrData.ConstructorArguments.Count == 1 &&
                attrData.ConstructorArguments[0].ArgumentType == typeof(object[]))
            {
                args = ((IEnumerable<CustomAttributeTypedArgument>)attrData.ConstructorArguments[0].Value)
                    .Select(a => a.Value).ToArray();
            }
            else
            {
                args = attrData.ConstructorArguments.Select(a => a.Value).ToArray();
            }

            // The attribute's raw constructor argument is typed by whatever
            // the literal's own C# form is (e.g. "10000" without an "L"
            // suffix is stored as a boxed int in the attribute metadata,
            // even when the target parameter is "long") -- NUnit itself
            // widens/coerces each value to the actual parameter type when
            // it binds a TestCase to the method at invocation time; do the
            // same here; WriteArgument dispatches on the boxed CLR type, so
            // an un-coerced int handed to a long parameter silently takes
            // the wrong (and narrower) encoding path, or trips its [-127,127]
            // range guard for a value that's perfectly valid as a long.
            for (int i = 0; i < args.Length && i < parameters.Length; i++)
            {
                if (args[i] != null && args[i].GetType() != parameters[i].ParameterType)
                {
                    try { args[i] = Convert.ChangeType(args[i], parameters[i].ParameterType); }
                    catch (InvalidCastException) { /* leave as-is; not a numeric widening case */ }
                }
            }

            var spec = new TestCaseSpec { Arguments = args };
            foreach (var named in attrData.NamedArguments)
            {
                if (named.MemberName == "ExpectedResult")
                {
                    spec.ExpectedResult = named.TypedValue.Value;
                    spec.HasExpectedResult = true;
                }
            }
            result.Add(spec);
        }

        if (result.Count == 0)
            result.Add(new TestCaseSpec());

        return result;
    }

    private SessionStop Advance(HashSet<int> breakpoints)
    {
        var result = _emulator.RunUntil(breakpoints, 50_000_000, out var stoppedAt, out _);

        if (result == RunResult.Halted)
        {
            _halted = true;
            return BuildHaltStop();
        }

        // Breakpoint hit, or step-limit reached (report as a breakpoint-style
        // stop either way -- the caller only distinguishes by not being
        // Passed/Failed).
        var entry = _model.FindByAddress(stoppedAt);
        return new SessionStop
        {
            Kind = SessionStopKind.Breakpoint,
            SourceFile = entry?.SourceFile ?? "?",
            Line = entry?.Line ?? -1,
        };
    }

    private SessionStop BuildHaltStop()
    {
        var resultByte = _emulator.GetMemory(0x20);
        var returnType = (_method.ReturnType == typeof(void)) ? null : _method.ReturnType;
        object returnValue = returnType != null
            ? EmulatorArgumentMarshaling.ReadReturnValue(_emulator, returnType, 0x2c)
            : null;

        if (resultByte != 0)
        {
            return new SessionStop
            {
                Kind = SessionStopKind.Failed,
                Message = RunInEmulatorAspect.GetMessage(_emulator),
                ReturnValue = returnValue,
            };
        }

        // No in-body assertion failed. For an [ExpectedResult]-style test
        // (no Assert calls at all -- NUnit itself compares the return value
        // outside the emulator), replicate that comparison here.
        if (_currentTestCase != null && _currentTestCase.HasExpectedResult)
        {
            var expected = _currentTestCase.ExpectedResult;
            var actual = returnValue;
            var isMatch = Equals(NormalizeForCompare(expected), NormalizeForCompare(actual));
            if (!isMatch)
            {
                return new SessionStop
                {
                    Kind = SessionStopKind.Failed,
                    Message = $"expected {expected}, got {actual}",
                    ReturnValue = returnValue,
                };
            }
        }

        return new SessionStop { Kind = SessionStopKind.Passed, ReturnValue = returnValue };
    }

    // ExpectedResult's compile-time type on the attribute doesn't always
    // exactly match the method's declared return type (e.g. ExpectedResult
    // stored as int while the method returns uint) -- compare on a common
    // numeric ground rather than requiring an exact type match.
    private static object NormalizeForCompare(object value)
    {
        if (value == null)
            return null;
        if (value is bool)
            return value;
        if (value is float || value is double)
            return Convert.ToDouble(value);
        return Convert.ToInt64(value);
    }
}
