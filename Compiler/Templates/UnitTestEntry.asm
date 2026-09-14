.include "./helper/loader.asm"
.include "./helper/stack.asm"
.include "./helper/localsStack.asm"
.include "./helper/heap.asm"
.include "./helper/arithmetic.asm"
.include "./helper/branch.asm"
.include "./helper/zp.asm"
.include "./helper/optimized.asm"
.include "./helper/memoryLayout.asm"
.include "./helper/banking.asm"

.include "./helper/8bit.asm"

start_address = $09FE

; Real RAM to $d000 (I/O) once BASIC ROM is banked out -- see
; ProgramEntry.asm's fuller rationale. Safe here too, and even more
; trivially: this harness has no BASIC stub at all (SimpleEmulator jumps
; straight to $1000, never runs BASIC's own boot code), so there's no
; "BASIC's job isn't done yet" window to worry about -- nothing in the
; test-execution path ever reads BASIC ROM. SimpleEmulator.SetMemory is
; bank-aware (tracks $01 writes), so this is exercised for real here,
; not just on VICE/hardware.
OBJ_TABLES_MAX_START = $c800
OBJ_TABLES_FALLBACK = $c800

* = $1000
Run_Test:
    #disable_basic_rom

    #locals_stack_init
    #initHeap heap

    lda #$FF             ; $FF to result byte (failed)
    sta result
    pha
    lda #0               ; push this
    #stack_push_int_a
    ldx #0
    cpx $09e0            ; get number of function parameters
    beq +
-   lda $09e1,x
    #stack_push_int_a    ; copy parameters to stack
    inx
    cpx $09e0
    bne -
+   lda #>endtest        ; push return address to stack
    pha
    lda #<endtest-1
    pha
    jmp (start_address)  ; call test method
endtest:
    lda #$00             ; 0 to status mem (succeeded)
    sta result
    #stack_pull_int_a
    sta zp_tmp5          ; copy method return value
    #stack_pull_int_a
    sta zp_tmp5+1        ; copy method return value

    brk

result = $20

.include "./system.asm"
.include "./GC.asm"
.include "./helper/object.asm"
.include "./{{FOLDER}}/library_flags.asm"
.include "./C64.asm"

.include "./{{FOLDER}}/generated.asm"

Assert_Fail:
    #stack_save_return_adress zp_tmp2_low
    #stack_pull_pointer zp_tmp4_low           ; copy message pointer
    lda #$FF
    sta result                                ; put FF to status mem (failed)
    brk

Assert_AreEqual:
    #stack_save_return_adress zp_tmp2_low
    #stack_pull_pointer zp_tmp4_low
    #stack_pull_int_a
    sta zp_tmp3
    #stack_pull_int_a
    cmp zp_tmp3
    beq +
    #stack_push_var16 zp_tmp4_low
    jsr Assert_Fail
+   #stack_return_to_saved_address zp_tmp2_low

Assert_IsTrue:
    #stack_save_return_adress zp_tmp2_low
    #stack_pull_pointer zp_tmp4_low
    #stack_pull_int_a
    cmp #0
    bne +
    #stack_push_var16 zp_tmp4_low
    jsr Assert_Fail
+   #stack_return_to_saved_address zp_tmp2_low

Assert_IsFalse:
    #stack_save_return_adress zp_tmp2_low
    #stack_pull_pointer zp_tmp4_low
    #stack_pull_int_a
    cmp #0
    beq +
    #stack_push_var16 zp_tmp4_low
    jsr Assert_Fail
+   #stack_return_to_saved_address zp_tmp2_low

.include "./helper/objectTables.asm"
