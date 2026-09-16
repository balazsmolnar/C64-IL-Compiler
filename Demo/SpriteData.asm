; Single standard-mode (non-multicolor) sprite: 24x21 pixels, 3 bytes/row +
; 1 padding byte (64 bytes total, matching Hunchback/SpriteData.asm's own
; layout convention). Referenced from Demo/Program.cs via
; C64Address.FromLabel("spt_ball").
;
; #align_vic_safe 64 is required, not optional -- Sprite_set_DataBlock
; (asm/c64sprite.asm) turns a data address into a VIC-II sprite pointer by
; dividing it by 64 (6x lsr/ror), so the data MUST start on a 64-byte
; boundary or that division truncates to the wrong block and the sprite
; shows garbage from whatever else landed nearby. Also dodges $1000-$1fff
; (the VIC-II character-ROM-shadow window, which corrupts sprite data
; fetches from that range regardless of alignment) -- see
; asm/helper/memoryLayout.asm's align_vic_safe for both parts. Without
; this, spt_ball's actual address is just "wherever data.asm's other
; content happens to end," which is not reliably 64-byte-aligned.
#align_vic_safe 64
spt_ball:
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $01, $FE
.byte $00, $0F, $FF
.byte $00, $3F, $FF
.byte $00, $FF, $FF
.byte $01, $FF, $FF
.byte $03, $FF, $FF
.byte $07, $FF, $FF
.byte $0F, $FF, $FF
.byte $1F, $FF, $FF
.byte $1F, $FF, $87
.byte $3F, $FE, $00
.byte $3F, $F8, $00
.byte $7F, $F0, $00
.byte $7F, $E0, $00
.byte $7F, $E0, $00
.byte $7F, $C0, $00
.byte $FF, $C0, $00
.byte $FF, $C0, $00
.byte $FF, $C0, $00
.byte $00
