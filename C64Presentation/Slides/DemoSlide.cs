using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.Slides;

// "Live demo": a section slide with a blinking red LIVE marker at the top
// right, up until the presenter comes back from the demo and presses SPACE.
class DemoSlide : Slide
{
    const uint MarkerColumn = 32;
    const uint MarkerRow = 5;
    const uint BlinkFrames = 25;

    public string What { get; init; }

    public override void Present()
    {
        Master.Section("Live", "Demo time", What ?? "Over to VICE", 0);
        ulong x = Screen.X(MarkerColumn);
        Text.Draw(x + 1, MarkerRow, Theme.Dot, Theme.Bad);
        Text.Draw(x + 10, MarkerRow, "LIVE", Theme.Heading);
        Deck.Reveal();

        uint frames = 0;
        bool lit = true;
        while (!KeyBoard.AdvancePressed())
        {
            C64.Screen.WaitForVBlank();
            if (++frames < BlinkFrames)
                continue;
            frames = 0;
            lit = !lit;
            Deck.Paint(MarkerColumn, MarkerRow, 1, 1, lit ? Theme.Bad : Theme.Background, Theme.Background);
        }
    }
}
