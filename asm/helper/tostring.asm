; Decimal-text conversion for this compiler's 1-byte (uint/byte, int/sbyte)
; and 2-byte (ulong, long) integer types -- see Compiler/ILNumericToStringPass.cs
; for how a real x.ToString() call gets rewritten into a jsr to one of the
; four public routines below. float.ToString() lives in asm/helper/float.asm
; instead (it needs the BASIC ROM's FOUT, via the same bank_in_basic_rom
; pattern float.asm's other routines already use).
;
; Gated by the per-method dead-code-elimination mechanism (Compiler/
; ILLibraryFlagsPass.cs), same pattern as asm/C64.asm/asm/c64sprite.asm/etc
; -- ILNumericToStringPass registers whichever of the four labels it
; actually needs into UsedLibraryLabels itself (not a Call/Callvirt scan,
; since these are synthesized jsr's with no real MethodBase behind them).
; Confirmed actually excluding unused routines by checking Hunchback's own
; assembled labels (it never calls ToString()): none of this file's four
; routines appear, only float.asm's always-included NumberFormat_FloatToString
; does. .include'd AFTER library_flags.asm/C64.asm (see Compiler/Templates/
; ProgramEntry.asm/UnitTestEntry.asm), NOT alongside float.asm (which sits
; much earlier, before #start_at, for its own unrelated ROM-banking address
; requirement that these routines don't share) -- matches every other
; gated file's position, simplest to reason about even though the 64tass
; README describes .weak resolving from the fully-read source regardless
; of include order (never separately tested whether the earlier position
; would actually have worked too).
;
; All four routines share tostring_buffer (asm/helper/objectTables.asm) and
; the small NumberFormat_Digit16/NumberFormat_EmitDigitOrSuppress helpers
; below -- see objectTables.asm's comment on tostring_buffer for the
; shared-buffer tradeoff (simple, but a second ToString() call overwrites
; the first result, and it's not protected across an interrupt).

; X = digit value (0-9) for the current decimal place. Prints it (and
; permanently stops suppressing further digits) unless still in a
; leading-zero run (zp_param2_low == 0, "haven't printed anything yet")
; AND this digit is also zero -- e.g. for 1005, the hundreds/tens zeros in
; the middle still print once the leading thousands digit already has.
; Y = tostring_buffer write index (in/out, this compiler's own evaluation
; stack never touches it). Clobbers A.
.weak
Flag_NumberFormat_UInt8ToString = 0
Flag_NumberFormat_Int8ToString = 0
Flag_NumberFormat_UInt16ToString = 0
Flag_NumberFormat_Int16ToString = 0
.endweak
.if Flag_NumberFormat_UInt8ToString | Flag_NumberFormat_Int8ToString | Flag_NumberFormat_UInt16ToString | Flag_NumberFormat_Int16ToString

NumberFormat_EmitDigitOrSuppress:
    cpx #0
    bne NumberFormat_EmitDigit_Print
    lda zp_param2_low
    beq NumberFormat_EmitDigit_Skip
NumberFormat_EmitDigit_Print:
    txa
    ora #$30
    sta tostring_buffer,y
    iny
    lda #1
    sta zp_param2_low
NumberFormat_EmitDigit_Skip:
    rts
.endif

.if Flag_NumberFormat_UInt8ToString | Flag_NumberFormat_Int8ToString

; A = magnitude (0-255). Writes decimal digits into tostring_buffer
; starting at offset Y (Y is an in/out cursor -- 0 for an unsigned value,
; already past a '-' for a negative one), null-terminates, leaves Y past
; the terminator. Shared by NumberFormat_UInt8ToString/Int8ToString below.
; Clobbers A/X, zp_param0_low (remaining value), zp_param2_low (leading-
; zero-suppression flag, shared with NumberFormat_EmitDigitOrSuppress).
NumberFormat_WriteUInt8Digits:
    sta zp_param0_low
    lda #0
    sta zp_param2_low

    ldx #0
NumberFormat_WriteUInt8Digits_HundredsLoop:
    lda zp_param0_low
    cmp #100
    bcc NumberFormat_WriteUInt8Digits_HundredsDone
    sbc #100
    sta zp_param0_low
    inx
    jmp NumberFormat_WriteUInt8Digits_HundredsLoop
NumberFormat_WriteUInt8Digits_HundredsDone:
    jsr NumberFormat_EmitDigitOrSuppress

    ldx #0
NumberFormat_WriteUInt8Digits_TensLoop:
    lda zp_param0_low
    cmp #10
    bcc NumberFormat_WriteUInt8Digits_TensDone
    sbc #10
    sta zp_param0_low
    inx
    jmp NumberFormat_WriteUInt8Digits_TensLoop
NumberFormat_WriteUInt8Digits_TensDone:
    jsr NumberFormat_EmitDigitOrSuppress

    lda zp_param0_low
    ora #$30
    sta tostring_buffer,y
    iny
    lda #0
    sta tostring_buffer,y
    rts
.endif

.if Flag_NumberFormat_UInt8ToString

NumberFormat_UInt8ToString:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    ldy #0
    jsr NumberFormat_WriteUInt8Digits
    #stack_push_pointer tostring_buffer
    #stack_return_to_saved_address zp_tmp1_low
.endif

.if Flag_NumberFormat_Int8ToString

NumberFormat_Int8ToString:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    ; Branch on A's sign right after the pull (pla itself sets N/Z) --
    ; deliberately NOT "ldy #0" first: LDY also sets N/Z from the loaded
    ; value, which would clobber the sign flag from pla before bpl ever
    ; gets to read it (found the hard way -- an earlier version did "ldy
    ; #0" before this branch, which made it unconditionally take the
    ; positive path regardless of A's real sign).
    bpl NumberFormat_Int8ToString_Positive
    ; Negate (two's complement) to get an unsigned 0..128 magnitude --
    ; -128's magnitude (128) doesn't fit signed but does fit as the same
    ; unsigned byte NumberFormat_WriteUInt8Digits already expects.
    eor #$ff
    clc
    adc #1
    tax
    ldy #0
    lda #$2d
    sta tostring_buffer,y
    iny
    txa
    jmp NumberFormat_Int8ToString_Digits
NumberFormat_Int8ToString_Positive:
    ldy #0
NumberFormat_Int8ToString_Digits:
    jsr NumberFormat_WriteUInt8Digits
    #stack_push_pointer tostring_buffer
    #stack_return_to_saved_address zp_tmp1_low
.endif

.if Flag_NumberFormat_UInt16ToString | Flag_NumberFormat_Int16ToString

; Repeatedly subtracts the 16-bit value in zp_param1_low/high (a power of
; ten, the current decimal place) from zp_param0_low/high (the remaining
; magnitude) while zp_param0 >= zp_param1 (plain 6502 cmp/sbc are unsigned,
; exactly what's wanted -- both values are magnitudes by the time this
; runs), counting subtractions in X. Used once per decimal place by
; NumberFormat_WriteUInt16Digits below. Clobbers A/X, zp_param0_low/high.
NumberFormat_Digit16:
    ldx #0
NumberFormat_Digit16_Loop:
    lda zp_param0_high
    cmp zp_param1_high
    bcc NumberFormat_Digit16_Done
    bne NumberFormat_Digit16_Sub
    lda zp_param0_low
    cmp zp_param1_low
    bcc NumberFormat_Digit16_Done
NumberFormat_Digit16_Sub:
    lda zp_param0_low
    sec
    sbc zp_param1_low
    sta zp_param0_low
    lda zp_param0_high
    sbc zp_param1_high
    sta zp_param0_high
    inx
    jmp NumberFormat_Digit16_Loop
NumberFormat_Digit16_Done:
    rts

; zp_param0_low/high = magnitude (0..65535) on entry. Writes decimal
; digits into tostring_buffer starting at offset Y (same in/out convention
; as NumberFormat_WriteUInt8Digits above), null-terminates, leaves Y past
; the terminator. Shared by NumberFormat_UInt16ToString/Int16ToString
; below. Clobbers A/X, zp_param0_low/high, zp_param1_low/high (current
; power-of-ten operand for NumberFormat_Digit16), zp_param2_low (leading-
; zero-suppression flag).
NumberFormat_WriteUInt16Digits:
    lda #0
    sta zp_param2_low

    lda #<10000
    sta zp_param1_low
    lda #>10000
    sta zp_param1_high
    jsr NumberFormat_Digit16
    jsr NumberFormat_EmitDigitOrSuppress

    lda #<1000
    sta zp_param1_low
    lda #>1000
    sta zp_param1_high
    jsr NumberFormat_Digit16
    jsr NumberFormat_EmitDigitOrSuppress

    lda #<100
    sta zp_param1_low
    lda #>100
    sta zp_param1_high
    jsr NumberFormat_Digit16
    jsr NumberFormat_EmitDigitOrSuppress

    lda #<10
    sta zp_param1_low
    lda #>10
    sta zp_param1_high
    jsr NumberFormat_Digit16
    jsr NumberFormat_EmitDigitOrSuppress

    lda zp_param0_low
    ora #$30
    sta tostring_buffer,y
    iny
    lda #0
    sta tostring_buffer,y
    rts
.endif

.if Flag_NumberFormat_UInt16ToString

NumberFormat_UInt16ToString:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int16 zp_param0_low
    ldy #0
    jsr NumberFormat_WriteUInt16Digits
    #stack_push_pointer tostring_buffer
    #stack_return_to_saved_address zp_tmp1_low
.endif

.if Flag_NumberFormat_Int16ToString

NumberFormat_Int16ToString:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int16 zp_param0_low
    ldy #0
    lda zp_param0_high
    bpl NumberFormat_Int16ToString_Positive
    ; 16-bit two's complement: low = (~low)+1 (sets carry on a wrap, i.e.
    ; when low was 0), high = (~high)+carry -- standard idiom, relies on
    ; the carry from the low-byte add propagating into the high-byte one.
    lda zp_param0_low
    eor #$ff
    clc
    adc #1
    sta zp_param0_low
    lda zp_param0_high
    eor #$ff
    adc #0
    sta zp_param0_high
    lda #$2d
    sta tostring_buffer,y
    iny
NumberFormat_Int16ToString_Positive:
    jsr NumberFormat_WriteUInt16Digits
    #stack_push_pointer tostring_buffer
    #stack_return_to_saved_address zp_tmp1_low
.endif
