GCTest_field_s_a1 .byte 0
GCTest_field_s_a2 .byte 0


;----------------------------------------
; TYPE: GCTest
; METHOD: Two_Instances_First_GCd_Static_Field
;----------------------------------------
GCTest_Two_Instances_First_GCd_Static_Field 
    #stack_save_return_adress GCTest_Two_Instances_First_GCd_Static_Field_ReturnAddress
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
    #stack_pull_int_ref GCTest_Two_Instances_First_GCd_Static_Field_var0, 0 ; Stloc_0
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_field_s_a2 ; Ldsfld
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Two_Instances_First_GCd_Static_Field_var1, 0 ; Stloc_1
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Two_Instances_First_GCd_Static_Field_var0 ; Ldloc_0
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Two_Instances_First_GCd_Static_Field_var2, 0 ; Stloc_2
    #stack_push_var GCTest_Two_Instances_First_GCd_Static_Field_var2 ; Ldloc_2
    #branch_false label_GCTest_Two_Instances_First_GCd_Static_Field_102 ; Brfalse_s
    nop ; Nop
    #stack_push_pointer string_1879048193 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
    jmp label_GCTest_Two_Instances_First_GCd_Static_Field_236 ; Br
label_GCTest_Two_Instances_First_GCd_Static_Field_102:    #stack_push_int 0 ; Ldnull
    #stack_pull_int_ref GCTest_field_s_a1, 1 ; Stsfld
    jsr GC_Collect ; Call
    nop ; Nop
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Two_Instances_First_GCd_Static_Field_var0 ; Ldloc_0
    jsr Debug_IsAlive ; Callvirt
    #stack_pull_int_ref GCTest_Two_Instances_First_GCd_Static_Field_var3, 0 ; Stloc_3
    #stack_push_var GCTest_Two_Instances_First_GCd_Static_Field_var3 ; Ldloc_3
    #branch_false label_GCTest_Two_Instances_First_GCd_Static_Field_140 ; Brfalse_s
    #stack_push_pointer string_1879048213 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Two_Instances_First_GCd_Static_Field_140:    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Two_Instances_First_GCd_Static_Field_var1 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Two_Instances_First_GCd_Static_Field_var4, 0 ; Stloc_s
    #stack_push_var GCTest_Two_Instances_First_GCd_Static_Field_var4 ; Ldloc_s
    #branch_false label_GCTest_Two_Instances_First_GCd_Static_Field_171 ; Brfalse_s
    #stack_push_pointer string_1879048245 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Two_Instances_First_GCd_Static_Field_171:    #stack_push_var GCTest_field_s_a2 ; Ldsfld
    #ldfld 1 ; Ldfld
    #stack_push_int 5 ; Ldc_i4_5
    #compareEqual ; Ceq
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Two_Instances_First_GCd_Static_Field_var5, 0 ; Stloc_s
    #stack_push_var GCTest_Two_Instances_First_GCd_Static_Field_var5 ; Ldloc_s
    #branch_false label_GCTest_Two_Instances_First_GCd_Static_Field_204 ; Brfalse_s
    #stack_push_pointer string_1879048273 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Two_Instances_First_GCd_Static_Field_204:    #stack_push_int 0 ; Ldc_i4_0
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
label_GCTest_Two_Instances_First_GCd_Static_Field_236:    #stack_return_to_saved_address GCTest_Two_Instances_First_GCd_Static_Field_ReturnAddress ; Ret
GCTest_Two_Instances_First_GCd_Static_Field_ReturnAddress .byte 0,0
GCTest_Two_Instances_First_GCd_Static_Field_var0 .byte 0,0
GCTest_Two_Instances_First_GCd_Static_Field_var1 .byte 0,0
GCTest_Two_Instances_First_GCd_Static_Field_var2 .byte 0,0
GCTest_Two_Instances_First_GCd_Static_Field_var3 .byte 0,0
GCTest_Two_Instances_First_GCd_Static_Field_var4 .byte 0,0
GCTest_Two_Instances_First_GCd_Static_Field_var5 .byte 0,0


