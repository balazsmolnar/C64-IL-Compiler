
!macro add16 {

    +stack_pull_int $32
    +stack_pull_int $34

    clc
    lda $34
    adc $32
    sta $34
    lda $35
    adc $33
    sta $35
    +stack_push_var $34
}

!macro compareLess16 {
        +stack_pull_int $34
        +stack_pull_int $32

        clc
        ldx #0
        lda $33
        cmp $35
        bcc +
        bne ++
        lda $32
        cmp $34
        bcs ++
+       ldx #1
++      stx $34

        ldx #0
        stx $35
        +stack_push_var $34
}

!macro compareEqual16 {
        +stack_pull_int $34
        +stack_pull_int $32

        clc
        ldx #0
        lda $33
        cmp $35
        bne ++
        lda $32
        cmp $34
        bne ++
        ldx #1
++      stx $34

        ldx #0
        stx $35
        +stack_push_var $34
}
