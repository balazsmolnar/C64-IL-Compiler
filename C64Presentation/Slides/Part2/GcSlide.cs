using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Slide 14 (DECK.md D6): mark and compact, one step per press. Left: six
// objects (1 and 3 are roots), each with its references; right: the four
// phases, lighting up in turn, and the mark stack; below: the heap in
// address order. Bars: Grey2 = not reached, yellow = being traced, cyan =
// marked. Unmarked objects fade out, the sort lists the live ones by
// address, and the live blocks slide down over the gaps.
//
// The state is static and each phase a static method of its own: static
// methods get fixed-address frames (Present, a virtual, does not), which
// keeps the animation small.
sealed class GcSlide : Slide
{
    const uint PhaseColumn = 22, StackRow = 15, HeapRow = 18, HeapHeight = 3;
    const uint BoxWidth = 5, BoxHeight = 5, BoxPitch = 6, RowPitch = 7, PerRow = 3;
    const uint StripColumn = 8, SlotWidth = 2;
    const uint SlideFrames = 3, FadeLevels = 3;    // fade_Levels 2, 1, 0 (black)
    static readonly uint[] AddressOrder = [3, 1, 6, 4, 2, 5];    // the heap, low addresses first
    static readonly string[] Phases = ["1 Roots: counted", "2 Mark: follow references", "3 Sort by address", "4 Slide down: compact"];

    // Set in Setup.
#pragma warning disable CS8618
    static HeapObject[] _objects;
    static HeapObject[] _heap;           // the same objects in address order
    static HeapObject[] _stack;
#pragma warning restore CS8618
    static uint _stackCount;

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
        _objects =
        [
            new() { Id = "1", Size = 4, Root = true, References = [3, 4] },
            new() { Id = "2", Size = 3, References = [1, 6] },
            new() { Id = "3", Size = 5, Root = true, References = [1, 4, 5] },
            new() { Id = "4", Size = 3, References = [1] },
            new() { Id = "5", Size = 3, References = [3] },
            new() { Id = "6", Size = 5, References = [2, 5] },
        ];
        _heap = new HeapObject[_objects.Length];
        _stack = new HeapObject[_objects.Length];
        for (uint i = 0; i < _objects.Length; i++)
        {
            _objects[i].Index = i;
            _heap[i] = Object(AddressOrder[i]);
        }
        _stackCount = 0;

