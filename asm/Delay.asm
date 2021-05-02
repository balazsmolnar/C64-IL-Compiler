

;----------------------------------------
; TYPE: Hunchback.Delay
; METHOD: Wait
;----------------------------------------
Delay_Wait 
    #init_locals_pull_parameters 2, [0]
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
    jmp Delay_Wait_21 ; Br_s
Delay_Wait_4:  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp Delay_Wait_12 ; Br_s
Delay_Wait_8:  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
Delay_Wait_12:    #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned Delay_Wait_8 ; Blt_un_s
    #branch_less_unsigned_const 10, Delay_Wait_8 ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
Delay_Wait_21:    #locals_push_value_8 2 ; Ldloc_0
    #locals_push_value_8 1 ; Ldarg_0
    #branch_less_unsigned Delay_Wait_4 ; Blt_un_s
    #method_exit 5, [] ; Ret
