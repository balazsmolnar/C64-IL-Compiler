.include "./data.asm"
.include "./Program.asm"
.include "./Delay.asm"
.include "./Enemy.asm"
.include "./Game.asm"
.include "./Knight.asm"
.include "./WallType.asm"
.include "./EnemyType.asm"
.include "./LevelDescription.asm"
.include "./LevelPlay.asm"
.include "./Player.asm"
.include "./PlayerStats.asm"
.include "./Rope.asm"
.include "./Screen.asm"
.include "./TitleScreen.asm"
.include "./Wall.asm"
string_1879048193 .text "hello world!",0
string_1879048219 .text "spt_arrow_right",0
string_1879048251 .text "spt_fireball_0",0
string_1879048281 .text "spt_fireball_1",0
string_1879048311 .text "spt_fireball_2",0
string_1879048341 .text "spt_fireball_3",0
string_1879048371 .text "charset",0
string_1879048387 .text "spt_knight_climb_0",0
string_1879048425 .text "spt_knight_climb_1",0
string_1879048463 .text "spt_knight_walk_0",0
string_1879048499 .text "spt_knight_walk_1",0
string_1879048535 .text "GET READY",0
string_1879048555 .text "         ",0
string_1879048575 .text "spt_player_right_0",0
string_1879048613 .text "spt_player_rope_left",0
string_1879048655 .text "spt_player_rope_right",0
string_1879048699 .text "spt_player_jump_left_0",0
string_1879048745 .text "spt_player_jump_left_1",0
string_1879048791 .text "spt_player_jump_left_2",0
string_1879048837 .text "spt_player_jump_right_0",0
string_1879048885 .text "spt_player_jump_right_1",0
string_1879048933 .text "spt_player_jump_right_2",0
string_1879048981 .text "spt_player_left_0",0
string_1879049017 .text "spt_player_left_3",0
string_1879049053 .text "spt_player_left_1",0
string_1879049089 .text "spt_player_left_2",0
string_1879049125 .text "spt_player_right_3",0
string_1879049163 .text "spt_player_right_1",0
string_1879049201 .text "spt_player_right_2",0
string_1879049239 .text "txt_PlayerStats",0
string_1879049271 .text "tbl_PlayerStatsColours",0
string_1879049317 .text "spt_rope_14",0
string_1879049341 .text "spt_rope_15",0
string_1879049365 .text "spt_rope_12",0
string_1879049389 .text "spt_rope_13",0
string_1879049413 .text "spt_rope_10",0
string_1879049437 .text "spt_rope_11",0
string_1879049461 .text "spt_rope_8",0
string_1879049483 .text "spt_rope_9",0
string_1879049505 .text "spt_rope_6",0
string_1879049527 .text "spt_rope_7",0
string_1879049549 .text "spt_rope_4",0
string_1879049571 .text "spt_rope_5",0
string_1879049593 .text "spt_rope_2",0
string_1879049615 .text "spt_rope_3",0
string_1879049637 .text "spt_rope_0",0
string_1879049659 .text "spt_rope_1",0
string_1879049681 .text "spt_rope_16",0
string_1879049705 .text "spt_rope_17",0
string_1879049729 .text "spt_rope_18",0
string_1879049753 .text "spt_rope_19",0
string_1879049777 .text "spt_rope_20",0
string_1879049801 .text "spt_rope_21",0
string_1879049825 .text "spt_rope_22",0
string_1879049849 .text "spt_rope_23",0
string_1879049873 .text "spt_rope_24",0
string_1879049897 .text "spt_rope_25",0
string_1879049921 .text "spt_rope_26",0
string_1879049945 .text "spt_rope_27",0
string_1879049969 .text "spt_rope_28",0
string_1879049993 .text "spt_rope_29",0
string_1879050017 .text "spt_rope_30",0
string_1879050041 .text "spt_rope_31",0
string_1879050065 .text "(screenMemory)",0
string_1879050095 .text "(screenMemory+$100)",0
string_1879050135 .text "(screenMemory+$200)",0
string_1879050175 .text "(screenMemory+$300)",0
string_1879050215 .text "(colorMemory)",0
string_1879050243 .text "(colorMemory+$100)",0
string_1879050281 .text "(colorMemory+$200)",0
string_1879050319 .text "(colorMemory+$300)",0
string_1879050357 .text "(screenMemory+$CD)",0
string_1879050395 .text "titleScreen",0
string_1879050419 .text "(screenMemory+$1CD)",0
string_1879050459 .text "titleScreen+$100",0
string_1879050493 .text "(screenMemory+$21B)",0
string_1879050533 .text "titleScreen+$14E",0
string_1879050567 .text "OCEAN SOFTWARE",0
string_1879050597 .text "   PRESENTS   ",0
string_1879050627 .text "   BY J.STEELE",0
string_1879050657 .text "PRESS F1 FOR INSTRUCTIONS",0
string_1879050709 .text "    OR SPACE TO START    ",0
string_1879050761 .text "(colorMemory+$CD)",0
string_1879050797 .text "(colorMemory+$1CD)",0
string_1879050835 .text "(colorMemory+$21B)",0
string_1879050873 .text "screenMemory+$01B8",0
string_1879050911 .text "screenMemory+$02B8",0
string_1879050949 .text "screenMemory+$0300",0
string_1879050987 .text "colorMemory+$01B8",0
string_1879051023 .text "colorMemory+$02B8",0
string_1879051059 .text "colorMemory+$0300",0
string_1879051095 .text "screenMemory+$0320",0
string_1879051133 .text "tbl_WallBaseChars",0
string_1879051169 .text "tbl_LevelEndBellChars",0
string_1879051213 .text "tbl_KnightChars",0
string_1879051245 .text "tbl_KnightCharsColours",0
string_1879051291 .text "tbl_EmptyPitChars",0
string_1879051327 .text "tbl_EmptyPitCharColours",0
