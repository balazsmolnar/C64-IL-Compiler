; Hi-res bitmap graphics -- see Compiler/Templates/ProgramEntry.asm's
; GRAPHICS_USED-gated reservation block for where Graphics_Bitmap ($2000,
; 8000 bytes) and Graphics_ColorMatrix ($0c00, 1000 bytes) come from. Every
; routine here follows the same .weak Flag_X = 0 .endweak / .if Flag_X
; ... .endif shape as Screen_SetChar (above, in C64.asm) -- "if you don't
; use it you don't pay for it," down to the memory layout itself.
;
; All six flags' .weak fallbacks are declared together, up front, even
; though not every routine below has a body yet -- ProgramEntry.asm/
; UnitTestEntry.asm's GRAPHICS_USED OR's all six together, and needs every
; name to resolve to SOMETHING (its .weak 0 default, absent a real caller)
; the moment that line assembles, regardless of which routines exist yet.
.weak
Flag_Screen_SetPixel = 0
Flag_Screen_EnableBitmapMode = 0
Flag_Screen_DisableBitmapMode = 0
Flag_Screen_DrawLine = 0
Flag_Screen_DrawRectangle = 0
Flag_Screen_DrawCircle = 0
.endweak

; ===========================================================================
; Shared pixel addressing -- used by SetPixel and every shape routine.
; ===========================================================================
.if Flag_Screen_SetPixel | Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawCircle

; Hi-res bitmap memory is organized in 8x8 cells (8 consecutive scanline
; bytes per cell, cells in the same row-major 40x25 order as the text
; screen/Graphics_ColorMatrix) -- NOT linear y*320+x rows, which is what
; TEXT screen memory uses. byte offset = (y/8)*320 + (x&~7) + (y&7), bit =
; 7-(x&7). Table indexed by CELL ROW (y/8, 0-24), not by y itself.
bitmap_cellrow_low
.for i = 0, i < 25, i = i + 1
    .byte <(Graphics_Bitmap + i * 320)
.next
bitmap_cellrow_high
.for i = 0, i < 25, i = i + 1
    .byte >(Graphics_Bitmap + i * 320)
.next

; MSB-first: bit 7 of a bitmap byte is the LEFTMOST pixel of its column.
bitmap_bit_table .byte $80,$40,$20,$10,$08,$04,$02,$01

; In: zp_gfx_x_low/high (0-319), zp_gfx_y (0-199).
; Out: zp_gfx_ptr_low/high (byte address), zp_gfx_mask (bit within byte).
; Destroys A, X.
Graphics_ComputePixelAddress:
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
    sta zp_gfx_ptr_high
    lda zp_gfx_y
    and #$07                     ; y&7 -- scanline within the cell
    clc
    adc zp_gfx_ptr_low
    sta zp_gfx_ptr_low
    bcc +
    inc zp_gfx_ptr_high
+   lda zp_gfx_x_low
    and #$07
    tax
    lda bitmap_bit_table,x
    sta zp_gfx_mask
    rts

; In: zp_gfx_x_low/high, zp_gfx_y, zp_gfx_on (nonzero=set/0=clear).
; Destroys A, X, Y.
Graphics_SetPixel_Core:
    jsr Graphics_ComputePixelAddress
    ldy #0
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
.endif

; ===========================================================================
; Screen.SetPixel
; ===========================================================================
.if Flag_Screen_SetPixel

Screen_SetPixel:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int zp_gfx_on
    #stack_pull_int16 zp_gfx_y
    #stack_pull_int16 zp_gfx_x_low
    jsr Graphics_SetPixel_Core
    #stack_return_to_saved_address zp_tmp1_low
.endif

; ===========================================================================
; Screen.EnableBitmapMode / DisableBitmapMode
; ===========================================================================
.if Flag_Screen_EnableBitmapMode | Flag_Screen_DisableBitmapMode
; $d018 already has other live bits (Screen_SetCharSet's charset-select
; field) -- toggling bitmap mode needs a full save/restore of the byte, not
; a read-modify-write, since bitmap mode's video-matrix field points
; somewhere completely different ($0c00) than text mode's normal screen
; pointer.
graphics_saved_d018 .byte 0
.endif

.if Flag_Screen_EnableBitmapMode

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
Graphics_ClearBitmap:
    lda #0
    sta zp_gfx_ptr_low
    lda #>Graphics_Bitmap
    sta zp_gfx_ptr_high
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

