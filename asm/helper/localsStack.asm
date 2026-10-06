; Stack for parameters and local variables

locals_stack_init .macro
  lda # 0
  sta stackPointer
.endm 

locals_pull_param_8 .macro ref

  #stack_pull_int_a
  ldx stackPointer
  sta localsStack,x
  inx
  stx stackPointer
  .if \ref == 1 
    tax
    inc objTableRootCount, x
  .endif
.endm

; local_ref_list: rel_pos of every reference-typed LOCAL (never a
; parameter) -- zeroed here, after the parameter-pulling loop (so Y is
; already at its final stackPointer value, matching the addressing
; every other rel_pos-indexed macro assumes), so a local's own first
; real write never "dec old"s whatever stale, possibly-now-reassigned
; handle happened to still be sitting in that shared localsStack
; position from an unrelated earlier call -- see
; CompilerMethodContext.GetLocalRefPositions's own comment for the
; real, reproduced bug this fixes.
init_locals_pull_parameters .macro localsSize, ref_list, local_ref_list

  #init_locals \localsSize
  .if len(\ref_list) > 0
    .for ref in \ref_list
      #stack_pull_int_a
      sta localsStack,y
      iny
      .if ref == 1
        tax
        inc objTableRootCount, x
      .endif
    .next
  .endif
  .if len(\local_ref_list) > 0
    lda #0
    .for pos in \local_ref_list
      sta localsStack-pos,y
    .next
  .endif
  sty stackPointer

.endm


init_locals .macro localsSize
  ldy stackPointer

  ; save return address from stack
  pla
  sta localsStack,y
  pla
  sta localsStack+1,y

  ; reserve memory for locals
  .if \localsSize == 0
    iny
    iny
  .elsif \localsSize == 1
    iny
    iny
    iny
  .elsif \localsSize == 2
    iny
    iny
    iny
    iny

  .else
    tya
    clc
    adc #(\localsSize+2)
    tay
  .endif
.endm

method_exit .macro stackSize, ref_list

  .if len(\ref_list) > 0
    ldy stackPointer
    .for ref in \ref_list
      ldx localsStack-ref,y
      jsr DecRefCountIfAllocated
    .next
    ; Y still holds stackPointer's value (the loop above never touches
    ; Y), so grab it from there instead of a redundant zero-page reload.
    tya
  .else
    lda stackPointer
  .endif

  sec
  sbc #\stackSize
  sta stackPointer
  tax
  lda localsStack+1,x 
  pha
  lda localsStack,x
  pha
  rts
.endm


locals_pull_value8 .macro rel_pos, ref 

  ldy stackPointer

  ; deref
  .if \ref == 1
    ldx localsStack-\rel_pos,y
    jsr DecRefCountIfAllocated
  .endif
  #stack_pull_int_a
  sta localsStack-\rel_pos,y
  ; ref
  .if \ref == 1 
    tax
    inc objTableRootCount, x
  .endif
.endm

locals_push_value8 .macro rel_pos
  ldy stackPointer
  lda localsStack-\rel_pos,y
  #stack_push_int_a
.endm

