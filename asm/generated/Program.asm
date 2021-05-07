

;----------------------------------------
; TYPE: Demo.Program
; METHOD: Foo
;----------------------------------------
Program_Foo 
    #init_locals_pull_parameters 0, [0,0, 0,0]
    #method_exit 6, [] ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 4, []
    #newObj 0, 0 ; Newobj
    #locals_pull_value8 1, 1 ; Stloc_0
    #locals_push_value8 1 ; Ldloc_0
    jsr Game_Start ; Callvirt
    jsr C64_get_Sprites ; Call
    jsr SpriteCollection_get_Sprite1 ; Callvirt
    #locals_pull_value8 2, 0 ; Stloc_1
    #locals_push_value8 2 ; Ldloc_1
  ; OPT   #stack_push_pointer string_1879048193 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer spt_player_right_0 ; Nop
    jsr Sprite_set_DataBlock ; Callvirt
    #locals_push_value8 2 ; Ldloc_1
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_MultiColor ; Callvirt
    #locals_push_value8 2 ; Ldloc_1
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Sprite_set_Visible ; Callvirt
    #locals_push_value8 2 ; Ldloc_1
    #stack_push_int8 5 ; Ldc_i4_5
    jsr Sprite_set_Color ; Callvirt
    #locals_push_value8 2 ; Ldloc_1
    #stack_push_int16 300 ; Ldc_i4
    ; conv ; Conv_i8
    jsr Sprite_set_X ; Callvirt
    #locals_push_value8 2 ; Ldloc_1
    #stack_push_int8 100 ; Ldc_i4_s
    jsr Sprite_set_Y ; Callvirt
    #stack_push_int16 0 ; Ldc_i4_0
    ; conv ; Conv_i8
    #locals_pull_value16 4, 0 ; Stloc_2
    jmp Program_Main_104 ; Br_s
Program_Main_85:    #locals_push_value8 2 ; Ldloc_1
    #locals_push_value16 4 ; Ldloc_2
    jsr Sprite_set_X ; Callvirt
    #stack_push_int8 10 ; Ldc_i4_s
    jsr Delay_Wait ; Call
    #locals_push_value16 4 ; Ldloc_2
    #stack_push_int16 1 ; Ldc_i4_1
    ; conv ; Conv_i8
    #add16 ; Add
    #locals_pull_value16 4, 0 ; Stloc_2
Program_Main_104:    #locals_push_value16 4 ; Ldloc_2
    #stack_push_int16 320 ; Ldc_i4
    ; conv ; Conv_i8
    #branch_less_unsigned16 Program_Main_85 ; Blt_un_s
    #method_exit 6, [1] ; Ret
