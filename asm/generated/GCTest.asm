GCTest_field_s_a1 .byte 0
GCTest_field_s_a2 .byte 0


;----------------------------------------
; TYPE: Demo.GCTest
; METHOD: Two_Instances_First_GCd_Static_Field
;----------------------------------------
GCTest_Two_Instances_First_GCd_Static_Field 
    #init_locals_pull_parameters 6, []
    nop ; Nop
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 4 ; Ldc_i4_4
    #stfld 1 ; Stfld
    #stack_pull_int_ref GCTest_field_s_a1, 1 ; Stsfld
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 1 ; Stfld
    #stack_pull_int_ref GCTest_field_s_a2, 1 ; Stsfld
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_field_s_a1 ; Ldsfld
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 1, 0 ; Stloc_0
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_field_s_a2 ; Ldsfld
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 2, 0 ; Stloc_1
    jsr C64_get_Debug ; Call
    #locals_push_value_8 1 ; Ldloc_0
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 3, 0 ; Stloc_2
    #locals_push_value_8 3 ; Ldloc_2
    #branch_false GCTest_Two_Instances_First_GCd_Static_Field_102 ; Brfalse_s
    nop ; Nop
    #stack_push_pointer string_1879048193 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
    jmp GCTest_Two_Instances_First_GCd_Static_Field_236 ; Br
GCTest_Two_Instances_First_GCd_Static_Field_102:    #stack_push_int 0 ; Ldnull
    #stack_pull_int_ref GCTest_field_s_a1, 1 ; Stsfld
    jsr GC_Collect ; Call
    nop ; Nop
    jsr C64_get_Debug ; Call
    #locals_push_value_8 1 ; Ldloc_0
    jsr Debug_IsAlive ; Callvirt
    #locals_pull_value_8 4, 0 ; Stloc_3
    #locals_push_value_8 4 ; Ldloc_3
    #branch_false GCTest_Two_Instances_First_GCd_Static_Field_140 ; Brfalse_s
    #stack_push_pointer string_1879048213 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Two_Instances_First_GCd_Static_Field_140:    jsr C64_get_Debug ; Call
    #locals_push_value_8 2 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 5, 0 ; Stloc_s
    #locals_push_value_8 5 ; Ldloc_s
    #branch_false GCTest_Two_Instances_First_GCd_Static_Field_171 ; Brfalse_s
    #stack_push_pointer string_1879048245 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Two_Instances_First_GCd_Static_Field_171:    #stack_push_var GCTest_field_s_a2 ; Ldsfld
    #ldfld 1 ; Ldfld
    #stack_push_int 5 ; Ldc_i4_5
    #compareEqual ; Ceq
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_false GCTest_Two_Instances_First_GCd_Static_Field_204 ; Brfalse_s
    #stack_push_pointer string_1879048273 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Two_Instances_First_GCd_Static_Field_204:    #stack_push_int 0 ; Ldc_i4_0
    #stack_push_int 1 ; Ldc_i4_1
    #stack_push_var GCTest_field_s_a2 ; Ldsfld
    #ldfld 1 ; Ldfld
    #stack_push_int 14 ; Ldc_i4_s
    jsr C64_SetChar ; Call
    nop ; Nop
    #stack_push_int 0 ; Ldnull
    #stack_duplicate ; Dup
    #stack_pull_int_ref GCTest_field_s_a2, 1 ; Stsfld
    #stack_pull_int_ref GCTest_field_s_a1, 1 ; Stsfld
GCTest_Two_Instances_First_GCd_Static_Field_236:    #method_exit 8 ; Ret


;----------------------------------------
; TYPE: Demo.GCTest
; METHOD: Single_Instance_GC
;----------------------------------------
GCTest_Single_Instance_GC 
    #init_locals_pull_parameters 3, []
    nop ; Nop
    #newObj 2, 1 ; Newobj
    #locals_pull_value_8 1, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #locals_push_value_8 1 ; Ldloc_0
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 2, 0 ; Stloc_1
    #stack_push_int 0 ; Ldnull
    #locals_pull_value_8 1, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #locals_push_value_8 2 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 3, 0 ; Stloc_2
    #locals_push_value_8 3 ; Ldloc_2
    #branch_false GCTest_Single_Instance_GC_50 ; Brfalse_s
    #stack_push_pointer string_1879048299 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Single_Instance_GC_50:    #method_exit 5 ; Ret


;----------------------------------------
; TYPE: Demo.GCTest
; METHOD: Change_Ref_Local_Variable
;----------------------------------------
GCTest_Change_Ref_Local_Variable 
    #init_locals_pull_parameters 3, []
    nop ; Nop
    #newObj 2, 1 ; Newobj
    #locals_pull_value_8 1, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #locals_push_value_8 1 ; Ldloc_0
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 2, 0 ; Stloc_1
    #newObj 2, 1 ; Newobj
    #locals_pull_value_8 1, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #locals_push_value_8 2 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 3, 0 ; Stloc_2
    #locals_push_value_8 3 ; Ldloc_2
    #branch_false GCTest_Change_Ref_Local_Variable_54 ; Brfalse_s
    #stack_push_pointer string_1879048299 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Change_Ref_Local_Variable_54:    #method_exit 5 ; Ret


