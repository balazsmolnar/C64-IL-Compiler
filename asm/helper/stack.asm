; Use C64 native stack
; less space, but faster
;
; Top of stack in A (Compiler/TopOfStackInA.cs): when one emitted line ends
; by pushing a byte and the next begins by pulling it, the compiler emits
; "tos_out ::= 1" before the first; its final push (stack_push_tos) then
; leaves the value in A and sets tos_in instead of doing pha, and the next
; pull (stack_pull_int_a, which every pull of the top byte goes through)
; skips its pla. Both are assembly-time variables, so the only change in
; the program is the missing pha/pla pair; the ".cerror" checks the
; compiler emits after each such line make a macro that doesn't play along
; fail the build instead of miscompiling.
tos_in := 0
tos_out := 0

; The final push of a macro the compiler may ask to leave its result in A.
; Only ever the LAST thing a macro does: whatever followed it would run
; while the value is not yet on the stack.
stack_push_tos .macro
  .cerror tos_in, "top of stack already in A"
  .if tos_out
tos_out ::= 0
tos_in ::= 1
  .else
    pha
  .endif
.endm

stack_push_pointer .macro ptr
  lda # > \ptr
  pha
  lda # < \ptr
  #stack_push_tos
.endm

stack_pull_pointer .macro address 
  pla
  sta \address
  pla
  sta \address+1
.endm

stack_push_int16 .macro value 
  lda # > \value
  pha
  lda # < \value
  #stack_push_tos
.endm

stack_push_int8 .macro value 
  lda # < \value
  #stack_push_tos
.endm

stack_pull_int16 .macro address
  #stack_pull_int_a
  sta \address
  pla
  sta \address+1
.endm

stack_pull_int8 .macro address
  #stack_pull_int_a
  sta \address
.endm

stack_push_var16 .macro address 
  lda \address+1
  pha
  lda \address
  #stack_push_tos
.endm

stack_push_var8 .macro address 
  lda \address
  #stack_push_tos
.endm

stack_duplicate8 .macro
  #stack_pull_int_a
  #stack_push_int_a
  #stack_push_tos
.endm

stack_duplicate16 .macro
  #stack_pull_int_x
  #stack_pull_int_y
  #stack_push_int_y
  #stack_push_int_x
  #stack_push_int_y
  #stack_push_int_x
.endm

stack_pop .macro
  #stack_pull_int_a
.endm

; Discards a 2-byte value (ulong, string pointer): the top byte (which may
; be in A, see TopOfStackInA), then the one under it.
stack_pop16 .macro
  #stack_pull_int_a
  pla
.endm

; Discards an n-byte value (a float: 5).
stack_pop_n .macro bytes
  #stack_pull_int_a
  .rept \bytes - 1
  pla
  .endrept
.endm

stack_save_return_adress .macro variable
  pla
  sta \variable
  pla
  sta \variable+1
.endm

stack_return_to_saved_address .macro  variable 
  lda \variable+1
  pha
  lda \variable
  pha
  rts
.endm

stack_pull_int_a .macro
  .if tos_in
tos_in ::= 0
  .else
    pla
  .endif
.endm

stack_pull_int_x .macro
  #stack_pull_int_a
  tax
.endm

stack_pull_int_y .macro 
  #stack_pull_int_a
  tay
.endm

stack_push_int_a .macro
  .cerror tos_in, "top of stack still in A"
  pha
.endm

stack_push_int_x .macro
  txa
  #stack_push_int_a
.endm

stack_push_int_y .macro
  tya
  #stack_push_int_a
.endm

stack_pull_int_ref .macro address, ref 

  ; deref
  .if \ref == 1 
    #deref \address
  .endif
  ; store
  #stack_pull_int_a
  sta \address

  ; ref
  .if \ref == 1 
    tax
    inc objTableRootCount, x
  .endif
.endm

deref .macro address
    ldx \address
    dec objTableRootCount, x
    lda #0
    sta \address
.endm

