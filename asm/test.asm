!src ".\\asm\\helper\\loader.asm"
!src ".\\asm\\helper\\stack.asm"
!src ".\\asm\\helper\\branch.asm"

+start_at $1000
+stack_init .stack
jmp x

!src ".\\asm\\helper\\arithmetic.asm"
x
        nop ;NOP
        +stack_push_int 0 ;Push: 0
        +stack_pull_int .var0 ;Pop: Var 0
        jmp l1
        nop
l0      +stack_push_pointer .string
        jsr Console_WriteLine
        nop
        nop     
        +stack_push_var .var0
        +stack_push_int 1

        +add16

        +stack_pull_int .var0

l1      +stack_push_var .var0
        +stack_push_int 10
        +compareLess16

        +stack_pull_int .var1
        +stack_push_var .var1
        +stack_pull_int .var1

        ; lda #1
        ; CMP .var1
        +branch_true l0
        rts
 
.var0 !byte 0,0
.var1 !byte 0,0

!src ".\\asm\\system.asm"

.string		!pet "hello world 2", 0
.stack
