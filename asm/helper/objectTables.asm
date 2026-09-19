; The 7x256-byte object/GC tables + 256-byte localsStack (2048 bytes total)
; are runtime-only scratch, never meant to contain meaningful data at load
; time:
;   - objTableSize/RootCount/Low/High are re-zeroed at startup by
;     #initHeap's own loop (asm/helper/heap.asm) -- that loop stays, this
;     file doesn't replace it.
;   - objTableReferences/DescLow/DescHigh are only ever read for a slot
;     whose handle was already produced by a prior newObjL/newObjLInit
;     call, which always writes all three via setObjParams before any
;     handle referencing that slot can exist (traced every read site in
;     asm/helper/object.asm and asm/GC.asm to confirm there's no
;     read-before-write path -- trackObjects's full-256-slot scan only
;     ever touches objTableRootCount, which IS zeroed).
;   - localsStack is a push-before-pop evaluation stack.
; None of that needs real bytes on disk. .virtual/.endv still assigns
; normal, real addresses to every label below (usable from code exactly
; like .fill would give them), but emits zero bytes into the assembled
; output -- this must be the last thing in the entry file, since .endv
; rewinds * back to where .virtual started, and anything emitted after
; would alias these same addresses.
;
; Table placement itself: floats right after code, but falls back if
; there isn't room for the full 2048-byte block before whatever comes
; next that isn't safe RAM. The entry template defines
; OBJ_TABLES_MAX_START/OBJ_TABLES_FALLBACK before including this file --
; a program that banked out BASIC ROM (asm/helper/banking.asm) has real
; RAM all the way to $d000 (I/O), so it gets a much higher ceiling than a
; target that didn't (unittest.asm never banks -- see asm/helper/banking.asm
; and ILEntryPointPass -- so it keeps the original conservative $9800/
; $c000 pair: BASIC ROM is mapped read-only there, and SimpleEmulator's
; SetMemory hard-blocks $a000-$c000 the same way unless it sees LORAM
; actually cleared).
.if * < OBJ_TABLES_MAX_START
.else
* = OBJ_TABLES_FALLBACK
.endif

.virtual *
objTableLow         .fill 256
objTableHigh        .fill 256
objTableSize        .fill 256
objTableReferences  .fill 256
objTableRootCount   .fill 256
objTableDescLow     .fill 256
objTableDescHigh    .fill 256

localsStack         .fill 256

; Shared scratch buffer for every NumberFormat_*ToString routine (asm/
; helper/tostring.asm, asm/helper/float.asm's Float_ToString) -- 16 bytes
; is enough for the widest case seen (a 2-byte signed int's "-32768", or a
; float's text -- 11 chars observed empirically, see float.asm's FOUT
; comment). One buffer shared by all of them, not one each: simple, and
; correct for the dominant use case of consuming a ToString() result
; immediately (e.g. Write(x, y, val.ToString())). Two live results held at
; once, or a ToString() call from inside a C64.Interrupt handler racing one
; from mainline code, will corrupt each other -- neither is protected
; (unlike zp_interrupt_save_start's range, this buffer is ordinary RAM, not
; zero page, and OnInterrupt never saves/restores it).
tostring_buffer     .fill 16

; Shared scratch buffer for String_Concat/String_PadLeft (asm/helper/
; stringops.asm) -- deliberately separate from tostring_buffer above: a
; concat's input is often a ToString() result still sitting in
; tostring_buffer (e.g. "L" + levelNumber.ToString()), so writing the
; concat's OUTPUT into that same buffer would corrupt the very input still
; being read mid-copy. 40 bytes covers a full C64 screen row (the practical
; upper bound for anything this buffer feeds into a Write() call), same
; single-shared-buffer/no-interrupt-protection tradeoffs as tostring_buffer
; above -- see that comment for the full reasoning (applies identically
; here: consume the result immediately, don't hold two live at once).
stringops_buffer    .fill 40

heap
.endv
