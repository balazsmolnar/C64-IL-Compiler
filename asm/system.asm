
	basout = $ffd2		; explicit global label def.
Console_WriteLine

        #stack_save_return_adress $22

        ; pull variable from stack
        jsr Console_Write
        lda #13
        jsr basout
        #stack_return_to_saved_address $22

Console_Write


        #stack_save_return_adress zp_tmp1_low
        ; pull variable from stack
        #stack_pull_pointer $30

        ldy #0
		beq +			; enter loop

-			jsr basout	; output character
			iny		; advance pointer
+			lda ($30),y	; get character
			bne -		; check whether last
        #stack_return_to_saved_address zp_tmp1_low

String_get_Length:

        #stack_save_return_adress zp_tmp1_low
        ; pull variable from stack
        #stack_pull_pointer $30

        ldy #0
	beq +			; enter loop
-		iny		; advance pointer
+		lda ($30),y	; get character
		bne -		; check whether last
        #stack_push_int_y
        #stack_return_to_saved_address zp_tmp1_low

