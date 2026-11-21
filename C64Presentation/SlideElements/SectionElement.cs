using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A section divider (Master.Section): the section's 6x6 icon, the title in
// the big face, a subtitle, all on one left edge.
//
//     new SectionElement { Title = "Small and fast",
//                          Subtitle = "Pay only for what you use",
//                          Icon = C64Address.FromLabel("img_SecChart") }
class SectionElement : SlideElement
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public ulong Icon { get; init; }

    public override void Draw() => Master.Section(Title, Subtitle, Icon);
}
