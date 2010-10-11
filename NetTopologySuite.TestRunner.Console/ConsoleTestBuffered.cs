using System;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using SysConsole = System.Console;

namespace NetTopologySuite.Console
{
    internal partial class ConsoleTest
    {
        #region nested buffered test

        private class Buffered
        {
            private static ICoordinateFactory<BufferedCoordinate> CreateManagedCoordinateFactory(PrecisionModelType type, Double scale)
            {
                if (Double.IsNaN(scale))
                    return new BufferedCoordinateFactory(type);
                return new BufferedCoordinateFactory(scale);
            }

            private static ICoordinateSequenceFactory<BufferedCoordinate> CreateManagedCoordinateSequenceFactory(ICoordinateFactory<BufferedCoordinate> coordinateFactory)
            {
                return new BufferedCoordinateSequenceFactory((BufferedCoordinateFactory)coordinateFactory);
            }

            public static void Start(String[] args)
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
                                TestRunner<BufferedCoordinate> runner = new TestRunner<BufferedCoordinate>(collection);
                                runner.Run(CreateManagedCoordinateFactory, CreateManagedCoordinateSequenceFactory);
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

            private static void RunInteractive(XmlTestType filter, Boolean verbose)
            {
                XmlTestController<BufferedCoordinate> controller = new XmlTestController<BufferedCoordinate>();

                TestRunner<BufferedCoordinate> runner = new TestRunner<BufferedCoordinate>(filter, verbose);

                PrintMenu();

                String fileName = (SysConsole.ReadLine() ?? String.Empty).Trim();

                while (fileName != "exit")
                {
                    XmlTestCollection<BufferedCoordinate> listTests = null;
                    try
                    {
                        if (fileName == "default")
                        {
                            RunDefault();
                        }
                        else
                        {
                            listTests = controller.Load(fileName, CreateManagedCoordinateFactory, CreateManagedCoordinateSequenceFactory);
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
                    TestRunner<BufferedCoordinate> runner = new TestRunner<BufferedCoordinate>(listTests);
                    runner.Run(CreateManagedCoordinateFactory, CreateManagedCoordinateSequenceFactory);
                    runner.PrintResult();
                }
            }

        }
        #endregion nested bufferd test
    }
}
