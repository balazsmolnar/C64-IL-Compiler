; Runtime faults: what happens when a compiled program hits a condition it
; can't continue from, instead of silently corrupting memory or crashing to
; $0000.
;
;   Runtime_Fault             A = fault code (FAULT_* below). Does not return.
;   Runtime_CheckHeapRoom     called by newObjL/newObjLInit before allocating
;   Runtime_FloatError        installed as BASIC's error vector ($0300): the
;                             BASIC ROM float routines jump through it on
;                             overflow, division by zero, etc., with X = the
;                             BASIC error number
;
; Program builds print "?<MESSAGE> ERROR" (KERNAL CHROUT), turn the border
; red and halt, so the failure is visible on screen. The fault code is in A
; at Runtime_Fault, and the message table is Runtime_FaultMsgLow/High, which
; is how the VS Code debugger reports a fault (TestDebugger/ProgramSession).
;
; Unit test builds (RUNTIME_FAULT_UNITTEST=1, set by UnitTestEntry.asm) skip
; the message text -- the test program is close to its memory ceiling -- and
; just leave the code in zp_tmp4_low with result = $FE, distinct from
; Assert_Fail's $FF; RunInEmulatorAspect maps the code back to a name
; ([ExpectFault], C64TestFramework/RuntimeFault.cs -- keep the numbers in
; sync).
;
; Must be included from the entry template's low, fixed-address code area
; (next to system.asm): the float error vector is entered while BASIC ROM is
; banked in, when code above $a000 isn't reachable.

.weak
RUNTIME_FAULT_UNITTEST = 0
.endweak

FAULT_OUT_OF_MEMORY = 0
FAULT_TOO_MANY_OBJECTS = 1
FAULT_OVERFLOW = 2
FAULT_DIVISION_BY_ZERO = 3
FAULT_ILLEGAL_QUANTITY = 4
FAULT_FLOAT_ERROR = 5

; Carry-safe "does heapPointer + size still fit below HEAP_LIMIT" check for an
; allocation of zp_param2_low bytes (set by newObjL/newObjLInit). HEAP_LIMIT
; is the entry template's: $d000 (I/O) for programs, $e000 (KERNAL ROM) for
; the unit test harness.
Runtime_CheckHeapRoom
    lda heapPointer
    clc
    adc zp_param2_low
    lda heapPointer+1
    adc #0
    cmp #>HEAP_LIMIT
    bcs Runtime_HeapFull
    rts
Runtime_HeapFull
    lda #FAULT_OUT_OF_MEMORY
    jmp Runtime_Fault

; Entered via JMP ($0300) from the BASIC ROM's error handler, X = BASIC error
; number (BASIC V2's 1-based numbering: 14 illegal quantity, 15 overflow,
; 20 division by zero, ...).
Runtime_FloatError
    ldy #FAULT_FLOAT_ERROR
    cpx #14
    bne +
    ldy #FAULT_ILLEGAL_QUANTITY
+   cpx #15
    bne +
    ldy #FAULT_OVERFLOW
+   cpx #20
    bne +
    ldy #FAULT_DIVISION_BY_ZERO
+   tya
    jmp Runtime_Fault

.if RUNTIME_FAULT_UNITTEST

Runtime_Fault
    sta zp_tmp4_low
    lda #$FE
    sta result
    brk

.else

Runtime_Fault
    tax
    lda #2                  ; red border
    sta $d020
    lda #13
    jsr basout
    lda #$3f                ; '?'
    jsr basout
    lda Runtime_FaultMsgLow,x
    sta zp_param0_low
    lda Runtime_FaultMsgHigh,x
    sta zp_param0_low+1
    ldy #0
-   lda (zp_param0_low),y
    beq +
    jsr basout
    iny
    bne -
+   ldy #0
-   lda Runtime_FaultSuffix,y
    beq Runtime_FaultHalt
    jsr basout
    iny
    bne -
Runtime_FaultHalt
    jmp Runtime_FaultHalt

; CHROUT wants PETSCII; uppercase ASCII letters, digits and space are the
; same bytes, so plain ASCII text is right whatever encoding the entry
; template has active for the program's own strings.
.enc "none"
Runtime_FaultMsg0 .text "OUT OF MEMORY", 0
Runtime_FaultMsg1 .text "TOO MANY OBJECTS", 0
Runtime_FaultMsg2 .text "OVERFLOW", 0
Runtime_FaultMsg3 .text "DIVISION BY ZERO", 0
Runtime_FaultMsg4 .text "ILLEGAL QUANTITY", 0
Runtime_FaultMsg5 .text "FLOAT", 0
Runtime_FaultSuffix .text " ERROR", 0
.enc "screen"      ; ProgramEntry.asm's encoding, which this block is only assembled under

Runtime_FaultMsgLow
    .byte <Runtime_FaultMsg0, <Runtime_FaultMsg1, <Runtime_FaultMsg2
    .byte <Runtime_FaultMsg3, <Runtime_FaultMsg4, <Runtime_FaultMsg5
Runtime_FaultMsgHigh
    .byte >Runtime_FaultMsg0, >Runtime_FaultMsg1, >Runtime_FaultMsg2
    .byte >Runtime_FaultMsg3, >Runtime_FaultMsg4, >Runtime_FaultMsg5

.endif
