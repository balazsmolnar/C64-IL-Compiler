; Rounds * up to a boundary, then dodges $1000-$1fff -- the VIC-II
; character-ROM-shadow window that intercepts ANY memory fetch landing
; there (confirmed live in VICE this session for BOTH charset data and
; sprite data, not just character-generator lookups) while VIC bank 0 is
; selected. This codebase never switches VIC bank (no write to CIA2
; $dd00 anywhere in asm/), so that's the only window ever worth dodging.
;
; boundary must be a power of two: 2048 for charset data (the VIC-II
; charset base register only selects 2KB-aligned addresses), 64 for
; sprite data (a sprite data pointer is a single byte, address>>6 -- see
; Sprite_set_DataBlock in asm/c64sprite.asm).
align_vic_safe .macro boundary
* = (* + (\boundary - 1)) & (~(\boundary - 1) & $ffff)
.if * >= $1000 && * < $2000
* = $2000
.endif
.endm
