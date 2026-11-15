using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Mark and compact, one step per press (DECK.md D6). A stepper on top (the
// current step white, the others Grey2); six objects, 1 and 3 roots, each
// object's references in Grey3 under it; the mark stack as a row of slots;
// the heap strip. Grey2 bar = not yet reached, cyan = marked, yellow = the
// object being processed.
sealed class GarbageCollectorSlide : Slide
{
    const uint StepRow = 5, ObjectRow = 7, ObjectHeight = 4, RefsRow = 12, StackRow = 15, HeapRow = 17;
    const uint ObjectWidth = 4, ObjectPitch = 6;
    const uint StripColumn = 8;                       // the stack slots and the heap strip
    const uint SlotWidth = 2, SlotPitch = 3;
    const uint HeapCells = Theme.Right - StripColumn;
    const ulong StepGap = 24;                         // pixels between steps
    static readonly string[] Steps = ["1  Count roots", "2  Mark", "3  Sort", "4  Slide down"];

    // Set in Present.
#pragma warning disable CS8618
    HeapObject[] _objects;
    HeapObject[] _stack;
#pragma warning restore CS8618
    uint _stackCount;

    public override void Present()
    {
        HeapObject one = new() { Id = "1", Size = 4, Root = true }, two = new() { Id = "2", Size = 3 },
            three = new() { Id = "3", Size = 5, Root = true }, four = new() { Id = "4", Size = 3 },
            five = new() { Id = "5", Size = 3 }, six = new() { Id = "6", Size = 5 };
        one.References = [three, four];
        two.References = [one, six];
        three.References = [one, four, five];
        four.References = [one];
        five.References = [three];
        six.References = [one, two, five];
        _objects = [one, two, three, four, five, six];
        _stack = new HeapObject[_objects.Length];

        Master.Content("GC without a scan");
        for (uint i = 0; i < Steps.Length; i++)
            Text.Draw(StepX(i), StepRow, Steps[i], Theme.Muted);
        DrawObjects();
        Label(StackRow, "stack");
        Label(HeapRow + 1, "heap");
        DrawStack();
        DrawHeap(compact: false);

        Step(0);
        foreach (var root in _objects)
            if (root.Root)
                Push(root);

        Step(1);
        while (_stackCount != 0)
        {
            var current = Pop();
            Recolor(current, Theme.Accent2);
            foreach (var reference in current.References!)
                if (!reference.Marked)
                    Push(reference);
            Recolor(current, Theme.Accent);
        }

        Step(2);
        foreach (var o in _objects)
            if (!o.Marked)
            {
                o.Dead = true;
                Recolor(o, Theme.Background);
            }
        DrawHeap(compact: false);

        Step(3);
        DrawHeap(compact: true);
    }

    // Steps sit one after another on one row, each in cells of its own.
    static ulong StepX(uint step)
    {
        ulong x = Screen.Px(Theme.Left);
        for (uint i = 0; i < step; i++)
            x = Screen.CellAfter(x + Text.Width(Steps[i]), StepGap);
        return x;
    }

    // Lights up one step in white; the one before goes back to Grey2.
    static void Step(uint step)
    {
        Deck.Step();
        Deck.Show();
        if (step != 0)
            PaintStep(step - 1, Theme.Muted);
        PaintStep(step, Theme.Heading);
    }

    static void PaintStep(uint step, Colors color)
    {
        ulong x = StepX(step);
        uint column = (uint)(x / Theme.CellPixels);
        Deck.Paint(column, StepRow, (uint)((x + Text.Width(Steps[step]) + 7) / Theme.CellPixels) - column, 1, color, Theme.Background);
    }

    static uint ObjectColumn(uint index) => Theme.Left + index * ObjectPitch;

    void DrawObjects()
    {
        for (uint i = 0; i < _objects.Length; i++)
        {
            var o = _objects[i];
            uint column = ObjectColumn(i);
            new Box
            {
                X = column, Y = ObjectRow, Width = ObjectWidth, Height = ObjectHeight,
                Text = o.Id, Caption = o.Root ? "root" : "", Color = Theme.Muted,
            }.Draw();
            string refs = Theme.Arrow;
            foreach (var r in o.References!)
            {
                if (refs.Length > 1)
                    refs = $"{refs},";
                refs = $"{refs}{r.Id}";
            }
            Text.CenterNow(Screen.Px(column), Screen.Px(ObjectWidth), RefsRow, refs, Theme.Body);
        }
    }

    // The object's accent bar: marked, processing, or gone (box and refs).
    void Recolor(HeapObject o, Colors color)
    {
        Deck.Step();
        uint column = ObjectColumn(IndexOf(o));
        if (color == Theme.Background)
            Deck.Paint(column, ObjectRow, ObjectWidth, RefsRow - ObjectRow + 1, Theme.Background, Theme.Background);
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

    // One slot per object: a Grey2 outline when empty, a chip with the
    // object's id when it holds one; the newest on the right.
    void DrawStack()
    {
        Deck.Show();
        for (uint i = 0; i < _stack.Length; i++)
        {
            uint column = StripColumn + i * SlotPitch;
            ulong x = Screen.Px(column), y = Screen.Px(StackRow);
            Screen.ClearRect(column, StackRow, SlotWidth, 1);
            if (i < _stackCount)
            {
                Deck.Paint(column, StackRow, SlotWidth, 1, Theme.Heading, Theme.Card);
                Text.Center(x, Screen.Px(SlotWidth), StackRow, _stack[i].Id, Theme.Heading, Theme.Card);
            }
            else
            {
                Deck.Paint(column, StackRow, SlotWidth, 1, Theme.Muted, Theme.Background);
                Screen.Frame(x, y, x + Screen.Px(SlotWidth) - 1, y + 7);
            }
        }
    }

    // The heap strip in object order, then the free rest to the content's
    // right edge; compact: the live blocks slid down.
    void DrawHeap(bool compact)
    {
        Deck.Show();
        Deck.Paint(StripColumn, HeapRow, HeapCells, 3, Theme.Background, Theme.Background);
        Screen.ClearRect(StripColumn, HeapRow, HeapCells, 3);
        uint column = StripColumn;
        foreach (var o in _objects)
        {
            if (!o.Dead)
                new Box { X = column, Y = HeapRow, Width = o.Size, Height = 3, Text = o.Id, Color = Theme.Accent, ConnectToLeft = column != StripColumn }.Draw();
            if (!o.Dead || !compact)
                column += o.Size;
        }
        new Box { X = column, Y = HeapRow, Width = Theme.Right - column, Height = 3, Text = "free", Color = Theme.Frame, Hollow = true }.Draw();
    }

    static void Label(uint row, string text) => Text.Draw(Screen.Px(Theme.Left), row, text, Theme.Muted);
}
