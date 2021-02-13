x__c_field_x__9__2_0 .byte 0
x__c_field_x__9 .byte 0


;----------------------------------------
; TYPE: Demo.Program+<>c
; METHOD: <Main>b__2_0
;----------------------------------------
x__c_x_Main_b__2_0 
    #locals_init_locals 0
    #locals_pull_param_8 1
    #locals_pull_param_8 1
    #locals_pull_param_8 0
    nop ; Nop
    jsr C64_GetBorderColor ; Call
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    #locals_method_exit 5 ; Ret
