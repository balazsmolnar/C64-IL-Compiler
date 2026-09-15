; heapPointer is defined in asm/helper/zeropage.asm, alongside every other
; zero-page claim in the codebase.
tmpPointer = heap_tmp_pointer

; resolveObjPtr (used by the macros below) is defined in object.asm, not
; here -- see the comment there for why.

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
  sta zp_param0_low
  lda #>\objDescriptor
  sta zp_param0_high

  lda #\size
  ldy #\referenceFields

  jsr newObjL

  .if \ctor=0
    #stack_push_int_a
  .else
    sta zp_ctor_result
    #stack_push_int_a
    jsr \ctor
    lda zp_ctor_result
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
  sta zp_param0_low
  lda #>\objDescriptor
  sta zp_param0_high

  lda #<\initValues
  sta zp_param1_low
  lda #>\initValues
  sta zp_param1_high

  lda #\size
  ldy #\referenceFields

  jsr newObjLInit

  .if \ctor=0
    #stack_push_int_a
  .else
    sta zp_ctor_result
    #stack_push_int_a
    jsr \ctor
    lda zp_ctor_result
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
  ldy #0              ; reference fields
  jsr newObjL

  #stack_push_int_a
.endm

newArr16 .macro

  #stack_pull_int_a  ; size
  asl
  ldy #0              ; reference fields
  jsr newObjL

  #stack_push_int_a
.endm

