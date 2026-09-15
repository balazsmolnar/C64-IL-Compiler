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

init_locals_pull_parameters .macro localsSize, ref_list

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
      dec objTableRootCount, x
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
    dec objTableRootCount, x
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
