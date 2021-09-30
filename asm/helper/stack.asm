; Use C64 native stack
; less space, but faster
;
stack_push_pointer .macro ptr
  lda # > \ptr
  pha
  lda # < \ptr
  pha
.endm

stack_pull_pointer .macro address 
  pla
  sta \address
  pla
  sta \address+1
.endm

stack_push_int16 .macro value 
  lda # > \value
  pha
  lda # < \value
  pha
.endm

stack_push_int8 .macro value 
  lda # < \value
  pha
.endm

stack_pull_int16 .macro address 
  pla
  sta \address
  pla
  sta \address+1
.endm

stack_pull_int8 .macro address 
  pla
  sta \address
.endm

stack_push_var16 .macro address 
  lda \address+1
  pha
  lda \address
  pha
.endm

stack_push_var8 .macro address 
  lda \address
  pha
.endm

stack_duplicate8 .macro
  #stack_pull_int_a
  #stack_push_int_a
  #stack_push_int_a
.endm

stack_duplicate16 .macro
  #stack_pull_int_x
  #stack_pull_int_y
  #stack_push_int_y
  #stack_push_int_x
  #stack_push_int_y
  #stack_push_int_x
.endm

stack_pop .macro
  pla
.endm

stack_save_return_adress .macro variable
  pla
  sta \variable
  pla
  sta \variable+1
.endm

stack_return_to_saved_address .macro  variable 
  lda \variable+1
  pha
  lda \variable
  pha
  rts
.endm

stack_pull_int_a .macro 
  pla
.endm

stack_pull_int_x .macro
  #stack_pull_int_a
  tax
.endm

stack_pull_int_y .macro 
  #stack_pull_int_a
  tay
.endm

stack_push_int_a .macro
  pha
.endm

stack_push_int_x .macro
  txa
  #stack_push_int_a
.endm

stack_push_int_y .macro
  tya
  #stack_push_int_a
.endm

stack_pull_int_ref .macro address, ref 

  ; deref
  .if \ref == 1 
    #deref \address
  .endif
  ; store
  #stack_pull_int_a
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

