using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler;

// Keeps the top of the evaluation stack in A across a macro boundary.
//
// The evaluation stack is the hardware stack, so a value one IL operation
// computes and the next consumes makes a round trip through it: the first
// macro ends with `pha`, the second begins with `pla`. When nothing (no
// label) separates the two, the pair is dropped: ILMethodEmitPass emits
// "tos_out ::= 1" right before the producing macro, whose final push
// (stack_push_tos, asm/helper/stack.asm) then leaves the value in A and sets
// tos_in, and the consumer's first pull (stack_pull_int_a, which every pull
// of the top byte goes through) skips its pla. Saves 2 bytes and 7 cycles
// per pair; Catacombs had about 570 of them.
//
// Only the macros listed here take part, each checked by hand:
//  - a producer's tos-aware push is its very last instruction and its only
//    one (an earlier one would take the request);
//  - a consumer pulls the top byte before it does anything else with A.
// The ".cerror" lines emitted after each pair catch a macro that doesn't
// play along (a producer that never took tos_out, a consumer that never
// pulled) at assembly time.
//
// Flags: a pla sets N and Z from the value. A consumer that branches on
// them straight away (branch_true8/branch_false8: `pla; bne`) is only
// paired with a producer whose last flag-setting instruction was on the
// value itself (lda, and, adc, txa...), not, say, a jsr whose result
// arrives in A with unrelated flags.
//
// Labels: a branch target between the two (on the consumer, or on an
// optimized-away line in between) means another path arrives with the
// value on the stack, so the pair stays. With EmitDebugInfo every sequence
// point is a label too (ILMethodDebugLabelPass), so debugger step points
// still see each statement's value on the stack.
//
// 8-bit pairs only in effect: a 16-bit producer leaves just its low (top)
// byte in A, which is all the consumer's first pull takes.
static class TopOfStackInA
{
    // Producer macro -> are N/Z set from the value when its final push is
    // skipped (the argument text decides for a few).
    private static readonly Dictionary<string, Func<string, bool>> Producers = new()
    {
        ["stack_push_int"] = Always, ["stack_push_int8"] = Always, ["stack_push_int16"] = Always,
        ["stack_push_var"] = Always, ["stack_push_var8"] = Always, ["stack_push_var16"] = Always,
        ["stack_push_pointer"] = Always,
        ["locals_push_value8"] = Always, ["locals_push_value8_ky"] = Always,
        ["locals_push_value16"] = Always, ["locals_push_value16_ky"] = Always,
        ["zp_push_value8"] = Always,
        ["pushfld8"] = Always, ["pushfld8_at"] = Always, ["pushfld8_cached"] = Always,
        ["pushfld16"] = Always, ["pushfld16_at"] = Always, ["pushfld16_cached"] = Always,
        ["ldfld8"] = Always, ["ldfld16"] = Always, ["ldelem"] = Always, ["ldelemRef"] = Always,
        ["ldelem16"] = Always, ["ldlen"] = Always, ["ldlen16"] = Always,
        // jsr newObjL/newObjLInit/Multiply8/DivConst8_d: the result comes back in A, flags unrelated.
        ["newObjInit"] = Never, ["newArr"] = Never, ["newArrRef"] = Never, ["newArr16"] = Never,
        ["newArrInit"] = Never, ["mul8"] = Never, ["mul8_tail"] = Never, ["rem_const8"] = Never,
        ["add8"] = Always, ["add8_tail"] = Always, ["sub8"] = Always, ["sub8_tail"] = Always,
        ["and8"] = Always, ["or8"] = Always, ["xor8"] = Always,
        ["and8_tail"] = Always, ["or8_tail"] = Always, ["xor8_tail"] = Always,
        // An identity constant (and #$ff, ora/eor #0) emits no instruction at all.
        ["and8_const"] = v => !IsByte(v, 0xFF), ["and8_const_tail"] = v => !IsByte(v, 0xFF),
        ["or8_const"] = v => !IsByte(v, 0), ["or8_const_tail"] = v => !IsByte(v, 0),
        ["xor8_const"] = v => !IsByte(v, 0), ["xor8_const_tail"] = v => !IsByte(v, 0),
        // A shift by 0 is just pull + push.
        ["mul_shift_const8"] = ShiftsAtLeastOnce, ["div_shift_const8"] = ShiftsAtLeastOnce,
        ["rem_mask8"] = Always, ["not8"] = Always, ["negate8"] = Always,
        // Final push through stack_push_var (lda).
        ["add16"] = Always, ["add16_tail"] = Always, ["sub16"] = Always, ["sub16_tail"] = Always,
        ["and16"] = Always, ["and16_tail"] = Always, ["or16"] = Always, ["or16_tail"] = Always,
        ["xor16"] = Always, ["xor16_tail"] = Always, ["mul16"] = Always, ["mul16_tail"] = Always,
        ["div_const8"] = Always, ["div_const16"] = Always, ["div_shift_const16"] = Always,
        ["conv_8_16"] = Always, ["conv_16_8"] = Always,
        ["compareEqual8"] = Always, ["compareEqual_const8"] = Always,
        ["compareLess8"] = Always, ["compareLess_const8"] = Always,
        ["compareLess_unsigned8"] = Always, ["compareLess_unsigned_const8"] = Always,
        ["compareGreater8"] = Always, ["compareGreater_const8"] = Always,
        ["compareGreater_unsigned8"] = Always, ["compareGreater_unsigned_const8"] = Always,
        // Its pull may itself have been skipped: flags are whatever came before.
        ["stack_duplicate8"] = Never,
        // A static-frame call's returned value (asm/helper/localsStack.asm):
        // one byte arrives in A from the callee's sf_ret, whose own pull may
        // have been skipped (flags unrelated); two bytes end with txa.
        ["sf_result"] = a => Arg(a, 0) == "2",
    };

