!src ".\\asm\\helper\\loader.asm"
!src ".\\asm\\helper\\stack.asm"
!src ".\\asm\\helper\\heap.asm"
!src ".\\asm\\helper\\arithmetic.asm"
!src ".\\asm\\helper\\branch.asm"

!src ".\\asm\\helper\\8bit.asm"

+start_at $1000
+initHeap heap

;+stack_init .stack

jsr Program_Main
rts

!src ".\\asm\\system.asm"
!src ".\\asm\\GC.asm"
!src ".\\asm\\helper\\object.asm"
!src ".\\asm\\C64.asm"

!src ".\\asm\\spritedata.asm"

!src ".\\asm\\generated\\generated.asm"



objTableLow
!fill 256, 0
objTableHigh
!fill 256, 0
objTableSize
!fill 256, 0
objTableReferences
!fill 256, 0
objTableRootCount
!fill 256, 0

heap

;.stack 