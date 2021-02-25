x__c_field_x__9__1_0 .byte 0
x__c_field_x__9 .byte 0


;----------------------------------------
; TYPE: Demo.Program+<>c
; METHOD: <Main>b__1_0
;----------------------------------------
x__c_x_Main_b__1_0 
    #init_locals_pull_parameters 0, [1,1,0]
    nop ; Nop
    jsr C64_GetBorderColor ; Call
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    #method_exit 5, [2,3] ; Ret
