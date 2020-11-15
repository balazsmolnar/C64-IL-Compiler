!src ".\\asm\\helper\\loader.asm"
!src ".\\asm\\helper\\stack.asm"

+start_at $1000
+stack_init .stack
!src ".\\asm\\helper\\arithmetic.asm"
!src ".\\asm\\system.asm"

        nop ;NOP
        +stack_push_int 0 ;Push: 0
        +stack_pull_int .var0 ;Pop: Var 0
        jmp l1
        nop
l0      +stack_push_pointer .string
        jsr WriteLine
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
        bne+
        jmp l0
+       rts
 
.var0 !byte 0,0
.var1 !byte 0,0

.string		!pet "hello world", 0
.stack
