spriteEnabled = $D015
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
    +stack_save_return_adress $20
    +stack_push_int 0
    +stack_return_to_saved_address $20

SpriteCollection_get_Sprite1:
    +stack_save_return_adress $20
    +stack_push_int 1
    +stack_return_to_saved_address $20

SpriteCollection_get_Sprite2:
    +stack_save_return_adress $20
    +stack_push_int 2
    +stack_return_to_saved_address $20

SpriteCollection_get_Sprite3:
    +stack_save_return_adress $20
    +stack_push_int 3
    +stack_return_to_saved_address $20

SpriteCollection_get_Sprite4:
    +stack_save_return_adress $20
    +stack_push_int 4
    +stack_return_to_saved_address $20

SpriteCollection_get_Sprite5:
    +stack_save_return_adress $20
    +stack_push_int 5
    +stack_return_to_saved_address $20

SpriteCollection_get_Sprite6:
    +stack_save_return_adress $20
    +stack_push_int 6
    +stack_return_to_saved_address $20

SpriteCollection_get_Sprite7:
    +stack_save_return_adress $20
    +stack_push_int 7
    +stack_return_to_saved_address $20

SpriteCollection_set_CommonColor1:
    +stack_save_return_adress $20
    +stack_pull_int_a
    sta spriteCommonColor1    
    +stack_return_to_saved_address $20

SpriteCollection_set_CommonColor2:
    +stack_save_return_adress $20
    +stack_pull_int_a
    sta spriteCommonColor2
    +stack_return_to_saved_address $20

Sprite_set_Visible:
    +stack_save_return_adress $20
    +stack_pull_int $34
    +stack_pull_int $35
    ldx $35
    
    lda #1
-   asl
    dex
    bpl -
    lsr
    ora spriteEnabled
    sta spriteEnabled
    +stack_return_to_saved_address $20

Sprite_set_MultiColor:
    +stack_save_return_adress $20
    +stack_pull_int $34
    +stack_pull_int $35
    ldx $35
    
    lda #1
-   asl
    dex
    bpl -
    lsr
    ora spriteMultiColor
    sta spriteMultiColor
    +stack_return_to_saved_address $20

Sprite_set_DataBlock:
    +stack_save_return_adress $20
    +stack_pull_int $34
    +stack_pull_int $35
    ldx $35

    lda #<(sprite0 / 64)
    clc
    adc $34
    sta spriteData,x
    
    +stack_return_to_saved_address $20

Sprite_set_Color:
    +stack_save_return_adress $20
    +stack_pull_int $34
    +stack_pull_int $35
    ldx $35
    lda $34
    sta spriteColor,x    
    +stack_return_to_saved_address $20

Sprite_set_X:
    +stack_save_return_adress $20
    +stack_pull_int $34
    +stack_pull_int_a
    asl
    tax
    lda $34
    sta spriteX,x    
    +stack_return_to_saved_address $20

Sprite_set_Y:
    +stack_save_return_adress $20
    +stack_pull_int $34
    +stack_pull_int_a
    asl
    tax
    lda $34
    sta spriteY,x
    +stack_return_to_saved_address $20