; "_tail" macros: everything AFTER an op's own operand-pulling, unchanged
; from the combined macros below -- split out purely so
; ILMethodIntOperandFusionOptimizer's fused operand classes
; (Compiler/Operands/OperandIntFusion.cs) can reuse the exact same
; computation after copying each operand directly into place themselves,
; without duplicating this logic. A zero-cost refactor: the non-fused
; macros below still just chain straight into their own "_tail", so their
; expansion is byte-for-byte identical to before.
add16_tail .macro
    clc
    lda zp_param2_low
    adc zp_param1_low
    sta zp_param2_low
    lda zp_param2_high
    adc zp_param1_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

add16 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high

    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    #add16_tail
.endm

add8_tail .macro
    clc
    adc zp_param1_low
    #stack_push_tos
.endm

add8 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int_a

    #add8_tail
.endm

sub8_tail .macro
    sec
    sbc zp_param1_low
    #stack_push_tos
.endm

sub8 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int_a

    #sub8_tail
.endm

sub16_tail .macro
    sec
    lda zp_param2_low
    sbc zp_param1_low
    sta zp_param2_low
    lda zp_param2_high
    sbc zp_param1_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

sub16 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high

    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    #sub16_tail
.endm

; The 6502 has no hardware multiply -- both of these are the standard
; shift-and-add algorithm, unrolled into a counted loop. They only ever
; produce the *truncated* low N bits of the product (8 bits for mul8, 16
; for mul16), which is all this compiler's arithmetic ever keeps anyway
; (see add8/sub8 above discarding carry the same way). Truncated
; multiplication is bit-identical for signed and unsigned operands (it's
; modular arithmetic mod 2^N either way), so one implementation serves
; both int and uint -- no _unsigned variant needed, unlike the compares.
mul8_tail .macro
    jsr Multiply8                  ; asm/helper/division.asm
    #stack_push_tos
.endm

mul8 .macro
    #stack_pull_int zp_param1_low  ; one operand, shifted right a bit at a time
    #stack_pull_int_a
    sta zp_param2_low              ; other operand, doubled a bit at a time
    #mul8_tail
.endm

; Needs a genuine third register pair for the accumulator: zp_param1 and
; zp_param2 both stay busy all the way through the loop (one shifting
; right for the next bit, one doubling for the next partial product), so
; unlike mul8 the running total can't just live in A. zp_param0 isn't
; used anywhere else in arithmetic.asm -- see zeropage.asm's ownership
; comment for this macro's entry there.
mul16_tail .macro
    jsr Multiply16                 ; asm/helper/division.asm
    lda zp_param0_high
    #stack_push_int_a
    #stack_push_var zp_param0_low
.endm

mul16 .macro
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    #mul16_tail
.endm

; Compile-time-constant multiply by a power of two (ILMethodMulConstOptimizer
; only ever emits shift=0/1/2, for x*1/x*2/x*4), as a fixed, unrolled shift
; with no runtime loop at all -- unlike #shift_left, whose count is a
; runtime stack value it has to loop on.
mul_shift_const8 .macro shift
    #stack_pull_int_a
    .if \shift > 0
    .rept \shift
        asl
    .next
    .endif
    #stack_push_tos
.endm

mul_shift_const16 .macro shift
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    .if \shift > 0
    .rept \shift
        asl zp_param1_low
        rol zp_param1_high
    .next
    .endif
    lda zp_param1_high
    #stack_push_int_a
    #stack_push_var zp_param1_low
.endm

; Unsigned divide / remainder by a constant 3..255 that is not a power of
; two (ILMethodDivConstOptimizer). Each divisor used gets its own routine,
; DivConst8_<d> / DivConst16_<d>, emitted once into generated.asm by the
; compiler (div_const8_body / div_const16_body below): a shift-subtract loop
; with the divisor as an immediate operand, the quotient bits rotated into
; the dividend's own bytes, and the first bits of the dividend -- which can
; never reach the divisor -- shifted in without a compare. About 40% faster
; than the general Divide16Core. Quotient in zp_param2, remainder in A.
div_const8 .macro d
    #stack_pull_int zp_param2_low
    jsr DivConst8_\d
    #stack_push_var zp_param2_low
.endm

