
;Variable stack
; Custom implementation
; Stack is behind code
; 16 bit of data is expected
!macro stack_init .stack {
  stackPointer = $21
  stackStart = $22

  lda # 0
  sta stackPointer
  lda # < .stack
  sta stackStart
  lda # > .stack
  sta stackStart+1
}

!macro stack_push_pointer_2 .ptr {
  ldy stackPointer
  lda # < .ptr
  sta (stackStart),y
  iny
  lda # > .ptr
  sta (stackStart),y
  iny
  sty stackPointer
}

!macro stack_pull_pointer_2 .address {
  ldy stackPointer
  dey
  lda (stackStart),y
  sta .address+1
  dey
  lda (stackStart),y
  sta .address
  sty stackPointer
}

!macro stack_push_int_2 value {
  ldy stackPointer
  lda # < value
  sta (stackStart),y
  iny
  lda # > value
  sta (stackStart),y
  iny
  sty stackPointer
}

!macro stack_pull_int_2 .address {
  ldy stackPointer
  dey
  lda (stackStart),y
  sta .address+1
  dey
  lda (stackStart),y
  sta .address
  sty stackPointer
}

!macro stack_push_var_2 .address {
  ldy stackPointer
  ldx #0
  lda .address,x
  sta (stackStart),y
  inx
  iny
  lda .address,x
  sta (stackStart),y
  iny
  sty stackPointer
}

; Use C64 native stack
; less space, but faster
;
!macro stack_push_pointer .ptr {
  lda # < .ptr
  pha
  lda # > .ptr
  pha
}

!macro stack_pull_pointer .address {
  pla
  sta .address+1
  pla
  sta .address
}

!macro stack_push_int value {
  lda # < value
  pha
  lda # > value
  pha
}

!macro stack_pull_int .address {
  pla
  sta .address+1
  pla
  sta .address
}

!macro stack_push_var .address {
  lda .address
  pha
  lda .address+1
  pha
}

!macro stack_save_return_adress .variable {
  pla
  sta .variable+1
  pla
  sta .variable
}

!macro stack_return_to_saved_address .variable {
  lda .variable
  pha
  lda .variable+1
  pha
  rts
}
