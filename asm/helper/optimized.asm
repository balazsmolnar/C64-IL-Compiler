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

; PROTOTYPE (see zp_local0's comment in zeropage.asm): zp-addressed twins of
; inc_var/dec_var/init_var above, for a promoted local -- same fusion, same
; guards (single-byte only), just a flat zero-page address instead of
; `localsStack-rel_pos,x/y`, so no stackPointer load at all.
inc_var_zp .macro addr
  inc \addr
.endm

dec_var_zp .macro addr
  dec \addr
.endm

init_var_zp .macro addr, value
  lda #\value
  sta \addr
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

; Float field of `this`, pushed straight from the object onto the evaluation
; stack (exponent first, same byte order as stack_push_var_mflpt) -- no detour
; through zp_flt_a like the unfused Ldarg_0 + ldfldflt.
pushfldflt .macro pos

  ldy stackPointer
  dey
  ldx localsStack,y
  jsr resolveObjPtr

  ldy #\pos
  lda (tmpPointer),y
  pha
  iny
  lda (tmpPointer),y
  pha
  iny
  lda (tmpPointer),y
  pha
  iny
  lda (tmpPointer),y
  pha
  iny
  lda (tmpPointer),y
  pha

.endm

; Same three macros as pushfld8/pushfld16/pushfldflt above, but for
; ILMethodInliningPass's trivial-argument substitution (see
; Operands/OperandInline.cs's OpPushFldAt): the object being read isn't
; always `this` (always at a fixed rel_pos of 1), it's whichever caller
; slot the substituted argument came from, at an arbitrary compile-time
; constant \relpos. `ldx localsStack-\relpos,y` generalizes the unfused
; macros' own `dey; ldx localsStack,y` (exactly equivalent when
; \relpos == 1 -- localsStack[y-1] == (localsStack-1)[y] -- but, unlike
; `dey`, also correct for any other compile-time constant without needing
; N separate dey's): both bake a constant into the LDX instruction's own
; address operand, which 6502 can index by Y directly, so there's no
; runtime cost to generalizing this beyond \relpos == 1.
pushfld8_at .macro relpos, pos

  ldy stackPointer
  ldx localsStack-\relpos,y
  jsr resolveObjPtr

  ldy #\pos
  lda (tmpPointer),y
  pha

.endm

pushfld16_at .macro relpos, pos

  ldy stackPointer
  ldx localsStack-\relpos,y
  jsr resolveObjPtr

  ldy #\pos+1
  lda (tmpPointer),y
  pha
  dey
  lda (tmpPointer),y
  pha

.endm

pushfldflt_at .macro relpos, pos

  ldy stackPointer
  ldx localsStack-\relpos,y
  jsr resolveObjPtr

  ldy #\pos
  lda (tmpPointer),y
  pha
  iny
  lda (tmpPointer),y
  pha
  iny
  lda (tmpPointer),y
  pha
  iny
  lda (tmpPointer),y
  pha
  iny
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
