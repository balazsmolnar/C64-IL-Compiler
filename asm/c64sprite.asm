spriteEnabled = $D015
spriteCollision = $D01E
spriteMultiColor = $D01C
spriteCommonColor1 = $D025
spriteCommonColor2 = $D026
spriteX = $D000
spriteY = $D001
spriteColor = $D027
spriteData = $07F8

C64_get_Sprites:
    rts

SpriteCollection_get_Sprite0:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 0
    #stack_return_to_saved_address zp_tmp1_low

SpriteCollection_get_Sprite1:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 1
    #stack_return_to_saved_address zp_tmp1_low

SpriteCollection_get_Sprite2:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 2
    #stack_return_to_saved_address zp_tmp1_low

SpriteCollection_get_Sprite3:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 3
    #stack_return_to_saved_address zp_tmp1_low

SpriteCollection_get_Sprite4:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 4
    #stack_return_to_saved_address zp_tmp1_low

SpriteCollection_get_Sprite5:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 5
    #stack_return_to_saved_address zp_tmp1_low

SpriteCollection_get_Sprite6:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 6
    #stack_return_to_saved_address zp_tmp1_low

SpriteCollection_get_Sprite7:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int 7
    #stack_return_to_saved_address zp_tmp1_low

SpriteCollection_set_CommonColor1:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta spriteCommonColor1    
    #stack_return_to_saved_address zp_tmp1_low

SpriteCollection_set_CommonColor2:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    sta spriteCommonColor2
    #stack_return_to_saved_address zp_tmp1_low

Sprite_set_Visible:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    #stack_pull_int_x
    
    lda #1
-   asl
    dex
    bpl -
    lsr
    ora spriteEnabled
    sta spriteEnabled
    #stack_return_to_saved_address zp_tmp1_low

Sprite_get_IsInCollision:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    
    lda #1
-   asl
    dex
    bpl -
    lsr
    and spriteCollision
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low

Sprite_set_MultiColor:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_a
    #stack_pull_int_x
    
    lda #1
-   asl
    dex
    bpl -
    lsr
    ora spriteMultiColor
    sta spriteMultiColor
    #stack_return_to_saved_address zp_tmp1_low

Sprite_set_DataBlock:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int $34
    #stack_pull_int_y
    #stack_pull_int_x
    tya

    lsr $34
    ror
    lsr $34
    ror
    lsr $34
    ror    
    lsr $34
    ror    
    lsr $34
    ror    
    lsr $34
    ror    

    sta $400

    sta spriteData,x
    
    #stack_return_to_saved_address zp_tmp1_low

Sprite_set_Color:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_x
    tya
    sta spriteColor,x    
    #stack_return_to_saved_address zp_tmp1_low

Sprite_set_X:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_a
    asl
    tax
    tya
    sta spriteX,x    
    #stack_return_to_saved_address zp_tmp1_low

Sprite_set_Y:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_y
    #stack_pull_int_a
    asl
    tax
    tya
    sta spriteY,x
    #stack_return_to_saved_address zp_tmp1_low
