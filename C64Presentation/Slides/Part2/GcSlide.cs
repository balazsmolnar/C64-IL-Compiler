using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 11 (DECK.md D6): mark and compact, one press per phase. Left: six
// objects (1 and 3 are roots) with their references; right: the four
// phases, lighting up in turn, and the mark stack, which becomes the sort
// table; below: the heap. An object being traced gets a yellow bar, a
// marked one cyan, card and heap block alike. The sort table lists the live
// handles, then reorders them by address; the dead (2 and 6, a cycle) fade
// out, and the live blocks slide down over the gaps.
//
// Every pause goes through Deck.Wait (and every press through Deck.Step), so
// N / B leave the slide at once, mid-animation.
//
// Each phase is a static method: static methods get fixed-address frames
// (a virtual like Present does not), which keeps the animation small.
sealed class GcSlide : Slide
{
    const uint PhaseColumn = 22, TableRow = 14, TableColumn = 29;
    const uint Left = Theme.Left, Middle = Left + 6, Right = Middle + 6, Top = Theme.BodyTop, Bottom = Top + 7;
    const uint BoxWidth = 5, BoxHeight = 5;
    const uint HeapColumn = 9, HeapRow = 18, HeapHeight = 3;
    const uint IdCells = 2;                          // one id in the stack / sort table
    const uint StepFrames = 20, SlideFrames = 3, FadeFrames = 2, FadeLevelBytes = 16;

    static readonly string[] Phases = ["1 Roots: counted", "2 Mark: follow references", "3 Sort by address", "4 Slide down: compact"];

    static HeapObject[] _objects = null!;              // built by Setup
    static HeapObject[] _heap = null!;                 // the same objects, by address
    static HeapObject[] _table = null!;                // the mark stack, then the sort table
    static uint _count;

    public static Slide Create() => new GcSlide();

    public override void Present()
    {
        Setup();
        CountRoots();
        Mark();
        Sort();
        SlideDown();
    }

    // The objects and the screen. Refs is the caption of References, kept
    // as a constant string (building it at run time costs more than it).
    static void Setup()
    {
        _objects =
        [
            new() { Id = "1", Refs = $"{Theme.Arrow}3,4", References = [3, 4], Size = 4, Root = true, Column = Left, Row = Top },
            new() { Id = "2", Refs = $"{Theme.Arrow}1,6", References = [1, 6], Size = 3, Column = Middle, Row = Top },
            new() { Id = "3", Refs = $"{Theme.Arrow}1,4,5", References = [1, 4, 5], Size = 5, Root = true, Column = Right, Row = Top },
            new() { Id = "4", Refs = $"{Theme.Arrow}1", References = [1], Size = 3, Column = Left, Row = Bottom },
            new() { Id = "5", Refs = $"{Theme.Arrow}3", References = [3], Size = 3, Column = Middle, Row = Bottom },
            new() { Id = "6", Refs = $"{Theme.Arrow}2,5", References = [2, 5], Size = 5, Column = Right, Row = Bottom },
        ];
        _heap = [Object(3), Object(1), Object(6), Object(4), Object(2), Object(5)];
        _table = new HeapObject[_objects.Length];
        _count = 0;

        Master.Content("GC without a scan");
        for (uint i = 0; i < Phases.Length; i++)
            Text.Draw(Screen.Px(PhaseColumn), PhaseRow(i), Phases[i], Theme.Muted);
        TableLabel("mark stack");
        Text.Draw(Screen.Px(Theme.Left), HeapRow + 1, "heap", Theme.Muted);
        uint column = HeapColumn;
        foreach (var o in _heap)
        {
            o.HeapColumn = column;
            column += o.Size;
            DrawObject(o);
            DrawBlock(o);
        }
        DrawFree(column);
    }

    // 1: the roots are known from their counts: no scan.
    static void CountRoots()
    {
        Phase(0);
        foreach (var o in _objects)
            if (o.Root)
                Push(o);
    }