    // Consumer macro -> (branches on the N/Z its pla set, accepts these arguments).
    private static readonly Dictionary<string, (bool needsFlags, Func<string, bool> accepts)> Consumers = BuildConsumers();

    private static Dictionary<string, (bool, Func<string, bool>)> BuildConsumers()
    {
        var plain = new[]
        {
            "stack_pull_int", "stack_pull_int8", "stack_pull_int16", "stack_pop", "zp_pull_value8",
            "locals_pull_value16", "locals_pull_value16_ky",
            "add8", "sub8", "mul8", "and8", "or8", "xor8", "and8_const", "or8_const", "xor8_const",
            "add16", "sub16", "mul16", "and16", "or16", "xor16",
            "mul_shift_const8", "div_shift_const8", "mul_shift_const16", "div_shift_const16",
            "div_const8", "rem_const8", "div_const16", "rem_const16", "rem_mask8", "rem_mask16",
            "not8", "negate8", "not16", "negate16", "conv_8_16", "conv_16_8",
            "compareEqual8", "compareEqual_const8", "compareEqual16",
            "compareLess8", "compareLess_const8", "compareLess_unsigned8", "compareLess_unsigned_const8",
            "compareGreater8", "compareGreater_const8", "compareGreater_unsigned8", "compareGreater_unsigned_const8",
            "compareLess16", "compareGreater16", "compareLess_unsigned16", "compareGreater_unsigned16",
            "newArr", "newArrRef", "newArr16", "stfld8", "stfld16", "stelem", "stelemRef", "stelem16",
            "ldfld8", "ldfld16", "ldelem", "ldelemRef", "ldelem16", "ldlen", "ldlen16",
            "stack_duplicate8", "switch",
            "branch_true16", "branch_false16",
            "branch_equal8", "branch_equal_const", "branch_equal16",
            "branch_not_equal8", "branch_not_equal_const", "branch_not_equal16",
            "branch_less8", "branch_less_const", "branch_less16",
            "branch_less_unsigned8", "branch_less_unsigned_const", "branch_less_unsigned16",
            "branch_less_equal8", "branch_less_equal_const", "branch_less_equal16",
            "branch_less_equal_unsigned8", "branch_less_equal_unsigned_const", "branch_less_equal_unsigned16",
            "branch_greater8", "branch_greater_const", "branch_greater16",
            "branch_greater_unsigned8", "branch_greater_unsigned_const", "branch_greater_unsigned16",
            "branch_greater_equal8", "branch_greater_equal_const", "branch_greater_equal16",
            "branch_greater_equal_unsigned8", "branch_greater_equal_unsigned_const", "branch_greater_equal_unsigned16",
        };
        var d = new Dictionary<string, (bool, Func<string, bool>)>();
        foreach (var name in plain)
            d[name] = (false, Always);
        // `pla; bne label`: the branch reads the flags the pla set.
        foreach (var name in new[] { "branch_true", "branch_true8", "branch_false", "branch_false8" })
            d[name] = (true, Always);
        // With ref=1 these first drop the old reference (ldx/dec/lda #0, or a
        // jsr), which overwrites A before the pull.
        d["stack_pull_int_ref"] = (false, a => Arg(a, 1) == "0");
        d["locals_pull_value8"] = (false, a => Arg(a, 1) == "0");
        d["locals_pull_value8_ky"] = (false, a => Arg(a, 1) == "0");
        // A static-frame return (sf_exit bytes, [refs]): pulls the value
        // first only when there are no references to release before it.
        d["sf_exit"] = (false, a => (Arg(a, 0) == "1" || Arg(a, 0) == "2") && Arg(a, 1) == "[]");
        return d;
    }

