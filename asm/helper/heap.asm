heapPointer = $fb
tmpPointer = heap_tmp_pointer

initHeap .macro heap 
  lda #<\heap
  sta heapPointer
  lda #>\heap
  sta heapPointer+1
  ldx #0
  lda #0
- sta objTableSize,x
  sta objTableRootCount,x
  sta objTableLow,x
  sta objTableHigh,x
  inx
  bne -
.endm

;
; Creates a new object on the heap
; Inputs:
; size: Size of the object on the heap
; referenceFields: Number of fields references to other objects (always the first fields)
; Output:
; Newly created object id on the stack
;
newObj .macro size, referenceFields 

  lda #\size
  ldy #\referenceFields

  jsr newObjL

  #stack_push_int_a
.endm

newArr .macro  

  #stack_pull_int_a  ; size
  tay                ; reference fields
  jsr newObjL

  #stack_push_int_a
.endm

stfld8 .macro  pos 
  #stack_pull_int $fd
  #stack_pull_int_x

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #\pos
  lda $fd
  sta (tmpPointer),y 
.endm

stfld16 .macro  pos 
  #stack_pull_int $fd
  #stack_pull_int $fe
  #stack_pull_int_x

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #\pos
  lda $fd
  sta (tmpPointer),y 
  lda $fe
  iny
  sta (tmpPointer),y 
.endm

stsfld .macro address 

  ldx \address
  dec objTableRootCount, x

  #stack_pull_int_x
  inc objTableRootCount, x
  stx \address
.endm

stelemRef .macro  
  #stack_pull_int $fd   ; value
  #stack_pull_int_y     ; index
  #stack_pull_int_x     ; object refernce to array

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  lda $fd
  sta (tmpPointer),y 
.endm

stelem .macro  
  #stack_pull_int $fd   ; value
  #stack_pull_int_y     ; index
  #stack_pull_int_x     ; object refernce to array

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  lda $fd
  sta (tmpPointer),y 
.endm

ldfld8 .macro pos 
  #stack_pull_int_x
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #\pos
  lda (tmpPointer),y

  #stack_push_int_a
.endm

ldfld16 .macro pos 
  #stack_pull_int_x
  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  ldy #\pos+1
  lda (tmpPointer),y
  #stack_push_int_a
  dey
  lda (tmpPointer),y
  #stack_push_int_a

.endm

ldelemRef .macro   
  #stack_pull_int_y
  #stack_pull_int_x

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  lda (tmpPointer),y

  #stack_push_int_a
.endm

ldelem .macro   
  #stack_pull_int_y
  #stack_pull_int_x

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  lda (tmpPointer),y

  #stack_push_int_a
.endm

ldlen .macro  
  #stack_pull_int_x
  lda objTableSize,x
  #stack_push_int_a
.endm