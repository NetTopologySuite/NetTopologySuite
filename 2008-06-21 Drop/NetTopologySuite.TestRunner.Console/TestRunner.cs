using System;
using System.IO;
using SysConsole = System.Console;

namespace GisSharpBlog.NetTopologySuite.Console
{
    internal class TestRunner
    {
        private static Double elapsedTime;

        private Int32 _simpleTestCount;
        private Int32 _simpleTestFailures;
        private Int32 _simpleTestExceptions;
        private XmlTestType _filterType = XmlTestType.None;
        private Boolean _verbose = true;

        private readonly TestInfoCollection _listTestInfo;
        private Int32 _testCount;
        private Int32 _failures;
        private Int32 _exceptions;
        private Int32 _totalCount;


        public TestRunner(TestInfoCollection listTestInfo)
        {
            _listTestInfo = listTestInfo;
        }

        public TestRunner(XmlTestType filter, Boolean verbose)
        {
            _filterType = filter;
            _verbose = verbose;
        }

        public void SimpleTestReset(XmlTestType filter, Boolean verbose)
        {
            _simpleTestCount = 0;
            _simpleTestFailures = 0;
            _simpleTestExceptions = 0;
            _filterType = filter;
            _verbose = verbose;
        }

        public void OnSimpleTest(Object sender, XmlTestEventArgs args)
        {
            if (_filterType == XmlTestType.None ||
                args.Test.TestType == _filterType)
            {
                if (_verbose)
                {
                    SysConsole.WriteLine("Test {0}, {1} ({2} : {3})",
                                      args.Index,
                                      args.Success,
                                      args.Test.TestType,
                                      args.Test.Description);
                }

                ++_simpleTestCount;

                if (!args.Success)
                {
                    _simpleTestFailures++;
                }

                if (args.Test.Thrown != null)
                {
                    _simpleTestExceptions++;
                }
            }
        }

        public void OnTest(Object sender, XmlTestEventArgs args)
        {
            ++_testCount;
            if (!args.Success)
            {
                _failures++;
            }
            if (args.Test.Thrown != null)
            {
                _exceptions++;
            }
        }

        public void PrintSimpleTestResult(Int32 totalTest)
        {
            SysConsole.WriteLine(
                "Test Cases : {0}, Test Run: {1}, Failures: {2}, Test Exceptions: {3}",
                totalTest,
                _simpleTestCount,
                _simpleTestFailures,
                _simpleTestExceptions);

            SysConsole.WriteLine();
        }

        public void PrintResult()
        {
            SysConsole.WriteLine();

            SysConsole.WriteLine("   ************************ Final Results ********************   ");
            SysConsole.WriteLine(
                            "Total Test Cases : {0}, Test Run: {1}, Failures: {2}, Test Exceptions: {3}",
                            _totalCount,
                            _testCount,
                            _failures,
                            _exceptions);
            SysConsole.WriteLine("Total elapsed time in milliseconds: " + elapsedTime);
            elapsedTime = 0;

            SysConsole.WriteLine();
        }

        public Boolean Run()
        {
            if (_listTestInfo != null)
            {
                try
                {
                    XmlTestController controller = new XmlTestController();

                    _totalCount = 0;
                    for (Int32 i = 0; i < _listTestInfo.Count; i++)
                    {
                        TestInfo info = _listTestInfo[i];
                        if (info != null)
                        {
                            if (info.FileName != null)
                            {
                                runTestFile(info, controller);
                            }
                            else if (info.Directory != null)
                            {
                                runTestDirectory(info, controller);
                            }
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

        public void OnErrorEvent(Object sender, XmlTestErrorEventArgs args)
        {
            Exception ex = args.Thrown;

            if (ex != null)
            {
                SysConsole.WriteLine(ex.Message);
                SysConsole.WriteLine();
                SysConsole.WriteLine(ex.Source);
                SysConsole.WriteLine();
                SysConsole.WriteLine(ex.StackTrace);
            }
        }

        private void runTestFile(TestInfo info, XmlTestController controller)
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
                    listTests.TestEvent += OnSimpleTest;
                    listTests.TestEvent += OnTest;

                    if (info.Exception)
                    {
                        XmlTestExceptionManager.ErrorEvent += OnErrorEvent;
                    }

                    try
                    {
                        SysConsole.WriteLine("Running...{0}", listTests.Name);

                        XmlTestTimer timer = new XmlTestTimer();

                        timer.Start();

                        listTests.RunTests();

                        timer.Stop();

                        PrintSimpleTestResult(listTests.Count);

                        SysConsole.WriteLine("Duration in milliseconds: {0}", timer.Duration * 1000);

                        elapsedTime += (timer.Duration * 1000);

                        _totalCount += listTests.Count;

                        listTests.TestEvent -= OnSimpleTest;
                        listTests.TestEvent -= OnTest;

                        if (info.Exception)
                        {
                            XmlTestExceptionManager.ErrorEvent -= OnErrorEvent;
                        }
                    }
                    catch (Exception ex)
                    {
                        XmlTestExceptionManager.Publish(ex);
                    }
                }
            }
        }

        private void runTestDirectory(TestInfo info, XmlTestController controller)
        {
            if (info != null && info.Directory != null)
            {
                try
                {
                    String[] files = Directory.GetFiles(info.Directory, "*.xml");

                    foreach (String file in files)
                    {
                        info.FileName = file;
                        runTestFile(info, controller);
                    }
                }
                catch (Exception ex)
                {
                    XmlTestExceptionManager.Publish(ex);
                }
            }
        }
    }
}