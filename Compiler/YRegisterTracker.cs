using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Compiler;

// Drops redundant `ldy stackPointer` reloads while a method is emitted.
// Most localsStack accesses (asm/helper/localsStack.asm's locals_* macros,
// optimized.asm's init_var) begin by loading stackPointer into Y and leave
// it there; when the previous access already did that and nothing in
// between could have changed Y or stackPointer, the access is rewritten to
// its "_ky" twin, which skips the reload (2 bytes, 3 cycles each).
//
// Straight-line only and deliberately conservative: anything not known to
// leave Y alone -- a label (a branch may arrive with any Y), a jsr, any
// macro not in NeutralMacros, any raw instruction not in NeutralOpcodes --
// forgets what Y holds.
class YRegisterTracker
{
    // Macros whose expansion (checked against asm/helper/*.asm, including
    // the macros they invoke) never writes Y or stackPointer and never jsr's
    // anywhere that does. A taken branch leaves the straight line (its
    // target is labelled, which resets the state); the fall-through keeps Y.
    private static readonly HashSet<string> NeutralMacros = new HashSet<string>
    {
        "add16", "add16_tail", "add8", "add8_tail", "and16", "and8",
        "branch_equal16", "branch_equal8", "branch_equal_const", "branch_false", "branch_false16", "branch_false8",
        "branch_greater16", "branch_greater8", "branch_greater_const", "branch_greater_equal16", "branch_greater_equal8",
        "branch_greater_equal_const", "branch_greater_equal_unsigned16", "branch_greater_equal_unsigned8",
        "branch_greater_equal_unsigned_const", "branch_greater_unsigned16", "branch_greater_unsigned8",
        "branch_greater_unsigned_const", "branch_less16", "branch_less8", "branch_less_const", "branch_less_equal16",
        "branch_less_equal8", "branch_less_equal_const", "branch_less_equal_unsigned16", "branch_less_equal_unsigned8",
        "branch_less_equal_unsigned_const", "branch_less_unsigned16", "branch_less_unsigned8", "branch_less_unsigned_const",
        "branch_not_equal16", "branch_not_equal8", "branch_not_equal_const", "branch_true", "branch_true16", "branch_true8",
        "compareEqual16", "compareEqual8", "compareEqual_const8", "compareGreater16", "compareGreater8",
        "compareGreater_const8", "compareGreater_unsigned16", "compareGreater_unsigned8", "compareGreater_unsigned_const8",
        "compareLess16", "compareLess8", "compareLess_const8", "compareLess_unsigned16", "compareLess_unsigned8",
        "compareLess_unsigned_const8",
        "conv_16_8", "conv_8_16", "copy_int16_const", "copy_int8_const",
        "dec_var", "dec_var_zp", "inc_var", "inc_var_zp", "init_var_zp", "deref",
        "ldlen", "ldlen16",
        "mul_shift_const16", "mul_shift_const8", "div_shift_const16", "div_shift_const8",
        // jsr Multiply8/Multiply16 (asm/helper/division.asm): A, X and
        // zp_param0..2 only.
        "mul8", "mul8_tail", "mul16", "mul16_tail",
        "negate16", "negate8", "not16", "not8", "or16", "or8", "xor16", "xor8",
        "shift_left8", "shift_right8",
        "stack_duplicate8", "stack_pop", "stack_pull_int", "stack_pull_int16", "stack_pull_int8", "stack_pull_int_a",
        "stack_pull_int_ref", "stack_pull_int_x", "stack_pull_pointer",
        "stack_push_int", "stack_push_int16", "stack_push_int8", "stack_push_int_a", "stack_push_int_x",
        "stack_push_int_y", "stack_push_pointer", "stack_push_var", "stack_push_var16", "stack_push_var8",
        "sub16", "sub16_tail", "sub8", "sub8_tail",
        "zp_pull_value8", "zp_push_value8",
    };

    // Accesses that begin with `ldy stackPointer` and end with Y still
    // holding it; each has a "_ky" twin without the load.
    private static readonly HashSet<string> Rewritable = new HashSet<string>
    {
        "locals_push_value8", "locals_pull_value8", "locals_push_value16", "locals_pull_value16",
        "locals_copy_value8", "locals_copy_value16", "init_var",
    };

    // Leave Y = stackPointer (the prologues end with `sty stackPointer`).
    private static readonly HashSet<string> Setters = new HashSet<string>
    {
        "init_locals_pull_parameters", "init_locals_pull_parameters_inline",
    };

    // Raw instructions (fused sequences emit a few) that never touch Y or
    // stackPointer.
    private static readonly HashSet<string> NeutralOpcodes = new HashSet<string>
    {
        "lda", "ldx", "pha", "pla", "tax", "txa", "clc", "sec", "adc", "sbc", "and", "ora", "eor",
        "cmp", "cpx", "inx", "dex", "asl", "lsr", "rol", "ror", "nop", "bit",
    };

    private bool _yIsStackPointer;

    public void Reset() => _yIsStackPointer = false;

    // One emitted IL line (possibly several asm lines). hasLabel: the line
    // is a branch target.
    public string Process(string text, bool hasLabel)
    {
        if (hasLabel)
            _yIsStackPointer = false;
        var lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
            lines[i] = ProcessLine(lines[i]);
        return string.Join("\n", lines.Where(l => l != null));
    }

    private static readonly Regex LabelLine = new Regex(@"^\s*([A-Za-z_][\w.]*:|[+\-]+(\s|$))");

    private string ProcessLine(string line)
    {
        var code = line.Split(';')[0].Trim();
        if (code.Length == 0)
            return line;
        if (LabelLine.IsMatch(code))
        {
            _yIsStackPointer = false;
            return line;
        }

        var parts = code.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
        var word = parts[0];
        var operand = parts.Length > 1 ? parts[1].Trim() : "";

        if (word.StartsWith("#"))
        {
            var name = word.Substring(1);
            if (Rewritable.Contains(name))
            {
                var result = _yIsStackPointer ? ReplaceFirst(line, word, word + "_ky") : line;
                _yIsStackPointer = true;
                return result;
            }
            if (Setters.Contains(name))
            {
                _yIsStackPointer = true;
                return line;
            }
            if (name == "method_enter_frameless")
            {
                // Only touches Y (ending in sty stackPointer) when it has locals.
                if (!operand.StartsWith("0"))
                    _yIsStackPointer = true;
                return line;
            }
            if (!NeutralMacros.Contains(name))
                _yIsStackPointer = false;
            return line;
        }

        var op = word.ToLowerInvariant();
        if (op == "ldy" && operand == "stackPointer")
        {
            if (_yIsStackPointer)
                return null;
            _yIsStackPointer = true;
            return line;
        }
        if (NeutralOpcodes.Contains(op) && !operand.Contains("stackPointer"))
            return line;
        _yIsStackPointer = false;
        return line;
    }

    private static string ReplaceFirst(string text, string search, string replacement)
    {
        int pos = text.IndexOf(search, StringComparison.Ordinal);
        return text.Substring(0, pos) + replacement + text.Substring(pos + search.Length);
    }
}
