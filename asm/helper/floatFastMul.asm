; ---------------------------------------------------------------------------
; Optional fast float multiply (see Flag_FastFloatMul in float.asm).
; ---------------------------------------------------------------------------
; The BASIC ROM's FMULT multiplies bit by bit (~2000 cycles). This builds the
; 32x32-bit mantissa product from 13 byte*byte partial products instead (the
; 3 lowest-order ones, which can only nudge the rounding byte, are dropped),
; using quarter-square tables: a*b = f(a+b) - f(a-b), f(n) = n*n/4. No ROM
; call, no banking. Measured in an emulator experiment over 1500 random
; operand pairs: 1126 cycles per multiply vs 2407 for Float_Mul_ROM (2.1x);
; bit-identical to the ROM's result 99.7% of the time and 1 ulp off otherwise
; (99.9% exactly rounded; the ROM is 100%). In the rotating-cube demo that is
; ~18% faster per frame. NOT covered by the regular test suite yet.
; Costs ~670 bytes of code and 2 KB of page-aligned tables, hence opt-in;
; kept as a reference implementation, the ROM path stays the default.
; Not yet wired into the program entry templates (ProgramEntry.asm): to try
; it, set Flag_FastFloatMul = 1 and .include this file after the generated
; code, before objectTables.asm.
; Exponent overflow/underflow falls back to the ROM path (Float_Mul_ROM), so
; runtime fault reporting is unchanged. Scratch is $54-$66: dead outside the
; routine, so it may overlap the ROM's own scratch.
fm_a3 = $54
fm_a2 = $55
fm_a1 = $56
fm_a0 = $57
fm_b3 = $58
fm_b2 = $59
fm_b1 = $5a
fm_b0 = $5b
fm_acc = $5c      ; 6 bytes: product bytes 2..7 (fm_acc+5 = most significant)
fm_lo = $62
fm_hi = $63
fm_sign = $64
fm_exp = $65      ; 16 bit

Float_Mul_Fast
    lda zp_flt_a
    beq FM_zero
    lda zp_flt_b
    beq FM_zero
    lda zp_flt_a+1
    eor zp_flt_b+1
    and #$80
    sta fm_sign
    clc
    lda zp_flt_a
    adc zp_flt_b
    sta fm_exp
    lda #0
    rol
    sta fm_exp+1
    lda zp_flt_a+1
    ora #$80
    sta fm_a3
    lda zp_flt_a+2
    sta fm_a2
    lda zp_flt_a+3
    sta fm_a1
    lda zp_flt_a+4
    sta fm_a0
    lda zp_flt_b+1
    ora #$80
    sta fm_b3
    lda zp_flt_b+2
    sta fm_b2
    lda zp_flt_b+3
    sta fm_b1
    lda zp_flt_b+4
    sta fm_b0
    lda #0
    sta fm_acc
    sta fm_acc+1
    sta fm_acc+2
    sta fm_acc+3
    sta fm_acc+4
    sta fm_acc+5

fm_setup .macro src
    lda \src
    sta FM_sm1+1
    sta FM_sm3+1
    eor #$ff
    sta FM_sm2+1
    sta FM_sm4+1
.endm

; add the 16-bit product (fm_lo, A=hi) into column k (k = 0..3)
fm_term .macro col, mult
    ldx \mult
    jsr FM_Mul8
    sta fm_hi
    lda fm_lo
    clc
    adc fm_acc+\col
    sta fm_acc+\col
    lda fm_hi
    adc fm_acc+\col+1
    sta fm_acc+\col+1
.if \col < 4
    bcc +
    inc fm_acc+\col+2
.if \col < 3
    bne +
    inc fm_acc+\col+3
.if \col < 2
    bne +
    inc fm_acc+\col+4
.if \col < 1
    bne +
    inc fm_acc+5
.endif
.endif
.endif
.endif
+
.endm

    #fm_setup fm_a3
    #fm_term 4, fm_b3
    #fm_term 3, fm_b2
    #fm_term 2, fm_b1
    #fm_term 1, fm_b0
    #fm_setup fm_a2
    #fm_term 3, fm_b3
    #fm_term 2, fm_b2
    #fm_term 1, fm_b1
    #fm_term 0, fm_b0
    #fm_setup fm_a1
    #fm_term 2, fm_b3
    #fm_term 1, fm_b2
    #fm_term 0, fm_b1
    #fm_setup fm_a0
    #fm_term 1, fm_b3
    #fm_term 0, fm_b2

    ; normalize: product of two mantissas in [.5,1) is in [.25,1)
    lda fm_acc+5
    bmi FM_normalized
    asl fm_acc
    rol fm_acc+1
    rol fm_acc+2
    rol fm_acc+3
    rol fm_acc+4
    rol fm_acc+5
    lda fm_exp
    bne +
    dec fm_exp+1
+   dec fm_exp
FM_normalized
    lda fm_acc+1
    bpl FM_rounded
    inc fm_acc+2
    bne FM_rounded
    inc fm_acc+3
    bne FM_rounded
    inc fm_acc+4
    bne FM_rounded
    inc fm_acc+5
    bne FM_rounded
    lda #$80
    sta fm_acc+5
    inc fm_exp
    bne FM_rounded
    inc fm_exp+1
FM_rounded
    sec
    lda fm_exp
    sbc #128
    sta fm_exp
    lda fm_exp+1
    sbc #0
    sta fm_exp+1
    bmi FM_fallback
    bne FM_fallback
    lda fm_exp
    beq FM_fallback
    sta zp_flt_a
    lda fm_acc+5
    and #$7f
    ora fm_sign
    sta zp_flt_a+1
    lda fm_acc+4
    sta zp_flt_a+2
    lda fm_acc+3
    sta zp_flt_a+3
    lda fm_acc+2
    sta zp_flt_a+4
    rts
FM_zero
    lda #0
    sta zp_flt_a
    sta zp_flt_a+1
    sta zp_flt_a+2
    sta zp_flt_a+3
    sta zp_flt_a+4
    rts
; Exponent overflow/underflow: the ROM path raises the proper error or
; returns the right zero (zp_flt_a/zp_flt_b are still untouched here).
FM_fallback
    jmp Float_Mul_ROM

; in: X = multiplier byte, patched operands = multiplicand. out: fm_lo, A = hi
FM_Mul8
    sec
FM_sm1
    lda fm_sq1lo,x
FM_sm2
    sbc fm_sq2lo,x
    sta fm_lo
FM_sm3
    lda fm_sq1hi,x
FM_sm4
    sbc fm_sq2hi,x
    rts

.align 256
FM_tables_start
fm_sq1lo
.for i = 0, i < 512, i = i + 1
    .byte <((i*i)>>2)
.next
fm_sq1hi
.for i = 0, i < 512, i = i + 1
    .byte >((i*i)>>2)
.next
fm_sq2lo
.for i = 0, i < 512, i = i + 1
    .byte <(((255-i)*(255-i))>>2)
.next
fm_sq2hi
.for i = 0, i < 512, i = i + 1
    .byte >(((255-i)*(255-i))>>2)
.next
FM_tables_end
