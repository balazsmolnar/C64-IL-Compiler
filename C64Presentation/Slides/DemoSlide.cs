using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.Slides;

// A live demo: a section slide, and a blinking red LIVE badge in the
// footer, up until the presenter comes back from the demo and presses SPACE.
sealed class DemoSlide : Slide
{
    const uint BadgeColumn = 27;
    const uint BlinkFrames = 25;

    public string? What { get; init; }

    public override void Present()
    {
        Master.Section("", "Demo time", What ?? "Over to VICE", 0);
        ulong x = Screen.Px(BadgeColumn);
        Text.Draw(x + 1, Theme.FooterRow, Theme.Dot, Theme.Bad);
        Text.Draw(x + 10, Theme.FooterRow, "LIVE", Theme.Heading);
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
            Deck.Paint(BadgeColumn, Theme.FooterRow, 1, 1, lit ? Theme.Bad : Theme.Background, Theme.Background);
        }
    }
}
