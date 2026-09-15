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
.include "./helper/floatBanking.asm"

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

    ; Static constructors -- see ProgramEntry.asm's fuller rationale.
{{STATIC_CTORS}}
    lda #$FF             ; $FF to result byte (failed)
    sta result
    pha
    lda #0               ; push this
    #stack_push_int_a
    ; 4 bytes of padding, unrelated to and never touched by the called
    ; method's own parameter/local addressing (that's entirely relative to
    ; where its own declared parameters start, not to anything pushed
    ; before "this") -- exists purely so endtest's return-value capture
    ; below can always safely pull 5 bytes (float's width) regardless of
    ; the actual test method's return type, without ever underflowing the
    ; stack on a narrow-return, few/no-parameter test. Narrower return
    ; types just end up with real data in the first 1-2 pulled bytes and
    ; harmless padding in the rest, exactly like the pre-existing "always
    ; pull 2 bytes even for a 1-byte return" behavior already relied on
    ; below.
    #stack_push_int_a
    #stack_push_int_a
    #stack_push_int_a
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
    ; Always pulls 5 bytes (float's width) regardless of the actual return
    ; type -- see the padding comment above for why this never underflows.
    ; RunInEmulatorAspect.CopyResultFromEmulator only ever reads however
    ; many of these 5 bytes its own reflected return type actually needs.
    #stack_pull_int_a
    sta zp_tmp5          ; copy method return value
    #stack_pull_int_a
    sta zp_tmp5+1        ; copy method return value
    #stack_pull_int_a
    sta zp_tmp5+2
    #stack_pull_int_a
    sta zp_tmp5+3
    #stack_pull_int_a
    sta zp_tmp5+4

    brk

; result aliases zp_tmp1_low ($20, see asm/helper/zeropage.asm) -- every
; C64Lib macro the test method under execution calls transiently scribbles
; over $20 as its own return-address save slot, so this byte's value is
; garbage for nearly the entire test run. Safe only because both exit
; paths write their own authoritative final value into it as their very
; last action before halting the emulator (endtest's "lda #$00 / sta
; result" below, and Assert_Fail's "lda #$FF / sta result" right before
; its own brk) -- RunInEmulatorAspect.CopyResultFromEmulator only ever
; reads this byte after the emulator has already halted, so whatever
; happened to it in between never matters.
result = $20

; Real subroutines (not macros), so must come after "* = $1000" above --
; see ProgramEntry.asm's identical include for the full reasoning
; (asm/helper/floatBanking.asm has the underlying "why").
.include "./helper/float.asm"

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
