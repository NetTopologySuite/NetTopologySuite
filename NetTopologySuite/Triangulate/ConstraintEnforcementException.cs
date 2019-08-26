using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// Indicates a failure during constraint enforcement.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <version>1.0</version>
    public class ConstraintEnforcementException : Exception
    {
        private static string MsgWithCoord(string msg, Coordinate pt) {
            if (pt != null)
                return msg + " [ " + WKTWriter.ToPoint(pt) + " ]";
            return msg;
        }

        private readonly Coordinate _pt;

        /// <summary>
        /// Creates a new instance with a given message.
        /// </summary>
        /// <param name="msg">a string</param>
        public ConstraintEnforcementException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// Creates a new instance with a given message and approximate location.
        /// </summary>
        /// <param name="msg">a string</param>
        /// <param name="pt">the location of the error</param>
        public ConstraintEnforcementException(string msg, Coordinate pt)
            : base(MsgWithCoord(msg, pt))
        {
            _pt = pt.Copy();
        }

        /// <summary>
        /// Gets the approximate location of this error.
        /// </summary>
        /// <remarks>a location</remarks>
        public Coordinate Coordinate => _pt;
    }
}
