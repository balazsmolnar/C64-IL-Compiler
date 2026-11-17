using C64Lib;

namespace C64Presentation;

/// <summary>The pictures that live on the deck's disk (disk.txt), not in the program.</summary>
public enum DiskPicture : uint
{
    None,
    ShotRoom,
    ShotTitle,
    ShotBoss,
    Room,       // the guardian's room without its characters (art/slides/make_room.py)
    Title,      // the cover (Master.Cover)
}

/// <summary>
/// Loads a picture from the disk into the one shared buffer and returns its
/// address, ready for <c>C64.Screen.DrawImage</c>:
/// <code>C64.Screen.DrawImage(0, 0, ArtDisk.Load(DiskPicture.ShotRoom));</code>
/// Call it before clearing the old slide: the old slide stays on the
/// screen while the drive works. A picture already in the buffer is not
/// loaded again, and one that cannot be loaded draws as nothing.
/// </summary>
static class ArtDisk
{
    // The first byte of a picture says which of its parts follow: none.
    const uint EmptyPicture = 0;

    static readonly ulong buffer = C64Address.FromLabel("buf_Picture");
    static DiskPicture _inBuffer = DiskPicture.None;

    public static ulong Load(DiskPicture picture)
    {
        if (picture != _inBuffer)
        {
            _inBuffer = C64.Disk.Load(FileName(picture), buffer) ? picture : DiskPicture.None;
            if (_inBuffer == DiskPicture.None)
                C64.FillMemory(buffer, EmptyPicture, 1);
        }
        return buffer;
    }

    static string FileName(DiskPicture picture) => picture switch
    {
        DiskPicture.ShotRoom => "shotroom",
        DiskPicture.ShotTitle => "shottitle",
        DiskPicture.Room => "room",
        DiskPicture.Title => "title",
        _ => "shotboss",
    };
}
