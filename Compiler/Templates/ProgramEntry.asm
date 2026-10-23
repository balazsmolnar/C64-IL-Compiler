.enc "screen"

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

; Which library routines the program calls (Flag_X = 1 each), and its sprite
; frames ([assembly: RawSprites]; counts, labels and two data macros). Both
; files only define symbols and macros -- no bytes are emitted here.
.include "./{{FOLDER}}/library_flags.asm"
.include "./{{FOLDER}}/sprites.asm"

; Bitmap graphics memory layout -- "if you don't use it you don't pay for
; it," extended from code size (the Flag_X mechanism) to the memory map.
; A flag not set by library_flags.asm falls back to asm/C64Graphics.asm's
; .weak default of 0. A program that calls no graphics method gets the plain
; layout: none of the blocks below emit a byte or reserve anything.
;
; GRAPHICS_HIGH (graphics without double buffering) -- the VIC looks at bank
; 3 ($C000-$FFFF), where it can see RAM the program could not use anyway:
;   $E000-$FF3F  bitmap: the RAM under the KERNAL ROM. Not part of the .prg.
;                (asm/C64Graphics.asm banks the ROM out while it draws.)
;   $D000-$DFFF  sprite frames 0-63: the RAM under the I/O area, filled once
;                at startup (below) from a copy at the end of the .prg, where
;                the object tables and heap then go -- so the frames cost no
;                program memory.
;   $CC00-$CFFF  video matrix (Graphics_ColorMatrix; the sprite pointers are
;                its last 8 bytes, $CFF8)
;   below $CC00  sprite frames 64 and up, if any, loaded in place
;   $1000-...    everything else, in one piece: runtime, program, assets,
;                then the object tables and the heap up to PROGRAM_LIMIT.
;
; GRAPHICS_DOUBLE_BUFFER (Screen.SetDrawBuffer/SwapBuffers) keeps the older
; low layout: buffer 0 in VIC bank 0 (matrix $0C00, bitmap $2000), buffer 1
; in bank 1 (bitmap $4000, matrix $6000), code from $6400. No sprite frame
; storage there.
GRAPHICS_DOUBLE_BUFFER = Flag_Screen_SetDrawBuffer | Flag_Screen_SwapBuffers
GRAPHICS_USED = Flag_Screen_SetScreenMode | Flag_Screen_SetPixel | Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawTrapezoid | Flag_Screen_DrawCircle | Flag_Screen_SetBitmapColors | Flag_Screen_ClearBitmap | Flag_Screen_DrawBitmap | Flag_Screen_DrawImage | Flag_Screen_SetCellColors | Flag_Screen_RemapCellColors | Flag_Screen_MirrorCells | Flag_Screen_MirrorCellColors | Flag_Screen_DrawText | Flag_Screen_DrawTiles | Flag_Screen_SetSplit | GRAPHICS_DOUBLE_BUFFER
GRAPHICS_HIGH = GRAPHICS_USED & (GRAPHICS_DOUBLE_BUFFER ^ 1)

.if GRAPHICS_HIGH
Graphics_Bitmap = $e000
Graphics_ColorMatrix = $cc00
SPRITE_STORE_A = $d000
SPRITE_STORE_B = Graphics_ColorMatrix - 64 * SPRITE_FRAMES_B
PROGRAM_LIMIT = SPRITE_STORE_B
.else
PROGRAM_LIMIT = $d000
.endif

#start_at $1000

