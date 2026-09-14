VCR2            = $D016 ;VIC Control Register 2

; zp_param0_low  - Position low
; zp_param0_high - Position high
; zp_param1_low  - X
; zp_param2_low  - Y
; zp_param3_low  - char
; zp_param4_low  - Color
; (see asm/helper/zeropage.asm for every other claimant of these bytes)

C64_Set_Screen_Ptr

    ; add Y*40
    lda zp_param2_low
    asl
    asl
    adc zp_param2_low
    asl
    asl
    bcc +
    inc zp_param0_high
    inc zp_param0_high
+   asl
    bcc +
    inc zp_param0_high
+   sta zp_param0_low
    ; +X
    lda zp_param1_low
    clc
    adc zp_param0_low
    bcc +
    inc zp_param0_high
+   sta zp_param0_low
    rts

C64_SetChar_Core

    ; Init
    lda #$00
    sta zp_param0_low
    lda #$d8
    sta zp_param0_high

    jsr C64_Set_Screen_Ptr
    ldy #0
    lda zp_param4_low
    sta (zp_param0_low),y
    lda zp_param0_high
    sec
    sbc #$d4
    sta zp_param0_high
    lda zp_param3_low
    cmp #$ff
    beq +
    sta (zp_param0_low),y
+   #stack_return_to_saved_address zp_tmp1_low

C64_GetChar_Core

    ; Init
    lda #$00
    sta zp_param0_low
    lda #$04
    sta zp_param0_high

    jsr C64_Set_Screen_Ptr
    ldy #0
    lda (zp_param0_low),y
    sta zp_param3_low
    rts

C64_SetChar
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_param4_low
    #stack_pull_int zp_param3_low
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param1_low

    jmp C64_SetChar_Core

C64_GetChar
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param1_low
    jsr C64_GetChar_Core
    #stack_push_var zp_param3_low
    #stack_return_to_saved_address zp_tmp1_low

C64_Write
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_param4_low
    #stack_pull_pointer zp_param3_low
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param1_low

    lda #0
    sta zp_param0_low
    lda #4
    sta zp_param0_high
    jsr C64_Set_Screen_Ptr

    lda zp_param0_high
    clc
    adc #$d4
    sta zp_param1_high
    lda zp_param0_low
    sta zp_param1_low

    ldy #0
-   lda (zp_param3_low),Y
    beq +
    sta (zp_param0_low),Y
    lda zp_param4_low
    sta (zp_param1_low),Y
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
    #stack_pull_pointer zp_param3_low
    #stack_pull_pointer zp_param4_low
-   lda (zp_param3_low),y
    sta (zp_param4_low),y
    dey
    bne -

    #stack_return_to_saved_address zp_tmp1_low

C64_FillMemory
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x
    #stack_pull_pointer zp_param4_low
    txa
-
    sta (zp_param4_low),y
    dey
    bne -

    #stack_return_to_saved_address zp_tmp1_low

C64_GetMemory
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_pointer zp_param4_low
    lda (zp_param4_low),y
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

.include "./c64sprite.asm"
.if USE_JOYSTICK
.include "./c64Joystick.asm"
.endif
.if USE_SOUND
.include "./C64Sound.asm"
.endif
.include "./c64Keys.asm"
.if USE_DEBUG
.include "./C64Debug.asm"
.endif
