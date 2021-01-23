heapPointer = $fb
tmpPointer = $25

!macro initHeap .heap {
  lda #<.heap
  sta heapPointer
  lda #>.heap
  sta heapPointer+1
}


!macro newObj .size {

  ldx #0
- inx
  lda objTableHigh,x
  bne -

  ldy #0
- lda #0 
  sta (heapPointer),y
  iny
  cpy #.size
  bne -
+ lda #.size
  sta objTableSize,x
  lda heapPointer
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

!macro newArr {

  +stack_pull_int $34
  ldx #0
- inx
  lda objTableHigh,x
  bne -

  ldy #0
- lda #0 
  sta (heapPointer),y
  iny
  cpy $34
  bne -
  lda $34
  sta objTableSize,x
  lda heapPointer
  sta objTableLow,x
  lda heapPointer+1
  sta objTableHigh,x
  lda heapPointer
  clc
  adc $34
  sta heapPointer
  bcc +
  inc heapPointer+1
+ txa
  +stack_push_int_a
}

!macro stfld .pos {
  +stack_pull_int $fd
  +stack_pull_int_x

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #.pos
  lda $fd
  sta (tmpPointer),y 
}

!macro stsfld .address {

  ldx .address
  dec objTableRootCount, x

  +stack_pull_int_x
  inc objTableRootCount, x
  stx .address
}

!macro stelemRef {
  +stack_pull_int $fd   ; value
  +stack_pull_int_y     ; index
  +stack_pull_int_x     ; object refernce to array

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

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

!macro ldelemRef  {
  +stack_pull_int_y
  +stack_pull_int_x

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  lda (tmpPointer),y

  +stack_push_int_a
}

!macro ldlen  {
  +stack_pull_int_x
  lda objTableSize,x
  +stack_push_int_a
}