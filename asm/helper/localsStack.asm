; Stack for parameters and local variables

locals_stack_init .macro
  lda # 0
  sta stackPointer
.endm 

locals_pull_param_8 .macro ref

  pla
  ldx stackPointer
  sta localsStack,x
  inx
  stx stackPointer
  .if \ref == 1 
    tax
    inc objTableRootCount, x
  .endif
.endm

init_locals_pull_parameters .macro localsSize, ref_list

  #init_locals \localsSize
  .if len(\ref_list) > 0
    .for ref in \ref_list
      pla
      sta localsStack,y
      iny
      .if ref == 1 
        tax
        inc objTableRootCount, x
      .endif
    .next
  .endif
  sty stackPointer

.endm


init_locals .macro localsSize
  ldy stackPointer

  ; save return address from stack
  pla
  sta localsStack,y
  iny
  pla
  sta localsStack,y
  iny

  ; init locals
  lda #0
  .if \localsSize == 0
  .elsif \localsSize == 1
    sta localsStack,y
    iny
  .elsif \localsSize == 2
    sta localsStack,y
    iny
    sta localsStack,y
    iny
  .elsif \localsSize == 3
    sta localsStack,y
    iny
    sta localsStack,y
    iny
    sta localsStack,y
    iny
  .else
    tax
-   sta localsStack,y
    inx
    iny
    cpx #\localsSize
    bne -
  .endif
.endm

method_exit .macro stackSize, ref_list

  .for ref in \ref_list
    ldy stackPointer
    ldx localsStack-ref,y
    dec objTableRootCount, x
  .next

  lda stackPointer
  sec
  sbc #\stackSize
  sta stackPointer
  tax
  inx
  lda localsStack,x 
  pha
  dex
  lda localsStack,x
  pha
  rts
.endm


locals_pull_value_8 .macro rel_pos, ref 

  ldy stackPointer

  ; deref
  .if \ref == 1 
    ldx localsStack-\rel_pos,y
    dec objTableRootCount, x
  .endif
  #stack_pull_int_a
  sta localsStack-\rel_pos,y
  ; ref
  .if \ref == 1 
    tax
    inc objTableRootCount, x
  .endif
.endm

locals_push_value_8 .macro rel_pos
  ldy stackPointer
  lda localsStack-\rel_pos,y
  #stack_push_int_a
.endm

locals_push_value_16 .macro rel_pos
  ldy stackPointer
  lda localsStack-\rel_pos+1,y
  #stack_push_int_a
  lda localsStack-\rel_pos,y
  #stack_push_int_a
.endm
