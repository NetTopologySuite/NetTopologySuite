using System;
using Open.Topology.TestRunner;

namespace ConsoleTestRunner
{
	/// <summary>
	/// Summary description for ConsoleTest.
	/// </summary>
    class ConsoleTest
    {        
        static void PrintMenu()
        {
            Console.WriteLine("** Interactive Test Instructions **");
            Console.WriteLine("a. Enter the name of the test script file to run.");
            Console.WriteLine("b. Or enter 'exit' (without the quote) to end the test.");
            Console.WriteLine("c. Or enter 'default' (without the quote) to run the default tests.");
            Console.WriteLine();
            Console.Write("Test Runner>>");
        }

        static void RunInteractive(XmlTestType filter, bool verbose)
        {
            string fileName = String.Empty;

            XmlTestController controller = new XmlTestController();

            TestRunner runner = new TestRunner(filter, verbose);

            PrintMenu();

            fileName = Console.ReadLine().Trim();

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
                    listTests.TestEvent += new XmlTextEventHandler(runner.OnSimpleTest);

                    try
                    {
                        Console.WriteLine("Running...{0}", listTests.Name);

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

                fileName = Console.ReadLine().Trim();
            }
        }

        static void OnErrorEvent(object sender, XmlTestErrorEventArgs args)
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

        static void RunDefault()
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
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                XmlTestExceptionManager.ErrorEvent += new XmlTestErrorEventHandler(OnErrorEvent);
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
                                    XmlTestExceptionManager.ErrorEvent += new XmlTestErrorEventHandler(OnErrorEvent);
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
                        XmlTestExceptionManager.ErrorEvent += new XmlTestErrorEventHandler(OnErrorEvent);
                        RunInteractive(XmlTestType.None, true);
                    }
                }
            }

        }
    }

}
