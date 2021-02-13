; Stack for parameters and local variables

stack_get_from_pos_x .macro  rel_pos
  .if \rel_pos==1
    ldx stackPointer
    dex
  .elsif \rel_pos==2
    ldx stackPointer
    dex
    dex
  .elsif \rel_pos==3
    ldx stackPointer
    dex
    dex
    dex
  .elsif \rel_pos==4
    ldx stackPointer
    dex
    dex
    dex
    dex
  .else  
    lda stackPointer
    sec
    sbc #\rel_pos
    tax
  .endif
.endm

stack_get_from_pos_y .macro  rel_pos
  .if \rel_pos==1
    ldy stackPointer
    dey
  .elsif \rel_pos==2
    ldy stackPointer
    dey
    dey
  .elsif \rel_pos==3
    ldy stackPointer
    dey
    dey
    dey
  .elsif \rel_pos==4
    ldy stackPointer
    dey
    dey
    dey
    dey
  .else  
    lda stackPointer
    sec
    sbc #\rel_pos
    tay
  .endif
.endm

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
    ldx #0
-   sta localsStack,y
    inx
    iny
    cpx #\localsSize
    bne -
  .endif
.endm

method_exit .macro stackSize
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

locals_set_value .macro rel_pos, value, ref 

   #stack_get_from_pos_y \rel_pos
  ; deref
  .if \ref == 1 
    ldx localsStack,y
    dec objTableRootCount,x
  .endif
  lda #\value
  sta localsStack,y
  ; ref
  .if \ref == 1 
    tax
    inc objTableRootCount, x
  .endif
.endm

locals_pull_value_8 .macro rel_pos, ref 

  #stack_get_from_pos_y \rel_pos
  ; deref
  .if \ref == 1 
    lda localsStack,y
    tax
    dec objTableRootCount, x
  .endif
  #stack_pull_int_a
  sta localsStack,y
  ; ref
  .if \ref == 1 
    tax
    inc objTableRootCount, x
  .endif
.endm

locals_push_value_8 .macro rel_pos
  #stack_get_from_pos_y \rel_pos
  lda localsStack,y
  #stack_push_int_a
.endm

