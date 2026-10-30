; Stack for parameters and local variables

; ---------------------------------------------------------------------------
; Static (compiled) frames -- Compiler/ILStaticFramePass.cs.
;
; A local/parameter is named by its rel_pos everywhere in the generated code
; and in the macros below. A rel_pos under 256 is the usual dynamic slot,
; localsStack-rel_pos indexed by Y (or X) = stackPointer. A rel_pos of 256 or
; more belongs to a method with a static frame: the slot is the fixed byte
; SF_TOP-rel_pos (inside StaticFrames, asm/helper/objectTables.asm), read and
; written with plain absolute (or zero-page) addressing and no stackPointer
; load. The compiler shifts a static method's whole frame layout by a
; constant, so every rel_pos arithmetic (+1 for a 16-bit high byte, ...)
; stays the same in both cases.
;
; ls_op op, rel_pos, k     op on byte k of the slot (op = lda/sta/ldx/...)
; ls_opx op, rel_pos, k    same, X-indexed for the dynamic case
; ls_ldyN / ls_ldxN        load Y / X = stackPointer unless all N rel_pos
;                          arguments are static
; ---------------------------------------------------------------------------
ls_op .macro op, rel_pos, k
  .if \rel_pos >= 256
    \op SF_TOP-\rel_pos+\k
  .else
    \op localsStack-\rel_pos+\k,y
  .endif
.endm

ls_opx .macro op, rel_pos, k
  .if \rel_pos >= 256
    \op SF_TOP-\rel_pos+\k
  .else
    \op localsStack-\rel_pos+\k,x
  .endif
.endm

ls_ldy1 .macro rel_pos
  .if \rel_pos < 256
    ldy stackPointer
  .endif
.endm

ls_ldy2 .macro rel_pos1, rel_pos2
  .if \rel_pos1 < 256 || \rel_pos2 < 256
    ldy stackPointer
  .endif
.endm

ls_ldx1 .macro rel_pos
  .if \rel_pos < 256
    ldx stackPointer
  .endif
.endm

; Return from a static-frame method (no prologue, the jsr return address
; never left the hardware stack). The return value, which IL leaves alone on
; the evaluation stack at a ret, sits on top of that return address, so it
; travels in registers instead: 1 byte in A, 2 bytes in A (high) and X
; (low). The caller pushes it back right after its jsr (sf_result).
sf_ret .macro bytes
  .if \bytes == 1
    pla
  .elsif \bytes == 2
    pla
    tax
    pla
  .endif
  rts
.endm

; Entry of a static-frame method with reference-typed locals: zero them, so
; a local's first store does not "dec old" whatever another method's frame
; (frames overlay each other) or an earlier call left in that byte -- the
; same reason init_locals_pull_parameters zeroes them.
sf_enter .macro local_ref_list
  .if len(\local_ref_list) > 0
    lda #0
    .for pos in \local_ref_list
      sta SF_TOP-pos
    .next
  .endif
.endm

