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
Flag_Screen_DrawTrapezoid = 0
Flag_Screen_DrawCircle = 0
Flag_Screen_SetDrawBuffer = 0
Flag_Screen_SwapBuffers = 0
Flag_Screen_SetBitmapColors = 0
Flag_Screen_WaitForVBlank = 0
Flag_Screen_ClearBitmap = 0
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
.if Flag_Screen_SetScreenMode | Flag_Screen_SetPixel | Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawTrapezoid | Flag_Screen_DrawCircle
graphics_multicolor_active .byte 0
.endif

; ===========================================================================
; Shared pixel addressing -- used by SetPixel and every shape routine.
; ===========================================================================
.if Flag_Screen_SetPixel | Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawTrapezoid | Flag_Screen_DrawCircle

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
; x+1 always share a pair. Both tables are indexed by the column within the
; byte (x&7): mc_col_clear ANDs a byte down to every bit EXCEPT that column's
; pair, mc_col_set is the pair's own two bits (a color's repeated pattern from
; mc_fill_pattern ANDed with it is that color's value for the pair) --
; table-driven, so no runtime shift is needed.
mc_col_clear .byte $3F,$3F,$CF,$CF,$F3,$F3,$FC,$FC
mc_col_set   .byte $C0,$C0,$30,$30,$0C,$0C,$03,$03

; Multicolor: repeated 2-bit color pattern for a FULLY covered byte (all 4
; pixel-pairs the same color) -- index by colorSource (0-3).
mc_fill_pattern .byte $00,$55,$AA,$FF

; In: zp_gfx_x_low/high (0-319), zp_gfx_y (0-199).
; Out: zp_gfx_ptr_low/high (byte address). Destroys A, X. Shared by both
; bitmap formats -- see the comment on bitmap_cellrow_low/high above.
; The bitmap and every 8x8 cell are 8-byte aligned, so (x&~7) + the cell
; row's offset has its low three bits clear and y&7 can simply be OR'ed in
; (no carry) -- which also means the pointer's low three bits are always y&7
; (DrawLine and the fill routines step the pointer relying on that).
.cerror (Graphics_Bitmap & 7) != 0, "Graphics_Bitmap must be 8-byte aligned"
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
    ora zp_gfx_ptr_low
    sta zp_gfx_ptr_low
    rts

; Hi-res only (multicolor works per column, see Graphics_SetPixel_Core and
; Graphics_DrawLine_MC_Setup). In: zp_gfx_x_low/high, zp_gfx_y.
; Out: zp_gfx_ptr_low/high (byte address), zp_gfx_mask (the pixel's bit).
; Destroys A, X.
Graphics_ComputePixelAddress:
    jsr Graphics_ComputePixelPointer
    lda zp_gfx_x_low
    and #$07
    tax
    lda bitmap_bit_table,x
    sta zp_gfx_mask
    rts

; In: zp_gfx_x_low/high, zp_gfx_y, zp_gfx_on (nonzero=set/0=clear),
; zp_gfx_color (multicolor mode only -- BitmapColorSource, 0-3).
; Destroys A, X, Y. Graphics_ComputePixelPointer is written out in line (one
; call and one return less per pixel -- this is the per-point cost of circle
; outlines and of dx>255 lines).
Graphics_SetPixel_Core:
    lda zp_gfx_y
    lsr
    lsr
    lsr
    tax
    lda zp_gfx_x_low
    and #$F8
    clc
    adc bitmap_cellrow_low,x
    sta zp_gfx_ptr_low
    lda bitmap_cellrow_high,x
    adc zp_gfx_x_high
.if GRAPHICS_DOUBLE_BUFFER
    clc
    adc graphics_draw_delta
.endif
    sta zp_gfx_ptr_high
    lda zp_gfx_y
    and #$07
    ora zp_gfx_ptr_low
    sta zp_gfx_ptr_low
    lda zp_gfx_x_low
    and #$07
    tax                          ; X = column within the byte
    ldy #0
    lda graphics_multicolor_active
    bne Graphics_SetPixel_Core_MC
    lda zp_gfx_on
    beq Graphics_SetPixel_Core_Clear
    lda bitmap_bit_table,x
    ora (zp_gfx_ptr_low),y
    sta (zp_gfx_ptr_low),y
    rts
Graphics_SetPixel_Core_Clear:
    lda bitmap_bit_table,x
    eor #$FF
    and (zp_gfx_ptr_low),y
    sta (zp_gfx_ptr_low),y
    rts
Graphics_SetPixel_Core_MC:
    lda (zp_gfx_ptr_low),y
    and mc_col_clear,x            ; clear this pixel's pair (to Background)
    ldy zp_gfx_on
    beq Graphics_SetPixel_Core_MC_Store   ; clearing: leave it at Background (Y is 0)
    sta zp_gfx_mask
    ldy zp_gfx_color
    lda mc_fill_pattern,y
    and mc_col_set,x              ; the color's bits for this pair
    ora zp_gfx_mask
    ldy #0
Graphics_SetPixel_Core_MC_Store:
    sta (zp_gfx_ptr_low),y
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

.if Flag_Screen_SetScreenMode | Flag_Screen_ClearBitmap

; Zeroes a bitmap's 8000 bytes at RUNTIME -- confirmed empirically (via
; vice-verify) that the assembled .prg does NOT guarantee these addresses
; start zeroed: 64tass's listing reports a .fill'd-then-jumped-past region
; the same way as a genuinely untouched one ("Gap"), and real C64 RAM has
; non-zero content before this program's own code runs, which showed up as
; visible speckled noise in an unplotted part of the bitmap. Same reasoning
; #initHeap already applies to the object/GC tables (asm/helper/heap.asm) --
; don't trust the loaded file's content for a scratch region, clear it
; explicitly on first use.
;
; Built for speed, since Screen.ClearBitmap runs once per frame in a
; double-buffered program: 8000 = 32 chunks of 250 bytes, cleared 8 chunks at
; a time by one loop of eight `sta abs,y` (5 cycles a byte, against 11 for a
; `sta (zp),y` sweep). Y counts 250..1 and each store's operand is the chunk's
; address minus 1, so all eight chunks are covered by the same Y; the eight
; operands are patched (self-modifying code -- program code is RAM, as in
; Graphics_DrawLine_Patch) for each of the 4 groups.
; In: A = high byte of the bitmap's base address (low byte is 0: $20 or $40).
; Destroys A, X, Y, zp_gfx_ptr_*, zp_gfx_dx_low.
Graphics_ClearBitmap:
    sta zp_gfx_ptr_high
    dec zp_gfx_ptr_high           ; ptr = base - 1 = $xxFF, one page down
    lda #$FF
    sta zp_gfx_ptr_low
    lda #4
    sta zp_gfx_dx_low             ; groups left
