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
string_1879048193 .text "hello c64",0
string_1879048529 .text "GET READY",0
string_1879048549 .text "         ",0
string_1879050561 .text "OCEAN SOFTWARE",0
string_1879050591 .text "   PRESENTS   ",0
string_1879050621 .text "   BY J.STEELE",0
string_1879050651 .text "PRESS F1 FOR INSTRUCTIONS",0
string_1879050703 .text "    OR SPACE TO START    ",0
Init_Values_0 .byte <0,<37,<11
Init_Values_1 .byte <3,<0,<11
Init_Values_2 .byte <1,<0,<11
Init_Values_3 .byte <2,<0,<15
Init_Values_4 .byte <2,<21,<15
Init_Values_5 .byte <1,<42,<15
Init_Values_6 .byte <3,<0,<9
Init_Values_7 .byte <2,<37,<9
Init_Values_8 .byte 0,0,0,0,0,0
Init_Values_9 .byte 0,0,0,0,0
Init_Values_10 .byte 0,0,0,0,0,0,0,0
Init_Values_11 .byte <3288,>3288,<10
Init_Values_12 .byte <4389,>4389,<20
Init_Values_13 .byte <5220,>5220,<10
Init_Values_14 .byte <4927,>4927,<10
Init_Values_15 .byte <4927,>4927,<20
Init_Values_16 .byte <4389,>4389,<10
Init_Values_17 .byte <4389,>4389,<30
Init_Values_18 .byte <0,>0,<10
Init_Values_19 .byte <3910,>3910,<10
Init_Values_20 .byte <5220,>5220,<20
Init_Values_21 .byte <6577,>6577,<10
Init_Values_22 .byte <5859,>5859,<10
Init_Values_23 .byte <5859,>5859,<20
Init_Values_24 .byte <5220,>5220,<30
Init_Values_25 .byte <7821,>7821,<10
Init_Values_26 .byte <8779,>8779,<20
Init_Values_27 .byte <6577,>6577,<20
Init_Values_28 .byte <3910,>3910,<20
Init_Values_29 .byte <3484,>3484,<10
Init_Values_30 .byte <5859,>5859,<40
Init_Values_31 .byte <5220,>5220,<50