;----------------------------------------
; TYPE: GCTest
; METHOD: Single_Instance_GC
;----------------------------------------
GCTest_Single_Instance_GC 
    #stack_save_return_adress GCTest_Single_Instance_GC_ReturnAddress
    nop ; Nop
    #newObj 2, 1 ; Newobj
    #stack_pull_int_ref GCTest_Single_Instance_GC_var0, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Single_Instance_GC_var0 ; Ldloc_0
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Single_Instance_GC_var1, 0 ; Stloc_1
    #stack_push_int 0 ; Ldnull
    #stack_pull_int_ref GCTest_Single_Instance_GC_var0, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Single_Instance_GC_var1 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Single_Instance_GC_var2, 0 ; Stloc_2
    #stack_push_var GCTest_Single_Instance_GC_var2 ; Ldloc_2
    #branch_false label_GCTest_Single_Instance_GC_50 ; Brfalse_s
    #stack_push_pointer string_1879048299 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
    #deref GCTest_Single_Instance_GC_var0 ; Nop
label_GCTest_Single_Instance_GC_50:    #stack_return_to_saved_address GCTest_Single_Instance_GC_ReturnAddress ; Ret
GCTest_Single_Instance_GC_ReturnAddress .byte 0,0
GCTest_Single_Instance_GC_var0 .byte 0,0
GCTest_Single_Instance_GC_var1 .byte 0,0
GCTest_Single_Instance_GC_var2 .byte 0,0


;----------------------------------------
; TYPE: GCTest
; METHOD: Change_Ref_Local_Variable
;----------------------------------------
GCTest_Change_Ref_Local_Variable 
    #stack_save_return_adress GCTest_Change_Ref_Local_Variable_ReturnAddress
    nop ; Nop
    #newObj 2, 1 ; Newobj
    #stack_pull_int_ref GCTest_Change_Ref_Local_Variable_var0, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Change_Ref_Local_Variable_var0 ; Ldloc_0
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Change_Ref_Local_Variable_var1, 0 ; Stloc_1
    #newObj 2, 1 ; Newobj
    #stack_pull_int_ref GCTest_Change_Ref_Local_Variable_var0, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Change_Ref_Local_Variable_var1 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Change_Ref_Local_Variable_var2, 0 ; Stloc_2
    #stack_push_var GCTest_Change_Ref_Local_Variable_var2 ; Ldloc_2
    #branch_false label_GCTest_Change_Ref_Local_Variable_54 ; Brfalse_s
    #stack_push_pointer string_1879048299 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
    #deref GCTest_Change_Ref_Local_Variable_var0 ; Nop
label_GCTest_Change_Ref_Local_Variable_54:    #stack_return_to_saved_address GCTest_Change_Ref_Local_Variable_ReturnAddress ; Ret
GCTest_Change_Ref_Local_Variable_ReturnAddress .byte 0,0
GCTest_Change_Ref_Local_Variable_var0 .byte 0,0
GCTest_Change_Ref_Local_Variable_var1 .byte 0,0
GCTest_Change_Ref_Local_Variable_var2 .byte 0,0


;----------------------------------------
; TYPE: GCTest
; METHOD: Array_Root_in_Local_Var
;----------------------------------------
GCTest_Array_Root_in_Local_Var 
    #stack_save_return_adress GCTest_Array_Root_in_Local_Var_ReturnAddress
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
    #stack_pull_int_ref GCTest_Array_Root_in_Local_Var_var0, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Array_Root_in_Local_Var_var0 ; Ldloc_0
    #stack_push_int 0 ; Ldc_i4_0
    #ldelemRef ; Ldelem_ref
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Array_Root_in_Local_Var_var1, 0 ; Stloc_1
    jsr GC_Collect ; Call
    nop ; Nop
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Array_Root_in_Local_Var_var1 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Array_Root_in_Local_Var_var2, 0 ; Stloc_2
    #stack_push_var GCTest_Array_Root_in_Local_Var_var2 ; Ldloc_2
    #branch_false label_GCTest_Array_Root_in_Local_Var_73 ; Brfalse_s
    #stack_push_pointer string_1879048299 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Array_Root_in_Local_Var_73:    #stack_push_int 0 ; Ldnull
    #stack_pull_int_ref GCTest_Array_Root_in_Local_Var_var0, 1 ; Stloc_0
    jsr GC_Collect ; Call
    nop ; Nop
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Array_Root_in_Local_Var_var1 ; Ldloc_1
    jsr Debug_IsAlive ; Callvirt
    #stack_pull_int_ref GCTest_Array_Root_in_Local_Var_var3, 0 ; Stloc_3
    #stack_push_var GCTest_Array_Root_in_Local_Var_var3 ; Ldloc_3
    #branch_false label_GCTest_Array_Root_in_Local_Var_107 ; Brfalse_s
    #stack_push_pointer string_1879048331 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
    #deref GCTest_Array_Root_in_Local_Var_var0 ; Nop
