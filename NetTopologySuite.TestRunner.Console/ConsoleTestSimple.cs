using System;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates.Simple;
using SysConsole = System.Console;

namespace NetTopologySuite.Console
{
    internal partial class ConsoleTest
    {
        #region nested simple test

        private class Simple
        {
            private static ICoordinateFactory<Coordinate> CreateSimpleCoordinateFactory(PrecisionModelType type, Double scale)
            {
                if (Double.IsNaN(scale))
                    return new CoordinateFactory(type);
                return new CoordinateFactory(scale);
            }

            private static ICoordinateSequenceFactory<Coordinate> CreateSimpleCoordinateSequenceFactory(ICoordinateFactory<Coordinate> coordinateFactory)
            {
                return new CoordinateSequenceFactory((CoordinateFactory)coordinateFactory);
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
                                TestRunner<Coordinate> runner = new TestRunner<Coordinate>(collection);
                                runner.Run(CreateSimpleCoordinateFactory, CreateSimpleCoordinateSequenceFactory);
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
                XmlTestController<Coordinate> controller = new XmlTestController<Coordinate>();

                TestRunner<Coordinate> runner = new TestRunner<Coordinate>(filter, verbose);

                PrintMenu();

                String fileName = (SysConsole.ReadLine() ?? String.Empty).Trim();

                while (fileName != "exit")
                {
                    XmlTestCollection<Coordinate> listTests = null;
                    try
                    {
                        if (fileName == "default")
                        {
                            RunDefault();
                        }
                        else
                        {
                            listTests = controller.Load(fileName, CreateSimpleCoordinateFactory, CreateSimpleCoordinateSequenceFactory);
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
                    TestRunner<Coordinate> runner = new TestRunner<Coordinate>(listTests);
                    runner.Run(CreateSimpleCoordinateFactory, CreateSimpleCoordinateSequenceFactory);
                    runner.PrintResult();
                }
            }

        }
        #endregion nested bufferd test
    }
}
