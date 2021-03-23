
pa = $dc00        ;row select
pb = $dc01        ;column read
ddra = $dc02      ;ddr for row select
ddrb = $dc03      ;ddr for column read


C64_IsKeyPressed:
    
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int $34
    sei
    ldx $34

    lda #$ff
    sta ddra
    lda #$00
    sta ddrb
    lda keyRow, x
    sta pa
    lda pb
    and mask, x
    beq +
    #stack_push_int 0
    jmp keysl1
+   #stack_push_int 1
keysl1  cli
    #stack_return_to_saved_address zp_tmp1_low

; https://www.c64-wiki.com/wiki/Keyboard
keyRow:
;     A    B    C    D    E    F    G    H    I    J    K    L    M    N    O    P    Q    R    S    T    U    V    W    X    Y    Z   SPACE
.byte $fd, $f7, $fb, $fb, $fd, $fb, $f7, $f7, $ef, $ef, $ef, $df, $ef, $ef, $ef, $df, $7f, $fb, $fd, $fb, $f7, $f7, $fd, $fb, $f7, $fd, $7F
mask:
.byte $04, $10, $10, $04, $40, $20, $04, $20, $02, $04, $20, $04, $10, $80, $40, $02, $40, $02, $20, $40, $40, $80, $02, $80, $02, $10, $10  