    // 2: trace from the stack.
    static void Mark()
    {
        Phase(1);
        while (_count != 0)
        {
            _count--;
            var current = _table[_count];
            DrawTable();
            Recolor(current, Theme.Accent2);
            foreach (var id in current.References)
                if (!Object(id).Marked)
                    Push(Object(id));
            Recolor(current, Theme.Accent);
        }
    }

    // 3: the live handles in a table, then sorted by heap address; the ids
    // that moved in yellow.
    static void Sort()
    {
        Phase(2);
        TableLabel("sort table");
        foreach (var o in _objects)
            if (o.Marked)
                _table[_count++] = o;
        DrawTable();
        Deck.Wait(StepFrames);
        uint slot = 0;
        foreach (var o in _heap)
            if (o.Marked)
            {
                DrawId(slot, o.Id, _table[slot] == o ? Theme.Heading : Theme.Accent2);
                _table[slot++] = o;
            }
    }

    // 4: the dead dim, are named (2 and 6 reference each other: still
    // garbage) and fade out; the live blocks slide down over the gaps.
    static void SlideDown()
    {
        Phase(3);
        FadeDead(2);
        Screen.ClearRect(PhaseColumn, TableRow, Theme.Right - PhaseColumn, 1);
        Text.Draw(Screen.Px(PhaseColumn), TableRow, $"2 {Theme.Arrow} 6 {Theme.Arrow} 2: a dead cycle", Theme.Body);
        Deck.Wait(StepFrames);
        FadeDead(1);
        FadeDead(0);
        uint column = HeapColumn;
        foreach (var o in _heap)
            if (o.Marked)
            {
                while (o.HeapColumn > column)
                    MoveLeft(o);
                column += o.Size;
            }
        Screen.ClearRect(column, HeapRow, Theme.Right - column, HeapHeight);
        DrawFree(column);
    }

    // The dead objects, card and heap block, at fade level `level` (0 = black).
    static void FadeDead(uint level)
    {
        Deck.Wait(FadeFrames);
        ulong table = C64Address.FromLabel("fade_Levels") + level * FadeLevelBytes;
        foreach (var o in _heap)
            if (!o.Marked)
            {
                C64.Screen.RemapCellColors(o.Column, o.Row, BoxWidth, BoxHeight, table);
                C64.Screen.RemapCellColors(o.HeapColumn, HeapRow, o.Size, HeapHeight, table);
            }
    }

    // One cell to the left: the block is redrawn there, the cell it leaves
    // cleared. (The dead blocks' pixels under it go too: they were black on black.)
    static void MoveLeft(HeapObject o)
    {
        uint column = --o.HeapColumn;
        Deck.Wait(1);
        Screen.ClearRect(column, HeapRow, o.Size + 1, HeapHeight);
        Deck.Paint(column + o.Size, HeapRow, 1, HeapHeight, Theme.Background, Theme.Background);
        DrawBlock(o);
        PaintBlock(o, Theme.Accent);
        Deck.Wait(SlideFrames);
    }

    static HeapObject Object(uint id) => _objects[id - 1];

    static uint PhaseRow(uint phase) => Theme.BodyTop + phase * 2;


    // A press; the phase lights up in white, the one before goes back to Grey2.
    static void Phase(uint phase)
    {
        Deck.Step();
        Deck.Show();
        if (phase != 0)
            Deck.Paint(PhaseColumn, PhaseRow(phase - 1), Theme.Right - PhaseColumn, 1, Theme.Muted, Theme.Background);
        Deck.Paint(PhaseColumn, PhaseRow(phase), Theme.Right - PhaseColumn, 1, Theme.Heading, Theme.Background);
    }