label_GCTest_Array_Root_in_Local_Var_107:    #stack_return_to_saved_address GCTest_Array_Root_in_Local_Var_ReturnAddress ; Ret
GCTest_Array_Root_in_Local_Var_ReturnAddress .byte 0,0
GCTest_Array_Root_in_Local_Var_var0 .byte 0,0
GCTest_Array_Root_in_Local_Var_var1 .byte 0,0
GCTest_Array_Root_in_Local_Var_var2 .byte 0,0
GCTest_Array_Root_in_Local_Var_var3 .byte 0,0


;----------------------------------------
; TYPE: GCTest
; METHOD: Hierarchies_Local_Variable_Root
;----------------------------------------
GCTest_Hierarchies_Local_Variable_Root 
    #stack_save_return_adress GCTest_Hierarchies_Local_Variable_Root_ReturnAddress
    nop ; Nop
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #newObj 2, 1 ; Newobj
    #stfld 0 ; Stfld
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var0, 1 ; Stloc_0
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 4 ; Ldc_i4_4
    #stfld 1 ; Stfld
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var1, 1 ; Stloc_1
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 2 ; Ldc_i4_2
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var0 ; Ldloc_0
    #stfld 0 ; Stfld
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var2, 1 ; Stloc_2
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 5 ; Ldc_i4_5
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var1 ; Ldloc_1
    #stfld 0 ; Stfld
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var3, 1 ; Stloc_3
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 3 ; Ldc_i4_3
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var2 ; Ldloc_2
    #stfld 0 ; Stfld
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var4, 1 ; Stloc_s
    #newObj 2, 1 ; Newobj
    #stack_duplicate ; Dup
    #stack_push_int 6 ; Ldc_i4_6
    #stfld 1 ; Stfld
    #stack_duplicate ; Dup
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var3 ; Ldloc_3
    #stfld 0 ; Stfld
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var5, 1 ; Stloc_s
    #stack_push_int 0 ; Ldnull
    #stack_duplicate ; Dup
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var4, 1 ; Stloc_s
    #stack_duplicate ; Dup
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var2, 1 ; Stloc_2
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var0, 1 ; Stloc_0
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var0 ; Ldloc_0
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var6, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var2 ; Ldloc_2
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var7, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var4 ; Ldloc_s
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var8, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var1 ; Ldloc_1
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var9, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var3 ; Ldloc_3
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var10, 0 ; Stloc_s
    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var5 ; Ldloc_s
    jsr Debug_GetObjectId ; Callvirt
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var11, 0 ; Stloc_s
    jsr GC_Collect ; Call
    nop ; Nop
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var5 ; Ldloc_s
    #ldfld 1 ; Ldfld
    #stack_push_int 6 ; Ldc_i4_6
    #compareEqual ; Ceq
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var12, 0 ; Stloc_s
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var12 ; Ldloc_s
    #branch_false label_GCTest_Hierarchies_Local_Variable_Root_243 ; Brfalse_s
    #stack_push_pointer string_1879048367 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Hierarchies_Local_Variable_Root_243:    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var5 ; Ldloc_s
    #ldfld 0 ; Ldfld
    #ldfld 1 ; Ldfld
    #stack_push_int 5 ; Ldc_i4_5
    #compareEqual ; Ceq
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var13, 0 ; Stloc_s
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var13 ; Ldloc_s
    #branch_false label_GCTest_Hierarchies_Local_Variable_Root_278 ; Brfalse_s
    #stack_push_pointer string_1879048389 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Hierarchies_Local_Variable_Root_278:    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var5 ; Ldloc_s
    #ldfld 0 ; Ldfld
    #ldfld 0 ; Ldfld
    #ldfld 1 ; Ldfld
    #stack_push_int 4 ; Ldc_i4_4
    #compareEqual ; Ceq
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var14, 0 ; Stloc_s
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var14 ; Ldloc_s
    #branch_false label_GCTest_Hierarchies_Local_Variable_Root_318 ; Brfalse_s
    #stack_push_pointer string_1879048423 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Hierarchies_Local_Variable_Root_318:    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var9 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var15, 0 ; Stloc_s
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var15 ; Ldloc_s
    #branch_false label_GCTest_Hierarchies_Local_Variable_Root_350 ; Brfalse_s
    #stack_push_pointer string_1879048469 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Hierarchies_Local_Variable_Root_350:    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var10 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var16, 0 ; Stloc_s
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var16 ; Ldloc_s
    #branch_false label_GCTest_Hierarchies_Local_Variable_Root_382 ; Brfalse_s
    #stack_push_pointer string_1879048503 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Hierarchies_Local_Variable_Root_382:    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var11 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #stack_push_int 0 ; Ldc_i4_0
    #compareEqual ; Ceq
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var17, 0 ; Stloc_s
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var17 ; Ldloc_s
    #branch_false label_GCTest_Hierarchies_Local_Variable_Root_414 ; Brfalse_s
    #stack_push_pointer string_1879048537 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Hierarchies_Local_Variable_Root_414:    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var6 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var18, 0 ; Stloc_s
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var18 ; Ldloc_s
    #branch_false label_GCTest_Hierarchies_Local_Variable_Root_443 ; Brfalse_s
    #stack_push_pointer string_1879048571 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Hierarchies_Local_Variable_Root_443:    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var7 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var19, 0 ; Stloc_s
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var19 ; Ldloc_s
    #branch_false label_GCTest_Hierarchies_Local_Variable_Root_472 ; Brfalse_s
    #stack_push_pointer string_1879048597 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
