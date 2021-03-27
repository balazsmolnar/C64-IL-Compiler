

;----------------------------------------
; TYPE: Hunchback.Screen
; METHOD: Clear
;----------------------------------------
Screen_Clear 
    #init_locals_pull_parameters 0, [0]
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049221 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory) ; Nop
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049251 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$100) ; Nop
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049291 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$200) ; Nop
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049331 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$300) ; Nop
    #stack_push_int 32 ; Ldc_i4_s
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049371 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory) ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049399 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$100) ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049437 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$200) ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
  ; OPT   #stack_push_pointer string_1879049475 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$300) ; Nop
    #locals_push_value_8 1 ; Ldarg_0
    #stack_push_int 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    nop ; Nop
    #method_exit 3, [] ; Ret
