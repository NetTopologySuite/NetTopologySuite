using System;
using System.Diagnostics;
using System.Globalization;
using GeoAPI.Geometries;
using GeoAPI.Operation.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using Open.Topology.TestRunner.Operations;
using Open.Topology.TestRunner.Result;
namespace Open.Topology.TestRunner
{
    using NetTopologySuite.Precision;
    #region Test Event Definitions
    public class XmlTestEventArgs : EventArgs
    {
        public XmlTestEventArgs(int index, bool success, XmlTest testItem)
        {
            Index   = index;
            Success = success;
            Test  = testItem;
        }
        public int Index { get; } = -1;
        public bool Success { get; }
        public XmlTest Test { get; }
    }
    public delegate void XmlTextEventHandler(object sender, XmlTestEventArgs args);
    #endregion
    #region XmlTestType Enumeration
    public enum XmlTestType
    {
        None                    = 0,
        Area                    = 1,
        Boundary                = 2,
        BoundaryDimension       = 3,
        Buffer                  = 4,
        Centroid                = 5,
        Contains                = 6,
        ConvexHull              = 7,
        Crosses                 = 8,
        Difference              = 9,
        Dimension               = 10,
        Disjoint                = 11,
        Distance                = 12,
        Envelope                = 13,
        Equals                  = 14,
        InteriorPoint           = 15,
        Intersection            = 16,
        Intersects              = 17,
        IsEmpty                 = 18,
        IsSimple                = 19,
        IsValid                 = 20,
        IsWithinDistance        = 21,
        Length                  = 22,
        NumPoints               = 23,
        Overlaps                = 24,
        Relate                  = 25,
        SRID                    = 26,
        SymmetricDifference     = 27,
        Touches                 = 28,
        Union                   = 29,
        Within                  = 30,
        Covers                  = 31,
        CoveredBy               = 32,
        BufferMitredJoin        = 33,
        Densify                 = 34,
        EqualsExact             = 35,
        EqualsNorm              = 36,
        MinClearance            = 37,
        MinClearanceLine        = 38,
        EqualsTopo              = 39,
    }
    #endregion
	/// <summary>
	/// Summary description for XmlTest.
	/// </summary>
	public class XmlTest
	{
        private static NumberFormatInfo _nfi;
        protected static IFormatProvider GetNumberFormatInfo()
        {
            if (_nfi == null)
            {
                _nfi = new NumberFormatInfo {NumberDecimalSeparator = "."};
            }
            return _nfi;
        }
        #region Private Members
        private static int _nCount        = 1;
	    private object[]  _objArguments = new object[3];
	    private readonly double    _dTolerance     = Double.Epsilon;
	    private IResultMatcher _resultMatcher;
	    private readonly IGeometryOperation _geometryOperation;
        #endregion
        #region Constructors and Destructor
        public XmlTest(string description, bool bIsDefaultTarget, double tolerance, IGeometryOperation geometryOperation, IResultMatcher resultMatcher)
		{
            if (!string.IsNullOrEmpty(description))
            {
                Description = description;
            }
            else
            {
                Description = "Untitled" + _nCount.ToString();
                ++_nCount;
            }
            IsDefaultTarget = bIsDefaultTarget;
            _dTolerance       = tolerance;
            _geometryOperation = geometryOperation;
            _resultMatcher = resultMatcher;
		}
        #endregion
        #region Public Properties
        public string Description { get; set; }
	    public Exception Thrown { get; set; }
	    public bool Success { get; private set; }
	    public IGeometry A { get; set; }
	    public IGeometry B { get; set; }
	    public XmlTestType TestType { get; set; } = XmlTestType.None;
	    public object Result { get; set; }
	    public object Argument1 { get; set; }
	    public object Argument2 { get; set; }
	    public bool IsDefaultTarget { get; set; } = true;
	    #endregion
        #region Public Methods
        public bool Run()
        {
            try
            {
                Success = this.RunTest();
                if (!Success)
                {
                    // DEBUG ERRORS: retry to launch the test and analyze...
                    Console.WriteLine();
                    Console.WriteLine("Retry failed test: {0}", Description);
                    Console.WriteLine(Argument1);
                    Console.WriteLine(Argument2);
                    Console.WriteLine(A);
                    Console.WriteLine(B);
                    Console.WriteLine("Test type: " + TestType);
                    Success = RunTest();
                    Console.WriteLine("Result expected is {0}, but was {1}", true, Success);
                    Console.WriteLine();
                }
                return Success;
            }
            catch (Exception ex)
            {
                Thrown = ex;
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                XmlTestExceptionManager.Publish(ex);
                return false;
            }
        }
        #endregion
        #region Protected Methods
	    public virtual bool RunTest()
        {
            try
            {
                if (_geometryOperation != null)
            {
                var arguments = ToArguments();
                IResult expectedResult = null;
                var returnType = _geometryOperation.GetReturnType(TestType);
                if (returnType == typeof(int))
                    expectedResult = new IntegerResult((int)Result);
                else if (returnType == typeof(bool))
                    expectedResult = new BooleanResult((bool)Result);
                else if (returnType == typeof(double))
                    expectedResult = new DoubleResult((int)Result);
                else if (returnType == typeof(IGeometry))
                    expectedResult = new GeometryResult((IGeometry)Result);
                else
                {
                    Debug.Assert(false);
                }
                var result = _geometryOperation.Invoke(TestType, IsDefaultTarget ? A : B, ToArguments());
                if (_resultMatcher == null)
                    _resultMatcher = CreateEqualityResultMatcher(returnType);
                                         {
                return _resultMatcher.IsMatch(IsDefaultTarget ? A : B, TestType.ToString(),
                                           arguments, result, expectedResult, _dTolerance);
                }
            }
            switch (TestType)
            {
                case XmlTestType.Area:
                    return TestArea();
                case XmlTestType.Boundary:
                    return TestBoundary();
                case XmlTestType.BoundaryDimension:
                    return TestBoundaryDimension();
                case XmlTestType.Buffer:
                    return TestBuffer();
                case XmlTestType.BufferMitredJoin:
                    return TestBufferMitredJoin();
                case XmlTestType.Centroid:
                    return TestCentroid();
                case XmlTestType.Contains:
                    return TestContains();
                case XmlTestType.ConvexHull:
                    return TestConvexHull();
                case XmlTestType.Crosses:
                    return TestCrosses();
                case XmlTestType.Densify:
                    return TestDensify();
                case XmlTestType.Difference:
                    return TestDifference();
                case XmlTestType.Dimension:
                    return TestDimension();
                case XmlTestType.Disjoint:
                    return TestDisjoint();
                case XmlTestType.Distance:
                    return TestDistance();
                case XmlTestType.Envelope:
                    return TestEnvelope();
                case XmlTestType.Equals:
                    return TestEquals();
                case XmlTestType.InteriorPoint:
                    return TestInteriorPoint();
                case XmlTestType.Intersection:
                    return TestIntersection();
                case XmlTestType.Intersects:
                    return TestIntersects();
                case XmlTestType.IsEmpty:
                    return TestIsEmpty();
                case XmlTestType.IsSimple:
                    return TestIsSimple();
                case XmlTestType.IsValid:
                    return TestIsValid();
                case XmlTestType.IsWithinDistance:
                    return TestIsWithinDistance();
                case XmlTestType.Length:
                    return TestLength();
                case XmlTestType.NumPoints:
                    return TestNumPoints();
                case XmlTestType.Overlaps:
                    return TestOverlaps();
                case XmlTestType.Relate:
                    return TestRelate();
                case XmlTestType.SRID:
                    return TestSRID();
                case XmlTestType.SymmetricDifference:
                    return TestSymDifference();
                case XmlTestType.Touches:
                    return TestTouches();
                case XmlTestType.Union:
                    return TestUnion();
                case XmlTestType.Within:
                    return TestWithin();
                case XmlTestType.Covers:
                    return TestCovers();
                case XmlTestType.CoveredBy:
                    return TestCoveredBy();
                case XmlTestType.EqualsExact:
                    return TestEqualsExact();
                case XmlTestType.EqualsNorm:
                    return TestEqualsNorm();
                case XmlTestType.MinClearance:
                    return TestMinClearance();
                case XmlTestType.MinClearanceLine:
                    return TestMinClearanceLine();
                case XmlTestType.EqualsTopo:
                    return TestEqualsTopo();
                default:
                    string format = String.Format("Test not implemented: {0}", this.TestType);
                    throw new NotImplementedException(format);
            }
            }
            catch (Exception)
            {
                Console.WriteLine(ToString());
                throw;
            }
        }
        private IResultMatcher CreateEqualityResultMatcher(Type returnType)
	    {
            if (returnType == typeof(int))
                return new EqualityResultMatcher<IntegerResult>();
            if (returnType == typeof(bool))
                return new EqualityResultMatcher<BooleanResult>();
            if (returnType == typeof(double))
                return new EqualityResultMatcher<DoubleResult>();
            if (returnType == typeof(IGeometry))
                return new EqualityResultMatcher<GeometryResult>();
            Debug.Assert(false);
	        return null;
	    }
	    private object[] ToArguments()
	    {
            var ret = new System.Collections.Generic.List<object>(2);
	        var o = ToGeometryOrString(Argument1);
            if (o != null) ret.Add(o);
            o = ToGeometryOrString(Argument2);
            if (o != null) ret.Add(o);
            return ret.ToArray();
	    }
        private object ToGeometryOrString(object o)
        {
            if (o == null)
                return null;
            if (o is IGeometry)
                return o;
            if (o is string)
            {
                var a = (string) o;
                if (a == "A" || a == "a")
                    return A;
                if (a == "B" || a == "b")
                    return B;
                return a;
            }
            return o.ToString();
        }
	    protected virtual bool TestArea()
        {
            double dAreaResult = (double)Result;
            if (IsDefaultTarget && A != null)
            {
                double dArea = A.Area;
                return Math.Abs(dArea - dAreaResult) <= _dTolerance;
            }
            else if (B != null)
            {
                double dArea = B.Area;
                return Math.Abs(dArea - dAreaResult) <= _dTolerance;
            }
            return false;
        }
        protected virtual bool TestBoundary()
        {
            Trace.Assert(Result != null, "The result object cannot be null");
            Geometry geoResult = (Geometry) Result;
            if (IsDefaultTarget && A != null)
            {
                Geometry boundary = (Geometry) A.Boundary;
                if (boundary != null)
                {
                    if (boundary.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }
                    if (geoResult.GetType().Name == "GeometryCollection")
                    {
                        return CompareGeometries(geoResult, boundary);
                    }
                    else
                    {
                        return boundary.Equals(geoResult);
                    }
                }
            }
            else if (B != null)
            {
                Geometry boundary = (Geometry) B.Boundary;
                if (boundary != null)
                {
                    if (boundary.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }
                    if (geoResult.GetType().Name == "GeometryCollection")
                    {
                        return CompareGeometries(geoResult, boundary);
                    }
                    else
                    {
                        return boundary.Equals(geoResult);
                    }
               }
            }
            return false;
        }
        protected virtual bool TestBoundaryDimension()
        {
            int nResult = (int)Result;
            if (IsDefaultTarget && A != null)
            {
                double dArea = A.Area;
                return Math.Abs(dArea - nResult) <= _dTolerance;
            }
            else if (B != null)
            {
                double dArea = B.Area;
                return Math.Abs(dArea - nResult) <= _dTolerance;
            }
            return false;
        }
        protected virtual bool TestBuffer()
        {
            Geometry geoResult = (Geometry)Result;
            double dArg;
            if (Argument1 is IGeometry)
                Double.TryParse((string)Argument2, NumberStyles.Any, GetNumberFormatInfo(), out dArg);
            else
                Double.TryParse((string)Argument1, NumberStyles.Any, GetNumberFormatInfo(), out dArg);
            if (IsDefaultTarget && A != null)
            {
                Geometry buffer = (Geometry) A.Buffer(dArg);
                if (buffer != null)
                {
                    if (_resultMatcher is IResultMatcher<GeometryResult>)
                    {
                        var exp = new GeometryResult(geoResult);
                        var res = new GeometryResult(buffer);
                        return ((IResultMatcher<GeometryResult>)_resultMatcher).IsMatch(
                            A, "buffer", new[] { Argument1 }, res, exp, _dTolerance);
                    }
                    if (buffer.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }
                    if (geoResult.GetType().Name == "GeometryCollection")
                    {
                        return CompareGeometries(geoResult, buffer);
                    }
                    return buffer.Equals(geoResult);
                }
            }
            else if (B != null)
            {
                Geometry buffer = (Geometry) B.Buffer(dArg);
                if (buffer != null)
                {
                    if (_resultMatcher is IResultMatcher<GeometryResult>)
                    {
                        var exp = new GeometryResult(geoResult);
                        var res = new GeometryResult(buffer);
                        return ((IResultMatcher<GeometryResult>)_resultMatcher).IsMatch(
                            B, "buffer", new[] { Argument1 }, res, exp, _dTolerance);
                    }
                    if (buffer.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }
                    if (geoResult.GetType().Name == "GeometryCollection")
                    {
                        return CompareGeometries(geoResult, buffer);
                    }
                    return buffer.Equals(geoResult);
                }
            }
            return false;
        }
        protected virtual bool TestBufferMitredJoin()
        {
            Geometry geoResult = (Geometry)Result;
            double dArg;
            Double.TryParse((string)Argument1, NumberStyles.Any, GetNumberFormatInfo(), out dArg);
            if (IsDefaultTarget && A != null)
            {
                var bp = new BufferParameters {JoinStyle = JoinStyle.Mitre};
                Geometry buffer = (Geometry) A.Buffer(dArg, bp);
                if (buffer != null)
                {
                    if (_resultMatcher is IResultMatcher<GeometryResult>)
                    {
                        var exp = new GeometryResult(geoResult);
                        var res = new GeometryResult(buffer);
                        return ((IResultMatcher<GeometryResult>) _resultMatcher).IsMatch(
                            A, "buffer", new[] {Argument1}, res, exp, _dTolerance);
                    }
                    return buffer.Equals(geoResult);
                }
            }
            return false;
        }
        protected virtual bool TestCentroid()
        {
            Trace.Assert(Result != null, "The result object cannot be null");
            Geometry geoResult = (Geometry)Result;
            if (IsDefaultTarget && A != null)
            {
                Geometry centroid = (Geometry) A.Centroid;
                if (centroid != null)
                {
                    if (centroid.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }
                    if (geoResult.GetType().Name == "GeometryCollection")
                    {
                        return CompareGeometries(geoResult, centroid);
                    }
                    return centroid.Equals(geoResult);
                }
            }
            else if (B != null)
            {
                Geometry centroid = (Geometry) B.Centroid;
                if (centroid != null)
                {
                    if (centroid.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }
                    if (geoResult.GetType().Name == "GeometryCollection")
                    {
                        return CompareGeometries(geoResult, centroid);
                    }
                    return centroid.Equals(geoResult);
                }
            }
            return false;
        }
        protected virtual bool TestContains()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.Contains(B) == bResult;
                }
                else
                {
                    return A.Contains((Geometry)Argument1) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.Contains(A) == bResult;
                }
                else
                {
                    return B.Contains((Geometry)Argument1) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestConvexHull()
        {
            Trace.Assert(Result != null, "The result object cannot be null");
            Geometry geoResult = (Geometry)Result;
            if (IsDefaultTarget && A != null)
            {
                Geometry convexhall = (Geometry) A.ConvexHull();
                if (convexhall != null)
                {
                    if (convexhall.IsEmpty && geoResult.IsEmpty)
                        return true;
                    bool bResult = CompareGeometries(geoResult, convexhall);
                    if (!bResult)
                    {
                        Console.WriteLine(A.ToString());
                        Console.WriteLine(convexhall.ToString());
                        Console.WriteLine(geoResult.ToString());
                    }
                    return bResult;
                }
            }
            else if (B != null)
            {
                Geometry convexhall = (Geometry) B.ConvexHull();
                if (convexhall != null)
                {
                    if (convexhall.IsEmpty && geoResult.IsEmpty)
                        return true;
                    return CompareGeometries(geoResult, convexhall);
                }
            }
            return false;
        }
        protected virtual bool TestCrosses()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.Crosses(B) == bResult;
                }
                else
                {
                    return A.Crosses((Geometry)Argument1) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.Crosses(A) == bResult;
                }
                else
                {
                    return B.Crosses((Geometry)Argument1) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestDensify()
        {
            var geoResult = Result as IGeometry;
            var dArg = GetDoubleArgument();
            var geom = IsDefaultTarget && A != null ? A : B;
            if (geom != null)
            {
                var res = NetTopologySuite.Densify.Densifier.Densify(geom, dArg);
                return res.Equals(geoResult);
            }
            return false;
        }
	    private double GetDoubleArgument()
	    {
            if (Argument1 is IGeometry)
                return Double.Parse((string) Argument2, NumberStyles.Any, GetNumberFormatInfo());
            return Double.Parse((string)Argument1, NumberStyles.Any, GetNumberFormatInfo());
        }
	    protected virtual bool TestDifference()
        {
            Trace.Assert(Result != null, "The result object cannot be null");
            Geometry geoResult = (Geometry)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    Geometry difference = (Geometry) A.Difference(B);
                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, difference);
                        }
                        else
                        {
                            return difference.Equals(geoResult);
                        }
                    }
                }
                else
                {
                    Geometry difference = (Geometry) A.Difference((Geometry)Argument1);
                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, difference);
                        }
                        else
                        {
                            return difference.Equals(geoResult);
                        }
                    }
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    Geometry difference = (Geometry) B.Difference(A);
                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, difference);
                        }
                        else
                        {
                            return difference.Equals(geoResult);
                        }
                    }
                }
                else
                {
                    Geometry difference = (Geometry) B.Difference((Geometry)Argument1);
                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, difference);
                        }
                        else
                        {
                            return difference.Equals(geoResult);
                        }
                    }
                }
            }
            return false;
        }
        protected virtual bool TestDimension()
        {
            int nResult = (int)Result;
            if (IsDefaultTarget && A != null)
            {
                int nDim = (int) A.Dimension;
                return Math.Abs(nDim - nResult) <= (int)_dTolerance;
            }
            else if (B != null)
            {
                int nDim = (int)B.Dimension;
                return Math.Abs(nDim - nResult) <= (int)_dTolerance;
            }
            return false;
        }
        protected virtual bool TestDisjoint()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.Disjoint(B) == bResult;
                }
                else
                {
                    return A.Disjoint((Geometry)Argument1) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.Disjoint(A) == bResult;
                }
                else
                {
                    return B.Disjoint((Geometry)Argument1) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestDistance()
        {
            double dResult = (double)Result;
            if (IsDefaultTarget && A != null)
            {
                double dDistance = 0;
                if (Argument1 == null)
                {
                    dDistance = A.Distance(B);
                }
                else
                {
                    dDistance = A.Distance((Geometry)Argument1);
                }
                return Math.Abs(dDistance - dResult) <= _dTolerance;
            }
            else if (B != null)
            {
                double dDistance = 0;
                if (Argument1 == null)
                {
                    dDistance = B.Distance(A);
                }
                else
                {
                    dDistance = B.Distance((Geometry)Argument1);
                }
                return Math.Abs(dDistance - dResult) <= _dTolerance;
            }
            return false;
        }
        protected virtual bool TestEnvelope()
        {
            Trace.Assert(Result != null, "The result object cannot be null");
            Geometry geoResult = (Geometry)Result;
            if (IsDefaultTarget && A != null)
            {
                Geometry envelope = (Geometry) A.Envelope;
                if (envelope != null)
                    return envelope.Equals(geoResult);
            }
            else if (B != null)
            {
                Geometry envelope = (Geometry) B.Envelope;
                if (envelope != null)
                    return envelope.Equals(geoResult);
            }
            return false;
        }
        protected virtual bool TestEquals()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.Equals(B) == bResult;
                }
                else
                {
                    return A.Equals((Geometry)Argument1) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.Equals(A) == bResult;
                }
                else
                {
                    return B.Equals((Geometry)Argument1) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestInteriorPoint()
        {
            Trace.Assert(Result != null, "The result object cannot be null");
            Geometry geoResult = (Geometry) Result;
            if (IsDefaultTarget && A != null)
            {
                Geometry interiorpoint = (Geometry) A.InteriorPoint;
                if (interiorpoint != null)
                {
                    if (interiorpoint.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }
                    return interiorpoint.Equals(geoResult);
                }
            }
            else if (B != null)
            {
                Geometry interiorpoint = (Geometry) B.InteriorPoint;
                if (interiorpoint != null)
                {
                    if (interiorpoint.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }
                    return interiorpoint.Equals(geoResult);
                }
            }
            return false;
        }
        protected virtual bool TestIntersection()
        {
            Trace.Assert(Result != null, "The result object cannot be null");
            Geometry geoResult = (Geometry)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    Geometry intersection = (Geometry) A.Intersection(B);
                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                            return true;
                        if (geoResult.GetType().Name == "GeometryCollection")
                             return CompareGeometries(geoResult, intersection);
                        else return intersection.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry intersection = (Geometry) A.Intersection((Geometry) Argument1);
                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                            return true;
                        if (geoResult.GetType().Name == "GeometryCollection")
                             return CompareGeometries(geoResult, intersection);
                        else return intersection.Equals(geoResult);
                    }
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    Geometry intersection = (Geometry) B.Intersection(A);
                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                            return true;
                        if (geoResult.GetType().Name == "GeometryCollection")
                             return CompareGeometries(geoResult, intersection);
                        else return intersection.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry intersection = (Geometry) B.Intersection((Geometry)Argument1);
                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                            return true;
                        if (geoResult.GetType().Name == "GeometryCollection")
                             return CompareGeometries(geoResult, intersection);
                        else return intersection.Equals(geoResult);
                    }
                }
            }
            return false;
        }
        protected virtual bool TestIntersects()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.Intersects(B) == bResult;
                }
                else
                {
                    return A.Intersects((Geometry)Argument1) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.Intersects(A) == bResult;
                }
                else
                {
                    return B.Intersects((Geometry)Argument1) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestIsEmpty()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                bool bState = A.IsEmpty;
                return bState == bResult;
            }
            else if (B != null)
            {
                bool bState = B.IsEmpty;
                return bState == bResult;
            }
            return false;
        }
        protected virtual bool TestIsSimple()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                bool bState = A.IsSimple;
                return bState == bResult;
            }
            else if (B != null)
            {
                bool bState = B.IsSimple;
                return bState == bResult;
            }
            return false;
        }
        protected virtual bool TestIsValid()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                bool bState = A.IsValid;
                return bState == bResult;
            }
            else if (B != null)
            {
                bool bState = B.IsValid;
                return bState == bResult;
            }
            return false;
        }
        protected virtual bool TestIsWithinDistance()
        {
            bool bResult = (bool)Result;
            double dArg = Double.Parse((string)Argument2, GetNumberFormatInfo());
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.IsWithinDistance(B, dArg) == bResult;
                }
                else
                {
                    return A.IsWithinDistance((Geometry)Argument1,
                        dArg) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.IsWithinDistance(A, dArg) == bResult;
                }
                else
                {
                    return B.IsWithinDistance((Geometry)Argument1,
                        dArg) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestLength()
        {
            double dLengthResult = (double)Result;
            if (IsDefaultTarget && A != null)
            {
                double dLength = A.Area;
                return Math.Abs(dLength - dLengthResult) <= _dTolerance;
            }
            else if (B != null)
            {
                double dLength = B.Area;
                return Math.Abs(dLength - dLengthResult) <= _dTolerance;
            }
            return false;
        }
        protected virtual bool TestNumPoints()
        {
            int nResult = (int)Result;
            if (IsDefaultTarget && A != null)
            {
                int nPoints = A.NumPoints;
                return Math.Abs(nPoints - nResult) <= (int)_dTolerance;
            }
            else if (B != null)
            {
                int nPoints = B.NumPoints;
                return Math.Abs(nPoints - nResult) <= (int)_dTolerance;
            }
            return false;
        }
        protected virtual bool TestOverlaps()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.Overlaps(B) == bResult;
                }
                else
                {
                    return A.Overlaps((Geometry)Argument1) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.Overlaps(A) == bResult;
                }
                else
                {
                    return B.Overlaps((Geometry)Argument1) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestRelate()
        {
            bool bResult = (bool)Result;
            string arg   = (string)Argument2;
            if (IsDefaultTarget && A != null)
            {
                IntersectionMatrix matrix = A.Relate(B);
                string strMatrix = matrix.ToString();
                return (strMatrix == arg) == bResult;
            }
            else if (B != null)
            {
                IntersectionMatrix matrix = B.Relate(A);
                string strMatrix = matrix.ToString();
                return (strMatrix == arg) == bResult;
            }
            return false;
        }
        protected virtual bool TestSRID()
        {
            int nResult = (int)Result;
            if (IsDefaultTarget && A != null)
            {
                int nSRID = A.SRID;
                return Math.Abs(nSRID - nResult) <= (int)_dTolerance;
            }
            else if (B != null)
            {
                int nSRID = B.SRID;
                return Math.Abs(nSRID - nResult) <= (int)_dTolerance;
            }
            return false;
        }
        protected virtual bool TestSymDifference()
        {
            Trace.Assert(Result != null, "The result object cannot be null");
            Geometry geoResult = (Geometry)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    Geometry difference = (Geometry) A.SymmetricDifference(B);
                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, difference);
                        }
                        else
                        {
                            return difference.Equals(geoResult);
                        }
                    }
                }
                else
                {
                    Geometry difference = (Geometry) A.SymmetricDifference((Geometry)Argument1);
                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, difference);
                        }
                        else
                        {
                            return difference.Equals(geoResult);
                        }
                    }
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    Geometry difference = (Geometry) B.SymmetricDifference(A);
                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, difference);
                        }
                        else
                        {
                            return difference.Equals(geoResult);
                        }
                    }
                }
                else
                {
                    Geometry difference = (Geometry) B.SymmetricDifference((Geometry)Argument1);
                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, difference);
                        }
                        else
                        {
                            return difference.Equals(geoResult);
                        }
                    }
                }
            }
            return false;
        }
        protected virtual bool TestTouches()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.Touches(B) == bResult;
                }
                else
                {
                    return A.Touches((Geometry)Argument1) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.Touches(A) == bResult;
                }
                else
                {
                    return B.Touches((Geometry)Argument1) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestUnion()
        {
            Trace.Assert(Result != null, "The result object cannot be null");
            Geometry geoResult = (Geometry)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    Geometry union = (Geometry)A.Union();
                    if (union != null)
                    {
                        if (union.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, union);
                        }
                        else
                        {
                            return union.Equals(geoResult);
                        }
                    }
                }
                else
                {
                    Geometry union = (Geometry) A.Union((Geometry)Argument1);
                    if (union != null)
                    {
                        if (union.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, union);
                        }
                        else
                        {
                            return union.Equals(geoResult);
                        }
                    }
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    Geometry union = (Geometry) B.Union(A);
                    if (union != null)
                    {
                        if (union.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, union);
                        }
                        else
                        {
                            return union.Equals(geoResult);
                        }
                    }
                }
                else
                {
                    Geometry union = (Geometry) B.Union((Geometry)Argument1);
                    if (union != null)
                    {
                        if (union.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }
                        if (geoResult.GetType().Name == "GeometryCollection")
                        {
                            return CompareGeometries(geoResult, union);
                        }
                        else
                        {
                            return union.Equals(geoResult);
                        }
                    }
                }
            }
            return false;
        }
        protected virtual bool TestWithin()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.Within(B) == bResult;
                }
                else
                {
                    return A.Within((Geometry)Argument1) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.Within(A) == bResult;
                }
                else
                {
                    return B.Within((Geometry)Argument1) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestCovers()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                     return A.Covers(B) == bResult;
                else return A.Covers((Geometry)Argument1) == bResult;
            }
            else if (B != null)
            {
                if (Argument1 == null)
                     return B.Covers(A) == bResult;
                else return B.Covers((Geometry)Argument1) == bResult;
            }
            return false;
        }
        protected virtual bool TestCoveredBy()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                     return A.CoveredBy(B) == bResult;
                else return A.CoveredBy((Geometry)Argument1) == bResult;
            }
            else if (B != null)
            {
                if (Argument1 == null)
                     return B.CoveredBy(A) == bResult;
                else return B.CoveredBy((Geometry)Argument1) == bResult;
            }
            return false;
        }
        protected virtual bool TestEqualsExact()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.EqualsExact(B) == bResult;
                }
                else
                {
                    return A.EqualsExact((Geometry)Argument1) == bResult;
                }
            }
            else if (B != null)
            {
                if (Argument1 == null)
                {
                    return B.EqualsExact(A) == bResult;
                }
                else
                {
                    return B.EqualsExact((Geometry)Argument1) == bResult;
                }
            }
            return false;
        }
        protected virtual bool TestEqualsNorm()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                {
                    return A.EqualsNormalized(B);
                }
                var g = (IGeometry)Argument1;
                return A.EqualsNormalized(g) == bResult;
            }
            if (B != null)
            {
                if (Argument1 == null)
                    return B.EqualsNormalized(A) == bResult;
                var g = (Geometry)Argument1;
                return B.EqualsNormalized(g) == bResult;
            }
            return false;
        }
	    protected virtual bool TestMinClearance()
        {
            double dResult = (double)Result;
            if (IsDefaultTarget && A != null)
            {
                MinimumClearance c = new MinimumClearance(A);
                double dClearance = c.GetDistance();
                return Math.Abs(dClearance - dResult) <= _dTolerance;
            }
            return false;
        }
        protected virtual bool TestMinClearanceLine()
        {
            IGeometry gResult = (IGeometry)Result;
            if (IsDefaultTarget && A != null)
            {
                MinimumClearance c = new MinimumClearance(A);
                IGeometry gClearance = c.GetLine();
                return gResult.EqualsNormalized(gClearance);
            }
            return false;
        }
        protected virtual bool TestEqualsTopo()
        {
            bool bResult = (bool)Result;
            if (IsDefaultTarget && A != null)
            {
                if (Argument1 == null)
                    return A.EqualsTopologically(B) == bResult;
                var g = (Geometry)Argument1;
                return A.EqualsTopologically(g) == bResult;
            }
            if (B != null)
            {
                if (Argument1 == null)
                    return B.EqualsTopologically(A) == bResult;
                var g = (Geometry)Argument1;
                return B.EqualsTopologically(g) == bResult;
            }
            return false;
        }
        #endregion
        #region Private Members
        private bool CompareGeometries(Geometry a, Geometry b)
        {
            if (a != null && b != null && a.GetType().Name == b.GetType().Name)
            {
                Geometry aClone = (Geometry)a.Copy();
                Geometry bClone = (Geometry)b.Copy();
                aClone.Normalize();
                bClone.Normalize();
                return aClone.EqualsExact(bClone, _dTolerance);
            }
            return false;
        }
        #endregion
	}
}
