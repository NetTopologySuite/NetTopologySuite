using System.Threading;

namespace NetTopologySuite.Tests.NUnit.Performance
{
    /// <summary>
    /// Runs a <see cref="ThreadTestCase"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class ThreadTestRunner
    {

        //Do not assign a value > 64. 
        //Test may provide WaitHandles for each job 
        //and more than 64 WaitHandles are not supported!
        public static readonly int DefaultThreadCount = 10;

        public static void Run(ThreadTestCase testcase)
        {
            testcase.Setup();

            for (var i = 0; i < testcase.ThreadCount; i++)
            {
                var runnable = testcase.GetRunnable(i);
                var t = new Thread(runnable);
                t.Start(testcase.Argument);
            }

            if (testcase.WaitHandles != null)
                WaitHandle.WaitAll(testcase.WaitHandles);
        }
    }
}