label_GCTest_Hierarchies_Local_Variable_Root_472:    jsr C64_get_Debug ; Call
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var8 ; Ldloc_s
    jsr Debug_IsAlive ; Callvirt
    #stack_pull_int_ref GCTest_Hierarchies_Local_Variable_Root_var20, 0 ; Stloc_s
    #stack_push_var GCTest_Hierarchies_Local_Variable_Root_var20 ; Ldloc_s
    #branch_false label_GCTest_Hierarchies_Local_Variable_Root_501 ; Brfalse_s
    #stack_push_pointer string_1879048623 ; Ldstr
    jsr Console_WriteLine ; Call
    nop ; Nop
    #deref GCTest_Hierarchies_Local_Variable_Root_var0 ; Nop
    #deref GCTest_Hierarchies_Local_Variable_Root_var1 ; Nop
    #deref GCTest_Hierarchies_Local_Variable_Root_var2 ; Nop
    #deref GCTest_Hierarchies_Local_Variable_Root_var3 ; Nop
    #deref GCTest_Hierarchies_Local_Variable_Root_var4 ; Nop
    #deref GCTest_Hierarchies_Local_Variable_Root_var5 ; Nop
label_GCTest_Hierarchies_Local_Variable_Root_501:    #stack_return_to_saved_address GCTest_Hierarchies_Local_Variable_Root_ReturnAddress ; Ret
GCTest_Hierarchies_Local_Variable_Root_ReturnAddress .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var0 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var1 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var2 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var3 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var4 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var5 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var6 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var7 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var8 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var9 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var10 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var11 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var12 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var13 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var14 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var15 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var16 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var17 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var18 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var19 .byte 0,0
GCTest_Hierarchies_Local_Variable_Root_var20 .byte 0,0


;----------------------------------------
; TYPE: GCTest
; METHOD: Start
;----------------------------------------
GCTest_Start 
    #stack_save_return_adress GCTest_Start_ReturnAddress
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
    #stack_return_to_saved_address GCTest_Start_ReturnAddress ; Ret
GCTest_Start_ReturnAddress .byte 0,0
