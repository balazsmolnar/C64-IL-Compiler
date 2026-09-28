using C64Lib;

// Bat sprite art: 3 wing poses (mid / up / down), 24x21, hi-res sprites.
//
// Placed at FIXED addresses, once per VIC bank, because with double
// buffering the two bitmaps live in different banks and a sprite's data is
// fetched from whichever bank the VIC-II is showing:
//   bank 0 (buffer 0): $3F40, $3F80, $3FC0 -- the 192 free bytes right
//     after Graphics_Bitmap ($2000-$3F3F), pointer values $FD, $FE, $FF;
//   bank 1 (buffer 1): $5F40, $5F80, $5FC0 -- the same gap after
//     Graphics_Bitmap2 ($4000-$5F3F), pointer values $7D, $7E, $7F
//     (address within the bank / 64).
// Nothing else writes there (Graphics_ClearBitmap only clears 8000 bytes).
// Program.cs sets the pointer bytes, which in bitmap mode live at
// (color matrix + $3F8), not the text screen's $07F8.
//
// The `* =` jumps are the same trick ProgramEntry.asm uses for the bitmap
// and matrix reservations: emit at a fixed address, then resume where the
// assembler was.
[assembly: RawAssembly(Order = 0, Asm = @"
bat_mid .macro
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00,$24,$00
    .byte $00,$3C,$00
    .byte $00,$7E,$00
    .byte $01,$FF,$80
    .byte $0F,$FF,$F0
    .byte $7F,$FF,$FE
    .byte $FF,$FF,$FF
    .byte $FB,$7E,$DF
    .byte $C3,$FF,$C3
    .byte $01,$FF,$80
    .byte $00,$FF,$00
    .byte $00,$7E,$00
    .byte $00,$3C,$00
    .byte $00,$18,$00
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00
.endm
bat_up .macro
    .byte $00,$00,$00
    .byte $80,$00,$01
    .byte $C0,$00,$03
    .byte $E0,$24,$07
    .byte $F0,$3C,$0F
    .byte $F8,$7E,$1F
    .byte $F8,$FF,$1F
    .byte $FD,$FF,$BF
    .byte $7F,$FF,$FE
    .byte $3F,$FF,$FC
    .byte $1F,$FF,$F8
    .byte $0F,$FF,$F0
    .byte $07,$FF,$E0
    .byte $03,$FF,$C0
    .byte $01,$FF,$80
    .byte $00,$FF,$00
    .byte $00,$7E,$00
    .byte $00,$3C,$00
    .byte $00,$18,$00
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00
.endm
bat_down .macro
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00,$24,$00
    .byte $00,$3C,$00
    .byte $00,$FF,$00
    .byte $07,$FF,$E0
    .byte $3F,$FF,$FC
    .byte $7F,$FF,$FE
    .byte $FF,$FF,$FF
    .byte $FF,$FF,$FF
    .byte $F7,$FF,$EF
    .byte $E1,$FF,$87
    .byte $C1,$FF,$83
    .byte $80,$FF,$01
    .byte $80,$7E,$01
    .byte $00,$3C,$00
    .byte $00,$18,$00
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00
.endm
bat_sprites_resume = *
* = $3F40
    #bat_mid
    #bat_up
    #bat_down
* = $5F40
    #bat_mid
    #bat_up
    #bat_down
* = bat_sprites_resume
")]
