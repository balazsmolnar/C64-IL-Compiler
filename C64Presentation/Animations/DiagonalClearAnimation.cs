using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.Animations;

class DiagonalClearAnimation : Animation
{
    public override void Animate()
    {
        for (int x = 0; x < Screen.Width + Screen.Height; x++)
        {
            for (int xx = x; xx >= 0; xx--)
            {
                if (xx > Screen.Width || (x - xx) > Screen.Height)
                    continue;
                C64.SetChar((uint)xx, (uint)(x-xx), 0x20);
            }
            Delay.Wait(50);
        }
    }
}