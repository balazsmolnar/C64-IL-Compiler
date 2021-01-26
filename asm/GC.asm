

GC_Collect:
    jsr trackObjects
    jsr fillSortTable
    jsr quicksort
    jsr sweepAndCompact
    rts

; finds all objects which can be reached from roots
; Input: None
; Output: None
; Sets the high bit of root table if the object can be tracked
trackObjects:

    ; push 0 to stack, if we reach the algorithm can stop
    lda #0
    pha

    lda #$80
    sta objTableRootCount   ; mark NULL, don't need to track anymore

    ; set initial track bit. 1 for roots, 0 for others. Push roots to the stack
    ldx #1
-   lda objTableRootCount, X
    and #$7f                 ; mask out track bit
    beq +
    ora #$80
    tay
    txa
    pha                     ; push root to stack
    tya
+   sta objTableRootCount, X
    inx
    bne -

--  pla
    beq ++                  ; end algorithm 
    tax
    lda objTableLow,X
    sta $33
    lda objTableHigh,X
    sta $34
    lda objTableReferences,X
    sta $35

    ldy #0
-   cpy $35
    beq --
    lda ($33), y
    tax
    lda objTableRootCount, x
    and #$80
    bne +
    lda objTableRootCount, x
    ora #$80
    sta objTableRootCount, x
    txa
    pha
+   iny
    bne -
++  rts

;
; Creates a table with active objects at the end to the heap
; To be able to sort
; Y: Returns Size of the Table
;

fillSortTable:
    ldx #0
    ldy #0
-   inx
    beq +
    lda objTableHigh,X
    beq -
    txa
    sta (heapPointer),Y
    iny
    bne -
    lda #0
    sta (heapPointer),Y
+   dey
    rts

LOW_INDEX = $34                ; Low index
HIGH_INDEX = $35               ; High index

PARTITION_LOW_INDEX = $36      ; partition low
PARTITION_HIGH_INDEX = $37     ; partition high
PIVOT_LOW = $39
PIVOT_HIGH = $3a
PIVOT_INDEX = $3b

;
; Sorts the sort table based on the pointer on the heap
;
quicksort:

    ; push end signal to stack
    lda #0
    pha
    ; low index to stack
    pha
    ; high index to stack
    tya
    pha

.start:                 ; start main loop
    pla
    beq .end            ; reached end signal
    sta HIGH_INDEX
    pla
    sta LOW_INDEX
    jsr partition       ; partition, partition index in X
    txa
    ; if parition_index > low_index 
    ; push (low_index, partition_index)
    cmp LOW_INDEX       
    beq +
    lda LOW_INDEX
    pha
    txa
    pha
+
    ; if parition_index+1 < high_index 
    ; push (partition_index+1, high_index)
    inx
    txa
    beq .start
    cmp HIGH_INDEX
    beq .start
    pha
    lda HIGH_INDEX
    pha
    jmp .start
.end
    rts

partition:
    ; select pivot index (low+high)/2
    lda LOW_INDEX
    clc
    adc HIGH_INDEX
    ror
    sta PIVOT_INDEX
    tay
    lda (heapPointer),y
    tax
    lda objTableLow, X
    sta PIVOT_LOW
    lda objTableHigh, X
    sta PIVOT_HIGH

    ldx LOW_INDEX
    dex
    stx PARTITION_LOW_INDEX
    ldx HIGH_INDEX
    inx
    stx PARTITION_HIGH_INDEX
--

    inc PARTITION_LOW_INDEX
    ldy PARTITION_LOW_INDEX
    jsr compare
    bcc --

-   
    dec PARTITION_HIGH_INDEX
    ldy PARTITION_HIGH_INDEX
    jsr compare
    beq +
    bcs -
+   ; swap items
    ldy PARTITION_LOW_INDEX
    cpy PARTITION_HIGH_INDEX
    bcs +
    lda (heapPointer),y
    tax
    ldy PARTITION_HIGH_INDEX
    lda (heapPointer),y
    ldy PARTITION_LOW_INDEX
    sta (heapPointer),y
    txa
    ldy PARTITION_HIGH_INDEX
    sta (heapPointer),y
    jmp --
+
    ldx PARTITION_HIGH_INDEX
    rts

compare:
    lda (heapPointer),y
    tay
    lda objTableHigh,y
    cmp PIVOT_HIGH
    bne +
    lda objTableLow,y
    cmp PIVOT_LOW
+
    rts

HEAP_POINTER_COMPACTED_LOW = $34                ; Pointer to the compacted heap
HEAP_POINTER_COMPACTED_HIGH = $35

HEAP_POINTER_ORIG_LOW = $34                     ; Pointer to the object being copied
HEAP_POINTER_ORIG_HIGH = $35                    
TMP = $36


sweepAndCompact:

    lda #<heap
    sta HEAP_POINTER_COMPACTED_LOW
    lda #>heap
    sta HEAP_POINTER_COMPACTED_HIGH
    ldy #0

.loop:
    lda (heapPointer),y
    beq .sweepend                       ; trailing zero found
    tax
    lda objTableRootCount, X
    and #$80                             
    bne +
    lda #0                              ; object can be deleted
    sta objTableRootCount, X            ; delete object ids
    sta objTableLow,X
    sta objTableHigh,x
    sta objTableSize,x
    iny
    jmp .loop
+   lda objTableHigh, X                 ; keep object, but copy data, move pointer
    sta HEAP_POINTER_ORIG_HIGH
    lda objTableLow, x
    sta HEAP_POINTER_ORIG_LOW
    lda HEAP_POINTER_COMPACTED_LOW     
    sta objTableLow, X
    lda HEAP_POINTER_COMPACTED_HIGH
    sta objTableHigh, x

    tya
    pha
    lda objTableSize, X
    beq ++
    sta TMP
    ldy #$00
-   lda (HEAP_POINTER_ORIG_LOW), y
    sta (HEAP_POINTER_COMPACTED_LOW),y
    iny
    cpy TMP
    bne -    
++  lda objTableSize, X
    clc
    adc HEAP_POINTER_COMPACTED_LOW
    sta HEAP_POINTER_COMPACTED_LOW
    bcc +
    inc HEAP_POINTER_COMPACTED_HIGH
+   pla
    tay
    iny
    jmp .loop
.sweepend
    lda HEAP_POINTER_COMPACTED_LOW
    sta heapPointer
    lda HEAP_POINTER_COMPACTED_HIGH
    sta heapPointer+1
    rts
