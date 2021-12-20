
PlayerStats_VTable:  .byte 0


;----------------------------------------
; TYPE: Hunchback.PlayerStats
; METHOD: Draw
;----------------------------------------
PlayerStats_Draw 
    #init_locals_pull_parameters 5, [0]
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #init_var 2, 0 ; Nop
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #init_var 3, 0 ; Nop
    jmp PlayerStats_Draw_72 ; Br_s
PlayerStats_Draw_6:  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 4, 0 ; Stloc_2
    #init_var 4, 0 ; Nop
    jmp PlayerStats_Draw_63 ; Br_s
PlayerStats_Draw_10:  ; OPT   #stack_push_pointer string_1879049213 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer txt_PlayerStats ; Nop
    #locals_push_value8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value8 5, 0 ; Stloc_3
  ; OPT   #stack_push_pointer string_1879049245 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer tbl_PlayerStatsColours ; Nop
    #locals_push_value8 2 ; Ldloc_0
    jsr C64_GetMemory ; Call
    #locals_pull_value8 6, 0 ; Stloc_s
    #locals_push_value8 4 ; Ldloc_2
    #locals_push_value8 3 ; Ldloc_1
    #locals_push_value8 5 ; Ldloc_3
    #locals_push_value8 6 ; Ldloc_s
    jsr C64_SetChar ; Call
  ; OPT   #locals_push_value8 2 ; Ldloc_0
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_0
    #inc_var 2 ; Nop
  ; OPT   #locals_push_value8 4 ; Ldloc_2
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 4, 0 ; Stloc_2
    #inc_var 4 ; Nop
PlayerStats_Draw_63:    #locals_push_value8 4 ; Ldloc_2
  ; OPT   #stack_push_int8 40 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned8 PlayerStats_Draw_10 ; Blt_un_s
    #branch_less_unsigned_const 40, PlayerStats_Draw_10 ; Nop
  ; OPT   #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 3, 0 ; Stloc_1
    #inc_var 3 ; Nop
PlayerStats_Draw_72:    #locals_push_value8 3 ; Ldloc_1
  ; OPT   #stack_push_int8 2 ; Ldc_i4_2
  ; OPT   #branch_less_unsigned8 PlayerStats_Draw_6 ; Blt_un_s
    #branch_less_unsigned_const 2, PlayerStats_Draw_6 ; Nop
    #method_exit 8, [] ; Ret
