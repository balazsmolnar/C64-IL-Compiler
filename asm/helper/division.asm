; ============================================================================
; Integer division/modulo -- Div/Div_un/Rem/Rem_un (Compiler/CommandMap.cs
; maps all four straight to OpArithmetic2, same as Add/Sub/Mul; its own
; SizeSuffix picks "8"/"16" the same way it already does for those).
;
; Real subroutines, not macros with an inline loop the way mul8/mul16 are --
; unlike those, division needs its shift-subtract loop reachable from EIGHT
; distinct call shapes (div/rem x signed/unsigned x 8/16-bit), and the
; signed wrappers need MULTIPLE separate conditional two's-complement
; negations in one expansion; keeping each as its own real subroutine
; (jsr'd, `.macro`'s automatic per-expansion scoping isn't needed since
; these are defined once) avoids both the code-size blowup of inlining the
; loop at every call site and the risk of 64tass's anonymous +/- labels
; between the two source files (../ arithmetic.asm is macro-only and lives
; in the pre-#start_at, no-real-code zone -- see this file's own .include
; comment in ProgramEntry.asm/UnitTestEntry.asm for why division needs to
; sit here instead, alongside float.asm, rather than in arithmetic.asm
; resolving to the wrong target across several negations in the same macro
; expansion (see git history for the specific bug this replaced).
;
; Signed division truncates toward zero and the remainder takes the
; dividend's sign (or is zero) -- matches C#/.NET's own int/long division
; semantics, and is what these signed wrappers implement: negate both
; operands to their unsigned magnitude, run the shared unsigned core, then
; correct the quotient's sign (negative iff exactly one operand was
; negative) or the remainder's sign (negative iff the dividend was).
; -128/-32768 (this compiler's int/long minimums) negate to themselves
; (two's complement has no positive counterpart for the minimum value) --
; not specially guarded, same as this codebase's other silent-overflow
; arithmetic (add8/sub8/etc in arithmetic.asm).
;
; Division by zero is undefined (whatever the raw shift-subtract loop
; produces -- for the unsigned core that's a quotient of all 1-bits and
; the dividend itself as the remainder, since every trial subtraction
; against a zero divisor always succeeds), not guarded against or thrown
; -- this compiler has no exceptions, consistent with every other
; arithmetic op here never checking for overflow either.
; ============================================================================

; Unsigned 8-bit divide. Input: zp_param2_low = dividend, zp_param1_low =
; divisor. Output: zp_param2_low = quotient, zp_param0_low = remainder.
; Standard shift-subtract restoring division: each iteration shifts the
; next dividend bit out of zp_param2_low (via asl) and into the remainder
; (via rol A), then subtracts the divisor if it fits, recording that as
; the new quotient bit. The "inc zp_param2_low to set the quotient bit"
; step is safe because the asl immediately above it always clears that
; same (now-vacated) bit first.
Divide8Core:
    lda #0
    ldx #8
-   asl zp_param2_low
    rol
    cmp zp_param1_low
    bcc +
    sbc zp_param1_low
    inc zp_param2_low
+   dex
    bne -
    sta zp_param0_low
    rts

; Same algorithm as Divide8Core, widened to 16 bits -- one logical 32-bit
; left-rotate-with-carry-in per iteration across all four bytes (dividend
; low/high, remainder low/high), extracting the dividend's MSB into the
; remainder's LSB each time. The trial subtraction (sec/sbc chain, using Y
; to hold the low-byte result until the high-byte SBC's final carry says
; whether to commit) is the standard 16-bit "subtract first, keep the
; result only if it didn't borrow" idiom -- avoids computing the subtract
; twice (once to check, once to commit).
; Input: zp_param2_low/high = dividend, zp_param1_low/high = divisor.
; Output: zp_param2_low/high = quotient, zp_param0_low/high = remainder.
Divide16Core:
    lda #0
    sta zp_param0_low
    sta zp_param0_high
    ldx #16
-   asl zp_param2_low
    rol zp_param2_high
    rol zp_param0_low
    rol zp_param0_high
    sec
    lda zp_param0_low
    sbc zp_param1_low
    tay
    lda zp_param0_high
    sbc zp_param1_high
    bcc +
    sta zp_param0_high
    sty zp_param0_low
    inc zp_param2_low
+   dex
    bne -
    rts

; Unconditionally two's-complement-negates zp_param2_low in place -- used by
; div8/rem8's signed wrappers for the FINAL sign-correction step. That step
; must always flip the sign (Divide8Core's raw result is always a positive
; magnitude, and the final result needs to become negative whenever the
; php/plp-carried flag says so), unlike NegateParam2If8Negative below (which
; is a no-op on an already-positive value) -- reusing the conditional
; version here was the actual bug this replaced: it silently did nothing
; because the quotient/remainder coming out of Divide8Core is never
; negative, so "negate only if negative" never fired (see git history).
NegateParam2_8:
    lda zp_param2_low
    eor #$FF
    clc
    adc #$1
    sta zp_param2_low
    rts

; Conditionally two's-complement-negates zp_param2_low in place (only if
; currently negative) -- used by div8/rem8's signed wrappers to convert the
; dividend to its unsigned magnitude before Divide8Core. Real subroutine
; rather than inlined in div8/rem8 directly so each of their several
; negation points can share one already-correct implementation instead of
; repeating (and risking a copy/paste mistake in) the same few instructions.
NegateParam2If8Negative:
    lda zp_param2_low
    bmi +
    rts
