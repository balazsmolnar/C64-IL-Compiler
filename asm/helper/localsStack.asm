; Stack for parameters and local variables

stackPointer = $24

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

locals_init_locals .macro localsSize
  ldx stackPointer
  ldy #0

  ; save return address from stack
  pla
  sta localsStack,x
  inx
  pla
  sta localsStack,x
  inx

  ; init locals
  lda #0
  ldy #0
- cpy #\localsSize
  beq +
  sta localsStack,x
  inx
  iny
  bne -
+ stx stackPointer
.endm

locals_method_exit .macro stackSize
  lda stackPointer
  sec
  sbc #\stackSize
  sta stackPointer
  ldx stackPointer
  inx
  lda localsStack,x 
  pha
  dex
  lda localsStack,x
  pha
  rts
.endm

locals_set_value .macro rel_pos, value, ref 

  lda stackPointer
  sec
  sbc #\rel_pos
  tay
  ; deref
  .if \ref == 1 
    lda localsStack,y
    tax
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

  lda stackPointer
  sec
  sbc #\rel_pos
  tay
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
  lda stackPointer
  sec
  sbc #\rel_pos
  tay
  lda localsStack,y
  #stack_push_int_a
.endm

inc_var .macro rel_pos
  lda stackPointer
  sec
  sbc #\rel_pos
  tax
  inc localsStack,x
.endm

init_var .macro  rel_pos, value  
  lda stackPointer
  sec
  sbc #\rel_pos
  tax
  lda #\value
  sta localsStack,x
.endm

pushfld .macro pos 

  ldy stackPointer
  dey
  ldx localsStack,y
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #\pos
  lda (tmpPointer),y
  pha

.endm
