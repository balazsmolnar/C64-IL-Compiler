!macro stack_push_int value {
  #stack_push_int16 value
}

!macro stack_push_var .address {
  #stack_push_var16 .address
}

!macro stack_pull_int .address {
  #stack_pull_int16 .address
}

!macro negate {
    #negate16
}

!macro compareLess {
    #compareLess16
}

!macro compareEqual {
    #compareEqual16
}

!macro add {
    #add16
}

!macro inc_var variable {
    #inc_var16 \variable
}
