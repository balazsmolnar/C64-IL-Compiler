!macro stack_push_int value {
  +stack_push_int8 value
}

!macro stack_push_var .address {
  +stack_push_var8 .address
}

!macro stack_pull_int .address {
  +stack_pull_int8 .address
}

!macro negate {
    +negate8
}

!macro compareLess {
    +compareLess8
}

!macro compareGreater {
    +compareGreater8
}

!macro compareEqual {
    +compareEqual8
}

!macro add {
    +add8
}

!macro inc_var .variable {
    +inc_var8 .variable
}

!macro init_var .variable, value {
    +init_var8 .variable, value
}
