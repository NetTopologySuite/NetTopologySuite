using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite;
using Xunit;

namespace NetTopologySuite.Tests.Vivid.XUnit
{
    /// <summary>
    /// A class designed to allow debugging of individual tests from within the vivid set
    /// mainly to aid debugging v2 side by side with v1.7.x
    /// </summary>
    public abstract class XUnitRunner
    {
        private const String TestLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\vivid";

        private readonly XmlTestController controller = new XmlTestController();
        private XmlTestCollection _tests;

        protected XUnitRunner(string testFile)
        {
            TestFile = testFile;
        }

        private string TestFile { get; set; }

        protected XmlTestCollection Tests
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

        private XmlTestCollection LoadTests()
        {
            return controller.Load(Path.Combine(TestLocation, TestFile));
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

        private void ExecuteTest(int i)
        {
            if (i < Count)
                Tests[i].RunTest();
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
                    exceptions.Add(new ExceptionWrapper { Exception = ex, TestIndex = i });
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
                return "\r\n" + string.Format("{0} Child tests failed \r\n", _innerExceptions.Count) + String.Join("\r\n==========================================\r\n",
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