    // The card: its id and its references; "root" in cyan under it.
    static void DrawObject(HeapObject o)
    {
        ulong x = Screen.Px(o.Column), width = Screen.Px(BoxWidth);
        Box.Card(o.Column, o.Row, BoxWidth, BoxHeight, Theme.Muted, Theme.Card);
        Text.Center(x, width, o.Row + 1, o.Id, Theme.Heading, Theme.Card);
        Text.Center(x, width, o.Row + 3, o.Refs, Theme.Body, Theme.Card);
        if (o.Root)
            Text.Center(x, width, o.Row + BoxHeight, "root", Theme.Accent);
    }

    // The heap block: a card with its id, and a black divider down its left
    // edge where it touches the block before it (as Box.ConnectToLeft).
    static void DrawBlock(HeapObject o)
    {
        uint column = o.HeapColumn;
        ulong x = Screen.Px(column);
        Box.Card(column, HeapRow, o.Size, HeapHeight, Theme.Muted, Theme.Card);
        Text.Center(x, Screen.Px(o.Size), HeapRow + 1, o.Id, Theme.Heading, Theme.Card);
        if (column == HeapColumn)
            return;
        Deck.Paint(column, HeapRow + 1, 1, HeapHeight - 1, Theme.Background, Theme.Card);
        Screen.Line(x, Screen.Px(HeapRow + 1), x, Screen.Px(HeapRow + HeapHeight) - 1);
    }

    // The heap block's bar and id in a colour (not its divider).
    static void PaintBlock(HeapObject o, Colors color)
    {
        Deck.Paint(o.HeapColumn, HeapRow, o.Size, 1, color, Theme.Card);
        Deck.Paint(o.HeapColumn + 1, HeapRow + 1, o.Size - 1, HeapHeight - 1, color, Theme.Card);
    }

    static void DrawFree(uint column)
    {
        Box.Card(column, HeapRow, Theme.Right - column, HeapHeight, Theme.Frame, Theme.Background);
        Text.Center(Screen.Px(column), Screen.Px(Theme.Right - column), HeapRow + 1, "free", Theme.Body);
    }

    // The object in a new colour: its card's bar, its heap block's bar and
    // id (text on a Grey1 card stays white); then a pause.
    static void Recolor(HeapObject o, Colors color)
    {
        Deck.Paint(o.Column, o.Row, BoxWidth, 1, color, Theme.Card);
        PaintBlock(o, color);
        Deck.Wait(StepFrames);
    }

    static void Push(HeapObject o)
    {
        o.Marked = true;
        _table[_count++] = o;
        DrawTable();
        Recolor(o, Theme.Accent);
    }

    static void TableLabel(string text)
    {
        Screen.ClearRect(PhaseColumn, TableRow, TableColumn - PhaseColumn, 1);
        Text.Draw(Screen.Px(PhaseColumn), TableRow, text, Theme.Muted);
    }

    // The ids in the table, first on the left; the slots after them empty.
    static void DrawTable()
    {
        for (uint slot = 0; slot < _table.Length; slot++)
            DrawId(slot, slot < _count ? _table[slot].Id : "", Theme.Heading);
    }

    static void DrawId(uint slot, string id, Colors color)
    {
        uint column = TableColumn + slot * IdCells;
        if (column >= Theme.Right)
            return;
        Screen.ClearRect(column, TableRow, IdCells, 1);
        Text.Draw(Screen.Px(column), TableRow, id, color);
    }
}

// One object of the GC slide's little heap. References are ids, so the
// objects can point at each other from their initializers; Refs is the
// same list as the card's caption.
sealed class HeapObject
{
    public required string Id { get; init; }
    public required string Refs { get; init; }               // e.g. "~3,4"
    public required uint[] References { get; init; }
    public uint Size { get; init; }                          // heap cells
    public bool Root { get; init; }
    public uint Column { get; init; }                        // the card's place in the object grid
    public uint Row { get; init; }
    public bool Marked { get; set; }
    public uint HeapColumn { get; set; }
}