;----------------------------------------
; TYPE: Demo.GCTest
; METHOD: Foo
;----------------------------------------
GCTest_Foo 
    #init_locals_pull_parameters 1, [1]
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #locals_pull_value_8 2, 1 ; Stloc_0
    #method_exit 4 ; Ret


;----------------------------------------
; TYPE: Demo.GCTest
; METHOD: Passed_As_Parameter_Deferenced
;----------------------------------------
GCTest_Passed_As_Parameter_Deferenced 
    #init_locals_pull_parameters 3, []
    nop ; Nop
    #newObj 2, 1 ; Newobj
    #locals_pull_value_8 1, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #locals_push_value_8 1 ; Ldloc_0
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 2, 0 ; Stloc_1
    #locals_push_value_8 1 ; Ldloc_0
    jsr GCTest_Foo ; Call
    nop ; Nop
    #newObj 2, 1 ; Newobj
    #locals_pull_value_8 1, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #locals_push_value_8 2 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 3, 0 ; Stloc_2
    #locals_push_value_8 3 ; Ldloc_2
    #branch_false GCTest_Passed_As_Parameter_Deferenced_61 ; Brfalse_s
    #stack_push_pointer string_1879048299 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Passed_As_Parameter_Deferenced_61:    #method_exit 5 ; Ret


;----------------------------------------
; TYPE: Demo.GCTest
; METHOD: Array_Root_in_Local_Var
;----------------------------------------
GCTest_Array_Root_in_Local_Var 
    #init_locals_pull_parameters 4, []
    nop ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #newArr  ; Newarr
    #stack_duplicate ; Dup
    #stack_push_int 0 ; Ldc_i4_0
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 23 ; Ldc_i4_s
    #stfld 1 ; Stfld
    #stelemRef ; Stelem_ref
    #locals_pull_value_8 1, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 0 ; Ldc_i4_0
    #ldelemRef ; Ldelem_ref
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 2, 0 ; Stloc_1
    jsr GC_Collect ; Call
    nop ; Nop
    jsr C64_get_Debug ; Call
    #locals_push_value_8 2 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 3, 0 ; Stloc_2
    #locals_push_value_8 3 ; Ldloc_2
    #branch_false GCTest_Array_Root_in_Local_Var_73 ; Brfalse_s
    #stack_push_pointer string_1879048299 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Array_Root_in_Local_Var_73:    #stack_push_int 0 ; Ldnull
    #locals_pull_value_8 1, 1 ; Stloc_0
    jsr GC_Collect ; Call
    nop ; Nop
    jsr C64_get_Debug ; Call
    #locals_push_value_8 2 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #locals_pull_value_8 4, 0 ; Stloc_3
    #locals_push_value_8 4 ; Ldloc_3
    #branch_false GCTest_Array_Root_in_Local_Var_107 ; Brfalse_s
    #stack_push_pointer string_1879048331 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Array_Root_in_Local_Var_107:    #method_exit 6 ; Ret


