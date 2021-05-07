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
string_1879048193 .text "spt_player_right_0",0
string_1879048231 .text "spt_arrow_right",0
string_1879048263 .text "spt_fireball_0",0
string_1879048293 .text "spt_fireball_1",0
string_1879048323 .text "spt_fireball_2",0
string_1879048353 .text "spt_fireball_3",0
string_1879048383 .text "charset",0
string_1879048399 .text "spt_knight_climb_0",0
string_1879048437 .text "spt_knight_climb_1",0
string_1879048475 .text "spt_knight_walk_0",0
string_1879048511 .text "spt_knight_walk_1",0
string_1879048547 .text "GET READY",0
string_1879048567 .text "         ",0
string_1879048587 .text "spt_player_rope_left",0
string_1879048629 .text "spt_player_rope_right",0
string_1879048673 .text "spt_player_jump_left_0",0
string_1879048719 .text "spt_player_jump_left_1",0
string_1879048765 .text "spt_player_jump_left_2",0
string_1879048811 .text "spt_player_jump_right_0",0
string_1879048859 .text "spt_player_jump_right_1",0
string_1879048907 .text "spt_player_jump_right_2",0
string_1879048955 .text "spt_player_left_0",0
string_1879048991 .text "spt_player_left_3",0
string_1879049027 .text "spt_player_left_1",0
string_1879049063 .text "spt_player_left_2",0
string_1879049099 .text "spt_player_right_3",0
string_1879049137 .text "spt_player_right_1",0
string_1879049175 .text "spt_player_right_2",0
string_1879049213 .text "txt_PlayerStats",0
string_1879049245 .text "tbl_PlayerStatsColours",0
string_1879049291 .text "spt_rope_14",0
string_1879049315 .text "spt_rope_15",0
string_1879049339 .text "spt_rope_12",0
string_1879049363 .text "spt_rope_13",0
string_1879049387 .text "spt_rope_10",0
string_1879049411 .text "spt_rope_11",0
string_1879049435 .text "spt_rope_8",0
string_1879049457 .text "spt_rope_9",0
string_1879049479 .text "spt_rope_6",0
string_1879049501 .text "spt_rope_7",0
string_1879049523 .text "spt_rope_4",0
string_1879049545 .text "spt_rope_5",0
string_1879049567 .text "spt_rope_2",0
string_1879049589 .text "spt_rope_3",0
string_1879049611 .text "spt_rope_0",0
string_1879049633 .text "spt_rope_1",0
string_1879049655 .text "spt_rope_16",0
string_1879049679 .text "spt_rope_17",0
string_1879049703 .text "spt_rope_18",0
string_1879049727 .text "spt_rope_19",0
string_1879049751 .text "spt_rope_20",0
string_1879049775 .text "spt_rope_21",0
string_1879049799 .text "spt_rope_22",0
string_1879049823 .text "spt_rope_23",0
string_1879049847 .text "spt_rope_24",0
string_1879049871 .text "spt_rope_25",0
string_1879049895 .text "spt_rope_26",0
string_1879049919 .text "spt_rope_27",0
string_1879049943 .text "spt_rope_28",0
string_1879049967 .text "spt_rope_29",0
string_1879049991 .text "spt_rope_30",0
string_1879050015 .text "spt_rope_31",0
string_1879050039 .text "(screenMemory)",0
string_1879050069 .text "(screenMemory+$100)",0
string_1879050109 .text "(screenMemory+$200)",0
string_1879050149 .text "(screenMemory+$300)",0
string_1879050189 .text "(colorMemory)",0
string_1879050217 .text "(colorMemory+$100)",0
string_1879050255 .text "(colorMemory+$200)",0
string_1879050293 .text "(colorMemory+$300)",0
string_1879050331 .text "(screenMemory+$CD)",0
string_1879050369 .text "titleScreen",0
string_1879050393 .text "(screenMemory+$1CD)",0
string_1879050433 .text "titleScreen+$100",0
string_1879050467 .text "(screenMemory+$21B)",0
string_1879050507 .text "titleScreen+$14E",0
string_1879050541 .text "OCEAN SOFTWARE",0
string_1879050571 .text "   PRESENTS   ",0
string_1879050601 .text "   BY J.STEELE",0
string_1879050631 .text "PRESS F1 FOR INSTRUCTIONS",0
string_1879050683 .text "    OR SPACE TO START    ",0
string_1879050735 .text "(colorMemory+$CD)",0
string_1879050771 .text "(colorMemory+$1CD)",0
string_1879050809 .text "(colorMemory+$21B)",0
string_1879050847 .text "screenMemory+$01B8",0
string_1879050885 .text "screenMemory+$02B8",0
string_1879050923 .text "screenMemory+$0300",0
string_1879050961 .text "colorMemory+$01B8",0
string_1879050997 .text "colorMemory+$02B8",0
string_1879051033 .text "colorMemory+$0300",0
string_1879051069 .text "screenMemory+$0320",0
string_1879051107 .text "tbl_WallBaseChars",0
string_1879051143 .text "tbl_LevelEndBellChars",0
string_1879051187 .text "tbl_KnightChars",0
string_1879051219 .text "tbl_KnightCharsColours",0
string_1879051265 .text "tbl_EmptyPitChars",0
string_1879051301 .text "tbl_EmptyPitCharColours",0
