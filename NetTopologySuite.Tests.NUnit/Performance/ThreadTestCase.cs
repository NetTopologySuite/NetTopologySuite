using System.Threading;

namespace NetTopologySuite.Tests.NUnit.Performance
{
    /// <summary>
    /// Base class for test cases which depend on threading.
    /// A common example of usage is to test for race conditions.
    /// </summary>
    /// <author>Martin Davis</author>
    public abstract class ThreadTestCase
    {
        public int ThreadCount
        {
            get { return ThreadTestRunner.DefaultThreadCount; }
        }

        public abstract void Setup();
        public WaitHandle[] WaitHandles { get; protected set; } 
        public abstract ParameterizedThreadStart GetRunnable(int threadIndex);
        public virtual object Argument { get { return null; } }
    }
}