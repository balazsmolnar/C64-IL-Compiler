namespace C64Presentation.SlideElements;

// Something a SimpleSlide draws, configured with an object initializer:
//
//     new BulletList { Items = ["One", "Two"], Wait = true }
//
// Properties left unset take the theme's value (Theme.Auto: 0, or
// Colors.Black for a colour). That is a convention rather than real
// defaults because this compiler runs no field initializers or constructor
// bodies and has no Nullable<T>; see Theme.Auto.
abstract class SlideElement
{
    public abstract void Draw();
}
