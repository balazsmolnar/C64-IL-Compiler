

;----------------------------------------
; TYPE: Demo.Program
; METHOD: Foo
;----------------------------------------
Program_Foo 
    #init_locals_pull_parameters 0, [0,0,0]
    nop ; Nop
    #method_exit 5, [] ; Ret


;----------------------------------------
; TYPE: Demo.Program
; METHOD: Main
;----------------------------------------
Program_Main 
    #init_locals_pull_parameters 0, []
    nop ; Nop
    jsr Ball_RunBalls ; Call
    nop ; Nop
    #method_exit 2, [] ; Ret
