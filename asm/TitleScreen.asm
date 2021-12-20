
TitleScreen_VTable:  .byte 0


;----------------------------------------
; TYPE: Hunchback.TitleScreen
; METHOD: Display
;----------------------------------------
TitleScreen_Display 
    #init_locals_pull_parameters 7, []
    #stack_push_int8 1 ; Ldc_i4_1
    jsr Screen_Clear ; Call
  ; OPT   #stack_push_pointer string_1879050331 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$CD) ; Nop
  ; OPT   #stack_push_pointer string_1879050369 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer titleScreen ; Nop
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_CopyMemory ; Call
  ; OPT   #stack_push_pointer string_1879050393 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$1CD) ; Nop
  ; OPT   #stack_push_pointer string_1879050433 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer titleScreen+$100 ; Nop
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_CopyMemory ; Call
  ; OPT   #stack_push_pointer string_1879050467 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$21B) ; Nop
  ; OPT   #stack_push_pointer string_1879050507 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer titleScreen+$14E ; Nop
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_CopyMemory ; Call
    #stack_push_int8 13 ; Ldc_i4_s
    #stack_push_int8 1 ; Ldc_i4_1
    #stack_push_pointer string_1879050541 ; Ldstr
    #stack_push_int8 5 ; Ldc_i4_5
    jsr C64_Write ; Call
    #stack_push_int8 13 ; Ldc_i4_s
    #stack_push_int8 3 ; Ldc_i4_3
    #stack_push_pointer string_1879050571 ; Ldstr
    #stack_push_int8 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    #stack_push_int8 25 ; Ldc_i4_s
    #stack_push_int8 21 ; Ldc_i4_s
    #stack_push_pointer string_1879050601 ; Ldstr
    #stack_push_int8 5 ; Ldc_i4_5
    jsr C64_Write ; Call
    #stack_push_int8 7 ; Ldc_i4_7
    #stack_push_int8 23 ; Ldc_i4_s
    #stack_push_pointer string_1879050631 ; Ldstr
    #stack_push_int8 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    #stack_push_int8 7 ; Ldc_i4_7
    #stack_push_int8 24 ; Ldc_i4_s
    #stack_push_pointer string_1879050683 ; Ldstr
    #stack_push_int8 1 ; Ldc_i4_1
    jsr C64_Write ; Call
    jsr TitleScreen_GetTones ; Call
    #locals_pull_value8 1, 1 ; Stloc_0
TitleScreen_Display_161:  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_1
    #init_var 2, 1 ; Nop
    #locals_push_value8 1 ; Ldloc_0
    #locals_pull_value8 3, 1 ; Stloc_2
  ; OPT   #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #locals_pull_value8 4, 0 ; Stloc_3
    #init_var 4, 0 ; Nop
    jmp TitleScreen_Display_316 ; Br
TitleScreen_Display_172:    #locals_push_value8 3 ; Ldloc_2
    #locals_push_value8 4 ; Ldloc_3
    #ldelemRef ; Ldelem_ref
    #locals_pull_value8 5, 1 ; Stloc_s
  ; OPT   #locals_push_value8 2 ; Ldloc_1
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_1
    #inc_var 2 ; Nop
    #locals_push_value8 2 ; Ldloc_1
  ; OPT   #stack_push_int8 7 ; Ldc_i4_7
  ; OPT   #branch_not_equal8 TitleScreen_Display_187 ; Bne_un_s
    #branch_not_equal_const 7, TitleScreen_Display_187 ; Nop
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #locals_pull_value8 2, 0 ; Stloc_1
    #init_var 2, 1 ; Nop
TitleScreen_Display_187:    jsr C64_get_Sound ; Call
    #stack_push_int8 32 ; Ldc_i4_s
    #locals_push_value8 5 ; Ldloc_s
    #ldfld16 0 ; Ldfld
    #stack_push_int16 0 ; Ldc_i4_0
    ; conv ; Conv_i8
    #stack_push_int8 9 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr Sound_PlayEffectReg1 ; Callvirt
  ; OPT   #stack_push_pointer string_1879050735 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$CD) ; Nop
    #locals_push_value8 2 ; Ldloc_1
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050771 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$1CD) ; Nop
    #locals_push_value8 2 ; Ldloc_1
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050809 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$21B) ; Nop
    #locals_push_value8 2 ; Ldloc_1
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    #stack_push_int8 0 ; Ldc_i4_0
    #locals_pull_value8 6, 0 ; Stloc_s
    jmp TitleScreen_Display_306 ; Br_s
TitleScreen_Display_268:    #stack_push_int8 0 ; Ldc_i4_0
    #locals_pull_value8 7, 0 ; Stloc_s
    jmp TitleScreen_Display_289 ; Br_s
