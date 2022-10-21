using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C64Lib;

namespace C64Presentation.Helper;

class Sprites
{
    public static void SetupLogo(bool expand)
    {
        var sprite = C64.Sprites.Sprite0;
        sprite.Visible = true;
        sprite.ExpandX = expand;
        sprite.ExpandY = expand;

        sprite.DataBlock = C64Address.FromLabel("spt_c_upper");
        sprite.Color = Colors.Blue;

        sprite = C64.Sprites.Sprite1;
        sprite.Visible = true;
        sprite.ExpandX = expand;
        sprite.ExpandY = expand;

        sprite.DataBlock = C64Address.FromLabel("spt_c_lower");
        sprite.Color = Colors.Blue;

        sprite = C64.Sprites.Sprite2;
        sprite.Visible = true;
        sprite.ExpandX = expand;
        sprite.ExpandY = expand;

        sprite.DataBlock = C64Address.FromLabel("spt_logo_upper");
        sprite.Color = Colors.Blue;

        sprite = C64.Sprites.Sprite3;
        sprite.Visible = true;
        sprite.ExpandX = expand;
        sprite.ExpandY = expand;

        sprite.DataBlock = C64Address.FromLabel("spt_logo_lower");
        sprite.Color = Colors.Red;

        sprite = C64.Sprites.Sprite4;
        sprite.Visible = true;
        sprite.ExpandX = expand;
        sprite.ExpandY = expand;

        sprite.DataBlock = C64Address.FromLabel("spt_c_upper");
        sprite.Color = Colors.Orange;

        sprite = C64.Sprites.Sprite5;
        sprite.Visible = true;
        sprite.ExpandX = expand;
        sprite.ExpandY = expand;

        sprite.DataBlock = C64Address.FromLabel("spt_c_lower");
        sprite.Color = Colors.Orange;

        sprite = C64.Sprites.Sprite6;
        sprite.Visible = true;
        sprite.ExpandX = expand;
        sprite.ExpandY = expand;

        sprite.DataBlock = C64Address.FromLabel("spt_sharp");
        sprite.Color = Colors.Orange;
    }

    public static void SetVisible(bool visible)
    {
        C64.Sprites.Sprite0.Visible = visible;
        C64.Sprites.Sprite1.Visible = visible;
        C64.Sprites.Sprite2.Visible = visible;
        C64.Sprites.Sprite3.Visible = visible;
        C64.Sprites.Sprite4.Visible = visible;
        C64.Sprites.Sprite5.Visible = visible;
        C64.Sprites.Sprite6.Visible = visible;
    }
}