; 5-byte MFLPT (float) push/pull -- asm/helper/float.asm's only consumer.
; Convention: byte[0] (exponent) pushed first, byte[4] (last mantissa byte)
; pushed last, so it's on top -- generalizes stack_push_var16/
; stack_pull_int16's "low byte pushed last, pulled first" convention from 2
; bytes to 5, just with "byte[0]" playing the role "high" plays there.
stack_push_var_mflpt .macro address
  lda \address
  pha
  lda \address+1
  pha
  lda \address+2
  pha
  lda \address+3
  pha
  lda \address+4
  pha
.endm

stack_pull_mflpt .macro address
  pla
  sta \address+4
  pla
  sta \address+3
  pla
  sta \address+2
  pla
  sta \address+1
  pla
  sta \address
.endm

; Newobj for a static-method delegate (`ldnull; ldftn M; newobj
; SomeDelegate::.ctor(object, native int)`) -- see OpNewObj's delegate
; special case in Compiler/Operands/OperandBase.cs for why this replaces
; #newObj entirely rather than allocating a real heap object. Ldftn pushes
; the method's address as a real 2-byte pointer (#stack_push_pointer,
; on top since it's pushed last); Ldnull pushes a 1-byte 0 underneath it
; (#stack_push_int8's convention -- object references are 1-byte
; object-table handles in this compiler, see objectTables.asm, and null is
; handle 0). Pulls the pointer aside, discards the 1-byte null target
; underneath it, then pushes the pointer back as the delegate's own value
; -- net effect: unwrap `ldnull; ldftn M` down to just `ldftn M`. Uses
; zp_tmp1_low/high as scratch -- safe here despite that pair's usual "save
; my own return address" role (see stack_save_return_adress/
; stack_return_to_saved_address above): this macro expands inline with
; nothing else running between these three steps, unlike the jsr'd
; subroutines that pair actually protects.
stack_construct_static_delegate .macro
  #stack_pull_pointer zp_tmp1_low
  #stack_pull_int_a
  #stack_push_var16 zp_tmp1_low
.endm

; Immediate 5-byte push, for Ldc_r4 (OpLdc_r4 in Compiler/Operands/
; OperandBase.cs already converts the float literal to MFLPT bytes at
; compile time -- this just pushes those literal values). Same forward
; push order as stack_push_var_mflpt above.
stack_push_mflpt_const .macro b0, b1, b2, b3, b4
  lda #\b0
  pha
  lda #\b1
  pha
  lda #\b2
  pha
  lda #\b3
  pha
  lda #\b4
  pha
.endm

; Direct copy of an int literal into a named zp byte/pair, for
; ILMethodIntOperandFusionOptimizer (Compiler/ILMethodIntOperandFusionOptimizer.cs)
; -- the integer counterpart of copy_mflpt_const below. Same "<"/">" byte-
; extraction 64tass itself does the sign/zero-extension math for, exactly
; as stack_push_int8/stack_push_int16 (asm/helper/stack.asm) already rely
; on for this same raw value.
copy_int8_const .macro value, target
  lda #<\value
  sta \target
.endm

copy_int16_const .macro value, target
  lda #>\value
  sta \target+1
  lda #<\value
  sta \target
.endm

; Direct copy of an Ldc_r4 literal's own 5 bytes into zp_flt_a/zp_flt_b,
; for ILMethodFloatOperandFusionOptimizer (Compiler/
; ILMethodFloatOperandFusionOptimizer.cs): skips the push/pull round trip
; through the hardware stack stack_push_mflpt_const above would otherwise
; use when a float constant feeds straight into a binary float op. Same
; byte order as stack_push_mflpt_const + stack_pull_mflpt's own net
; mapping (b0->target+0 ... b4->target+4, a plain copy -- see
; stack_push_mflpt_const's own "same forward push order as
; stack_push_var_mflpt" comment above).
copy_mflpt_const .macro b0, b1, b2, b3, b4, target
  lda #\b0
  sta \target
  lda #\b1
  sta \target+1
  lda #\b2
  sta \target+2
  lda #\b3
  sta \target+3
  lda #\b4
  sta \target+4
.endm

