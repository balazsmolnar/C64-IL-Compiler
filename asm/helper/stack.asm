; Use C64 native stack
; less space, but faster
;
!macro stack_push_pointer .ptr {
  lda # < .ptr
  pha
  lda # > .ptr
  pha
}

!macro stack_pull_pointer .address {
  pla
  sta .address+1
  pla
  sta .address
}

!macro stack_push_int16 value {
  lda # < value
  pha
  lda # > value
  pha
}

!macro stack_push_int8 value {
  lda # < value
  pha
}


!macro stack_pull_int16 .address {
  pla
  sta .address+1
  pla
  sta .address
}

!macro stack_pull_int8 .address {
  pla
  sta .address
}

!macro stack_push_var16 .address {
  lda .address
  pha
  lda .address+1
  pha
}

!macro stack_push_var8 .address {
  lda .address
  pha
}

!macro stack_duplicate {
  pla
  pha
  pha
}

!macro stack_save_return_adress .variable {
  pla
  sta .variable+1
  pla
  sta .variable
}

!macro stack_return_to_saved_address .variable {
  lda .variable
  pha
  lda .variable+1
  pha
  rts
}

!macro stack_pull_int_a {
  pla
}

!macro stack_pull_int_x {
  pla
  tax
}

!macro stack_pull_int_y {
  pla
  tay
}

!macro stack_push_int_a {
  pha
}
