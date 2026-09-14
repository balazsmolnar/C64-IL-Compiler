
; Resolves an object handle (X) into a pointer at tmpPointer/tmpPointer+1.
; Preserves X. Shared by every macro in heap.asm/optimized.asm that
; dereferences an object handle -- factored out since the load/store pair
; was duplicated inline nearly 800 times across those two files.
;
; Lives here (not in heap.asm, where those macros are defined) because this
; file is .include'd AFTER the #start_at/*= org directive in every entry
; point (main.asm/unittest.asm/presentation.asm), while heap.asm is
; included before it. heap.asm's contents were previously all .macro
; bodies -- which emit no bytes on their own -- so the org's placement
; relative to it never mattered until this subroutine (real code, not a
; macro) needed to live somewhere.
resolveObjPtr
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1
  rts

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
  sta zp_param2_low  ; Size
  sty zp_param2_high  ; Reference fields

  ; Find an empty entry in the object table --> X
  jsr findEmptySlot

  ; Clear memory for the object in the heap
  ldy #0
- lda #0
  sta (heapPointer),y
  iny
  cpy zp_param2_low
  bne -
  jmp setObjParams

;
; Creates a new object on the heap, initializes with initial values
; Inputs:
; A: Size: Size of the object
; Y: ReferenceFields: Number of .fields references to other objects (always the first fields)
; zp_param1_low Init values low byte
; zp_param1_high Init values high byte
; Output:
; Newly created object id --> A
;
; Destroys: X, Y, A
newObjLInit

  ; store input parameters
  sta zp_param2_low  ; Size
  sty zp_param2_high  ; Reference fields

  ; Find an empty entry in the object table --> X
  jsr findEmptySlot

  ; Copy values
  ldy #0
- lda (zp_param1_low),y
  sta (heapPointer),y
  iny
  cpy zp_param2_low
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
  lda zp_param0_low
  sta objTableDescLow,x
  lda zp_param0_high
  sta objTableDescHigh,x

  lda zp_param2_low
  sta objTableSize,x
  lda zp_param2_high
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
  adc zp_param2_low
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
  sta zp_param0_low
  lda objTableDescHigh,x
  sta zp_param0_high

  lda (zp_param0_low),y
  sta mod+1
  iny
  lda (zp_param0_low),y
  sta mod+2

mod:
  jmp 0
