spriteEnabled = $D015
spriteCollision = $D01E
spriteBackgroundCollision = $D01F
spriteMultiColor = $D01C
spriteExtraPosition = $D010
spriteCommonColor1 = $D025
spriteCommonColor2 = $D026
spriteX = $D000
spriteY = $D001
spriteColor = $D027
spriteData = $07F8
spriteExpandX = $D01D
spriteExpandY = $D017

.weak
Flag_C64_get_Sprites = 0
.endweak
.if Flag_C64_get_Sprites

C64_get_Sprites:
    rts
.endif

.weak
Flag_SpriteCollection_get_Sprite0 = 0
.endweak
.if Flag_SpriteCollection_get_Sprite0

SpriteCollection_get_Sprite0:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 0
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_get_Sprite1 = 0
.endweak
.if Flag_SpriteCollection_get_Sprite1

SpriteCollection_get_Sprite1:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 1
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_get_Sprite2 = 0
.endweak
.if Flag_SpriteCollection_get_Sprite2

SpriteCollection_get_Sprite2:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 2
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_get_Sprite3 = 0
.endweak
.if Flag_SpriteCollection_get_Sprite3

SpriteCollection_get_Sprite3:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 3
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_get_Sprite4 = 0
.endweak
.if Flag_SpriteCollection_get_Sprite4

SpriteCollection_get_Sprite4:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 4
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_get_Sprite5 = 0
.endweak
.if Flag_SpriteCollection_get_Sprite5

SpriteCollection_get_Sprite5:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 5
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_get_Sprite6 = 0
.endweak
.if Flag_SpriteCollection_get_Sprite6

SpriteCollection_get_Sprite6:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 6
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_get_Sprite7 = 0
.endweak
.if Flag_SpriteCollection_get_Sprite7

SpriteCollection_get_Sprite7:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 7
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_set_CommonColor1 = 0
.endweak
.if Flag_SpriteCollection_set_CommonColor1

SpriteCollection_set_CommonColor1:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta spriteCommonColor1
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_set_CommonColor2 = 0
.endweak
.if Flag_SpriteCollection_set_CommonColor2

SpriteCollection_set_CommonColor2:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta spriteCommonColor2
    #stack_return_to_saved_address zp_tmp1_low,
.endif

; SpriteCollection.MultiColorMask / BehindBackgroundMask: one bit per sprite
; (bit 0 = sprite 0), written whole to $D01C / $D01B.
.weak
Flag_SpriteCollection_set_MultiColorMask = 0
.endweak
.if Flag_SpriteCollection_set_MultiColorMask

SpriteCollection_set_MultiColorMask:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta spriteMultiColor
    #stack_return_to_saved_address zp_tmp1_low
.endif

; SpriteCollection.VisibleMask: one bit per sprite, $D015 whole.
.weak
Flag_SpriteCollection_set_VisibleMask = 0
.endweak
.if Flag_SpriteCollection_set_VisibleMask

SpriteCollection_set_VisibleMask:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta spriteEnabled
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_get_VisibleMask = 0
.endweak
.if Flag_SpriteCollection_get_VisibleMask

SpriteCollection_get_VisibleMask:
    #stack_save_return_adress zp_tmp1_low
    lda spriteEnabled
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

; SpriteCollection[index]: a Sprite is its number, which is already what the
; caller pushed as the index (see SpriteCollection_get_Sprite0).
.weak
Flag_SpriteCollection_get_Item = 0
.endweak
.if Flag_SpriteCollection_get_Item

SpriteCollection_get_Item:
    rts
.endif

; Sprite.Place(x, y, frame, color): X (9 bits), Y, data pointer and colour of
; one sprite. Stack, top first: color, frame, y, x low, x high, sprite number.
.weak
Flag_Sprite_Place = 0
.endweak
.if Flag_Sprite_Place

Sprite_Place:
    #stack_save_return_adress zp_tmp1_low
    pla
    sta zp_param1_low       ; colour
    pla
    sta zp_param1_high      ; frame
    pla
    sta zp_param2_low       ; y
    pla
    sta zp_param2_high      ; x low
    pla
    tay                     ; x high
    pla
    tax                     ; sprite number
    lda zp_param1_low
    sta spriteColor,x
    lda zp_param1_high
    sta spriteData,x
.if GRAPHICS_HIGH
    sta Graphics_ColorMatrix+$3f8,x
