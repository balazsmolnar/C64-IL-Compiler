; String.Concat(string,string)/.Length/.PadLeft(int,char) -- see
; Compiler/ILStringOpsPass.cs for how the real C# calls get rewritten into
; a jsr to one of the three routines below. All three read/write raw
; null-terminated character bytes (this compiler's only string
; representation -- see Assert_AreEqualString in the unit-test entry
; templates for the same convention) and, when they produce a new string,
; write it into stringops_buffer (asm/helper/objectTables.asm) -- see that
; file's comment for why this is a SEPARATE buffer from tostring_buffer
; (concatenating a ToString() result would otherwise alias its own input).
;
; Gated by the per-method dead-code-elimination mechanism (Compiler/
; ILLibraryFlagsPass.cs), same pattern as asm/helper/tostring.asm --
; ILStringOpsPass registers whichever of the three labels it actually needs
; into UsedLibraryLabels itself (synthesized jsr's, no real MethodBase to
; scan for). .include'd AFTER library_flags.asm/C64.asm, alongside
; tostring.asm, for the same reasons documented there.
;
; No bounds checking against stringops_buffer's fixed 40-byte size -- same
; silent-overflow philosophy as this codebase's arithmetic (add8/sub8 never
; check overflow either); Hunchback-scale strings (well under a 40-column
; screen row) never approach it.

.weak
Flag_String_Concat = 0
Flag_String_Length = 0
Flag_String_PadLeft = 0
Flag_String_Equals = 0
Flag_String_NotEquals = 0
.endweak

.if Flag_String_Concat

; Pops two string pointers (b popped first -- pushed last, per this
; compiler's usual argument order) and writes a's characters followed by
; b's into stringops_buffer, null-terminated. zp_param1_low/high = a,
; zp_param2_low/high = b. X = destination cursor throughout; Y = the
; current source string's own read offset (reset to 0 for each source in
; turn, since it's relative to that source's own pointer).
String_Concat:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_pointer zp_param2_low
    #stack_pull_pointer zp_param1_low

    ldx #0
    ldy #0
String_Concat_CopyA:
    lda (zp_param1_low),y
    beq String_Concat_CopyB_Start
    sta stringops_buffer,x
    iny
    inx
    jmp String_Concat_CopyA
String_Concat_CopyB_Start:
    ldy #0
String_Concat_CopyB:
    lda (zp_param2_low),y
    sta stringops_buffer,x
    beq String_Concat_Done
    iny
    inx
    jmp String_Concat_CopyB
String_Concat_Done:
    #stack_push_pointer stringops_buffer
    #stack_return_to_saved_address zp_tmp1_low
.endif

; Shared by both String_Equals and String_NotEquals -- a plain byte-by-byte
; comparison, stopping at the first mismatch or (if every byte compared so
; far matched) the null terminator both strings must then share.
;
; In: zp_param1_low/high, zp_param2_low/high (the two string pointers --
; already pulled off the hardware stack by the caller, NOT pulled here).
; Out: A = 1 (equal) or 0. This must NOT touch the hardware PHA/PLA stack
; itself (no #stack_pull_*/#stack_push_*): it's `jsr`'d from a wrapper that
; has already stripped ITS OWN return address to expose the real arguments
; (#stack_save_return_adress, same as String_Concat/Length/PadLeft above),
; so by the time this runs, the jsr into here has put ITS return address
; where those arguments used to be visible -- a #stack_pull_pointer in here
; would pop that return address instead of an operand, and this routine's
; own `rts` would then "return" into whatever the real operand bytes happen
; to encode as an address (confirmed live: landed inside a string literal's
; character data). Same zero-page-in/zero-page-out shape as
; Graphics_ComputePixelAddress (asm/C64Graphics.asm) and Float_LoadFac1_A
; (float.asm), which are `jsr`'d from inside other stack-pulling routines
; for exactly this reason.
.if Flag_String_Equals | Flag_String_NotEquals
String_Equals_Core:
    ldy #0
String_Equals_Loop:
    lda (zp_param1_low),y
    cmp (zp_param2_low),y
    bne String_Equals_Loop_NotEqual
    cmp #0
    beq String_Equals_Loop_Equal
    iny
    jmp String_Equals_Loop
String_Equals_Loop_Equal:
    lda #1
    rts
String_Equals_Loop_NotEqual:
    lda #0
    rts
.endif

.if Flag_String_Equals

; Pops two string pointers (b popped first, per this compiler's usual
; argument order) and pushes 1 (equal) or 0 as a 1-byte bool.
String_Equals:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_pointer zp_param2_low
    #stack_pull_pointer zp_param1_low
    jsr String_Equals_Core
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.if Flag_String_NotEquals

String_NotEquals:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_pointer zp_param2_low
    #stack_pull_pointer zp_param1_low
    jsr String_Equals_Core
    eor #1
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.if Flag_String_Length

; Pops one string pointer, returns its length (a plain null-terminated
; strlen) as a 1-byte int -- matches how every other "int" in this compiler
; is 1 byte (TypeExtensions.GetStorageBytes), even though real .NET's own
; String.Length is a 32-bit int; Hunchback-scale strings never approach 256
; characters, so the narrower width loses nothing here.
String_Length:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_pointer zp_param1_low

    ldy #0
String_Length_Loop:
    lda (zp_param1_low),y
    beq String_Length_Done
    iny
    jmp String_Length_Loop
String_Length_Done:
    tya
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.if Flag_String_PadLeft

; Pops paddingChar, totalWidth, then the source string pointer (this ==
; the receiver, pushed first so popped last) -- writes paddingChar
; max(0, totalWidth - sourceLength) times, then the source string itself,
; into stringops_buffer, null-terminated. zp_param0_low = source strlen,
; zp_param0_high = pad count (computed once, then counted down to 0 as the
; pad loop's own progress check -- when totalWidth <= sourceLength this is
; already 0, so the pad loop falls straight through to the copy with no
; special-casing needed). X = destination cursor throughout (persists
; across both loops); Y = source read offset, only used during the strlen
; pre-pass and the final copy (reset to 0 for the copy, since it's
; unrelated to the strlen pass's own count).
String_PadLeft:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_param3_low
    #stack_pull_int zp_param2_low
    #stack_pull_pointer zp_param1_low

    ldy #0
String_PadLeft_Strlen:
    lda (zp_param1_low),y
    beq String_PadLeft_StrlenDone
    iny
    jmp String_PadLeft_Strlen
String_PadLeft_StrlenDone:
    sty zp_param0_low

    lda zp_param2_low
    sec
    sbc zp_param0_low
    bcs String_PadLeft_HavePadCount
    lda #0
String_PadLeft_HavePadCount:
    sta zp_param0_high

    ldx #0
String_PadLeft_PadLoop:
    lda zp_param0_high
    beq String_PadLeft_CopySource
    lda zp_param3_low
    sta stringops_buffer,x
    inx
    dec zp_param0_high
    jmp String_PadLeft_PadLoop

String_PadLeft_CopySource:
    ldy #0
String_PadLeft_CopyLoop:
    lda (zp_param1_low),y
    sta stringops_buffer,x
    beq String_PadLeft_Done
    inx
    iny
    jmp String_PadLeft_CopyLoop

String_PadLeft_Done:
    #stack_push_pointer stringops_buffer
    #stack_return_to_saved_address zp_tmp1_low
.endif
