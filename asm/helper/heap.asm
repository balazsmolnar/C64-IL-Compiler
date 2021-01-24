heapPointer = $fb
tmpPointer = $25

!macro initHeap .heap {
  lda #<.heap
  sta heapPointer
  lda #>.heap
  sta heapPointer+1
}

;
; Creates a new object on the heap
; Inputs:
; .size: Size of the object on the heap
; .referenceFields: Number of .fields references to other objects (always the first fields)
; Output:
; Newly created object id on the stack
;
!macro newObj .size, .referenceFields {

  lda #.size
  ldy #.referenceFields

  jsr newObj

  +stack_push_int_a
}

!macro newArr {

  +stack_pull_int_a  ; size
  tay                ; reference fields
  jsr newObj

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