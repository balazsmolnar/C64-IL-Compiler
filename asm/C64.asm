VCR2            = $D016 ;VIC Control Register 2

; zp_param0_low  - Position low
; zp_param0_high - Position high
; zp_param1_low  - X
; zp_param2_low  - Y
; zp_param3_low  - char
; zp_param4_low  - Color
; (see asm/helper/zeropage.asm for every other claimant of these bytes)

; C64_Set_Screen_Ptr/C64_SetChar_Core/C64_GetChar_Core are asm-internal-only
; helpers -- never a direct C# call target, only reached via jsr/jmp from
; Screen_SetChar/Screen_GetChar/Screen_Write below, so ILLibraryUsagePass's
; Call/Callvirt scan can never see them directly. Rather than giving each of
; these its own Flag_ and wiring it up from a lookup table on the C# side
; (which a future asm-only edit could silently forget to update), each one's
; .if condition just directly references its caller(s)' own flag(s) --
; the dependency lives here, next to the code that has it, and needs no
; separate bookkeeping to stay correct.
.if Flag_Screen_SetChar | Flag_Screen_GetChar | Flag_Screen_Write

C64_Set_Screen_Ptr

    ; add Y*40
    lda zp_param2_low
    asl
    asl
    adc zp_param2_low
    asl
    asl
    bcc +
    inc zp_param0_high
    inc zp_param0_high
+   asl
    bcc +
    inc zp_param0_high
+   sta zp_param0_low
    ; +X
    lda zp_param1_low
    clc
    adc zp_param0_low
    bcc +
    inc zp_param0_high
+   sta zp_param0_low
    rts
.endif

.if Flag_Screen_SetChar

C64_SetChar_Core

    ; Init
    lda #$00
    sta zp_param0_low
    lda #$d8
    sta zp_param0_high

    jsr C64_Set_Screen_Ptr
    ldy #0
    lda zp_param4_low
    sta (zp_param0_low),y
    lda zp_param0_high
    sec
    sbc #$d4
    sta zp_param0_high
    lda zp_param3_low
    cmp #$ff
    beq +
    sta (zp_param0_low),y
+   #stack_return_to_saved_address zp_tmp1_low
.endif

.if Flag_Screen_GetChar

C64_GetChar_Core

    ; Init
    lda #$00
    sta zp_param0_low
    lda #$04
    sta zp_param0_high

    jsr C64_Set_Screen_Ptr
    ldy #0
    lda (zp_param0_low),y
    sta zp_param3_low
    rts
.endif

.weak
Flag_Screen_SetChar = 0
.endweak
.if Flag_Screen_SetChar

Screen_SetChar
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_param4_low
    #stack_pull_int zp_param3_low
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param1_low

    jmp C64_SetChar_Core
.endif

.weak
Flag_Screen_GetChar = 0
.endweak
.if Flag_Screen_GetChar

Screen_GetChar
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param1_low
    jsr C64_GetChar_Core
    #stack_push_var zp_param3_low
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Screen_Write = 0
.endweak
.if Flag_Screen_Write

Screen_Write
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_param4_low
    #stack_pull_pointer zp_param3_low
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param1_low

    lda #0
    sta zp_param0_low
    lda #4
    sta zp_param0_high
    jsr C64_Set_Screen_Ptr

    lda zp_param0_high
    clc
    adc #$d4
    sta zp_param1_high
    lda zp_param0_low
    sta zp_param1_low

    ldy #0
-   lda (zp_param3_low),Y
    beq +
    sta (zp_param0_low),Y
    lda zp_param4_low
    sta (zp_param1_low),Y
    iny
    bne -
+
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Screen_SetBorderColor = 0
.endweak
.if Flag_Screen_SetBorderColor

Screen_SetBorderColor
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta $D020
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Screen_SetBackgroundColor = 0
.endweak
.if Flag_Screen_SetBackgroundColor

Screen_SetBackgroundColor
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta $D021
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Screen_GetBorderColor = 0
.endweak
.if Flag_Screen_GetBorderColor

Screen_GetBorderColor
    #stack_save_return_adress zp_tmp1_low
    #stack_push_var $D020

    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Screen_SetCharSet = 0
.endweak
.if Flag_Screen_SetCharSet

Screen_SetCharSet
    #stack_save_return_adress zp_tmp1_low
    lda $D018
    and #%11110001
    sta $D018
    #stack_pull_int_a
    #stack_pull_int_a

    lsr
    lsr
    ora $D018
    sta $D018
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_C64_CopyMemory = 0
.endweak
.if Flag_C64_CopyMemory

C64_CopyMemory
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_pointer zp_param3_low
    #stack_pull_pointer zp_param4_low
-   lda (zp_param3_low),y
    sta (zp_param4_low),y
    dey
    bne -

    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_C64_FillMemory = 0
.endweak
.if Flag_C64_FillMemory

C64_FillMemory
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x
    #stack_pull_pointer zp_param4_low
    txa
-
    sta (zp_param4_low),y
    dey
    bne -

    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_C64_GetMemory = 0
