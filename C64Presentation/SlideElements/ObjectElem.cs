namespace C64Presentation.SlideElements;

// One object of the GC slide's little heap. (No property initializers: the
// compiler runs none, so every flag starts false: Dead, not Alive.)
sealed class HeapObject
{
    public required string Id { get; init; }
    public uint Size { get; init; }            // heap cells
    public bool Root { get; init; }
    public HeapObject[]? References { get; set; }
    public bool Marked { get; set; }
    public bool Dead { get; set; }
}
