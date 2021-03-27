

;----------------------------------------
; TYPE: Hunchback.TitleScreen
; METHOD: Display
;----------------------------------------
TitleScreen_Display 
    #init_locals_pull_parameters 7, []
    nop ; Nop
    #stack_push_int 1 ; Ldc_i4_1
    jsr Screen_Clear ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049513 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$CD) ; Nop
  ; OPT   #stack_push_pointer string_1879049551 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer titleScreen ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_CopyMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049575 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$1CD) ; Nop
  ; OPT   #stack_push_pointer string_1879049615 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer titleScreen+$100 ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_CopyMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049649 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$21B) ; Nop
  ; OPT   #stack_push_pointer string_1879049689 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer titleScreen+$14E ; Nop
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_CopyMemory ; Call
    nop ; Nop
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 1 ; Ldc_i4_1
    #stack_push_pointer string_1879049723 ; Ldstr
    #stack_push_int 5 ; Ldc_i4_5
    jsr C64_Write ; Call
    nop ; Nop
    #stack_push_int 13 ; Ldc_i4_s
    #stack_push_int 3 ; Ldc_i4_3
    #stack_push_pointer string_1879049753 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    nop ; Nop
    #stack_push_int 25 ; Ldc_i4_s
    #stack_push_int 21 ; Ldc_i4_s
    #stack_push_pointer string_1879049783 ; Ldstr
    #stack_push_int 5 ; Ldc_i4_5
    jsr C64_Write ; Call
    nop ; Nop
    #stack_push_int 7 ; Ldc_i4_7
    #stack_push_int 23 ; Ldc_i4_s
    #stack_push_pointer string_1879049813 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    nop ; Nop
    #stack_push_int 7 ; Ldc_i4_7
    #stack_push_int 24 ; Ldc_i4_s
    #stack_push_pointer string_1879049865 ; Ldstr
    #stack_push_int 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    nop ; Nop
    jmp TitleScreen_Display_309 ; Br
TitleScreen_Display_170:    nop ; Nop
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #init_var 1, 1 ; Nop
    jmp TitleScreen_Display_295 ; Br_s
TitleScreen_Display_175:    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049917 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$CD) ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049953 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$1CD) ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049991 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$21B) ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #init_var 2, 0 ; Nop
    jmp TitleScreen_Display_274 ; Br_s
TitleScreen_Display_234:    nop ; Nop
  ; OPT   #stack_push_int 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_2
    #init_var 3, 0 ; Nop
    jmp TitleScreen_Display_258 ; Br_s
TitleScreen_Display_239:    nop ; Nop
    #stack_push_int 26 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    #locals_pull_value_8 4, 0 ; Stloc_3
    #locals_push_value_8 4 ; Ldloc_3
    #branch_false TitleScreen_Display_253 ; Brfalse_s
    jmp TitleScreen_Display_314 ; Br_s
TitleScreen_Display_253:    nop ; Nop
  ; OPT   #locals_push_value_8 3 ; Ldloc_2
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 3, 0 ; Stloc_2
    #inc_var 3 ; Nop
TitleScreen_Display_258:    #locals_push_value_8 3 ; Ldloc_2
    #stack_push_int 10 ; Ldc_i4_s
    #compareLess ; Clt
    #locals_pull_value_8 5, 0 ; Stloc_s
    #locals_push_value_8 5 ; Ldloc_s
    #branch_true TitleScreen_Display_239 ; Brtrue_s
    nop ; Nop
  ; OPT   #locals_push_value_8 2 ; Ldloc_1
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 2, 0 ; Stloc_1
    #inc_var 2 ; Nop
TitleScreen_Display_274:    #locals_push_value_8 2 ; Ldloc_1
    nop ;Conv_i8 ; Conv_i8
    #stack_push_int 255 ; Ldc_i4
    nop ;Conv_i8 ; Conv_i8
    #compareLess ; Clt
    #locals_pull_value_8 6, 0 ; Stloc_s
    #locals_push_value_8 6 ; Ldloc_s
    #branch_true TitleScreen_Display_234 ; Brtrue_s
    nop ; Nop
  ; OPT   #locals_push_value_8 1 ; Ldloc_0
  ; OPT   #stack_push_int 1 ; Ldc_i4_1
  ; OPT   #add ; Add
  ; OPT   #locals_pull_value_8 1, 0 ; Stloc_0
    #inc_var 1 ; Nop
TitleScreen_Display_295:    #locals_push_value_8 1 ; Ldloc_0
    #stack_push_int 8 ; Ldc_i4_8
    #compareLess ; Clt_un
    #locals_pull_value_8 7, 0 ; Stloc_s
    #locals_push_value_8 7 ; Ldloc_s
    #branch_true TitleScreen_Display_175 ; Brtrue
    nop ; Nop
TitleScreen_Display_309:    jmp TitleScreen_Display_170 ; Br
TitleScreen_Display_314:    #method_exit 9, [] ; Ret