Graphics_ClearBitmap_Group:
    ldx #0
Graphics_ClearBitmap_Patch:
    lda zp_gfx_ptr_low
    sta Graphics_ClearBitmap_Store+1,x
    clc
    adc #250
    sta zp_gfx_ptr_low
    lda zp_gfx_ptr_high
    sta Graphics_ClearBitmap_Store+2,x
    adc #0
    sta zp_gfx_ptr_high
    inx
    inx
    inx
    cpx #24
    bne Graphics_ClearBitmap_Patch
    ldy #250
    lda #0
Graphics_ClearBitmap_Store:
    sta Graphics_Bitmap,y
    sta Graphics_Bitmap,y
    sta Graphics_Bitmap,y
    sta Graphics_Bitmap,y
    sta Graphics_Bitmap,y
    sta Graphics_Bitmap,y
    sta Graphics_Bitmap,y
    sta Graphics_Bitmap,y
    dey
    bne Graphics_ClearBitmap_Store
    dec zp_gfx_dx_low
    bne Graphics_ClearBitmap_Group
    rts
.endif

; Screen.ClearBitmap: clears the bitmap of the current draw target (buffer 0,
; or buffer 1 after Screen.SetDrawBuffer(1)/the last SwapBuffers). The color
; matrix is left alone.
.if Flag_Screen_ClearBitmap
Screen_ClearBitmap:
    #stack_save_return_adress zp_tmp1_low
    lda #>Graphics_Bitmap
.if GRAPHICS_DOUBLE_BUFFER
    clc
    adc graphics_draw_delta       ; 0 = buffer 0, else buffer 1's page offset
.endif
    jsr Graphics_ClearBitmap
    #stack_return_to_saved_address zp_tmp1_low
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
; Shared line-plotting helpers -- used by DrawRectangle/DrawTrapezoid
; directly, and by DrawLine/DrawCircle for their own straight spans.
; ===========================================================================
.if Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawTrapezoid | Flag_Screen_DrawCircle

; Which bits of a byte a span touches in its first / last byte. The first
; byte is covered from a column to its right edge (span_left_mask, indexed
; by x&7); the last from its left edge through a column (span_right_mask).
; Multicolor uses the same tables at the column rounded to its pair
; boundary (col&6 for the start, col|1 for the end): a 2-bit pair covers
; two columns, so whichever column of a pair a span touches, the whole pair
; is affected -- exactly what plotting the pixels one at a time did.
span_left_mask .byte $FF,$7F,$3F,$1F,$0F,$07,$03,$01
span_right_mask .byte $80,$C0,$E0,$F0,$F8,$FC,$FE,$FF

; ---------------------------------------------------------------------------
; Horizontal spans, a byte at a time. A byte holds 8 pixels (hi-res: 8 bits;
; multicolor: 4 two-bit pairs, still 8 columns of x) at one fixed address --
; unlike a vertical run, which lands in a different byte almost every
; pixel (Graphics_VLine_Core, deliberately not given this treatment). So a
; span [x0,x1] on one scanline is: the first byte (only some of its
; columns), some number of fully covered bytes, the last byte (only some of
; its columns). Every byte -- partial or not -- is written with one
; read-modify-write or plain store, never per pixel:
;     byte = (byte AND notMask) OR (fill AND mask)
; with fill $FF/$00 for hi-res set/clear, and for multicolor the color's
; repeated pattern (set) or $00 (clear, always to Background).
;
; Graphics_SpanSetup computes everything that depends only on x0/x1/color
; (so a rectangle does it ONCE for all its rows); Graphics_SpanRow then
; applies it to one row. Scratch: the zp_gfx_span_* names (zeropage.asm) --
; registers DrawLine uses, which nothing that calls these ever needs across
; the call (Graphics_HLine_Core is reached from DrawCircle and DrawRectangle
; with dx_low/dy/endy/cx/cy live, none of which are touched).
;
; In: zp_gfx_x_low/high <= zp_gfx_endx_low/high, zp_gfx_on, zp_gfx_color.
; Out: the zp_gfx_span_* bytes. Destroys A, X, zp_gfx_mask.
; ---------------------------------------------------------------------------
Graphics_SpanSetup:
    lda #0                             ; fill: what a fully covered byte becomes
    ldx zp_gfx_on
    beq Graphics_SpanSetup_FillDone
    lda graphics_multicolor_active
    bne Graphics_SpanSetup_FillMC
    lda #$FF
    jmp Graphics_SpanSetup_FillDone
