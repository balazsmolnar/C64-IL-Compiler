add16 .macro 

    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high

    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    clc
    lda zp_param2_low
    adc zp_param1_low
    sta zp_param2_low
    lda zp_param2_high
    adc zp_param1_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

add8 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int_a

    clc
    adc zp_param1_low
    #stack_push_int_a
.endm

sub8 .macro 

    #stack_pull_int zp_param1_low
    #stack_pull_int_a

    sec
    sbc zp_param1_low
    #stack_push_int_a
.endm

sub16 .macro 

    #stack_pull_int zp_param1_low
    #stack_pull_int zp_param1_high

    #stack_pull_int zp_param2_low
    #stack_pull_int zp_param2_high

    sec
    lda zp_param2_low
    sbc zp_param1_low
    sta zp_param2_low
    lda zp_param2_high
    sbc zp_param1_high
    #stack_push_int_a
    #stack_push_var zp_param2_low
.endm

negate16 .macro 

    #stack_pull_int zp_param2_low

    lda zp_param2_high
    eor #$FF
    sta zp_param2_high
    lda zp_param2_low
    eor #$FF
    clc
    adc #$1
    bcc +
    inc zp_param2_high
+   sta zp_param2_low
    #stack_push_var zp_param2_low
.endm

negate8 .macro  

    #stack_pull_int_a

    eor #$FF
    clc
    adc #$1
    #stack_push_int_a
.endm

compareLess16 .macro
; signed 16-bit a < b. zp_param2_low/zp_param2_high = a (low/high), zp_param1_low/zp_param1_high = b (low/high).
; Full 16-bit subtract (a - b) via chained CMP/SBC, with the standard
; overflow-corrected N-flag check for a signed result (a plain CMP/BCC
; chain, as used by the _unsigned16 variants below, is only valid for
; unsigned comparison). Relies on SBC's V flag being computed correctly
; (see SimpleEmulator/Emulator.cs ADC/SBC cases, fixed alongside this).
        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high

        ldx #0
        lda zp_param2_low
        cmp zp_param1_low
        lda zp_param2_high
        sbc zp_param1_high
        bvc +
        eor #$80
+       bpl l1
        inx
l1
        #stack_push_int_x
.endm

compareGreater16 .macro
; signed 16-bit a > b, implemented as signed (b - a) < 0. See compareLess16.
        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high

        ldx #0
        lda zp_param1_low
        cmp zp_param2_low
        lda zp_param1_high
        sbc zp_param2_high
        bvc +
        eor #$80
+       bpl l1
        inx
l1
        #stack_push_int_x
.endm

compareGreater_unsigned16 .macro

        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high

        ldx #0
        lda zp_param1_high
        cmp zp_param2_high
        bcc +
        bne l1
        lda zp_param1_low
        cmp zp_param2_low
        bcs l1
+       inx
l1      
        #stack_push_int_x
.endm

compareLess_unsigned16 .macro

        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high
        ; #stack_pull_int zp_param2_high ends on "pla / sta zp_param2_high" -- A already holds
        ; that value, so no need to reload it before the compare below.

        ldx #0
        cmp zp_param1_high
        bcc +
        bne l1
        lda zp_param2_low
        cmp zp_param1_low
        bcs l1
+       inx
l1      
        #stack_push_int_x
.endm

compareLess8 .macro 
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bpl +
        inx
+        #stack_push_int_x
.endm

compareLess_unsigned8 .macro 
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bcs +
        inx
+        #stack_push_int_x
.endm

compareGreater8 .macro 
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bmi +
        beq +
        inx
+       #stack_push_int_x
.endm

compareGreater_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bmi +
        beq +
        inx
+       #stack_push_int_x
.endm

compareGreater_unsigned8 .macro 
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bcc +
        beq +
        inx
+       #stack_push_int_x
.endm

compareGreater_unsigned_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bcc +
        beq +
        inx
+       #stack_push_int_x
.endm

compareLess_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bpl +
        inx
+       #stack_push_int_x
.endm

compareLess_unsigned_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bcs +
        inx
+       #stack_push_int_x
.endm

compareEqual16 .macro 
        #stack_pull_int zp_param1_low
        #stack_pull_int zp_param1_high
        #stack_pull_int zp_param2_low
        #stack_pull_int zp_param2_high

        ldx #0
        lda zp_param1_low
        cmp zp_param2_low
        bne +
        lda zp_param1_high
        cmp zp_param2_high
        bne +
        inx
+
        #stack_push_int_x 
.endm

compareEqual8 .macro
        #stack_pull_int zp_param2_low
        #stack_pull_int_a

        ldx #0
        cmp zp_param2_low
        bne +
        inx
+
        #stack_push_int_x
.endm

compareEqual_const8 .macro value
        #stack_pull_int_a

        ldx #0
        cmp #\value
        bne +
        inx
+
        #stack_push_int_x
.endm

shift_left8 .macro 

    #stack_pull_int_x
    #stack_pull_int_a
    
-   asl
    dex
    bne -
    #stack_push_int_a
.endm

shift_right8 .macro 

    #stack_pull_int_x
    #stack_pull_int_a
    
-   lsr
    dex
    bne -
    #stack_push_int_a
.endm

and8 .macro

    #stack_pull_int zp_param1_low
    #stack_pull_int_a

    and zp_param1_low
    #stack_push_int_a
.endm

conv_8_16 .macro

    #stack_pull_int_x
    lda #0
    stack_push_int_a
    stack_push_int_x

.endm

conv_16_8 .macro

    #stack_pull_int_x
    #stack_pull_int_a
    #stack_push_int_x
.endm