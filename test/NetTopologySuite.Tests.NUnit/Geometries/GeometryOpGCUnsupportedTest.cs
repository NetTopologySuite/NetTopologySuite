using System;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    public class GeometryOpGCUnsupportedTest : GeometryTestCase
    {

        private const string WKT_GC =
            "GEOMETRYCOLLECTION (POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200)), LINESTRING (150 250, 250 250))";

        private const string WKT_POLY = "POLYGON ((50 50, 50 150, 150 150, 150 50, 50 50))";

        [Test]
        public void TestBoundary()
        {
            var a = Read(WKT_GC);
            var b = Read(WKT_POLY);

            var fc = new FailureChecker(() =>
            {
                var bndry = a.Boundary;
            });
            fc.Check(typeof(ArgumentException));
        }

        [Test]
        public void TestRelate()
        {
            var a = Read(WKT_GC);
            var b = Read(WKT_POLY);

            var fc = new FailureChecker(() =>
            {
                var rlt = a.Relate(b);
            });
            fc.Check(typeof(ArgumentException));
            fc = new FailureChecker(() =>
            {
                var rlt = b.Relate(a);
            });
            fc.Check(typeof(ArgumentException));
        }

        [Test]
        public void TestUnion()
        {
            var a = Read(WKT_GC);
            var b = Read(WKT_POLY);

            var fc = new FailureChecker(() =>
            {
                var nn = a.Union(b);
            });
            fc.Check(typeof(ArgumentException));
            fc = new FailureChecker(() =>
            {
                var nn = b.Union(a);
            });
            fc.Check(typeof(ArgumentException));
        }

        [Test]
        public void TestDifference()
        {
            var a = Read(WKT_GC);
            var b = Read(WKT_POLY);

            var fc = new FailureChecker(() =>
            {
                var dff = a.Difference(b);
            });
            fc.Check(typeof(ArgumentException));
            fc = new FailureChecker(() =>
            {
                var dff = b.Difference(a);
            });
            fc.Check(typeof(ArgumentException));
        }

        [Test]
        public void TestSymDifference()
        {
            var a = Read(WKT_GC);
            var b = Read(WKT_POLY);

            var fc = new FailureChecker(() =>
            {
                var symdff = a.SymmetricDifference(b);
            });
            fc.Check(typeof(ArgumentException));
            fc = new FailureChecker(() =>
            {
                var symdff = b.SymmetricDifference(a);
            });
            fc.Check(typeof(ArgumentException));
        }



        class FailureChecker
        {
            /**
             * An operation which should throw an exception of the specified class
             */
            private readonly Action _operation;

            public FailureChecker(Action operation)
            {
                _operation = operation;
            }

            public void Check(Type exClz)
            {
                Assert.IsTrue(IsError(exClz));
            }

            bool IsError(Type exClz)
            {
                try
                {
                    _operation();
                    return false;
                }
                catch (Exception t)
                {
                    if (t.GetType() == exClz) return true;
                }

                return false;
            }
        }
    }
}

