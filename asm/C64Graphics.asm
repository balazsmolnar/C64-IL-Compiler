; Hi-res bitmap graphics -- see Compiler/Templates/ProgramEntry.asm's
; GRAPHICS_USED-gated reservation block for where Graphics_Bitmap ($2000,
; 8000 bytes) and Graphics_ColorMatrix ($0c00, 1000 bytes) come from. Every
; routine here follows the same .weak Flag_X = 0 .endweak / .if Flag_X
; ... .endif shape as Screen_SetChar (above, in C64.asm) -- "if you don't
; use it you don't pay for it," down to the memory layout itself.
;
; Every flag's .weak fallback is declared together, up front, even though
; not every routine below has a body yet -- ProgramEntry.asm/
; UnitTestEntry.asm's GRAPHICS_USED OR's them together, and needs every
; name to resolve to SOMETHING (its .weak 0 default, absent a real caller)
; the moment that line assembles, regardless of which routines exist yet.
.weak
Flag_Screen_SetPixel = 0
; Screen.SetScreenMode(ScreenMode) replaces the old EnableBitmapMode/
; DisableBitmapMode pair with one call taking Character/Bitmap/MultiColor --
; see Screen_SetScreenMode below.
Flag_Screen_SetScreenMode = 0
Flag_Screen_DrawLine = 0
Flag_Screen_DrawRectangle = 0
Flag_Screen_DrawCircle = 0
Flag_Screen_SetDrawBuffer = 0
Flag_Screen_SwapBuffers = 0
Flag_Screen_SetBitmapColors = 0
Flag_Screen_WaitForVBlank = 0
; 1 when the program uses double buffering (Screen.SetDrawBuffer/SwapBuffers):
; the entry file then also reserves Graphics_Bitmap2/Graphics_ColorMatrix2 in
; VIC bank 1 (see ProgramEntry.asm). Defined there as a strong symbol;
; harnesses that include this file directly get this default.
GRAPHICS_DOUBLE_BUFFER = 0
.endweak

; Distance from Graphics_Bitmap to Graphics_Bitmap2, in 256-byte pages: the
; amount Graphics_ComputePixelAddress adds to the high byte of every pixel
; address while buffer 1 is the draw target.
.if GRAPHICS_DOUBLE_BUFFER
GRAPHICS_BUFFER_DELTA = (>Graphics_Bitmap2) - (>Graphics_Bitmap)
.endif

; ===========================================================================
; Screen.WaitForVBlank -- see Raster_WaitVBlank. (Also the wait inside
; Screen.SwapBuffers, hence the shared core.)
; ===========================================================================
; Blocks until the raster beam reaches line $FB (251): the first line of the
; bottom border, below the last line the 25-row display fetches, so anything
; done right after this returns (flipping VIC banks/pointers, moving sprites)
; can't tear the visible frame. A call made while the beam is already on
; line $FB first waits for it to leave, so back-to-back calls are one frame
; apart instead of the second returning at once. The beam is polled every 9
; cycles against a 63-cycle line, so it can't be stepped over. Line numbers
; past 255 have a low byte of 0-55 (PAL 311 lines), never $FB, so $D011's
; raster bit 8 doesn't need checking. Destroys A.
.if Flag_Screen_WaitForVBlank | Flag_Screen_SwapBuffers
Raster_WaitVBlank:
-   lda $d012
    cmp #$fb
    beq -
-   lda $d012
    cmp #$fb
    bne -
    rts
.endif

.if Flag_Screen_WaitForVBlank
Screen_WaitForVBlank:
    #stack_save_return_adress zp_tmp1_low
    jsr Raster_WaitVBlank
    #stack_return_to_saved_address zp_tmp1_low
.endif

; Set by Screen_SetScreenMode (0 = Character or Bitmap, nonzero =
; MultiColor) -- read by Graphics_ComputePixelAddress/Graphics_SetPixel_Core
; (once per pixel op, not per pixel) and Graphics_DrawLine_Core (once per
; line) to pick which pixel format to plot. Declared here, gated broadly
; (writer OR every reader), so it exists whenever any of them are compiled,
; matching graphics_saved_d018's own pattern below.
.if Flag_Screen_SetScreenMode | Flag_Screen_SetPixel | Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawCircle
graphics_multicolor_active .byte 0
.endif

; ===========================================================================
; Shared pixel addressing -- used by SetPixel and every shape routine.
; ===========================================================================
.if Flag_Screen_SetPixel | Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawCircle

; Bitmap memory is organized in 8x8 cells (8 consecutive scanline bytes per
; cell, cells in the same row-major 40x25 order as the text screen/
; Graphics_ColorMatrix) -- NOT linear y*320+x rows, which is what TEXT
; screen memory uses. This layout is IDENTICAL for hi-res and multicolor
; bitmap mode -- multicolor only changes how each byte's 8 bits are read
; (four 2-bit pairs instead of eight single bits), not where a byte lives.
; byte offset = (y/8)*320 + (x&~7) + (y&7). Table indexed by CELL ROW (y/8,
; 0-24), not by y itself.
bitmap_cellrow_low
.for i = 0, i < 25, i = i + 1
    .byte <(Graphics_Bitmap + i * 320)
.next
bitmap_cellrow_high
.for i = 0, i < 25, i = i + 1
    .byte >(Graphics_Bitmap + i * 320)
.next

; Hi-res: MSB-first, one bit per pixel (bit 7 of a bitmap byte is the
; leftmost pixel of its column).
bitmap_bit_table .byte $80,$40,$20,$10,$08,$04,$02,$01

; Multicolor: two bits per pixel (pairs 76/54/32/10, MSB-first), so x and
; x+1 always share a pair -- pairIndex = (x&6)>>1, 0-3, falls out of the
; same bit math the hi-res table above uses, just grouped by 2 instead of
; by 1. mc_pair_clear_mask[pairIndex] ANDs a byte down to every bit EXCEPT
; that pair; mc_pair_value_table[pairIndex*4 + colorSource] is the
; already-shifted 2-bit value (BitmapColorSource, 0-3) to OR back in, one
; 4-entry group per pairIndex -- table-driven, like the hi-res mask, so no
; runtime shift is needed either.
mc_pair_clear_mask .byte %00111111,%11001111,%11110011,%11111100
mc_pair_value_table
    .byte $00,%01000000,%10000000,%11000000    ; pairIndex 0 (shift 6)
    .byte $00,%00010000,%00100000,%00110000    ; pairIndex 1 (shift 4)
    .byte $00,%00000100,%00001000,%00001100    ; pairIndex 2 (shift 2)
    .byte $00,%00000001,%00000010,%00000011    ; pairIndex 3 (shift 0)