    private static bool Always(string args) => true;
    private static bool Never(string args) => false;

    private static string Arg(string args, int index)
    {
        var parts = args.Split(',');
        return index < parts.Length ? parts[index].Trim() : null;
    }

    private static bool IsByte(string args, int b) =>
        !int.TryParse(Arg(args, 0), out var v) || (v & 0xFF) == b;

    private static bool ShiftsAtLeastOnce(string args) => int.TryParse(Arg(args, 0), out var s) && s > 0;

    // One emitted IL line: its label (a branch target) and its code, one asm
    // statement per text line.
    public class Line
    {
        public string Label;
        public bool Optimized;
        public List<string> Code;
    }

    private static string CodeOf(string text) => text.Split(';')[0].Trim();

    private static bool IsStatement(string text) => CodeOf(text).Length > 0;

    // "#name args" or "name args" -> (name, args). Some optimizers emit a
    // macro invocation without the '#'; only names in the tables above (and
    // a bare pha) are ever acted on, so other instructions never match.
    private static (string name, string args) Words(string text)
    {
        var s = CodeOf(text);
        if (s.StartsWith("#"))
            s = s.Substring(1);
        var parts = s.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
        return (parts.Length > 0 ? parts[0] : "", parts.Length > 1 ? parts[1].Trim() : "");
    }

    // Instructions that leave N/Z set from A: what a raw `pha` producer
    // (fused code, e.g. OpStaticArrayLoad's `lda (tmpPointer),y / pha`)
    // needs right in front of it for a flag-reading consumer.
    private static readonly HashSet<string> SetsFlagsFromA = new()
    {
        "lda", "txa", "tya", "pla", "and", "ora", "eor", "adc", "sbc",
    };

    private struct Statement
    {
        public int Line;
        public int Index;
        public bool BarrierBefore; // a branch target (or code the scan can't see) in front of it
    }

