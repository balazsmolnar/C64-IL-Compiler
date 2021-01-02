heapPointer = $23
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
  ldx #1
- inx
  inx
  cmp objTable,x
  bne -

  ldy #0
- lda #0 
  sta (heapPointer),y
  iny
  bne -
  dex
  lda heapPointer
  adc #.size
  sta heapPointer
  sta objTable,x
  bcc +
  inc heapPointer+1
  inx
  lda heapPointer+1
  sta objTable,x
+ 
  ;jsr .method
}

!macro stfld .pos {
  +stack_pull_int $27
  +stack_pull_int $28

  ldx $27
  lda heap,x
  sta tmpPointer
  lda heap+1,x
  sta tmpPointer+1

  ldy #.pos
  lda $28
  sta (tmpPointer),y 
}

!macro ldfld .pos {
  +stack_pull_int $27

  ldx $27
  lda heap,x
  sta tmpPointer
  lda heap+1,x
  sta tmpPointer+1

  ldy #.pos
  lda (tmpPointer),y

  pha
}