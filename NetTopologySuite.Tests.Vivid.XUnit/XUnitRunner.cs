using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite;
using Xunit;

#if BUFFERED
using Coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using CoordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using CoordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#else
using Coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using CoordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using CoordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;
#endif

namespace NetTopologySuite.Tests.Vivid.XUnit
{
    /// <summary>
    /// A class designed to allow debugging of individual tests from within the vivid set
    /// mainly to aid debugging v2 side by side with v1.7.x
    /// </summary>
    public abstract class XUnitRunner
    {
        static ICoordinateFactory<Coord> CreateCoordinateFactory(PrecisionModelType precisionModel, Double scale)
        {
            if (Double.IsNaN(scale))
                return new CoordFac(precisionModel);
            return new CoordFac(scale);
        }
        
        static ICoordinateSequenceFactory<Coord> CreateCoordinateSequenceFactory(ICoordinateFactory<Coord> coordinateFactory)
        {
            return new CoordSeqFac((CoordFac) coordinateFactory);
        }

        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\vivid";

        private readonly XmlTestController<Coord> _controller = new XmlTestController<Coord>();
        private XmlTestCollection<Coord> _tests;

        protected XUnitRunner(string testFile)
        {
            TestFile = testFile;
        }

        private string TestFile { get; set; }

        protected XmlTestCollection<Coord> Tests
        {
            get
            {
                _tests = _tests ?? LoadTests();
                return _tests;
            }
        }

        public Int32 Count
        {
            get { return Tests.Count; }
        }

        private XmlTestCollection<Coord> LoadTests()
        {
            XmlTestCollection<Coord> tests = _controller.Load( Path.Combine(TestLocation, TestFile), CreateCoordinateFactory, CreateCoordinateSequenceFactory);
            tests.TestEvent += tests_TestEvent;
            return tests;
        }

        private void tests_TestEvent(object sender, XmlTestEventArgs<Coord> args)
        {
            Assert.True(args.Success);
        }

        [Fact]
        public void TestCountOk()
        {
            if (Count > 20)
                TestAll();
        }

        [Fact]
        public void Test0()
        {
            ExecuteTest(0);
        }


        [Fact]
        public void Test1()
        {
            ExecuteTest(1);
        }

        [Fact]
        public void Test2()
        {
            ExecuteTest(2);
        }

        [Fact]
        public void Test3()
        {
            ExecuteTest(3);
        }

        [Fact]
        public void Test4()
        {
            ExecuteTest(4);
        }

        [Fact]
        public void Test5()
        {
            ExecuteTest(5);
        }

        [Fact]
        public void Test6()
        {
            ExecuteTest(6);
        }

        [Fact]
        public void Test7()
        {
            ExecuteTest(7);
        }

        [Fact]
        public void Test8()
        {
            ExecuteTest(8);
        }

        [Fact]
        public void Test9()
        {
            ExecuteTest(9);
        }

        [Fact]
        public void Test10()
        {
            ExecuteTest(10);
        }

        [Fact]
        public void Test11()
        {
            ExecuteTest(11);
        }

        [Fact]
        public void Test12()
        {
            ExecuteTest(12);
        }

        [Fact]
        public void Test13()
        {
            ExecuteTest(13);
        }

        [Fact]
        public void Test14()
        {
            ExecuteTest(14);
        }

        [Fact]
        public void Test15()
        {
            ExecuteTest(15);
        }

        [Fact]
        public void Test16()
        {
            ExecuteTest(16);
        }

        [Fact]
        public void Test17()
        {
            ExecuteTest(17);
        }

        [Fact]
        public void Test18()
        {
            ExecuteTest(18);
        }

        [Fact]
        public void Test19()
        {
            ExecuteTest(19);
        }

        [Fact]
        public void Test20()
        {
            ExecuteTest(20);
        }

        [Fact]
        public void ManualTest()
        {
            int id = GetTestId();
            {
                if (id > -1)
                    ExecuteTest(id);
            }
        }

        private int GetTestId()
        {
            using (ArbitaryTestIdForm frm = new ArbitaryTestIdForm())
            {
                frm.ShowDialog();
                return frm.TestId;
            }
        }

        private void ExecuteTest(int i)
        {
            if (i < Count)
            {
                Console.WriteLine(string.Format("Executing test {0}", i));
                Tests.RunTest(i);
            }
        }

        //some test files contain hundreds of tests..

        public void TestAll()
        {
            List<ExceptionWrapper> exceptions = new List<ExceptionWrapper>();


            for (int i = 0; i < Count; i++)
            {
                try
                {
                    ExecuteTest(i);
                }
                catch (Exception ex)
                {
                    exceptions.Add(new ExceptionWrapper {Exception = ex, TestIndex = i});
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }

    public class AggregateException : Exception
    {
        private readonly IList<ExceptionWrapper> _innerExceptions;

        internal AggregateException(IList<ExceptionWrapper> exceptions)
        {
            _innerExceptions = exceptions;
        }

        public override string Message
        {
            get
            {
                return "\r\n" + string.Format("{0} Child tests failed \r\n", _innerExceptions.Count) +
                       String.Join("\r\n==========================================\r\n",
                                   Enumerable.ToArray(Processor.Select(_innerExceptions,
                                                                       delegate(
                                                                           ExceptionWrapper
                                                                           o)
                                                                           {
                                                                               return
                                                                                   string.Format(
                                                                                       "Test Index : {0}\r\n{1}\r\n{2}",
                                                                                       o.TestIndex, o.Exception.Message,
                                                                                       o.Exception.StackTrace);
                                                                           })));
            }
        }
    }

    internal struct ExceptionWrapper
    {
        public Exception Exception { get; set; }
        public Int32 TestIndex { get; set; }
    }
}