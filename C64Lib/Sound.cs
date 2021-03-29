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

        public void PlayEffectReg1(WaveForm waveForm, uint lowFreq, uint highFreq, uint pulseWidthLow, uint pulseWidthHigh, uint attackDecay, uint sustain, bool pulse) { }
        public void PlayEffectReg2(WaveForm waveForm, uint lowFreq, uint highFreq, uint pulseWidthLow, uint pulseWidthHigh, uint attackDecay, uint sustain, bool pulse) { }
    }
}