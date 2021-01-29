
;Basic loader

;Creates a macro that sets the program counter to BASIC, then
;puts "10 SYS <address>". Address is written in base 10 and
;assumes 5 digits, although it will work with 4. Finally, it
;sets the program counter to the given address to start the
;assembly.
start_at .macro address
   * = $0801	;Set program counter to address of BASIC in memory.
   .byte $0c,$08,$00,$00,$9e
   .if \address > 100 
      .byte 48 + ((\address / 10000) % 10) 
   .endif
  .if \address >=  1000  
    .byte 48 + ((\address /  1000) % 10) 
  .endif
  .if \address >=   100  
    .byte 48 + ((\address /   100) % 10)
  .endif
  .if \address >=    10  
    .byte 48 + ((\address /    10) % 10) 
  .endif
  .byte $30 + (\address % 10), $00, $00, $00
  * = \address
.endm
;Source: https://github.com/cslarsen/c64-examples/blob/master/basic-boot.asm