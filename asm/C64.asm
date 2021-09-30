VCR2            = $D016 ;VIC Control Register 2

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
    cmp #$ff
    beq +
    sta ($30),y
+   #stack_return_to_saved_address zp_tmp1_low

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

C64_Write
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int $38
    #stack_pull_pointer $36
    #stack_pull_int $34
    #stack_pull_int $32

    lda #0
    sta $30
    lda #4
    sta $31
    jsr C64_Set_Screen_Ptr

    lda $31
    clc
    adc #$d4
    sta $33
    lda $30
    sta $32

    ldy #0
-   lda ($36),Y
    beq +
    sta ($30),Y
    lda $38
    sta ($32),Y
    iny
    bne -
+
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

C64_SetCharSet
    #stack_save_return_adress zp_tmp1_low
    lda $D018
    and #%11110001
    sta $D018
    #stack_pull_int_a
    #stack_pull_int_a

    lsr
    lsr
    ora $D018
    sta $D018
    #stack_return_to_saved_address zp_tmp1_low

C64_CopyMemory
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_pointer $36
    #stack_pull_pointer $38
-   lda ($36),y
    sta ($38),y
    dey
    bne -

    #stack_return_to_saved_address zp_tmp1_low

C64_FillMemory
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x
    #stack_pull_pointer $38
    txa
-   
    sta ($38),y
    dey
    bne -

    #stack_return_to_saved_address zp_tmp1_low

C64_GetMemory
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_pointer $38
    lda ($38),y
    stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low

C64_SetMultiColor
    #stack_save_return_adress zp_tmp1_low

    lda VCR2
    ora #%00010000          ; multi-colour mode on
    sta VCR2   
    #stack_return_to_saved_address zp_tmp1_low

C64_SetCharBackgroundColor
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    #stack_pull_int_y
    txa
    sta $D022, y
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
.include "./C64sound.asm"
.include "./C64Keys.asm"
.include "./C64Debug.asm"
