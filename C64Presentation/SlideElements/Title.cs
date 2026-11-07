using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

// A content slide's master: title, accent rule and footer (Master.Content).
class Title : SlideElement
{
    public string Text { get; init; }

    public override void Draw() => Master.Content(Text);
}
