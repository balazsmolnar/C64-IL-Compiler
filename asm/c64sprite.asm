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
