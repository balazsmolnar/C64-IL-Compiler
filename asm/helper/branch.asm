!macro branch_true label {
    +stack_pull_int $30
    beq+
    jmp label
+       

}

!macro branch_false label {
    +stack_pull_int $30
    bne+
    jmp label
+       

}
