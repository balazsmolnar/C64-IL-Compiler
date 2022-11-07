
Program_VTable:  .byte 0


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 1, []
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 1, 0 ; Stloc_0
    #init_var 1, 0 ; Nop
    jmp Program_Main_18 ; Br_s
Program_Main_4:    #stack_push_pointer string_1879048193 ; Ldstr
    jsr Console_WriteLine ; Call
  ; OPT   #locals_push_value8 1 ; Ldloc_0
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
Program_Main_18:    #locals_push_value8 1 ; Ldloc_0
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #branch_less8 Program_Main_4 ; Blt_s
    #branch_less_const 10, Program_Main_4 ; Nop
    #newObj 0, 0, Game_VTable, 0 ; Newobj
    jsr Game_Start ; Call
    #method_exit 3, [] ; Ret