        Master.Content("GC without a scan");
        for (uint i = 0; i < Phases.Length; i++)
            Text.Draw(Screen.Px(PhaseColumn), PhaseRow(i), Phases[i], Theme.Muted);
        uint column = StripColumn;
        foreach (var o in _heap)
        {
            DrawObject(o);
            o.HeapColumn = column;
            DrawBlock(o);
            column += o.Size;
        }
        DrawFree(column);
        StackLabel("mark stack");
        Text.Draw(Screen.Px(Theme.Left), HeapRow + 1, "heap", Theme.Muted);
        DrawStack();
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
        while (_stackCount != 0)
        {
            var current = _stack[--_stackCount];
            DrawStack();
            Recolor(current, Theme.Accent2);
            foreach (var id in current.References)
                if (!Object(id).Marked)
                    Push(Object(id));
            Recolor(current, Theme.Accent);
        }
    }

    // 3: the unmarked fade; the marked, listed by address.
    static void Sort()
    {
        Phase(2);
        foreach (var o in _objects)
            if (!o.Marked)
                FadeOut(ObjectColumn(o), ObjectRow(o), BoxWidth, BoxHeight + 1);
        Deck.Step();
        Deck.Show();
        foreach (var o in _heap)
            if (o.Marked)
                _stack[_stackCount++] = o;
        StackLabel("by address");
        DrawStack();
    }

    // 4: the gaps go, the live blocks slide down over them.
    static void SlideDown()
    {
        Phase(3);
        foreach (var o in _heap)
            if (!o.Marked)
                FadeOut(o.HeapColumn, HeapRow, o.Size, HeapHeight);
        uint column = StripColumn;
        foreach (var o in _heap)
        {
            if (!o.Marked)
                continue;
            while (o.HeapColumn > column)
            {
                o.HeapColumn--;
                C64.Screen.WaitForVBlank();
                Screen.ClearRect(o.HeapColumn, HeapRow, o.Size + 1, HeapHeight);
                Deck.Paint(o.HeapColumn + o.Size, HeapRow, 1, HeapHeight, Theme.Background, Theme.Background);
                DrawBlock(o);
                for (uint frame = 1; frame < SlideFrames; frame++)
                    C64.Screen.WaitForVBlank();
            }
            column += o.Size;
        }
        Screen.ClearRect(column, HeapRow, Theme.Right - column, HeapHeight);
        DrawFree(column);
    }
    static HeapObject Object(uint id) => _objects[id - 1];

    static uint PhaseRow(uint phase) => Theme.BodyTop + phase * 2;

    static uint ObjectColumn(HeapObject o) => Theme.Left + o.Index % PerRow * BoxPitch;

    static uint ObjectRow(HeapObject o) => Theme.BodyTop + o.Index / PerRow * RowPitch;

    // Lights up one phase in white; the one before goes back to Grey2.
    static void Phase(uint phase)
    {
        Deck.Step();
        Deck.Show();
        if (phase != 0)
            Deck.Paint(PhaseColumn, PhaseRow(phase - 1), Theme.Right - PhaseColumn, 1, Theme.Muted, Theme.Background);
        Deck.Paint(PhaseColumn, PhaseRow(phase), Theme.Right - PhaseColumn, 1, Theme.Heading, Theme.Background);
    }

    // The box: its id, and its references under it; "root" in cyan below.
    static void DrawObject(HeapObject o)
    {
        uint column = ObjectColumn(o), row = ObjectRow(o);
        string refs = Theme.Arrow;
        foreach (var id in o.References)
            refs = $"{(refs.Length > 1 ? $"{refs}," : refs)}{Object(id).Id}";
        new Box { X = column, Y = row, Width = BoxWidth, Height = BoxHeight, Text = o.Id, Caption = refs, Color = Theme.Muted }.Draw();
        if (o.Root)
            Text.Center(Screen.Px(column), Screen.Px(BoxWidth), row + BoxHeight, "root", Theme.Accent);
    }

    static void DrawBlock(HeapObject o)
        => new Box { X = o.HeapColumn, Y = HeapRow, Width = o.Size, Height = HeapHeight, Text = o.Id, ConnectToLeft = o.HeapColumn != StripColumn }.Draw();

    static void DrawFree(uint column)
        => new Box { X = column, Y = HeapRow, Width = Theme.Right - column, Height = HeapHeight, Text = "free", Color = Theme.Frame, Hollow = true }.Draw();

    // A press, then the object's accent bar in a new colour.
    static void Recolor(HeapObject o, Colors color)
    {
        Deck.Step();
        Deck.Paint(ObjectColumn(o), ObjectRow(o), BoxWidth, 1, color, Theme.Card);
    }

    static void Push(HeapObject o)
    {
        o.Marked = true;
        _stack[_stackCount++] = o;
        DrawStack();
        Recolor(o, Theme.Accent);
    }

    static void StackLabel(string text)
    {
        Screen.ClearRect(PhaseColumn, StackRow - 2, Theme.Right - PhaseColumn, 1);
        Text.Draw(Screen.Px(PhaseColumn), StackRow - 2, text, Theme.Muted);
    }

    // One slot per object, side by side, the top of the stack on the right:
    // a frame around the id row, Grey2 when empty, white with the id when full.
    static void DrawStack()
    {
        Deck.Show();
        for (uint i = 0; i < _stack.Length; i++)
        {
            uint column = PhaseColumn + i * SlotWidth;
            ulong x = Screen.Px(column), y = Screen.Px(StackRow);
            bool full = i < _stackCount;
            Screen.ClearRect(column, StackRow, SlotWidth, 1);
            Deck.Paint(column, StackRow - 1, SlotWidth, 3, full ? Theme.Heading : Theme.Muted, Theme.Background);
            Screen.Frame(x, y - 3, x + Screen.Px(SlotWidth) - 2, y + 10);
            if (full)
                Text.Center(x, Screen.Px(SlotWidth) - 1, StackRow, _stack[i].Id, Theme.Heading);
        }
    }

    // Fades a rectangle of cells to black through the deck's fade levels,
    // then clears its pixels.
    static void FadeOut(uint column, uint row, uint width, uint height)
    {
        for (uint level = FadeLevels; level != 0;)
        {
            level--;
            C64.Screen.WaitForVBlank();
            C64.Screen.WaitForVBlank();
            C64.Screen.RemapCellColors(column, row, width, height, C64Address.FromLabel("fade_Levels") + (ulong)(level << 4));
        }
        Screen.ClearRect(column, row, width, height);
    }
}

// One object of the GC slide's little heap. (No property initializers: the
// compiler runs none, so every flag starts false.)
sealed class HeapObject
{
    public required string Id { get; init; }
    public uint Index { get; set; }            // place in the object grid
    public uint Size { get; init; }            // heap cells
    public bool Root { get; init; }
    public required uint[] References { get; init; }   // ids
    public bool Marked { get; set; }
    public uint HeapColumn { get; set; }
}