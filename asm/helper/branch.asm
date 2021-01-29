
branch_true .macro label 
    #stack_pull_int $30
    bne \label
.endm

branch_equal .macro label 
    #stack_pull_int $30
    #stack_pull_int $32
    lda $30
    cmp $32
    beq \label
.endm

branch_less .macro label 
    #stack_pull_int $30
    #stack_pull_int $32
    lda $30
    cmp $32
    bpl \label
.endm

branch_false .macro label 
    #stack_pull_int $30
    beq \label
.endm

branch_if_var_less .macro variable, value, label 
    lda \variable
    cmp #\value
    bmi \label
.endm

branch_if_not_equal .macro variable, value, label 
    lda \variable
    cmp #\value
    bne \label
.endm