Graphics_SpanSetup_FillMC:
    ldx zp_gfx_color
    lda mc_fill_pattern,x
Graphics_SpanSetup_FillDone:
    sta zp_gfx_span_fill

    lda zp_gfx_endx_high               ; count = (endx>>3) - (x>>3): the number
    lsr                                ; of bytes after the first
    lda zp_gfx_endx_low
    ror
    lsr
    lsr
    sta zp_gfx_span_count
    lda zp_gfx_x_high
    lsr
    lda zp_gfx_x_low
    ror
    lsr
    lsr
    sta zp_gfx_sy                      ; scratch: the first byte's column index
    lda zp_gfx_span_count
    sec
    sbc zp_gfx_sy
    sta zp_gfx_span_count

    lda zp_gfx_x_low                   ; first byte's mask
    and #7
    ldx graphics_multicolor_active
    beq Graphics_SpanSetup_HeadCol
    and #6
Graphics_SpanSetup_HeadCol:
    tax
    lda span_left_mask,x
    sta zp_gfx_mask

    lda zp_gfx_endx_low                ; last byte's mask (raw, for now)
    and #7
    ldx graphics_multicolor_active
    beq Graphics_SpanSetup_TailCol
    ora #1
Graphics_SpanSetup_TailCol:
    tax
    lda span_right_mask,x
    sta zp_gfx_span_notTail

    lda zp_gfx_span_count              ; first byte == last byte: one byte,
    bne Graphics_SpanSetup_TwoBytes    ; touched only where both masks agree
    lda zp_gfx_mask
    and zp_gfx_span_notTail
    sta zp_gfx_mask
Graphics_SpanSetup_TwoBytes:
    lda zp_gfx_mask
    eor #$FF
    sta zp_gfx_span_notHead
    lda zp_gfx_mask
    and zp_gfx_span_fill
    sta zp_gfx_span_fillHead
    lda zp_gfx_span_notTail
    and zp_gfx_span_fill
    sta zp_gfx_span_fillTail
    lda zp_gfx_span_notTail
    eor #$FF
    sta zp_gfx_span_notTail
    rts

; Applies the span to one row. In: zp_gfx_ptr_low/high = the byte holding the
; span's first pixel (Graphics_ComputePixelPointer's result), after
; Graphics_SpanSetup. Destroys zp_gfx_ptr_low/high (left at the last byte),
; A, X, Y.
Graphics_SpanRow:
    ldy #0
    lda (zp_gfx_ptr_low),y
    and zp_gfx_span_notHead
    ora zp_gfx_span_fillHead
    sta (zp_gfx_ptr_low),y
    ldx zp_gfx_span_count
    beq Graphics_SpanRow_Done
    dex
    beq Graphics_SpanRow_Tail
Graphics_SpanRow_Interior:             ; fully covered bytes: a plain store,
    lda zp_gfx_ptr_low                 ; the next byte along a row is +8 (next
    clc                                ; cell, same scanline -- the (x&$F8) term
    adc #8                             ; Graphics_ComputePixelPointer adds)
    sta zp_gfx_ptr_low
    bcc Graphics_SpanRow_NoCarry
    inc zp_gfx_ptr_high
Graphics_SpanRow_NoCarry:
    lda zp_gfx_span_fill
    sta (zp_gfx_ptr_low),y
    dex
    bne Graphics_SpanRow_Interior
Graphics_SpanRow_Tail:
    lda zp_gfx_ptr_low
    clc
    adc #8
    sta zp_gfx_ptr_low
    bcc Graphics_SpanRow_TailNoCarry
    inc zp_gfx_ptr_high
Graphics_SpanRow_TailNoCarry:
    lda (zp_gfx_ptr_low),y
    and zp_gfx_span_notTail
    ora zp_gfx_span_fillTail
    sta (zp_gfx_ptr_low),y
Graphics_SpanRow_Done:
    rts

; Plots pixels (zp_gfx_x_low/high .. zp_gfx_endx_low/high, zp_gfx_y)
; inclusive. Caller must ensure start <= end. Leaves x/y as they were.
; Destroys the zp_gfx_span_* scratch (see above), zp_gfx_mask, zp_gfx_ptr_*.
Graphics_HLine_Core:
    jsr Graphics_SpanSetup
    jsr Graphics_ComputePixelPointer
    jmp Graphics_SpanRow

; Same inputs as Graphics_RectOutline_Core. Every row of a filled rectangle
; is the SAME span (same x0/x1, same masks, same fill) at a different
; scanline, so the span is set up once (Graphics_SpanSetup) and the start
; byte's address is computed once; then each row just applies the span
; (Graphics_SpanRow) and steps the row pointer -- +1 down a scanline within
; a cell, +313 (320-7) from a cell's last scanline to the next cell row,
; the same stepping DrawLine's gfx_ystep_down does (the pointer's low 3
; bits always equal y&7). The row pointer lives in zp_gfx_dx_low/high --
; DrawLine's registers, unused by DrawRectangle -- since SpanRow walks
; zp_gfx_ptr along the row.
Graphics_FillRect_Core:
    jsr Graphics_SpanSetup
    jsr Graphics_ComputePixelPointer
    lda zp_gfx_ptr_low
    sta zp_gfx_dx_low
    lda zp_gfx_ptr_high
    sta zp_gfx_dx_high
