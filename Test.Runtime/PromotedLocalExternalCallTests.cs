using NUnit.Framework;
using Assert = C64TestFramework.Assert;

namespace Compiler.RuntimeTest;

// Permanent regression coverage for a real bug (found via a screenshot
// of C64Presentation's AgendaSlide: Title.Draw()'s two underline rows
// drew at the wrong column, even though the title text itself was fine).
//
// Root cause: zp_local1 ($29, one of the 4 bytes ILMethodPromoteLocalsPass
// originally used for promoted locals) aliases tmpPointer+1
// (heap_tmp_pointer = $28, asm/helper/heap.asm) -- resolveObjPtr's own
// output register, which EVERY field-access macro (pushfld8/16/flt,
// setfld8/16(_const), incfld) overwrites on every single field read or
// write. A method with two promoted locals (here: a string's .Length,
// then a value computed from it) had the second one assigned zp_local1;
// the very next field access after it was computed silently stomped it
// with the resolved object's own pointer high byte. Harmless for as long
// as Ldfld/Stfld were excluded from ILMethodPromoteLocalsPass's
// SafeOpCodes; became live the moment that exclusion was lifted. Fixed by
// retiring zp_local1 entirely (zeropage.asm's own comment on zp_local0
// has the full incident); this test guards against it being silently
// reintroduced.
class TwoPromotedLocalsWithFieldWrite
{
    public string Text;
    public uint LengthResult;
    public uint StartXResult;

    public void Compute()
    {
        int length = Text.Length;
        int startX = (40 - length) >> 1;
        LengthResult = (uint)length;   // a field WRITE sits between
        StartXResult = (uint)startX;   // computing startX and using it again
    }
}

[TestFixture]
class PromotedLocalExternalCallTests
{
    [Test]
    public void Second_Promoted_Local_Survives_Field_Write_In_Between()
    {
        var m = new TwoPromotedLocalsWithFieldWrite { Text = "AGENDA" };
        m.Compute();
        Assert.AreEqual(m.LengthResult, 6u);
        Assert.AreEqual(m.StartXResult, 17u);
    }
}
