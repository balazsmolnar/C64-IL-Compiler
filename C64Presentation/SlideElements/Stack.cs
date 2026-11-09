using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// The GC slide's mark stack: a column of small cells growing up from row Y
// (the newest on top), redrawn after each push or pull.
class Stack : SlideElement
{
    private ObjectElem[] elements;
    private int pointer;
    private uint drawn;            // rows drawn last time

    public uint X;
    public uint Y;
    public const uint Width = 4;

    public void Init(int maxSize)
    {
        elements = new ObjectElem[maxSize];
        pointer = -1;
    }

    public void Push(ObjectElem elem)
    {
        pointer++;
        elements[pointer] = elem;
    }

    public ObjectElem Pull()
    {
        var result = elements[pointer];
        pointer--;
        return result;
    }

    public bool IsEmpty => pointer == -1;

    public override void Draw()
    {
        uint count = (uint)(pointer + 1);
        // Hide and clear what was there, then draw the cells bottom up.
        if (drawn != 0)
        {
            Deck.Paint(X, Y + 1 - drawn, Width, drawn, Theme.Background, Theme.Background);
            Helper.Screen.ClearRect(X, Y + 1 - drawn, Width, drawn);
        }
        for (uint i = 0; i < count; i++)
        {
            uint row = Y - i;
            bool top = i + 1 == count;
            Colors fill = top ? Theme.Accent2 : Theme.Card;
            Deck.Paint(X, row, Width, 1, top ? Colors.Black : Theme.Heading, fill);
            Text.Center(Helper.Screen.X(X), Helper.Screen.X(Width), row, elements[i].Id, top ? Colors.Black : Theme.Heading, fill);
        }
        drawn = count;
    }
}
