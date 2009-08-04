using System;
using System.Runtime.Serialization;

namespace GisSharpBlog.NetTopologySuite
{
    [Serializable]
    public class NtsException : Exception
    {
        public NtsException()
        {
        }

        public NtsException(string message) : base(message)
        {
        }

        public NtsException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NtsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}