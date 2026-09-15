; ===========================================================================
; Zero page allocation map -- every byte below $90 that this codebase's
; hand-written asm touches is named here, once, so a new routine claiming a
; byte can be checked against every existing claim in one place instead of
; grepping the whole tree for bare "$XX" literals.
;
; This file must be the FIRST .include in every entry point (main.asm,
; hunchback.asm, presentation.asm, unittest.asm), before heap.asm/GC.asm/
; object.asm/etc., so every name here is already defined by the time
; anything else references it.
;
; Sharing a byte across routines that never run at the same time is fine
; and is the normal case below (this is a single-threaded interpreter
; outside of IRQs). What's NOT fine is two roles that could be live at the
; same time within one call chain sharing a byte -- that exact mistake
; (HEAP_POINTER_ORIG aliasing HEAP_POINTER_COMPACTED in sweepAndCompact)
; silently corrupted live heap objects once real .cctor execution added a
; zero-size live object to the heap; see that commit for the full story.
; Before adding a new claim to any byte below, check every existing
; claimant's comment for whether it could ever be live across a call into
; the new one (most likely via GC.Collect() or a C64.Interrupt handler --
; see the warning on C64.Interrupt in C64Lib/C64.cs).
;
; $90-$ff is never claimed here: it's the KERNAL/BASIC's own zero page,
; touched every tick by the default raster/CIA IRQ handler (jiffy clock,
; SCNKEY, cursor blink), which keeps running on real hardware/VICE since
; this codebase never issues a blanket SEI. Nothing below may ever move
; into that range.
;
; $00/$01 is the 6510 CPU port (hardware register, not scratch): $01 is
; written once by #disable_basic_rom (asm/helper/banking.asm) for bank
; switching and is never reused as scratch. $00 (the port's data-direction
; register) is never touched at all -- KERNAL's own reset already leaves it
; correctly configured.
; ===========================================================================

; ---------------------------------------------------------------------------
; Return-address / stack-machine scratch
; ---------------------------------------------------------------------------
; zp_tmp1_low/high: the default "save my own return address" slot used by
; #stack_save_return_adress / #stack_return_to_saved_address at the top and
; bottom of nearly every C64Lib API macro (asm/C64.asm, c64sprite.asm,
; c64Joystick.asm, C64Sound.asm, C64Debug.asm, c64Keys.asm, system.asm) --
; needed because those macros pull their arguments off the stack machine
; after the real 6502 return address is already sitting on the hardware
; stack, so it has to be moved aside first.
zp_tmp1_low = $20
zp_tmp1_high = $21

; zp_tmp2_low/high: a second such slot, needed only where one of the
; routines above calls another one of them before returning (so the outer
; call's saved return address must survive the inner call using zp_tmp1_*
; for its own). Currently only asm/system.asm's Console_WriteLine (which
; calls Console_Write) needs this -- it uses the bare literal $22 rather
; than this name, but it's the exact same byte.
zp_tmp2_low = $22
zp_tmp2_high = $23

; General-purpose one-off scratch, each used by exactly one call site today
; (kept as separate names rather than folded into the param bank below
; since, unlike that bank, nothing else currently claims these bytes at
; all -- reusing them for something new is unusually low-risk).
zp_tmp3 = $2d
zp_tmp4_low = $2e
zp_tmp4_high = $2f
zp_tmp5 = $2c

stackPointer = $24              ; index into the RAM evaluation stack (not the 6502 hardware stack)

zp_interrupt_address_low = $26  ; C64.Interrupt subscriber's address, set by C64_add_Interrupt
zp_interrupt_address_high = $27
zp_interrupt_saved_low = $52    ; whatever was at $0314/$0315 before C64_add_Interrupt installed OnInterrupt
zp_interrupt_saved_high = $53

heap_tmp_pointer = $28          ; resolveObjPtr's output pointer (asm/helper/object.asm) -- aka tmpPointer

