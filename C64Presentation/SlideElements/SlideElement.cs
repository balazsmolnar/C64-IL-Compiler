namespace C64Presentation.SlideElements;

// Something a SimpleSlide draws, configured with an object initializer:
//
//     new BulletList { Items = ["One", "Two"], Wait = true }
//
// Properties left unset take the theme's value: 0 (Theme.Auto, or
// Colors.Black for a colour) means "the default", read through
// Theme.OrCells(value, fallback) / Theme.Or(colour, fallback) in Draw. That
// is a convention rather than real defaults: there is no Nullable<T>, and
// an initializer or constructor per element type costs code in every
// program (a two-field class measured 189 B more than an object
// initializer), while the elements are pure data written as initializers.
abstract class SlideElement
{
    public abstract void Draw();
}
