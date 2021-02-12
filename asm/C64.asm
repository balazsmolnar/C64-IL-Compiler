; $30 - Position low
; $31 - Position high
; $32 - X
; $34 - Y
; $36 - char
; $38 - Color

C64_Set_Screen_Ptr

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

C64_SetChar_Core

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
    #stack_return_to_saved_address zp_tmp1_low

C64_GetChar_Core

    ; Init
    lda #$00
    sta $30
    lda #$04
    sta $31

    jsr C64_Set_Screen_Ptr
    ldy #0
    lda ($30),y
    sta $36
    rts

C64_SetChar
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int $38
    #stack_pull_int $36
    #stack_pull_int $34
    #stack_pull_int $32

    jmp C64_SetChar_Core

C64_GetChar
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int $34
    #stack_pull_int $32
    jsr C64_GetChar_Core
    #stack_push_var $36
    #stack_return_to_saved_address zp_tmp1_low

C64_SetBorderColor
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta $D020
    #stack_return_to_saved_address zp_tmp1_low

C64_SetBackgroundColor
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta $D021
    #stack_return_to_saved_address zp_tmp1_low

C64_GetBorderColor
    #stack_save_return_adress zp_tmp1_low
    #stack_push_var $D020

    #stack_return_to_saved_address zp_tmp1_low

C64_add_Interrupt
    sei
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    #stack_pull_pointer zp_interrupt_address_low
    #stack_pull_int_a
    lda $0314
    sta zp_interrupt_saved_low
    lda $0315
    sta zp_interrupt_saved_high

    lda #< OnInterrupt
    sta $0314
    lda #> OnInterrupt
    sta $0315
    cli
    #stack_return_to_saved_address zp_tmp1_low

OnInterrupt
    #stack_push_int 0
    #stack_push_int 0
    #stack_push_int 0
    lda #> On_Interrupt_Ret-1
    pha
    lda #< On_Interrupt_Ret-1
    pha
    jmp (zp_interrupt_address_low)
On_Interrupt_Ret
    jmp (zp_interrupt_saved_low)
    rti

.include "./C64sprite.asm"
.include "./C64Keys.asm"
.include "./C64Debug.asm"
