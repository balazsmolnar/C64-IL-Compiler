using System;
using C64Lib;
using C64Presentation.Helper;
using C64Presentation.SlideElements;

namespace C64Presentation.Slides
{
    class GarbageCollectorSlide : Slide
    {
        private ObjectElem[] objects;
        private Stack stack;
        public override void Present()
        {
            objects = new []
            {
                new ObjectElem() {Id = "1", Alive = true, Size = 3, Root = true },
                new ObjectElem() {Id = "2", Alive = true, Size = 3 },
                new ObjectElem() {Id = "3", Alive = true, Size = 7, Root = true },
                new ObjectElem() {Id = "4", Alive = true, Size = 3 },
                new ObjectElem() {Id = "5", Alive = true, Size = 3 },
                new ObjectElem() {Id = "6", Alive = true, Size = 7 },
            };

            objects[0].References = new ObjectElem[] {objects[2], objects[3]};
            objects[1].References = new ObjectElem[] { objects[0], objects[5] };
            objects[2].References = new ObjectElem[] { objects[0], objects[3], objects[4] };
            objects[3].References = new ObjectElem[] { objects[0] };
            objects[4].References = new ObjectElem[] { objects[2] };
            objects[5].References = new ObjectElem[] { objects[0], objects[1], objects[4] };

            stack = new Stack() { X= 17, Y= 12 };
            stack.Init(objects.Length);
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
                    Screen.ClearRect(0, 39, 18 ,23);

                    RefreshWait();
                }
            }
        }

        private void RefreshWait()
        {
            Draw();
            KeyBoard.WaitForKeys();
        }

        private void Draw()
        {
            uint x = 0;
            uint x2 = 0;
            const uint width = 7;

            foreach (var objectElem in objects)
            {
                
                var color = objectElem.Root ? Colors.Cyan : Colors.Blue;
                if (objectElem.Marked)
                    color = Colors.Orange;

                if (!objectElem.Alive)
                    color = Colors.Black;

                var box = new Box() 
                { X = x, Y = 5, Width = width, Height = 3, Text = objectElem.Id, BorderColor = Colors.Blue, TextColor = color, ConnectToLeft = x != 0 };
                box.Draw();

                C64.SetChar(x+3, 8, objectElem.Processing ? 0u : 0x20u);
                x += width - 1;

                if (!objectElem.Alive)
                    continue;

                var width2 = objectElem.Size + 3;
                var box2 = new Box()
                    { X = x2, Y = 18, Width = width2, Height = 6, Text = objectElem.Id, BorderColor = Colors.Blue, TextColor = objectElem.Alive ? Colors.Yellow : Colors.Black, ConnectToLeft = x2 != 0 };
                box2.Draw();
                var refx = x2 + 1;
                if (objectElem.Alive)
                {
                    foreach (var reference in objectElem.References)
                    {
                        C64.Write(refx, 22, reference.Id);
                        refx += 2;
                    }
                }

                x2 += width2-1;

            }

            stack.Draw();
        }
    }
}
