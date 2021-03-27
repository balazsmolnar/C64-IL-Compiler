

;----------------------------------------
; TYPE: Hunchback.Delay
; METHOD: Wait
;----------------------------------------
Delay_Wait 
    #init_locals_pull_parameters 4, [0]
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
    jmp Delay_Wait_30 ; Br_s
Delay_Wait_5:    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp Delay_Wait_16 ; Br_s
Delay_Wait_10:    nop ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Delay_Wait_16:  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt_un
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #branch_true Delay_Wait_10 ; Brtrue_s
    #branch_if_var_less 3, 10, Delay_Wait_10 ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
Delay_Wait_30:    #locals_push_value_8 2 ; Ldloc_0
    #locals_push_value_8 1 ; Ldarg_0
    #compareLess ; Clt_un
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 5 ; Ldloc_3
    #branch_true Delay_Wait_5 ; Brtrue_s
    #method_exit 7, [] ; Ret
