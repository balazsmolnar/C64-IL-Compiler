namespace C64TestFramework;

public class Assert
{
    public static void Fail(string message = "TEST FAILED") { }

    public static void AreEqual(int actual, int expected, string message = "NOT EQUAL") { }

    public static void AreEqual(uint actual, uint expected, string message = "NOT EQUAL") { }

    // Distinct name, not another AreEqual overload -- this compiler's
    // GetLabel() (ReflectedType.Name + Method.Name only, no parameter
    // types) can't distinguish overloads, so a same-named string overload
    // would collide with AreEqual's existing int/uint asm label. Content
    // comparison, not pointer comparison: a ToString() result and a
    // string literal are almost never the same address even when equal.
    public static void AreEqualString(string actual, string expected, string message = "NOT EQUAL") { }

    public static void IsTrue(bool value, string message = "SHOULD BE TRUE") { }

    public static void IsFalse(bool value, string message = "SHOULD BE FALSE") { }
}