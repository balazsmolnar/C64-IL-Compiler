using C64Lib;
using System;

namespace Hunchback;

class TitleScreen
{
    public class Tone
    {
        public ulong Frequency;
        public uint Sustain;
    }
    public static void Display()
    {
        // Fetched once, here, and reused by this method's own tune loop
        // below. GetTones() allocates a fresh ~64-object array (1 array +
        // one Tone instance per note) every time it's called.
        var tones = GetTones();

        // The intro scroll (IntroScroll.Play/IntroPlayer) is parked --
        // too many unresolved issues (level ordering, music pacing, a
        // live-VICE crash after the title screen whose root cause was
        // never pinned down) to justify keeping it in the loop while
        // iterating on other things. Code is left in place, just unused.
        // IntroScroll.Play(tones);

        Screen.Clear(Colors.White);
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+$CD)"), C64Address.FromLabel("titleScreen"), 0);
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+$1CD)"), C64Address.FromLabel("titleScreen+$100"), 0);
        C64.CopyMemory(C64Address.FromLabel("(screenMemory+$21B)"), C64Address.FromLabel("titleScreen+$14E"), 0);

        C64.Write(13, 1, "OCEAN SOFTWARE", Colors.Green);
        C64.Write(13, 3, "   PRESENTS   ", Colors.White);
        C64.Write(25, 21, "   BY J.STEELE", Colors.Green);
        C64.Write(7, 23, "PRESS F1 FOR INSTRUCTIONS", Colors.White);
        C64.Write(7, 24, "    OR SPACE TO START    ", Colors.White);

        // Indexed for, not foreach -- foreach over an array of reference-
        // typed elements (Tone here) is untested territory in this
        // compiler (grep confirms nothing in Test/ uses foreach at all);
        // this specific loop runs forever (for(;;), never a normal method
        // exit) and its per-iteration loop variable is exactly the shape
        // that turned out to leak in a live VICE test (heap slots climbing
        // rapidly, ~1 per iteration, crashing back to BASIC's READY.
        // prompt) once this loop actually started running -- switching to
        // direct tones[i] indexing avoids needing a per-iteration
        // reference-typed loop variable at all.
        // tones.Length cached once as a uint: comparing "i < tones.Length"
        // directly (uint against Array.Length's real int) compiles to a
        // signed 16-bit branch (Blt) that isn't mapped in this compiler's
        // branch.asm (only the unsigned variant is) -- crashes
        // Compiler.exe outright. uint-vs-uint sidesteps it.
        // GC.Collect() once per full pass through the tune, not because a
        // specific leak in this loop was ever pinned down -- SimpleEmulator
        // can't usefully test this loop at all (its IsKeyPressed reads
        // uninitialized memory as "pressed", so the loop exits into real
        // gameplay within its first pass every time), so unlike everything
        // else fixed this session, this one couldn't be verified against a
        // known root cause. It's a real, reproducible crash in live VICE
        // though (resets to BASIC's READY. prompt after sitting at the
        // title screen for a while, confirmed with zero player input), and
        // this loop runs forever, so a periodic collect here is cheap
        // insurance regardless of what turns out to be causing it.
        uint toneCount = (uint)tones.Length;
        for (; ; )
        {
            GC.Collect();
            var color = (uint)1;
            for (uint i = 0; i < toneCount; i++)
            {
                var t = tones[i];
                color++;
                if (color == 7)
                    color = 1;
                C64.Sound.PlayEffectReg1(WaveForm.Saw, t.Frequency, 0, 9, 0, false);
                C64.FillMemory(C64Address.FromLabel("(colorMemory+$CD)"), color, 0);
                C64.FillMemory(C64Address.FromLabel("(colorMemory+$1CD)"), color, 0);
                C64.FillMemory(C64Address.FromLabel("(colorMemory+$21B)"), color, 0);

                for (uint k = 0; k < 50u; k++)
                {
                    for (uint kk = 0; kk < t.Sustain; kk++)
                    {
                        if (C64.IsKeyPressed(Keys.Space) || (C64.Joysticks.Joystick2.Pressed & JoystickButtons.Fire) == JoystickButtons.Fire)
                            return;
                    }
                }
            }
        }
    }
    private static Tone[] GetTones()
    {
        const ulong TONE_0 = 0x0000UL;
        const ulong TONE_1 = 0x0CD8UL;
        const ulong TONE_2 = 0x0D9CUL;
        const ulong TONE_3 = 0x0F46UL;
        const ulong TONE_4 = 0x1125UL;
        const ulong TONE_5 = 0x133FUL;
        const ulong TONE_6 = 0x1464UL;
        const ulong TONE_7 = 0x16E3UL;
        const ulong TONE_8 = 0x19B1UL;
        const ulong TONE_9 = 0x1E8DUL;
        const ulong TONE_10 = 0x224BUL;
        const uint DELAY_0 = 10;
        const uint DELAY_1 = 20;
        const uint DELAY_2 = 30;
        const uint DELAY_3 = 40;
        const uint DELAY_4 = 50;

        return new Tone[] {
            new Tone { Frequency = TONE_1, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_4, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_5, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_5, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_4, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_5, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_4, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_5, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_5, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_4, Sustain = DELAY_2  },
            new Tone { Frequency = TONE_0, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_3, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_8, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_8, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_8, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_8, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_8, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_8, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_2  },
            new Tone { Frequency = TONE_0, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_9, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_10, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_9, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_10, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_9, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_8, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_9, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_8, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_3, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_4, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_4, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_3, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_4, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_3, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_2, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_3, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_4, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_3, Sustain = DELAY_1  },
            new Tone { Frequency = TONE_8, Sustain = DELAY_0  },
            new Tone { Frequency = TONE_7, Sustain = DELAY_3  },
            new Tone { Frequency = TONE_6, Sustain = DELAY_4  },
        };
    }
}