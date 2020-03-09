#nullable disable
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Open.Topology.TestRunner;

namespace ConsoleTestRunner
{
    /// <summary>
    /// Summary description for ConsoleTest.
    /// </summary>
    class ConsoleTest
    {
        private static readonly string TestRunnerDirectory = GetTestRunnerTestDirectory();

        static void PrintMenu()
        {
            Console.WriteLine("\n\n**\n**\n** Interactive Test Instructions \n**\n**\n**\n");
            Console.WriteLine("a. Enter the name of the test script file to run.");
            Console.WriteLine("b. Enter (without the quote)");
            Console.WriteLine("   - 'default' to run the default tests.");
            Console.WriteLine("   - 'other' to run the other tests.");
            Console.WriteLine("   - 'all' to run default and other tests.");
            Console.WriteLine("c. Enter 'exit' (without the quote) to end the test.");
            Console.WriteLine();
            Console.Write("Test Runner>>");
        }

        static void RunInteractive(XmlTestType filter, bool verbose)
        {
            string fileName = string.Empty;

            var controller = new XmlTestController();

            var runner = new TestRunner(filter, verbose);

            PrintMenu();

            fileName = Console.ReadLine().Trim();

            while (fileName != "exit")
            {
                XmlTestCollection listTests = null;
                try
                {
                    switch (fileName)
                    {
                        case "default":
                            Run("Default.xml");
                            break;
                        case "other":
                            Run("Other.xml");
                            break;
                        case "all":
                            Run("Default.xml");
                            Run("Other.xml");
                            break;
                        default:
                            if (Directory.Exists(fileName))
                            {
                                string tmp = Path.GetTempFileName();
                                File.AppendAllText(tmp,
                                                   string.Format(
                                                       "<?xml version=\"1.0\" encoding=\"utf-8\" ?><project><test verbose=\"false\" exception=\"true\" interactive=\"false\" filter=\"none\"><dirs><dir>{0}</dir></dirs></test></project>",
                                                       fileName));
                                listTests = controller.Load(tmp);
                            }
                            else
                                listTests = controller.Load(fileName);

                            break;
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
            var ex = args.Thrown;

            if (ex != null)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine(ex.Source);
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }
        }

        static void Run(string fileName)
        {
            var parserOptions = new TestOptionsParser();
            var listTests =
                parserOptions.ParseProject(Path.Combine(TestRunnerDirectory, fileName));

            if (listTests != null && listTests.Count > 0)
            {
                var runner = new TestRunner(listTests);
                runner.Run();
                runner.PrintResult();
            }
        }

        private static string GetTestRunnerTestDirectory([CallerFilePath] string thisFilePath = null)
        {
            return new FileInfo(thisFilePath)                            // /src/NetTopologySuite.TestRunner.Console/ConsoleTest.cs
                .Directory                                               // /src/NetTopologySuite.TestRunner.Console
                .Parent                                                  // /src
                .Parent                                                  // /
                .GetDirectories("data")[0]                               // /data
                .GetDirectories("NetTopologySuite.TestRunner.Tests")[0]  // /data/NetTopologySuite.TestRunner.Tests
                .FullName;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // paths *to* the XML files, as well as paths *in* the XML files,
            // assume that we're running from the app's directory; apparently,
            // the VS2017 / new-style SDK changes did something to make that
            // no longer guaranteed to be the case at startup.
            Environment.CurrentDirectory = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;
            if (args == null || args.Length == 0)
            {
                XmlTestExceptionManager.ErrorEvent += new XmlTestErrorEventHandler(OnErrorEvent);
                RunInteractive(XmlTestType.None, true);
            }
            else
            {
                var parser = new TestOptionsParser();
                var collection = parser.Parse(args);

                if (parser.IsDefault)
                {
                    Run("Default.xml");
                }
                else
                {
                    if (collection != null)
                    {
                        if (collection.Count == 1)
                        {
                            // see if it is the interactive type
                            var info = collection[0];
                            if (info.Interactive)
                            {
                                if (info.Exception)
                                    XmlTestExceptionManager.ErrorEvent += new XmlTestErrorEventHandler(OnErrorEvent);
                                RunInteractive(info.Filter, info.Verbose);
                            }
                        }
                        else
                        {
                            var runner = new TestRunner(collection);
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
