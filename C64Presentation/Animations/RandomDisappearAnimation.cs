using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.Animations;

class RandomDisappearAnimation : Animation
{
    public override void Animate()
    {
        ulong primeNumber = 773;
        ulong tmp = 0;

        for (ulong i = 0; i < 1000; i++)
        {
            tmp += primeNumber;
            if (tmp > 1000)
                tmp -= 1000;
            C64.FillMemory(C64Address.FromLabel("(screenMemory)")+tmp, 32, 1);
        }
    }
}