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
    // Whether each of the 6 faces was drawn last frame (see UpdateFaces).
    bool visXp_, visXn_, visYp_, visYn_, visZp_, visZn_;

    // Double-buffered mode (see Init): the hidden buffer still holds the
    // frame from two swaps ago, so that frame's vertices (ppv*) and face
    // visibility (pvis*) are kept as well.
    bool doubleBuffered_;
    uint ppvx0_, ppvy0_, ppvx1_, ppvy1_, ppvx2_, ppvy2_, ppvx3_, ppvy3_;
    uint ppvx4_, ppvy4_, ppvx5_, ppvy5_, ppvx6_, ppvy6_, ppvx7_, ppvy7_;
    bool pvisXp_, pvisXn_, pvisYp_, pvisYn_, pvisZp_, pvisZn_;

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
            UpdateFacesBuffered(vx0, vy0, vx1, vy1, vx2, vy2, vx3, vy3,
                                vx4, vy4, vx5, vy5, vx6, vy6, vx7, vy7);
            C64.Screen.SwapBuffers();
        }
        else
        {
            UpdateFaces(vx0, vy0, vx1, vy1, vx2, vy2, vx3, vy3,
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

    // Hidden-surface removal by back-face culling: a cube is convex, so a
    // face is visible exactly when it faces the camera. With the camera at
    // z = -CamDist, the face whose outward normal is the rotated axis n
    // (unit length) is visible when n.z < -CubeSize/CamDist; ax_/bz_/cz_
    // are those axes' z components times CubeSize, hence the FaceTolerance
    // threshold. Only 6 float compares per frame.
    //
    // Erases each previously visible old face and draws each now-visible
    // new one, one face at a time (see class comment -- same reasoning as
    // the old per-edge interleaving, just at face granularity now that
    // faces are filled solid instead of outlined). The old vertices are
    // read from the pv*_ fields (Step overwrites them after this returns)
    // and the old visibility from vis*_; on the first frame both are
    // empty, so nothing is erased.
    void UpdateFaces(uint nx0, uint ny0, uint nx1, uint ny1, uint nx2, uint ny2, uint nx3, uint ny3,
                     uint nx4, uint ny4, uint nx5, uint ny5, uint nx6, uint ny6, uint nx7, uint ny7)
    {
        // Compares go through bool locals, see Step.
        bool xp = az_ < -FaceTolerance;   // +X face (v1,v2,v5,v6)
        bool xn = az_ > FaceTolerance;    // -X face (v0,v3,v4,v7)
        bool yp = bz_ < -FaceTolerance;   // +Y face (v2,v3,v6,v7)
        bool yn = bz_ > FaceTolerance;    // -Y face (v0,v1,v4,v5)
        bool zp = cz_ < -FaceTolerance;   // +Z face (v4,v5,v6,v7)
        bool zn = cz_ > FaceTolerance;    // -Z face (v0,v1,v2,v3)

        if (visZn_) FillFace(pvx0_, pvy0_, pvx1_, pvy1_, pvx2_, pvy2_, pvx3_, pvy3_, false);
        if (zn) FillFace(nx0, ny0, nx1, ny1, nx2, ny2, nx3, ny3, true);
        visZn_ = zn;
        if (visZp_) FillFace(pvx4_, pvy4_, pvx5_, pvy5_, pvx6_, pvy6_, pvx7_, pvy7_, false);
        if (zp) FillFace(nx4, ny4, nx5, ny5, nx6, ny6, nx7, ny7, true);
        visZp_ = zp;
        if (visYn_) FillFace(pvx0_, pvy0_, pvx1_, pvy1_, pvx4_, pvy4_, pvx5_, pvy5_, false);
        if (yn) FillFace(nx0, ny0, nx1, ny1, nx4, ny4, nx5, ny5, true);
        visYn_ = yn;
        if (visYp_) FillFace(pvx2_, pvy2_, pvx3_, pvy3_, pvx6_, pvy6_, pvx7_, pvy7_, false);
        if (yp) FillFace(nx2, ny2, nx3, ny3, nx6, ny6, nx7, ny7, true);
        visYp_ = yp;
        if (visXp_) FillFace(pvx1_, pvy1_, pvx2_, pvy2_, pvx5_, pvy5_, pvx6_, pvy6_, false);
        if (xp) FillFace(nx1, ny1, nx2, ny2, nx5, ny5, nx6, ny6, true);
        visXp_ = xp;
        if (visXn_) FillFace(pvx0_, pvy0_, pvx3_, pvy3_, pvx4_, pvy4_, pvx7_, pvy7_, false);
        if (xn) FillFace(nx0, ny0, nx3, ny3, nx4, ny4, nx7, ny7, true);
        visXn_ = xn;
    }

    // Double-buffered version of UpdateFaces: everything is drawn into the
    // hidden buffer, which still holds the frame from two swaps ago, so
    // that frame's faces are erased (all of them first -- nothing new to
    // clip yet), the new frame's visible faces are drawn, and Step then
    // swaps. Nothing is ever shown half-updated, so there is no blinking.
    // The vertex history shifts in Step; only face visibility shifts here.
    void UpdateFacesBuffered(uint nx0, uint ny0, uint nx1, uint ny1, uint nx2, uint ny2, uint nx3, uint ny3,
                             uint nx4, uint ny4, uint nx5, uint ny5, uint nx6, uint ny6, uint nx7, uint ny7)
    {
        bool xp = az_ < -FaceTolerance;
        bool xn = az_ > FaceTolerance;
        bool yp = bz_ < -FaceTolerance;
        bool yn = bz_ > FaceTolerance;
        bool zp = cz_ < -FaceTolerance;
        bool zn = cz_ > FaceTolerance;

        // Erase the frame this buffer still holds.
        if (pvisZn_) FillFace(ppvx0_, ppvy0_, ppvx1_, ppvy1_, ppvx2_, ppvy2_, ppvx3_, ppvy3_, false);
        if (pvisZp_) FillFace(ppvx4_, ppvy4_, ppvx5_, ppvy5_, ppvx6_, ppvy6_, ppvx7_, ppvy7_, false);
        if (pvisYn_) FillFace(ppvx0_, ppvy0_, ppvx1_, ppvy1_, ppvx4_, ppvy4_, ppvx5_, ppvy5_, false);
        if (pvisYp_) FillFace(ppvx2_, ppvy2_, ppvx3_, ppvy3_, ppvx6_, ppvy6_, ppvx7_, ppvy7_, false);
        if (pvisXp_) FillFace(ppvx1_, ppvy1_, ppvx2_, ppvy2_, ppvx5_, ppvy5_, ppvx6_, ppvy6_, false);
        if (pvisXn_) FillFace(ppvx0_, ppvy0_, ppvx3_, ppvy3_, ppvx4_, ppvy4_, ppvx7_, ppvy7_, false);

        // Draw the new frame.
        if (zn) FillFace(nx0, ny0, nx1, ny1, nx2, ny2, nx3, ny3, true);
        if (zp) FillFace(nx4, ny4, nx5, ny5, nx6, ny6, nx7, ny7, true);
        if (yn) FillFace(nx0, ny0, nx1, ny1, nx4, ny4, nx5, ny5, true);
        if (yp) FillFace(nx2, ny2, nx3, ny3, nx6, ny6, nx7, ny7, true);
        if (xp) FillFace(nx1, ny1, nx2, ny2, nx5, ny5, nx6, ny6, true);
        if (xn) FillFace(nx0, ny0, nx3, ny3, nx4, ny4, nx7, ny7, true);

        // History: what was one frame back is now two back.
        ppvx0_ = pvx0_; ppvy0_ = pvy0_;
        ppvx1_ = pvx1_; ppvy1_ = pvy1_;
        ppvx2_ = pvx2_; ppvy2_ = pvy2_;
        ppvx3_ = pvx3_; ppvy3_ = pvy3_;
        ppvx4_ = pvx4_; ppvy4_ = pvy4_;
        ppvx5_ = pvx5_; ppvy5_ = pvy5_;
        ppvx6_ = pvx6_; ppvy6_ = pvy6_;
        ppvx7_ = pvx7_; ppvy7_ = pvy7_;
        pvisZn_ = visZn_; visZn_ = zn;
        pvisZp_ = visZp_; visZp_ = zp;
        pvisYn_ = visYn_; visYn_ = yn;
        pvisYp_ = visYp_; visYp_ = yp;
        pvisXp_ = visXp_; visXp_ = xp;
        pvisXn_ = visXn_; visXn_ = xn;
    }

    // ---------------------------------------------------------------------
    // Solid face fill: a cube face projects to a convex quadrilateral (4
    // vertices, order doesn't matter -- see SortQuadByY), filled as 2-3
    // horizontal-banded trapezoids via Screen.DrawTrapezoid (on=false
    // erases, matching DrawLine/DrawRectangle's own convention).
    // ---------------------------------------------------------------------

    // SortQuadByY's own output: the 4 vertices sorted by y ascending, each
    // x kept paired with its own y. This compiler has neither tuples nor
    // out params (see Project's own comment), so -- like Project/
    // ProjectNeg -- results come back through fields, not a return value.
    uint qx0_, qy0_, qx1_, qy1_, qx2_, qy2_, qx3_, qy3_;

    // Sorts 4 (x,y) pairs by y using the standard 5-comparator optimal
    // sorting network for 4 elements (compare 0-1, 2-3, 0-2, 1-3, then
    // 1-2). For a convex quadrilateral this also happens to identify its
    // shape correctly for FillFace below: the min-y and max-y vertices of
    // a convex quad are its two opposite corners, so BOTH remaining
    // (middle-y) vertices connect to the top corner and to the bottom
    // corner by a real edge of the quad -- which edge specifically doesn't
    // matter, only their (x,y) values do, so sorting by y alone (without
    // tracking the original connectivity order) is enough.
    void SortQuadByY(uint px0, uint py0, uint px1, uint py1, uint px2, uint py2, uint px3, uint py3)
    {
        uint x0 = px0, y0 = py0, x1 = px1, y1 = py1, x2 = px2, y2 = py2, x3 = px3, y3 = py3;
        uint tx, ty;
        if (y0 > y1) { tx = x0; ty = y0; x0 = x1; y0 = y1; x1 = tx; y1 = ty; }
        if (y2 > y3) { tx = x2; ty = y2; x2 = x3; y2 = y3; x3 = tx; y3 = ty; }
        if (y0 > y2) { tx = x0; ty = y0; x0 = x2; y0 = y2; x2 = tx; y2 = ty; }
        if (y1 > y3) { tx = x1; ty = y1; x1 = x3; y1 = y3; x3 = tx; y3 = ty; }
        if (y1 > y2) { tx = x1; ty = y1; x1 = x2; y1 = y2; x2 = tx; y2 = ty; }
        qx0_ = x0; qy0_ = y0; qx1_ = x1; qy1_ = y1; qx2_ = x2; qy2_ = y2; qx3_ = x3; qy3_ = y3;
    }

    // The x of the line from (x0,y0) to (x1,y1) at row y (y0<=y<=y1 or
    // y1<=y<=y0). Written with only non-negative subtractions (no signed
    // type needed, matching this compiler's unsigned uint/ulong) --
    // subtracts the smaller x from the larger for the magnitude, then
    // re-applies the direction as a final add or subtract.
    static ulong InterpolateX(ulong x0, ulong y0, ulong x1, ulong y1, ulong y)
    {
        if (y0 == y1)
            return x0;
        ulong loY = y0 < y1 ? y0 : y1;
        ulong hiY = y0 < y1 ? y1 : y0;
        ulong loX = y0 < y1 ? x0 : x1;
        ulong hiX = y0 < y1 ? x1 : x0;
        ulong dy = hiY - loY;
        bool rising = loX < hiX;
        ulong dx = rising ? hiX - loX : loX - hiX;
        ulong offset = dx * (y - loY) / dy;
        return rising ? loX + offset : loX - offset;
    }

    // Fills (on=true) or erases (on=false) one convex quad face, given its
    // 4 vertices in ANY order. Sorts them by y (SortQuadByY) into a top
    // corner, two middle vertices and a bottom corner, decides which
    // middle is geometrically on the left (smaller x) vs right, then
    // covers the shape with 2 trapezoids (if both middles share a row) or
    // 3 (the usual case: down to the earlier middle, between the two
    // middles -- where the earlier-ending side has switched to its
    // mid->bottom edge while the other is still interpolated along its
    // top->mid edge -- and down to the bottom corner), the same "one side
    // constant, other side bends partway down" shape DungeonView's own
    // wall/door panels use, just with the bend point found at runtime
    // instead of known ahead of time.
    void FillFace(uint px0, uint py0, uint px1, uint py1, uint px2, uint py2, uint px3, uint py3, bool on)
    {
        SortQuadByY(px0, py0, px1, py1, px2, py2, px3, py3);
        uint topX = qx0_, topY = qy0_, botX = qx3_, botY = qy3_;
        uint leftX, leftY, rightX, rightY;
        if (qx1_ <= qx2_) { leftX = qx1_; leftY = qy1_; rightX = qx2_; rightY = qy2_; }
        else { leftX = qx2_; leftY = qy2_; rightX = qx1_; rightY = qy1_; }

        if (leftY == rightY)
        {
            C64.Screen.DrawTrapezoid(topX, topX, topY, leftX, rightX, leftY, on);
            C64.Screen.DrawTrapezoid(leftX, rightX, leftY, botX, botX, botY, on);
        }
        else if (leftY < rightY)
        {
            ulong splitTop = InterpolateX(topX, topY, rightX, rightY, leftY);
            ulong splitBot = InterpolateX(leftX, leftY, botX, botY, rightY);
            C64.Screen.DrawTrapezoid(topX, topX, topY, leftX, splitTop, leftY, on);
            C64.Screen.DrawTrapezoid(leftX, splitTop, leftY, splitBot, rightX, rightY, on);
            C64.Screen.DrawTrapezoid(splitBot, rightX, rightY, botX, botX, botY, on);
        }
        else
        {
            ulong splitTop = InterpolateX(topX, topY, leftX, leftY, rightY);
            ulong splitBot = InterpolateX(rightX, rightY, botX, botY, leftY);
            C64.Screen.DrawTrapezoid(topX, topX, topY, splitTop, rightX, rightY, on);
            C64.Screen.DrawTrapezoid(splitTop, rightX, rightY, leftX, splitBot, leftY, on);
            C64.Screen.DrawTrapezoid(leftX, splitBot, leftY, botX, botX, botY, on);
        }
    }
}