TitleScreen_Display_273:    #stack_push_int8 26 ; Ldc_i4_s
    jsr C64_IsKeyPressed ; Call
    #branch_false TitleScreen_Display_283 ; Brfalse_s
    #method_exit 9, [1,3,5] ; Ret
TitleScreen_Display_283:    #locals_push_value8 7 ; Ldloc_s
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #locals_pull_value8 7, 0 ; Stloc_s
TitleScreen_Display_289:    #locals_push_value8 7 ; Ldloc_s
    #locals_push_value8 5 ; Ldloc_s
    #ldfld8 2 ; Ldfld
    #branch_less_unsigned8 TitleScreen_Display_273 ; Blt_un_s
    #locals_push_value8 6 ; Ldloc_s
    #stack_push_int8 1 ; Ldc_i4_1
    #add8 ; Add
    #locals_pull_value8 6, 0 ; Stloc_s
TitleScreen_Display_306:    #locals_push_value8 6 ; Ldloc_s
  ; OPT   #stack_push_int8 80 ; Ldc_i4_s
  ; OPT   #branch_less_unsigned8 TitleScreen_Display_268 ; Blt_un_s
    #branch_less_unsigned_const 80, TitleScreen_Display_268 ; Nop
  ; OPT   #locals_push_value8 4 ; Ldloc_3
  ; OPT   #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #add8 ; Add
  ; OPT   #locals_pull_value8 4, 0 ; Stloc_3
    #inc_var 4 ; Nop
TitleScreen_Display_316:    #locals_push_value8 4 ; Ldloc_3
    #locals_push_value8 3 ; Ldloc_2
    #ldlen ; Ldlen
    ; conv ; Conv_i4
    #branch_less8 TitleScreen_Display_172 ; Blt
    jmp TitleScreen_Display_161 ; Br


;----------------------------------------
; TYPE: Hunchback.TitleScreen
; METHOD: GetTones
;----------------------------------------
TitleScreen_GetTones 
    #init_locals_pull_parameters 0, []
    #stack_push_int8 64 ; Ldc_i4_s
    #newArrRef ; Newarr
    #stack_duplicate8 ; Dup
    #stack_push_int8 0 ; Ldc_i4_0
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 3288 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_11, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 1 ; Ldc_i4_1
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4389 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_12, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 2 ; Ldc_i4_2
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 3 ; Ldc_i4_3
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4927 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_14, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 4 ; Ldc_i4_4
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 5 ; Ldc_i4_5
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4927 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_14, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 6 ; Ldc_i4_6
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4389 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_12, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 7 ; Ldc_i4_7
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 8 ; Ldc_i4_8
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4927 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_15, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 9 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4389 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_16, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 11 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4927 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_14, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 12 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 13 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4927 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_15, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 14 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 15 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4389 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 30 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_17, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 16 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 0 ; Ldc_i4_0
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_18, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 17 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 3910 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_19, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 18 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_20, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 19 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 6577 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_21, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_22, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 21 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 6577 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_21, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 22 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_22, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 23 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_20, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 24 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 6577 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_21, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 25 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_23, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 26 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 6577 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_21, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 27 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 28 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_22, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 29 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 6577 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_21, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 30 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_23, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 31 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 6577 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_21, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 32 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 30 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_24, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 33 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 0 ; Ldc_i4_0
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_18, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 34 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 7821 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_25, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 35 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 8779 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_26, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 36 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 7821 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_25, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 37 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 8779 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_26, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 38 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 7821 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_25, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 39 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_22, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 40 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 6577 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_21, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 41 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 7821 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_25, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 42 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 6577 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_27, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 43 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_22, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 44 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_23, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 45 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 46 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_23, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 47 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 48 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 3910 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_19, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 49 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4389 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_16, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 50 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 51 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4389 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_12, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 52 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 3910 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_19, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 53 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4389 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_12, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 54 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 55 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 3910 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_28, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 56 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_13, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 57 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 3484 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_29, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 58 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 3910 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_19, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 59 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 4389 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_16, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 60 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 3910 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 20 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_28, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 61 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 6577 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 10 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_21, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 62 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5859 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 40 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_30, 0 ; Nop
    #stelemRef ; Stelem_ref
    #stack_duplicate8 ; Dup
    #stack_push_int8 63 ; Ldc_i4_s
  ; OPT   #newObj 3, 0, Tone_VTable, 0 ; Newobj
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int16 5220 ; Ldc_i4
  ; OPT   ; conv ; Conv_i8
  ; OPT   #stfld16 0 ; Stfld
  ; OPT   #stack_duplicate8 ; Dup
  ; OPT   #stack_push_int8 50 ; Ldc_i4_s
  ; OPT   #stfld8 2 ; Stfld
    #newObjInit 3, 0,  Tone_VTable, Init_Values_31, 0 ; Nop
    #stelemRef ; Stelem_ref
    #method_exit 2, [] ; Ret
