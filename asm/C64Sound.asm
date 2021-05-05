
C64_get_Sound:
    #stack_save_return_adress zp_tmp1_low
    stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low

Sound_set_Volume:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int $D418
    #stack_pull_int_a
    #stack_return_to_saved_address zp_tmp1_low

Sound_PlayEffectReg1:
    #stack_save_return_adress zp_tmp1_low
    lda #0
    sta $D404
    #stack_pull_int_x
    #stack_pull_int $D406
    #stack_pull_int $D405
    #stack_pull_int $D402
    #stack_pull_int $D403
    #stack_pull_int $D400
    #stack_pull_int $D401
    #stack_pull_int_a
    eor #01
    sta $D404
    cpx #0
    beq +
    eor #01
    sta $D404
+   stack_pull_int_a
    #stack_return_to_saved_address zp_tmp1_low

Sound_PlayEffectReg2:
    #stack_save_return_adress zp_tmp1_low
    lda #0
    sta $D40B
    #stack_pull_int_x
    #stack_pull_int $D40D
    #stack_pull_int $D40C
    #stack_pull_int $D409
    #stack_pull_int $D40A
    #stack_pull_int $D407
    #stack_pull_int $D408
    #stack_pull_int_a
    eor #01
    sta $D40B
    cpx #0
    beq +
    eor #01
    sta $D40B
+   stack_pull_int_a
    #stack_return_to_saved_address zp_tmp1_low