; Banks out BASIC ROM -- safe here since the BASIC stub's SYS line has
; already done its one job of jumping into machine code, and gives
; objectTables.asm real RAM all the way to $d000 (I/O) instead of
; stopping at BASIC ROM ($a000). $d000 is the real floor itself --
; objectTables.asm's own .cerror checks the table block's END against
; this value directly, so no pre-baked headroom subtraction belongs here
; (a stale $c800 = "$d000 minus the 2048-byte block" value used to be
; correct back when the check only looked at the block's START; once it
; moved to checking the END, leaving this at $c800 silently cost every
; program built from this template ~2048 bytes of perfectly safe RAM it
; never needed to give up -- matches what UnitTestEntry.asm already does).
#disable_basic_rom
OBJ_TABLES_MAX_START = PROGRAM_LIMIT
OBJ_TABLES_FALLBACK = PROGRAM_LIMIT
; $0800-$0fff (below the code, which starts at $1000) holds only the BASIC
; stub once the program runs: the object tables and localsStack go there
; (asm/helper/objectTables.asm), leaving all the RAM after the code to the
; heap. Not in the double-buffered layout, whose colour matrix is at $0c00.
; Screen.DrawText's font occupies $0800-$0bff: then only half the tables
; fit, at $0c00.
.if !GRAPHICS_DOUBLE_BUFFER
.if Flag_Screen_DrawText
OBJ_TABLES_LOW = $0c00
OBJ_TABLES_LOW_ALL = 0
.else
OBJ_TABLES_LOW = $0800
OBJ_TABLES_LOW_ALL = 1
.endif
.endif

; Object allocation must stay below PROGRAM_LIMIT ($d000, the I/O area, or
; the video matrix / in-place sprite frames in the high graphics layout) --
; see asm/helper/fault.asm's Runtime_CheckHeapRoom.
HEAP_LIMIT = PROGRAM_LIMIT

.if GRAPHICS_HIGH
; The drawing routines run with the KERNAL ROM banked out (see #gfx_ram_on in
; asm/C64Graphics.asm); an interrupt arriving then takes its vector from the
; RAM under the ROM, so that RAM gets vectors of its own.
lda #<Graphics_RamNmi
sta $fffa
lda #>Graphics_RamNmi
sta $fffb
lda #<Graphics_RamIrq
sta $fffe
lda #>Graphics_RamIrq
sta $ffff
.if SPRITE_FRAMES_A > 0
; Sprite frames 0-63 go to the RAM under the I/O area ($D000), which neither
; LOAD nor ordinary code can reach: copied here, once, with everything banked
; out ($01 = $04: RAM everywhere) and interrupts off, from the end of the
; loaded program (Sprite_InitData, below) -- memory that #initHeap then turns
; into the object tables and heap, so this must stay ahead of it. Whole
; pages are copied; the tail of the last one is unused frames. Runs once
; even if the program is RUN again: by then the source has been overwritten.
lda Sprite_Init_Done
bne Sprite_Init_Skip
inc Sprite_Init_Done
sei
lda #$04
sta $01
ldx #(SPRITE_FRAMES_A + 3) / 4
ldy #0
Sprite_Init_Copy
lda Sprite_InitData,y
sta SPRITE_STORE_A,y
iny
bne Sprite_Init_Copy
inc Sprite_Init_Copy+2
inc Sprite_Init_Copy+5
dex
bne Sprite_Init_Copy
lda #$06
sta $01
cli
jmp Sprite_Init_Skip
Sprite_Init_Done .byte 0
Sprite_Init_Skip
.endif
.endif

.if Flag_Screen_DrawText
; Screen.DrawText's font: the character ROM copied into RAM.
jsr Graphics_InitFont
.endif

#initHeap heap

#locals_stack_init

; BASIC ROM float errors (overflow, division by zero...) jump through ($0300);
; point it at our fault handler -- see asm/helper/fault.asm. Put back below,
; before returning to BASIC.
lda #<Runtime_FloatError
sta $0300
lda #>Runtime_FloatError
sta $0301

; Static constructors -- nothing else ever calls a type's .cctor, so any
; static readonly field with a real initializer needs this to run once
; before Program_Main touches it.
{{STATIC_CTORS}}
jsr Program_Main

; Main returned: BASIC's own error handling needs its vector back
; ($e38b is the power-on value).
lda #$8b
sta $0300
lda #$e3
sta $0301