; Exit of a static-frame method: release every reference-typed parameter
; and local (method_exit's ref_list), then sf_ret.
sf_exit .macro bytes, ref_list
  .if len(\ref_list) > 0
    .for ref in \ref_list
      ldx SF_TOP-ref
      jsr DecRefCountIfAllocated
    .next
  .endif
  #sf_ret \bytes
.endm

; Callee-pulls static frame (StaticFrameInfo.CalleePulls): the entry saved
; the return address in the frame (low byte at SF_TOP-\ret_rel) and pulled
; the arguments, so the result is already where the caller wants it, on the
; stack. Release the references, push the return address back, rts.
sf_exit_saved .macro ret_rel, ref_list
  .if len(\ref_list) > 0
    .for ref in \ref_list
      ldx SF_TOP-ref
      jsr DecRefCountIfAllocated
    .next
  .endif
  lda SF_TOP-\ret_rel+1
  pha
  lda SF_TOP-\ret_rel
  pha
  rts
.endm

sf_result .macro bytes
  .if \bytes == 1
    pha
  .elsif \bytes == 2
    pha
    txa
    pha
  .endif
.endm

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

; Releases the frame's reference-typed params/locals inline, then jumps into
; the shared tail (asm/helper/frame.asm) that rewinds stackPointer and
; returns through the saved return address: 3 bytes per `ret` for the
; common frame sizes instead of the 17-byte inline epilogue this used to be.
method_exit .macro stackSize, ref_list

  .if len(\ref_list) > 0
    ldy stackPointer
    .for ref in \ref_list
      ldx localsStack-ref,y
      jsr DecRefCountIfAllocated
    .next
  .endif

  .if \stackSize == 2
    jmp Method_Exit_2
  .elsif \stackSize == 3
    jmp Method_Exit_3
  .elsif \stackSize == 4
    jmp Method_Exit_4
  .elsif \stackSize == 5
    jmp Method_Exit_5
  .elsif \stackSize == 6
    jmp Method_Exit_6
  .elsif \stackSize == 7
    jmp Method_Exit_7
  .elsif \stackSize == 8
    jmp Method_Exit_8
  .else
    lda #(256 - \stackSize) & 255
    jmp Method_Exit_A
  .endif
.endm

; "Frameless" methods (static, void, no parameters -- ILMethodEmitPass
; decides): the jsr return address simply stays on the hardware stack (a
; void method's evaluation stack is empty at every ret, so it is on top
; again by then), so there is nothing to save at entry or restore at exit.
; Only the locals (if any) get a localsStack slice, addressed exactly like a
; normal frame's (rel_pos counts down from stackPointer, and a normal
; frame's return-address bytes sit BELOW its locals, so dropping them moves
; nothing else). localsSize 0 and no reference locals: no prologue at all,
; and every ret is a bare rts.
method_enter_frameless .macro localsSize, local_ref_list
  .if \localsSize > 0
    ldy stackPointer
    .if \localsSize == 1
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
    .if len(\local_ref_list) > 0
      lda #0
      .for pos in \local_ref_list
        sta localsStack-pos,y
      .next
    .endif
    sty stackPointer
  .endif
.endm

method_leave .macro localsSize, ref_list
  .if len(\ref_list) > 0
    ldy stackPointer
    .for ref in \ref_list
      ldx localsStack-ref,y
      jsr DecRefCountIfAllocated
    .next
  .endif

  .if \localsSize == 0
    rts
  .elsif \localsSize == 1
    jmp Method_Leave_1
  .elsif \localsSize == 2
    jmp Method_Leave_2
  .elsif \localsSize == 3
    jmp Method_Leave_3
  .elsif \localsSize == 4
    jmp Method_Leave_4
  .else
    lda #(256 - \localsSize) & 255
    jmp Method_Leave_A
  .endif
.endm


locals_pull_value8 .macro rel_pos, ref
  #ls_ldy1 \rel_pos
  #locals_pull_value8_ky \rel_pos, \ref
.endm

; locals_pull_value8 with Y already holding stackPointer (ILMethodEmitPass's
; YRegisterTracker proves it and drops the reload).
locals_pull_value8_ky .macro rel_pos, ref

  ; deref
  .if \ref == 1
    #ls_op ldx, \rel_pos, 0
    jsr DecRefCountIfAllocated
  .endif
  #stack_pull_int_a
  #ls_op sta, \rel_pos, 0
  ; ref
  .if \ref == 1 
    tax
    inc objTableRootCount, x
  .endif
.endm

locals_push_value8 .macro rel_pos
  #ls_ldy1 \rel_pos
  #locals_push_value8_ky \rel_pos
.endm

; locals_push_value8 with Y already holding stackPointer (ILMethodEmitPass's
; YRegisterTracker proves it and drops the reload).
locals_push_value8_ky .macro rel_pos
  #ls_op lda, \rel_pos, 0
  #stack_push_tos
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
  #stack_push_tos
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
  #ls_ldy1 \rel_pos
  #locals_push_value16_ky \rel_pos
.endm

; locals_push_value16 with Y already holding stackPointer (ILMethodEmitPass's
; YRegisterTracker proves it and drops the reload).
locals_push_value16_ky .macro rel_pos
  #ls_op lda, \rel_pos, 1
  #stack_push_int_a
  #ls_op lda, \rel_pos, 0
  #stack_push_tos
.endm

locals_pull_value16 .macro rel_pos
  #ls_ldy1 \rel_pos
  #locals_pull_value16_ky \rel_pos
.endm

; locals_pull_value16 with Y already holding stackPointer (ILMethodEmitPass's
; YRegisterTracker proves it and drops the reload).
locals_pull_value16_ky .macro rel_pos
  #stack_pull_int_a
  #ls_op sta, \rel_pos, 0
  #stack_pull_int_a
  #ls_op sta, \rel_pos, 1
