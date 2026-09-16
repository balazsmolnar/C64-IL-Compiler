VCR2            = $D016 ;VIC Control Register 2

; zp_param0_low  - Position low
; zp_param0_high - Position high
; zp_param1_low  - X
; zp_param2_low  - Y
; zp_param3_low  - char
; zp_param4_low  - Color
; (see asm/helper/zeropage.asm for every other claimant of these bytes)

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

C64_SetChar
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_param4_low
    #stack_pull_int zp_param3_low
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param1_low

    jmp C64_SetChar_Core

C64_GetChar
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param1_low
    jsr C64_GetChar_Core
    #stack_push_var zp_param3_low
    #stack_return_to_saved_address zp_tmp1_low

C64_Write
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

C64_SetBorderColor
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta $D020
    #stack_return_to_saved_address zp_tmp1_low

C64_SetBackgroundColor
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta $D021
    #stack_return_to_saved_address zp_tmp1_low

C64_GetBorderColor
    #stack_save_return_adress zp_tmp1_low
    #stack_push_var $D020

    #stack_return_to_saved_address zp_tmp1_low

C64_SetCharSet
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

C64_GetMemory
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_pointer zp_param4_low
    lda (zp_param4_low),y
    stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low

C64_SetMultiColor
    #stack_save_return_adress zp_tmp1_low

    lda VCR2
    ora #%00010000          ; multi-colour mode on
    sta VCR2   
    #stack_return_to_saved_address zp_tmp1_low

C64_SetCharBackgroundColor
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    #stack_pull_int_y
    txa
    sta $D022, y
    #stack_return_to_saved_address zp_tmp1_low

; C64.Interrupt (C64Lib/C64.cs) is a parameterless delegate (InterruptHandler,
; not EventHandler -- see that file's comment), and single-subscriber only:
; a second `+=` just overwrites zp_interrupt_address_low/high, same as a
; second call here would. The value arriving here is a bare 2-byte method
; pointer (Compiler/Operands/OperandBase.cs's OpNewObj delegate special
; case + #stack_construct_static_delegate in asm/helper/stack.asm already
; unwrapped `ldnull; ldftn M; newobj ...` down to just that pointer -- no
; heap object, no sender/EventArgs to receive).
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

C64_get_Screen:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low

; $1B is this program's untouched KERNAL boot default for $d011 (25-row
; text mode, DEN=1, YSCROLL=3) -- confirmed nothing else in this codebase
; ever writes it, so it's safe to hardcode as "the" resting value rather
; than read-modify-write around just the DEN bit. $0B is the same value
; with DEN (bit 4) cleared, blanking the whole physical display.
Screen_BeginUpdate:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    lda #$0B
    sta $d011
    #stack_return_to_saved_address zp_tmp1_low

Screen_EndUpdate:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    lda #$1B
    sta $d011
    #stack_return_to_saved_address zp_tmp1_low

.include "./c64sprite.asm"
.if USE_JOYSTICK
.include "./c64Joystick.asm"
.endif
.if USE_SOUND
.include "./C64Sound.asm"
.endif
.include "./c64Keys.asm"
.if USE_DEBUG
.include "./C64Debug.asm"
.endif