    public static void Apply(List<Line> lines)
    {
        // Every statement that assembles to something, in order, with a
        // barrier wherever a branch may arrive. Pairs are found across line
        // boundaries and inside one line's code (a fused operation that
        // pushes and then pulls) alike.
        var statements = new List<Statement>();
        bool barrier = false;
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (!string.IsNullOrEmpty(line.Label))
                barrier = true;
            if (line.Optimized)
            {
                // ILMethodEmitPass only comments out the first text line of
                // an optimized-away line's code; anything after it is real.
                if (line.Code.Skip(1).Any(IsStatement))
                    barrier = true;
                continue;
            }
            for (int k = 0; k < line.Code.Count; k++)
            {
                if (!IsStatement(line.Code[k]))
                    continue;
                statements.Add(new Statement { Line = i, Index = k, BarrierBefore = barrier });
                barrier = false;
            }
        }

        // Find every pair first, then insert the directives.
        var producers = new HashSet<(int, int)>();
        var consumers = new HashSet<(int, int)>();
        for (int n = 0; n + 1 < statements.Count; n++)
        {
            var ps = statements[n];
            var cs = statements[n + 1];
            if (cs.BarrierBefore)
                continue;

            var (pName, pArgs) = Words(lines[ps.Line].Code[ps.Index]);
            bool flagsValid;
            if (pName == "pha" && pArgs.Length == 0)
            {
                var prev = n > 0 && !ps.BarrierBefore ? Words(lines[statements[n - 1].Line].Code[statements[n - 1].Index]).name : "";
                flagsValid = SetsFlagsFromA.Contains(prev.ToLowerInvariant());
            }
            else if (Producers.TryGetValue(pName, out var producerFlags))
                flagsValid = producerFlags(pArgs);
            else
                continue;

            var (cName, cArgs) = Words(lines[cs.Line].Code[cs.Index]);
            (bool needsFlags, Func<string, bool> accepts) spec;
            // A raw pla (fused code; a static-frame call pulling its
            // arguments into the callee's slots, OpCall.StaticFramePulls)
            // takes exactly the value the producer would have pushed. Its
            // N/Z may be read by whatever follows, so the producer's must
            // be valid.
            if (cName == "pla" && cArgs.Length == 0)
                spec = (true, Always);
            else if (!Consumers.TryGetValue(cName, out spec) || !spec.accepts(cArgs))
                continue;
            if (spec.needsFlags && !flagsValid)
                continue;

            producers.Add((ps.Line, ps.Index));
            consumers.Add((cs.Line, cs.Index));
        }

        var touched = producers.Select(p => p.Item1).Concat(consumers.Select(c => c.Item1)).ToHashSet();
        foreach (var index in touched)
        {
            var line = lines[index];
            var code = new List<string>();
            for (int k = 0; k < line.Code.Count; k++)
            {
                bool isProducer = producers.Contains((index, k));
                bool isConsumer = consumers.Contains((index, k));
                var text = line.Code[k];
                if (isProducer)
                {
                    code.Add("tos_out ::= 1");
                    // A raw pha becomes the tos-aware push.
                    if (Words(text).name == "pha")
                        text = "#stack_push_tos" + (text.Contains(';') ? " ;" + text.Split(';', 2)[1] : "");
                }
                // A raw pla becomes the tos-aware pull.
                if (isConsumer && Words(text).name == "pla")
                    text = "#stack_pull_int_a" + (text.Contains(';') ? " ;" + text.Split(';', 2)[1] : "");
                code.Add(text);
                if (isProducer)
                    code.Add("    .cerror tos_out, \"top of stack not left in A\"");
                // A statement that takes the value in A and leaves its own
                // result there needs no consumer check: stack_push_tos
                // already refuses to run while tos_in is still set, and sets
                // it again afterwards, for the next statement.
                else if (isConsumer)
                    code.Add("    .cerror tos_in, \"top of stack in A not consumed\"");
            }
            // The line's label is written in front of its first text line,
            // and 64tass takes no label in front of an assignment.
            if (code[0].StartsWith("tos_out"))
                code.Insert(0, "");
            line.Code = code;
        }
    }
}
