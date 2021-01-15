using C64Lib;

class Player
{

    private uint x_;
    private uint y_;

    private int data_;
    private bool jump_;
    private int jumpCounter_;
    public uint X
    {
        get { return x_; }
        set
        {
            x_ = value;
            sprite_.X = value;
        }
    }
    public uint Y
    {
        get { return y_; }
        set
        {
            y_ = value;
            sprite_.Y = value;
        }
    }
    private Sprite sprite_;

    public Sprite Sprite
    {
        set
        {
            C64.Sprites.CommonColor1 = Colors.LightRed;
            C64.Sprites.CommonColor2 = Colors.Blue;

            sprite_ = value;
            sprite_.DataBlock = 3;
            data_ = 4;
            sprite_.MultiColor = true;
            sprite_.Visible = true;
            sprite_.Color = Colors.Brown;
            jump_ = false;
        }
    }

    public void Move(uint distanceToPlatform)
    {
        C64.SetBorderColor(sprite_.IsInCollision ? Colors.Red : Colors.Black);
        var freefall = false;
        if (!jump_ && distanceToPlatform > 4)
            freefall = true;

        if (!jump_ && !freefall && C64.IsKeyPressed(Keys.W))
        {
            jump_ = true;
            jumpCounter_ = 0;
        }
        if (C64.IsKeyPressed(Keys.A))
            X--;
        if (C64.IsKeyPressed(Keys.D))
        {
            X++;
            data_++;
            if (data_ == 6)
                data_ = 2;
            if (jump_)
            {
                X++;
                data_ = 3;
            }
            if (freefall)
            {
                X++;
                data_ = 3;
            }
            sprite_.DataBlock = data_;
        }

        if (jump_)
        {
            jumpCounter_++;

            Y -= jumpCounter_ < 10 ? 3 : 1;
            if (jumpCounter_ == 16)
            {
                jump_ = false;
            }
        }

        if (freefall)
        {
            Y += 3;
        }
    }
}