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
; OBJ_TABLES_MAX_START/FALLBACK are the SAME value -- not two distinct
; thresholds (there's no real "float, else fall back to a different safe
; place" happening; see objectTables.asm's own comment on this, including
; what actually went wrong when Test/*.cs grew past the old $c800 value:
; silent runtime corruption of Assert_AreEqualString's own code, not a
; build failure -- objectTables.asm's .cerror check now converts any
; future recurrence into a loud assembly-time error instead).
;
; $d000, not $c800: tostring_buffer/stringops_buffer/heap all live in the
; SAME floating .virtual block as the tables themselves (objectTables.asm),
; not at any independently-fixed address -- so the real ceiling isn't
; "don't collide with tostring_buffer," it's "leave the runtime heap (the
; unbounded `heap` label at the very end of that block) enough room before
; $e000, KERNAL ROM, which stays mapped even with BASIC banked out (see
; asm/helper/floatBanking.asm's bank_out_basic_rom using $06, not a value
; that also clears HIRAM). At $d000: tables(2048)+tostring_buffer(16)+
; stringops_buffer(40) = 2104 bytes, leaving heap ~$d838-$e000 (~1992
; bytes) -- comfortably more than any single test's own allocations need,
; and 2048 bytes more code headroom than the old $c800 had.
OBJ_TABLES_MAX_START = $d000
OBJ_TABLES_FALLBACK = $d000

; Object allocation must stay below $e000 (KERNAL ROM, still mapped) -- see
; asm/helper/fault.asm's Runtime_CheckHeapRoom.
HEAP_LIMIT = $e000
RUNTIME_FAULT_UNITTEST = 1

* = $1000
Run_Test:
    #disable_basic_rom

    #locals_stack_init
    #initHeap heap

    ; BASIC ROM float errors (overflow, division by zero...) jump through
    ; ($0300); point it at our fault handler -- see asm/helper/fault.asm.
    lda #<Runtime_FloatError
    sta $0300
    lda #>Runtime_FloatError
    sta $0301

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
.include "./helper/division.asm"

.include "./system.asm"
.include "./helper/fault.asm"
.include "./GC.asm"
.include "./helper/object.asm"
.include "./{{FOLDER}}/library_flags.asm"

; Hi-res bitmap graphics reservation -- see ProgramEntry.asm's identical
; block for the full reasoning (memory layout, VIC bank/ROM-shadow
; constraints, why $0c00/$2000 specifically, why real bytes not .virtual).
; Double buffering (Screen.SetDrawBuffer/SwapBuffers) adds a second bitmap +
; color matrix in VIC bank 1, below; Screen.SetBitmapColors alone just needs
; the normal single buffer.
GRAPHICS_DOUBLE_BUFFER = Flag_Screen_SetDrawBuffer | Flag_Screen_SwapBuffers
GRAPHICS_USED = Flag_Screen_EnableBitmapMode | Flag_Screen_DisableBitmapMode | Flag_Screen_SetPixel | Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawCircle | Flag_Screen_SetBitmapColors | GRAPHICS_DOUBLE_BUFFER
.if GRAPHICS_USED
graphics_resume_point = *
* = $0c00
Graphics_ColorMatrix
.fill 1000
* = graphics_resume_point

* = $2000
Graphics_Bitmap
.fill 8000
.if GRAPHICS_DOUBLE_BUFFER
; Second buffer, in VIC bank 1 ($4000-$7fff; no character-ROM shadow there,
; and CIA2 $dd00 bits 0-1 switch to it -- see Screen_SwapBuffers in
; asm/C64Graphics.asm): the bitmap at bank offset $0000, its color matrix
; (1K, any 1K boundary) at offset $2000 right after it, and compiled code
; resuming at $6400 instead of $4000. Costs ~9 KB of address space, and only
; programs that actually double-buffer pay it. Sprite data pointers live at
; matrix+$3f8, so sprites don't survive a flip -- keep them hidden in bitmap
; mode, as with the single buffer.
* = $4000
Graphics_Bitmap2
.fill 8000
* = $6000
Graphics_ColorMatrix2
.fill 1000
* = $6400
.else
* = $4000
.endif
.endif

.include "./C64.asm"
.include "./helper/tostring.asm"
.include "./helper/stringops.asm"

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

; Byte-by-byte content comparison, unlike Assert_AreEqual's flat 1-byte
; cmp -- comparing the two pointers themselves would fail for almost any
; equal-content pair (a ToString() result and a string literal are
; virtually never the same address). zp_param0_low/zp_param1_low: the two
; string pointers -- safe to reuse here despite their broader claims
; elsewhere (asm/helper/zeropage.asm) since nothing else runs mid-assertion.
Assert_AreEqualString:
    #stack_save_return_adress zp_tmp2_low
    #stack_pull_pointer zp_tmp4_low           ; message pointer
    #stack_pull_pointer zp_param1_low         ; expected string pointer
    #stack_pull_pointer zp_param0_low         ; actual string pointer
    ldy #0
Assert_AreEqualString_Loop:
    lda (zp_param0_low),y
    cmp (zp_param1_low),y
    bne Assert_AreEqualString_Fail
    cmp #0
    beq Assert_AreEqualString_Pass
    iny
    jmp Assert_AreEqualString_Loop
Assert_AreEqualString_Fail:
    #stack_push_var16 zp_tmp4_low
    jsr Assert_Fail
Assert_AreEqualString_Pass:
    #stack_return_to_saved_address zp_tmp2_low

.include "./helper/objectTables.asm"
