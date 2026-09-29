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

    // One color per axis pair of faces -- Background is deliberately not
    // used here (see Program.cs): a face fill using the same color as the
    // screen's own background would be invisible.
    const BitmapColorSource ColorX = BitmapColorSource.MatrixHigh;
    const BitmapColorSource ColorY = BitmapColorSource.MatrixLow;
    const BitmapColorSource ColorZ = BitmapColorSource.ColorRam;

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
            // Buffer 0 is being shown; draw into the hidden buffer 1. Both
            // buffers' color matrices already have the 3 face colors --
            // Program.cs's own SetBitmapColors (called before Init) fills
            // both whenever double buffering is compiled in (which it is,
            // via the SetDrawBuffer call right below), so nothing further
            // is needed here.
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
        bool xp = az_ < -FaceTolerance;   // +X face (v1,v2,v6,v5)
        bool xn = az_ > FaceTolerance;    // -X face (v3,v0,v4,v7)
        bool yp = bz_ < -FaceTolerance;   // +Y face (v2,v3,v7,v6)
        bool yn = bz_ > FaceTolerance;    // -Y face (v0,v1,v5,v4)
        bool zp = cz_ < -FaceTolerance;   // +Z face (v4,v5,v6,v7)
        bool zn = cz_ > FaceTolerance;    // -Z face (v0,v1,v2,v3)

        if (visZn_) FillFace(pvx0_, pvy0_, pvx1_, pvy1_, pvx2_, pvy2_, pvx3_, pvy3_, false, ColorZ);
        if (zn) FillFace(nx0, ny0, nx1, ny1, nx2, ny2, nx3, ny3, true, ColorZ);
        visZn_ = zn;
        if (visZp_) FillFace(pvx4_, pvy4_, pvx5_, pvy5_, pvx6_, pvy6_, pvx7_, pvy7_, false, ColorZ);
        if (zp) FillFace(nx4, ny4, nx5, ny5, nx6, ny6, nx7, ny7, true, ColorZ);
        visZp_ = zp;
        if (visYn_) FillFace(pvx0_, pvy0_, pvx1_, pvy1_, pvx5_, pvy5_, pvx4_, pvy4_, false, ColorY);
        if (yn) FillFace(nx0, ny0, nx1, ny1, nx5, ny5, nx4, ny4, true, ColorY);
        visYn_ = yn;
        if (visYp_) FillFace(pvx2_, pvy2_, pvx3_, pvy3_, pvx7_, pvy7_, pvx6_, pvy6_, false, ColorY);
        if (yp) FillFace(nx2, ny2, nx3, ny3, nx7, ny7, nx6, ny6, true, ColorY);
        visYp_ = yp;
        if (visXp_) FillFace(pvx1_, pvy1_, pvx2_, pvy2_, pvx6_, pvy6_, pvx5_, pvy5_, false, ColorX);
        if (xp) FillFace(nx1, ny1, nx2, ny2, nx6, ny6, nx5, ny5, true, ColorX);
        visXp_ = xp;
        if (visXn_) FillFace(pvx3_, pvy3_, pvx0_, pvy0_, pvx4_, pvy4_, pvx7_, pvy7_, false, ColorX);
        if (xn) FillFace(nx3, ny3, nx0, ny0, nx4, ny4, nx7, ny7, true, ColorX);
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
        if (pvisZn_) FillFace(ppvx0_, ppvy0_, ppvx1_, ppvy1_, ppvx2_, ppvy2_, ppvx3_, ppvy3_, false, ColorZ);
        if (pvisZp_) FillFace(ppvx4_, ppvy4_, ppvx5_, ppvy5_, ppvx6_, ppvy6_, ppvx7_, ppvy7_, false, ColorZ);
        if (pvisYn_) FillFace(ppvx0_, ppvy0_, ppvx1_, ppvy1_, ppvx5_, ppvy5_, ppvx4_, ppvy4_, false, ColorY);
        if (pvisYp_) FillFace(ppvx2_, ppvy2_, ppvx3_, ppvy3_, ppvx7_, ppvy7_, ppvx6_, ppvy6_, false, ColorY);
        if (pvisXp_) FillFace(ppvx1_, ppvy1_, ppvx2_, ppvy2_, ppvx6_, ppvy6_, ppvx5_, ppvy5_, false, ColorX);
        if (pvisXn_) FillFace(ppvx3_, ppvy3_, ppvx0_, ppvy0_, ppvx4_, ppvy4_, ppvx7_, ppvy7_, false, ColorX);

        // Draw the new frame.
        if (zn) FillFace(nx0, ny0, nx1, ny1, nx2, ny2, nx3, ny3, true, ColorZ);
        if (zp) FillFace(nx4, ny4, nx5, ny5, nx6, ny6, nx7, ny7, true, ColorZ);
        if (yn) FillFace(nx0, ny0, nx1, ny1, nx5, ny5, nx4, ny4, true, ColorY);
        if (yp) FillFace(nx2, ny2, nx3, ny3, nx7, ny7, nx6, ny6, true, ColorY);
        if (xp) FillFace(nx1, ny1, nx2, ny2, nx6, ny6, nx5, ny5, true, ColorX);
        if (xn) FillFace(nx3, ny3, nx0, ny0, nx4, ny4, nx7, ny7, true, ColorX);

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
    // Solid face fill: a cube face projects to a convex quadrilateral,
    // filled as 2-3 horizontal-banded trapezoids via Screen.DrawTrapezoid
    // (on=false erases, matching DrawLine/DrawRectangle's own convention).
    //
    // The 4 vertices MUST be given in the face's own real cyclic order
    // (consecutive parameters, and the last back to the first, are true
    // polygon edges -- see each UpdateFaces*/UpdateFacesBuffered* call
    // site's own comment for each face's order). An earlier version sorted
    // the 4 vertices by y outright and assumed the top and bottom
    // (min/max-y) vertices were always the quad's two OPPOSITE corners --
    // true for a "roughly square" projection, but false whenever a face
    // rotates through a near-edge-on view (which every face does twice per
    // rotation): there, the projected quad becomes long and thin, and the
    // top/bottom vertices can end up ADJACENT (sharing a real edge)
    // instead. Sorting-by-y doesn't know the difference and produced a
    // wrong shape whose erase call (computed the same wrong way, but from
    // a slightly different rotation) didn't fully cover its own draw call,
    // leaving permanent stripes behind as the cube kept turning -- this
    // version instead finds the real top/bottom via the given order, so it
    // always knows which case it's in.
    // ---------------------------------------------------------------------

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
        // Callers expect y within [loY,hiY] (a real convex polygon's chain
        // is y-monotonic between its top and bottom), but this compiler's
        // ulong is unsigned: a caller-side rounding slip that puts y even
        // 1 outside that range would make (y-loY) underflow to a huge
        // value instead of going negative, producing a wild x -- and from
        // there a wild bitmap pointer that scribbles far past the bitmap
        // (confirmed: this, not just the left>right span DrawTrapezoidSafe
        // already guards against, is what actually produced the all-white
        // hung screen/UNDEF-opcode crash before this clamp existed -- see
        // git history). Clamping here costs nothing in the normal case and
        // makes the function's own contract hold unconditionally.
        y = y < loY ? loY : y > hiY ? hiY : y;   // reuse the parameter slot, not a new local
        ulong dy = hiY - loY;
        bool rising = loX < hiX;
        ulong dx = rising ? hiX - loX : loX - hiX;
        ulong offset = dx * (y - loY) / dy;
        return rising ? loX + offset : loX - offset;
    }

    // Screen.DrawTrapezoid does NOT sort x0Left/x0Right or x1Left/x1Right
    // itself (see its own doc comment -- they're directional, unlike
    // DrawRectangle's symmetric corners), so a caller must guarantee
    // left<=right at each row. FillFace below determines which side is
    // left/right from the real geometry, but a
    // face viewed near edge-on (twice per rotation) can make the shape
    // thin enough that this compiler's truncating integer interpolation,
    // computed independently at two different split rows, occasionally
    // disagrees by a pixel on which side is which right at the crossover
    // -- rare, but a real x0Left>x0Right there would make Graphics_
    // SpanRow's row-byte-count computation wrap to a huge value and write
    // far past the bitmap (confirmed: this is what an all-white, hung
    // screen turned out to be before this wrapper existed -- see git
    // history). Independently swapping each row's own pair here is always
    // safe (it can only ever narrow-then-correct a wrong span, never
    // change a valid one) and costs a handful of cycles next to
    // DrawTrapezoid's own division-based setup.
    static void DrawTrapezoidSafe(ulong x0Left, ulong x0Right, ulong y0, ulong x1Left, ulong x1Right, ulong y1, bool on, BitmapColorSource color)
    {
        ulong lo0 = x0Left < x0Right ? x0Left : x0Right;
        ulong hi0 = x0Left < x0Right ? x0Right : x0Left;
        ulong lo1 = x1Left < x1Right ? x1Left : x1Right;
        ulong hi1 = x1Left < x1Right ? x1Right : x1Left;
        C64.Screen.DrawTrapezoid(lo0, hi0, y0, lo1, hi1, y1, on, color);
    }

    // Fills (on=true) or erases (on=false) one convex quad face, given its
    // 4 vertices in real cyclic order p0-p1-p2-p3-p0 (see the section
    // comment above). Everything here is ulong, not uint -- this
    // compiler's uint is only 8 bits (fine for the cube's own vertices,
    // deliberately kept under 256, but NOT safe to mix with ulong in a
    // comparison, per this codebase's own established caution around
    // mixed uint/ulong arithmetic), so InterpolateX's ulong results never
    // need converting back.
    //
    // Finds whichever of p0..p3 has the minimum y (ties broken by lowest
    // index) and relabels starting there (t0=that vertex, t1/t2/t3 = 1/2/3
    // steps further around the SAME cycle) -- a pure relabeling, not a
    // sort, so t0's two real polygon neighbors are always t1 and t3, and
    // t2 is always the opposite corner. Comparing t1.y/t2.y/t3.y against
    // each other then finds the bottom (max y) among exactly those three
    // real possibilities and picks one of two shapes:
    //
    //  - opposite (the usual case): bottom is t2, the corner opposite the
    //    top; the other two (t1,t3, renamed left/right by an x-compare)
    //    each connect to both top and bottom via one real edge -- 2
    //    trapezoids if they share a row, 3 otherwise (down to the earlier
    //    one, between the two -- where the earlier-ending side has
    //    switched to its mid->bottom edge while the other is still
    //    interpolated along its top->mid edge -- and down to the bottom).
    //  - adjacent: bottom is t1 or t3, directly adjacent to the top (a
    //    real edge connects them straight); the OTHER two vertices bend
    //    through the long way around. Splits into up to 3 bands at the
    //    two bend points, interpolating the direct edge's x at each.
    //
    // This is the same "one side constant/straight, other side bends
    // partway down" shape DungeonView's own wall/door panels use, just
    // with the bend points found at runtime instead of known ahead of
    // time. Deliberately kept as ONE method (with the opposite/adjacent
    // logic inline below, not split into separate FillFaceOpposite/
    // FillFaceAdjacent helpers the way an early version had it) and
    // reusing p0..p3 in place instead of separate t0..t3 locals: this
    // compiler gives every program a fixed 256-byte locals stack (asm/
    // helper/objectTables.asm), and the earlier, more deeply-nested
    // shape (three custom-method frames deep, under Step's own already
    // substantial 123-byte frame, each with its own copy of every
    // coordinate as a separate parameter) silently overflowed it
    // (confirmed: this, not a geometry bug, is what the all-white hung
    // screen/UNDEF-opcode crash actually was -- see git history). Every
    // DrawTrapezoid call still goes through the DrawTrapezoidSafe helper
    // below -- ONE level deep, not nested under another custom frame --
    // for its left<=right clamp.
    void FillFace(ulong p0x, ulong p0y, ulong p1x, ulong p1y, ulong p2x, ulong p2y, ulong p3x, ulong p3y, bool on, BitmapColorSource color)
    {
        // Rotates p0..p3 IN PLACE (reusing FillFace's own parameter slots,
        // not new locals -- see the section comment on why every byte of
        // this method's locals-stack frame matters) so p0 ends up holding
        // whichever vertex has the minimum y; only a 2-ulong scratch pair
        // (tx,ty) is needed regardless of which rotation applies.
        ulong tx, ty;
        if (p0y <= p1y && p0y <= p2y && p0y <= p3y)
        {
            // Already in order.
        }
        else if (p1y <= p2y && p1y <= p3y)
        {
            tx = p0x; ty = p0y;
            p0x = p1x; p0y = p1y; p1x = p2x; p1y = p2y; p2x = p3x; p2y = p3y; p3x = tx; p3y = ty;
        }
        else if (p2y <= p3y)
        {
            tx = p0x; ty = p0y; p0x = p2x; p0y = p2y; p2x = tx; p2y = ty;
            tx = p1x; ty = p1y; p1x = p3x; p1y = p3y; p3x = tx; p3y = ty;
        }
        else
        {
            tx = p3x; ty = p3y;
            p3x = p2x; p3y = p2y; p2x = p1x; p2y = p1y; p1x = p0x; p1y = p0y; p0x = tx; p0y = ty;
        }

        if (p2y >= p1y && p2y >= p3y)
        {
            // Opposite case: top=p0, bottom=p2, mids=p1/p3.
            ulong leftX, leftY, rightX, rightY;
            if (p1x <= p3x) { leftX = p1x; leftY = p1y; rightX = p3x; rightY = p3y; }
            else { leftX = p3x; leftY = p3y; rightX = p1x; rightY = p1y; }

            if (leftY == rightY)
            {
                DrawTrapezoidSafe(p0x, p0x, p0y, leftX, rightX, leftY, on, color);
                DrawTrapezoidSafe(leftX, rightX, leftY, p2x, p2x, p2y, on, color);
            }
            else if (leftY < rightY)
            {
                ulong splitTop = InterpolateX(p0x, p0y, rightX, rightY, leftY);
                ulong splitBot = InterpolateX(leftX, leftY, p2x, p2y, rightY);
                DrawTrapezoidSafe(p0x, p0x, p0y, leftX, splitTop, leftY, on, color);
                DrawTrapezoidSafe(leftX, splitTop, leftY, splitBot, rightX, rightY, on, color);
                DrawTrapezoidSafe(splitBot, rightX, rightY, p2x, p2x, p2y, on, color);
            }
            else
            {
                ulong splitTop = InterpolateX(p0x, p0y, leftX, leftY, rightY);
                ulong splitBot = InterpolateX(rightX, rightY, p2x, p2y, leftY);
                DrawTrapezoidSafe(p0x, p0x, p0y, splitTop, rightX, rightY, on, color);
                DrawTrapezoidSafe(splitTop, rightX, rightY, leftX, splitBot, leftY, on, color);
                DrawTrapezoidSafe(leftX, splitBot, leftY, p2x, p2x, p2y, on, color);
            }
            return;
        }

        // Adjacent case: top=p0, bottom+direct-edge-partner = whichever of
        // p1/p3 has the larger y, the other two (the opposite corner and
        // the remaining neighbor) are the bend chain, nearer-first.
        ulong botX, botY, midAX, midAY, midBX, midBY;
        if (p1y >= p3y) { botX = p1x; botY = p1y; midAX = p3x; midAY = p3y; midBX = p2x; midBY = p2y; }
        else { botX = p3x; botY = p3y; midAX = p1x; midAY = p1y; midBX = p2x; midBY = p2y; }

        ulong directAtA = InterpolateX(p0x, p0y, botX, botY, midAY);
        ulong directAtB = InterpolateX(p0x, p0y, botX, botY, midBY);

        if (directAtA <= midAX)
        {
            DrawTrapezoidSafe(p0x, p0x, p0y, directAtA, midAX, midAY, on, color);
            DrawTrapezoidSafe(directAtA, midAX, midAY, directAtB, midBX, midBY, on, color);
            DrawTrapezoidSafe(directAtB, midBX, midBY, botX, botX, botY, on, color);
        }
        else
        {
            DrawTrapezoidSafe(p0x, p0x, p0y, midAX, directAtA, midAY, on, color);
            DrawTrapezoidSafe(midAX, directAtA, midAY, midBX, directAtB, midBY, on, color);
            DrawTrapezoidSafe(midBX, directAtB, midBY, botX, botX, botY, on, color);
        }
    }
}
