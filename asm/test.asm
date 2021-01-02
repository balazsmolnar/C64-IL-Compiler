
*=$1000


!macro dist label1, ~d {
    !ifndef label1  {
        !set d = 255
    } else {
        !set d = * - label1
    }
}

!macro bne .label {
    !set .d = 0
    !ifndef .label  {
        !set d = 255
    } else {
        !set d = * - .label
    }
    !if (.d < 127) {
        bne .label
    } else {
        beq+
        jmp .label
+       
    }
}

!macro bne2 .labelxxx {
    +bne .labelxxx
}


l2:
        +bne2 l1
        bne l1
        beq l1
        bne l2
        nop
        nop
        nop
l1:     nop
        nop
        


