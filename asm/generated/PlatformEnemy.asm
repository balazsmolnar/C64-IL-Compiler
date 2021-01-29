

;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Platform
;----------------------------------------
PlatformEnemy_set_Platform 
    #stack_save_return_adress PlatformEnemy_set_Platform_ReturnAddress
    #stack_pull_int_ref PlatformEnemy_set_Platform_value, 1
    #stack_pull_int PlatformEnemy_set_Platform_this
    nop ; Nop
    #stack_push_var PlatformEnemy_set_Platform_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_Platform_value ; Ldarg_1
    #ldfld 3 ; Ldfld
    #stack_push_int 20 ; Ldc_i4_s
    #add ; Add
    jsr PlatformEnemy_set_MinX ; Call
    nop ; Nop
    #stack_push_var PlatformEnemy_set_Platform_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_Platform_value ; Ldarg_1
    #ldfld 4 ; Ldfld
    #stack_push_int 20 ; Ldc_i4_s
    #sub ; Sub
    jsr PlatformEnemy_set_MaxX ; Call
    nop ; Nop
    #stack_push_var PlatformEnemy_set_Platform_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_Platform_value ; Ldarg_1
    #ldfld 2 ; Ldfld
    #stack_push_int 2 ; Ldc_i4_2
    #sub ; Sub
    #stfld 1 ; Stfld
    #deref PlatformEnemy_set_Platform_value ; Nop
    #stack_return_to_saved_address PlatformEnemy_set_Platform_ReturnAddress ; Ret
PlatformEnemy_set_Platform_this .byte 0, 0
PlatformEnemy_set_Platform_value .byte 0, 0
PlatformEnemy_set_Platform_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Sprite
;----------------------------------------
PlatformEnemy_set_Sprite 
    #stack_save_return_adress PlatformEnemy_set_Sprite_ReturnAddress
    #stack_pull_int_ref PlatformEnemy_set_Sprite_value, 0
    #stack_pull_int PlatformEnemy_set_Sprite_this
    nop ; Nop
    #stack_push_var PlatformEnemy_set_Sprite_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_Sprite_value ; Ldarg_1
    #stfld 8 ; Stfld
    #stack_push_var PlatformEnemy_set_Sprite_this ; Ldarg_0
    #ldfld 8 ; Ldfld
    #stack_push_var PlatformEnemy_set_Sprite_this ; Ldarg_0
    #ldfld 2 ; Ldfld
    jsr Sprite_set_DataBlock ; Callvirt
    nop ; Nop
    #stack_push_var PlatformEnemy_set_Sprite_this ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 7 ; Stfld
    #stack_push_var PlatformEnemy_set_Sprite_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_Sprite_this ; Ldarg_0
    jsr PlatformEnemy_get_MinX ; Call
    jsr PlatformEnemy_set_X ; Call
    nop ; Nop
    #stack_push_var PlatformEnemy_set_Sprite_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_Sprite_this ; Ldarg_0
    jsr PlatformEnemy_get_Y ; Call
    jsr PlatformEnemy_set_Y ; Call
    nop ; Nop
    #stack_push_var PlatformEnemy_set_Sprite_this ; Ldarg_0
    #ldfld 8 ; Ldfld
    #stack_push_int 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    nop ; Nop
    #stack_return_to_saved_address PlatformEnemy_set_Sprite_ReturnAddress ; Ret
PlatformEnemy_set_Sprite_this .byte 0, 0
PlatformEnemy_set_Sprite_value .byte 0, 0
PlatformEnemy_set_Sprite_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Color
;----------------------------------------
PlatformEnemy_set_Color 
    #stack_save_return_adress PlatformEnemy_set_Color_ReturnAddress
    #stack_pull_int_ref PlatformEnemy_set_Color_value, 0
    #stack_pull_int PlatformEnemy_set_Color_this
    nop ; Nop
    #stack_push_var PlatformEnemy_set_Color_this ; Ldarg_0
    #ldfld 8 ; Ldfld
    #stack_push_var PlatformEnemy_set_Color_value ; Ldarg_1
    jsr Sprite_set_Color ; Callvirt
    nop ; Nop
    #stack_return_to_saved_address PlatformEnemy_set_Color_ReturnAddress ; Ret
