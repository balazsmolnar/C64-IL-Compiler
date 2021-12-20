joy1 = $DC01
joy2 = $DC00


C64_get_Joysticks:
    rts

JoystickCollection_get_Joystick1:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 1
    #stack_return_to_saved_address zp_tmp1_low

JoystickCollection_get_Joystick2:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 0
    #stack_return_to_saved_address zp_tmp1_low

Joystick_get_Pressed:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    lda joy2,x
    eor #$ff
    and #$1f

    stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
