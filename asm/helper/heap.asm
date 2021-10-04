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
newObj .macro size, referenceFields, objDescriptor, ctor

  lda #<\objDescriptor
  sta $30
  lda #>\objDescriptor
  sta $31

  lda #\size
  ldy #\referenceFields

  jsr newObjL

  .if \ctor=0
    #stack_push_int_a
  .else
    sta $fa
    #stack_push_int_a
    jsr \ctor
    lda $fa
    #stack_push_int_a
  .endif
.endm

;
; Creates a new object on the heap and initializes with default values
; Inputs:
; size: Size of the object on the heap
; referenceFields: Number of fields references to other objects (always the first fields)
; initValues: address of initial value table
; Output:
; Newly created object id on the stack
;
newObjInit .macro size, referenceFields, objDescriptor, initValues, ctor

  lda #<\objDescriptor
  sta $30
  lda #>\objDescriptor
  sta $31

  lda #<\initValues
  sta $32
  lda #>\initValues
  sta $33

  lda #\size
  ldy #\referenceFields

  jsr newObjLInit

  .if \ctor=0
    #stack_push_int_a
  .else
    sta $fa
    #stack_push_int_a
    jsr \ctor
    lda $fa
    #stack_push_int_a
  .endif
.endm

newArrRef .macro  

  #stack_pull_int_a  ; size
  tay                ; reference fields
  jsr newObjL

  #stack_push_int_a
.endm

newArr .macro  

  #stack_pull_int_a  ; size
  ldy 0              ; reference fields
  jsr newObjL

  #stack_push_int_a
.endm

newArr16 .macro  

  #stack_pull_int_a  ; size
  asl
  ldy 0              ; reference fields
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

stelem16 .macro  
  #stack_pull_int16 $fd   ; value
  #stack_pull_int_a     ; index
  asl
  tay
  #stack_pull_int_x     ; object refernce to array

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  lda $fd
  sta (tmpPointer),y
  lda $fe
  sta (tmpPointer+1),y
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

ldelem16 .macro   
  #stack_pull_int_a
  asl
  tay
  #stack_pull_int_x

  lda objTableLow,x
  sta tmpPointer
  lda objTableHigh,x
  sta tmpPointer+1

  lda (tmpPointer+1),y
  #stack_push_int_a
  lda (tmpPointer),y
  #stack_push_int_a
.endm

ldlen .macro  
  #stack_pull_int_x
  lda objTableSize,x
  #stack_push_int_a
.endm

ldlen16 .macro  
  #stack_pull_int_x
  lda objTableSize,x
  lsr
  #stack_push_int_a
.endm