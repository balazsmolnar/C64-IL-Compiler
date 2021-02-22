using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PostSharp;
using PostSharp.Aspects;
using SimpleEmulator;

namespace C64TestFramework
{
    [Serializable]
    public class RunInEmulatorAspect : MethodInterceptionAspect
    {
        public override void OnInvoke(MethodInterceptionArgs args)
        {
            var location = typeof(RunInEmulatorAspect).Assembly.Location;
            var directory = new FileInfo(location).DirectoryName;
            var prgFolder = Path.Combine(directory, "..\\..\\..\\prg");
            var emulator = new Emulator();
            emulator.SetMemory(0x09fe, GetMethodAddress(prgFolder, args.Method));
            emulator.LoadPrg(Path.Combine(prgFolder, "unittest.prg"));

            emulator.SetMemory(0x9e0, (byte)args.Arguments.Count);
            for (byte i = 0; i < (byte)args.Arguments.Count; i++)
            {
                if (args.Arguments[i] is int)
                    emulator.SetMemory(0x9e1 + i, (byte)(int)args.Arguments[i]);
                if (args.Arguments[i] is uint)
                    emulator.SetMemory(0x9e1 + i, (byte)(uint)args.Arguments[i]);
            }

            emulator.Start(0x1000);

            var result = emulator.GetMemory(0x20);
            if (result != 0)
                NUnit.Framework.Assert.Fail(GetMessage(emulator));

            if (args.Method is MethodInfo)
            {
                var type = ((MethodInfo) args.Method).ReturnType;
                if (type == typeof(int))
                    args.ReturnValue = (int)emulator.GetMemory(0x2c);
                if (type == typeof(Boolean))
                    args.ReturnValue = emulator.GetMemory(0x2c) != 0;
            }
        }

        private static Dictionary<string, string> labels;
        public static byte[] GetMethodAddress(string prgFolder, MethodBase method)
        {
            string label = $".{method.DeclaringType.Name}_{method.Name}";

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
}
