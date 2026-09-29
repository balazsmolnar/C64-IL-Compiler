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

#start_at $1000

; Banks out BASIC ROM -- safe here since the BASIC stub's SYS line has
; already done its one job of jumping into machine code, and gives
; objectTables.asm real RAM all the way to $d000 (I/O) instead of
; stopping at BASIC ROM ($a000). $c800 = $d000-$0800, exactly enough
; headroom for the full 2048-byte table block.
#disable_basic_rom
OBJ_TABLES_MAX_START = $c800
OBJ_TABLES_FALLBACK = $c800

; Object allocation must stay below $d000 (I/O) -- see asm/helper/fault.asm's
; Runtime_CheckHeapRoom.
HEAP_LIMIT = $d000

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
.include "./{{FOLDER}}/library_flags.asm"

; Hi-res bitmap graphics reservation -- "if you don't use it you don't pay
; for it," extended from code size (the Flag_X mechanism above) to the
; memory layout itself. Flag_Screen_* here is already resolved by
; library_flags.asm (just included) if any graphics routine is actually
; called, else falls back to asm/C64Graphics.asm's own .weak defaults of 0
; -- same include-order reasoning C64_Set_Screen_Ptr's own comment
; (asm/C64.asm) already relies on. A program that never calls any graphics
; method gets today's layout exactly, byte for byte -- this whole block
; emits zero bytes and defines zero labels in that case.
; Double buffering (Screen.SetDrawBuffer/SwapBuffers) adds a second bitmap +
; color matrix in VIC bank 1, below; Screen.SetBitmapColors alone just needs
; the normal single buffer.
GRAPHICS_DOUBLE_BUFFER = Flag_Screen_SetDrawBuffer | Flag_Screen_SwapBuffers
GRAPHICS_USED = Flag_Screen_SetScreenMode | Flag_Screen_SetPixel | Flag_Screen_DrawLine | Flag_Screen_DrawRectangle | Flag_Screen_DrawCircle | Flag_Screen_SetBitmapColors | Flag_Screen_ClearBitmap | GRAPHICS_DOUBLE_BUFFER
.if GRAPHICS_USED
; Color matrix: 1000 bytes (one per 8x8 cell, hi nibble=foreground/lo
; nibble=background), must be 1K-aligned within VIC bank 0. $0c00 sits in
; the otherwise-dead gap between the BASIC boot stub (asm/helper/
; loader.asm's start_at macro: "* = $0801" + ~12 bytes) and "* = $1000"
; where compiled code starts -- genuinely free today regardless of
; graphics use, so this costs nothing even before the .if above.
graphics_resume_point = *
* = $0c00
Graphics_ColorMatrix
.fill 1000
* = graphics_resume_point

; Bitmap: 8000 bytes, must be 8K-aligned within VIC bank 0 -- $2000 is the
; only such address that's both non-zero (avoiding zero page/stack/screen/
; sprite-pointer territory at $0000) and outside the VIC-II's own
; character-ROM shadow at $1000-$1fff (asm/helper/memoryLayout.asm,
; confirmed live in VICE). NOT $4000 -- that's VIC BANK 1, not bank 0 (each
; bank is 16K), invisible to the VIC-II while bank 0 is selected, which it
; is by default -- double buffering (Screen.SetDrawBuffer/SwapBuffers,
; below) is the one thing that switches to bank 1, and only transiently,
; to show whichever buffer isn't the current draw target.
; Real (non-.virtual) bytes: this sits in the MIDDLE of the address space
; with real compiled code resuming right after it, unlike
; asm/helper/objectTables.asm's .virtual reservation, which is only
; correct because it's the very LAST thing in the file (.virtual/.endv
; rewinds * back afterward, which would let code emitted after THIS
; reservation alias straight back over these addresses).
;
; Known collision, not resolved automatically: Hunchback's own custom
; character set (Hunchback/CharSet.asm, #align_vic_safe 2048) is ALSO
; placed at exactly $2000. A program using both a charset raw-asm resource
; targeting $2000 and this graphics feature would collide -- moot today
; since Hunchback doesn't have the headroom for graphics mode at all (see
; the plan this was built from), but worth knowing if that ever changes.
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

.include "./helper/objectTables.asm"
