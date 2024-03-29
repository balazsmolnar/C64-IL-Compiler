﻿using System;
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
            if (type == typeof(int))
                args.ReturnValue = (int)(sbyte)emulator.GetMemory(RETURN_VALUE_ADDRESS);
            if (type == typeof(Boolean))
                args.ReturnValue = emulator.GetMemory(RETURN_VALUE_ADDRESS) != 0;
            if (type == typeof(uint))
                args.ReturnValue = (uint)emulator.GetMemory(RETURN_VALUE_ADDRESS);
            if (type == typeof(long))
                args.ReturnValue = (long)BitConverter.ToInt16(new[]
                    { emulator.GetMemory(RETURN_VALUE_ADDRESS), emulator.GetMemory(RETURN_VALUE_ADDRESS+1) });
            if (type == typeof(ulong))
                args.ReturnValue = (ulong)BitConverter.ToUInt16(new[]
                    { emulator.GetMemory(RETURN_VALUE_ADDRESS), emulator.GetMemory(RETURN_VALUE_ADDRESS+1) });
        }
    }

    private static void CopyMethodArgumentsToEmulator(MethodInterceptionArgs args, Emulator emulator)
    {
        const int ARGUMENTS_ADDRESS = 0x9e0;
        var pointer = ARGUMENTS_ADDRESS + 1;
        for (byte i = 0; i < (byte) args.Arguments.Count; i++)
        {
            if (args.Arguments[i] is int)
            {
                int v = (int) args.Arguments[i];
                if (v < -127 || v > 127)
                    throw new ArgumentOutOfRangeException("int");
                emulator.SetMemory(pointer++, v < 0 ? (byte) (256 + v) : (byte) v);
            }

            if (args.Arguments[i] is uint)
            {
                emulator.SetMemory(pointer++, (byte) (uint) args.Arguments[i]);
            }

            if (args.Arguments[i] is long)
            {
                var bytes = BitConverter.GetBytes((short)(long) args.Arguments[i]);
                emulator.SetMemory(pointer++, bytes[1]);
                emulator.SetMemory(pointer++, bytes[0]);
            }

            if (args.Arguments[i] is ulong)
            {
                var bytes = BitConverter.GetBytes((ushort)(ulong)args.Arguments[i]);
                emulator.SetMemory(pointer++, bytes[1]);
                emulator.SetMemory(pointer++, bytes[0]);
            }
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

    private static Dictionary<string, string> labels;
    public static byte[] GetMethodAddress(string prgFolder, MethodBase method)
    {
        string label = $".{method.ReflectedType.Name}_{method.Name}";

        if (labels == null)
        {
            labels = new Dictionary<string, string>();
            var lines = File.ReadAllLines(Path.Combine(prgFolder, "unittest.labels"));
            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                labels.Add(parts[2], parts[1]);
            }
        }

        var s = labels[label];
        return new byte[] {
            Convert.ToByte(s.Substring(2), 16),
            Convert.ToByte(s.Substring(0, 2), 16),
        };
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