joy1 = $DC01
joy2 = $DC00

.weak
Flag_C64_get_Joysticks = 0
.endweak
.if Flag_C64_get_Joysticks

C64_get_Joysticks:
    rts
.endif

.weak
Flag_JoystickCollection_get_Joystick1 = 0
.endweak
.if Flag_JoystickCollection_get_Joystick1

JoystickCollection_get_Joystick1:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 1
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_JoystickCollection_get_Joystick2 = 0
.endweak
.if Flag_JoystickCollection_get_Joystick2

JoystickCollection_get_Joystick2:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 0
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Joystick_get_Pressed = 0
.endweak
.if Flag_Joystick_get_Pressed

Joystick_get_Pressed:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    lda joy2,x
    eor #$ff
    and #$1f

    stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif
