#if HAS_SYSTEM_APPLICATIONEXCEPTION
using System;
#else
using ApplicationException = System.Exception;
#endif

namespace NetTopologySuite.Utilities
{
   /// <summary>
   ///
   /// </summary>
    public class AssertionFailedException : ApplicationException
    {
        /// <summary>
        ///
        /// </summary>
        public AssertionFailedException()
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        public AssertionFailedException(string message) : base(message) { }
    }
}
