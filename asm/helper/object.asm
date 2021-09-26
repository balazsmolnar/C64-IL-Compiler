
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
  jsr findEmptySlot
  
  ; Clear memory for the object in the heap
  ldy #0
- lda #0 
  sta (heapPointer),y
  iny
  cpy $34
  bne -
  jmp setObjParams

;
; Creates a new object on the heap, initializes with initial values
; Inputs:
; A: Size: Size of the object
; Y: ReferenceFields: Number of .fields references to other objects (always the first fields)
; $32 Init values low byte
; $33 Init values high byte
; Output:
; Newly created object id --> A
;
; Destroys: X, Y, A
newObjLInit

  ; store input parameters
  sta $34  ; Size
  sty $35  ; Reference fields

  ; Find an empty entry in the object table --> X
  jsr findEmptySlot
  
  ; Copy values
  ldy #0
- lda ($32),y
  sta (heapPointer),y
  iny
  cpy $34
  bne -
  jmp setObjParams

findEmptySlot
  ; Find an empty entry in the object table --> X
  ldx #0
- inx
  beq out_of_memory
  lda objTableHigh,x
  bne -
  rts
out_of_memory
  brk

setObjParams
  ; set values in the tables: Size, referenceFieldCount, pointer
  lda $30
  sta objTableDescLow,x
  lda $31
  sta objTableDescHigh,x

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

callVirt .macro methodIndex, objStackPos
  tsx
  lda $0100+\objStackPos, x
  tax
  lda #\methodIndex

  jsr callVirtL
  
.endm

;
; A - methodIndex
;
callVirtL

  asl
  tay

  lda objTableDescLow,x
  sta $30
  lda objTableDescHigh,x
  sta $31

  lda ($30),y
  sta mod+1
  iny
  lda ($30),y
  sta mod+2

mod:
  jmp 0
