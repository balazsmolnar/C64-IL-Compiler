using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Serialization;
using SimpleEmulator;

namespace C64TestFramework;

[PSerializable]
public class RunInEmulatorAspect : MethodInterceptionAspect
{
    private const int RETURN_VALUE_ADDRESS = 0x2c;
    public override void OnInvoke(MethodInterceptionArgs args)
    {
        var emulator = InitEmulator(args);

        CopyMethodArgumentsToEmulator(args, emulator);
        emulator.Start(0x1000);
        CopyResultFromEmulator(args, emulator);
    }

    private static void CopyResultFromEmulator(MethodInterceptionArgs args, Emulator emulator)
    {
        var result = emulator.GetMemory(0x20);
        if (result != 0)
            NUnit.Framework.Assert.Fail(GetMessage(emulator));

        if (args.Method is MethodInfo)
        {
            var type = ((MethodInfo)args.Method).ReturnType;
            var value = EmulatorArgumentMarshaling.ReadReturnValue(emulator, type, RETURN_VALUE_ADDRESS);
            if (value != null)
                args.ReturnValue = value;
        }
    }

    private static void CopyMethodArgumentsToEmulator(MethodInterceptionArgs args, Emulator emulator)
    {
        const int ARGUMENTS_ADDRESS = 0x9e0;
        var pointer = ARGUMENTS_ADDRESS + 1;
        for (byte i = 0; i < (byte) args.Arguments.Count; i++)
        {
            EmulatorArgumentMarshaling.WriteArgument(emulator, ref pointer, args.Arguments[i]);
        }

        emulator.SetMemory(ARGUMENTS_ADDRESS, (byte) (pointer- ARGUMENTS_ADDRESS-1));
    }

    private static Emulator InitEmulator(MethodInterceptionArgs args)
    {
        var location = typeof(RunInEmulatorAspect).Assembly.Location;
        var directory = new FileInfo(location).DirectoryName;
        var prgFolder = Path.Combine(directory, "..\\..\\..\\prg");
        var emulator = new Emulator();
        emulator.SetMemory(0x09fe, GetMethodAddress(prgFolder, args.Method));
        emulator.LoadPrg(Path.Combine(prgFolder, "unittest.prg"));
        return emulator;
    }

    // dotnet test/VSTest runs [TestCase]s from this assembly across several
    // worker threads concurrently (confirmed: 570+ tests, each involving
    // loading a ~47KB .prg and running thousands of emulated 6502 steps,
    // completing the whole run in ~30ms is only possible with real
    // parallelism) -- this method used to check-then-populate the shared
    // static `labels` field with no synchronization at all, a classic
    // unsafe lazy-init: one thread could observe `labels` as non-null
    // (another thread had just assigned `new Dictionary<...>()`) while
    // that other thread's populating `foreach` was still mid-flight, then
    // read a Dictionary that's actively being mutated on another thread --
    // Dictionary<TKey,TValue> gives no guarantees at all for that (not a
    // clean exception, potentially a wrong-but-valid-looking value from an
    // in-progress resize/rehash), which manifested as WIDESPREAD,
    // seemingly-unrelated test failures (many different test classes,
    // always reading back 0 instead of the real computed value) only once
    // the suite grew large enough for the race window to actually get hit
    // in practice -- never reproducible via a small --filter'd subset,
    // which is exactly what a race being "sometimes losing" looks like.
    // Locking the whole check-and-populate (not just wrapping the
    // assignment) closes the window entirely.
    private static readonly object labelsLock = new object();
    private static Dictionary<string, string> labels;
    public static byte[] GetMethodAddress(string prgFolder, MethodBase method)
    {
        string label = $".{method.ReflectedType.Name}_{method.Name}";

        lock (labelsLock)
        {
            if (labels == null)
            {
                var newLabels = new Dictionary<string, string>();
                var lines = File.ReadAllLines(Path.Combine(prgFolder, "unittest.labels"));
                foreach (var line in lines)
                {
                    var parts = line.Split(' ');
                    // Indexer assignment, not .Add() -- .Add() throws on a
                    // duplicate key, which would crash the entire test run
                    // (not just one test) the moment two labels ever
                    // collide; last-one-wins is a safer failure mode for a
                    // label-name clash that isn't expected to happen but
                    // also isn't specifically guarded against elsewhere.
                    newLabels[parts[2]] = parts[1];
                }
                labels = newLabels;
            }

            var s = labels[label];
            return new byte[] {
                Convert.ToByte(s.Substring(2), 16),
                Convert.ToByte(s.Substring(0, 2), 16),
            };
        }
    }

    public static string GetMessage(Emulator emulator)
    {
        var low = emulator.GetMemory(0x2E);
        var high = emulator.GetMemory(0x2F);
        var address = high * 256 + low;
        var b = emulator.GetMemory(address);
        string result = "";
        while (b != 0)
        {
            result += (char)b; // C64CharConverter.ConvertToAscii(b);
            b = emulator.GetMemory(++address);

        }
        return result;



    }
}