

;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 1, []
    #newObj 0, 0 ; Newobj
    #locals_pull_value8 1, 1 ; Stloc_0
    #locals_push_value8 1 ; Ldloc_0
    jsr Game_Start ; Callvirt
    #method_exit 3, [1] ; Ret
