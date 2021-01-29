
;
; Creates a new object on the heap
; Inputs:
; A: Size: Size of the object
; Y: ReferenceFields: Number of .fields references to other objects (always the first fields)
; Output:
; Newly created object id --> A
;
; Destroys: X, Y, A
newObjL

  ; store input parameters
  sta $34  ; Size
  sty $35  ; Reference fields

  ; Find an empty entry in the object table --> X
  ldx #0
- inx
  beq out_of_memory
  lda objTableHigh,x
  bne -

  ; Clear memory for the object in the heap
  ldy #0
- lda #0 
  sta (heapPointer),y
  iny
  cpy $34
  bne -

  ; set values in the tables: Size, referenceFieldCount, pointer
  lda $34
  sta objTableSize,x
  lda $35
  sta objTableReferences,x
  lda #0
  sta objTableRootCount, x
  lda heapPointer
  sta objTableLow,x
  lda heapPointer+1
  sta objTableHigh,x

  ; Increase heap counter
  lda heapPointer
  clc
  adc $34
  sta heapPointer
  bcc +
  inc heapPointer+1
+ txa
  rts
out_of_memory
  brk