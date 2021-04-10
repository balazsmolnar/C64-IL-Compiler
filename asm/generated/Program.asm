

;----------------------------------------
; TYPE: Demo.Program
; METHOD: Foo
;----------------------------------------
Program_Foo 
    #init_locals_pull_parameters 0, [0,0,0]
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 1, []
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 0 ; Nop
    jmp Program_Main_18 ; Br_s
Program_Main_4:    #stack_push_pointer string_1879048193 ; Ldstr
    jsr Console_WriteLine ; Call
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
Program_Main_18:    #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #branch_less Program_Main_4 ; Blt_s
    #branch_less_const 10, Program_Main_4 ; Nop
    #method_exit 3, [] ; Ret
