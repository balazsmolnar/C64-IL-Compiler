spriteEnabled = $D015
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

Sprite_set_Visible:
    +stack_save_return_adress $20
    +stack_pull_int $34
    +stack_pull_int $35
    ldx $35
    
    lda #1
 -  asl
    dex
    bpl -
    lsr
    ora spriteEnabled
    sta spriteEnabled

    lda #80
    sta $D000
    sta $D001

    lda #0
    sta $D010
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
    +stack_pull_int $35
    ldx $35
    lda $34
    sta spriteX,x    
    +stack_return_to_saved_address $20

Sprite_set_Y:
    +stack_save_return_adress $20
    +stack_pull_int $34
    +stack_pull_int $35
    ldx $35
    lda $34
    sta spriteY,x    
    +stack_return_to_saved_address $20