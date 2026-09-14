.enc "screen"

.include "./helper/zeropage.asm"
.include "./helper/loader.asm"
.include "./helper/stack.asm"
.include "./helper/localsStack.asm"
.include "./helper/heap.asm"
.include "./helper/arithmetic.asm"
.include "./helper/branch.asm"
.include "./helper/optimized.asm"
.include "./helper/memoryLayout.asm"
.include "./helper/banking.asm"

.include "./helper/8bit.asm"

#start_at $1000

; Banks out BASIC ROM -- safe here since the BASIC stub's SYS line has
; already done its one job of jumping into machine code, and gives
; objectTables.asm real RAM all the way to $d000 (I/O) instead of
; stopping at BASIC ROM ($a000). $c800 = $d000-$0800, exactly enough
; headroom for the full 2048-byte table block.
#disable_basic_rom
OBJ_TABLES_MAX_START = $c800
OBJ_TABLES_FALLBACK = $c800

#initHeap heap

#locals_stack_init

; Static constructors -- nothing else ever calls a type's .cctor, so any
; static readonly field with a real initializer needs this to run once
; before Program_Main touches it.
{{STATIC_CTORS}}
jsr Program_Main
rts

.include "./system.asm"
.include "./GC.asm"
.include "./helper/object.asm"
.include "./{{FOLDER}}/library_flags.asm"
.include "./C64.asm"

.include "./{{FOLDER}}/generated.asm"

.include "./helper/objectTables.asm"
