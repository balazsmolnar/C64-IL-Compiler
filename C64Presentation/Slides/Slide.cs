namespace C64Presentation
{
    abstract class Slide
    {
        public abstract void Present();

        public virtual void CleanUp()
        {
        }
    }
}