

;----------------------------------------
; TYPE: Hunchback.TitleScreen
; METHOD: Display
;----------------------------------------
TitleScreen_Display 
    #init_locals_pull_parameters 3, []
    #stack_push_int 1 ; Ldc_i4_1
    jsr Screen_Clear ; Call
  ; OPT   #stack_push_pointer string_1879050331 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$CD) ; Nop
  ; OPT   #stack_push_pointer string_1879050369 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer titleScreen ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_CopyMemory ; Call
  ; OPT   #stack_push_pointer string_1879050393 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$1CD) ; Nop
  ; OPT   #stack_push_pointer string_1879050433 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer titleScreen+$100 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_CopyMemory ; Call
  ; OPT   #stack_push_pointer string_1879050467 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$21B) ; Nop
  ; OPT   #stack_push_pointer string_1879050507 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer titleScreen+$14E ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_CopyMemory ; Call
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 1 ; Ldc_i4_1
    #stack_push_pointer string_1879050541 ; Ldstr
    #stack_push_int 5 ; Ldc_i4_5
    jsr C64_Write ; Call
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 3 ; Ldc_i4_3
    #stack_push_pointer string_1879050571 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    #stack_push_int 25 ; Ldc_i4_s
    #stack_push_int 21 ; Ldc_i4_s
    #stack_push_pointer string_1879050601 ; Ldstr
    #stack_push_int 5 ; Ldc_i4_5
    jsr C64_Write ; Call
    #stack_push_int 7 ; Ldc_i4_7
    #stack_push_int 23 ; Ldc_i4_s
    #stack_push_pointer string_1879050631 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    #stack_push_int 7 ; Ldc_i4_7
    #stack_push_int 24 ; Ldc_i4_s
    #stack_push_pointer string_1879050683 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
TitleScreen_Display_155:  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 1 ; Nop
    jmp TitleScreen_Display_253 ; Br_s
TitleScreen_Display_159:  ; OPT   #stack_push_pointer string_1879050735 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$CD) ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050771 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$1CD) ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050809 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$21B) ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #init_var 2, 0 ; Nop
    jmp TitleScreen_Display_241 ; Br_s
TitleScreen_Display_214:  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_2
    #init_var 3, 0 ; Nop
    jmp TitleScreen_Display_232 ; Br_s
TitleScreen_Display_218:    #stack_push_int 26 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    #branch_false TitleScreen_Display_228 ; Brfalse_s
    #method_exit 5, [] ; Ret
TitleScreen_Display_228:  ; OPT   #locals_push_value_8 3 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_2
    #inc_var 3 ; Nop
TitleScreen_Display_232:    #locals_push_value_8 3 ; Ldloc_2
  ; OPT   #stack_push_int 10 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned TitleScreen_Display_218 ; Blt_un_s
    #branch_less_unsigned_const 10, TitleScreen_Display_218 ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #inc_var 2 ; Nop
TitleScreen_Display_241:    #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 255 ; Ldc_i4
  ; OPT   #branch_less_unsigned TitleScreen_Display_214 ; Blt_un_s
    #branch_less_unsigned_const 255, TitleScreen_Display_214 ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
TitleScreen_Display_253:    #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 8 ; Ldc_i4_8
  ; OPT   #branch_less_unsigned TitleScreen_Display_159 ; Blt_un_s
    #branch_less_unsigned_const 8, TitleScreen_Display_159 ; Nop
    jmp TitleScreen_Display_155 ; Br_s
