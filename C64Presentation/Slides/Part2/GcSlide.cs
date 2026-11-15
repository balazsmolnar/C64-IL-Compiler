using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 14 (DECK.md D6): mark and compact, one press per phase. Left: six
// objects (1 and 3 are roots) with their references; right: the four
// phases, lighting up in turn, and the mark stack; below: the heap, by
// address. An object being traced turns yellow, a marked one cyan, box and
// heap block alike. Then the live blocks light up in address order, the
// dead ones (2 and 6, a cycle) fade, and the live blocks slide down.
//
// Each phase is a static method: static methods get fixed-address frames
// (a virtual like Present does not), which keeps the animation small.
sealed class GcSlide : Slide
{
    const uint PhaseColumn = 22, StackRow = 14, NoteRow = 16;
    const uint BoxWidth = 5, BoxHeight = 5, BoxPitch = 6, RowPitch = 7, PerRow = 3;
    const uint HeapColumn = 9, HeapRow = 18, HeapHeight = 3;
    const uint StackColumn = 30;                     // the ids on the stack line
    const ulong IdPitch = 10;                        // pixels
    const uint StepFrames = 20, SlideFrames = 3, FadeFrames = 2, FadeLevelBytes = 16;

    static readonly string[] Phases = ["1 Roots: counted", "2 Mark: follow references", "3 Sort by address", "4 Slide down: compact"];
    static readonly uint[] FadeLevels = [2, 1, 0];   // fade_Levels: dimmer, dimmer, black

    static readonly HeapObject[] Objects =
    [
        new() { Id = "1", Refs = $"{Theme.Arrow}3,4", References = [3, 4], Size = 4, Root = true },
        new() { Id = "2", Refs = $"{Theme.Arrow}1,6", References = [1, 6], Size = 3 },
        new() { Id = "3", Refs = $"{Theme.Arrow}1,4,5", References = [1, 4, 5], Size = 5, Root = true },
        new() { Id = "4", Refs = $"{Theme.Arrow}1", References = [1], Size = 3 },
        new() { Id = "5", Refs = $"{Theme.Arrow}3", References = [3], Size = 3 },
        new() { Id = "6", Refs = $"{Theme.Arrow}2,5", References = [2, 5], Size = 5 },
    ];
    static readonly HeapObject[] Heap = [Object(3), Object(1), Object(6), Object(4), Object(2), Object(5)];   // by address
    static readonly HeapObject[] Stack = new HeapObject[Objects.Length];
    static uint _depth;

    public static Slide Create() => new GcSlide();

    public override void Present()
    {
        Setup();
        CountRoots();
        Mark();
        Sort();
        SlideDown();
    }

    static void Setup()
    {
        Master.Content("GC without a scan");
        for (uint i = 0; i < Phases.Length; i++)
            Text.Draw(Screen.Px(PhaseColumn), PhaseRow(i), Phases[i], Theme.Muted);
        Text.Draw(Screen.Px(PhaseColumn), StackRow, "mark stack", Theme.Muted);
        Text.Draw(Screen.Px(Theme.Left), HeapRow, "heap,", Theme.Muted);
        Text.Draw(Screen.Px(Theme.Left), HeapRow + 2, "by address", Theme.Muted);
        for (uint i = 0; i < Objects.Length; i++)
        {
            bool second = i >= PerRow;      // the grid's second row
            Objects[i].Reset(Theme.Left + (second ? i - PerRow : i) * BoxPitch, second ? Theme.BodyTop + RowPitch : Theme.BodyTop);
        }
        uint column = HeapColumn;
        foreach (var o in Heap)
        {
            o.HeapColumn = column;
            column += o.Size;
            DrawObject(o);
            DrawBlock(o, Theme.Muted);
        }
        DrawFree(column);
        _depth = 0;
    }

    // 1: the roots are known from their counts: no scan.
    static void CountRoots()
    {
        Phase(0);
        foreach (var o in Objects)
            if (o.Root)
                Push(o);
    }

    // 2: trace from the stack.
    static void Mark()
    {
        Phase(1);
        while (_depth != 0)
        {
            _depth--;
            var current = Stack[_depth];
            DrawStack();
            Recolor(current, Theme.Accent2);
            foreach (var id in current.References)
                if (!Object(id).Marked)
                    Push(Object(id));
            Recolor(current, Theme.Accent);
        }
    }

    // 3: the live blocks, in address order.
    static void Sort()
    {
        Phase(2);
        foreach (var o in Heap)
            if (o.Marked)
            {
                Recolor(o, Theme.Heading);
                Recolor(o, Theme.Accent);
            }
    }

