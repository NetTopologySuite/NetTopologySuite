using System.Globalization;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A utility for making programming assertions.
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Tests if <paramref name="assertion"/> is <c>true</c>
        /// <para/>
        /// If the test fails, <see cref="AssertionFailedException"/> with no message is thrown.
        /// </summary>
        /// <param name="assertion">The assertion value</param>
        public static void IsTrue(bool assertion)
        {
            IsTrue(assertion, null);
        }

        /// <summary>
        /// Tests if <paramref name="assertion"/> is <c>true</c>
        /// <para/>
        /// If the test fails, <see cref="AssertionFailedException"/> with <paramref name="message"/> is thrown.
        /// </summary>
        /// <param name="assertion">The assertion value</param>
        /// <param name="message">A message describing the failure condition.</param>
        public static void IsTrue(bool assertion, string message)
        {
            if (assertion) return;
            if (message == null)
                throw new AssertionFailedException();
            throw new AssertionFailedException(message);
        }

        /// <summary>
        /// Tests if two values are equal.
        /// <para/>
        /// If the test fails, <see cref="AssertionFailedException"/> with no specific message is thrown.
        /// </summary>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        public static void IsEquals(object expectedValue, object actualValue)
        {
            IsEquals(expectedValue, actualValue, null);
        }

        /// <summary>
        /// Tests if two values are equal.
        /// <para/>
        /// If the test fails, <see cref="AssertionFailedException"/> with <paramref name="message"/> is thrown.
        /// </summary>
        /// <param name="expectedValue">The expected value</param>
        /// <param name="actualValue">The actual value</param>
        /// <param name="message">A message describing the failure condition.</param>
        public static void IsEquals(object expectedValue, object actualValue, string message)
        {
            if (actualValue.Equals(expectedValue))
                return;
            string s = message != null ? ": " + message : string.Empty;
            string format = string.Format(CultureInfo.InvariantCulture, "Expected {0} but encountered {1}{2}", expectedValue, actualValue, s);
            throw new AssertionFailedException(format);
        }

        /// <summary>
        /// Throws an <see cref="AssertionFailedException"/> with no specific message text.
        /// </summary>
        public static void ShouldNeverReachHere()
        {
            ShouldNeverReachHere(null);
        }

        /// <summary>
        /// Throws an <see cref="AssertionFailedException"/> with <paramref name="message"/> as specific message text.
        /// </summary>
        /// <param name="message">A text describing the failure condition</param>
        public static void ShouldNeverReachHere(string message)
        {
            string s = (message != null ? ": " + message : string.Empty);
            string format = string.Format("Should never reach here{0}", s);
            throw new AssertionFailedException(format);
        }
    }
}