PlatformEnemy_set_Color_this .byte 0, 0
PlatformEnemy_set_Color_value .byte 0, 0
PlatformEnemy_set_Color_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: Move
;----------------------------------------
PlatformEnemy_Move 
    #stack_save_return_adress PlatformEnemy_Move_ReturnAddress
    #stack_pull_int PlatformEnemy_Move_this
    nop ; Nop
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    #ldfld 7 ; Ldfld
    #stack_pull_int_ref PlatformEnemy_Move_var0, 0 ; Stloc_0
    #stack_push_var PlatformEnemy_Move_var0 ; Ldloc_0
    #branch_false label_PlatformEnemy_Move_60 ; Brfalse_s
    nop ; Nop
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    #ldfld 3 ; Ldfld
    #add ; Add
    jsr PlatformEnemy_set_X ; Call
    nop ; Nop
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_MaxX ; Call
    #compareGreater ; Cgt_un
    #stack_pull_int_ref PlatformEnemy_Move_var1, 0 ; Stloc_1
    #stack_push_var PlatformEnemy_Move_var1 ; Ldloc_1
    #branch_false label_PlatformEnemy_Move_57 ; Brfalse_s
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    #stfld 7 ; Stfld
label_PlatformEnemy_Move_57:    nop ; Nop
    jmp label_PlatformEnemy_Move_107 ; Br_s
label_PlatformEnemy_Move_60:    nop ; Nop
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    #ldfld 4 ; Ldfld
    #sub ; Sub
    jsr PlatformEnemy_set_X ; Call
    nop ; Nop
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_X ; Call
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    jsr PlatformEnemy_get_MinX ; Call
    #compareLess ; Clt_un
    #stack_pull_int_ref PlatformEnemy_Move_var2, 0 ; Stloc_2
    #stack_push_var PlatformEnemy_Move_var2 ; Ldloc_2
    #branch_false label_PlatformEnemy_Move_106 ; Brfalse_s
    #stack_push_var PlatformEnemy_Move_this ; Ldarg_0
    #stack_push_int 1 ; Ldc_i4_1
    #stfld 7 ; Stfld
label_PlatformEnemy_Move_106:    nop ; Nop
label_PlatformEnemy_Move_107:    #stack_return_to_saved_address PlatformEnemy_Move_ReturnAddress ; Ret
PlatformEnemy_Move_this .byte 0, 0
PlatformEnemy_Move_ReturnAddress .byte 0,0
PlatformEnemy_Move_var0 .byte 0,0
PlatformEnemy_Move_var1 .byte 0,0
PlatformEnemy_Move_var2 .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_X
;----------------------------------------
PlatformEnemy_get_X 
    #stack_save_return_adress PlatformEnemy_get_X_ReturnAddress
    #stack_pull_int PlatformEnemy_get_X_this
    nop ; Nop
    #stack_push_var PlatformEnemy_get_X_this ; Ldarg_0
    #ldfld 0 ; Ldfld
    #stack_pull_int_ref PlatformEnemy_get_X_var0, 0 ; Stloc_0
    jmp label_PlatformEnemy_get_X_10 ; Br_s
label_PlatformEnemy_get_X_10:    #stack_push_var PlatformEnemy_get_X_var0 ; Ldloc_0
    #stack_return_to_saved_address PlatformEnemy_get_X_ReturnAddress ; Ret
PlatformEnemy_get_X_this .byte 0, 0
PlatformEnemy_get_X_ReturnAddress .byte 0,0
PlatformEnemy_get_X_var0 .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_X
;----------------------------------------
PlatformEnemy_set_X 
    #stack_save_return_adress PlatformEnemy_set_X_ReturnAddress
    #stack_pull_int_ref PlatformEnemy_set_X_value, 0
    #stack_pull_int PlatformEnemy_set_X_this
    nop ; Nop
    #stack_push_var PlatformEnemy_set_X_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_X_value ; Ldarg_1
    #stfld 0 ; Stfld
    #stack_push_var PlatformEnemy_set_X_this ; Ldarg_0
    #ldfld 8 ; Ldfld
    #stack_push_var PlatformEnemy_set_X_value ; Ldarg_1
    jsr Sprite_set_X ; Callvirt
    nop ; Nop
    #stack_return_to_saved_address PlatformEnemy_set_X_ReturnAddress ; Ret
PlatformEnemy_set_X_this .byte 0, 0
PlatformEnemy_set_X_value .byte 0, 0
PlatformEnemy_set_X_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_Y
;----------------------------------------
PlatformEnemy_get_Y 
    #stack_save_return_adress PlatformEnemy_get_Y_ReturnAddress
    #stack_pull_int PlatformEnemy_get_Y_this
    nop ; Nop
    #stack_push_var PlatformEnemy_get_Y_this ; Ldarg_0
    #ldfld 1 ; Ldfld
    #stack_pull_int_ref PlatformEnemy_get_Y_var0, 0 ; Stloc_0
    jmp label_PlatformEnemy_get_Y_10 ; Br_s
