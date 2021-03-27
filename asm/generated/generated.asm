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
string_1879048193 .text "spt_fireball_0",0
string_1879048223 .text "spt_fireball_1",0
string_1879048253 .text "spt_fireball_2",0
string_1879048283 .text "spt_fireball_3",0
string_1879048313 .text "charset",0
string_1879048329 .text "spt_knight_climb_0",0
string_1879048367 .text "spt_knight_climb_1",0
string_1879048405 .text "spt_knight_walk_0",0
string_1879048441 .text "spt_knight_walk_1",0
string_1879048477 .text "GET READY",0
string_1879048497 .text "         ",0
string_1879048517 .text "spt_player_right_0",0
string_1879048555 .text "spt_player_rope_left",0
string_1879048597 .text "spt_player_rope_right",0
string_1879048641 .text "spt_player_jump_left_0",0
string_1879048687 .text "spt_player_jump_left_1",0
string_1879048733 .text "spt_player_jump_left_2",0
string_1879048779 .text "spt_player_jump_right_0",0
string_1879048827 .text "spt_player_jump_right_1",0
string_1879048875 .text "spt_player_jump_right_2",0
string_1879048923 .text "spt_player_left_0",0
string_1879048959 .text "spt_player_left_3",0
string_1879048995 .text "spt_player_left_1",0
string_1879049031 .text "spt_player_left_2",0
string_1879049067 .text "spt_player_right_3",0
string_1879049105 .text "spt_player_right_1",0
string_1879049143 .text "spt_player_right_2",0
string_1879049181 .text "txt_PlayerStats",0
string_1879049213 .text "tbl_PlayerStatsColours",0
string_1879049259 .text "spt_rope_14",0
string_1879049283 .text "spt_rope_15",0
string_1879049307 .text "spt_rope_12",0
string_1879049331 .text "spt_rope_13",0
string_1879049355 .text "spt_rope_10",0
string_1879049379 .text "spt_rope_11",0
string_1879049403 .text "spt_rope_8",0
string_1879049425 .text "spt_rope_9",0
string_1879049447 .text "spt_rope_6",0
string_1879049469 .text "spt_rope_7",0
string_1879049491 .text "spt_rope_4",0
string_1879049513 .text "spt_rope_5",0
string_1879049535 .text "spt_rope_2",0
string_1879049557 .text "spt_rope_3",0
string_1879049579 .text "spt_rope_0",0
string_1879049601 .text "spt_rope_1",0
string_1879049623 .text "spt_rope_16",0
string_1879049647 .text "spt_rope_17",0
string_1879049671 .text "spt_rope_18",0
string_1879049695 .text "spt_rope_19",0
string_1879049719 .text "spt_rope_20",0
string_1879049743 .text "spt_rope_21",0
string_1879049767 .text "spt_rope_22",0
string_1879049791 .text "spt_rope_23",0
string_1879049815 .text "spt_rope_24",0
string_1879049839 .text "spt_rope_25",0
string_1879049863 .text "spt_rope_26",0
string_1879049887 .text "spt_rope_27",0
string_1879049911 .text "spt_rope_28",0
string_1879049935 .text "spt_rope_29",0
string_1879049959 .text "spt_rope_30",0
string_1879049983 .text "spt_rope_31",0
string_1879050007 .text "(screenMemory)",0
string_1879050037 .text "(screenMemory+$100)",0
string_1879050077 .text "(screenMemory+$200)",0
string_1879050117 .text "(screenMemory+$300)",0
string_1879050157 .text "(colorMemory)",0
string_1879050185 .text "(colorMemory+$100)",0
string_1879050223 .text "(colorMemory+$200)",0
string_1879050261 .text "(colorMemory+$300)",0
string_1879050299 .text "(screenMemory+$CD)",0
string_1879050337 .text "titleScreen",0
string_1879050361 .text "(screenMemory+$1CD)",0
string_1879050401 .text "titleScreen+$100",0
string_1879050435 .text "(screenMemory+$21B)",0
string_1879050475 .text "titleScreen+$14E",0
string_1879050509 .text "OCEAN SOFTWARE",0
string_1879050539 .text "   PRESENTS   ",0
string_1879050569 .text "   BY J.STEELE",0
string_1879050599 .text "PRESS F1 FOR INSTRUCTIONS",0
string_1879050651 .text "    OR SPACE TO START    ",0
string_1879050703 .text "(colorMemory+$CD)",0
string_1879050739 .text "(colorMemory+$1CD)",0
string_1879050777 .text "(colorMemory+$21B)",0
string_1879050815 .text "screenMemory+$01B8",0
string_1879050853 .text "screenMemory+$02B8",0
string_1879050891 .text "screenMemory+$0300",0
string_1879050929 .text "colorMemory+$01B8",0
string_1879050965 .text "colorMemory+$02B8",0
string_1879051001 .text "colorMemory+$0300",0
string_1879051037 .text "screenMemory+$0320",0
string_1879051075 .text "tbl_WallBaseChars",0
string_1879051111 .text "tbl_LevelEndBellChars",0
string_1879051155 .text "tbl_KnightChars",0
string_1879051187 .text "tbl_KnightCharsColours",0
string_1879051233 .text "tbl_EmptyPitChars",0
string_1879051269 .text "tbl_EmptyPitCharColours",0
