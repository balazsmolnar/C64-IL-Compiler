

;----------------------------------------
; TYPE: Platform
; METHOD: Draw
;----------------------------------------
Platform_Draw 
    #stack_save_return_adress Platform_Draw_ReturnAddress
    #stack_pull_int Platform_Draw_this
    nop ; Nop
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld Platform_Draw_this, 0 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stack_pull_int_ref Platform_Draw_var0, 0 ; Stloc_0
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld Platform_Draw_this, 0 ; Nop
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 5 ; Ldfld
    #pushfld Platform_Draw_this, 5 ; Nop
    #add ; Add
    #stack_pull_int_ref Platform_Draw_var1, 0 ; Stloc_1
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld Platform_Draw_this, 1 ; Nop
    #stack_pull_int_ref Platform_Draw_var2, 0 ; Stloc_2
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 1 ; Ldfld
    #pushfld Platform_Draw_this, 1 ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stack_pull_int_ref Platform_Draw_var3, 0 ; Stloc_3
    #stack_push_var Platform_Draw_this ; Ldarg_0
    #stack_push_var Platform_Draw_var2 ; Ldloc_2
    #stack_push_int 3 ; Ldc_i4_3
    #shift_left ; Shl
    #stack_push_int 33 ; Ldc_i4_s
    #add ; Add
    #stfld 2 ; Stfld
    #stack_push_var Platform_Draw_this ; Ldarg_0
    #stack_push_var Platform_Draw_var0 ; Ldloc_0
    #stack_push_int 3 ; Ldc_i4_3
    #shift_left ; Shl
    #stack_push_int 3 ; Ldc_i4_3
    #add ; Add
    #stfld 3 ; Stfld
    #stack_push_var Platform_Draw_this ; Ldarg_0
    #stack_push_var Platform_Draw_var1 ; Ldloc_1
    #stack_push_int 3 ; Ldc_i4_3
    #shift_left ; Shl
    #stack_push_int 20 ; Ldc_i4_s
    #add ; Add
    #stfld 4 ; Stfld
    #stack_push_var Platform_Draw_var0 ; Ldloc_0
    #stack_pull_int_ref Platform_Draw_var4, 0 ; Stloc_s
    jmp label_Platform_Draw_122 ; Br_s
label_Platform_Draw_80:    nop ; Nop
    #stack_push_var Platform_Draw_var4 ; Ldloc_s
    #stack_push_var Platform_Draw_var2 ; Ldloc_2
    #stack_push_int 67 ; Ldc_i4_s
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld Platform_Draw_this, 6 ; Nop
    jsr C64_SetChar ; Call
    nop ; Nop
    #stack_push_var Platform_Draw_var4 ; Ldloc_s
    #stack_push_var Platform_Draw_var3 ; Ldloc_3
    #stack_push_int 67 ; Ldc_i4_s
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld Platform_Draw_this, 6 ; Nop
    jsr C64_SetChar ; Call
    nop ; Nop
    nop ; Nop
    #stack_push_var Platform_Draw_var4 ; Ldloc_s
    #stack_push_int 1 ; Ldc_i4_1
    #add ; Add
    #stack_pull_int_ref Platform_Draw_var4, 0 ; Stloc_s