;
; Creates a new array on the heap and initializes it by copying size bytes
; from initValues -- like newObjInit, but simpler: an array of primitives
; has no vtable/reference fields (matches newArr/newArrRef, which also
; leave zp_param0_low/high alone; zeroed here for cleanliness, not because
; anything reads an array's descriptor fields -- callVirtL, the only
; reader, is only ever reached through an object reference, never an array
; one).
; Inputs:
; size: element count (compile-time constant, unlike newArr's runtime size)
; initValues: address of the constant byte data to copy in
; Output:
; Newly created array's object id on the stack
;
newArrInit .macro size, initValues

  lda #0
  sta zp_param0_low
  sta zp_param0_high

  lda #<\initValues
  sta zp_param1_low
  lda #>\initValues
  sta zp_param1_high

  lda #\size
  ldy #0

  jsr newObjLInit

  #stack_push_int_a
.endm

stfld8 .macro  pos 
  #stack_pull_int zp_field_value_low
  #stack_pull_int_x

  jsr resolveObjPtr

  ldy #\pos
  lda zp_field_value_low
  sta (tmpPointer),y 
.endm

stfld16 .macro  pos 
  #stack_pull_int zp_field_value_low
  #stack_pull_int zp_field_value_high
  #stack_pull_int_x

  jsr resolveObjPtr

  ldy #\pos
  lda zp_field_value_low
  sta (tmpPointer),y 
  lda zp_field_value_high
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
  #stack_pull_int zp_field_value_low   ; value
  #stack_pull_int_y     ; index
  #stack_pull_int_x     ; object refernce to array

  jsr resolveObjPtr

  lda zp_field_value_low
  sta (tmpPointer),y 
.endm

stelem .macro  
  #stack_pull_int zp_field_value_low   ; value
  #stack_pull_int_y     ; index
  #stack_pull_int_x     ; object refernce to array

  jsr resolveObjPtr

  lda zp_field_value_low
  sta (tmpPointer),y 
.endm

stelem16 .macro  
  #stack_pull_int16 zp_field_value_low   ; value
  #stack_pull_int_a     ; index
  asl
  tay
  #stack_pull_int_x     ; object refernce to array

  jsr resolveObjPtr

  lda zp_field_value_low
  sta (tmpPointer),y
  lda zp_field_value_high
  iny
  sta (tmpPointer),y
.endm

ldfld8 .macro pos 
  #stack_pull_int_x
  jsr resolveObjPtr

  ldy #\pos
  lda (tmpPointer),y

  #stack_push_int_a
.endm

ldfld16 .macro pos 
  #stack_pull_int_x
  jsr resolveObjPtr

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

  jsr resolveObjPtr

  lda (tmpPointer),y

  #stack_push_int_a
.endm

ldelem .macro   
  #stack_pull_int_y
  #stack_pull_int_x

  jsr resolveObjPtr

  lda (tmpPointer),y

  #stack_push_int_a
.endm

ldelem16 .macro   
  #stack_pull_int_a
  asl
  tay
  #stack_pull_int_x

  jsr resolveObjPtr

  iny
  lda (tmpPointer),y
  #stack_push_int_a
  dey
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

; --- float (5-byte MFLPT) fields/elements -- asm/helper/float.asm's
; zp_flt_a is reused here as the value-in-transit buffer (there's no
; equivalent of zp_field_value_low/high wide enough for 5 bytes, and
; nothing else needing zp_flt_a is live across these). zp_flt_b is reused
; as one-off scratch for the index*5 byte-offset computation in
; stelemflt/ldelemflt/newArrflt below -- see zeropage.asm's entry for both.

newArrflt .macro
  #stack_pull_int_a   ; size (element count)
  sta zp_flt_b
  asl
  asl
  clc
  adc zp_flt_b         ; A = size*5 (total bytes)
  ldy #0                ; reference fields
  jsr newObjL

  #stack_push_int_a
.endm

stfldflt .macro pos
  #stack_pull_mflpt zp_flt_a
  #stack_pull_int_x

  jsr resolveObjPtr

  ldy #\pos
  lda zp_flt_a
  sta (tmpPointer),y
  ldy #\pos+1
  lda zp_flt_a+1
  sta (tmpPointer),y
  ldy #\pos+2
  lda zp_flt_a+2
  sta (tmpPointer),y
  ldy #\pos+3
  lda zp_flt_a+3
  sta (tmpPointer),y
  ldy #\pos+4
  lda zp_flt_a+4
  sta (tmpPointer),y
.endm

ldfldflt .macro pos
  #stack_pull_int_x
  jsr resolveObjPtr

  ldy #\pos
  lda (tmpPointer),y
  sta zp_flt_a
  ldy #\pos+1
  lda (tmpPointer),y
  sta zp_flt_a+1
  ldy #\pos+2
  lda (tmpPointer),y
  sta zp_flt_a+2
  ldy #\pos+3
  lda (tmpPointer),y
  sta zp_flt_a+3
  ldy #\pos+4
  lda (tmpPointer),y
  sta zp_flt_a+4

  #stack_push_var_mflpt zp_flt_a
.endm

stelemflt .macro
  #stack_pull_mflpt zp_flt_a   ; value
  #stack_pull_int_a             ; index
  sta zp_flt_b
  asl
  asl
  clc
  adc zp_flt_b                  ; A = index*5 (byte offset)
  tay
  #stack_pull_int_x             ; object reference to array

  jsr resolveObjPtr

  lda zp_flt_a
  sta (tmpPointer),y
  iny
  lda zp_flt_a+1
  sta (tmpPointer),y
  iny
  lda zp_flt_a+2
  sta (tmpPointer),y
  iny
  lda zp_flt_a+3
  sta (tmpPointer),y
  iny
  lda zp_flt_a+4
  sta (tmpPointer),y
.endm

ldelemflt .macro
  #stack_pull_int_a             ; index
  sta zp_flt_b
  asl
  asl
  clc
  adc zp_flt_b                  ; A = index*5 (byte offset)
  tay
  #stack_pull_int_x             ; object reference to array

  jsr resolveObjPtr

  lda (tmpPointer),y
  sta zp_flt_a
  iny
  lda (tmpPointer),y
  sta zp_flt_a+1
  iny
  lda (tmpPointer),y
  sta zp_flt_a+2
  iny
  lda (tmpPointer),y
  sta zp_flt_a+3
  iny
  lda (tmpPointer),y
  sta zp_flt_a+4

  #stack_push_var_mflpt zp_flt_a
.endm

; objTableSize holds total BYTE count (max 255) -- element count is that
; divided by 5, which unlike the 8/16-bit cases isn't a shift, so this is
; an actual (small, bounded to <=51 iterations) division loop.
ldlenflt .macro
  #stack_pull_int_x
  lda objTableSize,x
  ldx #0
-   cmp #5
    bcc +
    sbc #5
    inx
    jmp -
+   #stack_push_int_x
.endm