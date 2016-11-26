#if !PCL
using System;
#else
using Exception = System.Exception;
#endif

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// 
    /// </summary>
    public class NotRepresentableException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public NotRepresentableException() : base("Projective point not representable on the Cartesian plane.") { }
    }
}
