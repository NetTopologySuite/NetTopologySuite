using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


#if SILVERLIGHT
namespace System
{
    public class ApplicationException : Exception
    {
        public ApplicationException(string message)
            : base(message)
        {

        }

        public ApplicationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public ApplicationException()
        {
           
        }
    }
}
#endif

