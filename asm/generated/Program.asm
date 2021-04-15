

;----------------------------------------
; TYPE: Demo.Program
; METHOD: Foo
;----------------------------------------
Program_Foo 
    #init_locals_pull_parameters 0, [0,0,0]
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 1, []
    #newObj 0, 0 ; Newobj
    #locals_pull_value_8 1, 1 ; Stloc_0
    #locals_push_value_8 1 ; Ldloc_0
    jsr Game_Start ; Callvirt
    #method_exit 3, [1] ; Ret
