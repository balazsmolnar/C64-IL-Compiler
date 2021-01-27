.Program_field_x !byte 0


;----------------------------------------
; TYPE: Demo.Program
; METHOD: OnInterrupt
;----------------------------------------
Program_OnInterrupt 
    +stack_save_return_adress .Program_OnInterrupt_ReturnAddress
    +stack_pull_int_ref .Program_OnInterrupt_args, 1
    +stack_pull_int_ref .Program_OnInterrupt_sender, 1
    nop ; Nop
    +stack_push_var .Program_field_x ; Ldsfld
    +stack_push_int 1 ; Ldc_i4_1
    +add ; Add
    +stack_pull_int_ref .Program_field_x, 0 ; Stsfld
    +stack_push_var .Program_field_x ; Ldsfld
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    +deref .Program_OnInterrupt_sender ; Nop
    +deref .Program_OnInterrupt_args ; Nop
    +stack_return_to_saved_address .Program_OnInterrupt_ReturnAddress ; Ret
.Program_OnInterrupt_sender !byte 0, 0
.Program_OnInterrupt_args !byte 0, 0
.Program_OnInterrupt_ReturnAddress !byte 0,0


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    +stack_save_return_adress .Program_Main_ReturnAddress
    nop ; Nop
    +newObj 3, 3 ; Newobj
    +stack_pull_int_ref .Program_Main_var0, 1 ; Stloc_0
    +stack_push_var .Program_Main_var0 ; Ldloc_0
    jsr Game_Init ; Callvirt
    nop ; Nop
    +stack_push_var .Program_Main_var0 ; Ldloc_0
    jsr Game_Run ; Callvirt
    nop ; Nop
    +deref .Program_Main_var0 ; Nop
    +stack_return_to_saved_address .Program_Main_ReturnAddress ; Ret
.Program_Main_ReturnAddress !byte 0,0
.Program_Main_var0 !byte 0,0
