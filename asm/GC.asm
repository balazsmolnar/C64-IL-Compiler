

; SEI/PLP (not SEI/CLI) around the whole collection -- zero-page save/
; restore (asm/C64.asm's OnInterrupt) protects scratch *values*, not the
; *consistency of heap contents mid-mutation*: sweepAndCompact physically
; moves objects, and an interrupt landing mid-move whose handler triggers
; another allocation or another GC.Collect() would walk/move objects
; through a half-moved heap regardless of zero-page protection. PLP (not
; a blind CLI) so this nests correctly if GC.Collect() is ever called from
; inside a compiled interrupt handler -- see floatBanking.asm's
; bank_in_basic_rom/bank_out_basic_rom for the identical reasoning.
GC_Collect
    php
    sei
    jsr trackObjects
    jsr fillSortTable
    jsr quicksort
    jsr sweepAndCompact
    plp
    rts

; finds all objects which can be reached from roots
; Input: None
; Output: None
; Sets the high bit of root table if the object can be tracked
trackObjects

    ; push 0 to stack, signal we reached the end of the algorithm
    lda #0
    pha

    lda #$80
    sta objTableRootCount    ; mark NULL, don't need to track anymore

    ; set initial track bit. 1 for roots, 0 for others. Push roots to the stack
    ldx #1
-   lda objTableRootCount, X
    and #$7f                 ; mask out track bit
    beq +                    ; jump if root count is 0 (not a root)
    ora #$80                 ; we found a root, set track bit
    tay
    txa
    pha                      ; push root to stack
    tya
+   sta objTableRootCount, X
    inx
    bne -                    ; repeat 0-255

    ; pull object from stack
    ; mark the track bit of all references
l2  pla
    beq endtrack                  ; end algorithm 
    tax
    lda objTableLow,X
    sta zp_gc_track_ptr_low
    lda objTableHigh,X
    sta zp_gc_track_ptr_high
    lda objTableReferences,X
    sta zp_gc_track_refcount

    ldy #0
-   cpy zp_gc_track_refcount
    beq l2                        ; no refences, end this phase
    lda (zp_gc_track_ptr_low), y
    tax
    lda objTableRootCount, x      ; check if track bit already set for the reference (N flag = bit 7)
    bmi +                         ; it is already set, nothing to do --> continue
    ora #$80                      ; set track bit
    sta objTableRootCount, x
    txa
    pha                           ; push to stack
+   iny                           ; next reference
    bne -
endtrack
    rts

;
; Creates a temp table with active objects at the end to the heap
; To be able to sort
; Y: Returns Size of the Table
;

fillSortTable:
    ldx #0
    ldy #0
-   inx                             ; 0th element (null) can be skipped
    beq +                           ; X wrapped: scanned all 255 slots, done
    lda objTableHigh,X              ; if the high part of the pointer is 0 the object is not used
    beq -                           ; get next object
    txa
    sta (heapPointer),Y
    iny
    bne -
+   lda #0                          ; put a trailing 0, found all active objects
    sta (heapPointer),Y             ; (must run on every exit, not just the Y-wrap case --
    dey                             ;  Y can't realistically wrap since there are at most 255
    rts                             ;  live slots, so this was previously unreachable)

; LOW_INDEX/HIGH_INDEX/PARTITION_LOW_INDEX/PARTITION_HIGH_INDEX/PIVOT_LOW/
; PIVOT_HIGH are defined in asm/helper/zeropage.asm, alongside every other
; zero-page claim in the codebase -- see that file for why these bytes are
; safe to share with the scratch-parameter bank used elsewhere.
;
; Sorts the sort table based on the pointer on the heap
;
quicksort:

    cpy #0
    beq end
    ; push end signal to stack
    lda #0
    pha
    ; low index to stack
    pha
    ; high index to stack
    tya
    pha

start:                 ; start main loop
    pla
    beq end            ; reached end signal
    sta HIGH_INDEX
    pla
    sta LOW_INDEX
    jsr partition       ; partition, partition index in X
    ; if parition_index > low_index
    ; push (low_index, partition_index)
    cpx LOW_INDEX
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
    beq start
    cmp HIGH_INDEX
    beq start
    pha
    lda HIGH_INDEX
    pha
    jmp start
end
    rts

partition:
    ; select pivot index (low+high)/2
    lda LOW_INDEX
    clc
    adc HIGH_INDEX
    ror
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
l4

    inc PARTITION_LOW_INDEX
    ldy PARTITION_LOW_INDEX
    jsr compare
    bcc l4

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
    jmp l4
+
    ldx PARTITION_HIGH_INDEX
    rts

compare
    lda (heapPointer),y
    tay
    lda objTableHigh,y
    cmp PIVOT_HIGH
    bne +
    lda objTableLow,y
    cmp PIVOT_LOW
+
    rts

; HEAP_POINTER_COMPACTED_LOW/HIGH, HEAP_POINTER_ORIG_LOW/HIGH, and TMP are
; also defined in asm/helper/zeropage.asm -- see that file for why
; HEAP_POINTER_ORIG must never alias HEAP_POINTER_COMPACTED (the bug this
; fixed) and for the full sharing picture across the rest of the codebase.

sweepAndCompact

    lda #<heap
    sta HEAP_POINTER_COMPACTED_LOW
    lda #>heap
    sta HEAP_POINTER_COMPACTED_HIGH
    ldy #0

loop:
    lda (heapPointer),y
    beq sweepend                        ; trailing zero found
    tax
    lda objTableRootCount, X
    bmi keep
    lda #0                              ; object can be deleted
    sta objTableRootCount, X            ; delete object ids
    sta objTableLow,X
    sta objTableHigh,x
    sta objTableSize,x
    beq tail                            ; A still 0 from lda #0 (sta leaves flags untouched) --> always taken
keep
    lda objTableHigh, X                 ; keep object, but copy data, move pointer
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
    beq l1
    sta TMP
    ldy #$00
-   lda (HEAP_POINTER_ORIG_LOW), y
    sta (HEAP_POINTER_COMPACTED_LOW),y
    iny
    cpy TMP
    bne -
l1  lda objTableSize, X
    clc
    adc HEAP_POINTER_COMPACTED_LOW
    sta HEAP_POINTER_COMPACTED_LOW
    bcc +
    inc HEAP_POINTER_COMPACTED_HIGH
+   pla
    tay
tail
    iny
    jmp loop
sweepend
    lda HEAP_POINTER_COMPACTED_LOW
    sta heapPointer
    lda HEAP_POINTER_COMPACTED_HIGH
    sta heapPointer+1
    rts
