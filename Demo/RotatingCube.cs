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
    // Rotated cube axes, see BuildBasis (by_/bz_ never change).
    float ax_, ay_, az_, cx_, cy_, cz_, by_, bz_;
    uint lastVx_, lastVy_;

    uint pvx0_, pvy0_, pvx1_, pvy1_, pvx2_, pvy2_, pvx3_, pvy3_;
    uint pvx4_, pvy4_, pvx5_, pvy5_, pvx6_, pvy6_, pvx7_, pvy7_;
    // Whether each of the 12 edges was drawn last frame (see UpdateEdges).
    bool vis0_, vis1_, vis2_, vis3_, vis4_, vis5_, vis6_, vis7_, vis8_, vis9_, vis10_, vis11_;

    // Double-buffered mode (see Init): the hidden buffer still holds the
    // frame from two swaps ago, so that frame's vertices (ppv*) and edge
    // visibility (pvis*) are kept as well.
    bool doubleBuffered_;
    uint ppvx0_, ppvy0_, ppvx1_, ppvy1_, ppvx2_, ppvy2_, ppvx3_, ppvy3_;
    uint ppvx4_, ppvy4_, ppvx5_, ppvy5_, ppvx6_, ppvy6_, ppvx7_, ppvy7_;
    bool pvis0_, pvis1_, pvis2_, pvis3_, pvis4_, pvis5_, pvis6_, pvis7_, pvis8_, pvis9_, pvis10_, pvis11_;

    // The cube's screen position moves every frame and bounces off the
    // edges. Bounds keep every projected vertex inside 0-255 (8-bit
    // float->int, see class comment) and inside the 200-line screen: the
    // cube extends ~46 pixels from its center once rotation and
    // perspective are included.
    float centerX_, centerY_;
    float velX_, velY_;

    const float CubeSize = 20f;      // half-edge length
    const float CubeCamDist = 150f;  // pseudo-perspective "camera" distance
    const float FaceTolerance = CubeSize * CubeSize / CubeCamDist;  // see UpdateEdges
    // Colors for both bitmaps' color matrices in double-buffered mode (high
    // nibble foreground, low background): the same white-on-light-blue as
    // Program.cs, which only fills buffer 0's matrix.
    const uint BitmapColors = 0x1E;
    // Byte in the free gap under the color matrix ($0fe8-$0fff): the demo
    // runs double-buffered while it is 0 (the normal case: nothing else
    // ever writes there, and the assembled .prg has zeros in that gap) and
    // falls back to the single-buffered, erase-and-redraw path otherwise, so
    // one program image serves both: patch this byte to 1 in the .prg file
    // to see the flicker the double buffer removes (that is how the two ran
    // side by side in VICE).
    const ulong ModeByteAddress = 0x0FF0UL;
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
        by_ = CubeSize * cosAx_;
        bz_ = CubeSize * sinAx_;
        cosAy_ = 1f;
        sinAy_ = 0f;
        cosStep_ = MathF.Cos(0.15f);
        sinStep_ = MathF.Sin(0.15f);
        centerX_ = 128f;
        centerY_ = 100f;
        velX_ = 4f;
        velY_ = 3f;

        doubleBuffered_ = C64.GetMemory(ModeByteAddress, 0) == 0;
        if (doubleBuffered_)
        {
            // Buffer 0 is being shown; draw into the hidden buffer 1.
            C64.Screen.SetBitmapColors(BitmapColors);
            C64.Screen.SetDrawBuffer(1);
        }
    }

    public void Step()
    {
        BuildBasis();

        // Every vertex is (+-1,+-1,+-1)*CubeSize, so its rotated position is
        // +-A +-B +-C (the rotated axis vectors, size folded in). Opposite
        // vertices are exact negatives, so 4 sums cover all 8 vertices and
        // the other 4 use ProjectNeg (which projects -p without negating).
        //   v6 = P6   v0 = -P6     v7 = P7   v1 = -P7
        //   v2 = P2   v4 = -P2     v3 = P3   v5 = -P3
        float bcx = cx_;
        float bcy = by_ + cy_;
        float bcz = bz_ + cz_;
        float bmcx = -cx_;
        float bmcy = by_ - cy_;
        float bmcz = bz_ - cz_;

        float p6x = ax_ + bcx; float p6y = ay_ + bcy; float p6z = az_ + bcz;
        float p7x = bcx - ax_; float p7y = bcy - ay_; float p7z = bcz - az_;
        float p2x = ax_ + bmcx; float p2y = ay_ + bmcy; float p2z = az_ + bmcz;
        float p3x = bmcx - ax_; float p3y = bmcy - ay_; float p3z = bmcz - az_;

        ProjectNeg(p6x, p6y, p6z); uint vx0 = lastVx_; uint vy0 = lastVy_;
        ProjectNeg(p7x, p7y, p7z); uint vx1 = lastVx_; uint vy1 = lastVy_;
        Project(p2x, p2y, p2z); uint vx2 = lastVx_; uint vy2 = lastVy_;
        Project(p3x, p3y, p3z); uint vx3 = lastVx_; uint vy3 = lastVy_;
        ProjectNeg(p2x, p2y, p2z); uint vx4 = lastVx_; uint vy4 = lastVy_;
        ProjectNeg(p3x, p3y, p3z); uint vx5 = lastVx_; uint vy5 = lastVy_;
        Project(p6x, p6y, p6z); uint vx6 = lastVx_; uint vy6 = lastVy_;
        Project(p7x, p7y, p7z); uint vx7 = lastVx_; uint vy7 = lastVy_;

        if (doubleBuffered_)
        {
            UpdateEdgesBuffered(vx0, vy0, vx1, vy1, vx2, vy2, vx3, vy3,
                                vx4, vy4, vx5, vy5, vx6, vy6, vx7, vy7);
            C64.Screen.SwapBuffers();
        }
        else
        {
            UpdateEdges(vx0, vy0, vx1, vy1, vx2, vy2, vx3, vy3,
                        vx4, vy4, vx5, vy5, vx6, vy6, vx7, vy7);
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

    // The rotated cube axes (X/Y/Z unit vectors after rotating about Y then
    // X, scaled by CubeSize): 6 float multiplies per frame instead of
    // rotating all 8 vertices separately (~11 multiplies each). B.x is 0.
    //   X axis -> (ax_, ay_, az_)   Y axis -> (0, by_, bz_)   Z axis -> (cx_, cy_, cz_)
    void BuildBasis()
    {
        float sc = CubeSize * cosAy_;
        float ss = CubeSize * sinAy_;
        ax_ = sc;
        ay_ = ss * sinAx_;
        az_ = -(ss * cosAx_);
        cx_ = ss;
        cy_ = -(sc * sinAx_);
        cz_ = sc * cosAx_;
    }

    // Perspective-projects a rotated point (x, y, z) into lastVx_/lastVy_;
    // two output fields because this compiler supports neither tuples nor
    // out params.
    void Project(float x, float y, float z)
    {
        float scale = CubeCamDist / (CubeCamDist + z);
        lastVx_ = (uint)(centerX_ + x * scale);
        lastVy_ = (uint)(centerY_ - y * scale);
    }

    // Same as Project(-x, -y, -z), without the negations.
    void ProjectNeg(float x, float y, float z)
    {
        float scale = CubeCamDist / (CubeCamDist - z);
        lastVx_ = (uint)(centerX_ - x * scale);
        lastVy_ = (uint)(centerY_ + y * scale);
    }

    // Hidden-line removal by back-face culling: a cube is convex, so an edge
    // is visible exactly when at least one of its two faces faces the camera.
    // With the camera at z = -CamDist, the face whose outward normal is the
    // rotated axis n (unit length) is visible when n.z < -CubeSize/CamDist;
    // ax_/bz_/cz_ are those axes' z components times CubeSize, hence the
    // FaceTolerance threshold. Only 6 float compares per frame.
    //
    // Erases each previously visible old edge and draws each now-visible new
    // one, one edge at a time (see class comment). The old vertices are read
    // from the pv*_ fields (Step overwrites them after this returns) and the
    // old visibility from vis*_; on the first frame both are empty, so
    // nothing is erased.
    void UpdateEdges(uint nx0, uint ny0, uint nx1, uint ny1, uint nx2, uint ny2, uint nx3, uint ny3,
                     uint nx4, uint ny4, uint nx5, uint ny5, uint nx6, uint ny6, uint nx7, uint ny7)
    {
        // Compares go through bool locals, see Step.
        bool xp = az_ < -FaceTolerance;   // +X face (v1,v2,v5,v6)
        bool xn = az_ > FaceTolerance;    // -X face (v0,v3,v4,v7)
        bool yp = bz_ < -FaceTolerance;   // +Y face (v2,v3,v6,v7)
        bool yn = bz_ > FaceTolerance;    // -Y face (v0,v1,v4,v5)
        bool zp = cz_ < -FaceTolerance;   // +Z face (v4,v5,v6,v7)
        bool zn = cz_ > FaceTolerance;    // -Z face (v0,v1,v2,v3)

        bool e0 = yn || zn;
        if (vis0_) C64.Screen.DrawLine(pvx0_, pvy0_, pvx1_, pvy1_, false);
        if (e0) C64.Screen.DrawLine(nx0, ny0, nx1, ny1, true);
        vis0_ = e0;
        bool e1 = xp || zn;
        if (vis1_) C64.Screen.DrawLine(pvx1_, pvy1_, pvx2_, pvy2_, false);
        if (e1) C64.Screen.DrawLine(nx1, ny1, nx2, ny2, true);
        vis1_ = e1;
        bool e2 = yp || zn;
        if (vis2_) C64.Screen.DrawLine(pvx2_, pvy2_, pvx3_, pvy3_, false);
        if (e2) C64.Screen.DrawLine(nx2, ny2, nx3, ny3, true);
        vis2_ = e2;
        bool e3 = xn || zn;
        if (vis3_) C64.Screen.DrawLine(pvx3_, pvy3_, pvx0_, pvy0_, false);
        if (e3) C64.Screen.DrawLine(nx3, ny3, nx0, ny0, true);
        vis3_ = e3;
        bool e4 = yn || zp;
        if (vis4_) C64.Screen.DrawLine(pvx4_, pvy4_, pvx5_, pvy5_, false);
        if (e4) C64.Screen.DrawLine(nx4, ny4, nx5, ny5, true);
        vis4_ = e4;
        bool e5 = xp || zp;
        if (vis5_) C64.Screen.DrawLine(pvx5_, pvy5_, pvx6_, pvy6_, false);
        if (e5) C64.Screen.DrawLine(nx5, ny5, nx6, ny6, true);
        vis5_ = e5;
        bool e6 = yp || zp;
        if (vis6_) C64.Screen.DrawLine(pvx6_, pvy6_, pvx7_, pvy7_, false);
        if (e6) C64.Screen.DrawLine(nx6, ny6, nx7, ny7, true);
        vis6_ = e6;
        bool e7 = xn || zp;
        if (vis7_) C64.Screen.DrawLine(pvx7_, pvy7_, pvx4_, pvy4_, false);
        if (e7) C64.Screen.DrawLine(nx7, ny7, nx4, ny4, true);
        vis7_ = e7;
        bool e8 = xn || yn;
        if (vis8_) C64.Screen.DrawLine(pvx0_, pvy0_, pvx4_, pvy4_, false);
        if (e8) C64.Screen.DrawLine(nx0, ny0, nx4, ny4, true);
        vis8_ = e8;
        bool e9 = xp || yn;
        if (vis9_) C64.Screen.DrawLine(pvx1_, pvy1_, pvx5_, pvy5_, false);
        if (e9) C64.Screen.DrawLine(nx1, ny1, nx5, ny5, true);
        vis9_ = e9;
        bool e10 = xp || yp;
        if (vis10_) C64.Screen.DrawLine(pvx2_, pvy2_, pvx6_, pvy6_, false);
        if (e10) C64.Screen.DrawLine(nx2, ny2, nx6, ny6, true);
        vis10_ = e10;
        bool e11 = xn || yp;
        if (vis11_) C64.Screen.DrawLine(pvx3_, pvy3_, pvx7_, pvy7_, false);
        if (e11) C64.Screen.DrawLine(nx3, ny3, nx7, ny7, true);
        vis11_ = e11;
    }

    // Double-buffered version of UpdateEdges: everything is drawn into the
    // hidden buffer, which still holds the frame from two swaps ago, so that
    // frame's edges are erased (all of them first -- nothing new to clip
    // yet), the new frame's visible edges are drawn, and Step then swaps.
    // Nothing is ever shown half-updated, so there is no blinking. The
    // vertex/visibility history shifts at the end.
    void UpdateEdgesBuffered(uint nx0, uint ny0, uint nx1, uint ny1, uint nx2, uint ny2, uint nx3, uint ny3,
                             uint nx4, uint ny4, uint nx5, uint ny5, uint nx6, uint ny6, uint nx7, uint ny7)
    {
        bool xp = az_ < -FaceTolerance;
        bool xn = az_ > FaceTolerance;
        bool yp = bz_ < -FaceTolerance;
        bool yn = bz_ > FaceTolerance;
        bool zp = cz_ < -FaceTolerance;
        bool zn = cz_ > FaceTolerance;

        bool e0 = yn || zn;
        bool e1 = xp || zn;
        bool e2 = yp || zn;
        bool e3 = xn || zn;
        bool e4 = yn || zp;
        bool e5 = xp || zp;
        bool e6 = yp || zp;
        bool e7 = xn || zp;
        bool e8 = xn || yn;
        bool e9 = xp || yn;
        bool e10 = xp || yp;
        bool e11 = xn || yp;

        // Erase the frame this buffer still holds.
        if (pvis0_) C64.Screen.DrawLine(ppvx0_, ppvy0_, ppvx1_, ppvy1_, false);
        if (pvis1_) C64.Screen.DrawLine(ppvx1_, ppvy1_, ppvx2_, ppvy2_, false);
        if (pvis2_) C64.Screen.DrawLine(ppvx2_, ppvy2_, ppvx3_, ppvy3_, false);
        if (pvis3_) C64.Screen.DrawLine(ppvx3_, ppvy3_, ppvx0_, ppvy0_, false);
        if (pvis4_) C64.Screen.DrawLine(ppvx4_, ppvy4_, ppvx5_, ppvy5_, false);
        if (pvis5_) C64.Screen.DrawLine(ppvx5_, ppvy5_, ppvx6_, ppvy6_, false);
        if (pvis6_) C64.Screen.DrawLine(ppvx6_, ppvy6_, ppvx7_, ppvy7_, false);
        if (pvis7_) C64.Screen.DrawLine(ppvx7_, ppvy7_, ppvx4_, ppvy4_, false);
        if (pvis8_) C64.Screen.DrawLine(ppvx0_, ppvy0_, ppvx4_, ppvy4_, false);
        if (pvis9_) C64.Screen.DrawLine(ppvx1_, ppvy1_, ppvx5_, ppvy5_, false);
        if (pvis10_) C64.Screen.DrawLine(ppvx2_, ppvy2_, ppvx6_, ppvy6_, false);
        if (pvis11_) C64.Screen.DrawLine(ppvx3_, ppvy3_, ppvx7_, ppvy7_, false);

        // Draw the new frame.
        if (e0) C64.Screen.DrawLine(nx0, ny0, nx1, ny1, true);
        if (e1) C64.Screen.DrawLine(nx1, ny1, nx2, ny2, true);
        if (e2) C64.Screen.DrawLine(nx2, ny2, nx3, ny3, true);
        if (e3) C64.Screen.DrawLine(nx3, ny3, nx0, ny0, true);
        if (e4) C64.Screen.DrawLine(nx4, ny4, nx5, ny5, true);
        if (e5) C64.Screen.DrawLine(nx5, ny5, nx6, ny6, true);
        if (e6) C64.Screen.DrawLine(nx6, ny6, nx7, ny7, true);
        if (e7) C64.Screen.DrawLine(nx7, ny7, nx4, ny4, true);
        if (e8) C64.Screen.DrawLine(nx0, ny0, nx4, ny4, true);
        if (e9) C64.Screen.DrawLine(nx1, ny1, nx5, ny5, true);
        if (e10) C64.Screen.DrawLine(nx2, ny2, nx6, ny6, true);
        if (e11) C64.Screen.DrawLine(nx3, ny3, nx7, ny7, true);

        // History: what was one frame back is now two back.
        ppvx0_ = pvx0_; ppvy0_ = pvy0_;
        ppvx1_ = pvx1_; ppvy1_ = pvy1_;
        ppvx2_ = pvx2_; ppvy2_ = pvy2_;
        ppvx3_ = pvx3_; ppvy3_ = pvy3_;
        ppvx4_ = pvx4_; ppvy4_ = pvy4_;
        ppvx5_ = pvx5_; ppvy5_ = pvy5_;
        ppvx6_ = pvx6_; ppvy6_ = pvy6_;
        ppvx7_ = pvx7_; ppvy7_ = pvy7_;
        pvis0_ = vis0_; vis0_ = e0;
        pvis1_ = vis1_; vis1_ = e1;
        pvis2_ = vis2_; vis2_ = e2;
        pvis3_ = vis3_; vis3_ = e3;
        pvis4_ = vis4_; vis4_ = e4;
        pvis5_ = vis5_; vis5_ = e5;
        pvis6_ = vis6_; vis6_ = e6;
        pvis7_ = vis7_; vis7_ = e7;
        pvis8_ = vis8_; vis8_ = e8;
        pvis9_ = vis9_; vis9_ = e9;
        pvis10_ = vis10_; vis10_ = e10;
        pvis11_ = vis11_; vis11_ = e11;
    }
}
