

;----------------------------------------
; TYPE: Hunchback.LevelDescription
; METHOD: get_Levels
;----------------------------------------
LevelDescription_get_Levels 
    #init_locals_pull_parameters 0, []
    #stack_push_int 4 ; Ldc_i4_4
    #newArr  ; Newarr
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #newObj 3, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 3 ; Ldc_i4_3
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 11 ; Ldc_i4_s
    #stfld 2 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #newObj 3, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 11 ; Ldc_i4_s
    #stfld 2 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #newObj 3, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 11 ; Ldc_i4_s
    #stfld 2 ; Stfld
    #stelemRef ; Stelem_ref
    #stack_duplicate ; Dup
    #stack_push_int 3 ; Ldc_i4_3
    #newObj 3, 0 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 0 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_int 15 ; Ldc_i4_s
    #stfld 2 ; Stfld
    #stelemRef ; Stelem_ref
    #method_exit 2, [] ; Ret
