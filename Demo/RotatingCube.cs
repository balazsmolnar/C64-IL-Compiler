using System;
using C64Lib;

namespace Demo;

// Continuously rotating wireframe cube: 8-vertex/12-edge projection.
// cosAy_/sinAy_ advance every frame via the angle-addition formulas
// (cos(a+d)=cos(a)cos(d)-sin(a)sin(d), sin(a+d)=sin(a)cos(d)+cos(a)sin(d)) --
// cheaper than calling MathF.Cos/Sin every frame.
//
// Initialised through Init(), not a constructor body: this compiler never
// runs a constructor body for an ordinary class (OpNewObj hardcodes no ctor
// call) and now rejects one at compile time -- see Compiler/Operands/
// OperandBase.cs.
//
// No double buffering exists in this graphics library, so each frame erases
// the previous one and draws the new one -- but UpdateEdges interleaves
// erase-this-edge/draw-its-replacement one edge at a time, not "erase all
// 12, then draw all 12": the latter makes the whole cube vanish for a
// moment every frame (visible as blinking).
//
// CubeSize/CamDist/CenterX/Y keep every projected coordinate inside 0-255:
// this compiler's float->int conversion is 8-bit, so a value reaching 256+
// would silently wrap.
class RotatingCube
{
    float cosAy_, sinAy_, cosAx_, sinAx_;
    float cosStep_, sinStep_;
    uint lastVx_, lastVy_;

    uint pvx0_, pvy0_, pvx1_, pvy1_, pvx2_, pvy2_, pvx3_, pvy3_;
    uint pvx4_, pvy4_, pvx5_, pvy5_, pvx6_, pvy6_, pvx7_, pvy7_;
    bool haveDrawnFrame_;

    // The cube's screen position moves every frame and bounces off the
    // edges. Bounds keep every projected vertex inside 0-255 (8-bit
    // float->int, see class comment) and inside the 200-line screen: the
    // cube extends ~46 pixels from its center once rotation and
    // perspective are included.
    float centerX_, centerY_;
    float velX_, velY_;

    const float CubeSize = 25f;      // half-edge length
    const float CubeCamDist = 150f;  // pseudo-perspective "camera" distance
    // Margins leave room for one frame's overshoot (velocity is applied
    // before the bounds check) on top of the ~46 pixel extent.
    const float MinCenterX = 54f;
    const float MaxCenterX = 200f;
    const float MinCenterY = 54f;
    const float MaxCenterY = 145f;

    public void Init()
    {
        cosAx_ = MathF.Cos(0.4f);
        sinAx_ = MathF.Sin(0.4f);
        cosAy_ = 1f;
        sinAy_ = 0f;
        cosStep_ = MathF.Cos(0.15f);
        sinStep_ = MathF.Sin(0.15f);
        haveDrawnFrame_ = false;
        centerX_ = 128f;
        centerY_ = 100f;
        velX_ = 4f;
        velY_ = 3f;
    }

    public void Step()
    {
        ComputeVertexProjection(-CubeSize, -CubeSize, -CubeSize); uint vx0 = lastVx_; uint vy0 = lastVy_;
        ComputeVertexProjection(CubeSize, -CubeSize, -CubeSize); uint vx1 = lastVx_; uint vy1 = lastVy_;
        ComputeVertexProjection(CubeSize, CubeSize, -CubeSize); uint vx2 = lastVx_; uint vy2 = lastVy_;
        ComputeVertexProjection(-CubeSize, CubeSize, -CubeSize); uint vx3 = lastVx_; uint vy3 = lastVy_;
        ComputeVertexProjection(-CubeSize, -CubeSize, CubeSize); uint vx4 = lastVx_; uint vy4 = lastVy_;
        ComputeVertexProjection(CubeSize, -CubeSize, CubeSize); uint vx5 = lastVx_; uint vy5 = lastVy_;
        ComputeVertexProjection(CubeSize, CubeSize, CubeSize); uint vx6 = lastVx_; uint vy6 = lastVy_;
        ComputeVertexProjection(-CubeSize, CubeSize, CubeSize); uint vx7 = lastVx_; uint vy7 = lastVy_;

        if (haveDrawnFrame_)
        {
            UpdateEdges(pvx0_, pvy0_, pvx1_, pvy1_, pvx2_, pvy2_, pvx3_, pvy3_,
                        pvx4_, pvy4_, pvx5_, pvy5_, pvx6_, pvy6_, pvx7_, pvy7_,
                        vx0, vy0, vx1, vy1, vx2, vy2, vx3, vy3,
                        vx4, vy4, vx5, vy5, vx6, vy6, vx7, vy7);
        }
        else
        {
            DrawEdges(vx0, vy0, vx1, vy1, vx2, vy2, vx3, vy3,
                      vx4, vy4, vx5, vy5, vx6, vy6, vx7, vy7, true);
            haveDrawnFrame_ = true;
        }

        pvx0_ = vx0; pvy0_ = vy0; pvx1_ = vx1; pvy1_ = vy1;
        pvx2_ = vx2; pvy2_ = vy2; pvx3_ = vx3; pvy3_ = vy3;
        pvx4_ = vx4; pvy4_ = vy4; pvx5_ = vx5; pvy5_ = vy5;
        pvx6_ = vx6; pvy6_ = vy6; pvx7_ = vx7; pvy7_ = vy7;

        float newCosAy = cosAy_ * cosStep_ - sinAy_ * sinStep_;
        float newSinAy = sinAy_ * cosStep_ + cosAy_ * sinStep_;
        cosAy_ = newCosAy;
        sinAy_ = newSinAy;

        centerX_ = centerX_ + velX_;
        centerY_ = centerY_ + velY_;
        // Comparisons go through bool locals: this compiler has compare
        // macros that produce a value (#compareGreaterflt etc.) but no
        // fused float compare-and-branch (#branch_greaterflt), which
        // `if (a > b)` would compile to.
        bool tooRight = centerX_ > MaxCenterX;
        bool tooLeft = centerX_ < MinCenterX;
        bool tooLow = centerY_ > MaxCenterY;
        bool tooHigh = centerY_ < MinCenterY;
        if (tooRight || tooLeft)
            velX_ = -velX_;
        if (tooLow || tooHigh)
            velY_ = -velY_;
    }

