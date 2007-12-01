using System;
using System.Diagnostics;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A utility for making programming assertions.
    /// </summary>
    public static class Assert
    {
        public static void IsTrue(Boolean assertion)
        {
            IsTrue(assertion, null);
        }

        public static void IsTrue(Boolean assertion, string message)
        {
            if (!assertion)
            {
                if (message == null)
                {
                    throw new AssertionFailedException();
                }
                else
                {
                    throw new AssertionFailedException(message);
                }
            }
        }

        public static void IsEquals(Object expectedValue, Object actualValue)
        {
            IsEquals(expectedValue, actualValue, null);
        }

        public static void IsEquals(Object expectedValue, Object actualValue, string message)
        {
            if (!actualValue.Equals(expectedValue))
            {
                throw new AssertionFailedException("Expected " + expectedValue + " but encountered "
                                                   + actualValue + (message != null ? ": " + message : String.Empty));
            }
        }

        [Conditional("DEBUG")]
        public static void ShouldNeverReachHere()
        {
            ShouldNeverReachHere(null);
        }

        [Conditional("DEBUG")]
        public static void ShouldNeverReachHere(string message)
        {
            throw new AssertionFailedException("Should never reach here"
                                               + (message != null 
                                                    ? ": " + message 
                                                    : String.Empty));
        }
    }
}