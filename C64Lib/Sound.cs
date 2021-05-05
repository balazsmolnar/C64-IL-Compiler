namespace C64Lib
{
    public enum WaveForm : uint
    {
        Triangle = 16,
        Saw = 32,
        Rectangle = 64,
        Noise = 128
    }
    public class Sound
    {
        public uint Volume { get; set; }

        public void PlayEffectReg1(WaveForm waveForm, ulong freq, ulong pulseWidth, uint attackDecay, uint sustain, bool pulse) { }
        public void PlayEffectReg2(WaveForm waveForm, ulong freq, ulong pulseWidth, uint attackDecay, uint sustain, bool pulse) { }
    }
}