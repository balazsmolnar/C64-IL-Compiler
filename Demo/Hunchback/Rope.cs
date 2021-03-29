using C64Lib;
namespace Hunchback
{
    class Rope
    {

        private uint frameCounter_;
        private uint counter_;

        private Sprite sprite1_;
        private Sprite sprite2_;

        public uint PlayerX;

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
            counter_++;
            if (counter_ < 2)
                return;
            counter_ = 0;

            if (frameCounter_ == 0 || frameCounter_ == 16)
                C64.Sound.PlayEffectReg2(WaveForm.Noise, 100, 15, 0, 0, 176, 0, false);
            frameCounter_++;
            if (frameCounter_ == 32)
                frameCounter_ = 0;
            SetFrame();
        }

        private void SetFrame()
        {
            // tbl_RopeUpperFrame      
            //                 byte $28, $26, $24, $22, $20, $1E, $1C, $1A
            //                 byte $2A, $2C, $2E, $30, $32, $34, $36, $38

            // tbl_RopeLowerFrame
            //                 byte $29, $27, $25, $23, $21, $1F, $1D, $1B
            //                 byte $2B, $2D, $2F, $31, $33, $35, $37, $39

            // tbl_RopeUpperX          
            //                         byte $B8, $B8, $B8, $B8, $B8, $B8, $B8, $B8
            //                         byte $8A, $8A, $8A, $8A, $8A, $8A, $8A, $8A

            // tbl_RopeLowerX          
            //                         byte $C0, $BA, $B8, $B8, $B8, $B8, $B8, $B8
            //                         byte $8A, $8A, $8A, $8A, $8A, $8A, $88, $82

            // tbl_QuasiRopeXPosition  
            //             byte $C6, $C2, $BA, $B2, $AC, $A6, $9E, $98
            //             byte $94, $8A, $82, $7C, $76, $6E, $66, $62


            uint frame = frameCounter_ >= 16 ? 31 - frameCounter_ : frameCounter_;

            switch (frame)
            {
                case 0:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_14");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_15");
                    sprite1_.X = 0xB8;
                    sprite2_.X = 0xC0;
                    PlayerX = 0xC6;
                    break;
                case 1:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_12");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_13");
                    sprite1_.X = 0xB8;
                    sprite2_.X = 0xBA;
                    PlayerX = 0xC2;
                    break;
                case 2:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_10");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_11");
                    sprite1_.X = 0xB8;
                    sprite2_.X = 0xB8;
                    PlayerX = 0xBA;
                    break;
                case 3:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_8");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_9");
                    sprite1_.X = 0xB8;
                    sprite2_.X = 0xB8;
                    PlayerX = 0xB2;
                    break;
                case 4:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_6");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_7");
                    sprite1_.X = 0xB8;
                    sprite2_.X = 0xB8;
                    PlayerX = 0xAC;
                    break;
                case 5:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_4");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_5");
                    sprite1_.X = 0xB8;
                    sprite2_.X = 0xB8;
                    PlayerX = 0xA6;
                    break;
                case 6:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_2");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_3");
                    sprite1_.X = 0xB8;
                    sprite2_.X = 0xB8;
                    PlayerX = 0x9E;
                    break;
                case 7:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_0");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_1");
                    sprite1_.X = 0xB8;
                    sprite2_.X = 0xB8;
                    PlayerX = 0x98;
                    break;
                case 8:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_16");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_17");
                    sprite1_.X = 0x8A;
                    sprite2_.X = 0x8A;
                    PlayerX = 0x94;
                    break;
                case 9:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_18");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_19");
                    sprite1_.X = 0x8A;
                    sprite2_.X = 0x8A;
                    PlayerX = 0x8A;
                    break;
                case 10:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_20");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_21");
                    sprite1_.X = 0x8A;
                    sprite2_.X = 0x8A;
                    PlayerX = 0x82;
                    break;
                case 11:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_22");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_23");
                    sprite1_.X = 0x8A;
                    sprite2_.X = 0x8A;
                    PlayerX = 0x7C;
                    break;
                case 12:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_24");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_25");
                    sprite1_.X = 0x8A;
                    sprite2_.X = 0x8A;
                    PlayerX = 0x76;
                    break;
                case 13:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_26");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_27");
                    sprite1_.X = 0x8A;
                    sprite2_.X = 0x8A;
                    PlayerX = 0x6E;
                    break;
                case 14:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_28");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_29");
                    sprite1_.X = 0x8A;
                    sprite2_.X = 0x88;
                    PlayerX = 0x66;
                    break;
                case 15:
                    sprite1_.DataBlock = C64Address.FromLabel("spt_rope_30");
                    sprite2_.DataBlock = C64Address.FromLabel("spt_rope_31");
                    sprite1_.X = 0x8A;
                    sprite2_.X = 0x82;
                    PlayerX = 0x62;
                    break;
            }
            PlayerX += 20;
        }
    }
}