.endif
    lda sprite_bit_table,x
    cpy #0
    beq +
    ora spriteExtraPosition
    bne Sprite_Place_l1
+   eor #$FF
    and spriteExtraPosition
Sprite_Place_l1:
    sta spriteExtraPosition
    txa
    asl
    tax
    lda zp_param2_high
    sta spriteX,x
    lda zp_param2_low
    sta spriteY,x
    #stack_return_to_saved_address zp_tmp1_low
.endif

; SpriteCollection.PlaceGroup(first, count, x, y, parts, offset): `count`
; hardware sprites from sprite `first` on, placed as one object at (x, y).
; Each sprite is four bytes of the byte array `parts`, starting at `offset`:
; dx (0-255, added to x), dy (added to y, wrapping), frame (the data
; pointer, as Sprite.Frame), colour (bit 7 set = multicolour, as in a .spr
; frame's last byte). Sets X (9 bits), Y, pointer, colour and the
; multicolour bit; which sprites are shown is the caller's (VisibleMask).
; Stack, top first: offset, parts, y, x low, x high, count, first.
.weak
Flag_SpriteCollection_PlaceGroup = 0
.endweak
.if Flag_SpriteCollection_PlaceGroup

SpriteCollection_PlaceGroup:
    #stack_save_return_adress zp_tmp1_low
    pla
    sta zp_param0_low           ; offset
    pla
    tax
    jsr resolveObjPtr           ; tmpPointer = the array's bytes
    clc
    lda tmpPointer
    adc zp_param0_low
    sta zp_param3_low
    lda tmpPointer+1
    adc #0
    sta zp_param3_high          ; -> the first sprite's four bytes
    pla
    sta zp_param2_low           ; y
    pla
    sta zp_param1_low           ; x low
    pla
    sta zp_param1_high          ; x high
    pla
    sta zp_param2_high          ; count
    pla
    tax                         ; sprite number
    ldy #0
SpriteCollection_PlaceGroup_next:
    lda zp_param2_high
    beq SpriteCollection_PlaceGroup_done
    dec zp_param2_high
    txa
    asl
    sta zp_param0_low           ; sprite number * 2: its X/Y register pair
    clc
    lda (zp_param3_low),y       ; dx
    iny
    adc zp_param1_low
    pha
    lda zp_param1_high
    adc #0
    lsr                         ; bit 8 of x -> carry
    lda sprite_bit_table,x
    bcs SpriteCollection_PlaceGroup_xhigh
    eor #$FF
    and spriteExtraPosition
    bcc SpriteCollection_PlaceGroup_xset
SpriteCollection_PlaceGroup_xhigh:
    ora spriteExtraPosition
SpriteCollection_PlaceGroup_xset:
    sta spriteExtraPosition
    clc
    lda (zp_param3_low),y       ; dy
    iny
    adc zp_param2_low
    stx zp_param4_low
    ldx zp_param0_low
    sta spriteY,x
    pla
    sta spriteX,x
    ldx zp_param4_low
    lda (zp_param3_low),y       ; frame
    iny
    sta spriteData,x
.if GRAPHICS_HIGH
    sta Graphics_ColorMatrix+$3f8,x
.endif
    lda (zp_param3_low),y       ; colour; bit 7 = multicolour
    iny
    sta spriteColor,x
    asl
    lda sprite_bit_table,x
    bcs SpriteCollection_PlaceGroup_multi
    eor #$FF
    and spriteMultiColor
    bcc SpriteCollection_PlaceGroup_mset
SpriteCollection_PlaceGroup_multi:
    ora spriteMultiColor
SpriteCollection_PlaceGroup_mset:
    sta spriteMultiColor
    inx
    bne SpriteCollection_PlaceGroup_next
SpriteCollection_PlaceGroup_done:
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_set_BehindBackgroundMask = 0
.endweak
.if Flag_SpriteCollection_set_BehindBackgroundMask

SpriteCollection_set_BehindBackgroundMask:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta $d01b
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_SpriteCollection_get_Collisions = 0
.endweak
.if Flag_SpriteCollection_get_Collisions

SpriteCollection_get_Collisions:
    #stack_save_return_adress zp_tmp1_low
    lda spriteCollision
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

sprite_bit_table: .byte $01, $02, $04, $08, $10, $20, $40, $80

.weak
Flag_Sprite_set_Visible = 0
.endweak
.if Flag_Sprite_set_Visible

Sprite_set_Visible:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x

    lda sprite_bit_table, x
    cpy #1
    bne +
    ora spriteEnabled
    jmp Sprite_set_Visible_l1
+   eor #$FF
    and spriteEnabled
Sprite_set_Visible_l1:
    sta spriteEnabled
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_get_IsInCollision = 0
.endweak
.if Flag_Sprite_get_IsInCollision

Sprite_get_IsInCollision:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x

    lda sprite_bit_table, x
    and spriteCollision
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_get_IsInBackgroundCollision = 0
.endweak
.if Flag_Sprite_get_IsInBackgroundCollision

Sprite_get_IsInBackgroundCollision:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x

    lda sprite_bit_table, x
    and spriteBackgroundCollision
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_set_MultiColor = 0
.endweak
.if Flag_Sprite_set_MultiColor

Sprite_set_MultiColor:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    #stack_pull_int_x

    lda sprite_bit_table, x
    ora spriteMultiColor
    sta spriteMultiColor
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_set_HighPosition = 0
.endweak
.if Flag_Sprite_set_HighPosition

Sprite_set_HighPosition:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x

    lda sprite_bit_table, x
    cpy #1
    bne +
    ora spriteExtraPosition
    jmp Sprite_set_HighPosition_l1
+   eor #$FF
    and spriteExtraPosition
Sprite_set_HighPosition_l1:
    sta spriteExtraPosition
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_set_ExpandX = 0
.endweak
.if Flag_Sprite_set_ExpandX

Sprite_set_ExpandX:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x

    lda sprite_bit_table, x
    cpy #1
    bne +
    ora spriteExpandX
    jmp Sprite_set_ExpandX_l1
+   eor #$FF
    and spriteExpandX
Sprite_set_ExpandX_l1:
    sta spriteExpandX
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_set_ExpandY = 0
.endweak
.if Flag_Sprite_set_ExpandY

Sprite_set_ExpandY:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x

    lda sprite_bit_table, x
    cpy #1
    bne +
    ora spriteExpandY
    jmp Sprite_set_ExpandY_l1
+   eor #$FF
    and spriteExpandY
Sprite_set_ExpandY_l1:
    sta spriteExpandY
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_set_DataBlock = 0
.endweak
.if Flag_Sprite_set_DataBlock

Sprite_set_DataBlock:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int zp_param2_low
    #stack_pull_int_x
    tya

    lsr zp_param2_low
    ror
    lsr zp_param2_low
    ror
    lsr zp_param2_low
    ror
    lsr zp_param2_low
    ror
    lsr zp_param2_low
    ror
    lsr zp_param2_low
    ror
    sta spriteData,x
.if GRAPHICS_HIGH
    sta Graphics_ColorMatrix+$3f8,x
.endif

    #stack_return_to_saved_address zp_tmp1_low
.endif

; Sprite.Frame: the sprite's data pointer itself -- a block number, the
; frame's address / 64 within the VIC's 16K bank (what C64Address.FromLabel(
; "frm_<Label>") gives for a [RawSprites] frame; frames of one sheet are
; consecutive). The VIC reads the eight pointers from the last 8 bytes of
; whichever 1K it is using as the video matrix: the text screen's ($07F8),
; and in bitmap mode (the high layout) Graphics_ColorMatrix's. Both are
; written, so a frame set in one mode is still right after switching.
.weak
Flag_Sprite_set_Frame = 0
.endweak
.if Flag_Sprite_set_Frame

Sprite_set_Frame:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x
    tya
    sta spriteData,x
.if GRAPHICS_HIGH
    sta Graphics_ColorMatrix+$3f8,x
.endif
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_set_Color = 0
.endweak
.if Flag_Sprite_set_Color

Sprite_set_Color:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x
    tya
    sta spriteColor,x
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_set_X = 0
.endweak
.if Flag_Sprite_set_X

Sprite_set_X:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int zp_param1_low
    #stack_pull_int_a
    asl
    tax
    tya
    sta spriteX,x
    txa
    lsr
    tay
    lda sprite_bit_table, y
    ldy zp_param1_low
    beq +
    ora spriteExtraPosition
    jmp Sprite_set_X_l1
+   eor #$FF
    and spriteExtraPosition
Sprite_set_X_l1:
    sta spriteExtraPosition
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Sprite_set_Y = 0
.endweak
.if Flag_Sprite_set_Y

Sprite_set_Y:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_a
    asl
    tax
    tya
    sta spriteY,x
    #stack_return_to_saved_address zp_tmp1_low
.endif