Graphics_ClearColorMatrix:
    lda #0
    sta zp_gfx_ptr_low
    lda #>Graphics_ColorMatrix
    sta zp_gfx_ptr_high
    ldx #3                         ; 3 full pages (3*256=768)
Graphics_ClearColorMatrix_PageLoop:
    ldy #0
Graphics_ClearColorMatrix_ByteLoop:
    sta (zp_gfx_ptr_low),y
    iny
    bne Graphics_ClearColorMatrix_ByteLoop
    inc zp_gfx_ptr_high
    dex
    bne Graphics_ClearColorMatrix_PageLoop
    ldy #0                         ; final partial page: 1000-768=232 bytes
Graphics_ClearColorMatrix_TailLoop:
    sta (zp_gfx_ptr_low),y
    iny
    cpy #232
    bne Graphics_ClearColorMatrix_TailLoop
    rts

; %00111000: bits 7-4 (video matrix, 1K units) = %0011 = block 3 = $0c00
; (Graphics_ColorMatrix); bit 3 (bitmap half, 8K units) = 1 = $2000
; (Graphics_Bitmap). $d011 bit 5 = BMM (bitmap mode enable).
Screen_EnableBitmapMode:
    #stack_save_return_adress zp_tmp1_low
    jsr Graphics_ClearBitmap
    jsr Graphics_ClearColorMatrix
    lda $d018
    sta graphics_saved_d018
    lda #%00111000
    sta $d018
    lda $d011
    ora #%00100000
    sta $d011
    #stack_return_to_saved_address zp_tmp1_low
.endif

.if Flag_Screen_DisableBitmapMode

Screen_DisableBitmapMode:
    #stack_save_return_adress zp_tmp1_low
    lda $d011
    and #%11011111
    sta $d011
    lda graphics_saved_d018
    sta $d018
    #stack_return_to_saved_address zp_tmp1_low
.endif

; ===========================================================================
; Shared line-plotting helpers -- used by DrawRectangle directly, and by
; DrawLine/DrawCircle for their own straight spans.
; ===========================================================================
.if Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawCircle

; Plots pixels (zp_gfx_x_low/high .. zp_gfx_endx_low/high, zp_gfx_y)
; inclusive. Caller must ensure start <= end. Destroys zp_gfx_x_low/high
; (advances it to end+1).
Graphics_HLine_Core:
Graphics_HLine_Core_Loop:
    jsr Graphics_SetPixel_Core
    lda zp_gfx_x_low
    cmp zp_gfx_endx_low
    bne Graphics_HLine_Core_Advance
    lda zp_gfx_x_high
    cmp zp_gfx_endx_high
    beq Graphics_HLine_Core_Done
Graphics_HLine_Core_Advance:
    inc zp_gfx_x_low
    bne Graphics_HLine_Core_Loop
    inc zp_gfx_x_high
    jmp Graphics_HLine_Core_Loop
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

; Classic (non-symmetric) integer Bresenham: drives the loop along whichever
; axis has the larger delta, so "err" only ever needs a "< 0" check (the N
; flag straight off a 16-bit SBC) instead of a signed compare against a
; second nonzero threshold -- much cheaper on a 6502 than the textbook
; "single loop, e2=2*err" formulation, which needs two signed comparisons
; per step. dx/dy are stored as unsigned magnitudes (16-bit -- x spans
; 0-319, needs the high byte; y's 0-199 never does, but dy/dy_high are kept
; 16-bit for symmetry with dx in the shared shift/compare code below), sx/sy
; are 0/1 "increasing" flags (not signed +-1 bytes) so stepping x/y is a
; plain inc/dec, never a signed add.
;
; In: zp_gfx_x_low/high=x0, zp_gfx_y=y0 (start -- also the live "current
; point" Graphics_SetPixel_Core plots from every step), zp_gfx_endx_low/
; high=x1, zp_gfx_endy=y1 (end), zp_gfx_on. Destroys zp_gfx_x_low/high,
; zp_gfx_y (advanced to the endpoint), zp_gfx_dx_low/high, zp_gfx_dy/
; zp_gfx_dy_high, zp_gfx_sx, zp_gfx_sy, zp_gfx_err_low/high.
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
    ; Graphics_NormalizeRectCoords).
    sec
    lda zp_gfx_dx_low
    sbc zp_gfx_dy
    lda zp_gfx_dx_high
    sbc zp_gfx_dy_high
    bcs Graphics_DrawLine_XMajor
    jmp Graphics_DrawLine_YMajor