    // 4: the dead fade out together (2 and 6 reference each other: still
    // garbage), and the live blocks slide down over the gaps.
    static void SlideDown()
    {
        Phase(3);
        foreach (var level in FadeLevels)
        {
            Wait(FadeFrames);
            ulong table = C64Address.FromLabel("fade_Levels") + level * FadeLevelBytes;
            foreach (var o in Heap)
                if (!o.Marked)
                {
                    C64.Screen.RemapCellColors(o.Column, o.Row, BoxWidth, BoxHeight, table);
                    C64.Screen.RemapCellColors(o.HeapColumn, HeapRow, o.Size, HeapHeight, table);
                }
        }
        Text.Draw(Screen.Px(PhaseColumn), NoteRow, $"2 {Theme.Arrow} 6 {Theme.Arrow} 2: a dead cycle", Theme.Body);
        uint column = HeapColumn;
        foreach (var o in Heap)
            if (o.Marked)
            {
                while (o.HeapColumn > column)
                    MoveLeft(o);
                column += o.Size;
            }
        Screen.ClearRect(column, HeapRow, Theme.Right - column, HeapHeight);
        DrawFree(column);
    }

    // One cell to the left: the block is redrawn there, the cell it leaves
    // cleared. (The dead blocks' pixels under it go too: they were black on black.)
    static void MoveLeft(HeapObject o)
    {
        uint column = --o.HeapColumn;
        C64.Screen.WaitForVBlank();
        Screen.ClearRect(column, HeapRow, o.Size + 1, HeapHeight);
        Deck.Paint(column + o.Size, HeapRow, 1, HeapHeight, Theme.Background, Theme.Background);
        DrawBlock(o, Theme.Accent);
        Wait(SlideFrames);
    }
    static HeapObject Object(uint id) => Objects[id - 1];

    static uint PhaseRow(uint phase) => Theme.BodyTop + phase * 2;


    static void Wait(uint frames)
    {
        for (uint frame = 0; frame < frames; frame++)
            C64.Screen.WaitForVBlank();
    }

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
        uint column = o.Column, row = o.Row;
        ulong x = Screen.Px(column), width = Screen.Px(BoxWidth);
        Box.Card(column, row, BoxWidth, BoxHeight, Theme.Muted, Theme.Card);
        Text.Center(x, width, row + 1, o.Id, Theme.Heading, Theme.Card);
        Text.Center(x, width, row + 3, o.Refs, Theme.Body, Theme.Card);
        if (o.Root)
            Text.Center(x, width, row + BoxHeight, "root", Theme.Accent);
    }

    // The heap block: a card with the id and a divider down its left edge,
    // in the object's colour (white on a not yet reached, Grey2 one).
    static void DrawBlock(HeapObject o, Colors color)
    {
        ulong x = Screen.Px(o.HeapColumn);
        var ink = color == Theme.Muted ? Theme.Heading : color;
        Box.Card(o.HeapColumn, HeapRow, o.Size, HeapHeight, color, Theme.Card);
        Deck.Paint(o.HeapColumn, HeapRow + 1, o.Size, HeapHeight - 1, ink, Theme.Card);
        Text.Center(x, Screen.Px(o.Size), HeapRow + 1, o.Id, ink, Theme.Card);
        Screen.Line(x, Screen.Px(HeapRow + 1), x, Screen.Px(HeapRow + HeapHeight) - 1);
    }

    static void DrawFree(uint column)
    {
        Box.Card(column, HeapRow, Theme.Right - column, HeapHeight, Theme.Frame, Theme.Background);
        Text.Center(Screen.Px(column), Screen.Px(Theme.Right - column), HeapRow + 1, "free", Theme.Body);
    }

    // The object, box and heap block, in a new colour; then a pause.
    static void Recolor(HeapObject o, Colors color)
    {
        Deck.Paint(o.Column, o.Row, BoxWidth, BoxHeight, color, Theme.Card);
        Deck.Paint(o.HeapColumn, HeapRow, o.Size, HeapHeight, color, Theme.Card);
        Wait(StepFrames);
    }

    static void Push(HeapObject o)
    {
        o.Marked = true;
        Stack[_depth++] = o;
        DrawStack();
        Recolor(o, Theme.Accent);
    }

    // The ids on the stack, bottom first, after the label.
    static void DrawStack()
    {
        Screen.ClearRect(StackColumn, StackRow, Theme.Right - StackColumn, 1);
        Deck.Paint(StackColumn, StackRow, Theme.Right - StackColumn, 1, Theme.Heading, Theme.Background);
        ulong x = Screen.Px(StackColumn);
        for (uint i = 0; i < _depth; i++)
            Text.Draw(x + i * IdPitch, StackRow, Stack[i].Id, Theme.Heading);
    }
}

// One object of the GC slide's little heap. References are ids, so the
// objects can point at each other from their initializers.
sealed class HeapObject
{
    public required string Id { get; init; }
    public required string Refs { get; init; }               // the caption, e.g. "~3,4"
    public required uint[] References { get; init; }
    public uint Size { get; init; }                          // heap cells
    public bool Root { get; init; }
    public uint Column { get; private set; }                 // the card's place in the object grid
    public uint Row { get; private set; }
    public bool Marked { get; set; }
    public uint HeapColumn { get; set; }

    public void Reset(uint column, uint row)
    {
        Column = column;
        Row = row;
        Marked = false;
    }
}
