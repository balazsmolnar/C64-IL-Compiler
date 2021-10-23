
Program_VTable:  .byte 0


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 1, []
    #stack_push_int8 12 ; Ldc_i4_s
    #newArr16 ; Newarr
    #stack_duplicate8 ; Dup
    #stack_push_int8 0 ; Ldc_i4_0
    #stack_push_pointer string_1879048193 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 1 ; Ldc_i4_1
    #stack_push_pointer string_1879048265 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 2 ; Ldc_i4_2
    #stack_push_pointer string_1879048343 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 3 ; Ldc_i4_3
    #stack_push_pointer string_1879048389 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 4 ; Ldc_i4_4
    #stack_push_pointer string_1879048461 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 5 ; Ldc_i4_5
    #stack_push_pointer string_1879048539 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 6 ; Ldc_i4_6
    #stack_push_pointer string_1879048585 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 7 ; Ldc_i4_7
    #stack_push_pointer string_1879048631 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 8 ; Ldc_i4_8
    #stack_push_pointer string_1879048685 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 9 ; Ldc_i4_s
    #stack_push_pointer string_1879048731 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 10 ; Ldc_i4_s
    #stack_push_pointer string_1879048777 ; Ldstr
    #stelem16 ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 11 ; Ldc_i4_s
    #stack_push_pointer string_1879048831 ; Ldstr
    #stelem16 ; Stelem_ref
    #locals_pull_value8 1, 1 ; Stloc_0
    #method_exit 3, [1] ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Add
;----------------------------------------
Program_Add 
    #init_locals_pull_parameters 0, [0,0]
    #locals_push_value8 1 ; Ldarg_0
    #locals_push_value8 2 ; Ldarg_1
    #add8 ; Add
    #method_exit 4, [] ; Ret
