; Shared method-exit tails (real subroutine code, not macros -- included
; after the org directive, like object.asm). Every compiled method used to
; expand its whole epilogue inline at each `ret` (17 bytes: rewind
; stackPointer, push the saved return address back, rts); now a `ret` is a
; 3-byte jmp into one of these (asm/helper/localsStack.asm's method_exit /
; method_leave macros pick the entry point). Cost: about 6 cycles per
; return over the inline version.
;
; Method_Exit_N (N = the frame's stackSize: 2 return-address bytes + locals
; + parameters): rewinds stackPointer by N, then returns through the
; return address the prologue (init_locals) saved at the frame's bottom.
; Frames of 2..8 bytes have an entry point; larger ones come in through
; Method_Exit_A with A = -N (5 bytes at the call site).
Method_Exit_2
  lda #254
  bne Method_Exit_A
Method_Exit_3
  lda #253
  bne Method_Exit_A
Method_Exit_4
  lda #252
  bne Method_Exit_A
Method_Exit_5
  lda #251
  bne Method_Exit_A
Method_Exit_6
  lda #250
  bne Method_Exit_A
Method_Exit_7
  lda #249
  bne Method_Exit_A
Method_Exit_8
  lda #248
Method_Exit_A
  clc
  adc stackPointer
  sta stackPointer
  tax
  lda localsStack+1,x
  pha
  lda localsStack,x
  pha
  rts

; Method_Leave_N: the epilogue of a "frameless" method (static, void, no
; parameters -- see method_enter_frameless in localsStack.asm): its jsr
; return address never left the hardware stack, so leaving is just
; dropping its N bytes of locals and an rts. N = 0 never gets here (the
; macro emits a bare rts). Larger frames: Method_Leave_A with A = -N.
Method_Leave_1
  dec stackPointer
  rts
Method_Leave_2
  lda #254
  bne Method_Leave_A
Method_Leave_3
  lda #253
  bne Method_Leave_A
Method_Leave_4
  lda #252
Method_Leave_A
  clc
  adc stackPointer
  sta stackPointer
  rts
