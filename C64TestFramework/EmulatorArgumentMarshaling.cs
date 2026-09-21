using System;
using SimpleEmulator;

namespace C64TestFramework;

// Per-type encode/decode logic shared between RunInEmulatorAspect (the real
// NUnit test-running path) and TestDebugger (the CLI step-debugger). Factored
// out of RunInEmulatorAspect's former private CopyMethodArgumentsToEmulator/
// CopyResultFromEmulator so both consumers use the exact same byte-for-byte
// convention instead of risking silent drift between two copies.
public static class EmulatorArgumentMarshaling
{
    // Writes one test-method-argument value at `pointer`, advancing it past
    // the bytes written -- identical body to RunInEmulatorAspect's former
    // CopyMethodArgumentsToEmulator loop, just factored to run per-value.
    // Note this intentionally has no case for every possible parameter type
    // (e.g. bool, string) -- it only supports what the real test suite has
    // ever actually exercised. Silently writing nothing for an unsupported
    // type matches the exact pre-refactor behavior.
    public static void WriteArgument(Emulator emulator, ref int pointer, object value)
    {
        if (value is int)
        {
            int v = (int)value;
            if (v < -127 || v > 127)
                throw new ArgumentOutOfRangeException("int");
            emulator.SetMemory(pointer++, v < 0 ? (byte)(256 + v) : (byte)v);
        }

        if (value is uint)
        {
            emulator.SetMemory(pointer++, (byte)(uint)value);
        }

        if (value is long)
        {
            var bytes = BitConverter.GetBytes((short)(long)value);
            emulator.SetMemory(pointer++, bytes[1]);
            emulator.SetMemory(pointer++, bytes[0]);
        }

        if (value is ulong)
        {
            var bytes = BitConverter.GetBytes((ushort)(ulong)value);
            emulator.SetMemory(pointer++, bytes[1]);
            emulator.SetMemory(pointer++, bytes[0]);
        }

        if (value is float)
        {
            // Natural Mflpt byte[0..4] order -- matches how
            // #locals_push_valueflt/stack_push_var_mflpt push a float
            // (byte[0] first ... byte[4] last).
            var mflpt = Compiler.Mflpt.ToBytes((float)value);
            foreach (var b in mflpt)
                emulator.SetMemory(pointer++, b);
        }
    }

    // Decodes a RETURN value at `address` -- identical body to
    // RunInEmulatorAspect's former CopyResultFromEmulator per-type block.
    // The 2/5-byte cases read in REVERSED byte order because the value was
    // PULLED off the emulated evaluation stack (top/last-pushed byte first,
    // see asm/helper/stack.asm's stack_pull_mflpt and
    // Compiler/Templates/UnitTestEntry.asm's endtest) -- do not reuse this
    // for a local variable read, which is a flat memory read, not a stack
    // pull; use ReadLocal for that instead.
    public static object ReadReturnValue(Emulator emulator, Type type, int address)
    {
        if (type == typeof(int))
            return (int)(sbyte)emulator.GetMemory(address);
        if (type == typeof(bool))
            return emulator.GetMemory(address) != 0;
        if (type == typeof(uint))
            return (uint)emulator.GetMemory(address);
        if (type == typeof(long))
            return (long)BitConverter.ToInt16(new[]
                { emulator.GetMemory(address), emulator.GetMemory(address + 1) }, 0);
        if (type == typeof(ulong))
            return (ulong)BitConverter.ToUInt16(new[]
                { emulator.GetMemory(address), emulator.GetMemory(address + 1) }, 0);
        if (type == typeof(float))
        {
            var mflpt = new[]
            {
                emulator.GetMemory(address + 4),
                emulator.GetMemory(address + 3),
                emulator.GetMemory(address + 2),
                emulator.GetMemory(address + 1),
                emulator.GetMemory(address),
            };
            return Compiler.Mflpt.FromBytes(mflpt);
        }
        return null;
    }

    // Decodes a LOCAL VARIABLE (or parameter) value stored directly in
    // localsStack at `address`. Empirically confirmed against
    // asm/helper/localsStack.asm's locals_push_value16/locals_pull_value16/
    // locals_push_valueflt/locals_pull_valueflt macros (both push a value
    // onto/off of the SAME hardware stack stack_push_int16/stack_push_pointer/
    // stack_push_var_mflpt use elsewhere, so the same LIFO byte-order
    // reversal rules apply):
    //  - 16-bit values (long/ulong/string pointer) end up stored NATURALLY
    //    (low byte at `address`, high byte at `address+1`) -- confirmed by
    //    tracing stack_push_int16 (pushes high first, low last -> low ends
    //    up on top) through locals_pull_value16 (first pull, i.e. whatever
    //    is on top, goes to `address`).
    //  - float (MFLPT, 5 bytes) ends up stored REVERSED, the SAME
    //    convention ReadReturnValue uses (`address` holds Mflpt byte[4],
    //    `address+4` holds byte[0]) -- see locals_push_valueflt's own
    //    comment in localsStack.asm, which states this explicitly.
    public static object ReadLocal(Emulator emulator, Type type, int address)
    {
        if (type == typeof(int))
            return (int)(sbyte)emulator.GetMemory(address);
        if (type == typeof(bool))
            return emulator.GetMemory(address) != 0;
        if (type == typeof(uint) || type == typeof(byte))
            return (uint)emulator.GetMemory(address);
        if (type == typeof(long))
            return (long)BitConverter.ToInt16(new[]
                { emulator.GetMemory(address), emulator.GetMemory(address + 1) }, 0);
        if (type == typeof(ulong))
            return (ulong)BitConverter.ToUInt16(new[]
                { emulator.GetMemory(address), emulator.GetMemory(address + 1) }, 0);
        if (type == typeof(float))
        {
            var mflpt = new[]
            {
                emulator.GetMemory(address + 4),
                emulator.GetMemory(address + 3),
                emulator.GetMemory(address + 2),
                emulator.GetMemory(address + 1),
                emulator.GetMemory(address),
            };
            return Compiler.Mflpt.FromBytes(mflpt);
        }
        if (type == typeof(string))
        {
            // Stored as a 2-byte pointer, same layout as long/ulong above,
            // to a null-terminated string in the heap/string-resource area.
            var low = emulator.GetMemory(address);
            var high = emulator.GetMemory(address + 1);
            int strAddress = high * 256 + low;
            var result = "";
            var b = emulator.GetMemory(strAddress);
            while (b != 0)
            {
                result += (char)b;
                b = emulator.GetMemory(++strAddress);
            }
            return result;
        }
        return null;
    }
}
