using System;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A utility for making programming assertions.
    /// </summary>
    public static class Assert
    {
        public static void IsTrue(bool assertion)
        {
            IsTrue(assertion, null);
        }

        public static void IsTrue(bool assertion, string message)
        {
            if (assertion) return;
            if (message == null)
                throw new AssertionFailedException();
            throw new AssertionFailedException(message);
        }

        public static void IsEquals(object expectedValue, object actualValue)
        {
            IsEquals(expectedValue, actualValue, null);
        }

        public static void IsEquals(object expectedValue, object actualValue, string message)
        {
            if (actualValue.Equals(expectedValue))
                return;
            string s = message != null ? ": " + message : string.Empty;
            string format = string.Format("Expected {0} but encountered {1}{2}", expectedValue, actualValue, s);
            throw new AssertionFailedException(format);
        }

        public static void ShouldNeverReachHere()
        {
            ShouldNeverReachHere(null);
        }

        public static void ShouldNeverReachHere(string message)
        {
            string s = (message != null ? ": " + message : string.Empty);
            string format = string.Format("Should never reach here{0}", s);
            throw new AssertionFailedException(format);
        }
    }
}
