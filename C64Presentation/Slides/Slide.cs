namespace C64Presentation.Slides;

// A slide draws itself in Present (the screen is blanked and faded in
// afterwards, see Helper/Deck); builds call Deck.Step() between parts.
// CleanUp runs before the slide fades out (hide sprites, stop sound).
abstract class Slide
{
    public abstract void Present();

    public virtual void CleanUp() { }
}
