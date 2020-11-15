
;Variable stack

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

!macro stack_push_pointer .ptr {
  ldy stackPointer
  lda # < .ptr
  sta (stackStart),y
  iny
  lda # > .ptr
  sta (stackStart),y
  iny
  sty stackPointer
}

!macro stack_pull_pointer .address {
  ldy stackPointer
  dey
  lda (stackStart),y
  sta .address+1
  dey
  lda (stackStart),y
  sta .address
  sty stackPointer
}

!macro stack_push_int value {
  ldy stackPointer
  lda # < value
  sta (stackStart),y
  iny
  lda # > value
  sta (stackStart),y
  iny
  sty stackPointer
}

!macro stack_pull_int .address {
  ldy stackPointer
  dey
  lda (stackStart),y
  sta .address+1
  dey
  lda (stackStart),y
  sta .address
  sty stackPointer
}

!macro stack_push_var .address {
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
;