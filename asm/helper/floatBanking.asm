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
; SEI/PLP: scoped to exactly this ROM-banked window (SEI here, PLP at the
; end of bank_out_basic_rom below) -- NOT the float macros' whole body
; (addflt, branch_lessflt, etc.), which used to wrap SEI/CLI around
; everything from pulling operands off this compiler's own evaluation
; stack through pushing/branching on the result. That was both broader
; than necessary and, once C64.Interrupt actually worked, actively wrong:
; a macro's own `cli` would prematurely re-enable interrupts if that macro
; were ever called from inside a compiled interrupt handler, permitting a
; nested/reentrant IRQ into a dispatch mechanism not designed for it (see
; asm/C64.asm's OnInterrupt, which now centralizes interrupt-time
; zero-page safety in the ONE place actually positioned to do it
; correctly, instead of every macro defending itself individually).
;
; A blind CLI here would have the exact same problem one level down,
; though, just for a narrower window: if Float_Add/etc. (and so this
; macro pair) is ever called from code running inside a compiled
; interrupt handler, an unconditional `cli` would still prematurely
; re-enable interrupts before that handler -- and OnInterrupt -- has
; returned. PHP/PLP instead of SEI/CLI is what actually nests correctly:
; PLP restores whatever the interrupt-enable flag was *before* this pair
; ran, so it only re-enables interrupts here if they were already enabled
; (ordinary mainline code); called from inside a handler, where hardware
; auto-set the flag on IRQ entry and nothing since has cleared it, PLP
; leaves it exactly as set as it already was.
;
; This window still needs its OWN protection, though, for two reasons SEI
; per macro used to cover for free: (1) the original reason -- an IRQ
; firing while BASIC ROM is banked in, if a handler expects KERNAL/IO in a
; different state, is exactly the kind of intermittent bug worth a few
; cycles of SEI to avoid categorically; (2) OnInterrupt reads
; zp_interrupt_address_low/high (see below) to know where to dispatch --
; if an interrupt landed between the save below and the restore in
; bank_out_basic_rom, while this byte legitimately holds FMULT's clobbered
; value rather than the real subscriber address, OnInterrupt would jump to
; garbage.
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
  php
  sei
  lda zp_interrupt_address_low
  sta zp_flt_saved_interrupt_low
  lda zp_interrupt_address_high
  sta zp_flt_saved_interrupt_high
  lda #$07
  sta $01
.endm

; Preserves A across the whole sequence (pha/pla) -- Float_Compare's
; caller needs FCOMP's result, still sitting in A, to survive this call
; unchanged. PLP must be the very last thing (after A is restored) since
; it's what actually re-enables interrupts (or doesn't -- see above); the
; pha/pla pair nests fine underneath it, same as any other balanced
; push/pop between bank_in_basic_rom's php and this plp.
bank_out_basic_rom .macro
  pha
  lda #$06
  sta $01
  lda zp_flt_saved_interrupt_low
  sta zp_interrupt_address_low
  lda zp_flt_saved_interrupt_high
  sta zp_interrupt_address_high
  pla
  plp
.endm
