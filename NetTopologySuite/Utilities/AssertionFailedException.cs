using System;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
   /// <summary>
   /// 
   /// </summary>
    public class AssertionFailedException : ApplicationException 
    {
        /// <summary>
        /// 
        /// </summary>
        public AssertionFailedException() : base() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public AssertionFailedException(string message) : base(message) { }
    }
}
