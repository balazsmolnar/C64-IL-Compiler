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

locals_init_locals .macro localsSize
  ldx stackPointer

  ; save return address from stack
  pla
  sta localsStack,x
  inx
  pla
  sta localsStack,x
  inx

  ; init locals
  lda #0
  .if \localsSize == 0
  .elsif \localsSize == 1
    sta localsStack,x
    inx
  .elsif \localsSize == 2
    sta localsStack,x
    inx 
    sta localsStack,x
    inx 
  .elsif \localsSize == 3
    sta localsStack,x
    inx 
    sta localsStack,x
    inx 
    sta localsStack,x
    inx 
  .else
    ldy #0
-   cpy #\localsSize
    beq +
    sta localsStack,x
    inx
    iny
    bne -
  .endif
+ stx stackPointer
.endm

locals_method_exit .macro stackSize
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

inc_var .macro rel_pos
  #stack_get_from_pos_x \rel_pos
  inc localsStack,x
.endm

init_var .macro  rel_pos, value  
  #stack_get_from_pos_x \rel_pos
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

incfld .macro pos 

  ldy stackPointer
  dey
  ldx localsStack,y

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #\pos
  lda (tmpPointer),y
  clc
  adc #1
  sta (tmpPointer),y

.endm

branch_if_var_less .macro rel_pos, value, label 
    #stack_get_from_pos_x \rel_pos
    lda localsStack, x
    cmp #\value
    bmi \label
.endm

branch_if_not_equal .macro rel_pos, value, label 
    #stack_get_from_pos_x \rel_pos
    lda localsStack, x
    cmp #\value
    bne \label
.endm