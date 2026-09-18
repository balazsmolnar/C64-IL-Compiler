.weak
Flag_Debug_GetObjectRoots = 0
.endweak
.if Flag_Debug_GetObjectRoots

Debug_GetObjectRoots:
    #stack_save_return_adress zp_tmp1_low

    #stack_pull_int_x
    #stack_pull_int_a
    lda objTableRootCount,X
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Debug_GetObjectId = 0
.endweak
.if Flag_Debug_GetObjectId

Debug_GetObjectId:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    #stack_pull_int_a
    #stack_push_int_x
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_Debug_IsAlive = 0
.endweak
.if Flag_Debug_IsAlive

Debug_IsAlive:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    #stack_pull_int_a
    lda objTableHigh,X
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif

.weak
Flag_C64_get_Debug = 0
.endweak
.if Flag_C64_get_Debug

C64_get_Debug:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif
