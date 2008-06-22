using System;
using SysConsole = System.Console;

namespace GisSharpBlog.NetTopologySuite.Console
{
    internal class ConsoleTest
    {
        private static void PrintMenu()
        {
            SysConsole.WriteLine("** Interactive Test Instructions **");
            SysConsole.WriteLine("a. Enter the name of the test script file to run.");
            SysConsole.WriteLine("b. Or enter 'exit' (without the quote) to end the test.");
            SysConsole.WriteLine("c. Or enter 'default' (without the quote) to run the default tests.");
            SysConsole.WriteLine();
            SysConsole.Write("Test Runner>>");
        }

        private static void RunInteractive(XmlTestType filter, Boolean verbose)
        {
            XmlTestController controller = new XmlTestController();

            TestRunner runner = new TestRunner(filter, verbose);

            PrintMenu();

            String fileName = (SysConsole.ReadLine() ?? String.Empty).Trim();

            while (fileName != "exit")
            {
                XmlTestCollection listTests = null;
                try
                {
                    if (fileName == "default")
                    {
                        RunDefault();
                    }
                    else
                    {
                        listTests = controller.Load(fileName);
                    }
                }
                catch (Exception ex)
                {
                    XmlTestExceptionManager.Publish(ex);
                }

                if (listTests != null && listTests.Count > 0)
                {
                    listTests.TestEvent += runner.OnSimpleTest;

                    try
                    {
                        SysConsole.WriteLine("Running...{0}", listTests.Name);

                        listTests.RunTests();

                        runner.PrintSimpleTestResult(listTests.Count);

                        runner.SimpleTestReset(XmlTestType.None, verbose);
                    }
                    catch (Exception ex)
                    {
                        XmlTestExceptionManager.Publish(ex);
                    }
                }

                PrintMenu();

                fileName = (SysConsole.ReadLine() ?? String.Empty).Trim();
            }
        }

        private static void OnErrorEvent(Object sender, XmlTestErrorEventArgs args)
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

        private static void RunDefault()
        {
            TestOptionsParser parserOptions = new TestOptionsParser();
            TestInfoCollection listTests =
                parserOptions.ParseProject(@"..\..\..\NetTopologySuite.TestRunner.Tests\Default.xml");

            if (listTests != null && listTests.Count > 0)
            {
                TestRunner runner = new TestRunner(listTests);
                runner.Run();
                runner.PrintResult();
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(String[] args)
        {
            if (args == null || args.Length == 0)
            {
                XmlTestExceptionManager.ErrorEvent += OnErrorEvent;
                RunInteractive(XmlTestType.None, true);
            }
            else
            {
                TestOptionsParser parser = new TestOptionsParser();
                TestInfoCollection collection = parser.Parse(args);

                if (parser.IsDefault)
                {
                    RunDefault();
                }
                else
                {
                    if (collection != null)
                    {
                        if (collection.Count == 1)
                        {
                            // see if it is the interactive type
                            TestInfo info = collection[0];

                            if (info.Interactive)
                            {
                                if (info.Exception)
                                {
                                    XmlTestExceptionManager.ErrorEvent += OnErrorEvent;
                                }
                                
                                RunInteractive(info.Filter, info.Verbose);
                            }
                        }
                        else
                        {
                            TestRunner runner = new TestRunner(collection);
                            runner.Run();
                        }
                    }
                    else
                    {
                        XmlTestExceptionManager.ErrorEvent += OnErrorEvent;
                        RunInteractive(XmlTestType.None, true);
                    }
                }
            }
        }
    }
}