rem_const8 .macro d
    #stack_pull_int zp_param2_low
    jsr DivConst8_\d
    #stack_push_tos
.endm

div_const16 .macro d
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high
    jsr DivConst16_\d
    lda zp_param2_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

rem_const16 .macro d
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high
    jsr DivConst16_\d
    tax
    lda #0
    #stack_push_int_a
    txa
    #stack_push_int_a
.endm

; One byte of the dividend (\byte, in zero page) through the loop: A holds
; the running remainder (< d) on entry and exit, C must be clear on entry
; and is clear on exit, \byte ends up holding that byte of the quotient.
; \skip leading bits are only shifted in: valid while the remainder is still
; known to stay below d (the top bits of the dividend's first byte).
div_const_byte .macro byte, d, skip
    .if \skip > 0
    ldx #\skip
-   asl \byte
    rol a
    dex
    bne -
    .endif
    ldx #8 - \skip
-   rol \byte               ; next dividend bit out, last quotient bit in
    rol a
    bcs +                   ; 9-bit remainder: certainly >= d
    cmp #\d
    bcc ++                  ; quotient bit 0 (C clear)
+   sbc #\d
    sec                     ; quotient bit 1
+   dex
    bne -
    rol \byte               ; the last quotient bit (C ends clear: it was 0 on entry)
.endm

div_const8_body .macro d, skip
    lda #0
    clc
    #div_const_byte zp_param2_low, \d, \skip
    rts
.endm

div_const16_body .macro d, skip
    lda #0
    clc
    #div_const_byte zp_param2_high, \d, \skip
    #div_const_byte zp_param2_low, \d, 0
    rts
.endm

; Unsigned remainder by a power of two up to 256: a mask.
rem_mask8 .macro mask
    #stack_pull_int_a
    and #\mask
    #stack_push_tos
.endm

rem_mask16 .macro mask
    #stack_pull_int_a
    and #<\mask
    tax
    #stack_pull_int_a           ; the high byte: always 0 for a mask below 256
    lda #0
    #stack_push_int_a
    txa
    #stack_push_int_a
.endm

; Compile-time-constant UNSIGNED divide by a power of two
; (ILMethodDivConstOptimizer only ever emits shift=0/1/2, for
; x/1, x/2, x/4), as a fixed, unrolled logical right shift -- same
; mirrored-direction idea as mul_shift_const8/16 above, but ONLY valid
; for unsigned operands: lsr/ror zero-fill from the top regardless of
; the operand's own sign bit, which matches truncating division exactly
; for an unsigned value (no sign to get wrong), but silently corrupts a
; negative signed one into an unrelated large positive number instead
; (not merely a rounding difference). ILMethodDivConstOptimizer's own
; comment is the single source of truth for why it only ever matches
; Div_un, never the signed Div, for exactly this reason.
div_shift_const8 .macro shift
    #stack_pull_int_a
    .if \shift > 0
    .rept \shift
        lsr
    .next
    .endif
    #stack_push_tos
.endm

div_shift_const16 .macro shift
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    .if \shift > 0
    .rept \shift
        lsr zp_param1_high
        ror zp_param1_low
    .next
    .endif
    lda zp_param1_high
    #stack_push_int_a
    #stack_push_var zp_param1_low
.endm

negate16 .macro

    ; 16-bit pull/push (stack_pull_int/stack_push_var are the 8-bit ones: with
    ; them a long's high byte never came off the stack, so -x was wrong)
    #stack_pull_int16 zp_param2_low

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
    #stack_push_var16 zp_param2_low
.endm

; Bitwise complement (C#'s ~x). Like every macro here it costs nothing unless a
; program actually uses it.
not8 .macro
    #stack_pull_int_a
    eor #$FF
    #stack_push_tos
.endm

not16 .macro
    #stack_pull_int16 zp_param2_low
    lda zp_param2_high
    eor #$FF
    sta zp_param2_high
    lda zp_param2_low
    eor #$FF
    sta zp_param2_low
    #stack_push_var16 zp_param2_low
.endm

negate8 .macro

    #stack_pull_int_a

    eor #$FF
    clc
    adc #$1
    #stack_push_tos
.endm

compareLess16 .macro
; signed 16-bit a < b. zp_param2_low/zp_param2_high = a (low/high), zp_param1_low/zp_param1_high = b (low/high).
; Full 16-bit subtract (a - b) via chained CMP/SBC, with the standard
; overflow-corrected N-flag check for a signed result (a plain CMP/BCC
; chain, as used by the _unsigned16 variants below, is only valid for
; unsigned comparison). Relies on SBC's V flag being computed correctly
; (see SimpleEmulator/Emulator.cs ADC/SBC cases, fixed alongside this).
        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high

        ldx #0
        lda zp_param2_low
        cmp zp_param1_low
        lda zp_param2_high
        sbc zp_param1_high
        bvc +
        eor #$80
+       bpl l1
        inx
l1
        #stack_push_int_x
.endm

compareGreater16 .macro
; signed 16-bit a > b, implemented as signed (b - a) < 0. See compareLess16.
        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high

        ldx #0
        lda zp_param1_low
        cmp zp_param2_low
        lda zp_param1_high
        sbc zp_param2_high
        bvc +
        eor #$80
+       bpl l1
        inx
l1
        #stack_push_int_x
.endm

compareGreater_unsigned16 .macro

        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high

        ldx #0
        lda zp_param1_high
        cmp zp_param2_high
        bcc +
        bne l1
        lda zp_param1_low
        cmp zp_param2_low
        bcs l1
+       inx
l1      
        #stack_push_int_x
.endm

compareLess_unsigned16 .macro

        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high
        ; #stack_pull_int zp_param2_high ends on "pla / sta zp_param2_high" -- A already holds
        ; that value, so no need to reload it before the compare below.

        ldx #0
        cmp zp_param1_high
        bcc +
        bne l1
        lda zp_param2_low
        cmp zp_param1_low
        bcs l1
+       inx
l1      
        #stack_push_int_x
.endm

compareLess8 .macro 
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bpl +
        inx
+        txa
         #stack_push_tos
.endm

compareLess_unsigned8 .macro 
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bcs +
        inx
+        txa
         #stack_push_tos
.endm

compareGreater8 .macro 
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bmi +
        beq +
        inx
+       txa
        #stack_push_tos
.endm

compareGreater_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bmi +
        beq +
        inx
+       txa
        #stack_push_tos
.endm

compareGreater_unsigned8 .macro 
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bcc +
        beq +
        inx
+       txa
        #stack_push_tos
.endm

compareGreater_unsigned_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bcc +
        beq +
        inx
+       txa
        #stack_push_tos
.endm

compareLess_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bpl +
        inx
+       txa
        #stack_push_tos
.endm

compareLess_unsigned_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bcs +
        inx
+       txa
        #stack_push_tos
.endm

compareEqual16 .macro 
        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high

        ldx #0
        lda zp_param1_low
        cmp zp_param2_low
        bne +
        lda zp_param1_high
        cmp zp_param2_high
        bne +
        inx
+
        #stack_push_int_x 
.endm

compareEqual8 .macro
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bne +
        inx
+
        txa
        #stack_push_tos
.endm

compareEqual_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bne +
        inx
+
        txa
        #stack_push_tos
.endm

shift_left8 .macro 

    #stack_pull_int_x
    #stack_pull_int_a
    
-   asl
    dex
    bne -
    #stack_push_int_a
.endm

shift_right8 .macro 

    #stack_pull_int_x
    #stack_pull_int_a
    
-   lsr
    dex
    bne -
    #stack_push_int_a
.endm

; And/Or/Xor. The "_tail" macros (first operand in A, second in
; zp_param1_low; 16-bit: first in zp_param2_low/high, second in
; zp_param1_low/high) are what ILMethodIntOperandFusionOptimizer's fused
; operand copies (OpFusedIntBinOp) chain into, same as add8_tail above.
;
; The "_const" macros take the second operand as an assembly-time constant
; (OpFusedIntConstOp): "and8_const c" works on the value already on the
; stack (pla / and #c / pha: 4 bytes instead of pushing c and calling and8,
; 10), "and8_const_tail c" on a first operand already in A. and_imm8 &
; co. are the byte operation itself: an identity constant (and #$ff, ora
; #0, eor #0) emits nothing and an absorbing one (and #0, ora #$ff) a plain
; lda, so the 16-bit forms only touch the bytes the constant changes.

and_imm8 .macro value
  .if ((\value) & $ff) == 0
    lda #0
  .elsif ((\value) & $ff) != $ff
    and #<(\value)
  .endif
.endm

ora_imm8 .macro value
  .if ((\value) & $ff) == $ff
    lda #$ff
  .elsif ((\value) & $ff) != 0
    ora #<(\value)
  .endif
.endm

eor_imm8 .macro value
  .if ((\value) & $ff) != 0
    eor #<(\value)
  .endif
.endm

; A byte the constant leaves unchanged: and with $ff, or/xor with 0.
logic_identity8 .function op, value
.endf (op == 0 && ((value) & $ff) == $ff) || (op != 0 && ((value) & $ff) == 0)

; Shared body of and16_const/or16_const/xor16_const (op: 0 and, 1 or,
; 2 xor). The value's low byte is on top of the stack, its high byte below
; it; only the bytes the constant changes are touched.
logic16_const .macro op, value
  .if logic_identity8(\op, (\value) >> 8)
    ; high byte unchanged: only the top byte
    pla
    #logic_imm8 \op, \value
    pha
  .elsif logic_identity8(\op, \value)
    ; low byte unchanged: only the byte below the top
    tsx
    .if (\op == 0 && ((\value) & $ff00) == 0) || (\op == 1 && ((\value) & $ff00) == $ff00)
      #logic_imm8 \op, (\value) >> 8
    .else
      lda $102,x
      #logic_imm8 \op, (\value) >> 8
    .endif
    sta $102,x
  .else
    pla
    #logic_imm8 \op, \value
    sta zp_param1_low
    pla
    #logic_imm8 \op, (\value) >> 8
    pha
    lda zp_param1_low
    pha
  .endif
.endm

logic_imm8 .macro op, value
  .if \op == 0
    #and_imm8 \value
  .elsif \op == 1
    #ora_imm8 \value
  .else
    #eor_imm8 \value
  .endif
.endm

and8_tail .macro
    and zp_param1_low
    #stack_push_tos
.endm

and8 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int_a

    #and8_tail
.endm

and8_const_tail .macro value
    #and_imm8 \value
    #stack_push_tos
.endm

and8_const .macro value
    #stack_pull_int_a
    #and8_const_tail \value
.endm

and16_tail .macro
    lda zp_param2_low
    and zp_param1_low
    sta zp_param2_low
    lda zp_param2_high
    and zp_param1_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

; and8 had no 16-bit counterpart before Or/Xor were added alongside it here --
; `long & long`/`ulong & ulong` would have hit the exact same undefined-
; symbol assembler error the missing or16/xor16 would have, so this closes
; that gap too while giving And the same 8/16 coverage Or/Xor get below.
and16 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high

    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    #and16_tail
.endm

and16_const .macro value
    #logic16_const 0, \value
.endm

or8_tail .macro
    ora zp_param1_low
    #stack_push_tos
.endm

or8 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int_a

    #or8_tail
.endm

or8_const_tail .macro value
    #ora_imm8 \value
    #stack_push_tos
.endm

or8_const .macro value
    #stack_pull_int_a
    #or8_const_tail \value
.endm

or16_tail .macro
    lda zp_param2_low
    ora zp_param1_low
    sta zp_param2_low
    lda zp_param2_high
    ora zp_param1_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

or16 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high

    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    #or16_tail
.endm

or16_const .macro value
    #logic16_const 1, \value
.endm

xor8_tail .macro
    eor zp_param1_low
    #stack_push_tos
.endm

xor8 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int_a

    #xor8_tail
.endm

xor8_const_tail .macro value
    #eor_imm8 \value
    #stack_push_tos
.endm

xor8_const .macro value
    #stack_pull_int_a
    #xor8_const_tail \value
.endm

xor16_tail .macro
    lda zp_param2_low
    eor zp_param1_low
    sta zp_param2_low
    lda zp_param2_high
    eor zp_param1_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

xor16 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high

    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    #xor16_tail
.endm

xor16_const .macro value
    #logic16_const 2, \value
.endm

conv_8_16 .macro

    #stack_pull_int_x
    lda #0
    stack_push_int_a
    txa
    #stack_push_tos

.endm

conv_16_8 .macro

    #stack_pull_int_x
    #stack_pull_int_a
    txa
    #stack_push_tos
.endm