    // Computes a vertex's screen X and Y in one pass (into lastVx_/lastVy_);
    // two output fields because this compiler supports neither tuples nor
    // out params.
    void ComputeVertexProjection(float px, float py, float pz)
    {
        float x1 = px * cosAy_ + pz * sinAy_;
        float z1 = pz * cosAy_ - px * sinAy_;
        float y2 = py * cosAx_ - z1 * sinAx_;
        float z2 = py * sinAx_ + z1 * cosAx_;
        float scale = CubeCamDist / (CubeCamDist + z2);
        lastVx_ = (uint)(centerX_ + x1 * scale);
        lastVy_ = (uint)(centerY_ - y2 * scale);
    }

    // First-frame-only: draws all 12 edges with no prior frame to erase.
    void DrawEdges(uint x0, uint y0, uint x1, uint y1, uint x2, uint y2, uint x3, uint y3,
                   uint x4, uint y4, uint x5, uint y5, uint x6, uint y6, uint x7, uint y7,
                   bool on)
    {
        C64.Screen.DrawLine(x0, y0, x1, y1, on);
        C64.Screen.DrawLine(x1, y1, x2, y2, on);
        C64.Screen.DrawLine(x2, y2, x3, y3, on);
        C64.Screen.DrawLine(x3, y3, x0, y0, on);
        C64.Screen.DrawLine(x4, y4, x5, y5, on);
        C64.Screen.DrawLine(x5, y5, x6, y6, on);
        C64.Screen.DrawLine(x6, y6, x7, y7, on);
        C64.Screen.DrawLine(x7, y7, x4, y4, on);
        C64.Screen.DrawLine(x0, y0, x4, y4, on);
        C64.Screen.DrawLine(x1, y1, x5, y5, on);
        C64.Screen.DrawLine(x2, y2, x6, y6, on);
        C64.Screen.DrawLine(x3, y3, x7, y7, on);
    }

    // Every other frame: erases each old edge and immediately draws its
    // replacement, one edge at a time (see class comment).
    void UpdateEdges(uint ox0, uint oy0, uint ox1, uint oy1, uint ox2, uint oy2, uint ox3, uint oy3,
                     uint ox4, uint oy4, uint ox5, uint oy5, uint ox6, uint oy6, uint ox7, uint oy7,
                     uint nx0, uint ny0, uint nx1, uint ny1, uint nx2, uint ny2, uint nx3, uint ny3,
                     uint nx4, uint ny4, uint nx5, uint ny5, uint nx6, uint ny6, uint nx7, uint ny7)
    {
        C64.Screen.DrawLine(ox0, oy0, ox1, oy1, false); C64.Screen.DrawLine(nx0, ny0, nx1, ny1, true);
        C64.Screen.DrawLine(ox1, oy1, ox2, oy2, false); C64.Screen.DrawLine(nx1, ny1, nx2, ny2, true);
        C64.Screen.DrawLine(ox2, oy2, ox3, oy3, false); C64.Screen.DrawLine(nx2, ny2, nx3, ny3, true);
        C64.Screen.DrawLine(ox3, oy3, ox0, oy0, false); C64.Screen.DrawLine(nx3, ny3, nx0, ny0, true);
        C64.Screen.DrawLine(ox4, oy4, ox5, oy5, false); C64.Screen.DrawLine(nx4, ny4, nx5, ny5, true);
        C64.Screen.DrawLine(ox5, oy5, ox6, oy6, false); C64.Screen.DrawLine(nx5, ny5, nx6, ny6, true);
        C64.Screen.DrawLine(ox6, oy6, ox7, oy7, false); C64.Screen.DrawLine(nx6, ny6, nx7, ny7, true);
        C64.Screen.DrawLine(ox7, oy7, ox4, oy4, false); C64.Screen.DrawLine(nx7, ny7, nx4, ny4, true);
        C64.Screen.DrawLine(ox0, oy0, ox4, oy4, false); C64.Screen.DrawLine(nx0, ny0, nx4, ny4, true);
        C64.Screen.DrawLine(ox1, oy1, ox5, oy5, false); C64.Screen.DrawLine(nx1, ny1, nx5, ny5, true);
        C64.Screen.DrawLine(ox2, oy2, ox6, oy6, false); C64.Screen.DrawLine(nx2, ny2, nx6, ny6, true);
        C64.Screen.DrawLine(ox3, oy3, ox7, oy7, false); C64.Screen.DrawLine(nx3, ny3, nx7, ny7, true);
    }
}