; Main returned. Put BASIC ROM back before returning to the SYS line that
; started us: #disable_basic_rom above turned it into RAM, so without this
; the rts below lands in BASIC's (now RAM) SYS handler, executes garbage,
; and the machine ends up in the reset path -- clearing the screen and
; printing READY. With BASIC mapped back in, control returns to BASIC
; normally: READY. is printed on the next line and whatever the program
; left on the screen stays there. ($37 = LORAM/HIRAM/CHAREN all set, the
; power-on value.)
lda #$37
sta $01
rts

; Real subroutines (not macros), so unlike everything included above they
; DO emit actual code at this exact point in the file -- must come after
; #start_at $1000 (this compiler's own generated code isn't confined below
; $a000, so these need a real, low, stable address here, not wherever the
; default pre-#start_at location counter happens to be -- see
; asm/helper/floatBanking.asm's comment for the full reasoning). Same
; requirement as object.asm's resolveObjPtr just below, which is why this
; sits in the same spot.
.include "./helper/float.asm"
.include "./helper/division.asm"

.include "./system.asm"
.include "./helper/fault.asm"
.include "./GC.asm"
.include "./helper/object.asm"
.include "./helper/frame.asm"
.if GRAPHICS_DOUBLE_BUFFER
; The low, double-buffered layout (see the top of this file). Real
; (non-.virtual) bytes: these areas sit in the MIDDLE of the address space
; with compiled code resuming after them.
.cerror SPRITE_FRAME_COUNT > 0, "[RawSprites] is not available together with double buffering (Screen.SetDrawBuffer/SwapBuffers): the two buffers are in different VIC banks."
; Color matrix: 1000 bytes, 1K-aligned within VIC bank 0. $0c00 sits in the
; otherwise-dead gap between the BASIC boot stub and "* = $1000".
graphics_resume_point = *
* = $0c00
Graphics_ColorMatrix
.fill 1000
* = graphics_resume_point

; Bitmap: 8000 bytes, 8K-aligned within VIC bank 0 -- $2000 is the only such
; address that is both non-zero and outside the VIC-II's character-ROM shadow
; at $1000-$1fff (asm/helper/memoryLayout.asm).
* = $2000
Graphics_Bitmap
.fill 8000
; Second buffer, in VIC bank 1 ($4000-$7fff; CIA2 $dd00 bits 0-1 switch to it
; -- see Screen_SwapBuffers in asm/C64Graphics.asm): the bitmap at bank offset
; $0000, its color matrix at offset $2000, compiled code resuming at $6400.
* = $4000
Graphics_Bitmap2
.fill 8000
* = $6000
Graphics_ColorMatrix2
.fill 1000
* = $6400
.endif

.include "./C64.asm"
.include "./helper/tostring.asm"
.include "./helper/stringops.asm"

.if (GRAPHICS_USED == 0) && (SPRITE_FRAME_COUNT > 0)
; [RawSprites] frames of a program without bitmap graphics: stored in line,
; on a 64-byte boundary the VIC can see in its default bank 0 (below $4000
; and outside the character ROM shadow at $1000-$1fff).
#align_vic_safe 64
SPRITE_STORE_A
#sprite_store_data_a
SPRITE_STORE_B
#sprite_store_data_b
.cerror * > $4000, "[RawSprites] frames end above $4000, outside VIC bank 0 where a text-mode program's sprites must be."
.endif

.include "./{{FOLDER}}/generated.asm"

.if GRAPHICS_HIGH
; Sprite frames 0-63 as loaded; the startup code above moves them under the
; I/O area. The location counter is then put back, so the object tables and
; heap (objectTables.asm, .virtual) start right where this data was.
Sprite_InitData
#sprite_store_data_a
Sprite_InitData_End
.cerror Sprite_InitData_End > PROGRAM_LIMIT, "Compiled program is too large: the sprite frame data at its end runs past the video matrix / in-place sprite frames."
.if SPRITE_FRAMES_B > 0
* = SPRITE_STORE_B
#sprite_store_data_b
.endif
* = Sprite_InitData
.endif

.include "./helper/objectTables.asm"
