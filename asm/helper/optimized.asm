inc_var .macro rel_pos
  ldx stackPointer
  inc localsStack-\rel_pos,x
.endm

init_var .macro  rel_pos, value  
  ldy stackPointer
  lda #\value
  sta localsStack-\rel_pos,y
.endm

setfld8 .macro objRelPos, objValuePos, pos 

  ldy stackPointer
  ldx localsStack-\objRelPos,y
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy stackPointer
  ldx localsStack-\objValuePos,y
  txa
  ldy #\pos
  sta (tmpPointer),y
.endm

setfld16 .macro objRelPos, objValuePos, pos 

  ldy stackPointer
  ldx localsStack-\objRelPos,y
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy stackPointer
  ldx localsStack-\objValuePos,y
  txa
  ldy #\pos
  sta (tmpPointer),y

  ldy stackPointer
  ldx localsStack-\objValuePos+1,y
  txa
  ldy #\pos+1
  sta (tmpPointer),y

.endm

pushfld8 .macro pos 

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

pushfld16 .macro pos 

  ldy stackPointer
  dey
  ldx localsStack,y
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #\pos+1
  lda (tmpPointer),y
  pha
  dey
  lda (tmpPointer),y
  pha

.endm

incfld .macro pos 

  ldy stackPointer
  ldx localsStack-1,y

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
    ldy stackPointer
    lda localsStack-\rel_pos, y
    cmp #\value
    bmi \label
.endm

branch_if_not_equal .macro rel_pos, value, label 
    ldy stackPointer
    lda localsStack-\rel_pos, y
    cmp #\value
    bne \label
.endm
