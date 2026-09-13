.include "./helper/loader.asm"
.include "./helper/stack.asm"
.include "./helper/localsStack.asm"
.include "./helper/heap.asm"
.include "./helper/arithmetic.asm"
.include "./helper/branch.asm"
.include "./helper/zp.asm"
.include "./helper/optimized.asm"

.include "./helper/8bit.asm"

start_address = $09FE

* = $1000
Run_Test:

    #locals_stack_init
    #initHeap heap

    lda #$FF             ; $FF to result byte (failed)
    sta result
    pha
    lda #0               ; push this
    #stack_push_int_a
    ldx #0
    cpx $09e0            ; get number of function parameters
    beq +          
-   lda $09e1,x
    #stack_push_int_a    ; copy parameters to stack
    inx
    cpx $09e0
    bne -
+   lda #>endtest        ; push return address to stack
    pha
    lda #<endtest-1
    pha
    jmp (start_address)  ; call test method
endtest:
    lda #$00             ; 0 to status mem (succeeded)
    sta result
    #stack_pull_int_a
    sta zp_tmp5          ; copy method return value
    #stack_pull_int_a
    sta zp_tmp5+1        ; copy method return value

    brk

result = $20

.include "./system.asm"
.include "./GC.asm"
.include "./helper/object.asm"
.include "./unittest/library_flags.asm"
.include "./C64.asm"

.include "./unittest/generated.asm"


Assert_Fail:
    #stack_save_return_adress zp_tmp2_low 
    #stack_pull_pointer zp_tmp4_low           ; copy message pointer
    lda #$FF
    sta result                                ; put FF to status mem (failed)
    brk

Assert_AreEqual:
    #stack_save_return_adress zp_tmp2_low
    #stack_pull_pointer zp_tmp4_low
    #stack_pull_int_a
    sta zp_tmp3
    #stack_pull_int_a
    cmp zp_tmp3
    beq +
    #stack_push_var16 zp_tmp4_low
    jsr Assert_Fail
+   #stack_return_to_saved_address zp_tmp2_low

Assert_IsTrue:
    #stack_save_return_adress zp_tmp2_low
    #stack_pull_pointer zp_tmp4_low
    #stack_pull_int_a
    cmp #0
    bne +
    #stack_push_var16 zp_tmp4_low
    jsr Assert_Fail
+   #stack_return_to_saved_address zp_tmp2_low

Assert_IsFalse:
    #stack_save_return_adress zp_tmp2_low
    #stack_pull_pointer zp_tmp4_low
    #stack_pull_int_a
    cmp #0
    beq +
    #stack_push_var16 zp_tmp4_low
    jsr Assert_Fail
+   #stack_return_to_saved_address zp_tmp2_low

; This already floated right after code (no fixed org here), which is
; correct/desired -- but with no safety fallback if the test assembly
; ever grew large enough to risk landing in BASIC ROM at $a000. Added the
; same guard the other three entry points use, for consistency: fall
; back to $c000 (real, safe RAM) if there isn't room for the full
; 2048-byte object-table block before $a000. See main.asm/hunchback.asm/
; presentation.asm for the fuller rationale (SimpleEmulator.SetMemory
; hard-blocks $a000-$c000/>$e000 the same way real ROM would).
.if * < $9800
.else
* = $c000
.endif
objTableLow
.fill 256, 0
objTableHigh
.fill 256, 0
objTableSize
.fill 256, 0
objTableReferences
.fill 256, 0
objTableRootCount
.fill 256, 0
.fill 256, 0
objTableDescLow
.fill 256, 0
objTableDescHigh
.fill 256, 0

localsStack
.fill 256, 0 
heap

