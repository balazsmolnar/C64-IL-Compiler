namespace C64TestFramework
{
    public class Assert
    {
        public static void Fail(string message = "TEST FAILED") { }

        public static void AreEqual(int actual, int expected, string message = "NOT EQUAL") { }
    }
}