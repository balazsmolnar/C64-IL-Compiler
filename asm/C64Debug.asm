Debug_GetObjectRoots:
    #stack_save_return_adress zp_tmp1_low

    #stack_pull_int_x
    #stack_pull_int_a
    lda objTableRootCount,X
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low

Debug_GetObjectId:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    #stack_pull_int_a
    #stack_push_int_x
    #stack_return_to_saved_address zp_tmp1_low

Debug_IsAlive:
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int_x
    #stack_pull_int_a
    lda objTableHigh,X
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low

C64_get_Debug:
    #stack_save_return_adress zp_tmp1_low
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
