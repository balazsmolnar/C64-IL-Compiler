using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.Slides;

// "Live demo": a section-style band with a blinking LIVE marker, up until
// the presenter comes back from the demo and presses SPACE.
class DemoSlide : Slide
{
    public string What;           // default "Over to VICE"

    public override void Present()
    {
        Master.Section("Live demo", "Demo time", What ?? "Over to VICE");
        Text.Draw(272, 9, "LIVE", Theme.Heading, Theme.Card);
        Text.Draw(258, 9, "\\", Theme.Bad, Theme.Card);
        Deck.Reveal();

        uint frames = 0;
        while (!KeyBoard.AdvancePressed())
        {
            C64.Screen.WaitForVBlank();
            frames++;
            if (frames == 25)
                Deck.Paint(32, 9, 1, 1, Theme.Card, Theme.Card);
            if (frames == 50)
            {
                Deck.Paint(32, 9, 1, 1, Theme.Bad, Theme.Card);
                frames = 0;
            }
        }
    }
}
