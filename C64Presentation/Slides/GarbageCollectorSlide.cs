using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Mark and compact, one step per press (DECK.md D6). Six objects, 1 and 3
// roots; each object's references in Grey3 under it; the mark stack; the
// heap strip. Grey2 = not yet reached, cyan = marked, yellow = the object
// being processed. The phase on the right lights up as it starts.
sealed class GarbageCollectorSlide : Slide
{
    const uint ObjectRow = 6, RefsRow = 9, StackRow = 13, HeapRow = 17;
    const uint ObjectWidth = 3, ObjectPitch = 4, StackCellWidth = 2;
    const uint PhaseColumn = 28;
    static readonly string[] Phases = ["1 Roots", "2 Mark", "3 Sort", "4 Slide down"];
    static readonly string[] PhaseDetails = ["counted", "follow refs", "by address", "compact"];

    // Set in Present: the compiler runs no field initializers.
#pragma warning disable CS8618
    HeapObject[] _objects;
    HeapObject[] _stack;
#pragma warning restore CS8618
    uint _stackCount;
    uint _heapCellsDrawn;

    public override void Present()
    {
        HeapObject one = new() { Id = "1", Size = 2, Root = true }, two = new() { Id = "2", Size = 2 },
            three = new() { Id = "3", Size = 4, Root = true }, four = new() { Id = "4", Size = 2 },
            five = new() { Id = "5", Size = 2 }, six = new() { Id = "6", Size = 4 };
        one.References = [three, four];
        two.References = [one, six];
        three.References = [one, four, five];
        four.References = [one];
        five.References = [three];
        six.References = [one, two, five];
        _objects = [one, two, three, four, five, six];
        _stack = new HeapObject[_objects.Length];

        Master.Content("GC without a scan");
        DrawObjects();
        DrawPhases();
        Label(StackRow - 1, "mark stack");
        Label(HeapRow - 1, "heap");
        DrawHeap();

        Phase(0);
        foreach (var root in _objects)
            if (root.Root)
                Push(root);

        Phase(1);
        while (_stackCount != 0)
        {
            var current = Pop();
            Recolor(current, Theme.Accent2);
            foreach (var reference in current.References!)
                if (!reference.Marked)
                    Push(reference);
            Recolor(current, Theme.Accent);
        }

        Phase(2);
        foreach (var o in _objects)
            if (!o.Marked)
            {
                o.Dead = true;
                Recolor(o, Theme.Background);
            }
        DrawHeap();

        Phase(3);
        DrawHeap(compact: true);
    }

    static ulong ObjectX(uint index) => Screen.Px(Theme.Left + index * ObjectPitch);

    void DrawObjects()
    {
        for (uint i = 0; i < _objects.Length; i++)
        {
            var o = _objects[i];
            uint column = Theme.Left + i * ObjectPitch;
            new Box { X = column, Y = ObjectRow, Width = ObjectWidth, Height = 3, Text = o.Id, Color = Theme.Muted }.Draw();
            if (o.Root)
                Text.Center(ObjectX(i), Screen.Px(ObjectWidth), ObjectRow - 1, "root", Theme.Body);
            string refs = Theme.Arrow;
            foreach (var r in o.References!)
                refs = $"{refs}{r.Id}";
            Text.Center(ObjectX(i), Screen.Px(ObjectPitch), RefsRow, refs, Theme.Body);
        }
    }

    // The object's accent bar: marked, processing, or gone.
    void Recolor(HeapObject o, Colors color)
    {
        Deck.Step();
        uint column = Theme.Left + IndexOf(o) * ObjectPitch;
        if (color == Theme.Background)
            Deck.Paint(column, ObjectRow - 1, ObjectWidth, 5, Theme.Background, Theme.Background);
        else
            Deck.Paint(column, ObjectRow, ObjectWidth, 1, color, Theme.Card);
    }

    uint IndexOf(HeapObject o)
    {
        uint i = 0;
        while (_objects[i] != o)
            i++;
        return i;
    }

    void Push(HeapObject o)
    {
        o.Marked = true;
        _stack[_stackCount++] = o;
        DrawStack();
        Recolor(o, Theme.Accent);
    }

    HeapObject Pop()
    {
        var o = _stack[--_stackCount];
        DrawStack();
        return o;
    }

    // The stack as a row of chips, the newest on the right.
    void DrawStack()
    {
        Deck.Show();
        Deck.Paint(Theme.Left, StackRow, StackCellWidth * (uint)_stack.Length, 1, Theme.Background, Theme.Background);
        Screen.ClearRect(Theme.Left, StackRow, StackCellWidth * (uint)_stack.Length, 1);
        for (uint i = 0; i < _stackCount; i++)
        {
            uint column = Theme.Left + i * StackCellWidth;
            Deck.Paint(column, StackRow, StackCellWidth - 1, 1, Theme.Heading, Theme.Card);
            Text.Center(Screen.Px(column), Screen.Px(StackCellWidth - 1), StackRow, _stack[i].Id, Theme.Heading, Theme.Card);
        }
    }

    // The heap strip in object order; compact: the live blocks slid down.
    void DrawHeap(bool compact = false)
    {
        Deck.Show();
        if (_heapCellsDrawn != 0)
        {
            Deck.Paint(Theme.Left, HeapRow, _heapCellsDrawn, 3, Theme.Background, Theme.Background);
            Screen.ClearRect(Theme.Left, HeapRow, _heapCellsDrawn, 3);
        }
        uint column = Theme.Left;
        foreach (var o in _objects)
        {
            if (!o.Dead)
                new Box { X = column, Y = HeapRow, Width = o.Size, Height = 3, Text = o.Id, Color = Theme.Accent, ConnectToLeft = column != Theme.Left }.Draw();
            if (!o.Dead || !compact)
                column += o.Size;
        }
        _heapCellsDrawn = column - Theme.Left;
        new Box { X = column, Y = HeapRow, Width = 26 - _heapCellsDrawn, Height = 3, Text = "free", Color = Theme.Frame, Hollow = true }.Draw();
        _heapCellsDrawn = 26;
    }

    static void DrawPhases()
    {
        for (uint i = 0; i < Phases.Length; i++)
        {
            Text.Draw(Screen.Px(PhaseColumn), PhaseRow(i), Phases[i], Theme.Muted);
            Text.Draw(Screen.Px(PhaseColumn), PhaseRow(i) + 1, PhaseDetails[i], Theme.Muted);
        }
    }

    static uint PhaseRow(uint phase) => Theme.BodyTop + 1 + phase * 3;

    // Lights up one phase (white name, Grey3 detail), dims the one before.
    static void Phase(uint phase)
    {
        Deck.Step();
        Deck.Show();
        if (phase != 0)
            Deck.Paint(PhaseColumn, PhaseRow(phase - 1), Theme.Right - PhaseColumn, 2, Theme.Muted, Theme.Background);
        Deck.Paint(PhaseColumn, PhaseRow(phase), Theme.Right - PhaseColumn, 1, Theme.Heading, Theme.Background);
        Deck.Paint(PhaseColumn, PhaseRow(phase) + 1, Theme.Right - PhaseColumn, 1, Theme.Body, Theme.Background);
    }

    static void Label(uint row, string text) => Text.Draw(Screen.Px(Theme.Left), row, text, Theme.Muted);
}
