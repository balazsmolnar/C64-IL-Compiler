!src ".\\asm\\helper\\loader.asm"
!src ".\\asm\\helper\\stack.asm"
!src ".\\asm\\helper\\arithmetic.asm"
!src ".\\asm\\helper\\branch.asm"

+start_at $1000
+stack_init .stack

jsr Program_Main
rts

!src ".\\asm\\generated.asm"

!src ".\\asm\\system.asm"
!src ".\\asm\\C64.asm"

.stack 