
branch_true .macro label 
    #stack_pull_int_a
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
    #stack_pull_int_a
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
