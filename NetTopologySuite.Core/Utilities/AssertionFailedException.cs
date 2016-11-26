#if !PCL
using System;
#else
using Exception = System.Exception;
#endif

namespace NetTopologySuite.Utilities
{
   /// <summary>
   /// 
   /// </summary>
    public class AssertionFailedException : Exception 
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
