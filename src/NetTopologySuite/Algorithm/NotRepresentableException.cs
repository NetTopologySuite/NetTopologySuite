using System;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    ///
    /// </summary>
    public class NotRepresentableException : ApplicationException
    {
        /// <summary>
        ///
        /// </summary>
        public NotRepresentableException() : base("Projective point not representable on the Cartesian plane.") { }
    }
}
