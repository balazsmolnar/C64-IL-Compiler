
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

branch_less .macro label 
    #stack_pull_int $30
    #stack_pull_int $32
    lda $30
    cmp $32
    bpl \label
.endm

branch_less_equal .macro label 
    #stack_pull_int $30
    #stack_pull_int $32
    lda $30
    cmp $32
    bpl \label
    beq \label
.endm

branch_greater .macro label 
    #stack_pull_int $30
    #stack_pull_int $32
    lda $30
    cmp $32
    bmi \label
    beq \label
.endm

branch_false .macro label 
    #stack_pull_int_a
    beq \label
.endm

