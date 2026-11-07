using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A section divider (Master.Section): the part number in big cyan figures,
// the title, a subtitle, and the section's 6x6 icon on the right.
//
//     new SectionElement { Number = "02", Title = "The hard parts",
//                          Subtitle = "Six things a 6502 does not have",
//                          Icon = C64Address.FromLabel("img_SecChip") }
class SectionElement : SlideElement
{
    public string? Number { get; init; }
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public ulong Icon { get; init; }

    public override void Draw() => Master.Section(Number ?? "", Title, Subtitle, Icon);
}
