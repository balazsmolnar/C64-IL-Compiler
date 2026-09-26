; ============================================================================
; Float arithmetic/comparison/conversion, backed by the C64 BASIC ROM's own
; 5-byte MFLPT routines -- see Compiler/Mflpt.cs for the format `float` is
; stored in throughout this compiler (chosen specifically so these routines
; can be handed values with zero conversion).
;
; ROM entry points (verified against c64-wiki.com's per-routine pages and
; codebase64.net's routine include file, cross-checked across both,
; September 2026):
;   MOVFM  = $BBA2  -- FAC1 = *(A=lo,Y=hi)
;   MOVMF  = $BBD4  -- *(X=lo,Y=hi) = FAC1
;   FADD   = $B867  -- FAC1 += *(A=lo,Y=hi)
;   FSUB   = $B850  -- FAC1 = *(A=lo,Y=hi) - FAC1   (NOT FAC1 - memory!)
;   FMULT  = $BA28  -- FAC1 *= *(A=lo,Y=hi)
;   FDIV   = $BB0F  -- *(A=lo,Y=hi) / FAC1, result left in ARG, NOT FAC1
;                      (confirmed empirically with a zero-page-diff probe,
;                      not from any documentation -- every other routine
;                      here leaves its result in FAC1 as described, FDIV
;                      alone doesn't; MOVEF below copies it across).
;   MOVEF  = $BBFC  -- FAC1 = ARG (needed only after FDIV, see above)
;   FCOMP  = $BC5B  -- A = sign(FAC1 - *(A=lo,Y=hi)): 0, 1, or $FF(-1)
;   GIVAYF = $B391  -- FAC1 = signed 16-bit int, A=high byte, Y=low byte
;   FAC1YA = $B1AA  -- Y=low/A=high of FAC1 truncated to a 16-bit int
;                      (codebase64 calls this "fac1ya"; a second ROM entry,
;                      $B1BF/"ayint", does the same conversion but leaves the
;                      result only at $64-$65, not in registers -- $B1AA is
;                      used here since it hands the result back directly).
;                      Floors (rounds toward negative infinity, matching
;                      BASIC's INT()), NOT C#'s (int) cast semantics
;                      (truncate toward zero) -- a negative value with a
;                      fractional part converts one lower than C# would
;                      (e.g. (int)(-3.5f) is -3 in C#, but this produces
;                      -4). Also has its own "range check": a value outside
;                      16-bit signed range jumps into the ROM's normal BASIC
;                      error-handling path, which this freestanding program
;                      never installed a handler for -- behavior in that
;                      case is unverified, not guarded against here.
;
; Real subroutines (jsr'd from the thin macros below), not inlined macros --
; see asm/helper/floatBanking.asm's comment for why this matters: the
; bank-in/call-ROM/bank-out sequence must live at one fixed, stable,
; guaranteed-below-$a000 address, never duplicated at arbitrary call sites
; (this compiler's own generated code is not confined below $a000).
; ============================================================================

MOVFM  = $BBA2
MOVMF  = $BBD4
FADD   = $B867
FSUB   = $B850
FMULT  = $BA28
FDIV   = $BB0F
MOVEF  = $BBFC
FCOMP  = $BC5B
GIVAYF = $B391
FAC1YA = $B1AA
; FOUT: FAC1 -> null-terminated decimal text, returned as A=lo/Y=hi of the
; ROM's OWN buffer. Confirmed empirically (not from documentation, same
; discipline as FDIV's note above) via a throwaway SimpleEmulator diagnostic
; against the real embedded ROM image: that buffer is $0100 (the low end of
; the 6502 hardware stack page) for every value tried, and FOUT prepends a
; single leading space for non-negative numbers only (classic BASIC PRINT's
; reserved sign column -- e.g. "5" comes back as " 5", "-5" comes back as
; "-5" with no extra space). Float_ToString below strips that space and
; copies the result out before returning.
FOUT   = $BDDD
ROUND  = $BC1B   ; FAC1 rounding, the first thing MOVMF does

; Replacements for the ROM's MOVFM/MOVMF calls, which cost ~80 and ~110 cycles
; and dominated the per-operation overhead of Float_Add etc. (see the
; byte-for-byte equivalence test in SimpleEmulator.Test's FloatWrapperTests).
; Real subroutines rather than macros: float.asm is always included, and the
; inline copies at every call site cost ~360 bytes for a ~12 cycle saving.
; They work on this compiler's own zero-page MFLPT copy (5 bytes: exponent,
; then four mantissa bytes with the sign in bit 7 of the first one) and the
; ROM's unpacked FAC1 ($61 exponent, $62-$65 mantissa with the implied
; leading 1 made explicit, $66 sign, $70 rounding byte).
;
; Float_LoadFac1_A/_B = MOVFM: FAC1 = zp_flt_a/zp_flt_b. $66 gets the raw
; first mantissa byte (only its bit 7, the sign, is ever looked at), $62 gets
; it with bit 7 forced on, and the rounding byte $70 is cleared -- exactly
; what the ROM does.
flt_load_fac1 .macro addr
    lda \addr+4
    sta $65
    lda \addr+3
    sta $64
    lda \addr+2
    sta $63
    lda \addr+1
    sta $66
    ora #$80
    sta $62
    lda \addr
    sta $61
    lda #0
    sta $70
    rts
.endm

Float_LoadFac1_A
    #flt_load_fac1 zp_flt_a
Float_LoadFac1_B
    #flt_load_fac1 zp_flt_b

; Float_StoreFac1_A = MOVMF: zp_flt_a = FAC1, rounded first (ROUND at $BC1B is
; the very routine MOVMF itself calls, and needs the ROM banked in). The sign
; is merged back into bit 7 of the first mantissa byte and $70 is left at 0.
Float_StoreFac1_A
    jsr ROUND
    lda $65
    sta zp_flt_a+4
    lda $64
    sta zp_flt_a+3
    lda $63
    sta zp_flt_a+2
    lda $66
    ora #$7f
    and $62
    sta zp_flt_a+1
    lda $61
    sta zp_flt_a
    lda #0
    sta $70
    rts


; Both operands already sit in zp_flt_a/zp_flt_b (the calling macro pulls
; them off this compiler's own evaluation stack first -- that doesn't touch
; $01/ROM banking, so it's safe to inline anywhere). Result left in zp_flt_a.
Float_Add
    #bank_in_basic_rom
    jsr Float_LoadFac1_A
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr FADD
    jsr Float_StoreFac1_A
    #bank_out_basic_rom
    rts

; a - b: FSUB computes memory-operand minus FAC1, so FAC1 is loaded with b
; first and the ROM call points at a.
Float_Sub
    #bank_in_basic_rom
    jsr Float_LoadFac1_B
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr FSUB
    jsr Float_StoreFac1_A
    #bank_out_basic_rom
    rts

Float_Mul
    #bank_in_basic_rom
    jsr Float_LoadFac1_A
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr FMULT
    jsr Float_StoreFac1_A
    #bank_out_basic_rom
    rts

; a / b: FDIV computes memory-operand divided by FAC1, so FAC1 is loaded
; with b (the divisor) first and the ROM call points at a.
; a / b. Per c64-wiki.de's FDIV page (the one source found with a complete,
; unambiguous calling convention and a worked example): FAC1 must hold the
; divisor (b, pre-loaded via MOVFM) *before* the call, and A/Y points at
; the dividend (a) in memory; the result is left directly in FAC1, no
; MOVEF needed (that was an earlier, wrong guess -- see below).
;
; Was briefly suspected broken (a compiled divflt read back 0 instead of
; the quotient under SimpleEmulator, while confirmed correct on real VICE)
; -- root cause found and fixed: SimpleEmulator's zero-page,X/zero-page,Y/
; (zp,X)/(zp),Y addressing never wrapped within page 0 like real 6502
; hardware does (see SimpleEmulator/Emulator.cs's GetAddress). FDIV's own
; internal long-division loop computes its quotient-byte scratch address
; that way (an offset that counts X down from a negative starting value),
; so the computed quotient silently landed outside the zero page and the
; routine's own "copy quotient into FAC1" step read back stale zeros in
; its place -- nothing to do with this call's operand order/convention,
; which was already correct.
Float_Div
    #bank_in_basic_rom
    jsr Float_LoadFac1_B
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr FDIV
    jsr Float_StoreFac1_A
    #bank_out_basic_rom
    rts

; Returns A = sign(a - b): 0 (a==b), 1 (a>b), $FF (a<b) -- FCOMP's own
; convention, passed straight through to the caller.
Float_Compare
    #bank_in_basic_rom
    jsr Float_LoadFac1_A
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr FCOMP
    #bank_out_basic_rom
    rts

; Input: 16-bit signed int in zp_flt_int_hi/zp_flt_int_lo (the calling
; macro sign- or zero-extends this compiler's 1-byte int/uint first).
; Output: 5-byte MFLPT result in zp_flt_a.
Float_FromInt
    #bank_in_basic_rom
    lda zp_flt_int_hi
    ldy zp_flt_int_lo
    jsr GIVAYF
    jsr Float_StoreFac1_A
    #bank_out_basic_rom
    rts

; Input: 5-byte MFLPT value in zp_flt_a. Output: low byte of the truncated
; result in A (this compiler's int/uint are 1 byte -- the high byte from
; FAC1YA is discarded, same silent-wraparound behavior every other 1-byte
; arithmetic op here already has for out-of-range values).
Float_ToInt
    #bank_in_basic_rom
    jsr Float_LoadFac1_A
    jsr FAC1YA
    #bank_out_basic_rom
    tya
    rts

; Input: 5-byte MFLPT value in zp_flt_a. Output: decimal text copied into
; tostring_buffer (asm/helper/objectTables.asm), null-terminated, FOUT's
; leading sign-column space stripped (see FOUT's comment above). Copies out
; of FOUT's own $0100 buffer before bank_out_basic_rom re-enables
; interrupts -- not otherwise protected against a pathologically deep call
; stack legitimately reaching down into page 1 by the time this runs (same
; class of accepted, undefended-against edge case as this codebase's other
; "not guarded against here" ROM notes, e.g. FAC1YA's range check above).
; Callable directly by NumberFormat_FloatToString below; also usable on its
; own by anything that already has FAC1 loaded and just wants the text.
;
; KNOWN ISSUE, unresolved: reliably crashes to BASIC when called live on
; VICE from a program with an ACTIVE C64.Interrupt subscriber registered
; (via C64_add_Interrupt) -- confirmed reproducible even with a completely
; empty handler, and even when the interrupt has no chance to fire during
; THIS call specifically (masking CIA1's own timer-A interrupt source,
; $DC0D, around the bank_in/FOUT/bank_out window here did NOT fix it --
; only "the interrupt is registered but never dispatches before this call
; runs" avoids the crash, e.g. reordering C64.Interrupt += ... to AFTER a
; ToString() call). Every other Float_* routine here (Add/Sub/Mul/Div/
; Compare/FromInt/ToInt) coexists with an active interrupt correctly, live,
; extensively verified (Demo's own sprite-moved-from-MoveBall + concurrent
; float-arithmetic demo). SimpleEmulator can't reproduce this at all (no
; interrupt-timer model), and an exhaustive zero-page survey of FOUT itself
; (SimpleEmulator.Test's RomFloatRoutineTests.FOUT_NeverTouches) found no
; clobbering of any zero page byte this codebase's interrupt machinery
; cares about (zp_param0-2, stackPointer, heapPointer, zp_ctor_result,
; zp_field_value_low/high, zp_interrupt_address_low/high). Root cause not
; found -- something specific to FOUT (a much longer, more complex ROM
; routine than any other Float_* dependency) interacting badly with
; OnInterrupt/the compiled-method calling convention (asm/helper/
; localsStack.asm's init_locals/method_exit, which relocates a method's
; real return address into localsStack, itself indexed by stackPointer)
; once an interrupt has dispatched at least once, ANYWHERE, before this
; runs -- not simply during this call's own execution window. Demo/
; Program.cs deliberately does NOT call float.ToString() for this reason
; (uint/int/ulong/long ToString() are unaffected and fully verified
; working alongside the active interrupt). Safe to use in a program with NO
; C64.Interrupt subscriber.
Float_ToString
    #bank_in_basic_rom
    jsr Float_LoadFac1_A
    jsr FOUT
    sta zp_param0_low
    sty zp_param0_high
    ldy #0
    ldx #0
    lda (zp_param0_low),y
    cmp #$20
    bne Float_ToString_CopyLoop
    iny
Float_ToString_CopyLoop:
    lda (zp_param0_low),y
    sta tostring_buffer,x
    beq Float_ToString_Done
    iny
    inx
    jmp Float_ToString_CopyLoop
Float_ToString_Done:
    #bank_out_basic_rom
    rts

; Real library entry point (the calling-convention wrapper: pull the
; argument off this compiler's own evaluation stack, push the result back)
; -- Float_ToString above is the reusable core, following the same split as
; Float_Add/mulflt etc. Not gated by the per-method dead-code-elimination
; mechanism (Compiler/ILLibraryFlagsPass.cs): asm/helper/*.asm is always
; included regardless of use, same as every other Float_* routine here.
NumberFormat_FloatToString
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_mflpt zp_flt_a
    jsr Float_ToString
    #stack_push_pointer tostring_buffer
    #stack_return_to_saved_address zp_tmp1_low

; No SEI/CLI here (or in any macro below) -- interrupt-time safety for
; zp_flt_a/b (and everything else these macros touch: the evaluation
; stack, whatever's in A/X/Y) is handled centrally by asm/C64.asm's
; OnInterrupt, which saves/restores the whole transient zero-page scratch
; range (see zeropage.asm's zp_interrupt_save_start) around every
; dispatch into a subscriber's code, instead of every macro defending
; itself. A macro-level SEI/CLI would also be actively wrong now that
; C64.Interrupt actually works: if this macro were ever expanded inside a
; compiled interrupt handler's own body, its `cli` would prematurely
; re-enable interrupts before that handler -- and OnInterrupt -- has
; returned. Float_Add/Sub/Mul/Div/Compare/FromInt/ToInt's own ROM-banked
; window still has its own protection (SEI/PLP, not SEI/CLI, precisely so
; it nests correctly if ever called from inside a handler) -- see
; floatBanking.asm's bank_in_basic_rom/bank_out_basic_rom.
addflt .macro
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Add
    #stack_push_var_mflpt zp_flt_a
.endm

subflt .macro
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Sub
    #stack_push_var_mflpt zp_flt_a
.endm

mulflt .macro
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Mul
    #stack_push_var_mflpt zp_flt_a
.endm

divflt .macro
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Div
    #stack_push_var_mflpt zp_flt_a
.endm

; No ROM call needed -- MFLPT's sign lives in bit 7 of the first mantissa
; byte (zp_flt_a+1, since zp_flt_a+0 is the exponent), so negation is a
; direct flip. Skipped when the exponent byte is 0 (canonical zero) so
; negating zero doesn't produce a non-canonical "negative zero" bit pattern
; (MFLPT's own zero is exponent-byte-only; a stray sign bit on an
; all-zero-exponent value isn't a value FCOMP/the ROM ever produces itself).
negateflt .macro
    #stack_pull_mflpt zp_flt_a
    lda zp_flt_a
    beq +
    lda zp_flt_a+1
    eor #$80
    sta zp_flt_a+1
+   #stack_push_var_mflpt zp_flt_a
.endm

compareEqualflt .macro
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bne +
    inx
+   #stack_push_int_x
.endm

compareLessflt .macro
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bpl +
    inx
+   #stack_push_int_x
.endm

compareGreaterflt .macro
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bmi +
    beq +
    inx
+   #stack_push_int_x
.endm

; Float has no real unsigned concept -- see branch.asm's identical comment
; on its own _unsignedflt macros for why these exist (Clt_un/Cgt_un's
; "unordered" distinction from Clt/Cgt can't arise since NaN is refused at
; compile time) and why they're identical to compareLessflt/
; compareGreaterflt above.
compareLess_unsignedflt .macro
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bpl +
    inx
+   #stack_push_int_x
.endm

compareGreater_unsignedflt .macro
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bmi +
    beq +
    inx
+   #stack_push_int_x
.endm

; Sign-extends the 1-byte int to 16 bits before handing it to Float_FromInt.
conv_int_to_float .macro
    #stack_pull_int_a
    sta zp_flt_int_lo
    ldy #0
    cmp #0
    bpl +
    dey
+   sty zp_flt_int_hi
    jsr Float_FromInt
    #stack_push_var_mflpt zp_flt_a
.endm

; Zero-extends instead of sign-extending -- the only difference from
; conv_int_to_float above.
conv_uint_to_float .macro
    #stack_pull_int_a
    sta zp_flt_int_lo
    lda #0
    sta zp_flt_int_hi
    jsr Float_FromInt
    #stack_push_var_mflpt zp_flt_a
.endm

conv_float_to_int .macro
    #stack_pull_mflpt zp_flt_a
    jsr Float_ToInt
    #stack_push_int_a
.endm

; ============================================================================
; System.MathF.Sin/Cos/Sqrt -- the REAL BCL type (not a C64Lib stand-in):
; ILLibraryUsagePass has a narrow, explicit allowlist for exactly these
; three MathF methods (same technique as its existing System.GC.Collect()
; and Func<T>.Invoke() special cases -- see Operands/OperandBase.cs's
; EnsureCallIsResolvable), so calling e.g. MathF.Tan (unimplemented) is a
; clear compiler error, not a confusing 64tass "not defined symbol" one.
;
; Unlike every Float_* routine above (always included: float's +/-/*/</>
; are raw IL opcodes the dead-code-elimination scan can't see a method call
; for, so it has no way to know a program doesn't need them), a MathF.Sin/
; Cos/Sqrt call IS an ordinary Call the usage scan can see -- exactly what
; ILLibraryUsagePass/ILLibraryFlagsPass already track for every C64Lib
; method (Screen_BeginUpdate etc. in asm/C64.asm). So these three get the
; same per-method .weak Flag_X / .if guard those use, even though they live
; here in float.asm rather than one of the files ILLibraryFlagsPass's own
; comment lists -- that list isn't a hard restriction, just where the
; pattern happened to be used before now; the mechanism itself is a plain
; 64tass .weak symbol, readable from anywhere. Result: a program that calls
; MathF.Sqrt but never MathF.Sin pays for SQR's ROM call and not SIN's.
;
; Each is fully self-contained inside its own .if block (no shared
; "core" routine split the way Float_Add/Float_ToString have,
; deliberately: unlike those, nothing else ever calls into MathF.Sin's
; implementation, so splitting it out would only move code outside the
; .weak guard and defeat the point). #bank_in_basic_rom/#bank_out_basic_rom
; used directly here is safe for the same reason floatBanking.asm's own
; comment restricts them to "Float_* subroutines" in the first place: each
; is a real, once-defined subroutine at a fixed, stable, guaranteed-below-
; $a000 address (this file's own include position), not a macro expanded
; at scattered call sites -- the label prefix isn't what that restriction
; is actually about.
;
; ROM entry points (verified against c64-wiki.com's SIN and BASIC-ROM
; overview pages, cross-checked, September 2026):
;   SQR = $BF71 -- FAC1 = sqrt(FAC1), in place. Lives in BASIC ROM's own
;                  $A000-$BFFF range, alongside SGN/ABS/INT/LOG/EXP, all
;                  documented with the same "evaluate the argument into
;                  FAC1, JSR, result left in FAC1" convention MOVFM/MOVMF
;                  below implement explicitly.
;   SIN = $E26B -- FAC1 = sin(FAC1), in place. This address falls in what's
;                  normally called the KERNAL range ($E000-$FFFF), but
;                  #bank_in_basic_rom's $07 write to $01 maps BOTH BASIC
;                  ($A000-$BFFF) AND KERNAL ($E000-$FFFF) ROM at once (the
;                  C64's default/normal banking state) -- the exact same
;                  banking call already used for FOUT (confirmed at $BDDD,
;                  BASIC ROM's own range) works unchanged for SIN.
;   COS = $E264 -- FAC1 = cos(FAC1), in place. Documented as simply adding
;                  a quarter-turn constant to FAC1 and falling straight
;                  into SIN's own code -- consistent with the two entry
;                  points sitting only 7 bytes apart.
;
; KNOWN ISSUE, unresolved, same unexplained class as Float_ToString/FOUT's
; own "KNOWN ISSUE" comment above: confirmed reproducible live on VICE that
; calling any of MathF.Sin/Cos/Sqrt from a program with an ACTIVE
; C64.Interrupt subscriber registered crashes to BASIC (Demo/Program.cs,
; which has exactly one such subscriber for its sprite-moving MoveBall --
; adding a single MathF.Sqrt call to its existing float-arithmetic demo
; reproduced this every time; removing the C64.Interrupt += MoveBall line,
; with the MathF.Sqrt call otherwise unchanged, made it run correctly, which
; is what isolates the interrupt subscriber specifically rather than
; something about the call site). Same suspected cause as FOUT's own
; unresolved case: SIN/SQR/COS are, like FOUT, much longer and more complex
; ROM routines than the simple Float_Add/Sub/Mul/Div/Compare/FromInt/ToInt
; family, which all coexist correctly with an active interrupt. Not
; independently root-caused here -- filed as the same open class of
; problem rather than reinvestigated from scratch. Safe to use in a program
; with NO C64.Interrupt subscriber (this is how Test/FloatTests.cs's
; TestSin/TestCos/TestSqrt exercise it -- via SimpleEmulator, which, per
; Float_ToString's own comment, can't reproduce this at all anyway, so
; those tests only ever verified correctness, never this interrupt
; interaction).
; ============================================================================

SIN = $E26B
COS = $E264
SQR = $BF71

.weak
Flag_MathF_Sin = 0
.endweak
.if Flag_MathF_Sin

MathF_Sin
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_mflpt zp_flt_a
    #bank_in_basic_rom
    jsr Float_LoadFac1_A
    jsr SIN
    jsr Float_StoreFac1_A
    #bank_out_basic_rom
    #stack_push_var_mflpt zp_flt_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_MathF_Cos = 0
.endweak
.if Flag_MathF_Cos

MathF_Cos
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_mflpt zp_flt_a
    #bank_in_basic_rom
    jsr Float_LoadFac1_A
    jsr COS
    jsr Float_StoreFac1_A
    #bank_out_basic_rom
    #stack_push_var_mflpt zp_flt_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_MathF_Sqrt = 0
.endweak
.if Flag_MathF_Sqrt

MathF_Sqrt
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_mflpt zp_flt_a
    #bank_in_basic_rom
    jsr Float_LoadFac1_A
    jsr SQR
    jsr Float_StoreFac1_A
    #bank_out_basic_rom
    #stack_push_var_mflpt zp_flt_a
    #stack_return_to_saved_address zp_tmp1_low
.endif
