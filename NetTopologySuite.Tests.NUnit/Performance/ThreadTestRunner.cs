using System.Threading;

namespace NetTopologySuite.Tests.NUnit.Performance
{
    /// <summary>
    /// Runs a <see cref="ThreadTestCase"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class ThreadTestRunner
    {

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
        }
    }
}