;----------------------------------------
; TYPE: Demo.GCTest
; METHOD: Hierarchies_Local_Variable_Root
;----------------------------------------
GCTest_Hierarchies_Local_Variable_Root 
    #init_locals_pull_parameters 21, []
    nop ; Nop
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #newObj 2, 1 ; Newobj
    #stfld 0 ; Stfld
    #locals_pull_value_8 1, 1 ; Stloc_0
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 4 ; Ldc_i4_4
    #stfld 1 ; Stfld
    #locals_pull_value_8 2, 1 ; Stloc_1
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #locals_push_value_8 1 ; Ldloc_0
    #stfld 0 ; Stfld
    #locals_pull_value_8 3, 1 ; Stloc_2
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #locals_push_value_8 2 ; Ldloc_1
    #stfld 0 ; Stfld
    #locals_pull_value_8 4, 1 ; Stloc_3
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 3 ; Ldc_i4_3
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #locals_push_value_8 3 ; Ldloc_2
    #stfld 0 ; Stfld
    #locals_pull_value_8 5, 1 ; Stloc_s
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 6 ; Ldc_i4_6
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #locals_push_value_8 4 ; Ldloc_3
    #stfld 0 ; Stfld
    #locals_pull_value_8 6, 1 ; Stloc_s
    #stack_push_int 0 ; Ldnull
    #stack_duplicate ; Dup
    #locals_pull_value_8 5, 1 ; Stloc_s
    #stack_duplicate ; Dup
    #locals_pull_value_8 3, 1 ; Stloc_2
    #locals_pull_value_8 1, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #locals_push_value_8 1 ; Ldloc_0
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 7, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #locals_push_value_8 3 ; Ldloc_2
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 8, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #locals_push_value_8 5 ; Ldloc_s
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 9, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #locals_push_value_8 2 ; Ldloc_1
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 10, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #locals_push_value_8 4 ; Ldloc_3
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 11, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #locals_push_value_8 6 ; Ldloc_s
    jsr Debug_GetObjectId ; Callvirt
    #locals_pull_value_8 12, 0 ; Stloc_s
    jsr GC_Collect ; Call
    nop ; Nop
    #locals_push_value_8 6 ; Ldloc_s
    #ldfld 1 ; Ldfld
    #stack_push_int 6 ; Ldc_i4_6
    #compareEqual ; Ceq
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 13, 0 ; Stloc_s
    #locals_push_value_8 13 ; Ldloc_s
    #branch_false GCTest_Hierarchies_Local_Variable_Root_243 ; Brfalse_s
    #stack_push_pointer string_1879048367 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Hierarchies_Local_Variable_Root_243:    #locals_push_value_8 6 ; Ldloc_s
    #ldfld 0 ; Ldfld
    #ldfld 1 ; Ldfld
    #stack_push_int 5 ; Ldc_i4_5
    #compareEqual ; Ceq
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 14, 0 ; Stloc_s
    #locals_push_value_8 14 ; Ldloc_s
    #branch_false GCTest_Hierarchies_Local_Variable_Root_278 ; Brfalse_s
    #stack_push_pointer string_1879048389 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Hierarchies_Local_Variable_Root_278:    #locals_push_value_8 6 ; Ldloc_s
    #ldfld 0 ; Ldfld
    #ldfld 0 ; Ldfld
    #ldfld 1 ; Ldfld
    #stack_push_int 4 ; Ldc_i4_4
    #compareEqual ; Ceq
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 15, 0 ; Stloc_s
    #locals_push_value_8 15 ; Ldloc_s
    #branch_false GCTest_Hierarchies_Local_Variable_Root_318 ; Brfalse_s
    #stack_push_pointer string_1879048423 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Hierarchies_Local_Variable_Root_318:    jsr C64_get_Debug ; Call
    #locals_push_value_8 10 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 16, 0 ; Stloc_s
    #locals_push_value_8 16 ; Ldloc_s
    #branch_false GCTest_Hierarchies_Local_Variable_Root_350 ; Brfalse_s
    #stack_push_pointer string_1879048469 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Hierarchies_Local_Variable_Root_350:    jsr C64_get_Debug ; Call
    #locals_push_value_8 11 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 17, 0 ; Stloc_s
    #locals_push_value_8 17 ; Ldloc_s
    #branch_false GCTest_Hierarchies_Local_Variable_Root_382 ; Brfalse_s
    #stack_push_pointer string_1879048503 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Hierarchies_Local_Variable_Root_382:    jsr C64_get_Debug ; Call
    #locals_push_value_8 12 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #locals_pull_value_8 18, 0 ; Stloc_s
    #locals_push_value_8 18 ; Ldloc_s
    #branch_false GCTest_Hierarchies_Local_Variable_Root_414 ; Brfalse_s
    #stack_push_pointer string_1879048537 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Hierarchies_Local_Variable_Root_414:    jsr C64_get_Debug ; Call
    #locals_push_value_8 7 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #locals_pull_value_8 19, 0 ; Stloc_s
    #locals_push_value_8 19 ; Ldloc_s
    #branch_false GCTest_Hierarchies_Local_Variable_Root_443 ; Brfalse_s
    #stack_push_pointer string_1879048571 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Hierarchies_Local_Variable_Root_443:    jsr C64_get_Debug ; Call
    #locals_push_value_8 8 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #locals_pull_value_8 20, 0 ; Stloc_s
    #locals_push_value_8 20 ; Ldloc_s
    #branch_false GCTest_Hierarchies_Local_Variable_Root_472 ; Brfalse_s
    #stack_push_pointer string_1879048597 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Hierarchies_Local_Variable_Root_472:    jsr C64_get_Debug ; Call
    #locals_push_value_8 9 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #locals_pull_value_8 21, 0 ; Stloc_s
    #locals_push_value_8 21 ; Ldloc_s
    #branch_false GCTest_Hierarchies_Local_Variable_Root_501 ; Brfalse_s
    #stack_push_pointer string_1879048623 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
GCTest_Hierarchies_Local_Variable_Root_501:    #method_exit 23 ; Ret


;----------------------------------------
; TYPE: Demo.GCTest
; METHOD: Start
;----------------------------------------
GCTest_Start 
    #init_locals_pull_parameters 0, []
    nop ; Nop
    jsr GCTest_Single_Instance_GC ; Call
    nop ; Nop
    jsr GCTest_Change_Ref_Local_Variable ; Call
    nop ; Nop
    jsr GCTest_Two_Instances_First_GCd_Static_Field ; Call
    nop ; Nop
    jsr GCTest_Array_Root_in_Local_Var ; Call
    nop ; Nop
    jsr GCTest_Hierarchies_Local_Variable_Root ; Call
    nop ; Nop
    jsr GCTest_Passed_As_Parameter_Deferenced ; Call
    nop ; Nop
    #method_exit 2 ; Ret
