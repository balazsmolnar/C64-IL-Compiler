
locals_stack_init .macro
  lda # 0
  sta evaluationStackPointer
.endm 

stack_push_pointer .macro ptr
  ldy evaluationStackPointer
  lda # < \ptr
  sta (evaluationStack),y
  iny
  lda # > \ptr
  sta (evaluationStack),y
  iny
.endm

stack_pull_pointer .macro address 
  ldy evaluationStackPointer
  lda (evaluationStackPointer),y
  sta \address+1
  dey
  lda (evaluationStackPointer),y
  dey
  sta \address
.endm

stack_push_int16 .macro value 
  ldy evaluationStackPointer
  lda # < \value
  sta (evaluationStack),y
  iny
  lda # > \value
  sta (evaluationStack),y
  iny
.endm

stack_push_int8 .macro value 
  lda # < \value
  ldy evaluationStackPointer
  sta (evaluationStack),y
  iny
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

stack_duplicate8 .macro
  #stack_pull_int_a
  #stack_push_int_a
  #stack_push_int_a
.endm

stack_duplicate16 .macro
  #stack_pull_int_x
  #stack_pull_int_a
  #stack_push_int_a
  #stack_push_int_x
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