label_Platform_Draw_122:    #stack_push_var Platform_Draw_var4 ; Ldloc_s
    #stack_push_var Platform_Draw_var1 ; Ldloc_1
    #compareLess ; Clt_un
    #stack_pull_int_ref Platform_Draw_var5, 0 ; Stloc_s
    #stack_push_var Platform_Draw_var5 ; Ldloc_s
    #branch_true label_Platform_Draw_80 ; Brtrue_s
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld Platform_Draw_this, 0 ; Nop
    #stack_push_var Platform_Draw_var2 ; Ldloc_2
    #stack_push_int 85 ; Ldc_i4_s
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld Platform_Draw_this, 6 ; Nop
    jsr C64_SetChar ; Call
    nop ; Nop
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 0 ; Ldfld
    #pushfld Platform_Draw_this, 0 ; Nop
    #stack_push_var Platform_Draw_var3 ; Ldloc_3
    #stack_push_int 74 ; Ldc_i4_s
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld Platform_Draw_this, 6 ; Nop
    jsr C64_SetChar ; Call
    nop ; Nop
    #stack_push_var Platform_Draw_var1 ; Ldloc_1
    #stack_push_var Platform_Draw_var2 ; Ldloc_2
    #stack_push_int 73 ; Ldc_i4_s
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld Platform_Draw_this, 6 ; Nop
    jsr C64_SetChar ; Call
    nop ; Nop
    #stack_push_var Platform_Draw_var1 ; Ldloc_1
    #stack_push_var Platform_Draw_var3 ; Ldloc_3
    #stack_push_int 75 ; Ldc_i4_s
  ; OPT   #stack_push_var Platform_Draw_this ; Ldarg_0
  ; OPT   #ldfld 6 ; Ldfld
    #pushfld Platform_Draw_this, 6 ; Nop
    jsr C64_SetChar ; Call
    nop ; Nop
    #stack_return_to_saved_address Platform_Draw_ReturnAddress ; Ret
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
    #stack_save_return_adress Platform_DistanceToPlatform_ReturnAddress
    #stack_pull_int_ref Platform_DistanceToPlatform_y, 0
    #stack_pull_int_ref Platform_DistanceToPlatform_x, 0
    #stack_pull_int Platform_DistanceToPlatform_this
    nop ; Nop
    #stack_push_var Platform_DistanceToPlatform_x ; Ldarg_1
  ; OPT   #stack_push_var Platform_DistanceToPlatform_this ; Ldarg_0
  ; OPT   #ldfld 3 ; Ldfld
    #pushfld Platform_DistanceToPlatform_this, 3 ; Nop
    #compareLess ; Clt_un
    #stack_pull_int_ref Platform_DistanceToPlatform_var1, 0 ; Stloc_1
    #stack_push_var Platform_DistanceToPlatform_var1 ; Ldloc_1
    #branch_false label_Platform_DistanceToPlatform_22 ; Brfalse_s
  ; OPT   #stack_push_int 255 ; Ldc_i4
  ; OPT   #stack_pull_int_ref Platform_DistanceToPlatform_var2, 0 ; Stloc_2
    #init_var Platform_DistanceToPlatform_var2, 255 ; Nop
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_22:    #stack_push_var Platform_DistanceToPlatform_x ; Ldarg_1
  ; OPT   #stack_push_var Platform_DistanceToPlatform_this ; Ldarg_0
  ; OPT   #ldfld 4 ; Ldfld
    #pushfld Platform_DistanceToPlatform_this, 4 ; Nop
    #compareGreater ; Cgt_un
    #stack_pull_int_ref Platform_DistanceToPlatform_var3, 0 ; Stloc_3
    #stack_push_var Platform_DistanceToPlatform_var3 ; Ldloc_3
    #branch_false label_Platform_DistanceToPlatform_43 ; Brfalse_s
  ; OPT   #stack_push_int 255 ; Ldc_i4
  ; OPT   #stack_pull_int_ref Platform_DistanceToPlatform_var2, 0 ; Stloc_2
    #init_var Platform_DistanceToPlatform_var2, 255 ; Nop
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_43:    #stack_push_var Platform_DistanceToPlatform_y ; Ldarg_2
  ; OPT   #stack_push_var Platform_DistanceToPlatform_this ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld Platform_DistanceToPlatform_this, 2 ; Nop
    #compareGreater ; Cgt_un
    #stack_pull_int_ref Platform_DistanceToPlatform_var4, 0 ; Stloc_s
    #stack_push_var Platform_DistanceToPlatform_var4 ; Ldloc_s
    #branch_false label_Platform_DistanceToPlatform_66 ; Brfalse_s
  ; OPT   #stack_push_int 255 ; Ldc_i4
  ; OPT   #stack_pull_int_ref Platform_DistanceToPlatform_var2, 0 ; Stloc_2
    #init_var Platform_DistanceToPlatform_var2, 255 ; Nop
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_66:  ; OPT   #stack_push_var Platform_DistanceToPlatform_this ; Ldarg_0
  ; OPT   #ldfld 2 ; Ldfld
    #pushfld Platform_DistanceToPlatform_this, 2 ; Nop
    #stack_push_var Platform_DistanceToPlatform_y ; Ldarg_2
    #sub ; Sub
    #stack_pull_int_ref Platform_DistanceToPlatform_var0, 0 ; Stloc_0
    #stack_push_var Platform_DistanceToPlatform_var0 ; Ldloc_0
    #stack_pull_int_ref Platform_DistanceToPlatform_var2, 0 ; Stloc_2
    jmp label_Platform_DistanceToPlatform_79 ; Br_s
label_Platform_DistanceToPlatform_79:    #stack_push_var Platform_DistanceToPlatform_var2 ; Ldloc_2
    #stack_return_to_saved_address Platform_DistanceToPlatform_ReturnAddress ; Ret
Platform_DistanceToPlatform_this .byte 0, 0
Platform_DistanceToPlatform_x .byte 0, 0
Platform_DistanceToPlatform_y .byte 0, 0
Platform_DistanceToPlatform_ReturnAddress .byte 0,0
Platform_DistanceToPlatform_var0 .byte 0,0
Platform_DistanceToPlatform_var1 .byte 0,0
Platform_DistanceToPlatform_var2 .byte 0,0
Platform_DistanceToPlatform_var3 .byte 0,0
Platform_DistanceToPlatform_var4 .byte 0,0
