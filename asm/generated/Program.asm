

;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 2, []
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 0 ; Nop
    jmp Program_Main_20 ; Br_s
Program_Main_5:    #stack_push_pointer string_1879048649 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
Program_Main_20:  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #compareLess ; Clt
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #branch_true Program_Main_5 ; Brtrue_s
    #branch_if_var_less 1, 10, Program_Main_5 ; Nop
    #method_exit 4 ; Ret
