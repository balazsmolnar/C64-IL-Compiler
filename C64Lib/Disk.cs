namespace C64Lib;

public static partial class C64
{
    /// <summary>
    /// Files on a floppy disk, read through the KERNAL (asm/C64Disk.asm).
    /// Nested for the same reason as <see cref="Screen"/>: every call
    /// compiles to a plain static call. A program that never calls it pays
    /// nothing.
    /// </summary>
    /// <remarks>
    /// Put the files on the program's own .d64: list them in a
    /// <c>disk.txt</c> next to the project's run.bat and the build writes
    /// the program and every listed file onto <c>prg\&lt;name&gt;.d64</c>
    /// (tools/make-d64.ps1; see the top of that script for the format).
    /// </remarks>
    public static class Disk
    {
        /// <summary>
        /// Loads a file into memory at <paramref name="address"/>, whatever
        /// load address the file itself names, from the drive the program
        /// was started from (device 8 if it was not loaded from a drive).
        /// </summary>
        /// <param name="fileName">
        /// The file's name on the disk, at most 16 characters; upper and
        /// lower case are the same. <c>*</c> and <c>?</c> wildcards work.
        /// </param>
        /// <param name="address">
        /// Where the file's bytes go (after its 2-byte load address, which is
        /// skipped): RAM the program owns and large enough for the whole
        /// file, e.g. a buffer reserved with
        /// <c>[assembly: RawAssembly(Asm = "buf_Picture .fill 4096")]</c> and
        /// <c>C64Address.FromLabel("buf_Picture")</c>. Nothing checks the
        /// length: a larger file overwrites whatever follows.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the file was loaded; <see langword="false"/>
        /// if it was not found, no drive answered, the disk could not be read
        /// or RUN/STOP was pressed.
        /// </returns>
        /// <remarks>
        /// Takes the drive's time: on a real 1541 about 400 bytes a second
        /// (a 4 KB picture is some 10 seconds); with VICE's virtual drive it
        /// is all but instant. Interrupts keep running, but the KERNAL holds
        /// them off while it waits for each byte, so a raster split
        /// (<see cref="Screen.SetSplit"/>) flickers and music stutters during
        /// a load: load while a screen without a split is showing. The
        /// picture on the screen stays as it is.
        /// </remarks>
        public static bool Load(string fileName, ulong address) => false;
    }
}
