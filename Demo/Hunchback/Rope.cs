using C64Lib;
namespace Hunchback
{
    class Rope
    {

        private int frameCounter_;

        private Sprite sprite1_;
        private Sprite sprite2_;

        public Sprite Sprite1
        {
            set
            {
                sprite1_ = value;
            }
        }

        public Sprite Sprite2
        {
            set
            {
                sprite2_ = value;
            }
        }

        public void Init()
        {
            sprite1_.Visible = true;
            sprite2_.Visible = true;
            sprite1_.Color = Colors.White;
            sprite2_.Color = Colors.White;
            sprite1_.ExpandX = sprite1_.ExpandY = true;
            sprite2_.ExpandX = sprite2_.ExpandY = true;
            sprite1_.X = 0xB8;
            sprite2_.X = 0xC0;
            sprite1_.Y = 66;
            sprite2_.Y = 108;
            sprite1_.DataBlock = C64Address.FromLabel("spt_rope_14");
            sprite2_.DataBlock = C64Address.FromLabel("spt_rope_15");
            frameCounter_ = 0;
        }

        public void Move()
        {
            SetFrame();
        }

        private void SetFrame()
        {
            int frame = frameCounter_ > 15 ? 31 - frameCounter_ : frameCounter_;

            switch (frame)
            {
                case 0:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_14");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_15");
                    break;
                case 1:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_14");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_15");
                    break;

            }

            // if (frameCounter_ == 0)
            //     sprite_.DataBlock = C64Address.FromLabel("spt_fireball_0");
            // else if (frameCounter_ == 1)
            //     sprite_.DataBlock = C64Address.FromLabel("spt_fireball_1");
            // else if (frameCounter_ == 2)
            //     sprite_.DataBlock = C64Address.FromLabel("spt_fireball_2");
            // else if (frameCounter_ == 3)
            //     sprite_.DataBlock = C64Address.FromLabel("spt_fireball_3");
        }
    }
}