Graphics_FillRect_Core_RowLoop:
    lda zp_gfx_dx_low
    sta zp_gfx_ptr_low
    lda zp_gfx_dx_high
    sta zp_gfx_ptr_high
    jsr Graphics_SpanRow
    lda zp_gfx_y
    cmp zp_gfx_endy
    beq Graphics_FillRect_Core_Done
    inc zp_gfx_y
    lda zp_gfx_dx_low
    and #7
    cmp #7
    beq Graphics_FillRect_Core_NextCell
    inc zp_gfx_dx_low
    jmp Graphics_FillRect_Core_RowLoop
Graphics_FillRect_Core_NextCell:
    lda zp_gfx_dx_low
    clc
    adc #<313
    sta zp_gfx_dx_low
    lda zp_gfx_dx_high
    adc #>313
    sta zp_gfx_dx_high
    jmp Graphics_FillRect_Core_RowLoop
Graphics_FillRect_Core_Done:
    rts

; Plots pixels (zp_gfx_x_low/high, zp_gfx_y .. zp_gfx_endy) inclusive.
; Caller must ensure start <= end. Destroys x, y, the zp_gfx_span_* scratch,
; zp_gfx_mask, zp_gfx_ptr_*, A, X, Y.
;
; A vertical run is one column, so it is a span whose first and last byte are
; the same one: Graphics_SpanSetup (with the end column = the start column)
; leaves the single byte's AND/OR masks in zp_gfx_span_notHead/fillHead --
; the same masks for every row -- and each row is then one read-modify-write.
; Within an 8x8 cell the 8 scanlines are consecutive bytes, so the pointer
; is kept at the cell's first byte and Y walks the scanline; leaving a cell
; moves the pointer down one cell row (320 bytes) and Y back to 0.
Graphics_VLine_Core:
    lda zp_gfx_x_low
    sta zp_gfx_endx_low
    lda zp_gfx_x_high
    sta zp_gfx_endx_high
    jsr Graphics_SpanSetup
    jsr Graphics_ComputePixelPointer
    lda zp_gfx_ptr_low
    and #7
    tay                                ; Y = scanline within the cell
    lda zp_gfx_ptr_low
    and #$F8
    sta zp_gfx_ptr_low                 ; pointer at the cell's first byte
    lda zp_gfx_endy
    sec
    sbc zp_gfx_y
    tax
    inx                                ; X = number of rows (1-200)
Graphics_VLine_Core_Loop:
    lda (zp_gfx_ptr_low),y
    and zp_gfx_span_notHead
    ora zp_gfx_span_fillHead
    sta (zp_gfx_ptr_low),y
    dex
    beq Graphics_VLine_Core_Done
    iny
    cpy #8
    bne Graphics_VLine_Core_Loop
    ldy #0                             ; next cell row: +320
    lda zp_gfx_ptr_low
    clc
    adc #<320
    sta zp_gfx_ptr_low
    lda zp_gfx_ptr_high
    adc #>320
    sta zp_gfx_ptr_high
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
    lda zp_gfx_endx_low             ; right edge first: Graphics_VLine_Core
    sta zp_gfx_x_low                ; overwrites zp_gfx_endx with its own x
    lda zp_gfx_endx_high
    sta zp_gfx_x_high
    lda zp_gfx_rect_y0
    sta zp_gfx_y
    jsr Graphics_VLine_Core         ; right edge: x1, y0..y1
    lda zp_gfx_rect_x0_low
    sta zp_gfx_x_low
    lda zp_gfx_rect_x0_high
    sta zp_gfx_x_high
    lda zp_gfx_rect_y0
    sta zp_gfx_y
    jmp Graphics_VLine_Core         ; left edge: x0, y0..y1

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

    ; A horizontal line is just a span (byte-at-a-time, see
    ; Graphics_HLine_Core): far faster than per-pixel plotting, and the only
    ; fast option for a multicolor line. HLine wants start <= end; sx says
    ; which way this line runs, so swap the ends first if it runs leftward.
    lda zp_gfx_dy
    bne Graphics_DrawLine_NotHorizontal
    lda zp_gfx_sx
    bne Graphics_DrawLine_HorizontalOrdered
    lda zp_gfx_x_low
    ldx zp_gfx_endx_low
    stx zp_gfx_x_low
    sta zp_gfx_endx_low
    lda zp_gfx_x_high
    ldx zp_gfx_endx_high
    stx zp_gfx_x_high
    sta zp_gfx_endx_high
Graphics_DrawLine_HorizontalOrdered:
    jmp Graphics_HLine_Core
Graphics_DrawLine_NotHorizontal:

    ; A vertical line (dx is 0) is a one-column span, in either mode:
    ; Graphics_VLine_Core sets the column's masks up once and then makes one
    ; read-modify-write per row (~30 cycles/pixel, against ~56 for the
    ; hi-res Y-major loop below). It wants y0 <= y1 (sy says which way this
    ; line runs).
    lda zp_gfx_dx_low
    ora zp_gfx_dx_high
    bne Graphics_DrawLine_NotVertical
    lda zp_gfx_sy
    bne Graphics_DrawLine_VerticalOrdered
    lda zp_gfx_y
    ldx zp_gfx_endy
    stx zp_gfx_y
    sta zp_gfx_endy
