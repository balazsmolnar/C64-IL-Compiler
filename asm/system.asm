
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

Object_x_ctor:
        rts

Func_1_x_ctor:

        #init_locals_pull_parameters 0, [0, 0, 0,0]
        #setfld16 4, 3, 1 ; address to field at 1
        #setfld8 4, 1, 0 ; object to field at 0
        #method_exit 6, [] ; Ret

Func_1_Invoke:

        #init_locals_pull_parameters 0, [0]
        ; nop ; Nop
        ; OPT   #locals_push_value8 1 ; Ldarg_0
        ; OPT   #ldfld16 1 ; Ldfld
        #pushfld16 1 ; Nop
        #stack_pull_int_a
        sta Func_1_Invoke_l1+1
        #stack_pull_int_a
        sta Func_1_Invoke_l1+2

        #pushfld8 0             ; get object
        pla
        beq Func_1_Invoke_l1    ; if 0, static                        
        pha                     ; otherwise put object to the stack

Func_1_Invoke_l1:
        jsr 0 ; Call

        ; nop ; Nop
        #method_exit 3, [] ; Ret
