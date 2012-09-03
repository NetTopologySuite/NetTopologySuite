using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace NetTopologySuite.Tests.NUnit.Performance
{
    /// <summary>
    /// Runs <see cref="PerformanceTestCase"/> classes which contain performance tests.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PerformanceTestRunner
    {
        private const String RunPrefix = "Run";
        private const String InitMethod = "Init";

        public static void Run(Type clz)
        {
            var runner = new PerformanceTestRunner();
            runner.RunInternal(clz);
        }

        private PerformanceTestRunner()
        {

        }

        private void RunInternal(Type clz)
        {
            try
            {
                var ctor = clz.GetConstructor(new Type[0]);

                var test = (PerformanceTestCase) ctor.Invoke(new object[0]);
                var runSize = test.RunSize;
                int runIter = test.RunIterations;
                var runMethod = FindMethods(clz, RunPrefix);

                // do the run
                test.SetUp();
                for (var runNum = 0; runNum < runSize.Length; runNum++)
                {
                    int size = runSize[runNum];
                    test.StartRun(size);
                    for (var i = 0; i < runMethod.Length; i++)
                    {
                        var sw = new Stopwatch();
                        sw.Start();
                        for (var iter = 0; iter < runIter; iter++)
                        {
                            runMethod[i].Invoke(test, new object[0]);
                        }
                        Console.WriteLine(runMethod[i].Name + " : " + sw.Elapsed);
                    }
                    test.EndRun();
                }
                test.TearDown();
            }
            catch (Exception e)
            {
                // TODO Auto-generated catch block
                Console.WriteLine(e.StackTrace);
            }
        }


        private static MethodInfo[] FindMethods(Type clz, String methodPrefix)
        {
            var runMeths = new List<MethodInfo>();
            var meth = clz.GetMethods();
            for (var i = 0; i < meth.Length; i++)
            {
                if (meth[i].Name.StartsWith(methodPrefix))
                {
                    runMeths.Add(meth[i]);
                }
            }
            return runMeths.ToArray();
        }
    }
}