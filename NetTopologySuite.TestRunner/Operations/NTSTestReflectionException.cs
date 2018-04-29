using System;
using System.Runtime.Serialization;
using System.Text;

namespace Open.Topology.TestRunner.Operations
{
    public class NTSTestReflectionException : Exception
    {
        public NTSTestReflectionException()
        {}

        public NTSTestReflectionException(string message)
            :base(message)
        {
        }

        public NTSTestReflectionException(string message, Exception exception)
            : base(message, exception)
        {
        }

        public NTSTestReflectionException(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {}

        public NTSTestReflectionException(string opName, object[] args)
            :this(GenerateMessage(opName, args))
        {
        }

        private static string GenerateMessage(string opName, object[] args)
        {
            var sb = new StringBuilder("Cannot invoke ");
            sb.AppendFormat("'{0}'", opName);
            if (args == null || args.Length == 0)
                sb.Append("!");
            else
            {
                sb.AppendFormat(" with the following Parameters: ({0}", args[0]);
                for (var i = 1; i < args.Length; i++)
                    sb.AppendFormat(", {0}", args[i]);
                sb.Append(")!");
            }
            return sb.ToString();
        }
    }
}