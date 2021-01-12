
!macro bne .label {
            beq+
            jmp .label
+
}

!macro bmi .label {
            bpl+
            jmp .label
+
}

!macro bpl .label {
            bmi+
            jmp .label
+
}

!macro beq .label {
            bne+
            jmp .label
+
}

!macro branch_true .label {
    +stack_pull_int $30
    +bne .label
}

!macro branch_equal .label {
    +stack_pull_int $30
    +stack_pull_int $32
    lda $30
    cmp $32
    +beq .label
}

!macro branch_less .label {
    +stack_pull_int $30
    +stack_pull_int $32
    lda $30
    cmp $32
    +bpl .label
}

!macro branch_false .label {
    +stack_pull_int $30
    +beq .label
}

!macro branch_if_var_less .variable, .value, .label {
    lda .variable
    cmp #.value
    +bmi .label
}

!macro branch_if_not_equal .variable, .value, .label {
    lda .variable
    cmp #.value
    +bne .label
}