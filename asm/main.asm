.include "./helper/loader.asm"
.include "./helper/stack.asm"
.include "./helper/localsstack.asm"
.include "./helper/heap.asm"
.include "./helper/arithmetic.asm"
.include "./helper/branch.asm"
.include "./helper/zp.asm"

.include "./helper/8bit.asm"

#start_at $1000
#initHeap heap

#locals_stack_init

jsr Program_Main
rts

.include "./system.asm"
.include "./GC.asm"
.include "./helper/object.asm"
.include "./C64.asm"

.include "./spritedata.asm"

.include "./generated/generated.asm"



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

localsStack
.fill 256, 0 
heap