.endm

; float locals -- rel_pos+0 through rel_pos+4 hold MFLPT byte[4] through
; byte[0] respectively (generalizing the 16-bit convention above, where
; rel_pos+0 is the "low"/last-pushed byte and rel_pos+1 is "high"/
; first-pushed, out to 5 bytes: rel_pos+4 plays "high" here). No ref
; parameter -- float is a value type, never GC-tracked, same as
; locals_push_value16/locals_pull_value16 above.
locals_push_valueflt .macro rel_pos
  #ls_ldy1 \rel_pos
  #ls_op lda, \rel_pos, 4
  #stack_push_int_a
  #ls_op lda, \rel_pos, 3
  #stack_push_int_a
  #ls_op lda, \rel_pos, 2
  #stack_push_int_a
  #ls_op lda, \rel_pos, 1
  #stack_push_int_a
  #ls_op lda, \rel_pos, 0
  #stack_push_int_a
.endm

locals_pull_valueflt .macro rel_pos
  #ls_ldy1 \rel_pos
  #stack_pull_int_a
  #ls_op sta, \rel_pos, 0
  #stack_pull_int_a
  #ls_op sta, \rel_pos, 1
  #stack_pull_int_a
  #ls_op sta, \rel_pos, 2
  #stack_pull_int_a
  #ls_op sta, \rel_pos, 3
  #stack_pull_int_a
  #ls_op sta, \rel_pos, 4
.endm

; Direct local/param-to-zp_flt copy, for ILMethodFloatOperandFusionOptimizer
; (Compiler/ILMethodFloatOperandFusionOptimizer.cs): skips the push/pull
; round trip through the hardware stack that locals_push_valueflt +
; stack_pull_mflpt (asm/helper/stack.asm) would otherwise use when a float
; Ldloc/Ldarg feeds straight into a binary float op.
;
; NOT a straight byte-for-byte copy -- rel_pos+0 is localsStack's "low"/
; last-pushed byte (see locals_push_valueflt's own comment above), which is
; the FIRST byte stack_pull_mflpt would land, landing it at \target+4, not
; \target+0. Reproducing that same reversal here (rather than the simpler-
; looking rel_pos+N -> target+N) is what keeps this byte-identical to the
; unfused push-then-pull path; getting this backwards would silently
; corrupt every fused float value while still "working" structurally (the
; same class of bug as the OpPushFld float-width miss documented in
; Test/InstanceFloatFieldBugTest.cs).
; Direct local/param-to-zp copy, for ILMethodIntOperandFusionOptimizer
; (Compiler/ILMethodIntOperandFusionOptimizer.cs) -- the 8/16-bit integer
; counterpart of locals_copy_valueflt below. No byte reversal here (unlike
; the float case): locals_push_value16's own rel_pos+0/+1 convention
; already maps straight onto a pulling consumer's low/high byte order (see
; that macro's own comment), so a plain copy is correct.
locals_copy_value8 .macro rel_pos, target
  #ls_ldy1 \rel_pos
  #locals_copy_value8_ky \rel_pos, \target
.endm

; locals_copy_value8 with Y already holding stackPointer (ILMethodEmitPass's
; YRegisterTracker proves it and drops the reload).
locals_copy_value8_ky .macro rel_pos, target
  #ls_op lda, \rel_pos, 0
  sta \target
.endm

locals_copy_value16 .macro rel_pos, target
  #ls_ldy1 \rel_pos
  #locals_copy_value16_ky \rel_pos, \target
.endm

; locals_copy_value16 with Y already holding stackPointer (ILMethodEmitPass's
; YRegisterTracker proves it and drops the reload).
locals_copy_value16_ky .macro rel_pos, target
  #ls_op lda, \rel_pos, 0
  sta \target
  #ls_op lda, \rel_pos, 1
  sta \target+1
.endm

locals_copy_valueflt .macro rel_pos, target
  #ls_ldy1 \rel_pos
  #ls_op lda, \rel_pos, 0
  sta \target+4
  #ls_op lda, \rel_pos, 1
  sta \target+3
  #ls_op lda, \rel_pos, 2
  sta \target+2
  #ls_op lda, \rel_pos, 3
  sta \target+1
  #ls_op lda, \rel_pos, 4
  sta \target
.endm
