using System.Collections.Generic;

namespace Compiler;

// The one list of which System.MathF methods this compiler actually links
// against a hand-written asm/helper/float.asm routine (MathF_Sin/Cos/Sqrt).
// Shared by ILLibraryUsagePass (decides whether a program pays for one of
// these) and Operands/OperandBase.cs's EnsureCallIsResolvable (decides
// whether calling it is even allowed to compile) -- kept in exactly one
// place so the two can never drift out of sync: a method missing from this
// set would otherwise pass one check and fail the other, either as a
// confusing 64tass "not defined symbol" (usage-tracking knows it, resolve
// check doesn't) or a silently-always-excluded routine nothing ever jsr's
// correctly (resolve check knows it, usage-tracking doesn't).
static class MathFSupport
{
    public static readonly HashSet<string> SupportedMethods = new() { "Sin", "Cos", "Sqrt" };
}
