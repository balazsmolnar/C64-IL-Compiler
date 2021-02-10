

;----------------------------------------
; TYPE: Platform
; METHOD: Draw
;----------------------------------------
Platform_Draw 
    #locals_init_locals 6
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 0 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #locals_pull_value_8 2, 0 ; Stloc_0
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 0 ; Ldfld
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 5 ; Ldfld
    #add ; Add
    #locals_pull_value_8 3, 0 ; Stloc_1
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 1 ; Ldfld
    #locals_pull_value_8 4, 0 ; Stloc_2
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 1 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #locals_pull_value_8 5, 0 ; Stloc_3
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 4 ; Ldloc_2
    #stack_push_int 3 ; Ldc_i4_3
    #shift_left ; Shl
    #stack_push_int 33 ; Ldc_i4_s
    #add ; Add
    #stfld 2 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 2 ; Ldloc_0
    #stack_push_int 3 ; Ldc_i4_3
    #shift_left ; Shl
    #stack_push_int 3 ; Ldc_i4_3
    #add ; Add
    #stfld 3 ; Stfld
    #locals_push_value_8 1 ; Ldarg_0
    #locals_push_value_8 3 ; Ldloc_1
    #stack_push_int 3 ; Ldc_i4_3
    #shift_left ; Shl
    #stack_push_int 20 ; Ldc_i4_s
    #add ; Add
    #stfld 4 ; Stfld
    #locals_push_value_8 2 ; Ldloc_0
    #locals_pull_value_8 6, 0 ; Stloc_s
    jmp label_Platform_Draw_122 ; Br_s
label_Platform_Draw_80:    nop ; Nop
    #locals_push_value_8 6 ; Ldloc_s
    #locals_push_value_8 4 ; Ldloc_2
    #stack_push_int 67 ; Ldc_i4_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 6 ; Ldloc_s
    #locals_push_value_8 5 ; Ldloc_3
    #stack_push_int 67 ; Ldc_i4_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    nop ; Nop
    #locals_push_value_8 6 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #locals_pull_value_8 6, 0 ; Stloc_s
label_Platform_Draw_122:    #locals_push_value_8 6 ; Ldloc_s
    #locals_push_value_8 3 ; Ldloc_1
    #compareLess ; Clt_un
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_true label_Platform_Draw_80 ; Brtrue_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 0 ; Ldfld
    #locals_push_value_8 4 ; Ldloc_2
    #stack_push_int 85 ; Ldc_i4_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 0 ; Ldfld
    #locals_push_value_8 5 ; Ldloc_3
    #stack_push_int 74 ; Ldc_i4_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_1
    #locals_push_value_8 4 ; Ldloc_2
    #stack_push_int 73 ; Ldc_i4_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_push_value_8 3 ; Ldloc_1
    #locals_push_value_8 5 ; Ldloc_3
    #stack_push_int 75 ; Ldc_i4_s
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 6 ; Ldfld
    jsr C64_SetChar ; Call
    nop ; Nop
    #locals_method_exit 9 ; Ret
Platform_Draw_this .byte 0, 0
Platform_Draw_ReturnAddress .byte 0,0
Platform_Draw_var0 .byte 0,0
Platform_Draw_var1 .byte 0,0
Platform_Draw_var2 .byte 0,0
Platform_Draw_var3 .byte 0,0
Platform_Draw_var4 .byte 0,0
Platform_Draw_var5 .byte 0,0


;----------------------------------------
; TYPE: Platform
; METHOD: DistanceToPlatform
;----------------------------------------
Platform_DistanceToPlatform 
    #locals_init_locals 5
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    #locals_pull_param_8 0
    nop ; Nop
    #locals_push_value_8 2 ; Ldarg_1
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 3 ; Ldfld
    #compareLess ; Clt_un
    #locals_pull_value_8 5, 0 ; Stloc_1
    #locals_push_value_8 5 ; Ldloc_1
    #branch_false label_Platform_DistanceToPlatform_22 ; Brfalse_s
    #stack_push_int 255 ; Ldc_i4
    #locals_pull_value_8 6, 0 ; Stloc_2
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_22:    #locals_push_value_8 2 ; Ldarg_1
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 4 ; Ldfld
    #compareGreater ; Cgt_un
    #locals_pull_value_8 7, 0 ; Stloc_3
    #locals_push_value_8 7 ; Ldloc_3
    #branch_false label_Platform_DistanceToPlatform_43 ; Brfalse_s
    #stack_push_int 255 ; Ldc_i4
    #locals_pull_value_8 6, 0 ; Stloc_2
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_43:    #locals_push_value_8 3 ; Ldarg_2
    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #compareGreater ; Cgt_un
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_false label_Platform_DistanceToPlatform_66 ; Brfalse_s
    #stack_push_int 255 ; Ldc_i4
    #locals_pull_value_8 6, 0 ; Stloc_2
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_66:    #locals_push_value_8 1 ; Ldarg_0
    #ldfld 2 ; Ldfld
    #locals_push_value_8 3 ; Ldarg_2
    #sub ; Sub
    #locals_pull_value_8 4, 0 ; Stloc_0
    #locals_push_value_8 4 ; Ldloc_0
    #locals_pull_value_8 6, 0 ; Stloc_2
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_79:    #locals_push_value_8 6 ; Ldloc_2
    #locals_method_exit 10 ; Ret
Platform_DistanceToPlatform_this .byte 0, 0
Platform_DistanceToPlatform_x .byte 0, 0
Platform_DistanceToPlatform_y .byte 0, 0
Platform_DistanceToPlatform_ReturnAddress .byte 0,0
Platform_DistanceToPlatform_var0 .byte 0,0
Platform_DistanceToPlatform_var1 .byte 0,0
Platform_DistanceToPlatform_var2 .byte 0,0
Platform_DistanceToPlatform_var3 .byte 0,0
Platform_DistanceToPlatform_var4 .byte 0,0
