Program_Main 
    nop 
    +stack_push_int 0 
    +stack_pull_int .var0 
    jmp label_0
label_1:    nop 
    +stack_push_pointer .string_1879048193
    jsr Console_WriteLine
    nop 
    nop 
    +stack_push_var .var0 
    +stack_push_int 1 
    +add16 
    +stack_pull_int .var0 
label_0:    +stack_push_var .var0 
    +stack_push_int 10
    +compareLess16 
    +stack_pull_int .var1 
    +stack_push_var .var1 
    +branch_true label_1
    rts 
.var0 !byte 0,0
.var1 !byte 0,0
.string_1879048193 !pet "hello world",0