Graphics_DrawLine_VerticalOrdered:
    jmp Graphics_VLine_Core
Graphics_DrawLine_NotVertical:

    ; X-major if dx>=dy (unsigned 16-bit compare, same SBC/carry idiom as
    ; Graphics_NormalizeRectCoords). Multicolor lines can't use the hi-res
    ; loops below (the self-modified bit/eor plotting and the mask-shift
    ; stepping both assume a single-bit mask) -- graphics_multicolor_active
    ; routes them to Graphics_DrawLine_MC_XMajor / _YMajor instead, which
    ; are the same Bresenham stepping with a per-column pair plot.
    sec
    lda zp_gfx_dx_low
    sbc zp_gfx_dy
    lda zp_gfx_dx_high
    sbc zp_gfx_dy_high
    bcs Graphics_DrawLine_XMajorSelect
    jmp Graphics_DrawLine_YMajorSelect

Graphics_DrawLine_XMajorSelect:
    lda graphics_multicolor_active
    bne Graphics_DrawLine_MC_XMajor
    jmp Graphics_DrawLine_XMajor

Graphics_DrawLine_YMajorSelect:
    lda graphics_multicolor_active
    bne Graphics_DrawLine_MC_YMajor
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
; error term, from zp_gfx_x/y -- for dx > 255 (up to 319), where the fast
; loops' 8-bit error term would overflow, in either bitmap format. A full
; SetPixel per pixel, so much slower than the loops above, but such wide
; non-horizontal lines are rare; fully general (termination is "x reached
; endx", not tied to a byte-sized error term).
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
; Multicolor lines. The same Bresenham stepping (and the same pixels) as the
; hi-res loops above, with a different plot: a pixel is two bits shared with
; its x-neighbour, so instead of a single-bit mask the loops keep the pixel's
; column x&7 in X and plot through two 8-entry tables --
;   byte = (byte AND mc_col_clear[X]) OR mc_line_value[X]
; where mc_line_value is built once per line (Graphics_DrawLine_MC_Setup) as
; the color's repeated 2-bit pattern ANDed with each column's pair mask
; (0 when clearing, so the pair goes back to Background). An X step is inx
; (or dex), moving to the neighbouring byte, 8 bytes away, when it leaves
; 0-7; Y steps are the same gfx_ystep_* macros as hi-res (they only touch
; the pointer). Y stays 0 for the indirect access, the pixel count is in
; zp_gfx_line_count.
; ---------------------------------------------------------------------------
gfx_xstep_right_mc .macro
    inx
    cpx #8
    bne +
    ldx #0
    lda zp_gfx_ptr_low
    clc
    adc #8
    sta zp_gfx_ptr_low
    bcc +
    inc zp_gfx_ptr_high
+
.endm

gfx_xstep_left_mc .macro
    dex
    bpl +
    ldx #7
    lda zp_gfx_ptr_low
    sec
    sbc #8
    sta zp_gfx_ptr_low
    bcs +
    dec zp_gfx_ptr_high
+
.endm

mc_line_value .fill 8, 0

; In: zp_gfx_x_low/high, zp_gfx_y (start), zp_gfx_on, zp_gfx_color.
; Out: mc_line_value filled, zp_gfx_ptr_low/high = the start pixel's byte,
; X = its column (x&7), Y = 0. Destroys A, zp_gfx_mask.
Graphics_DrawLine_MC_Setup:
    lda #0
    ldx zp_gfx_on
    beq +
    ldx zp_gfx_color
    lda mc_fill_pattern,x
+   sta zp_gfx_mask
    ldx #7
-   lda zp_gfx_mask
    and mc_col_set,x
    sta mc_line_value,x
    dex
    bpl -
    jsr Graphics_ComputePixelPointer
    lda zp_gfx_x_low
    and #$07
    tax
    ldy #0
    rts

; X-major (dx >= dy). dx > 255 takes the 16-bit per-pixel loop above.
Graphics_DrawLine_MC_XMajor:
    lda zp_gfx_dx_high
    beq +
    jmp Graphics_DrawLine_XMajor_Wide
+   jsr Graphics_DrawLine_MC_Setup
    lda zp_gfx_dx_low               ; err = dx >> 1
    lsr
    sta zp_gfx_err_low
    lda zp_gfx_dx_low               ; pixel count dx+1 (256 wraps to 0, which
    clc                             ; the dec/beq below counts as 256)
    adc #1
    sta zp_gfx_line_count
    lda zp_gfx_sx
    beq Graphics_DrawLine_MCXL_Loop

Graphics_DrawLine_MCXR_Loop:
    lda (zp_gfx_ptr_low),y
    and mc_col_clear,x
    ora mc_line_value,x
    sta (zp_gfx_ptr_low),y
    dec zp_gfx_line_count
    beq Graphics_DrawLine_MC_Done
    #gfx_xstep_right_mc
    lda zp_gfx_err_low              ; err -= dy; borrow = went negative
    sec
    sbc zp_gfx_dy
    sta zp_gfx_err_low
    bcs Graphics_DrawLine_MCXR_Loop
    clc                             ; err += dx, and step Y
    adc zp_gfx_dx_low
    sta zp_gfx_err_low
    lda zp_gfx_sy
    beq Graphics_DrawLine_MCXR_Up
    #gfx_ystep_down Graphics_DrawLine_MCXR_Loop
Graphics_DrawLine_MCXR_Up:
    #gfx_ystep_up Graphics_DrawLine_MCXR_Loop

Graphics_DrawLine_MCXL_Loop:
    lda (zp_gfx_ptr_low),y
    and mc_col_clear,x
    ora mc_line_value,x
    sta (zp_gfx_ptr_low),y
    dec zp_gfx_line_count
    beq Graphics_DrawLine_MC_Done
    #gfx_xstep_left_mc
    lda zp_gfx_err_low
    sec
    sbc zp_gfx_dy
    sta zp_gfx_err_low
    bcs Graphics_DrawLine_MCXL_Loop
    clc
    adc zp_gfx_dx_low
    sta zp_gfx_err_low
    lda zp_gfx_sy
    beq Graphics_DrawLine_MCXL_Up
    #gfx_ystep_down Graphics_DrawLine_MCXL_Loop
Graphics_DrawLine_MCXL_Up:
    #gfx_ystep_up Graphics_DrawLine_MCXL_Loop

; Y-major (dy > dx, so dx <= 198 and the 8-bit error always fits).
Graphics_DrawLine_MC_YMajor:
    jsr Graphics_DrawLine_MC_Setup
    lda zp_gfx_dy                   ; err = dy >> 1
    lsr
    sta zp_gfx_err_low
    lda zp_gfx_dy                   ; pixel count dy+1 (<= 200)
    clc
    adc #1
    sta zp_gfx_line_count
    lda zp_gfx_sy
    beq Graphics_DrawLine_MCYU_Loop

Graphics_DrawLine_MCYD_Loop:
    lda (zp_gfx_ptr_low),y
    and mc_col_clear,x
    ora mc_line_value,x
    sta (zp_gfx_ptr_low),y
    dec zp_gfx_line_count
    beq Graphics_DrawLine_MC_Done
    #gfx_ystep_down Graphics_DrawLine_MCYD_Err
Graphics_DrawLine_MCYD_Err:
    lda zp_gfx_err_low              ; err -= dx; borrow = went negative
    sec
    sbc zp_gfx_dx_low
    sta zp_gfx_err_low
    bcs Graphics_DrawLine_MCYD_Loop
    clc                             ; err += dy, and step X
    adc zp_gfx_dy
    sta zp_gfx_err_low
    lda zp_gfx_sx
    beq Graphics_DrawLine_MCYD_Left
    #gfx_xstep_right_mc
    jmp Graphics_DrawLine_MCYD_Loop
Graphics_DrawLine_MCYD_Left:
    #gfx_xstep_left_mc
    jmp Graphics_DrawLine_MCYD_Loop

Graphics_DrawLine_MCYU_Loop:
    lda (zp_gfx_ptr_low),y
    and mc_col_clear,x
    ora mc_line_value,x
    sta (zp_gfx_ptr_low),y
    dec zp_gfx_line_count
    beq Graphics_DrawLine_MC_Done
    #gfx_ystep_up Graphics_DrawLine_MCYU_Err
Graphics_DrawLine_MCYU_Err:
    lda zp_gfx_err_low
    sec
    sbc zp_gfx_dx_low
    sta zp_gfx_err_low
    bcs Graphics_DrawLine_MCYU_Loop
    clc
    adc zp_gfx_dy
    sta zp_gfx_err_low
    lda zp_gfx_sx
    beq Graphics_DrawLine_MCYU_Left
    #gfx_xstep_right_mc
    jmp Graphics_DrawLine_MCYU_Loop
Graphics_DrawLine_MCYU_Left:
    #gfx_xstep_left_mc
    jmp Graphics_DrawLine_MCYU_Loop

Graphics_DrawLine_MC_Done:
    rts

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
; Screen.DrawTrapezoid
; ===========================================================================
.if Flag_Screen_DrawTrapezoid

; Ensures zp_gfx_y <= zp_gfx_endy, swapping each row's x-pair together with
; the y's if not (a trapezoid's two rows can be given in either order) --
; same unsigned-compare idiom as Graphics_NormalizeRectCoords, but three
; pairs move together here, so both edges stay matched to their own row.
Graphics_NormalizeTrapezoidCoords:
    lda zp_gfx_y
    cmp zp_gfx_endy
    bcc Graphics_NormalizeTrapezoidCoords_Ok
    beq Graphics_NormalizeTrapezoidCoords_Ok
    ldy zp_gfx_y
    lda zp_gfx_endy
    sta zp_gfx_y
    sty zp_gfx_endy
    ldy zp_gfx_x_low
    lda zp_gfx_endx_low
    sta zp_gfx_x_low
    sty zp_gfx_endx_low
    ldy zp_gfx_x_high
    lda zp_gfx_endx_high
    sta zp_gfx_x_high
    sty zp_gfx_endx_high
    ldy zp_gfx_cx_low
    lda zp_gfx_dx_low
    sta zp_gfx_cx_low
    sty zp_gfx_dx_low
    ldy zp_gfx_cx_high
    lda zp_gfx_dx_high
    sta zp_gfx_cx_high
    sty zp_gfx_dx_high
