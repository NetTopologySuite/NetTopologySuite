using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public class RectangleLineIntersectorTest
    {

        [TestAttribute]
        public void Test300Points()
        {
            var test = new RectangleLineIntersectorValidator();
            test.Init(300);
            Assert.IsTrue(test.Validate());
        }

        [TestAttribute]
        [CategoryAttribute("Stress")]
        public void TestPerformance()
        {
            var test = new RectangleLineIntersectorValidator();
            test.RunBoth(5);
            test.RunBoth(30);
            test.RunBoth(30);
            test.RunBoth(100);
            test.RunBoth(300);
            test.RunBoth(600);
            test.RunBoth(1000);
            test.RunBoth(6000);
        }
    }

    /// <summary>
    /// Tests optimized RectangleLineIntersector against a brute force approach (which is assumed to be correct).
    /// </summary>
    /// <author>Martin Davis</author>
    internal class RectangleLineIntersectorValidator
    {
        private readonly IGeometryFactory _geomFact = new GeometryFactory();

        private double baseX;
        private double baseY;
        private double rectSize = 100;
        private Envelope _rectEnv;
        private Coordinate[] _pts;
        private bool _isValid = true;

        public void Init(int nPts)
        {
            _rectEnv = CreateRectangle();
            _pts = CreateTestPoints(nPts);

        }

        public bool Validate()
        {
            RunCompare(true, true);
            return _isValid;
        }

        public void RunBoth(int nPts)
        {
            Init(nPts);
            Run(true, false);
            Run(false, true);
        }

        public void Run(bool useSegInt, bool useSideInt)
        {
            if (useSegInt) Console.WriteLine("Using Segment Intersector");
            if (useSideInt) Console.WriteLine("Using Side Intersector");
            
            Console.WriteLine("# pts: " + _pts.Length);

            var rectSegIntersector = new RectangleLineIntersector(_rectEnv);
            var rectSideIntersector = new SimpleRectangleIntersector(_rectEnv);

            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < _pts.Length; i++)
            {
                for (var j = 0; j < _pts.Length; j++)
                {
                    if (i == j) continue;

                    var segResult = false;
                    if (useSegInt)
                        segResult = rectSegIntersector.Intersects(_pts[i], _pts[j]);
                    var sideResult = false;
                    if (useSideInt)
                        sideResult = rectSideIntersector.Intersects(_pts[i], _pts[j]);

                    if (useSegInt && useSideInt)
                    {
                        if (segResult != sideResult)
                            throw new ApplicationException("Seg and Side values do not match");
                    }
                }
            }
            sw.Stop();
            Console.WriteLine("Finished in " + sw.Elapsed);
            Console.WriteLine();
        }

        private void RunCompare(bool useSegInt, bool useSideInt)
        {
            var rectSegIntersector = new RectangleLineIntersector(_rectEnv);
            var rectSideIntersector = new SimpleRectangleIntersector(_rectEnv);

            for (int i = 0; i < _pts.Length; i++)
            {
                for (int j = 0; j < _pts.Length; j++)
                {
                    if (i == j) continue;

                    var segResult = false;
                    if (useSegInt)
                        segResult = rectSegIntersector.Intersects(_pts[i], _pts[j]);
                    var sideResult = false;
                    if (useSideInt)
                        sideResult = rectSideIntersector.Intersects(_pts[i], _pts[j]);

                    if (useSegInt && useSideInt)
                    {
                        if (segResult != sideResult)
                            _isValid = false;
                    }
                }
            }
        }

        private Coordinate[] CreateTestPoints(int nPts)
        {
            var pt = _geomFact.CreatePoint(new Coordinate(baseX, baseY));
            var circle = pt.Buffer(2*rectSize, nPts/4);
            return circle.Coordinates;
        }

        private Envelope CreateRectangle()
        {
            var rectEnv = new Envelope(
                new Coordinate(baseX, baseY),
                new Coordinate(baseX + rectSize, baseY + rectSize));
            return rectEnv;
        }

    }

    /// <summary>
    /// Tests intersection of a segment against a rectangle by computing intersection against all side segments.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class SimpleRectangleIntersector
    {
        // for intersection testing, don't need to set precision model
        private readonly LineIntersector _li = new RobustLineIntersector();

        private readonly Envelope _rectEnv;
        /**
     * The corners of the rectangle, in the order:
     *  10
     *  23
     */
        private readonly Coordinate[] _corner = new Coordinate[4];

        public SimpleRectangleIntersector(Envelope rectEnv)
        {
            _rectEnv = rectEnv;
            InitCorners(rectEnv);
        }

        private void InitCorners(Envelope rectEnv)
        {
            _corner[0] = new Coordinate(rectEnv.MaxX, rectEnv.MaxY);
            _corner[1] = new Coordinate(rectEnv.MinX, rectEnv.MaxY);
            _corner[2] = new Coordinate(rectEnv.MinX, rectEnv.MinY);
            _corner[3] = new Coordinate(rectEnv.MaxX, rectEnv.MinY);
        }

        public bool Intersects(Coordinate p0, Coordinate p1)
        {
            var segEnv = new Envelope(p0, p1);
            if (!_rectEnv.Intersects(segEnv))
                return false;

            _li.ComputeIntersection(p0, p1, _corner[0], _corner[1]);
            if (_li.HasIntersection) return true;
            _li.ComputeIntersection(p0, p1, _corner[1], _corner[2]);
            if (_li.HasIntersection) return true;
            _li.ComputeIntersection(p0, p1, _corner[2], _corner[3]);
            if (_li.HasIntersection) return true;
            _li.ComputeIntersection(p0, p1, _corner[3], _corner[0]);
            if (_li.HasIntersection) return true;

            return false;
        }
    }
}