+   eor #$FF
    clc
    adc #$1
    sta zp_param2_low
    rts

; Same as above, for zp_param1_low (the divisor) -- div8/rem8 need both
; operands negated independently before Divide8Core sees them.
NegateParam1If8Negative:
    lda zp_param1_low
    bmi +
    rts
+   eor #$FF
    clc
    adc #$1
    sta zp_param1_low
    rts

; 16-bit counterpart of NegateParam2_8 above -- unconditional, for div16/
; rem16's final sign-correction step.
NegateParam2_16:
    lda zp_param2_high
    eor #$FF
    sta zp_param2_high
    lda zp_param2_low
    eor #$FF
    clc
    adc #$1
    bcc +
    inc zp_param2_high
+   sta zp_param2_low
    rts

; 16-bit counterparts of the two conditional negates above, same two's-
; complement sequence negate16 (arithmetic.asm) uses, just conditional and
; operating in place rather than pulled from/pushed to the eval stack.
NegateParam2If16Negative:
    lda zp_param2_high
    bmi +
    rts
+   eor #$FF
    sta zp_param2_high
    lda zp_param2_low
    eor #$FF
    clc
    adc #$1
    bcc +
    inc zp_param2_high
+   sta zp_param2_low
    rts

NegateParam1If16Negative:
    lda zp_param1_high
    bmi +
    rts
+   eor #$FF
    sta zp_param1_high
    lda zp_param1_low
    eor #$FF
    clc
    adc #$1
    bcc +
    inc zp_param1_high
+   sta zp_param1_low
    rts

div_unsigned8 .macro
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param2_low
    jsr Divide8Core
    #stack_push_var zp_param2_low
.endm

rem_unsigned8 .macro
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param2_low
    jsr Divide8Core
    #stack_push_var zp_param0_low
.endm

; php/plp carries the sign decision (the N flag from the eor below) across
; the jsr calls -- both Divide8Core and the Negate* subroutines only ever
; push/pop the hardware stack in matched jsr/rts pairs, so a php pushed
; before them and popped after stays correctly aligned.
div8 .macro
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param2_low

    lda zp_param2_low
    eor zp_param1_low
    php

    jsr NegateParam2If8Negative
    jsr NegateParam1If8Negative
    jsr Divide8Core

    plp
    bpl +
    jsr NegateParam2_8
+   #stack_push_var zp_param2_low
.endm

rem8 .macro
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param2_low

    lda zp_param2_low
    php

    jsr NegateParam2If8Negative
    jsr NegateParam1If8Negative
    jsr Divide8Core

    lda zp_param0_low
    sta zp_param2_low

    plp
    bpl +
    jsr NegateParam2_8
+   #stack_push_var zp_param2_low
.endm

div_unsigned16 .macro
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high
    jsr Divide16Core
    lda zp_param2_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

rem_unsigned16 .macro
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high
    jsr Divide16Core
    lda zp_param0_high
    #stack_push_int_a
    #stack_push_var zp_param0_low
.endm

div16 .macro
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    lda zp_param2_high
    eor zp_param1_high
    php

    jsr NegateParam2If16Negative
    jsr NegateParam1If16Negative
    jsr Divide16Core

    plp
    bpl +
    jsr NegateParam2_16
+   lda zp_param2_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

rem16 .macro
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    lda zp_param2_high
    php

    jsr NegateParam2If16Negative
    jsr NegateParam1If16Negative
    jsr Divide16Core

    lda zp_param0_low
    sta zp_param2_low
    lda zp_param0_high
    sta zp_param2_high

    plp
    bpl +
    jsr NegateParam2_16
+   lda zp_param2_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm
