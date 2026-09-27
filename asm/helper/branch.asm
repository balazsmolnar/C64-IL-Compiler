
; branch_true/branch_false default to the 1-byte form (every truthiness test
; here used to be exactly 1 byte: bool, or an ordinary object reference,
; which is a 1-byte object-table handle) -- kept as a bare alias so nothing
; that already names "branch_true"/"branch_false" directly needs to change.
branch_true .macro label
    #branch_true8 \label
.endm

branch_true8 .macro label
    #stack_pull_int_a
    bne \label
.endm

; string is the one reference type wider than 1 byte (a real 2-byte
; pointer, not an object-table handle -- TypeExtensions.GetStorageBytes),
; so `if (s == null)`/`if (s != null)` (which Roslyn compiles straight to
; brtrue.s/brfalse.s on s, no Ceq involved) need a 2-byte truthiness test:
; nonzero if EITHER byte is nonzero. zp_tmp3 is scratch, dead between any
; two compiler-emitted instructions (asm/helper/zeropage.asm).
branch_true16 .macro label
    #stack_pull_int_a
    sta zp_tmp3
    #stack_pull_int_a
    ora zp_tmp3
    bne \label
.endm

branch_equal8 .macro label 
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    beq \label
.endm

branch_equal_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    beq \label
.endm

branch_equal16 .macro label

    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high
    lda zp_param1_low
    cmp zp_param2_low
    bne +
    lda zp_param1_high
    cmp zp_param2_high
    beq \label
+
.endm

branch_not_equal8 .macro label
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    bne \label
.endm

branch_not_equal16 .macro label 

    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high
    lda zp_param1_low
    cmp zp_param2_low
    bne \label
    lda zp_param1_high
    cmp zp_param2_high
    bne \label
.endm

branch_not_equal_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bne \label
.endm

branch_less8 .macro label 
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    bmi \label
.endm

branch_less_const .macro value, label 
    #stack_pull_int_a
    cmp #\value.
    bmi \label
.endm

branch_less_unsigned8 .macro label 
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    bcc \label
.endm

; Signed 16-bit relational branches -- previously missing entirely (only
; 8-bit signed and 16-bit UNSIGNED existed below), meaning Blt/Ble/Bgt/Bge
; on a long/ulong-width signed operand (this compiler's long/ulong are 2
; bytes -- TypeExtensions.GetStorageBytes) emitted a reference to a macro
; that simply didn't exist, failing only as a confusing 64tass "not
; defined symbol" error at assembly time (Compiler/CommandMap.cs's
; Blt/Ble/Bgt/Bge entries already pick the "16" suffix purely from operand
; width, same as every other branch family here -- no C#-level
; NotSupportedException guards this the way most other unsupported
; constructs are guarded).
;
; Same overflow-corrected-subtraction technique arithmetic.asm's
; compareLess16/compareGreater16 already use for Clt/Cgt (see those for the
; fuller derivation): CMP the low bytes (sets the borrow SBC needs), SBC
; the high bytes, then XOR the result's sign with V whenever V is set --
; the standard 6502 signed-16-bit-compare idiom, where V catches a raw
; two's-complement overflow and the XOR corrects N back to the
; subtraction's TRUE mathematical sign in that case. Once corrected, N
; precisely reflects "negative" for the full signed difference, for every
; input including the a==b case (a zero difference is never negative) --
; so unlike the _unsigned16 macros below (which need a separate low/high
; double-check for their "or equal" variants), less-or-equal/
; greater-or-equal here just reuse the OTHER macro's (operand-swapped)
; subtraction with the opposite branch: "a <= b" is "NOT (b < a)", i.e.
; "(b - a) is not negative" -- the exact same subtraction
; branch_greater16 does, just testing bpl instead of bmi.
branch_less16 .macro label
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    lda zp_param2_low
    cmp zp_param1_low
    lda zp_param2_high
    sbc zp_param1_high
    bvc +
    eor #$80
+   bmi \label
.endm

branch_greater_equal16 .macro label
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    lda zp_param2_low
    cmp zp_param1_low
    lda zp_param2_high
    sbc zp_param1_high
    bvc +
    eor #$80
+   bpl \label
.endm

branch_greater16 .macro label
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    lda zp_param1_low
    cmp zp_param2_low
    lda zp_param1_high
    sbc zp_param2_high
    bvc +
    eor #$80
+   bmi \label
.endm

branch_less_equal16 .macro label
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    lda zp_param1_low
    cmp zp_param2_low
    lda zp_param1_high
    sbc zp_param2_high
    bvc +
    eor #$80
+   bpl \label
.endm

