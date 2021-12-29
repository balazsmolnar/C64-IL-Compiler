using C64Lib;

namespace Hunchback
{
    class TitleScreen
    {
        public class Tone
        {
            public ulong Frequency;
            public uint Sustain;
        }
        public static void Display()
        {
            Screen.Clear(Colors.White);
            C64.CopyMemory(C64Address.FromLabel("(screenMemory+$CD)"), C64Address.FromLabel("titleScreen"), 0);
            C64.CopyMemory(C64Address.FromLabel("(screenMemory+$1CD)"), C64Address.FromLabel("titleScreen+$100"), 0);
            C64.CopyMemory(C64Address.FromLabel("(screenMemory+$21B)"), C64Address.FromLabel("titleScreen+$14E"), 0);

            C64.Write(13, 1, "OCEAN SOFTWARE", Colors.Green);
            C64.Write(13, 3, "   PRESENTS   ", Colors.White);
            C64.Write(25, 21, "   BY J.STEELE", Colors.Green);
            C64.Write(7, 23, "PRESS F1 FOR INSTRUCTIONS", Colors.White);
            C64.Write(7, 24, "    OR SPACE TO START    ", Colors.White);

            var tones = GetTones();

            for (; ; )
            {
                var color = (uint)1;
                foreach (var t in tones)
                {
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
}