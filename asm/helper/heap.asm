heapPointer = $fb
tmpPointer = $25

!macro initHeap .heap {
  lda #<.heap
  sta heapPointer
  lda #>.heap
  sta heapPointer+1
}

!macro newObj .size {

  lda #0
  cmp #.size
  beq +
  ldx #0
- inx
  cmp objTableHigh,x
  bne -

  ldy #0
- lda #0 
  sta (heapPointer),y
  iny
  cpy #.size
  bne -
+ lda heapPointer
  sta objTableLow,x
  lda heapPointer+1
  sta objTableHigh,x
  lda heapPointer
  clc
  adc #.size
  sta heapPointer
  bcc +
  inc heapPointer+1
  ;jsr .method
+ txa
  pha
}

!macro stfld .pos {
  +stack_pull_int $fd
  +stack_pull_int $fe

  ldx $fe
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #.pos
  lda $fd
  sta (tmpPointer),y 
}

!macro ldfld .pos {
  +stack_pull_int $fd

  ldx $fd
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #.pos
  lda (tmpPointer),y

  pha
}