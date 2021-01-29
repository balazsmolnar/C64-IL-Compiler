Program_field_x .byte 0


;----------------------------------------
; TYPE: Demo.Program
; METHOD: OnInterrupt
;----------------------------------------
Program_OnInterrupt 
    #stack_save_return_adress Program_OnInterrupt_ReturnAddress
    #stack_pull_int_ref Program_OnInterrupt_args, 1
    #stack_pull_int_ref Program_OnInterrupt_sender, 1
    nop ; Nop
    #stack_push_var Program_field_x ; Ldsfld
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stack_pull_int_ref Program_field_x, 0 ; Stsfld
    #stack_push_var Program_field_x ; Ldsfld
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    #deref Program_OnInterrupt_sender ; Nop
    #deref Program_OnInterrupt_args ; Nop
    #stack_return_to_saved_address Program_OnInterrupt_ReturnAddress ; Ret
Program_OnInterrupt_sender .byte 0, 0
Program_OnInterrupt_args .byte 0, 0
Program_OnInterrupt_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #stack_save_return_adress Program_Main_ReturnAddress
    nop ; Nop
    jsr GCTest_Start ; Call
    nop ; Nop
    #stack_return_to_saved_address Program_Main_ReturnAddress ; Ret
Program_Main_ReturnAddress .byte 0,0
