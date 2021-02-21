namespace C64TestFramework
{
    public class Assert
    {
        public static void Fail(string message = "TEST FAILED") { }

        public static void AreEqual(int actual, int expected, string message = "NOT EQUAL") { }

        public static void AreEqual(uint actual, uint expected, string message = "NOT EQUAL") { }

        public static void IsTrue(bool value, string message = "SHOULD BE TRUE") { }

        public static void IsFalse(bool value, string message = "SHOULD BE FALSE") { }
    }
}