Graphics_DrawLine_XMajor:
    lda zp_gfx_dx_high              ; err = dx >> 1 (unsigned)
    lsr
    sta zp_gfx_err_high
    lda zp_gfx_dx_low
    ror
    sta zp_gfx_err_low
Graphics_DrawLine_XMajor_Loop:
    jsr Graphics_SetPixel_Core
    lda zp_gfx_x_low
    cmp zp_gfx_endx_low
    bne Graphics_DrawLine_XMajor_Step
    lda zp_gfx_x_high
    cmp zp_gfx_endx_high
    beq Graphics_DrawLine_Done
Graphics_DrawLine_XMajor_Step:
    lda zp_gfx_sx                   ; x += sx (16-bit)
    beq Graphics_DrawLine_XMajor_DecX
    inc zp_gfx_x_low
    bne Graphics_DrawLine_XMajor_AfterX
    inc zp_gfx_x_high
    jmp Graphics_DrawLine_XMajor_AfterX
Graphics_DrawLine_XMajor_DecX:
    lda zp_gfx_x_low
    bne Graphics_DrawLine_XMajor_DecX_NoBorrow
    dec zp_gfx_x_high
Graphics_DrawLine_XMajor_DecX_NoBorrow:
    dec zp_gfx_x_low
Graphics_DrawLine_XMajor_AfterX:
    sec                              ; err -= dy
    lda zp_gfx_err_low
    sbc zp_gfx_dy
    sta zp_gfx_err_low
    lda zp_gfx_err_high
    sbc zp_gfx_dy_high
    sta zp_gfx_err_high
    bpl Graphics_DrawLine_XMajor_Loop
    lda zp_gfx_sy                   ; err < 0: y += sy
    beq Graphics_DrawLine_XMajor_DecY
    inc zp_gfx_y
    jmp Graphics_DrawLine_XMajor_AfterY
Graphics_DrawLine_XMajor_DecY:
    dec zp_gfx_y
Graphics_DrawLine_XMajor_AfterY:
    clc                              ; err += dx
    lda zp_gfx_err_low
    adc zp_gfx_dx_low
    sta zp_gfx_err_low
    lda zp_gfx_err_high
    adc zp_gfx_dx_high
    sta zp_gfx_err_high
    jmp Graphics_DrawLine_XMajor_Loop

Graphics_DrawLine_YMajor:
    lda zp_gfx_dy_high               ; err = dy >> 1 (unsigned)
    lsr
    sta zp_gfx_err_high
    lda zp_gfx_dy
    ror
    sta zp_gfx_err_low
Graphics_DrawLine_YMajor_Loop:
    jsr Graphics_SetPixel_Core
    lda zp_gfx_y
    cmp zp_gfx_endy
    beq Graphics_DrawLine_Done
    lda zp_gfx_sy                    ; y += sy
    beq Graphics_DrawLine_YMajor_DecY
    inc zp_gfx_y
    jmp Graphics_DrawLine_YMajor_AfterY
Graphics_DrawLine_YMajor_DecY:
    dec zp_gfx_y
Graphics_DrawLine_YMajor_AfterY:
    sec                               ; err -= dx
    lda zp_gfx_err_low
    sbc zp_gfx_dx_low
    sta zp_gfx_err_low
    lda zp_gfx_err_high
    sbc zp_gfx_dx_high
    sta zp_gfx_err_high
    bpl Graphics_DrawLine_YMajor_Loop
    lda zp_gfx_sx                    ; err < 0: x += sx (16-bit)
    beq Graphics_DrawLine_YMajor_DecX
    inc zp_gfx_x_low
    bne Graphics_DrawLine_YMajor_AfterX
    inc zp_gfx_x_high
    jmp Graphics_DrawLine_YMajor_AfterX
Graphics_DrawLine_YMajor_DecX:
    lda zp_gfx_x_low
    bne Graphics_DrawLine_YMajor_DecX_NoBorrow
    dec zp_gfx_x_high
Graphics_DrawLine_YMajor_DecX_NoBorrow:
    dec zp_gfx_x_low
Graphics_DrawLine_YMajor_AfterX:
    clc                               ; err += dy
    lda zp_gfx_err_low
    adc zp_gfx_dy
    sta zp_gfx_err_low
    lda zp_gfx_err_high
    adc zp_gfx_dy_high
    sta zp_gfx_err_high
    jmp Graphics_DrawLine_YMajor_Loop

Graphics_DrawLine_Done:
    rts

Screen_DrawLine:
    #stack_save_return_adress zp_tmp1_low
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
