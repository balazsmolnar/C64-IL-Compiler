

;----------------------------------------
; TYPE: Demo.Program+<>c__DisplayClass4_0
; METHOD: <Main>b__0
;----------------------------------------
x__c__DisplayClass4_0_x_Main_b__0 
    #locals_init_locals 1
    #locals_pull_param_8 1
    #locals_pull_param_8 1
    #locals_pull_param_8 0
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld 0 ; Nop
    #locals_pull_value_8 4, 0 ; Stloc_0
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 4 ; Ldloc_0
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stfld 0 ; Stfld
    #locals_push_value_8 4 ; Ldloc_0
    jsr C64_SetBorderColor ; Call
    nop ; Nop
    #locals_method_exit 6 ; Ret
