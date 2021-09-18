

;----------------------------------------
; TYPE: Hunchback.Screen
; METHOD: Clear
;----------------------------------------
Screen_Clear 
    #init_locals_pull_parameters 0, [0]
  ; OPT   #stack_push_pointer string_1879050065 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory) ; Nop
    #stack_push_int8 32 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050095 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$100) ; Nop
    #stack_push_int8 32 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050135 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$200) ; Nop
    #stack_push_int8 32 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050175 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (screenMemory+$300) ; Nop
    #stack_push_int8 32 ; Ldc_i4_s
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050215 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory) ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050243 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$100) ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050281 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$200) ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
  ; OPT   #stack_push_pointer string_1879050319 ; Ldstr
  ; OPT   jsr C64Address_FromLabel ; Call
    #stack_push_pointer (colorMemory+$300) ; Nop
    #locals_push_value8 1 ; Ldarg_0
    #stack_push_int8 0 ; Ldc_i4_0
    jsr C64_FillMemory ; Call
    #method_exit 3, [] ; Ret