; ---------------------------------------------------------------------------
; Generic scratch-parameter bank ($30-$39)
; ---------------------------------------------------------------------------
; Every one of these ten bytes follows the same pattern across four
; unrelated subsystems: pull argument(s) off the evaluation stack into
; here, then operate on them inline before the routine returns. None of
; these subsystems ever call into each other while a value is live here,
; so sharing the same ten bytes across all of them is safe -- but it means
; any new routine claiming one of these bytes must be checked against
; EVERY subsystem listed below, not just the most recently read file.
;
; $30/$31 (zp_param0_low/high) -- claimed by:
;   - object creation (asm/helper/heap.asm newObj/newObjInit/newArrInit,
;     asm/helper/object.asm setObjParams): new object's vtable/descriptor
;     pointer.
;   - screen drawing (asm/C64.asm C64_Set_Screen_Ptr/SetChar_Core/
;     GetChar_Core/C64_Write): computed screen RAM pointer.
;   - string output (asm/system.asm Console_Write/String_get_Length):
;     string character pointer.
;   - 8-bit branch compares (asm/helper/branch.asm branch_equal8 and
;     friends): the "other" operand -- $30 only, single byte, $31 unused
;     by these.
;   - 16-bit multiply (asm/helper/arithmetic.asm mul16): running product
;     accumulator, both bytes -- needed because mul16's other two operand
;     registers (zp_param1/zp_param2) both stay live for the whole loop,
;     unlike mul8 which can keep its accumulator in A alone. mul16 is a
;     leaf macro (calls nothing that also touches $30/$31), so this is
;     the same "sequential reuse, not concurrent" reasoning as the GC
;     entries below, not an actual conflict with any of the above.
zp_param0_low = $30
zp_param0_high = $31

; $32/$33 (zp_param1_low/high) -- claimed by:
;   - object creation (heap.asm newObjInit/newArrInit -> object.asm
;     newObjLInit): initial-values source pointer.
;   - arithmetic/compare (asm/helper/arithmetic.asm): 8-bit ops' single
;     operand ($32 only); 16-bit ops' operand B (the value pulled second).
;   - branch compares (branch.asm's *16 macros): operand B, same
;     convention as arithmetic.asm.
;   - screen drawing (C64.asm): X coordinate parameter ($32 only).
;   - sprite positioning (asm/c64sprite.asm Sprite_set_X): scratch byte
;     for the X-parameter's low bit.
;   - C64_Write (C64.asm): reused mid-routine (after the X-parameter use
;     above has already been consumed) as a second pointer's low/high --
;     sequential reuse within one routine, not a real second role.
;   - GC's trackObjects (asm/GC.asm, mark phase): zp_gc_track_ptr_low ($33
;     only) -- see the GC section below. trackObjects is the first thing
;     GC_Collect calls and returns before fillSortTable/quicksort/
;     sweepAndCompact touch any of $32-$39, so this is sequential reuse.
zp_param1_low = $32
zp_param1_high = $33

; $34/$35 (zp_param2_low/high) -- claimed by:
;   - object creation (heap.asm newObjL, object.asm setObjParams): new
;     object's Size ($34) and ReferenceFields ($35).
;   - arithmetic/compare/branch (arithmetic.asm, branch.asm): 8-bit ops'
;     single operand ($34 only); 16-bit ops' operand A (the value pulled
;     first, i.e. the earlier-pushed one).
;   - screen drawing (C64.asm): Y coordinate parameter ($34 only).
;   - sprite data block (c64sprite.asm Sprite_set_DataBlock): scratch byte.
;   - keyboard scan (asm/c64Keys.asm C64_IsKeyPressed): key row index.
;   - GC's trackObjects (GC.asm, mark phase): zp_gc_track_ptr_high ($34)
;     and zp_gc_track_refcount ($35) -- same reasoning as $33 above.
;   - GC's quicksort (asm/GC.asm): LOW_INDEX/HIGH_INDEX -- see the GC
;     section below; quicksort always finishes (returns) before any of the
;     above could run again within the same GC_Collect call, so this is
;     sequential reuse, not concurrent.
;   - GC's sweepAndCompact (GC.asm): HEAP_POINTER_COMPACTED_LOW/HIGH --
;     same reasoning, runs strictly after quicksort returns.
zp_param2_low = $34
zp_param2_high = $35

; GC's trackObjects (mark phase) needs a 2-byte object pointer straddling
; zp_param1_high/zp_param2_low (not aligned to either slot) plus one more
; byte for a reference-field count -- own names since they don't fit the
; param-bank's slot boundaries cleanly, but they still alias the exact
; same physical bytes as zp_param1_high/zp_param2_low/zp_param2_high, so
; the sharing above still applies.
zp_gc_track_ptr_low = zp_param1_high    ; $33
zp_gc_track_ptr_high = zp_param2_low    ; $34
zp_gc_track_refcount = zp_param2_high   ; $35

