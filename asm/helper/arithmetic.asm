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

!macro add8 {

    +stack_pull_int $32
    +stack_pull_int $34

    clc
    lda $34
    adc $32
    sta $34
    +stack_push_var $34
}

!macro sub8 {

    +stack_pull_int $32
    +stack_pull_int $34

    sec
    lda $34
    sbc $32
    sta $34
    +stack_push_var $34
}

!macro negate16 {

    +stack_pull_int $34

    lda $35
    eor #$FF
    sta $35
    lda $34
    eor #$FF
    clc
    adc #$1
    bcc +
    inc $35
+   sta $34
    +stack_push_var $34
}

!macro negate8 {

    +stack_pull_int $34

    lda $34
    eor #$FF
    clc
    adc #$1
    sta $34
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

!macro compareLess8 {
        +stack_pull_int $34
        +stack_pull_int $32

        ldx #0
        lda $32
        cmp $34
        bcs +
        ldx #1
+       stx $34
        +stack_push_var $34
}

!macro compareGreater8 {
        +stack_pull_int $34
        +stack_pull_int $32

        ldx #0
        lda $32
        cmp $34
        bcc +
        ldx #1
+       stx $34
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

!macro compareEqual8 {
        +stack_pull_int $34
        +stack_pull_int $32

        ldx #0
        lda $32
        cmp $34
        bne ++
        ldx #1
++
        stx $34
        +stack_push_var $34
}

!macro inc_var8 .variable {
        inc .variable
}

!macro init_var8 .variable, value  {
        lda #<value
        sta .variable
}

!macro shl {

    +stack_pull_int_x
    +stack_pull_int_a
    
 -  asl
    dex
    bne -
    +stack_push_int_a
}