branch_less_unsigned16 .macro label

        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high

        lda zp_param2_high
        cmp zp_param1_high
        bcc \label
        bne +
        lda zp_param2_low
        cmp zp_param1_low
        bcc \label
+       
.endm

branch_less_unsigned_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bcc \label
.endm

branch_less_equal8 .macro label 
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    bmi \label
    beq \label
.endm

branch_less_equal_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bmi \label
    beq \label
.endm

branch_less_equal_unsigned8 .macro label 
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    bcc \label
    beq \label
.endm

branch_less_equal_unsigned16 .macro label 
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high
    lda zp_param2_high
    cmp zp_param1_high
    bcc \label
    bne+
    lda zp_param2_low
    cmp zp_param1_low
    bcc \label
    beq \label
+
.endm

branch_less_equal_unsigned_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bcc \label
    beq \label
.endm

branch_greater_equal_unsigned8 .macro label 
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    bcs \label
    beq \label
.endm

branch_greater_equal_unsigned16 .macro label 
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    lda zp_param1_high
    cmp zp_param2_high
    bcc \label
    bne +
    lda zp_param2_low
    cmp zp_param1_low
    bcs \label
    beq \label
+
.endm


branch_greater_equal_unsigned_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bcs \label
    beq \label
.endm

branch_greater_unsigned8 .macro label 
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    bcs \label
.endm

branch_greater_unsigned16 .macro label
    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    lda zp_param1_high
    cmp zp_param2_high
    bcc \label
    bne +
    lda zp_param1_low
    cmp zp_param2_low
    bcc \label
+
.endm

branch_greater_unsigned_const .macro value, label
    #stack_pull_int_a
    cmp #\value
    bcs \label
.endm

branch_greater8 .macro label 
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    bpl \label
.endm

branch_greater_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bpl \label
.endm

branch_greater_equal8 .macro label 
    #stack_pull_int zp_param0_low
    #stack_pull_int_a
    cmp zp_param0_low
    bpl \label
    beq \label
.endm

branch_greater_equal_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bpl \label
    beq \label
.endm

branch_false .macro label
    #branch_false8 \label
.endm

branch_false8 .macro label
    #stack_pull_int_a
    beq \label
.endm

; See branch_true16's comment.
branch_false16 .macro label
    #stack_pull_int_a
    sta zp_tmp3
    #stack_pull_int_a
    ora zp_tmp3
    beq \label
.endm

; Float compare-and-branch, for Roslyn's fused "if (a < b)"-style IL (Beq/
; Blt/etc. instead of a separate Ceq/Clt + Brtrue) -- see
; asm/helper/float.asm for Float_Compare (a real subroutine, not inlined
; here, for the same reason every other ROM-calling float op is a
; subroutine: the bank-in/call-ROM/bank-out sequence must live at one fixed
; address below $a000). Reuses the exact same A=0/1/$FF convention the
; compareXflt macros in float.asm already test.
;
; No SEI/CLI here -- see float.asm's identical note above its own macros:
; interrupt-time safety is centralized in asm/C64.asm's OnInterrupt now,
; and a macro-level CLI would be actively wrong if this macro were ever
; expanded inside a compiled interrupt handler's own body.
branch_equalflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    beq \label
.endm

branch_not_equalflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    bne \label
.endm

branch_lessflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    bmi \label
.endm

branch_less_equalflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    bmi \label
    beq \label
.endm

branch_greater_equalflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    bpl \label
.endm

branch_greterflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    bmi +
    beq +
    jmp \label
+
.endm

; Float has no real unsigned concept -- these exist because IL's Bge_un/
; Blt_un/etc. double as "unordered-or-..." for floating-point operands
; (Roslyn uses the _un family for a negated float branch, e.g. compiling
; "if (a < b) ... else ..." with Bge_un_s testing the inverse), which only
; differs from the plain signed version when a NaN is involved -- Mflpt.cs
; already refuses to convert NaN/Infinity at compile time, so that case
; can never actually arise here. Identical logic to the non-_un macros
; above, just under the name IL asks for.
branch_less_unsignedflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    bmi \label
.endm

branch_less_equal_unsignedflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    bmi \label
    beq \label
.endm

branch_greater_equal_unsignedflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    bpl \label
.endm

branch_greater_unsignedflt .macro label
    #stack_pull_mflpt zp_flt_b
    #stack_pull_mflpt zp_flt_a
    jsr Float_Compare
    cmp #0
    bmi +
    beq +
    jmp \label
+
.endm

switch .macro jump_table
    #stack_pull_int_a
    asl
    tax
    lda \jump_table+1,x
    pha
    lda \jump_table,x
    pha
    rts
.endm