; In: zp_gfx_x_low/high (0-319), zp_gfx_y (0-199).
; Out: zp_gfx_ptr_low/high (byte address). Destroys A, X. Shared by both
; bitmap formats -- see the comment on bitmap_cellrow_low/high above.
Graphics_ComputePixelPointer:
    lda zp_gfx_y
    lsr
    lsr
    lsr                          ; A = cell_row = y/8 (0-24)
    tax
    lda zp_gfx_x_low
    and #$F8                     ; low byte of (x & ~7)
    clc
    adc bitmap_cellrow_low,x
    sta zp_gfx_ptr_low
    lda bitmap_cellrow_high,x
    adc zp_gfx_x_high            ; high byte of (x & ~7) (x's 256s bit, 0 or 1) + carry
.if GRAPHICS_DOUBLE_BUFFER
    clc
    adc graphics_draw_delta      ; 0 = Graphics_Bitmap, else Graphics_Bitmap2
.endif
    sta zp_gfx_ptr_high
    lda zp_gfx_y
    and #$07                     ; y&7 -- scanline within the cell
    clc
    adc zp_gfx_ptr_low
    sta zp_gfx_ptr_low
    bcc +
    inc zp_gfx_ptr_high
+   rts

; In: zp_gfx_x_low/high, zp_gfx_y. Out: zp_gfx_ptr_low/high (byte address),
; plus, depending on graphics_multicolor_active: hi-res -- zp_gfx_mask (bit
; within byte); multicolor -- zp_gfx_mask (AND-mask that clears this
; pixel's 2-bit pair) and zp_gfx_mc_pair_x4 (pairIndex*4, an offset into
; mc_pair_value_table). Destroys A, X.
Graphics_ComputePixelAddress:
    jsr Graphics_ComputePixelPointer
    lda graphics_multicolor_active
    bne Graphics_ComputePixelAddress_MC
    lda zp_gfx_x_low
    and #$07
    tax
    lda bitmap_bit_table,x
    sta zp_gfx_mask
    rts
Graphics_ComputePixelAddress_MC:
    lda zp_gfx_x_low
    and #$06
    lsr                          ; A = pairIndex (0-3) = (x&6)>>1
    tax
    lda mc_pair_clear_mask,x
    sta zp_gfx_mask
    txa
    asl
    asl                          ; A = pairIndex*4
    sta zp_gfx_mc_pair_x4
    rts

; In: zp_gfx_x_low/high, zp_gfx_y, zp_gfx_on (nonzero=set/0=clear),
; zp_gfx_color (multicolor mode only -- BitmapColorSource, 0-3).
; Destroys A, X, Y.
Graphics_SetPixel_Core:
    jsr Graphics_ComputePixelAddress
    ldy #0
    lda graphics_multicolor_active
    bne Graphics_SetPixel_Core_MC
    lda zp_gfx_on
    beq Graphics_SetPixel_Core_Clear
    lda (zp_gfx_ptr_low),y
    ora zp_gfx_mask
    sta (zp_gfx_ptr_low),y
    rts
Graphics_SetPixel_Core_Clear:
    lda zp_gfx_mask
    eor #$FF
    and (zp_gfx_ptr_low),y
    sta (zp_gfx_ptr_low),y
    rts
Graphics_SetPixel_Core_MC:
    lda (zp_gfx_ptr_low),y
    and zp_gfx_mask               ; clear this pixel's pair (to Background)
    sta (zp_gfx_ptr_low),y
    lda zp_gfx_on
    beq Graphics_SetPixel_Core_MC_Done   ; clearing: leave it at Background
    lda zp_gfx_color
    clc
    adc zp_gfx_mc_pair_x4
    tax
    lda mc_pair_value_table,x
    ora (zp_gfx_ptr_low),y
    sta (zp_gfx_ptr_low),y
Graphics_SetPixel_Core_MC_Done:
    rts
.endif

; ===========================================================================
; Screen.SetPixel
; ===========================================================================
.if Flag_Screen_SetPixel

Screen_SetPixel:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_gfx_color
    #stack_pull_int zp_gfx_on
    #stack_pull_int16 zp_gfx_y
    #stack_pull_int16 zp_gfx_x_low
    jsr Graphics_SetPixel_Core
    #stack_return_to_saved_address zp_tmp1_low
.endif

; ===========================================================================
; Screen.SetScreenMode
; ===========================================================================
.if Flag_Screen_SetScreenMode
; $d018 already has other live bits (Screen_SetCharSet's charset-select
; field) -- switching to/from bitmap mode needs a full save/restore of the
; byte, not a read-modify-write, since bitmap mode's video-matrix field
; points somewhere completely different ($0c00) than text mode's normal
; screen pointer.
graphics_saved_d018 .byte 0
; $d016 has other live bits too (screen-width/scroll-X, and
; Screen_SetMultiColor's own MCM use) -- MultiColor mode needs to restore
; exactly what was there before on the way back to Character mode, not
; just clear MCM unconditionally (which would also wipe those other bits
; for a caller who set them before ever touching bitmap mode).
graphics_saved_d016 .byte 0
.endif

.if Flag_Screen_SetScreenMode

; Zeroes Graphics_Bitmap's 8000 bytes / Graphics_ColorMatrix's 1000 bytes at
; RUNTIME -- confirmed empirically (via vice-verify) that the assembled
; .prg does NOT guarantee these addresses start zeroed: 64tass's listing
; reports a .fill'd-then-jumped-past region the same way as a genuinely
; untouched one ("Gap"), and real C64 RAM has non-zero content before this
; program's own code runs, which showed up as visible speckled noise in an
; unplotted part of the bitmap. Same reasoning #initHeap already applies to
; the object/GC tables (asm/helper/heap.asm) -- don't trust the loaded
; file's content for a scratch region, clear it explicitly on first use.
; Both bases are page-aligned ($2000/$0c00 both have a zero low byte), so
; the low byte of the pointer never needs to change -- only Y (0-255,
; wrapping) sweeps each page and the pointer's high byte advances between
; pages.
; In: A = high byte of the bitmap's base address ($20 or $40; low byte is 0).
Graphics_ClearBitmap:
    sta zp_gfx_ptr_high
    lda #0
    sta zp_gfx_ptr_low
    ldx #31                       ; 31 full 256-byte pages (31*256=7936)
    lda #0
Graphics_ClearBitmap_PageLoop:
    ldy #0
Graphics_ClearBitmap_ByteLoop:
    sta (zp_gfx_ptr_low),y
    iny
    bne Graphics_ClearBitmap_ByteLoop
    inc zp_gfx_ptr_high
    dex
    bne Graphics_ClearBitmap_PageLoop
    ldy #0                        ; final partial page: 8000-7936=64 bytes
Graphics_ClearBitmap_TailLoop:
    sta (zp_gfx_ptr_low),y
    iny
    cpy #64
    bne Graphics_ClearBitmap_TailLoop
    rts
.endif

.if Flag_Screen_SetScreenMode | Flag_Screen_SetBitmapColors

; In: A = high byte of the color matrix's base address (low byte is 0),
; X = value to fill all 1000 cells with. Destroys A, X, Y.
Graphics_FillColorMatrix:
    sta zp_gfx_ptr_high
    lda #0
    sta zp_gfx_ptr_low
    txa
    ldx #3                         ; 3 full pages (3*256=768)
Graphics_FillColorMatrix_PageLoop:
    ldy #0
Graphics_FillColorMatrix_ByteLoop:
    sta (zp_gfx_ptr_low),y
    iny
    bne Graphics_FillColorMatrix_ByteLoop
    inc zp_gfx_ptr_high
    dex
    bne Graphics_FillColorMatrix_PageLoop
    ldy #0                         ; final partial page: 1000-768=232 bytes
Graphics_FillColorMatrix_TailLoop:
    sta (zp_gfx_ptr_low),y
    iny
    cpy #232
    bne Graphics_FillColorMatrix_TailLoop
    rts
.endif

.if Flag_Screen_SetScreenMode

; Shared body of ScreenMode.Bitmap and ScreenMode.MultiColor: clears the
; bitmap(s), fills the color matrix/matrices, sets up double buffering if
; used, and points $d018 at bitmap mode's matrix/bitmap, plus turns on BMM
; ($d011 bit 5 -- true for both bitmap sub-modes alike). Does NOT touch
; $d016 (MCM) -- that differs between the two (MultiColor also needs to
; save $d016 first), so it's left to Screen_SetScreenMode's own two tails
; below. %00111000: bits 7-4 (video matrix, 1K units) = %0011 = block 3 =
; $0c00 (Graphics_ColorMatrix); bit 3 (bitmap half, 8K units) = 1 = $2000
; (Graphics_Bitmap).
Graphics_EnterBitmapMode:
    lda #>Graphics_Bitmap
    jsr Graphics_ClearBitmap
    lda #>Graphics_ColorMatrix
    ldx #0
    jsr Graphics_FillColorMatrix
.if GRAPHICS_DOUBLE_BUFFER
    lda #>Graphics_Bitmap2
    jsr Graphics_ClearBitmap
    lda #>Graphics_ColorMatrix2
    ldx #0
    jsr Graphics_FillColorMatrix
    lda #0
    sta graphics_draw_delta       ; draw into (and show) buffer 0 to start with
    lda $dd00
    ora #%00000011                ; VIC bank 0
    sta $dd00
.endif
    lda $d018
    sta graphics_saved_d018
    lda #%00111000
    sta $d018
    lda $d011
    ora #%00100000                ; BMM on
    sta $d011
    rts

; In: zp_gfx_on = ScreenMode (0=Character, 1=Bitmap, 2=MultiColor).
Screen_SetScreenMode:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_gfx_on
    lda zp_gfx_on
    beq Screen_SetScreenMode_Character
    cmp #2
    beq Screen_SetScreenMode_MultiColor

Screen_SetScreenMode_Bitmap:
    jsr Graphics_EnterBitmapMode
    lda #0
    sta graphics_multicolor_active
    lda $d016
    and #%11101111                 ; MCM off, in case a prior call set it
    sta $d016
    jmp Screen_SetScreenMode_Done

Screen_SetScreenMode_MultiColor:
    jsr Graphics_EnterBitmapMode
    lda #1
    sta graphics_multicolor_active
    lda $d016
    sta graphics_saved_d016
    ora #%00010000                 ; MCM on
    sta $d016
    jmp Screen_SetScreenMode_Done

; $d016 is only touched here if graphics_multicolor_active says MultiColor
; was actually the last mode entered -- a Bitmap-only (or never-bitmap)
; caller's $d016 is left completely alone, matching the old
; DisableBitmapMode's behavior exactly for that case.
Screen_SetScreenMode_Character:
    lda $d011
    and #%11011111                 ; BMM off
    sta $d011
    lda graphics_saved_d018
    sta $d018
    lda graphics_multicolor_active
    beq Screen_SetScreenMode_Character_NoD016
    lda graphics_saved_d016
    sta $d016
    lda #0
    sta graphics_multicolor_active
Screen_SetScreenMode_Character_NoD016:
.if GRAPHICS_DOUBLE_BUFFER
    lda $dd00
    ora #%00000011                 ; VIC bank 0 again (buffer 1 lives in bank 1)
    sta $dd00
.endif

Screen_SetScreenMode_Done:
    #stack_return_to_saved_address zp_tmp1_low
.endif

; ===========================================================================
; Double buffering: Screen.SetDrawBuffer / SwapBuffers / SetBitmapColors
; ===========================================================================
; Buffer 0 is Graphics_Bitmap/Graphics_ColorMatrix in VIC bank 0 ($D018=$38),
; buffer 1 is Graphics_Bitmap2/Graphics_ColorMatrix2 in VIC bank 1 ($D018=$80:
; matrix at bank offset $2000, bitmap at offset 0) -- see the layout block in
; ProgramEntry.asm. Drawing goes into whichever buffer graphics_draw_delta
; selects; the display shows the other one.
.if GRAPHICS_DOUBLE_BUFFER

graphics_draw_delta .byte 0      ; 0 = draw into buffer 0, else GRAPHICS_BUFFER_DELTA

.if Flag_Screen_SetDrawBuffer
Screen_SetDrawBuffer:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_gfx_on
    lda zp_gfx_on
    beq +
    lda #GRAPHICS_BUFFER_DELTA
+   sta graphics_draw_delta
    #stack_return_to_saved_address zp_tmp1_low
.endif

; Shows the buffer that was just drawn into, then makes the other one the
; draw target. The wait puts the beam on line 251, in the bottom border,
; where the VIC reads no bitmap data, so the two register writes below
; (which land a few cycles apart) can't tear the picture; the new buffer is
; what the next frame starts with. $DD00's upper bits are the serial bus, so
; only bits 0-1 (VIC bank: %11 = bank 0, %10 = bank 1) are touched.
.if Flag_Screen_SwapBuffers
Screen_SwapBuffers:
    #stack_save_return_adress zp_tmp1_low
    jsr Raster_WaitVBlank
    lda graphics_draw_delta
    bne Graphics_Swap_ShowBuffer1
    lda #%00111000                ; buffer 0: matrix $0c00, bitmap $2000
    sta $d018
    lda $dd00
    ora #%00000011
    sta $dd00
    lda #GRAPHICS_BUFFER_DELTA
    sta graphics_draw_delta       ; next frame is drawn into buffer 1
    jmp Graphics_Swap_Done
Graphics_Swap_ShowBuffer1:
    lda #%10000000                ; buffer 1: matrix $6000, bitmap $4000
    sta $d018
    lda $dd00
    and #%11111100
    ora #%00000010
    sta $dd00
    lda #0
    sta graphics_draw_delta       ; next frame is drawn into buffer 0
Graphics_Swap_Done:
    #stack_return_to_saved_address zp_tmp1_low
.endif

.endif

; Sets every cell of the color matrix (both buffers' when double buffering)
; to one foreground/background byte (high nibble = foreground, low =
; background).
.if Flag_Screen_SetBitmapColors
Screen_SetBitmapColors:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_gfx_on
    ldx zp_gfx_on
    lda #>Graphics_ColorMatrix
    jsr Graphics_FillColorMatrix
.if GRAPHICS_DOUBLE_BUFFER
    ldx zp_gfx_on
    lda #>Graphics_ColorMatrix2
    jsr Graphics_FillColorMatrix
.endif
    #stack_return_to_saved_address zp_tmp1_low
.endif

; ===========================================================================
; Shared line-plotting helpers -- used by DrawRectangle directly, and by
; DrawLine/DrawCircle for their own straight spans.
; ===========================================================================
.if Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawCircle

; Multicolor: repeated 2-bit color pattern for a FULLY interior byte (all 4
; pixel-pairs the same color) -- index by colorSource (0-3). Used only by
; Graphics_HLine_Core's fast interior-byte blit below; a single pixel still
; goes through Graphics_SetPixel_Core_MC's own mc_pair_value_table.
mc_fill_pattern .byte $00,$55,$AA,$FF

; Plots pixels (zp_gfx_x_low/high .. zp_gfx_endx_low/high, zp_gfx_y)
; inclusive. Caller must ensure start <= end. Leaves zp_gfx_x_low/high at
; endx. Destroys zp_gfx_sx/sy (both throwaway across any HLine_Core call --
; see asm/helper/zeropage.asm) and zp_gfx_err_low/high (DrawLine's own
; registers -- never touched by anything that calls HLine_Core, same
; "never concurrent" reasoning as DrawCircle's own reuse of
; zp_gfx_dx_low/dy, just for a different pair of registers).
;
; A bitmap byte holds 8 pixels (hi-res: 8 bits; multicolor: 4 two-bit
; pairs, but still 8 pixels' worth of x -- see the pairIndex comment
; above) at a FIXED address for that whole width, unlike a vertical run,
; which crosses a different byte almost every pixel (Graphics_VLine_Core,
; deliberately not given this treatment). So: any byte NOT fully covered
; by [x,endx] -- the first, if x isn't already byte-aligned, and the
; last -- is still plotted one pixel at a time via Graphics_SetPixel_Core,
; same as before; every byte FULLY covered in between instead gets one
; blind byte write ($FF/$00 hi-res, mc_fill_pattern multicolor) -- no
; per-pixel address recomputation, no read-modify-write, since the whole
; byte is being overwritten anyway.
Graphics_HLine_Core:
    ; End byte's address, stashed in zp_gfx_err_low/high: swap endx into
    ; x_low/high, compute, stash, restore x_low/high.
    lda zp_gfx_x_low
    sta zp_gfx_sx
    lda zp_gfx_x_high
    sta zp_gfx_sy
    lda zp_gfx_endx_low
    sta zp_gfx_x_low
    lda zp_gfx_endx_high
    sta zp_gfx_x_high
    jsr Graphics_ComputePixelPointer
    lda zp_gfx_ptr_low
    sta zp_gfx_err_low
    lda zp_gfx_ptr_high
    sta zp_gfx_err_high
    lda zp_gfx_sx
    sta zp_gfx_x_low
    lda zp_gfx_sy
    sta zp_gfx_x_high

    ; Head: per-pixel until x lands on a byte boundary (x&7==0), or the
    ; whole span turns out to fit within this one partial byte.
Graphics_HLine_Core_Head:
    lda zp_gfx_x_low
    and #7
    beq Graphics_HLine_Core_HeadDone
    jsr Graphics_SetPixel_Core
    lda zp_gfx_x_low
    cmp zp_gfx_endx_low
    bne Graphics_HLine_Core_HeadAdvance
    lda zp_gfx_x_high
    cmp zp_gfx_endx_high
    beq Graphics_HLine_Core_Done
Graphics_HLine_Core_HeadAdvance:
    inc zp_gfx_x_low
    bne Graphics_HLine_Core_Head
    inc zp_gfx_x_high
    jmp Graphics_HLine_Core_Head
Graphics_HLine_Core_HeadDone:

    ; Interior: blit whole bytes, from x's now byte-aligned position, up
    ; to (not including) the end byte.
    jsr Graphics_ComputePixelPointer   ; ptr_low/high = the byte x now starts

    lda zp_gfx_on
    beq Graphics_HLine_Core_FillZero
    lda graphics_multicolor_active
    bne Graphics_HLine_Core_FillMC
    lda #$FF
    jmp Graphics_HLine_Core_FillDone
Graphics_HLine_Core_FillMC:
    ldx zp_gfx_color
    lda mc_fill_pattern,x
    jmp Graphics_HLine_Core_FillDone
Graphics_HLine_Core_FillZero:
    lda #$00
Graphics_HLine_Core_FillDone:
    sta zp_gfx_sx                      ; fill_byte_value, reloaded each iteration

Graphics_HLine_Core_Interior:
    lda zp_gfx_ptr_low
    cmp zp_gfx_err_low
    bne Graphics_HLine_Core_InteriorGo
    lda zp_gfx_ptr_high
    cmp zp_gfx_err_high
    beq Graphics_HLine_Core_InteriorDone
Graphics_HLine_Core_InteriorGo:
    ldy #0
    lda zp_gfx_sx
    sta (zp_gfx_ptr_low),y
    lda zp_gfx_ptr_low                 ; next byte along this row is +8
    clc                                ; (same cell row, next cell: bytes
    adc #8                             ; within a row are 8 apart, not 1 --
    sta zp_gfx_ptr_low                 ; see the (x&$F8) term ComputePixel-
    bcc +                              ; Pointer adds directly)
    inc zp_gfx_ptr_high
+   lda zp_gfx_x_low
    clc
    adc #8
    sta zp_gfx_x_low
    bcc +
    inc zp_gfx_x_high
+   jmp Graphics_HLine_Core_Interior
Graphics_HLine_Core_InteriorDone:

    ; Tail: per-pixel for whatever's left of the end byte (at most 8
    ; pixels, never accelerated -- keeps this simple, and it's a small,
    ; bounded cost next to however many interior bytes preceded it).
Graphics_HLine_Core_Tail:
    jsr Graphics_SetPixel_Core
    lda zp_gfx_x_low
    cmp zp_gfx_endx_low
    bne Graphics_HLine_Core_TailAdvance
    lda zp_gfx_x_high
    cmp zp_gfx_endx_high
    beq Graphics_HLine_Core_Done
Graphics_HLine_Core_TailAdvance:
    inc zp_gfx_x_low
    bne Graphics_HLine_Core_Tail
    inc zp_gfx_x_high
    jmp Graphics_HLine_Core_Tail

Graphics_HLine_Core_Done:
    rts

; Plots pixels (zp_gfx_x_low/high, zp_gfx_y .. zp_gfx_endy) inclusive.
; Caller must ensure start <= end. Destroys zp_gfx_y.
Graphics_VLine_Core:
Graphics_VLine_Core_Loop:
    jsr Graphics_SetPixel_Core
    lda zp_gfx_y
    cmp zp_gfx_endy
    beq Graphics_VLine_Core_Done
    inc zp_gfx_y
    jmp Graphics_VLine_Core_Loop
Graphics_VLine_Core_Done:
    rts
.endif

; ===========================================================================
; Screen.DrawRectangle
; ===========================================================================
.if Flag_Screen_DrawRectangle

; Ensures zp_gfx_x_low/high <= zp_gfx_endx_low/high and zp_gfx_y <=
; zp_gfx_endy, swapping each pair if not (a rectangle's corners can be
; passed in any order). Unsigned 16-bit compare via SBC: computing
; end-start and checking the resulting carry (set = did not borrow = end
; >= start) is the standard 6502 idiom.
Graphics_NormalizeRectCoords:
    sec
    lda zp_gfx_endx_low
    sbc zp_gfx_x_low
    lda zp_gfx_endx_high
    sbc zp_gfx_x_high
    bcs Graphics_NormalizeRectCoords_XOk
    ldy zp_gfx_x_low
    lda zp_gfx_endx_low
    sta zp_gfx_x_low
    sty zp_gfx_endx_low
    ldy zp_gfx_x_high
    lda zp_gfx_endx_high
    sta zp_gfx_x_high
    sty zp_gfx_endx_high
Graphics_NormalizeRectCoords_XOk:
    lda zp_gfx_y
    cmp zp_gfx_endy
    bcc Graphics_NormalizeRectCoords_YOk
    ldy zp_gfx_y
    lda zp_gfx_endy
    sta zp_gfx_y
    sty zp_gfx_endy
Graphics_NormalizeRectCoords_YOk:
    rts

; In: zp_gfx_x_low/high=x0, zp_gfx_y=y0, zp_gfx_endx_low/high=x1,
; zp_gfx_endy=y1 (already normalized), zp_gfx_on. Draws all 4 sides.
Graphics_RectOutline_Core:
    lda zp_gfx_x_low
    sta zp_gfx_rect_x0_low
    lda zp_gfx_x_high
    sta zp_gfx_rect_x0_high
    lda zp_gfx_y
    sta zp_gfx_rect_y0
    jsr Graphics_HLine_Core         ; top edge: y0, x0..x1
    lda zp_gfx_rect_x0_low
    sta zp_gfx_x_low
    lda zp_gfx_rect_x0_high
    sta zp_gfx_x_high
    lda zp_gfx_endy
    sta zp_gfx_y
    jsr Graphics_HLine_Core         ; bottom edge: y1, x0..x1
    lda zp_gfx_rect_x0_low
    sta zp_gfx_x_low
    lda zp_gfx_rect_x0_high
    sta zp_gfx_x_high
    lda zp_gfx_rect_y0
    sta zp_gfx_y
    jsr Graphics_VLine_Core         ; left edge: x0, y0..y1
    lda zp_gfx_endx_low
    sta zp_gfx_x_low
    lda zp_gfx_endx_high
    sta zp_gfx_x_high
    lda zp_gfx_rect_y0
    sta zp_gfx_y
    jsr Graphics_VLine_Core         ; right edge: x1, y0..y1
    rts

; Same inputs as Graphics_RectOutline_Core. One HLine per row, y0..y1.
Graphics_FillRect_Core:
    lda zp_gfx_x_low
    sta zp_gfx_rect_x0_low
    lda zp_gfx_x_high
    sta zp_gfx_rect_x0_high
    lda zp_gfx_y
    sta zp_gfx_rect_y0
Graphics_FillRect_Core_RowLoop:
    lda zp_gfx_rect_x0_low
    sta zp_gfx_x_low
    lda zp_gfx_rect_x0_high
    sta zp_gfx_x_high
    jsr Graphics_HLine_Core
    lda zp_gfx_rect_y0
    cmp zp_gfx_endy
    beq Graphics_FillRect_Core_Done
    inc zp_gfx_rect_y0
    lda zp_gfx_rect_y0
    sta zp_gfx_y
    jmp Graphics_FillRect_Core_RowLoop
Graphics_FillRect_Core_Done:
    rts

Screen_DrawRectangle:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_gfx_color
    #stack_pull_int zp_gfx_on
    #stack_pull_int zp_gfx_filled
    #stack_pull_int16 zp_gfx_endy
    #stack_pull_int16 zp_gfx_endx_low
    #stack_pull_int16 zp_gfx_y
    #stack_pull_int16 zp_gfx_x_low
    jsr Graphics_NormalizeRectCoords
    lda zp_gfx_filled
    beq Screen_DrawRectangle_Outline
    jsr Graphics_FillRect_Core
    jmp Screen_DrawRectangle_Done
Screen_DrawRectangle_Outline:
    jsr Graphics_RectOutline_Core
Screen_DrawRectangle_Done:
    #stack_return_to_saved_address zp_tmp1_low
.endif

; ===========================================================================
; Screen.DrawLine
; ===========================================================================
.if Flag_Screen_DrawLine

; Integer Bresenham with the exact same pixels as the textbook version (X-major
; when dx >= dy, error starts at half the major delta, the minor axis steps
; when the error goes negative -- SimpleEmulator.Test's GraphicsLineTests
; checks every pixel against a reference of that), but built for speed:
;
;   * The bitmap address and bit mask are computed once, for the start point,
;     and then stepped: an X step shifts the mask (adding/subtracting 8 to the
;     pointer only when it wraps to the next byte); a Y step moves the pointer
;     one scanline (adding/subtracting 313 = 320-7 only when it crosses into
;     the next 8-pixel cell row). The pointer's low three bits always equal
;     y&7 (the bitmap and every cell are 8-byte aligned), so that's what the
;     cell-crossing test looks at. The old version recomputed the whole
;     address from x and y for every pixel.
;   * The error term is 8 bits, with the borrow from SBC as the "went
;     negative" test. That needs the major delta to fit in a byte: always
;     true for Y-major (dy <= 199), and for X-major unless dx > 255, which
;     takes the original 16-bit loop below (Graphics_DrawLine_XMajor_Wide).
;   * Each loop counts its pixels in X (Y stays 0 for the indirect access)
;     instead of comparing the coordinate with the end point, and there is a
;     separate loop per step direction, so nothing tests a direction per pixel.
;   * Set vs clear is patched into the loops' plot instruction before they
;     run (Graphics_DrawLine_Patch): "bit mask" (no effect on A) sets, "eor
;     mask" clears, since (b | m) ^ m == b & ~m. Program code is RAM, so this
;     is safe here.
;
; In: zp_gfx_x_low/high=x0, zp_gfx_y=y0 (start), zp_gfx_endx_low/high=x1,
; zp_gfx_endy=y1 (end), zp_gfx_on (nonzero = set, 0 = clear). Destroys A, X,
; Y and the zp_gfx_* registers -- including x/y, which are NOT advanced to the
; end point any more.

; Advance one pixel in X (falls through). The mask is a single bit: shifting
; it out of the byte means moving to the neighbouring byte, 8 bytes away.
gfx_xstep_right .macro
    lsr zp_gfx_mask
    bcc +
    ror zp_gfx_mask                 ; carry (1) becomes bit 7 again
    lda zp_gfx_ptr_low
    clc
    adc #8
    sta zp_gfx_ptr_low
    bcc +
    inc zp_gfx_ptr_high
+
.endm

gfx_xstep_left .macro
    asl zp_gfx_mask
    bcc +
    rol zp_gfx_mask                 ; carry (1) becomes bit 0 again
    lda zp_gfx_ptr_low
    sec
    sbc #8
    sta zp_gfx_ptr_low
    bcs +
    dec zp_gfx_ptr_high
+
.endm

; Advance one scanline down/up, then continue at \back. Within a cell the
; pointer moves by 1; crossing a cell boundary moves it 313 (down: from the
; cell's last scanline to the first of the next cell row) or -313.
gfx_ystep_down .macro back
    lda zp_gfx_ptr_low
    and #7
    cmp #7
    beq +
    inc zp_gfx_ptr_low
    jmp \back
+   lda zp_gfx_ptr_low
    clc
    adc #<313
    sta zp_gfx_ptr_low
    lda zp_gfx_ptr_high
    adc #>313
    sta zp_gfx_ptr_high
    jmp \back
.endm

gfx_ystep_up .macro back
    lda zp_gfx_ptr_low
    and #7
    beq +
    dec zp_gfx_ptr_low
    jmp \back
+   lda zp_gfx_ptr_low
    sec
    sbc #<313
    sta zp_gfx_ptr_low
    lda zp_gfx_ptr_high
    sbc #>313
    sta zp_gfx_ptr_high
    jmp \back
.endm

; Turns each loop's "bit mask" into "eor mask" when clearing ($24 = BIT zp,
; $45 = EOR zp). Destroys A, X.
Graphics_DrawLine_Patch:
    ldx #$45
    lda zp_gfx_on
    beq +
    ldx #$24
+   stx Graphics_DrawLine_XR_Plot
    stx Graphics_DrawLine_XL_Plot
    stx Graphics_DrawLine_YD_Plot
    stx Graphics_DrawLine_YU_Plot
    rts

Graphics_DrawLine_Core:
    ; dx = |x1-x0| (16-bit), sx = 1 if x1>=x0 else 0
    sec
    lda zp_gfx_endx_low
    sbc zp_gfx_x_low
    sta zp_gfx_dx_low
    lda zp_gfx_endx_high
    sbc zp_gfx_x_high
    sta zp_gfx_dx_high
    bpl Graphics_DrawLine_DXPositive
    sec
    lda #0
    sbc zp_gfx_dx_low
    sta zp_gfx_dx_low
    lda #0
    sbc zp_gfx_dx_high
    sta zp_gfx_dx_high
    lda #0
    sta zp_gfx_sx
    jmp Graphics_DrawLine_DXDone
Graphics_DrawLine_DXPositive:
    lda #1
    sta zp_gfx_sx
Graphics_DrawLine_DXDone:

    ; dy = |y1-y0| (8-bit magnitude, zero-extended to 16-bit), sy = 1 if
    ; y1>=y0 else 0.
    lda zp_gfx_endy
    sec
    sbc zp_gfx_y
    bcs Graphics_DrawLine_DYPositive
    eor #$ff
    clc
    adc #1
    sta zp_gfx_dy
    lda #0
    sta zp_gfx_sy
    jmp Graphics_DrawLine_DYDone
Graphics_DrawLine_DYPositive:
    sta zp_gfx_dy
    lda #1
    sta zp_gfx_sy
Graphics_DrawLine_DYDone:
    lda #0
    sta zp_gfx_dy_high

    ; X-major if dx>=dy (unsigned 16-bit compare, same SBC/carry idiom as
    ; Graphics_NormalizeRectCoords). Multicolor lines can't use the fast
    ; inline hi-res paths below (the self-modified bit/eor plotting and
    ; incremental mask-shift stepping both assume a single-bit mask) --
    ; graphics_multicolor_active instead routes them to
    ; Graphics_DrawLine_XMajor_Wide (already a complete, self-contained
    ; per-pixel X-major loop via Graphics_SetPixel_Core -- previously only
    ; reached for dx>255, reused unconditionally for multicolor too) or the
    ; analogous Graphics_DrawLine_YMajor_Simple below.
    sec
    lda zp_gfx_dx_low
    sbc zp_gfx_dy
    lda zp_gfx_dx_high
    sbc zp_gfx_dy_high
    bcs Graphics_DrawLine_XMajorSelect
    jmp Graphics_DrawLine_YMajorSelect

Graphics_DrawLine_XMajorSelect:
    lda graphics_multicolor_active
    bne Graphics_DrawLine_XMajor_Wide
    jmp Graphics_DrawLine_XMajor

Graphics_DrawLine_YMajorSelect:
    lda graphics_multicolor_active
    bne Graphics_DrawLine_YMajor_Simple
    jmp Graphics_DrawLine_YMajor

; ---------------------------------------------------------------------------
; X-major (dx >= dy). dx <= 255: fast loops; wider: the 16-bit loop below.
; ---------------------------------------------------------------------------
Graphics_DrawLine_XMajor:
    lda zp_gfx_dx_high
    beq +
    jmp Graphics_DrawLine_XMajor_Wide
+   jsr Graphics_DrawLine_Patch
    jsr Graphics_ComputePixelAddress
    lda zp_gfx_dx_low               ; err = dx >> 1
    lsr
    sta zp_gfx_err_low
    lda zp_gfx_dx_low               ; X = pixel count, dx+1 (256 wraps to 0,
    clc                             ; which the dex/beq below counts as 256)
    adc #1
    tax
    ldy #0
    lda zp_gfx_sx
    beq Graphics_DrawLine_XL_Loop

Graphics_DrawLine_XR_Loop:
    lda (zp_gfx_ptr_low),y
    ora zp_gfx_mask
Graphics_DrawLine_XR_Plot:
    bit zp_gfx_mask
    sta (zp_gfx_ptr_low),y
    dex
    beq Graphics_DrawLine_Done
    #gfx_xstep_right
    lda zp_gfx_err_low              ; err -= dy; borrow = went negative
    sec
    sbc zp_gfx_dy
    sta zp_gfx_err_low
    bcs Graphics_DrawLine_XR_Loop
    clc                             ; err += dx, and step Y
    adc zp_gfx_dx_low
    sta zp_gfx_err_low
    lda zp_gfx_sy
    beq Graphics_DrawLine_XR_Up
    #gfx_ystep_down Graphics_DrawLine_XR_Loop
Graphics_DrawLine_XR_Up:
    #gfx_ystep_up Graphics_DrawLine_XR_Loop

Graphics_DrawLine_XL_Loop:
    lda (zp_gfx_ptr_low),y
    ora zp_gfx_mask
Graphics_DrawLine_XL_Plot:
    bit zp_gfx_mask
    sta (zp_gfx_ptr_low),y
    dex
    beq Graphics_DrawLine_Done
    #gfx_xstep_left
    lda zp_gfx_err_low
    sec
    sbc zp_gfx_dy
    sta zp_gfx_err_low
    bcs Graphics_DrawLine_XL_Loop
    clc
    adc zp_gfx_dx_low
    sta zp_gfx_err_low
    lda zp_gfx_sy
    beq Graphics_DrawLine_XL_Up
    #gfx_ystep_down Graphics_DrawLine_XL_Loop
Graphics_DrawLine_XL_Up:
    #gfx_ystep_up Graphics_DrawLine_XL_Loop

; ---------------------------------------------------------------------------
; Y-major (dy > dx, so dx <= 198 and the 8-bit error always fits).
; ---------------------------------------------------------------------------
Graphics_DrawLine_YMajor:
    jsr Graphics_DrawLine_Patch
    jsr Graphics_ComputePixelAddress
    lda zp_gfx_dy                   ; err = dy >> 1
    lsr
    sta zp_gfx_err_low
    lda zp_gfx_dy                   ; X = pixel count, dy+1 (<= 200)
    clc
    adc #1
    tax
    ldy #0
    lda zp_gfx_sy
    beq Graphics_DrawLine_YU_Loop

Graphics_DrawLine_YD_Loop:
    lda (zp_gfx_ptr_low),y
    ora zp_gfx_mask
Graphics_DrawLine_YD_Plot:
    bit zp_gfx_mask
    sta (zp_gfx_ptr_low),y
    dex
    beq Graphics_DrawLine_Done
    #gfx_ystep_down Graphics_DrawLine_YD_Err
Graphics_DrawLine_YD_Err:
    lda zp_gfx_err_low              ; err -= dx; borrow = went negative
    sec
    sbc zp_gfx_dx_low
    sta zp_gfx_err_low
    bcs Graphics_DrawLine_YD_Loop
    clc                             ; err += dy, and step X
    adc zp_gfx_dy
    sta zp_gfx_err_low
    lda zp_gfx_sx
    beq Graphics_DrawLine_YD_Left
    #gfx_xstep_right
    jmp Graphics_DrawLine_YD_Loop
Graphics_DrawLine_YD_Left:
    #gfx_xstep_left
    jmp Graphics_DrawLine_YD_Loop

Graphics_DrawLine_YU_Loop:
    lda (zp_gfx_ptr_low),y
    ora zp_gfx_mask
Graphics_DrawLine_YU_Plot:
    bit zp_gfx_mask
    sta (zp_gfx_ptr_low),y
    dex
    beq Graphics_DrawLine_Done
    #gfx_ystep_up Graphics_DrawLine_YU_Err
Graphics_DrawLine_YU_Err:
    lda zp_gfx_err_low
    sec
    sbc zp_gfx_dx_low
    sta zp_gfx_err_low
    bcs Graphics_DrawLine_YU_Loop
    clc
    adc zp_gfx_dy
    sta zp_gfx_err_low
    lda zp_gfx_sx
    beq Graphics_DrawLine_YU_Left
    #gfx_xstep_right
    jmp Graphics_DrawLine_YU_Loop
Graphics_DrawLine_YU_Left:
    #gfx_xstep_left
    jmp Graphics_DrawLine_YU_Loop

Graphics_DrawLine_Done:
    rts

; ---------------------------------------------------------------------------
; X-major, plotting per-pixel through Graphics_SetPixel_Core with a 16-bit
; error term, from zp_gfx_x/y -- instead of the fast loops above, which
; assume a single-bit mask. Reached two ways: dx > 255 (up to 319, where
; the fast loops' 8-bit error term would overflow) for EITHER bitmap
; format, or ANY X-major multicolor line regardless of width (see
; Graphics_DrawLine_Core's graphics_multicolor_active dispatch) -- already
; fully general (termination is "x reached endx", not tied to a byte-sized
; error term), so no separate multicolor-only copy is needed.
; ---------------------------------------------------------------------------
Graphics_DrawLine_XMajor_Wide:
    lda zp_gfx_dx_high              ; err = dx >> 1 (unsigned)
    lsr
    sta zp_gfx_err_high
    lda zp_gfx_dx_low
    ror
    sta zp_gfx_err_low
Graphics_DrawLine_Wide_Loop:
    jsr Graphics_SetPixel_Core
    lda zp_gfx_x_low
    cmp zp_gfx_endx_low
    bne Graphics_DrawLine_Wide_Step
    lda zp_gfx_x_high
    cmp zp_gfx_endx_high
    beq Graphics_DrawLine_Done
Graphics_DrawLine_Wide_Step:
    lda zp_gfx_sx                   ; x += sx (16-bit)
    beq Graphics_DrawLine_Wide_DecX
    inc zp_gfx_x_low
    bne Graphics_DrawLine_Wide_AfterX
    inc zp_gfx_x_high
    jmp Graphics_DrawLine_Wide_AfterX
Graphics_DrawLine_Wide_DecX:
    lda zp_gfx_x_low
    bne Graphics_DrawLine_Wide_DecX_NoBorrow
    dec zp_gfx_x_high
Graphics_DrawLine_Wide_DecX_NoBorrow:
    dec zp_gfx_x_low
Graphics_DrawLine_Wide_AfterX:
    sec                              ; err -= dy
    lda zp_gfx_err_low
    sbc zp_gfx_dy
    sta zp_gfx_err_low
    lda zp_gfx_err_high
    sbc zp_gfx_dy_high
    sta zp_gfx_err_high
    bpl Graphics_DrawLine_Wide_Loop
    lda zp_gfx_sy                   ; err < 0: y += sy
    beq Graphics_DrawLine_Wide_DecY
    inc zp_gfx_y
    jmp Graphics_DrawLine_Wide_AfterY
Graphics_DrawLine_Wide_DecY:
    dec zp_gfx_y
Graphics_DrawLine_Wide_AfterY:
    clc                              ; err += dx
    lda zp_gfx_err_low
    adc zp_gfx_dx_low
    sta zp_gfx_err_low
    lda zp_gfx_err_high
    adc zp_gfx_dx_high
    sta zp_gfx_err_high
    jmp Graphics_DrawLine_Wide_Loop

; ---------------------------------------------------------------------------
; Y-major, multicolor only -- the analogous per-pixel fallback to
; Graphics_DrawLine_XMajor_Wide above, for when dy > dx (so termination is
; "y reached endy" instead). dy <= 199 always fits an 8-bit error term, so
; unlike the X-major case there's no separate "wide" reason to reach this
; for hi-res lines -- hi-res Y-major always uses the fast YD/YU loops.
; ---------------------------------------------------------------------------
Graphics_DrawLine_YMajor_Simple:
    lda zp_gfx_dy                   ; err = dy >> 1
    lsr
    sta zp_gfx_err_low
Graphics_DrawLine_YMajor_Simple_Loop:
    jsr Graphics_SetPixel_Core
    lda zp_gfx_y
    cmp zp_gfx_endy
    bne Graphics_DrawLine_YMajor_Simple_Step
    lda zp_gfx_x_low
    cmp zp_gfx_endx_low
    bne Graphics_DrawLine_YMajor_Simple_Step
    lda zp_gfx_x_high
    cmp zp_gfx_endx_high
    beq Graphics_DrawLine_Done
Graphics_DrawLine_YMajor_Simple_Step:
    lda zp_gfx_sy                   ; y += sy
    beq Graphics_DrawLine_YMajor_Simple_DecY
    inc zp_gfx_y
    jmp Graphics_DrawLine_YMajor_Simple_AfterY
Graphics_DrawLine_YMajor_Simple_DecY:
    dec zp_gfx_y
Graphics_DrawLine_YMajor_Simple_AfterY:
    sec                              ; err -= dx (dx <= dy <= 199, fits 8 bits)
    lda zp_gfx_err_low
    sbc zp_gfx_dx_low
    sta zp_gfx_err_low
    bpl Graphics_DrawLine_YMajor_Simple_Loop
    lda zp_gfx_sx                    ; err < 0: x += sx (16-bit)
    beq Graphics_DrawLine_YMajor_Simple_DecX
    inc zp_gfx_x_low
    bne Graphics_DrawLine_YMajor_Simple_AfterX
    inc zp_gfx_x_high
    jmp Graphics_DrawLine_YMajor_Simple_AfterX
Graphics_DrawLine_YMajor_Simple_DecX:
    lda zp_gfx_x_low
    bne Graphics_DrawLine_YMajor_Simple_DecX_NoBorrow
    dec zp_gfx_x_high
Graphics_DrawLine_YMajor_Simple_DecX_NoBorrow:
    dec zp_gfx_x_low
Graphics_DrawLine_YMajor_Simple_AfterX:
    lda zp_gfx_err_low               ; err += dy
    clc
    adc zp_gfx_dy
    sta zp_gfx_err_low
    jmp Graphics_DrawLine_YMajor_Simple_Loop

Screen_DrawLine:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_gfx_color
    #stack_pull_int zp_gfx_on
    #stack_pull_int16 zp_gfx_endy
    #stack_pull_int16 zp_gfx_endx_low
    #stack_pull_int16 zp_gfx_y
    #stack_pull_int16 zp_gfx_x_low
    jsr Graphics_DrawLine_Core
    #stack_return_to_saved_address zp_tmp1_low
.endif

; ===========================================================================
; Screen.DrawCircle
; ===========================================================================
.if Flag_Screen_DrawCircle

; Standard integer midpoint circle algorithm, 8-way octant symmetry.
; zp_gfx_circle_x/y are the algorithm's own (x,y) offsets from the center
; (x starts at radius and only ever decreases, y starts at 0 and only ever
; increases, until y>x); zp_gfx_circle_d_low/high is the signed 16-bit
; decision variable. zp_gfx_dx_low/zp_gfx_dy (DrawLine's own registers,
; unused here since DrawLine/DrawCircle never run concurrently) double as
; this routine's "offset_a"/"offset_b" scratch for Graphics_Circle_Plot4/
; RowPair below.
;
; In: zp_gfx_cx_low/high, zp_gfx_cy, zp_gfx_radius, zp_gfx_filled,
; zp_gfx_on. Destroys zp_gfx_x_low/high, zp_gfx_y, zp_gfx_endx_low/high
; (outline/filled both route through Graphics_SetPixel_Core/HLine_Core,
; same as every other shape routine in this file).

; zp_gfx_x_low/high = cx + zp_gfx_dx_low (16-bit + zero-extended 8-bit).
Graphics_Circle_AddOffA:
    lda zp_gfx_cx_low
    clc
    adc zp_gfx_dx_low
    sta zp_gfx_x_low
    lda zp_gfx_cx_high
    adc #0
    sta zp_gfx_x_high
    rts

; zp_gfx_x_low/high = cx - zp_gfx_dx_low.
Graphics_Circle_SubOffA:
    lda zp_gfx_cx_low
    sec
    sbc zp_gfx_dx_low
    sta zp_gfx_x_low
    lda zp_gfx_cx_high
    sbc #0
    sta zp_gfx_x_high
    rts

; Plots the 4 points (cx+-offset_a, cy+-offset_b).
Graphics_Circle_Plot4:
    jsr Graphics_Circle_AddOffA
    lda zp_gfx_cy
    clc
    adc zp_gfx_dy
    sta zp_gfx_y
    jsr Graphics_SetPixel_Core        ; (cx+a, cy+b)
    jsr Graphics_Circle_SubOffA
    jsr Graphics_SetPixel_Core        ; (cx-a, cy+b)
    lda zp_gfx_cy
    sec
    sbc zp_gfx_dy
    sta zp_gfx_y
    jsr Graphics_SetPixel_Core        ; (cx-a, cy-b)
    jsr Graphics_Circle_AddOffA
    jsr Graphics_SetPixel_Core        ; (cx+a, cy-b)
    rts

; Fills the 2 rows y=cy+-offset_b, each spanning x=[cx-offset_a, cx+offset_a].
Graphics_Circle_RowPair:
    jsr Graphics_Circle_AddOffA
    lda zp_gfx_x_low
    sta zp_gfx_endx_low
    lda zp_gfx_x_high
    sta zp_gfx_endx_high
    jsr Graphics_Circle_SubOffA
    lda zp_gfx_cy
    clc
    adc zp_gfx_dy
    sta zp_gfx_y
    jsr Graphics_HLine_Core           ; row cy+b (HLine advances x, so recompute below)
    jsr Graphics_Circle_SubOffA
    lda zp_gfx_cy
    sec
    sbc zp_gfx_dy
    sta zp_gfx_y
    jsr Graphics_HLine_Core           ; row cy-b
    rts

Graphics_DrawCircle_Core:
    lda zp_gfx_radius
    sta zp_gfx_circle_x
    lda #0
    sta zp_gfx_circle_y
    sec                                ; d = 1 - radius (signed 16-bit)
    lda #1
    sbc zp_gfx_radius
    sta zp_gfx_circle_d_low
    lda #0
    sbc #0
    sta zp_gfx_circle_d_high
Graphics_DrawCircle_Loop:
    lda zp_gfx_circle_y                ; loop while y<=x (x>=y)
    cmp zp_gfx_circle_x
    beq Graphics_DrawCircle_Plot
    bcc Graphics_DrawCircle_Plot
    jmp Graphics_DrawCircle_Done
Graphics_DrawCircle_Plot:
    lda zp_gfx_filled
    beq Graphics_DrawCircle_Outline
    lda zp_gfx_circle_x
    sta zp_gfx_dx_low
    lda zp_gfx_circle_y
    sta zp_gfx_dy
    jsr Graphics_Circle_RowPair
    lda zp_gfx_circle_y
    sta zp_gfx_dx_low
    lda zp_gfx_circle_x
    sta zp_gfx_dy
    jsr Graphics_Circle_RowPair
    jmp Graphics_DrawCircle_Step
Graphics_DrawCircle_Outline:
    lda zp_gfx_circle_x
    sta zp_gfx_dx_low
    lda zp_gfx_circle_y
    sta zp_gfx_dy
    jsr Graphics_Circle_Plot4
    lda zp_gfx_circle_y
    sta zp_gfx_dx_low
    lda zp_gfx_circle_x
    sta zp_gfx_dy
    jsr Graphics_Circle_Plot4
Graphics_DrawCircle_Step:
    inc zp_gfx_circle_y
    lda zp_gfx_circle_d_high
    bmi Graphics_DrawCircle_DNeg

    ; d>=0: x--, delta = 2*(y-x)+1. Unlike the d<0 branch, (y-x) is NOT
    ; guaranteed non-negative here -- x is still close to radius early in
    ; the octant, well above y -- so this must sign-extend the 8-bit
    ; subtraction result into a real signed 16-bit value (standard 6502
    ; idiom: "lda #0 / sbc #0" turns the SBC's borrow/no-borrow into a
    ; $00/$ff high byte), not zero-extend it like the d<0 case below does.
    dec zp_gfx_circle_x
    sec
    lda zp_gfx_circle_y
    sbc zp_gfx_circle_x
    sta zp_gfx_dx_low
    lda #0
    sbc #0
    sta zp_gfx_dy
    jmp Graphics_DrawCircle_Delta

Graphics_DrawCircle_DNeg:
    lda zp_gfx_circle_y                ; d<0: delta = 2*y+1 (y is always >=0)
    sta zp_gfx_dx_low
    lda #0
    sta zp_gfx_dy

Graphics_DrawCircle_Delta:
    ; (zp_gfx_dx_low, zp_gfx_dy) = pre-value*2+1, 16-bit
    asl zp_gfx_dx_low
    rol zp_gfx_dy
    lda zp_gfx_dx_low
    clc
    adc #1
    sta zp_gfx_dx_low
    lda zp_gfx_dy
    adc #0
    sta zp_gfx_dy
    clc                                 ; d += delta (signed 16-bit add)
    lda zp_gfx_circle_d_low
    adc zp_gfx_dx_low
    sta zp_gfx_circle_d_low
    lda zp_gfx_circle_d_high
    adc zp_gfx_dy
    sta zp_gfx_circle_d_high
    jmp Graphics_DrawCircle_Loop
Graphics_DrawCircle_Done:
    rts

Screen_DrawCircle:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_gfx_color
    #stack_pull_int zp_gfx_on
    #stack_pull_int zp_gfx_filled
    ; radius and cy are pulled into DrawLine's scratch bytes, not their own
    ; final homes: #stack_pull_int16's implicit "address+1" write would
    ; otherwise land on the NEXT gfx_circle_* byte -- radius/cy sit
    ; back-to-back in the zp map ($18/$19), unlike DrawLine's x/y, which
    ; each have a dedicated spare "_high" neighbor set aside for exactly
    ; this. Copied into their real homes below, after every pull is done.
    #stack_pull_int16 zp_gfx_dx_low    ; radius -> scratch (dx_high discarded)
    #stack_pull_int16 zp_gfx_dy        ; cy -> scratch (dy_high discarded)
    #stack_pull_int16 zp_gfx_cx_low
    lda zp_gfx_dx_low
    sta zp_gfx_radius
    lda zp_gfx_dy
    sta zp_gfx_cy
    jsr Graphics_DrawCircle_Core
    #stack_return_to_saved_address zp_tmp1_low
.endif