; PROTOTYPE (see zp_local0's own comment in zeropage.asm): the same shape as
; locals_push_value8/locals_pull_value8 above, but \addr is a fixed
; zero-page byte instead of an index into localsStack -- no Y, no
; stackPointer, no GC ref-tracking (a promoted local is never
; reference-counted by construction). Only ever used for a LOCAL variable's
; own ldloc/stloc -- never a parameter, never `this`, never the
; return-address handling method entry/exit do -- see the pool's own
; comment for why that scoping matters.
zp_push_value8 .macro addr
  lda \addr
  #stack_push_int_a
.endm

zp_pull_value8 .macro addr
  #stack_pull_int_a
  sta \addr
.endm

; PROTOTYPE (ILMethodInliningPass, Compiler/ILMethodInliningPass.cs): inline
; counterparts of init_locals/init_locals_pull_parameters/method_exit above,
; used at a call site whose target gets SPLICED IN instead of jsr'd into
; (exactly one call site in the whole program, callee is a leaf -- see that
; pass's own comment for the full eligibility reasoning). The callee's own
; params/locals still get a real, freshly-reserved slice of localsStack --
; same addressing (localsStack-rel_pos,y), same GC ref-list decrement/
; increment -- the ONLY difference from a real call is that there is no
; jsr, so there is no hardware-stack return address to save or restore
; here at all.
init_locals_inline .macro localsSize
  ldy stackPointer
  .if \localsSize == 0
    ; nothing to reserve
  .elsif \localsSize == 1
    iny
  .elsif \localsSize == 2
    iny
    iny
  .else
    tya
    clc
    adc #\localsSize
    tay
  .endif
.endm

; Inline counterpart of init_locals_pull_parameters: reserves the callee's
; own locals space via init_locals_inline (no return-address handling, no
; +2), then pulls its parameters/this off the evaluation stack exactly the
; same way init_locals_pull_parameters does -- see that macro's own comment
; for the ref_list shape and pull order, and for local_ref_list's own
; zeroing (same reason, same reproduced bug -- a materializing splice's
; callee can have its own local variables too, reserving a fresh slice
; of the same shared, never-re-zeroed localsStack region as any other
; call).
init_locals_pull_parameters_inline .macro localsSize, ref_list, local_ref_list
  #init_locals_inline \localsSize
  .if len(\ref_list) > 0
    .for ref in \ref_list
      #stack_pull_int_a
      sta localsStack,y
      iny
      .if ref == 1
        tax
        inc objTableRootCount, x
      .endif
    .next
  .endif
  .if len(\local_ref_list) > 0
    lda #0
    .for pos in \local_ref_list
      sta localsStack-pos,y
    .next
  .endif
  sty stackPointer
.endm

; Inline counterpart of method_exit: decrements GC refcounts for every
; reference-typed local/param and rewinds stackPointer exactly the same
; way, but ends with a jmp to \continueLabel instead of popping a
; (nonexistent) return address and rts -- there was no jsr into this
; splice, so there is nothing on the hardware stack to pop here.
method_exit_inline .macro stackSize, ref_list, continueLabel
  .if len(\ref_list) > 0
    ldy stackPointer
    .for ref in \ref_list
      ldx localsStack-ref,y
      jsr DecRefCountIfAllocated
    .next
    tya
  .else
    lda stackPointer
  .endif

  sec
  sbc #\stackSize
  sta stackPointer
  jmp \continueLabel
.endm

; Same as method_exit_inline, minus the final jmp -- used only when this is
; the callee's single Ret AND it's the last operation in its body, so
; control already falls straight through into the continuation marker
; right after this (ILMethodInliningPass's own fast-path: the common case
; for a small, single-exit helper, saving one jmp's bytes/cycles).
method_exit_inline_fallthrough .macro stackSize, ref_list
  .if len(\ref_list) > 0
    ldy stackPointer
    .for ref in \ref_list
      ldx localsStack-ref,y
      jsr DecRefCountIfAllocated
    .next
    tya
  .else
    lda stackPointer
  .endif

  sec
  sbc #\stackSize
  sta stackPointer
.endm

locals_push_value16 .macro rel_pos
  ldy stackPointer
  lda localsStack-\rel_pos+1,y
  #stack_push_int_a
  lda localsStack-\rel_pos,y
  #stack_push_int_a
.endm

locals_pull_value16 .macro rel_pos
  ldy stackPointer
  #stack_pull_int_a
  sta localsStack-\rel_pos,y
  #stack_pull_int_a
  sta localsStack-\rel_pos+1,y
.endm

; float locals -- rel_pos+0 through rel_pos+4 hold MFLPT byte[4] through
; byte[0] respectively (generalizing the 16-bit convention above, where
; rel_pos+0 is the "low"/last-pushed byte and rel_pos+1 is "high"/
; first-pushed, out to 5 bytes: rel_pos+4 plays "high" here). No ref
; parameter -- float is a value type, never GC-tracked, same as
; locals_push_value16/locals_pull_value16 above.
locals_push_valueflt .macro rel_pos
  ldy stackPointer
  lda localsStack-\rel_pos+4,y
  #stack_push_int_a
  lda localsStack-\rel_pos+3,y
  #stack_push_int_a
  lda localsStack-\rel_pos+2,y
  #stack_push_int_a
  lda localsStack-\rel_pos+1,y
  #stack_push_int_a
  lda localsStack-\rel_pos,y
  #stack_push_int_a
.endm

locals_pull_valueflt .macro rel_pos
  ldy stackPointer
  #stack_pull_int_a
  sta localsStack-\rel_pos,y
  #stack_pull_int_a
  sta localsStack-\rel_pos+1,y
  #stack_pull_int_a
  sta localsStack-\rel_pos+2,y
  #stack_pull_int_a
  sta localsStack-\rel_pos+3,y
  #stack_pull_int_a
  sta localsStack-\rel_pos+4,y
.endm
