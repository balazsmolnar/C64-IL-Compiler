.enc "screen"

.include "./helper/loader.asm"
.include "./helper/stack.asm"
.include "./helper/localsStack.asm"
.include "./helper/heap.asm"
.include "./helper/arithmetic.asm"
.include "./helper/branch.asm"
.include "./helper/zp.asm"
.include "./helper/optimized.asm"

.include "./helper/8bit.asm"

#start_at $1000
#initHeap heap
    
#locals_stack_init

jsr Program_Main
rts

.include "./system.asm"
.include "./GC.asm"
.include "./helper/object.asm"
.include "./presentation/library_flags.asm"
.include "./C64.asm"

.include "./presentation/generated.asm"

; The object tables (2048 bytes total) used to always start at a fixed
; $c000, wasting whatever gap was left between the end of code and there.
; Let them float right after the code instead -- but only if there's
; still room for the full 2048-byte block before BASIC ROM at $a000
; ($9800 = $a000-$0800, i.e. exactly that much headroom). Landing there
; would silently corrupt memory, not just waste it: BASIC ROM is mapped
; read-only on real hardware unless banked out (which nothing here does),
; and SimpleEmulator.SetMemory hard-blocks writes to $a000-$c000/>$e000
; the same way. Falling back to the old fixed $c000 keeps this safe even
; once a program grows too large to compress.
.if * < $9800
.else
* = $c000
.endif
objTableLow
.fill 256, 0
objTableHigh
.fill 256, 0
objTableSize
.fill 256, 0
objTableReferences
.fill 256, 0
objTableRootCount
.fill 256, 0
objTableDescLow
.fill 256, 0
objTableDescHigh
.fill 256, 0

localsStack
.fill 256, 0 
heap

