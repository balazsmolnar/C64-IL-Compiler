
; Landed on a 64-byte boundary only by coincidence before (CharSet.asm's
; preceding 2048-byte block happens to be a multiple of 64) -- fragile if
; charset size or resource order ever changes. Make it explicit. Every
; other sprite this port uses now comes from SpriteImport.cs's
; RawSpriteAttribute entries (sourced directly from the original game's
; own sprHunchback.spt project file -- see ILRawAssemblyPass), which don't
; need their own align directive: each one is already a full, individually
; 64-byte-aligned block by construction, so they can follow this one in
; any order without needing it repeated.
#align_vic_safe 64

; New sprite -- hand-authored for LevelPlay.cs's Esmeralda's-Tower finale
; (the original never had a heart sprite -- Esmeralda's Tower's finale
; existed in the original, but nothing in sprHunchback.spt matches this
; shape; see LevelPlay.cs's own comment on RescueEsmeralda/
; DrawEsmereldaTower for the full story). Single color mode (not
; multicolor, unlike most sprites here) -- simpler byte encoding (1 bit =
; 1 pixel, no bit-pair decoding) for a hand-drawn shape, and this port
; already has single-color sprite precedent (spt_rope_* imports).
; Classic symmetric heart: two rounded lobes narrowing to a point.
spt_heart:
.byte $1E, $1E, $00
.byte $3F, $3F, $00
.byte $7F, $FF, $80
.byte $7F, $FF, $80
.byte $7F, $FF, $80
.byte $7F, $FF, $80
.byte $3F, $FF, $00
.byte $3F, $FF, $00
.byte $1F, $FE, $00
.byte $1F, $FE, $00
.byte $0F, $FC, $00
.byte $0F, $FC, $00
.byte $07, $F8, $00
.byte $07, $F8, $00
.byte $03, $F0, $00
.byte $03, $F0, $00
.byte $01, $E0, $00
.byte $01, $E0, $00
.byte $00, $C0, $00
.byte $00, $C0, $00
.byte $00, $00, $00
.byte $00
