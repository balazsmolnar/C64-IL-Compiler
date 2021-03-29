.enc "screen"

.include "./helper/loader.asm"
.include "./helper/stack.asm"
.include "./helper/localsstack.asm"
.include "./helper/heap.asm"
.include "./helper/arithmetic.asm"
.include "./helper/branch.asm"
.include "./helper/zp.asm"
.include "./helper/optimized.asm"

.include "./helper/8bit.asm"

#start_at $1000
#initHeap heap

#locals_stack_init

; jsr Sound_QuasiJump
jsr Program_Main
rts

; SND_PULSE_GATE_ON               = 65
; SND_PULSE_GATE_OFF              = 64
; VCREG1          = $D404 ;V1 control register
; FREL1           = $D400 ;V1 frequency low-byte
; SIDVOL          = $D418 ;Volume

; Sound_QuasiJump
;         ldx #6
; QuasiJumpSoundLoop
;         lda #15
;         sta SIDVOL
;         lda tbl_QuasiJumpFreqLo,X
;         sta FREL1,X
;         dex
;         bpl QuasiJumpSoundLoop
;         lda #SND_PULSE_GATE_ON
;         sta VCREG1
;         lda #SND_PULSE_GATE_OFF
;         sta VCREG1
;         rts
; tbl_QuasiJumpFreqLo     .byte $00, $04, $28, $00, $40, $00, $09

.include "./system.asm"
.include "./GC.asm"
.include "./helper/object.asm"
.include "./C64.asm"

.include "./generated/generated.asm"


objTableLow
.fill 256, 0
objTableHigh
.fill 256, 0
objTableSize
.fill 256, 0
objTableReferences
.fill 256, 0
objTableRootCount
.fill 256, 0

localsStack
.fill 256, 0 
heap

