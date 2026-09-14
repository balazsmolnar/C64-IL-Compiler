
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
