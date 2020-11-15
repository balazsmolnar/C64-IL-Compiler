!src ".\\asm\\helper\\loader.asm"
!src ".\\asm\\helper\\stack.asm"
!src ".\\asm\\helper\\arithmetic.asm"

+start_at $1000
+stack_init .stack

!src ".\\asm\\system.asm"

!src ".\\asm\\generated.asm"
jsr Program_Main
rts
.stack