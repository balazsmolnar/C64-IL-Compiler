; Banks out BASIC ROM ($a000-$bfff), reclaiming it as usable RAM: writes
; $06 to the CPU I/O port ($01) -- LORAM=0 (BASIC ROM out), HIRAM=1
; (KERNAL stays mapped: this codebase's IRQ chaining in asm/C64.asm's
; C64_add_Interrupt/OnInterrupt falls through to whatever was previously
; at $0314/$0315 -- KERNAL's default handler under normal boot -- and
; asm/system.asm calls KERNAL CHROUT ($ffd2) directly; both need KERNAL
; mapped), CHAREN=1 (I/O stays visible at $d000-$dfff, not character ROM).
; Safe to call once the BASIC stub's SYS line has already done its one
; job of jumping into machine code -- BASIC is never re-entered
; afterward, so losing it costs nothing.
disable_basic_rom .macro
lda #$06
sta $01
.endm
