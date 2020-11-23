; $30 - Position low
; $31 - Position high
; $32 - X
; $34 - Y
; $36 - char
; $38 - Color

C64_Set_Screen_Ptr:

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
    rts

C64_SetChar_Core:

    ; Init
    lda #$00
    sta $30
    lda #$d8
    sta $31

    jsr C64_Set_Screen_Ptr
    ldy #0
    lda $38
    sta ($30),y
    lda $31
    sec
    sbc #$d4
    sta $31
    lda $36
    sta ($30),y
    rts

C64_GetChar_Core:

    ; Init
    lda #$00
    sta $30
    lda #$04
    sta $31

    jsr C64_Set_Screen_Ptr
    ldy #0
    lda ($30),y
    sta $36
    sty $37
    rts

C64_SetChar:
    +stack_pull_int $38
    +stack_pull_int $36
    +stack_pull_int $34
    +stack_pull_int $32
    jmp C64_SetChar_Core

C64_GetChar:
    +stack_pull_int $34
    +stack_pull_int $32
    jsr C64_GetChar_Core
    +stack_push_var $36
    rts

C64_SetBorderColor:
    +stack_pull_int $34
    lda $34
    sta $D020
    rts