Graphics_NormalizeTrapezoidCoords_Ok:
    rts

; In: zp_gfx_x_low/high=x0Left, zp_gfx_cx_low/high=x0Right, zp_gfx_y=y0,
; zp_gfx_endx_low/high=x1Left, zp_gfx_dx_low/high=x1Right, zp_gfx_endy=y1,
; zp_gfx_on, zp_gfx_color. Fills every row y0..y1 inclusive with the span
; whose left/right x are linearly interpolated between (x0Left,x0Right) at
; y0 and (x1Left,x1Right) at y1 -- a general 4-corner trapezoid (a
; rectangle is the special case x0Left=x1Left/x0Right=x1Right; a triangle
; is x0Left=x0Right or x1Left=x1Right, a corner degenerating to a point).
;
; Each edge's per-row movement is a one-time division (Divide16Core, the
; same shift-subtract routine the compiler's own / operator uses) into an
; integer step (added to that edge's x every row) plus a remainder; a
; per-row DDA error accumulator folds the remainder in without any further
; division (same idea as Graphics_DrawLine's Bresenham error term, just
; between rows instead of between pixels). The row itself is filled via
; Graphics_SpanSetup/SpanRow, the same byte-level span fill as HLine_Core/
; FillRect_Core -- SpanSetup runs fresh every row (unlike FillRect_Core's
; once), since a trapezoid's left/right x change every row, but that cost
; is small next to the interior-byte fill itself.
;
; Destroys A, X, Y and every zp_gfx_* register except zp_gfx_on/zp_gfx_color.
Graphics_Trapezoid_Core:
    jsr Graphics_NormalizeTrapezoidCoords

    lda zp_gfx_endy                  ; height = y1 - y0, cached once: y
    sec                               ; itself becomes the row loop counter
    sbc zp_gfx_y                      ; below, so this can't be recomputed
    sta zp_gfx_trap_height            ; from y/endy partway through

    ; --- left edge: delta = x1Left(endx) - x0Left(x), into zp_param2 ---
    sec
    lda zp_gfx_endx_low
    sbc zp_gfx_x_low
    sta zp_param2_low
    lda zp_gfx_endx_high
    sbc zp_gfx_x_high
    sta zp_param2_high
    bpl Graphics_Trapezoid_LeftPositive
    sec
    lda #0
    sbc zp_param2_low
    sta zp_param2_low
    lda #0
    sbc zp_param2_high
    sta zp_param2_high
    lda #1
    sta zp_gfx_trap_left_sign
    jmp Graphics_Trapezoid_LeftSignDone
Graphics_Trapezoid_LeftPositive:
    lda #0
    sta zp_gfx_trap_left_sign
Graphics_Trapezoid_LeftSignDone:
    lda zp_gfx_trap_height
    sta zp_param1_low
    lda #0
    sta zp_param1_high
    jsr Divide16Core
    lda zp_param2_low
    sta zp_gfx_trap_left_step_low
    lda zp_param2_high
    sta zp_gfx_trap_left_step_high
    lda zp_param0_low
    sta zp_gfx_trap_left_rem
    lda #0
    sta zp_gfx_trap_left_error

    ; --- move x0Right (cx) into endx: row-0 starting value for the right edge ---
    lda zp_gfx_cx_low
    sta zp_gfx_endx_low
    lda zp_gfx_cx_high
    sta zp_gfx_endx_high

    ; --- right edge: delta = x1Right(dx) - x0Right(cx), into zp_param2 ---
    sec
    lda zp_gfx_dx_low
    sbc zp_gfx_cx_low
    sta zp_param2_low
    lda zp_gfx_dx_high
    sbc zp_gfx_cx_high
    sta zp_param2_high
    bpl Graphics_Trapezoid_RightPositive
    sec
    lda #0
    sbc zp_param2_low
    sta zp_param2_low
    lda #0
    sbc zp_param2_high
    sta zp_param2_high
    lda #1
    sta zp_gfx_trap_right_sign
    jmp Graphics_Trapezoid_RightSignDone
Graphics_Trapezoid_RightPositive:
    lda #0
    sta zp_gfx_trap_right_sign
Graphics_Trapezoid_RightSignDone:
    lda zp_gfx_trap_height
    sta zp_param1_low
    lda #0
    sta zp_param1_high
    jsr Divide16Core
    lda zp_param2_low
    sta zp_gfx_trap_right_step_low
    lda zp_param2_high
    sta zp_gfx_trap_right_step_high
    lda zp_param0_low
    sta zp_gfx_trap_right_rem
    lda #0
    sta zp_gfx_trap_right_error

Graphics_Trapezoid_RowLoop:
    jsr Graphics_SpanSetup
    jsr Graphics_ComputePixelPointer
    jsr Graphics_SpanRow
    lda zp_gfx_y
    cmp zp_gfx_endy
    beq Graphics_Trapezoid_Done
    inc zp_gfx_y

    ; --- step left edge's x (zp_gfx_x_low/high) by its per-row step ---
    lda zp_gfx_trap_left_sign
    bne Graphics_Trapezoid_LeftStepNeg
    lda zp_gfx_x_low
    clc
    adc zp_gfx_trap_left_step_low
    sta zp_gfx_x_low
    lda zp_gfx_x_high
    adc zp_gfx_trap_left_step_high
    sta zp_gfx_x_high
    jmp Graphics_Trapezoid_LeftStepDone
Graphics_Trapezoid_LeftStepNeg:
    lda zp_gfx_x_low
    sec
    sbc zp_gfx_trap_left_step_low
    sta zp_gfx_x_low
    lda zp_gfx_x_high
    sbc zp_gfx_trap_left_step_high
    sta zp_gfx_x_high
Graphics_Trapezoid_LeftStepDone:
    ; fold in the remainder: overflow (error+rem >= height) tested as
    ; rem >= height-error (zp_gfx_mask as transient scratch -- free between
    ; one row's SpanRow and the next row's SpanSetup) so the add itself
    ; never needs to exceed a byte.
    lda zp_gfx_trap_height
    sec
    sbc zp_gfx_trap_left_error
    sta zp_gfx_mask
    lda zp_gfx_trap_left_rem
    cmp zp_gfx_mask
    bcc Graphics_Trapezoid_LeftNoExtra
    sec
    sbc zp_gfx_mask
    sta zp_gfx_trap_left_error
    lda zp_gfx_trap_left_sign
    bne Graphics_Trapezoid_LeftExtraNeg
    inc zp_gfx_x_low
    bne Graphics_Trapezoid_LeftExtraDone
    inc zp_gfx_x_high
    jmp Graphics_Trapezoid_LeftExtraDone
Graphics_Trapezoid_LeftExtraNeg:
    lda zp_gfx_x_low
    bne Graphics_Trapezoid_LeftExtraNoBorrow
    dec zp_gfx_x_high
Graphics_Trapezoid_LeftExtraNoBorrow:
    dec zp_gfx_x_low
    jmp Graphics_Trapezoid_LeftExtraDone
Graphics_Trapezoid_LeftNoExtra:
    lda zp_gfx_trap_left_error
    clc
    adc zp_gfx_trap_left_rem
    sta zp_gfx_trap_left_error
Graphics_Trapezoid_LeftExtraDone:

    ; --- step right edge's x (zp_gfx_endx_low/high), same shape ---
    lda zp_gfx_trap_right_sign
    bne Graphics_Trapezoid_RightStepNeg
    lda zp_gfx_endx_low
    clc
    adc zp_gfx_trap_right_step_low
    sta zp_gfx_endx_low
    lda zp_gfx_endx_high
    adc zp_gfx_trap_right_step_high
    sta zp_gfx_endx_high
    jmp Graphics_Trapezoid_RightStepDone
Graphics_Trapezoid_RightStepNeg:
    lda zp_gfx_endx_low
    sec
    sbc zp_gfx_trap_right_step_low
    sta zp_gfx_endx_low
    lda zp_gfx_endx_high
    sbc zp_gfx_trap_right_step_high
    sta zp_gfx_endx_high
Graphics_Trapezoid_RightStepDone:
    lda zp_gfx_trap_height
    sec
    sbc zp_gfx_trap_right_error
    sta zp_gfx_mask
    lda zp_gfx_trap_right_rem
    cmp zp_gfx_mask
    bcc Graphics_Trapezoid_RightNoExtra
    sec
    sbc zp_gfx_mask
    sta zp_gfx_trap_right_error
    lda zp_gfx_trap_right_sign
    bne Graphics_Trapezoid_RightExtraNeg
    inc zp_gfx_endx_low
    bne Graphics_Trapezoid_RightExtraDone
    inc zp_gfx_endx_high
    jmp Graphics_Trapezoid_RightExtraDone
Graphics_Trapezoid_RightExtraNeg:
    lda zp_gfx_endx_low
    bne Graphics_Trapezoid_RightExtraNoBorrow
    dec zp_gfx_endx_high
Graphics_Trapezoid_RightExtraNoBorrow:
    dec zp_gfx_endx_low
    jmp Graphics_Trapezoid_RightExtraDone
Graphics_Trapezoid_RightNoExtra:
    lda zp_gfx_trap_right_error
    clc
    adc zp_gfx_trap_right_rem
    sta zp_gfx_trap_right_error
Graphics_Trapezoid_RightExtraDone:
    jmp Graphics_Trapezoid_RowLoop

Graphics_Trapezoid_Done:
    rts

Screen_DrawTrapezoid:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_gfx_color
    #stack_pull_int zp_gfx_on
    #stack_pull_int16 zp_gfx_endy          ; y1
    #stack_pull_int16 zp_gfx_dx_low        ; x1Right
    #stack_pull_int16 zp_gfx_endx_low      ; x1Left
    #stack_pull_int16 zp_gfx_y             ; y0
    #stack_pull_int16 zp_gfx_cx_low        ; x0Right
    #stack_pull_int16 zp_gfx_x_low         ; x0Left
    jsr Graphics_Trapezoid_Core
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
    ; The second pair of rows (cy+-x, spanning cx+-y) only needs drawing when
    ; it is the widest span those rows will get: while x stays the same, every
    ; following y widens it (the spans are nested), so it is skipped unless x
    ; is about to step (d >= 0) or this is the last iteration (y >= x, since
    ; d < 0 keeps x).
    lda zp_gfx_circle_d_high
    bpl Graphics_DrawCircle_SecondPair
    lda zp_gfx_circle_y
    cmp zp_gfx_circle_x
    bcc Graphics_DrawCircle_Step
Graphics_DrawCircle_SecondPair:
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
    lda zp_gfx_circle_x                ; radius 0: x would wrap below 0
    beq Graphics_DrawCircle_Done
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