; $36/$37 (zp_param3_low/high) -- claimed by:
;   - screen drawing (C64.asm SetChar_Core/GetChar_Core): char parameter
;     ($36 only).
;   - memory/string routines (C64.asm C64_Write/C64_CopyMemory): source
;     pointer (both bytes, via #stack_pull_pointer).
;   - GC's quicksort (GC.asm): PARTITION_LOW_INDEX/PARTITION_HIGH_INDEX.
;   - GC's sweepAndCompact (GC.asm): TMP ($36 only) -- runs after
;     quicksort returns, same reasoning as the $34/$35 entry above.
zp_param3_low = $36
zp_param3_high = $37

; $38/$39 (zp_param4_low/high) -- claimed by:
;   - screen drawing (C64.asm SetChar_Core): color parameter ($38 only).
;   - memory routines (C64.asm C64_CopyMemory/FillMemory/GetMemory):
;     destination pointer (both bytes).
;   - GC's quicksort (GC.asm): PIVOT_LOW uses $39 only (PIVOT_HIGH spills
;     one byte past this bank into $3a -- see below).
zp_param4_low = $38
zp_param4_high = $39

; ---------------------------------------------------------------------------
; GC quicksort/sweepAndCompact -- own names, partly overlapping the param
; bank above (see each entry's reasoning there), partly extending past it
; into bytes nothing else claims.
; ---------------------------------------------------------------------------
LOW_INDEX = zp_param2_low                       ; quicksort: current partition low index
HIGH_INDEX = zp_param2_high                     ; quicksort: current partition high index
PARTITION_LOW_INDEX = zp_param3_low             ; quicksort/partition: scan-from-low cursor
PARTITION_HIGH_INDEX = zp_param3_high           ; quicksort/partition: scan-from-high cursor
PIVOT_LOW = zp_param4_high                      ; quicksort/partition: pivot object's heap address (low)
PIVOT_HIGH = $3a                                ; quicksort/partition: pivot object's heap address (high) -- not part of the param bank, unclaimed elsewhere

HEAP_POINTER_COMPACTED_LOW = zp_param2_low      ; sweepAndCompact: running write cursor into the compacted heap
HEAP_POINTER_COMPACTED_HIGH = zp_param2_high
TMP = zp_param3_low                             ; sweepAndCompact: kept object's size, during its copy loop

; Must NOT alias HEAP_POINTER_COMPACTED_LOW/HIGH -- they previously both
; pointed at the same bytes, which silently turned the whole compaction
; copy into a self-copy (src and dst were the same pointer) and made the
; "compacted" pointer reset to each kept object's own original address
; instead of accumulating. Harmless (data-preserving no-op) as long as
; objects are processed in strictly increasing, non-overlapping address
; order, but whenever two live objects share the same starting address --
; e.g. a zero-size object (a static singleton with no fields, size 0
; passed to #newObj) sitting immediately before another live object --
; quicksort's tie-breaking can process them in either order, and if the
; zero-size one is processed last, HEAP_POINTER_COMPACTED regresses
; backward past the real object's true end. The next GC.Collect() then has
; fillSortTable write its scratch table straight into that
; wrongly-reclaimed, still-live object's field bytes. Confirmed via a
; diagnostic driver against
; GCTest.Repeated_Collect_On_Stable_Object_Does_Not_Corrupt_State:
; heapPointer regressed by exactly the live object's size after the first
; collect, and the second collect's fillSortTable scratch write landed on
; that object's Id field, changing it from 99 to 2.
HEAP_POINTER_ORIG_LOW = $3b                     ; sweepAndCompact: object being copied FROM (its current heap address)
HEAP_POINTER_ORIG_HIGH = $3c

; ---------------------------------------------------------------------------
; Heap allocation / field access scratch (asm/helper/heap.asm)
; ---------------------------------------------------------------------------
heapPointer = $fb                ; low byte / high byte: next free heap address (the allocation bump pointer)
                                  ; ($fc, paired via heapPointer+1 -- not given its own name)

zp_ctor_result = $fa             ; newObj/newObjInit: preserves the new object's id across "jsr \ctor"
zp_field_value_low = $fd         ; stfld8/16, stelem/stelemRef/stelem16: value being stored, low byte
zp_field_value_high = $fe        ; stfld16, stelem16: value being stored, high byte
