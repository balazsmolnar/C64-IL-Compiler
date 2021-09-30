
*=$2000

spt_c_upper:
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $01, $FE
.byte $00, $0F, $FF
.byte $00, $3F, $FF
.byte $00, $FF, $FF
.byte $01, $FF, $FF
.byte $03, $FF, $FF
.byte $07, $FF, $FF
.byte $0F, $FF, $FF
.byte $1F, $FF, $FF
.byte $1F, $FF, $87
.byte $3F, $FE, $00
.byte $3F, $F8, $00
.byte $7F, $F0, $00
.byte $7F, $E0, $00
.byte $7F, $E0, $00
.byte $7F, $C0, $00
.byte $FF, $C0, $00
.byte $FF, $C0, $00
.byte $FF, $C0, $00
.byte $00

spt_logo_upper:
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $FF, $FF, $00
.byte $FF, $FE, $00
.byte $FF, $FC, $00
.byte $FF, $F8, $00
.byte $FF, $F0, $00
.byte $FF, $E0, $00
.byte $FF, $C0, $00
.byte $FF, $80, $00
.byte $00, $00, $00
.byte $00

spt_sharp:
.byte $01, $04, $00
.byte $01, $86, $00
.byte $01, $86, $00
.byte $01, $86, $00
.byte $1F, $FF, $C0
.byte $0F, $FF, $E0
.byte $01, $86, $00
.byte $01, $86, $00
.byte $01, $86, $00
.byte $01, $86, $00
.byte $1F, $FF, $C0
.byte $0F, $FF, $E0
.byte $01, $86, $00
.byte $01, $86, $00
.byte $01, $86, $00
.byte $00, $82, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00

spt_c_lower:
.byte $FF, $C0, $00
.byte $FF, $C0, $00
.byte $FF, $C0, $00
.byte $7F, $C0, $00
.byte $7F, $E0, $00
.byte $7F, $E0, $00
.byte $7F, $F0, $00
.byte $3F, $F8, $00
.byte $3F, $FE, $00
.byte $1F, $FF, $87
.byte $1F, $FF, $FF
.byte $0F, $FF, $FF
.byte $07, $FF, $FF
.byte $03, $FF, $FF
.byte $01, $FF, $FF
.byte $00, $FF, $FF
.byte $00, $3F, $FF
.byte $00, $0F, $FF
.byte $00, $01, $FE
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00

spt_logo_lower:
.byte $00, $00, $00
.byte $FF, $80, $00
.byte $FF, $C0, $00
.byte $FF, $E0, $00
.byte $FF, $F0, $00
.byte $FF, $F8, $00
.byte $FF, $FC, $00
.byte $FF, $FE, $00
.byte $FF, $FF, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00, $00, $00
.byte $00

screenMemory = $400
colorMemory = $D800
