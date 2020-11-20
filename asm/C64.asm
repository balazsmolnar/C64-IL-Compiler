; $30 - Position low
; $31 - Position high
; $32 - X
; $34 - Y
; $36 - char

C64_SetChar_Core:

    ; Init
    lda #$00
    sta $30
    lda #$04
    sta $31

    ; add Y*40
    lda $34
    asl
    asl
    adc $34
    asl
    asl
    bcc +
    inc $31
    inc $31
+   asl
    bcc +
    inc $31
+   sta $30
    ; +X
    lda $32
    clc
    adc $30
    bcc +
    inc $31
+   sta $30
    lda $36
    ldy #0
    sta ($30),y
    rts

C64_SetChar:
    +stack_pull_pointer $36
    +stack_pull_pointer $34
    +stack_pull_pointer $32
    jmp C64_SetChar_Core



