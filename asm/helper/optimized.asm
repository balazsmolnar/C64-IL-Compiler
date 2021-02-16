inc_var .macro rel_pos
  #stack_get_from_pos_x \rel_pos
  inc localsStack,x
.endm

init_var .macro  rel_pos, value  
  #stack_get_from_pos_x \rel_pos
  lda #\value
  sta localsStack,x
.endm

setfld .macro objRelPos, objValuePos, pos 

  #stack_get_from_pos_y \objRelPos
  ldx localsStack,y
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  #stack_get_from_pos_y \objValuePos
  ldx localsStack,y
  txa
  ldy #\pos
  sta (tmpPointer),y
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
