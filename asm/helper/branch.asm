
!macro bne .label {
    !set .d = 0
    !ifndef .label  {
        !set .d = 255
    } else {
        !set .d = * - .label
    }
    !if (.d < 127) {
        bne .label
    } else {
        beq+
        jmp .label
+       
    }
}

!macro bmi .label {
    !set .d = 0
    !ifndef .label  {
        !set .d = 255
    } else {
        !set .d = * - .label
    }
    !if (.d < 127) {
        bmi .label
    } else {
        bpl+
        jmp .label
+       
    }
}

!macro bpl .label {
    !set .d = 0
    !ifndef .label  {
        !set .d = 255
    } else {
        !set .d = * - .label
    }
    !if (.d < 127) {
        bpl .label
    } else {
        bmi+
        jmp .label
+       
    }
}

!macro beq .label {
    !set .d = 0
    !ifndef .label  {
        !set d = 255
    } else {
        !set d = * - .label
    }
    !if (.d < 127) {
        beq .label
    } else {
        bne+
        jmp .label
+       
    }
}

!macro branch_true .label {
    +stack_pull_int $30
    +bne .label
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