.endweak
.if Flag_C64_GetMemory

C64_GetMemory
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_pointer zp_param4_low
    lda (zp_param4_low),y
    stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_C64_Random = 0
.endweak
.if Flag_C64_Random

; 8-bit maximal-length Galois LFSR (period 255, every nonzero byte value
; visited exactly once per cycle) -- $1D is the tap byte for THIS exact
; left-shift-then-conditionally-EOR formulation, found by brute-force
; search over all 255 candidate taps rather than trusted from a
; remembered/textbook value (an initial guess of $B8, plausible-looking
; and cited in some LFSR references, turned out NOT to be maximal-length
; for this specific left-shift construction -- Test/RandomTests.cs's
; FullPeriod_NoRepeatsNoZero caught it immediately). Verified empirically:
; 255 consecutive calls from the fixed seed below visit every value 1..255
; exactly once and never produce 0.
; c64_rng_state needs real backing storage with a genuine nonzero initial
; value (not objectTables.asm's zero-cost .virtual scratch buffers, which
; exist purely to claim an address with no real data) -- an ordinary
; labeled byte, costing 1 real byte in the assembled program same as any
; other small piece of static data here. A zero seed would make the LFSR
; degenerate (0 forever), so the (already non-zero) constant below matters;
; never reseed this with a runtime-computed 0.
c64_rng_state .byte $A5

C64_Random
    #stack_save_return_adress zp_tmp1_low
    lda c64_rng_state
    asl
    bcc +
    eor #$1D
+   sta c64_rng_state
    stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_C64_Delay = 0
.endweak
.if Flag_C64_Delay

