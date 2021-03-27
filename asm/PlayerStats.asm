

;----------------------------------------
; TYPE: Hunchback.PlayerStats
; METHOD: Draw
;----------------------------------------
PlayerStats_Draw 
    #init_locals_pull_parameters 7, [0]
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp PlayerStats_Draw_84 ; Br_s
PlayerStats_Draw_7:    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #init_var 4, 0 ; Nop
    jmp PlayerStats_Draw_68 ; Br_s
PlayerStats_Draw_12:    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049095 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer txt_PlayerStats ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value_8 5, 0 ; Stloc_3
  ; OPT   #stack_push_pointer string_1879049127 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_PlayerStatsColours ; Nop
    #locals_push_value_8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 4 ; Ldloc_2
    #locals_push_value_8 3 ; Ldloc_1
    #locals_push_value_8 5 ; Ldloc_3
    #locals_push_value_8 6 ; Ldloc_s
    jsr C64_SetChar ; Call
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
    nop ; Nop
  ; OPT   #locals_push_value_8 4 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
PlayerStats_Draw_68:    #locals_push_value_8 4 ; Ldloc_2
    #stack_push_int 40 ; Ldc_i4_s
    #compareLess ; Clt_un
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_true PlayerStats_Draw_12 ; Brtrue_s
    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
PlayerStats_Draw_84:    #locals_push_value_8 3 ; Ldloc_1
    #stack_push_int 2 ; Ldc_i4_2
    #compareLess ; Clt_un
    #locals_pull_value_8 8, 0 ; Stloc_s
    #locals_push_value_8 8 ; Ldloc_s
    #branch_true PlayerStats_Draw_7 ; Brtrue_s
    #method_exit 10, [] ; Ret
