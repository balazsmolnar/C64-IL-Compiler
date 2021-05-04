stack_push_int .macro value 
  #stack_push_int8 \value
.endm

stack_push_var .macro address 
  #stack_push_var8 \address
.endm

stack_pull_int .macro address 
  #stack_pull_int8 \address
.endm

; negate .macro
;     #negate8
; .endm

; compareLess .macro
;     #compareLess8
; .endm

; compareGreater .macro
;     #compareGreater8
; .endm

; compareEqual .macro
;     #compareEqual8
; .endm

; add .macro
;     #add8
; .endm

; sub .macro
;     #sub8
; .endm
