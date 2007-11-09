using System;
using System.Runtime.Serialization;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    [Serializable]
    public class AssertionFailedException : NtsException
    {
        public AssertionFailedException() {}
        public AssertionFailedException(string message) : base(message) {}
        public AssertionFailedException(string message, Exception inner) 
            : base(message, inner) {}

        protected AssertionFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}