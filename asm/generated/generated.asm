.include "./data.asm"
.include "./Program.asm"
.include "./Delay.asm"
.include "./Enemy.asm"
.include "./Game.asm"
.include "./GameObject.asm"
.include "./Knight.asm"
.include "./LevelDescription.asm"
.include "./LevelPlay.asm"
.include "./Player.asm"
.include "./PlayerStats.asm"
.include "./Rope.asm"
.include "./Screen.asm"
.include "./TitleScreen.asm"
.include "./Wall.asm"
.include "./Tone.asm"
string_1879048193 .text ".3C0F   A4 24      LDY STACKPOINTER",0
string_1879048265 .text ".3C11   B9 FF C6   LDA LOCALSSTACK-1,Y",0
string_1879048343 .text ".3C14   48         PHA",0
string_1879048389 .text ".3C15   A4 24      LDY STACKPOINTER",0
string_1879048461 .text ".3C17   B9 FE C6   LDA LOCALSSTACK-2,Y",0
string_1879048539 .text ".3C1A   48         PHA",0
string_1879048585 .text ".3C1B   68         PLA",0
string_1879048631 .text ".3C1C   85 32      STA $32",0
string_1879048685 .text ".3C1E   68         PLA",0
string_1879048731 .text ".3C1F   18         CLC",0
string_1879048777 .text ".3C20   65 32      ADC $32",0
string_1879048831 .text ".3C22   48         PHA",0
string_1879049193 .text "GET READY",0
string_1879049213 .text "         ",0
string_1879051225 .text "OCEAN SOFTWARE",0
string_1879051255 .text "   PRESENTS   ",0
string_1879051285 .text "   BY J.STEELE",0
string_1879051315 .text "PRESS F1 FOR INSTRUCTIONS",0
string_1879051367 .text "    OR SPACE TO START    ",0
Init_Values_0 .byte 0,37,11
Init_Values_1 .byte 3,0,11
Init_Values_2 .byte 1,0,11
Init_Values_3 .byte 2,0,15
Init_Values_4 .byte 2,21,15
Init_Values_5 .byte 1,42,15
Init_Values_6 .byte 3,0,9
Init_Values_7 .byte 2,37,9
Init_Values_8 .byte 0,0,0,0,0,0
Init_Values_9 .byte 0,0,0,0,0
Init_Values_10 .byte 0,0,0,0,0,0,0,0
Init_Values_11 .byte 216,12,10
Init_Values_12 .byte 37,17,20
Init_Values_13 .byte 100,20,10
Init_Values_14 .byte 63,19,10
Init_Values_15 .byte 63,19,20
Init_Values_16 .byte 37,17,10
Init_Values_17 .byte 37,17,30
Init_Values_18 .byte 0,0,10
Init_Values_19 .byte 70,15,10
Init_Values_20 .byte 100,20,20
Init_Values_21 .byte 177,25,10
Init_Values_22 .byte 227,22,10
Init_Values_23 .byte 227,22,20
Init_Values_24 .byte 100,20,30
Init_Values_25 .byte 141,30,10
Init_Values_26 .byte 75,34,20
Init_Values_27 .byte 177,25,20
Init_Values_28 .byte 70,15,20
Init_Values_29 .byte 156,13,10
Init_Values_30 .byte 227,22,40
Init_Values_31 .byte 100,20,50
