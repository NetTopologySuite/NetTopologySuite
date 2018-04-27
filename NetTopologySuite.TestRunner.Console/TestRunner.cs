using System;
using System.IO;
using Open.Topology.TestRunner;

namespace ConsoleTestRunner
{
    /// <summary>
	/// Summary description for TestRunner.
	/// </summary>
	class TestRunner
	{
        private static double elapsedTime = 0;

        private int m_nSimpleTestCount       = 0;
        private int m_nSimpleTestFailures    = 0;
        private int m_nSimpleTestExceptions  = 0;
        private XmlTestType m_enumFilterType = XmlTestType.None;
        private bool m_bVerbose              = true;

        private TestInfoCollection m_listTestInfo = null;
        private int m_nTestCount  = 0;
        private int m_nFailures   = 0;
        private int m_nExceptions = 0;
        private int m_nTotalCount = 0;


        public TestRunner(TestInfoCollection listTestInfo)
        {
            m_listTestInfo = listTestInfo;
        }

        public TestRunner(XmlTestType filter, bool verbose)
        {
            m_enumFilterType = filter;
            m_bVerbose       = verbose;
        }

        public void SimpleTestReset(XmlTestType filter, bool verbose)
        {
            m_nSimpleTestCount      = 0;
            m_nSimpleTestFailures   = 0;
            m_nSimpleTestExceptions = 0;
            m_enumFilterType        = filter;
            m_bVerbose              = verbose;
        }

        public void OnSimpleTest(object sender, XmlTestEventArgs args)
        {
            if (m_enumFilterType == XmlTestType.None || 
                args.Test.TestType == m_enumFilterType)
            {
                if (m_bVerbose)
                {
                    Console.WriteLine("Test {0}, {1} ({2} : {3})", 
                        args.Index, args.Success, args.Test.TestType.ToString(),
                        args.Test.Description);
                }

                ++m_nSimpleTestCount;
                if (!args.Success)
                    m_nSimpleTestFailures++;
                if (args.Test.Thrown != null)
                    m_nSimpleTestExceptions++;
            }
        }
 
        public void OnTest(object sender, XmlTestEventArgs args)
        {
            ++m_nTestCount;
            if (!args.Success)
                m_nFailures++;
            if (args.Test.Thrown != null)            
                m_nExceptions++;
        }
                      
        public void PrintSimpleTestResult(int totalTest)
        {
            Console.WriteLine("Test Cases : {0}, Test Run: {1}, Failures: {2}, Test Exceptions: {3}",
                totalTest, m_nSimpleTestCount, m_nSimpleTestFailures, m_nSimpleTestExceptions);
            
            Console.WriteLine();
        }

        public void PrintResult()
        {
            Console.WriteLine();

            Console.WriteLine("   ************************ Final Results ********************   ");
            Console.WriteLine("Total Test Cases : {0}, Test Run: {1}, Failures: {2}, Test Exceptions: {3}",
                m_nTotalCount, m_nTestCount, m_nFailures, m_nExceptions);
            Console.WriteLine("Total elapsed time in milliseconds: " + elapsedTime);
            elapsedTime = 0;
            
            Console.WriteLine();
        }

        public bool Run()
        {
            if (m_listTestInfo != null)
            {
                try
                {
                    XmlTestController controller = new XmlTestController();

                    m_nTotalCount = 0;
                    for (int i = 0; i < m_listTestInfo.Count; i++)
                    {
                        TestInfo info = m_listTestInfo[i];
                        if (info != null)
                        {
                            if (info.FileName != null)
                                RunTestFile(info, controller);
                            else if (info.Directory != null)
                                RunTestDirectory(info, controller);
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    XmlTestExceptionManager.Publish(ex);
                    return false;
                }
            }
            return false;
        }
 
        public void OnErrorEvent(object sender, XmlTestErrorEventArgs args)
        {
            Exception ex = args.Thrown;
            if (ex != null)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine(ex.Source);
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }
        }

        private bool RunTestFile(TestInfo info, XmlTestController controller)
        {
            if (info != null)
            {
                XmlTestCollection listTests = null;
                try
                {
                    listTests = controller.Load(info.FileName);
                }
                catch (Exception ex)
                {
                    XmlTestExceptionManager.Publish(ex);
                } 

                SimpleTestReset(info.Filter, info.Verbose);

                if (listTests != null && listTests.Count > 0)
                {
                    listTests.TestEvent += new XmlTextEventHandler(OnSimpleTest);
                    listTests.TestEvent += new XmlTextEventHandler(OnTest);

                    if (info.Exception)
                    {
                        XmlTestExceptionManager.ErrorEvent += 
                            new XmlTestErrorEventHandler(this.OnErrorEvent);
                    }

                    try
                    {
                        Console.WriteLine("Running...{0}", listTests.Name);

                        XmlTestTimer timer = new XmlTestTimer();

                        timer.Start();

                        listTests.RunTests();

                        timer.Stop();

                        PrintSimpleTestResult(listTests.Count);

                        Console.WriteLine("Duration in milliseconds: {0}", timer.Duration * 1000);

                        elapsedTime += (timer.Duration * 1000);

                        m_nTotalCount += listTests.Count;

                        listTests.TestEvent -= new XmlTextEventHandler(OnSimpleTest);
                        listTests.TestEvent -= new XmlTextEventHandler(OnTest);

                        if (info.Exception)
                        {
                            XmlTestExceptionManager.ErrorEvent -= 
                                new XmlTestErrorEventHandler(this.OnErrorEvent);
                        }
                    }
                    catch (Exception ex)
                    {
                        XmlTestExceptionManager.Publish(ex);
                    }
                }

                return true;
            }

            return false;
        }

        private bool RunTestDirectory(TestInfo info, XmlTestController controller)
        {
            if (info != null && info.Directory != null)
            {
                try
                {
                    string currentDir = Environment.CurrentDirectory;
                    string[] files = Directory.GetFiles(info.Directory, "*.xml");
                    foreach (string file in files) 
                    {
                        info.FileName = file;
                        RunTestFile(info, controller);
                    }               
                    return true;
                }
                catch (Exception ex)
                {
                    XmlTestExceptionManager.Publish(ex);
                }
                return true;
            }
            return false;
        }
	}
}
