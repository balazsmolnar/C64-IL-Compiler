using System;
using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides;

// Mark and compact, one step per press: six objects (1 and 3 are roots),
// the mark stack, the heap. Most of the animation is colour only: cards
// are drawn once and recoloured; the heap is redrawn when it compacts.
class GarbageCollectorSlide : Slide
{
    private ObjectElem[] objects;
    private Stack stack;
    private bool drawn;
    private bool heapClean;

    public override void Present()
    {
        objects = new[]
        {
            new ObjectElem() {Id = "1", Alive = true, Size = 3, Root = true },
            new ObjectElem() {Id = "2", Alive = true, Size = 3 },
            new ObjectElem() {Id = "3", Alive = true, Size = 7, Root = true },
            new ObjectElem() {Id = "4", Alive = true, Size = 3 },
            new ObjectElem() {Id = "5", Alive = true, Size = 3 },
            new ObjectElem() {Id = "6", Alive = true, Size = 7 },
        };

        objects[0].References = new[] { objects[2], objects[3] };
        objects[1].References = new[] { objects[0], objects[5] };
        objects[2].References = new[] { objects[0], objects[3], objects[4] };
        objects[3].References = new[] { objects[0] };
        objects[4].References = new[] { objects[2] };
        objects[5].References = new[] { objects[0], objects[1], objects[4] };

        stack = new Stack() { X = 18, Y = 14 };
        stack.Init(objects.Length);

        Master.Content("Garbage collector");
        Text.Draw(16, 10, "Mark stack", Theme.Muted);
        Text.Draw(16, 16, "Heap", Theme.Muted);
        RefreshWait();
        AddRoots();
        MarkObjects();
        Kill();
    }

    private void AddRoots()
    {
        foreach (var objectElem in objects)
        {
            if (objectElem.Root)
            {
                objectElem.Marked = true;
                stack.Push(objectElem);
                RefreshWait();
            }
        }
    }

    private void MarkObjects()
    {
        while (!stack.IsEmpty)
        {
            var obj = stack.Pull();
            obj.Processing = true;
            RefreshWait();
            foreach (var reference in obj.References)
            {
                if (!reference.Marked)
                {
                    stack.Push(reference);
                    reference.Marked = true;
                    RefreshWait();
                }
            }

            obj.Processing = false;
        }
    }

    private void Kill()
    {
        foreach (var objectElem in objects)
        {
            if (!objectElem.Marked)
            {
                objectElem.Alive = false;
                heapClean = false;
                RefreshWait();
            }
        }
    }

    private void RefreshWait()
    {
        Draw();
        Deck.Step();
        Deck.Show();
    }

    private void Draw()
    {
        uint x = Theme.Left;
        foreach (var o in objects)
        {
            if (!drawn)
            {
                Box.DrawCard(x, 5, 5, 3, Theme.Muted, Theme.Card);
                ulong px = Helper.Screen.X(x);
                Text.Center(px, 40, 6, o.Id, Theme.Heading, Theme.Card);
                Helper.Screen.Fill(px + 8, 66, px + 31, 67);     // the "processing" marker
            }
            if (!o.Alive)
                Deck.Paint(x, 5, 5, 3, Theme.Background, Theme.Background);
            else
                Deck.Paint(x, 5, 5, 1, o.Marked ? Theme.Accent2 : (o.Root ? Theme.Accent : Theme.Muted), Theme.Card);
            Deck.Paint(x, 8, 5, 1, o.Processing ? Theme.Accent2 : Theme.Background, Theme.Background);
            x += 6;
        }
        drawn = true;

        stack.Draw();

        if (!heapClean)
        {
            heapClean = true;
            Deck.Paint(Theme.Left, 17, Theme.Width, 4, Theme.Background, Theme.Background);
            Helper.Screen.ClearRect(Theme.Left, 17, Theme.Width, 4);
            uint hx = Theme.Left;
            foreach (var o in objects)
            {
                if (!o.Alive)
                    continue;
                string refs = "";
                foreach (var r in o.References)
                    {
                    if (refs != "")
                        refs = refs + " ";
                    refs = refs + r.Id;
                }
                new Box { X = hx, Y = 17, Width = o.Size + 1, Height = 4, Text = o.Id, Caption = refs, Color = Theme.Accent2, ConnectToLeft = hx != Theme.Left }.Draw();
                hx += o.Size + 1;
            }
            if (hx < Theme.Right)
                new Box { X = hx, Y = 17, Width = Theme.Right - hx, Height = 4, Text = "free", Color = Theme.Card, Hollow = true }.Draw();
        }
    }
}
