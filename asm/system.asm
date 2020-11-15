
	basout = $ffd2		; explicit global label def.
WriteLine

        ; pull variable from stack
        +stack_pull_pointer $30
 
        ldy #0
		beq +			; enter loop

-			jsr basout	; output character
			iny		; advance pointer
+			lda ($30),y	; get character
			bne -		; check whether last
            lda #13
            jsr basout
		rts
