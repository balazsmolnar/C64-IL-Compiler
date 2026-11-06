; ===========================================================================
; C64.Disk (C64Lib/Disk.cs): files on a floppy, through the KERNAL.
;
; Disk_Load is the KERNAL's own LOAD (SETNAM, SETLFS, LOAD with secondary
; address 0: "load at the address in X/Y"), so it works with a real 1541 or
; any compatible drive, VICE's true drive emulation or its virtual (trapped)
; devices, and fast-loader cartridges that patch the KERNAL vectors.
;
; What it relies on:
; - The KERNAL ROM is mapped in. It is, except inside the bitmap graphics
;   routines (#gfx_ram_on, asm/C64Graphics.asm), which never call this; it
;   is forced in anyway (HIRAM + CHAREN) and $01 put back afterwards.
; - The KERNAL's zero page ($90-$FF) is its own (asm/helper/zeropage.asm).
; - Interrupts stay on. The KERNAL's serial byte receive (ACPTR) runs with
;   them off while it waits for the drive, which can be a whole sector's
;   time, so a raster split (Screen.SetSplit) may come late and flicker, and
;   the jiffy clock / music fall behind, for the length of the load. CIA 1's
;   timer B, which ACPTR uses for its EOI timeout, belongs to the KERNAL.
; ===========================================================================

.weak
Flag_Disk_Load = 0
.endweak

.if Flag_Disk_Load

KERNAL_SETMSG = $ff90           ; A = message flags ($9D): 0 = no "SEARCHING FOR" / "LOADING"
KERNAL_SETLFS = $ffba           ; A = logical file, X = device, Y = secondary address
KERNAL_SETNAM = $ffbd           ; A = name length, X/Y = name address
KERNAL_LOAD   = $ffd5           ; A = 0 (load, not verify), X/Y = address; C set on error
KERNAL_MSGFLG = $9d             ; the message flags SETMSG sets
KERNAL_FA     = $ba             ; device number of the last file used: the program's own drive

DISK_FIRST_DRIVE = 8            ; devices 8-30 are drives; 0-7 tape, keyboard, screen, printers
DISK_LOGICAL_FILE = 1           ; LOAD opens no file table entry; any number does
DISK_LOAD_AT_ADDRESS = 0        ; secondary address 0: ignore the file's own load address
DISK_NAME_MAX = 16              ; longest CBM DOS file name
CPU_PORT_KERNAL_IO = %00000110  ; $01 bits HIRAM (KERNAL ROM) + CHAREN (I/O)

disk_name    .fill DISK_NAME_MAX ; the name, as PETSCII
disk_address .word 0

; C64.Disk.Load(fileName, address) -> bool
Disk_Load
    #stack_save_return_adress zp_tmp1_low
    #stack_pull_int16 disk_address
    #stack_pull_pointer zp_tmp2_low

    ; String literals are screen codes (.enc "screen"): letters typed in
    ; lower case are 1-26, upper case $41-$5A. Both become PETSCII $41-$5A,
    ; the upper-case letters CBM DOS (and c1541's "write") use, so the
    ; name's case does not matter. Codes $20-$3F (digits, space, *, ?) are
    ; the same in both.
    ldy #0
Disk_Load_Name
    lda (zp_tmp2_low),y
    beq Disk_Load_NameDone
    cmp #$20
    bcs +
    ora #$40
+   sta disk_name,y
    iny
    cpy #DISK_NAME_MAX
    bne Disk_Load_Name
Disk_Load_NameDone
    tya
    ldx #<disk_name
    ldy #>disk_name
    jsr KERNAL_SETNAM

    lda $01
    pha
    ora #CPU_PORT_KERNAL_IO
    sta $01
    lda KERNAL_MSGFLG
    pha
    lda #0
    jsr KERNAL_SETMSG

    ldx KERNAL_FA
    cpx #DISK_FIRST_DRIVE
    bcs +
    ldx #DISK_FIRST_DRIVE
+   lda #DISK_LOGICAL_FILE
    ldy #DISK_LOAD_AT_ADDRESS
    jsr KERNAL_SETLFS

    lda #0
    ldx disk_address
    ldy disk_address+1
    jsr KERNAL_LOAD
    lda #0
    rol                         ; A = carry: 1 = error
    eor #1                      ; -> true = loaded
    tax

    pla
    sta KERNAL_MSGFLG
    pla
    sta $01
    txa
    #stack_push_int_a
    #stack_return_to_saved_address zp_tmp1_low
.endif
