; Temporarily borrows BASIC ROM back for a single ROM float routine call --
; asm/helper/banking.asm's #disable_basic_rom banks it out permanently, once,
; right after the BASIC stub hands off, and nothing else ever banks it back
; in.
;
; #bank_in_basic_rom / #bank_out_basic_rom must ONLY ever be used inside
; asm/helper/float.asm's own Float_* subroutines, NEVER inlined directly
; into a macro that gets expanded at arbitrary call sites throughout the
; program. Reason: while BASIC ROM is banked in, $a000-$bfff reads as ROM,
; not RAM -- and this compiler's own generated code is NOT confined below
; $a000 (Hunchback's compiled output already spans past $a000, since that
; whole range is ordinary free RAM for the entire rest of the program's
; life). If the bank-in/call-ROM/bank-out sequence were inlined into a
; macro, every one of its (potentially hundreds of) expansion sites would
; carry a copy of "sta $01" -- and any copy that happened to land inside
; $a000-$bfff would corrupt its own next instruction fetch the instant it
; ran. Keeping the actual bank-toggling code in real subroutines (jsr'd
; from float.asm's macros, the same way heap.asm's macros jsr a fixed
; resolveObjPtr instead of inlining it) pins it to ONE stable, low address
; -- guaranteed below $a000 by include order (this file loads immediately
; after banking.asm, long before generated.asm's bulk).
;
; SEI/CLI: NOT done here -- asm/helper/float.asm's own macros (addflt,
; branch_lessflt, etc., the actual call sites) own SEI/CLI instead, each
; wrapping its ENTIRE body (pulling operands off this compiler's own
; evaluation stack, through the ROM call, to pushing/branching on the
; result), not just the bank_in_basic_rom/bank_out_basic_rom pair. (A
; suspected live-VICE crash briefly motivated widening this from "just the
; ROM call" to "the whole operation" -- turned out to be a test-harness
; mistake, not a real bug: the program had simply finished and returned to
; BASIC normally, which looks identical to a crash in a single screenshot
; unless the test also proves the program is still running, e.g. via an
; infinite loop. Kept anyway since it's harmless and one less thing to
; reconsider later.)
;
; zp_interrupt_address save/restore: confirmed empirically (a full
; zero-page diff survey across every ROM routine this project calls -- see
; SimpleEmulator.Test's DEBUG_ZeroPageFootprintOfEveryRoutine) that FMULT
; clobbers $26/$27, this program's zp_interrupt_address (see zeropage.asm)
; -- BASIC's zero page usage is extensive and not something this project
; can fully enumerate ahead of time, so this is saved/restored defensively
; rather than chasing down every routine's exact footprint by hand.
; stackPointer had the exact same problem (FCOMP clobbers $24) but was
; fixed by relocating it instead, since nothing else references that
; address by number -- zp_interrupt_address's mechanism spans too much
; other code (asm/C64.asm, any C64Lib interrupt subscriber) to safely
; relocate the same way.
bank_in_basic_rom .macro
  lda zp_interrupt_address_low
  sta zp_flt_saved_interrupt_low
  lda zp_interrupt_address_high
  sta zp_flt_saved_interrupt_high
  lda #$07
  sta $01
.endm

; Preserves A across the whole sequence (pha/pla) -- Float_Compare's
; caller needs FCOMP's result, still sitting in A, to survive this call
; unchanged.
bank_out_basic_rom .macro
  pha
  lda #$06
  sta $01
  lda zp_flt_saved_interrupt_low
  sta zp_interrupt_address_low
  lda zp_flt_saved_interrupt_high
  sta zp_interrupt_address_high
  pla
.endm
