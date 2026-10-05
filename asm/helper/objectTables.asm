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
; Table placement: floats right after code, as long as that's still below
; OBJ_TABLES_MAX_START (the entry template defines this, and the identical
; OBJ_TABLES_FALLBACK, before including this file). Those two names used to
; suggest a real "try floating, else fall back to a different safe place"
; scheme, but both templates set them to the SAME value -- there never was
; a second, safe address to fall back to, just "if code reached here,
; forcibly pin the tables here regardless." Since the tables themselves are
; .virtual (assigns addresses, emits zero real bytes -- see above), pinning
; * BACKWARD into a region that already has real, assembled code sitting in
; it doesn't error or even look wrong at assembly time: it just makes
; objTableLow/etc alias whatever code is physically already there. The
; failure shows up only at RUNTIME, when the startup object-table-clearing
; loop overwrites that aliased code with zeros before anything gets a
; chance to run it -- confirmed to happen for real once already (Test/*.cs
; grew past OBJ_TABLES_MAX_START, silently zeroing out
; Assert_AreEqualString's own machine code before any test could call it;
; every test using it failed with no useful error, since the corruption
; happened during otherwise-unrelated startup code, long before whichever
; test method was actually being dispatched).
;
; Turned into a hard compile-time failure instead: if code is still at or
; past OBJ_TABLES_MAX_START here, there is no safe address left to use, so
; fail loudly now rather than silently corrupt memory at runtime. Raising
; OBJ_TABLES_MAX_START/FALLBACK is possible in the entry template (check
; headroom against the next fixed boundary first -- for unittest builds,
; that's $e000, KERNAL ROM, still mapped even with BASIC banked out; see
; asm/helper/floatBanking.asm's bank_out_basic_rom using $06, not a value
; that also clears HIRAM), but the real fix, once available headroom is
; gone, is to shrink the compiled program.
;
; The check itself moved below (see the one right before .endv): checking
; `*` HERE only verifies the block's own STARTING address, never where it
; ENDS -- and this block is 2104 bytes wide (every .fill below, not just
; the "2048 bytes" the message names), while OBJ_TABLES_MAX_START ($d000)
; is the floor of real VIC-II/SID/CIA I/O space, not a soft "probably fine"
; marker. A block that starts safely below $d000 can -- and, once
; Test/*.cs's unittest.asm grew enough, routinely does -- still run past
; it: with objTableLow starting at $ce8b (just 365 bytes of headroom) and
; this block needing 2104, every table here already overlaps $d000-$dfff
; by construction, regardless of program size elsewhere. On real hardware
; (CHAREN is always 1 in this project) that range is never plain RAM --
; it's live VIC-II/SID/CIA registers -- so any object id whose table
; entry lands past $d000 (id >= ~117 here) reads/writes a HARDWARE
; REGISTER instead of its own data. Confirmed via SimpleEmulator's own
; (accurate) raster-line simulation at $d012 making a GC quicksort compare
; a value against itself and get "not equal" -- not a quicksort bug, not
; an emulator bug: objTableHigh[135] really is $d012 in this build, and
; reads of it never return the same thing twice.

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
; THE real check: `heap`'s own address is exactly "where this virtual
; block ends" (every .fill above it, in order), so checking IT against
; OBJ_TABLES_MAX_START verifies the block's full 2104-byte extent, not
; just its starting point -- see this file's own top-of-file comment for
; why checking `*` before `.virtual` instead (the original version of this
; check) missed exactly the failure this exists to catch.
.cerror heap >= OBJ_TABLES_MAX_START, "Compiled program is too large: the object/GC tables + localsStack + tostring_buffer/stringops_buffer (2104 bytes) can no longer fit before OBJ_TABLES_MAX_START without overlapping real VIC-II/SID/CIA I/O space ($d000-$dfff on real hardware, CHAREN=1). Raise OBJ_TABLES_MAX_START/OBJ_TABLES_FALLBACK in the entry template (checking headroom against the next fixed boundary), or shrink the program."
.endv
