

;----------------------------------------
; TYPE: Demo.Program
; METHOD: Recursive
;----------------------------------------
Program_Recursive 
    #init_locals_pull_parameters 1, [0]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 14 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 10 ; Ldc_i4_s
    #compareEqual ; Ceq
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 2 ; Ldloc_0
    #branch_false Program_Recursive_23 ; Brfalse_s
    jmp Program_Recursive_32 ; Br_s
Program_Recursive_23:    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    jsr Program_Recursive ; Call
    nop ; Nop
Program_Recursive_32:    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Test
;----------------------------------------
Program_Test 
    #init_locals_pull_parameters 3, [0,0]
    nop ; Nop
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_0
    #init_var 3, 1 ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    #locals_pull_value_8 4, 0 ; Stloc_1
    #locals_push_value_8 1 ; Ldarg_0
    #locals_pull_value_8 3, 0 ; Stloc_0
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 2 ; Ldc_i4_2
    #compareEqual ; Ceq
    #locals_pull_value_8 5, 0 ; Stloc_2
    #locals_push_value_8 5 ; Ldloc_2
    #branch_false Program_Test_22 ; Brfalse_s
    #stack_push_int 3 ; Ldc_i4_3
    jsr C64_SetBorderColor ; Call
    nop ; Nop
Program_Test_22:    #method_exit 7 ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 1, []
    nop ; Nop
    #newObj 3, 3 ; Newobj
    #locals_pull_value_8 1, 1 ; Stloc_0
    #locals_push_value_8 1 ; Ldloc_0
    jsr Game_Init ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    jsr Game_Run ; Callvirt
    nop ; Nop
    #method_exit 3 ; Ret
