using System;
using System.Runtime.Serialization;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    [Serializable]
    public class NotRepresentableException : Exception
    {
        public NotRepresentableException() : base("Projective point not representable on the Cartesian plane.")
        {
        }

        protected NotRepresentableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}