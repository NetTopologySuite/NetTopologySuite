using System;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A utility for making programming assertions.
    /// </summary>
    public class Assert
    {
        /// <summary>
        /// Only static methods!
        /// </summary>
        private Assert() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assertion"></param>
        public static void IsTrue(bool assertion)
        {
            IsTrue(assertion, null);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assertion"></param>
        /// <param name="message"></param>
        public static void IsTrue(bool assertion, string message)
        {
            if (!assertion)
            {
                if (message == null)               
                     throw new AssertionFailedException();                
                else throw new AssertionFailedException(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expectedValue"></param>
        /// <param name="actualValue"></param>
       
        public static void IsEquals(Object expectedValue, Object actualValue)
        {
            IsEquals(expectedValue, actualValue, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expectedValue"></param>
        /// <param name="actualValue"></param>
        /// <param name="message"></param>
        public static void IsEquals(Object expectedValue, Object actualValue, string message)
        {
            if (!actualValue.Equals(expectedValue))
                throw new AssertionFailedException("Expected " + expectedValue + " but encountered "
                            + actualValue + (message != null ? ": " + message : String.Empty));            
        }

        /// <summary>
        /// 
        /// </summary>
        public static void ShouldNeverReachHere()
        {
            ShouldNeverReachHere(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void ShouldNeverReachHere(string message)
        {
            throw new AssertionFailedException("Should never reach here"
                + (message != null ? ": " + message : String.Empty));
        }
    }
}
