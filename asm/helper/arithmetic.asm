add16 .macro 

    #stack_pull_int $32
    #stack_pull_int $34

    clc
    lda $34
    adc $32
    sta $34
    lda $35
    adc $33
    sta $35
    #stack_push_var $34
.endm

add8 .macro

    #stack_pull_int $32
    #stack_pull_int $34

    clc
    lda $34
    adc $32
    sta $34
    #stack_push_var $34
.endm

sub8 .macro 

    #stack_pull_int $32
    #stack_pull_int $34

    sec
    lda $34
    sbc $32
    sta $34
    #stack_push_var $34
.endm

negate16 .macro 

    #stack_pull_int $34

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
    #stack_push_var $34
.endm

negate8 .macro  

    #stack_pull_int $34

    lda $34
    eor #$FF
    clc
    adc #$1
    sta $34
    #stack_push_var $34
.endm

compareLess16 .macro 
        #stack_pull_int $34
        #stack_pull_int $32

        clc
        ldx #0
        lda $33
        cmp $35
        bcc +
        bne l1
        lda $32
        cmp $34
        bcs l1
+       ldx #1
l1      stx $34

        ldx #0
        stx $35
        #stack_push_var $34
.endm

compareLess8 .macro 
        #stack_pull_int $34
        #stack_pull_int $32

        ldx #0
        lda $32
        cmp $34
        bcs +
        ldx #1
+       stx $34
        #stack_push_var $34
.endm

compareGreater8 .macro 
        #stack_pull_int $34
        #stack_pull_int $32

        ldx #0
        lda $32
        cmp $34
        bcc +
        ldx #1
+       stx $34
        #stack_push_var $34
.endm

compareEqual16 .macro 
        #stack_pull_int $34
        #stack_pull_int $32

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
        #stack_push_var $34
.endm

compareEqual8 .macro
        #stack_pull_int $34
        #stack_pull_int $32

        ldx #0
        lda $32
        cmp $34
        bne +
        ldx #1
+
        stx $34
        #stack_push_var $34
.endm

shift_left .macro 

    #stack_pull_int_x
    #stack_pull_int_a
    
-   asl
    dex
    bne -
    #stack_push_int_a
.endm
