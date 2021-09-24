
Program_VTable:  .byte 0


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 2, []
    #newObj 0, 0, A_VTable ; Newobj
    #callVirt 0 ; Callvirt
    #newObj 0, 0, Game_VTable ; Newobj
    #locals_pull_value8 1, 1 ; Stloc_0
    #locals_push_value8 1 ; Ldloc_0
    jsr Game_Start ; Callvirt
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_1
    #init_var 2, 0 ; Nop
    jmp Program_Main_40 ; Br_s
Program_Main_26:    #stack_push_pointer string_1879048193 ; Ldstr
    jsr Console_WriteLine ; Call
  ; OPT   #locals_push_value8 2 ; Ldloc_1
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_1
    #inc_var 2 ; Nop
Program_Main_40:    #locals_push_value8 2 ; Ldloc_1
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #branch_less8 Program_Main_26 ; Blt_s
    #branch_less_const 10, Program_Main_26 ; Nop
    #method_exit 4, [1] ; Ret
