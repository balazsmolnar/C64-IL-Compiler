inc_var .macro rel_pos
  ldx stackPointer
  inc localsStack-\rel_pos,x
.endm

; Single-byte only, like inc_var -- no carry into a second byte. Callers
; must only use this for a confirmed 8-bit local (see ILMethodDecOptimizer's
; WithGuard checks).
dec_var .macro rel_pos
  ldx stackPointer
  dec localsStack-\rel_pos,x
.endm

init_var .macro  rel_pos, value
  ldy stackPointer
  lda #\value
  sta localsStack-\rel_pos,y
.endm

; Direct local/param-to-local copy, skipping the push/pop round trip
; through the hardware stack the unfused path (locals_push_value8 +
; locals_pull_value8) would use. 8-bit only, and only ever emitted for a
; non-reference-counted destination -- see ILMethodSetVariableOptimizer's
; guard, which keeps the ref-counting-aware locals_pull_value8 path for
; anything reference-typed instead of using this.
copy_var .macro src_rel_pos, dst_rel_pos
  ldy stackPointer
  lda localsStack-\src_rel_pos,y
  sta localsStack-\dst_rel_pos,y
.endm

setfld8 .macro objRelPos, objValuePos, pos

  ldy stackPointer
  ldx localsStack-\objRelPos,y
  jsr resolveObjPtr

  ; resolveObjPtr only touches A, never Y, so it's still holding
  ; stackPointer's value from the ldy above -- no need to reload it.
  ldx localsStack-\objValuePos,y
  txa
  ldy #\pos
  sta (tmpPointer),y
.endm

setfld16 .macro objRelPos, objValuePos, pos

  ldy stackPointer
  ldx localsStack-\objRelPos,y
  jsr resolveObjPtr

  ; see setfld8 -- resolveObjPtr doesn't touch Y.
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
  jsr resolveObjPtr

  ldy #\pos
  lda (tmpPointer),y
  pha

.endm

pushfld16 .macro pos 

  ldy stackPointer
  dey
  ldx localsStack,y
  jsr resolveObjPtr

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

  jsr resolveObjPtr

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
