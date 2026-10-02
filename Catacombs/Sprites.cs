using C64Lib;

// Spider sprite art: 1 static pose, generated programmatically (a filled
// body/head ellipse plus 8 bent legs as ray-cast lines, not hand-drawn --
// see git history for the generator script) rather than transcribed from
// an existing asset the way the bat art originally was. Only 1 pose (see
// the earlier ""TRIED AND REVERTED"-style lesson here: a 2nd pose was once
// placed at a hand-picked address ($1700) that LOOKED free in one build's
// segment-size report, but the assembler not erroring on that placement
// is not the same as it actually being safe -- confirmed empirically via
// VICE, that pose rendered as garbage/garbled pixels, not a real bat
// shape. Unlike the $3F40 block below, which asm/Templates/ProgramEntry.asm's
// own structure GUARANTEES is free (compiled code always resumes at
// $4000, right after it), $1700 had no such guarantee behind it, just an
// observation from a single measure.bat run -- not something to build on
// again without first finding an address this compiler's own template
// actually commits to leaving alone). Player sprite art: copied byte-for-byte from
// Hunchback's own sprHunchback.spt (SpritePad project file, indices
// 40/42 = "player_right_0"/"player_right_2"), converted to raw .byte
// triples by hand from the .spt's <data> row values (row = 24-bit int,
// byte0=bits23-16/byte1=bits15-8/byte2=bits7-0) rather than pulling in
// Hunchback's whole RawSprite/SpritePad-resource pipeline for 2 frames.
// This is a MULTICOLOR sprite (see Hunchback's own <multi>True</multi>)
// -- PlayerSprite.cs sets Sprite2.MultiColor=true and pokes the shared
// $D025/$D026 palette registers to match the original's mcolour1=6(Blue)/
// mcolour2=7(Yellow).
//
// All 3 frames fit exactly in the one guaranteed-free 192-byte gap right
// after the (single, now) bitmap -- see below.
//
// Placed at a FIXED address because the VIC-II reads a sprite's data
// pointer from (video matrix + $3F8), not the text screen's $07F8, in
// bitmap mode -- Program.cs sets those pointer bytes.
//
// The `* =` jumps are the same trick ProgramEntry.asm uses for the bitmap
// and matrix reservations: emit at a fixed address, then resume where the
// assembler was.
[assembly: RawAssembly(Order = 0, Asm = @"
spider_pose .macro
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $40,$00,$02
    .byte $30,$1C,$0C
    .byte $0C,$3E,$10
    .byte $03,$7F,$60
    .byte $00,$3E,$00
    .byte $FF,$FF,$FF
    .byte $01,$FF,$C0
    .byte $01,$FF,$C0
    .byte $01,$FF,$C0
    .byte $1F,$FF,$FC
    .byte $E0,$3E,$03
    .byte $00,$00,$00
    .byte $03,$00,$60
    .byte $0C,$00,$18
    .byte $30,$00,$04
    .byte $40,$00,$02
    .byte $00,$00,$00
    .byte $00,$00,$00
    .byte $00
.endm
player_right_0 .macro
    .byte $00,$10,$00
    .byte $00,$54,$00
    .byte $00,$5C,$00
    .byte $00,$7C,$00
    .byte $02,$7C,$00
    .byte $02,$B0,$00
    .byte $01,$A0,$00
    .byte $01,$A8,$00
    .byte $01,$A8,$00
    .byte $01,$A8,$00
    .byte $01,$A8,$00
    .byte $03,$A8,$00
    .byte $03,$54,$00
    .byte $02,$A8,$00
    .byte $02,$A8,$00
    .byte $00,$A0,$00
    .byte $00,$A0,$00
    .byte $00,$80,$00
    .byte $00,$80,$00
    .byte $00,$C0,$00
    .byte $00,$A0,$00
    .byte $00
.endm
player_right_2 .macro
    .byte $00,$10,$00
    .byte $00,$54,$00
    .byte $00,$5C,$00
    .byte $00,$7C,$00
    .byte $02,$7C,$00
    .byte $02,$B0,$00
    .byte $01,$A0,$00
    .byte $01,$A8,$00
    .byte $01,$A9,$00
    .byte $01,$A9,$00
    .byte $01,$A9,$C0
    .byte $01,$E8,$C0
    .byte $02,$D4,$00
    .byte $02,$A8,$00
    .byte $00,$A8,$00
    .byte $02,$A0,$00
    .byte $0A,$20,$00
    .byte $0C,$20,$00
    .byte $08,$20,$00
    .byte $08,$30,$00
    .byte $00,$28,$00
    .byte $00
.endm
sprites_resume = *
* = $3F40
    #spider_pose
    #player_right_0
    #player_right_2
* = sprites_resume
")]
