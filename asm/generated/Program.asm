

;----------------------------------------
; TYPE: Demo.Program
; METHOD: Foo
;----------------------------------------
Program_Foo 
    #init_locals_pull_parameters 0, [0, 0]
    nop ; Nop
    #locals_push_value_16 2 ; Ldarg_0
    jsr Console_WriteLine ; Call
    nop ; Nop
    #method_exit 4, [] ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 1, []
    nop ; Nop
    #newObj 3, 3 ; Newobj
    #locals_pull_value_8 1, 1 ; Stloc_0
    #locals_push_value_8 1 ; Ldloc_0
    jsr Game_Init ; Callvirt
    nop ; Nop
    #locals_push_value_8 1 ; Ldloc_0
    jsr Game_Run ; Callvirt
    nop ; Nop
    #method_exit 3, [1] ; Ret
