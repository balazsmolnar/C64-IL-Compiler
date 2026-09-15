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

; Both operands already sit in zp_flt_a/zp_flt_b (the calling macro pulls
; them off this compiler's own evaluation stack first -- that doesn't touch
; $01/ROM banking, so it's safe to inline anywhere). Result left in zp_flt_a.
Float_Add
    #bank_in_basic_rom
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVFM
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr FADD
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
    #bank_out_basic_rom
    rts

; a - b: FSUB computes memory-operand minus FAC1, so FAC1 is loaded with b
; first and the ROM call points at a.
Float_Sub
    #bank_in_basic_rom
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr MOVFM
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr FSUB
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
    #bank_out_basic_rom
    rts

Float_Mul
    #bank_in_basic_rom
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVFM
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr FMULT
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
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
    lda #<zp_flt_b
    ldy #>zp_flt_b
    jsr MOVFM
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr FDIV
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
    #bank_out_basic_rom
    rts

; Returns A = sign(a - b): 0 (a==b), 1 (a>b), $FF (a<b) -- FCOMP's own
; convention, passed straight through to the caller.
Float_Compare
    #bank_in_basic_rom
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVFM
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
    ldx #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVMF
    #bank_out_basic_rom
    rts

; Input: 5-byte MFLPT value in zp_flt_a. Output: low byte of the truncated
; result in A (this compiler's int/uint are 1 byte -- the high byte from
; FAC1YA is discarded, same silent-wraparound behavior every other 1-byte
; arithmetic op here already has for out-of-range values).
Float_ToInt
    #bank_in_basic_rom
    lda #<zp_flt_a
    ldy #>zp_flt_a
    jsr MOVFM
    jsr FAC1YA
    #bank_out_basic_rom
    tya
    rts

; SEI/CLI wrap the whole macro (operand pulls through result push), not
; just the ROM call inside Float_Add -- see floatBanking.asm's comment.
addflt .macro
    sei
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Add
    #stack_push_var_mflpt zp_flt_a
    cli
.endm

subflt .macro
    sei
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Sub
    #stack_push_var_mflpt zp_flt_a
    cli
.endm

mulflt .macro
    sei
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Mul
    #stack_push_var_mflpt zp_flt_a
    cli
.endm

divflt .macro
    sei
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Div
    #stack_push_var_mflpt zp_flt_a
    cli
.endm

; No ROM call needed -- MFLPT's sign lives in bit 7 of the first mantissa
; byte (zp_flt_a+1, since zp_flt_a+0 is the exponent), so negation is a
; direct flip. Skipped when the exponent byte is 0 (canonical zero) so
; negating zero doesn't produce a non-canonical "negative zero" bit pattern
; (MFLPT's own zero is exponent-byte-only; a stray sign bit on an
; all-zero-exponent value isn't a value FCOMP/the ROM ever produces itself).
; No ROM call, so no banking risk -- SEI/CLI added anyway for the same
; "no interrupt in the middle of a float operation" reasoning as every
; other macro here, in case an interrupt handler that reads zp_flt_a
; mid-flip (not something the default KERNAL handler does, but this is
; cheap insurance either way) is ever a factor.
negateflt .macro
    sei
    #stack_pull_mflpt zp_flt_a
    lda zp_flt_a
    beq +
    lda zp_flt_a+1
    eor #$80
    sta zp_flt_a+1
+   #stack_push_var_mflpt zp_flt_a
    cli
.endm

compareEqualflt .macro
    sei
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bne +
    inx
+   #stack_push_int_x
    cli
.endm

compareLessflt .macro
    sei
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bpl +
    inx
+   #stack_push_int_x
    cli
.endm

compareGreaterflt .macro
    sei
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bmi +
    beq +
    inx
+   #stack_push_int_x
    cli
.endm

; Float has no real unsigned concept -- see branch.asm's identical comment
; on its own _unsignedflt macros for why these exist (Clt_un/Cgt_un's
; "unordered" distinction from Clt/Cgt can't arise since NaN is refused at
; compile time) and why they're identical to compareLessflt/
; compareGreaterflt above.
compareLess_unsignedflt .macro
    sei
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bpl +
    inx
+   #stack_push_int_x
    cli
.endm

compareGreater_unsignedflt .macro
    sei
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    ldx #0
    cmp #0
    bmi +
    beq +
    inx
+   #stack_push_int_x
    cli
.endm

; Sign-extends the 1-byte int to 16 bits before handing it to Float_FromInt.
conv_int_to_float .macro
    sei
    #stack_pull_int_a
    sta zp_flt_int_lo
    ldy #0
    cmp #0
    bpl +
    dey
+   sty zp_flt_int_hi
    jsr Float_FromInt
    #stack_push_var_mflpt zp_flt_a
    cli
.endm

; Zero-extends instead of sign-extending -- the only difference from
; conv_int_to_float above.
conv_uint_to_float .macro
    sei
    #stack_pull_int_a
    sta zp_flt_int_lo
    lda #0
    sta zp_flt_int_hi
    jsr Float_FromInt
    #stack_push_var_mflpt zp_flt_a
    cli
.endm

conv_float_to_int .macro
    sei
    #stack_pull_mflpt zp_flt_a
    jsr Float_ToInt
    #stack_push_int_a
    cli
.endm
