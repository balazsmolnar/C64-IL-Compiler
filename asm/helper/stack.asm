; Use C64 native stack
; less space, but faster
;
stack_push_pointer .macro ptr
  lda # < \ptr
  pha
  lda # > \ptr
  pha
.endm

stack_pull_pointer .macro address 
  pla
  sta \address+1
  pla
  sta \address
.endm

stack_push_int16 .macro value 
  lda # < \value
  pha
  lda # > \value
  pha
.endm

stack_push_int8 .macro value 
  lda # < \value
  pha
.endm

stack_pull_int16 .macro address 
  pla
  sta \address+1
  pla
  sta \address
.endm

stack_pull_int8 .macro address 
  pla
  sta \address
.endm

stack_push_var16 .macro address 
  lda \address
  pha
  lda \address+1
  pha
.endm

stack_push_var8 .macro address 
  lda \address
  pha
.endm

stack_duplicate .macro
  pla
  pha
  pha
.endm

stack_pop .macro
  pla
.endm

stack_save_return_adress .macro variable
  pla
  sta \variable+1
  pla
  sta \variable
.endm

stack_return_to_saved_address .macro  variable 
  lda \variable
  pha
  lda \variable+1
  pha
  rts
.endm

stack_pull_int_a .macro 
  pla
.endm

stack_pull_int_x .macro
  pla
  tax
.endm

stack_pull_int_y .macro 
  pla
  tay
.endm

stack_push_int_a .macro
  pha
.endm

stack_push_int_x .macro
  txa
  pha
.endm

stack_pull_int_ref .macro address, ref 

  ; deref
  .if \ref == 1 
    #deref \address
  .endif
  ; store
  pla
  sta \address

  ; ref
  .if \ref == 1 
    tax
    inc objTableRootCount, x
  .endif
.endm

deref .macro address 
    ldx \address
    dec objTableRootCount, x
    lda #0
    sta \address
.endm

