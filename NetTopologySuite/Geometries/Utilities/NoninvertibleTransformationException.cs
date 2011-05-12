using System;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    ///<summary>
    /// Indicates that an <see cref="AffineTransformation"/> is non-invertible.
    ///</summary>
    ///<author>Martin Davis</author>
    public class NoninvertibleTransformationException : Exception
    {
        public NoninvertibleTransformationException()
        {
        }
        public NoninvertibleTransformationException(string transformationIsNonInvertible)
            :base(transformationIsNonInvertible)
        {
                
        }
    }
}