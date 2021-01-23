Debug_GetObjectRoots:
    +stack_save_return_adress $20

    +stack_pull_int_x
    +stack_pull_int_a
    lda objTableRootCount,X
    +stack_push_int_a
    +stack_return_to_saved_address $20

C64_get_Debug:
    +stack_save_return_adress $20
    +stack_push_int_a
    +stack_return_to_saved_address $20