label_PlatformEnemy_get_Y_10:    #stack_push_var PlatformEnemy_get_Y_var0 ; Ldloc_0
    #stack_return_to_saved_address PlatformEnemy_get_Y_ReturnAddress ; Ret
PlatformEnemy_get_Y_this .byte 0, 0
PlatformEnemy_get_Y_ReturnAddress .byte 0,0
PlatformEnemy_get_Y_var0 .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_Y
;----------------------------------------
PlatformEnemy_set_Y 
    #stack_save_return_adress PlatformEnemy_set_Y_ReturnAddress
    #stack_pull_int_ref PlatformEnemy_set_Y_value, 0
    #stack_pull_int PlatformEnemy_set_Y_this
    nop ; Nop
    #stack_push_var PlatformEnemy_set_Y_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_Y_value ; Ldarg_1
    #stfld 1 ; Stfld
    #stack_push_var PlatformEnemy_set_Y_this ; Ldarg_0
    #ldfld 8 ; Ldfld
    #stack_push_var PlatformEnemy_set_Y_value ; Ldarg_1
    jsr Sprite_set_Y ; Callvirt
    nop ; Nop
    #stack_return_to_saved_address PlatformEnemy_set_Y_ReturnAddress ; Ret
PlatformEnemy_set_Y_this .byte 0, 0
PlatformEnemy_set_Y_value .byte 0, 0
PlatformEnemy_set_Y_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_MinX
;----------------------------------------
PlatformEnemy_get_MinX 
    #stack_save_return_adress PlatformEnemy_get_MinX_ReturnAddress
    #stack_pull_int PlatformEnemy_get_MinX_this
    #stack_push_var PlatformEnemy_get_MinX_this ; Ldarg_0
    #ldfld 5 ; Ldfld
    #stack_return_to_saved_address PlatformEnemy_get_MinX_ReturnAddress ; Ret
PlatformEnemy_get_MinX_this .byte 0, 0
PlatformEnemy_get_MinX_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_MinX
;----------------------------------------
PlatformEnemy_set_MinX 
    #stack_save_return_adress PlatformEnemy_set_MinX_ReturnAddress
    #stack_pull_int_ref PlatformEnemy_set_MinX_value, 0
    #stack_pull_int PlatformEnemy_set_MinX_this
    #stack_push_var PlatformEnemy_set_MinX_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_MinX_value ; Ldarg_1
    #stfld 5 ; Stfld
    #stack_return_to_saved_address PlatformEnemy_set_MinX_ReturnAddress ; Ret
PlatformEnemy_set_MinX_this .byte 0, 0
PlatformEnemy_set_MinX_value .byte 0, 0
PlatformEnemy_set_MinX_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: get_MaxX
;----------------------------------------
PlatformEnemy_get_MaxX 
    #stack_save_return_adress PlatformEnemy_get_MaxX_ReturnAddress
    #stack_pull_int PlatformEnemy_get_MaxX_this
    #stack_push_var PlatformEnemy_get_MaxX_this ; Ldarg_0
    #ldfld 6 ; Ldfld
    #stack_return_to_saved_address PlatformEnemy_get_MaxX_ReturnAddress ; Ret
PlatformEnemy_get_MaxX_this .byte 0, 0
PlatformEnemy_get_MaxX_ReturnAddress .byte 0,0


;----------------------------------------
; TYPE: PlatformEnemy
; METHOD: set_MaxX
;----------------------------------------
PlatformEnemy_set_MaxX 
    #stack_save_return_adress PlatformEnemy_set_MaxX_ReturnAddress
    #stack_pull_int_ref PlatformEnemy_set_MaxX_value, 0
    #stack_pull_int PlatformEnemy_set_MaxX_this
    #stack_push_var PlatformEnemy_set_MaxX_this ; Ldarg_0
    #stack_push_var PlatformEnemy_set_MaxX_value ; Ldarg_1
    #stfld 6 ; Stfld
    #stack_return_to_saved_address PlatformEnemy_set_MaxX_ReturnAddress ; Ret
PlatformEnemy_set_MaxX_this .byte 0, 0
PlatformEnemy_set_MaxX_value .byte 0, 0
PlatformEnemy_set_MaxX_ReturnAddress .byte 0,0
