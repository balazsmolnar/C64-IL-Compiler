
branch_true .macro label 
    #stack_pull_int_a
    bne \label
.endm

branch_equal .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
    beq \label
.endm

branch_equal_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    beq \label
.endm

branch_not_equal .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
    bne \label
.endm

branch_not_equal_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bne \label
.endm

branch_less .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
    bmi \label
.endm

branch_less_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bmi \label
.endm

branch_less_unsigned .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
    bcc \label
.endm

branch_less_unsigned_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bcc \label
.endm

branch_less_equal .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
    bmi \label
    beq \label
.endm

branch_less_equal_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bmi \label
    beq \label
.endm

branch_less_equal_unsigned .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
    bcc \label
    beq \label
.endm

branch_less_equal_unsigned_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bcc \label
    beq \label
.endm

branch_greater_equal_unsigned .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
    bcs \label
    beq \label
.endm

branch_greater_equal_unsigned_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bcs \label
    beq \label
.endm

branch_greater_unsigned .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
    bcs \label
.endm

branch_greater_unsigned_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bcs \label
.endm

branch_greater .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
    bpl \label
.endm

branch_greater_const .macro value, label 
    #stack_pull_int_a
    cmp #\value
    bpl \label
.endm

branch_greater_equal .macro label 
    #stack_pull_int $30
    #stack_pull_int_a
    cmp $30
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