; The jiffy tick (c64_delay_last_lo/hi) Delay() returned at -- NOT a
; computed future target. Each call just measures real ticks elapsed
; since this fixed, already-past point and waits for at least s50 of
; them, then stores the real "now" back here for next time -- no
; accumulating schedule, so a call that overshoots (other code ran long
; between Delay calls) never leaves anything to "catch up" on afterward.
;
; Seeded from the REAL jiffy clock on the first call ever (c64_delay_init
; below), not a fixed 0: a real program always does some real work (title
; screen, setup) before its first Delay call, so by then the jiffy clock
; is already well past 0 -- confirmed as a real, reproduced bug, not
; theorized: Hunchback's enemy ball ran with no pacing at all right after
; a level start, then suddenly correct. A fixed-0 start, even with this
; same elapsed-since-last design, would still make that very first call
; return immediately (elapsed looks huge) -- seeding avoids even that.
;
; 16-bit (the jiffy clock's middle and low bytes, $A1:$A2), not just the
; low byte: C64.Delay is ONE shared timer across every call site in the
; whole program (the ball's own pacing, a level-transition fade loop,
; title-screen pacing, ...), so "how long ago did the clock last get
; stored" can easily exceed several seconds -- e.g. a 30-iteration fade
; loop elsewhere calling Delay(8) runs for ~4.8s on its own. With only
; the 8-bit low byte, elapsed wraps every ~5.1s, so a stale last from a
; different call site could occasionally read back smaller than the true
; gap, delaying "already passed" detection by up to s50 extra ticks
; (confirmed as a real source of felt non-determinism, not just a
; theoretical concern -- the ball's first wait after a level transition
; varied depending on exactly where some unrelated pacing loop had left
; the shared clock). 16 bits (65536 ticks, ~21 minutes) makes elapsed
; exact for any realistic real-world gap between call sites, removing
; that residual coupling entirely rather than merely bounding it.
c64_delay_init .byte 0
c64_delay_last_lo .byte 0
c64_delay_last_hi .byte 0
c64_delay_s50 .byte 0

C64_Delay
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x              ; X = s50
    stx c64_delay_s50

    lda c64_delay_init
    bne +
    lda $a2                      ; KERNAL jiffy clock low byte -- alive
                                  ; since this codebase never issues a
                                  ; blanket SEI (see zeropage.asm's own
                                  ; comment on $90-$ff)
    sta c64_delay_last_lo
    lda $a1                      ; jiffy clock middle byte
    sta c64_delay_last_hi
    lda #1
    sta c64_delay_init
+

-   lda $a2
    sec
    sbc c64_delay_last_lo
    tay                           ; stash elapsed's low byte
    lda $a1
    sbc c64_delay_last_hi         ; elapsed's high byte (16-bit subtract, borrow propagated)
    bne +                         ; high byte != 0 -> elapsed >= 256, always >= any s50 (max 255) -> done
    tya
    cmp c64_delay_s50
    bcc -                         ; high byte == 0 and low byte < s50 -> not yet, keep polling
+
    lda $a2
    sta c64_delay_last_lo         ; last = now, for next call
    lda $a1
    sta c64_delay_last_hi

    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Screen_SetMultiColor = 0
.endweak
.if Flag_Screen_SetMultiColor

Screen_SetMultiColor
    #stack_save_return_adress zp_tmp1_low

    lda VCR2
    ora #%00010000          ; multi-colour mode on
    sta VCR2
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Screen_SetCharBackgroundColor = 0
.endweak
.if Flag_Screen_SetCharBackgroundColor

Screen_SetCharBackgroundColor
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    #stack_pull_int_y
    txa
    sta $D022, y
    #stack_return_to_saved_address zp_tmp1_low
.endif

; C64.Interrupt (C64Lib/C64.cs) is a parameterless delegate (InterruptHandler,
; not EventHandler -- see that file's comment), and single-subscriber only:
; a second `+=` just overwrites zp_interrupt_address_low/high, same as a
; second call here would. The value arriving here is a bare 2-byte method
; pointer (Compiler/Operands/OperandBase.cs's OpNewObj delegate special
; case + #stack_construct_static_delegate in asm/helper/stack.asm already
; unwrapped `ldnull; ldftn M; newobj ...` down to just that pointer -- no
; heap object, no sender/EventArgs to receive).
.weak
Flag_C64_add_Interrupt = 0
.endweak
.if Flag_C64_add_Interrupt

C64_add_Interrupt
    sei
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_pointer zp_interrupt_address_low
    lda $0314
    sta zp_interrupt_saved_low
    lda $0315
    sta zp_interrupt_saved_high

    lda #< OnInterrupt
    sta $0314
    lda #> OnInterrupt
    sta $0315
    cli
    #stack_return_to_saved_address zp_tmp1_low
.endif

; Dispatches into the subscriber's compiled method -- a single, fixed
; subroutine (never a macro expanded per callsite), so this is the one
; place interrupt-time zero-page safety needs solving, instead of every
; macro in the codebase defending itself individually (the previous
; approach, still visible in git history as SEI/CLI wrapping every
; asm/helper/float.asm and branch.asm macro body -- removed once this
; existed, since it was both narrower than the real problem and actively
; wrong: a macro's own `cli` would have prematurely re-enabled interrupts
; while still inside this handler if that macro were ever called from a
; subscriber's own code).
;
; Saves $01 (the ROM banking register) and the whole transient zero-page
; scratch range before running the subscriber's code, restores it after,
; so the subscriber can safely call into arithmetic/heap/GC/C64Lib/float
; code without corrupting whatever the interrupted mainline code had
; in-flight in that same scratch. See zeropage.asm's
; zp_interrupt_save_start/zp_interrupt_save_len for exactly what's
; covered and why stackPointer/heapPointer are deliberately excluded.
; $01 specifically: if mainline was mid-float-op with BASIC ROM banked in
; (asm/helper/floatBanking.asm's bank_in_basic_rom, $01=$07) when
; interrupted, and the subscriber's own code also does a float op, its
; bank_out_basic_rom would otherwise bank ROM back OUT ($01=$06) while
; the interrupted code is still mid-Float_Add, expecting $01 to still be
; $07 for its own subsequent MOVMF read.
;
; Asm-internal-only (see the note above C64_Set_Screen_Ptr) -- only ever
; reached by C64_add_Interrupt poking its address into the IRQ vector, so
; it just reuses that single caller's own flag directly instead of getting
; one of its own.
.if Flag_C64_add_Interrupt

OnInterrupt
    lda $01
    pha
    ldx #0
-   lda zp_interrupt_save_start,x
    pha
    inx
    cpx #zp_interrupt_save_len
    bne -
    lda zp_ctor_result
    pha
    lda zp_field_value_low
    pha
    lda zp_field_value_high
    pha

    lda #> On_Interrupt_Ret-1
    pha
    lda #< On_Interrupt_Ret-1
    pha
    jmp (zp_interrupt_address_low)
On_Interrupt_Ret
    pla
    sta zp_field_value_high
    pla
    sta zp_field_value_low
    pla
    sta zp_ctor_result
    ldx #zp_interrupt_save_len-1
-   pla
    sta zp_interrupt_save_start,x
    dex
    bpl -
    pla
    sta $01
    jmp (zp_interrupt_saved_low)
    rti
.endif

; Read-modify-write, touching only DEN (bit 4) -- NOT a flat "$d011 = $0B/
; $1B" overwrite like this pair used to be (that was safe only as long as
; nothing else in this codebase ever wrote $d011; asm/C64Graphics.asm's
; Screen_EnableBitmapMode now sets bit 5, BMM, which a flat overwrite here
; would silently clear back off). Preserves BMM/ECM/RSEL/YSCROLL either way.
.weak
Flag_Screen_BeginUpdate = 0
.endweak
.if Flag_Screen_BeginUpdate

Screen_BeginUpdate:
    #stack_save_return_adress zp_tmp1_low
    lda $d011
    and #%11101111
    sta $d011
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Screen_EndUpdate = 0
.endweak
.if Flag_Screen_EndUpdate

Screen_EndUpdate:
    #stack_save_return_adress zp_tmp1_low
    lda $d011
    ora #%00010000
    sta $d011
    #stack_return_to_saved_address zp_tmp1_low
.endif

.include "./c64sprite.asm"
.include "./c64Joystick.asm"
.include "./C64Sound.asm"
.include "./c64Keys.asm"
.include "./C64Debug.asm"
.include "./C64Graphics.asm"
