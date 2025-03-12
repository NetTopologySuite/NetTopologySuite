using System;
using System.Diagnostics;
using System.Globalization;
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
        private readonly int _nIndex      = -1;
        private readonly bool _bSuccess;
        private readonly XmlTest _objTest;

        public XmlTestEventArgs(int index, bool success, XmlTest testItem)
        {
            _nIndex   = index;
            _bSuccess = success;
            _objTest  = testItem;
        }

        public int Index => _nIndex;

        public bool Success => _bSuccess;

        public XmlTest Test => _objTest;
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
                _nfi = NumberFormatInfo.InvariantInfo;
            }
            return _nfi;
        }

        #region Private Members

        private static int _nCount        = 1;

        private bool    _bIsDefaultTarget = true;

        private Exception _objException;

        private bool      _bSuccess;
        private object    _objResult;

        private Geometry  _objGeometryA;
        private Geometry  _objGeometryB;

        private object[]  _objArguments = new object[3];
        private object    _objArgument1;
        private object    _objArgument2;

        private XmlTestType _enumTestType = XmlTestType.None;

        private string    _strDescription;

        private readonly double    _dTolerance     = double.Epsilon;

        private IResultMatcher _resultMatcher;
        private readonly IGeometryOperation _geometryOperation;
        #endregion

        #region Constructors and Destructor

        public XmlTest(string description, bool bIsDefaultTarget, double tolerance, IGeometryOperation geometryOperation, IResultMatcher resultMatcher)
        {
            if (!string.IsNullOrEmpty(description))
            {
                _strDescription = description;
            }
            else
            {
                _strDescription = "Untitled" + _nCount.ToString();

                ++_nCount;
            }

            _bIsDefaultTarget = bIsDefaultTarget;
            _dTolerance       = tolerance;
            _geometryOperation = geometryOperation;
            _resultMatcher = resultMatcher;
        }

        #endregion

        #region Public Properties

        public string Description
        {
            get => _strDescription;

            set => _strDescription = value;
        }

        public Exception Thrown
        {
            get => _objException;

            set => _objException = value;
        }

        public bool Success => _bSuccess;

        public Geometry A
        {
            get => _objGeometryA;

            set => _objGeometryA = value;
        }

        public Geometry B
        {
            get => _objGeometryB;

            set => _objGeometryB = value;
        }

        public XmlTestType TestType
        {
            get => _enumTestType;

            set => _enumTestType = value;
        }

        public object Result
        {
            get => _objResult;

            set => _objResult = value;
        }

        public object Argument1
        {
            get => _objArgument1;

            set => _objArgument1 = value;
        }

        public object Argument2
        {
            get => _objArgument2;

            set => _objArgument2 = value;
        }

        public bool IsDefaultTarget
        {
            get => _bIsDefaultTarget;

            set => _bIsDefaultTarget = value;
        }

        #endregion

        #region Public Methods
        public bool Run()
        {
            var c = Console.ForegroundColor;
            try
            {
                _bSuccess = RunTest();
                if (!_bSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    // DEBUG ERRORS: retry to launch the test and analyze...
                    Console.WriteLine("\n*********************************************");

                    Console.WriteLine("Encountered failing test!");
                    Console.WriteLine("Description: '{0}'", Description);
                    Console.WriteLine("A: {0}", A?.AsText() ?? "null");
                    Console.WriteLine("B: {0}", B?.AsText() ?? "null");
                    Console.WriteLine("Test method: '{0}'", TestType);
                    Console.WriteLine("arg1 {0}", Argument1);
                    Console.WriteLine("arg2 {0}", Argument1);

                    Console.WriteLine("Result expected is {0}, but was {1}", true, _bSuccess);
                    Console.WriteLine("*********************************************\n");

                }
                return _bSuccess;
            }
            catch (Exception ex)
            {
                _objException = ex;
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                XmlTestExceptionManager.Publish(ex);
                return false;
            }
            finally
            {
                Console.ForegroundColor = c;
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
                    object[] arguments = ToArguments();

                IResult expectedResult = null;
                var returnType = _geometryOperation.GetReturnType(_enumTestType);
                if (returnType == typeof(int))
                    expectedResult = new IntegerResult((int)Result);
                else if (returnType == typeof(bool))
                    expectedResult = new BooleanResult((bool)Result);
                else if (returnType == typeof(double))
                    expectedResult = new DoubleResult((int)Result);
                else if (returnType == typeof(Geometry))
                    expectedResult = new GeometryResult((Geometry)Result);
                else
                {
                    Debug.Assert(false);
                }

                var result = _geometryOperation.Invoke(_enumTestType, IsDefaultTarget ? _objGeometryA : _objGeometryB, ToArguments());
                if (_resultMatcher == null)
                    _resultMatcher = CreateEqualityResultMatcher(returnType);
                                         {
                return _resultMatcher.IsMatch(IsDefaultTarget ? _objGeometryA : _objGeometryB, _enumTestType.ToString(),
                                           arguments, result, expectedResult, _dTolerance);
                }
            }

            switch (_enumTestType)
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
                    string format = string.Format("Test not implemented: {0}", this._enumTestType);
                    throw new NotImplementedException(format);
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test '{Description}' threw exception!");
                Console.WriteLine($"Test type: {TestType}");
                Console.WriteLine($"A: {A}");
                Console.WriteLine($"B: {B?.ToString() ?? new string('-',3)}");
                if (!(ReferenceEquals(Argument1, A) || ReferenceEquals(Argument1, B)) && Argument1 != null)
                    Console.WriteLine($"Argument1: {Argument1}");
                if (!(ReferenceEquals(Argument2, A) || ReferenceEquals(Argument2, B)) && Argument2 != null)
                    Console.WriteLine($"Argument2: {Argument2}");
                Console.WriteLine($"Tolerance: {_dTolerance}");
                Console.WriteLine($"Exception:\n{ex.Message}");
                Console.WriteLine($"Stack trace:\n{ex.StackTrace}");

                // Set the exception thrown
                Thrown = ex;
                //Console.WriteLine(ToString());
                //throw;
                return false;
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
            if (returnType == typeof(Geometry))
                return new EqualityResultMatcher<GeometryResult>();

            Debug.Assert(false);
            return null;
        }

        private object[] ToArguments()
        {
            var ret = new System.Collections.Generic.List<object>(2);
            object o = ToGeometryOrString(Argument1);
            if (o != null) ret.Add(o);
            o = ToGeometryOrString(Argument2);
            if (o != null) ret.Add(o);

            return ret.ToArray();
        }

        private object ToGeometryOrString(object o)
        {
            if (o == null)
                return null;

            if (o is Geometry)
                return o;

            if (o is string)
            {
                string a = (string) o;
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
            double dAreaResult = (double)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                double dArea = _objGeometryA.Area;

                return Math.Abs(dArea - dAreaResult) <= _dTolerance;
            }
            else if (_objGeometryB != null)
            {
                double dArea = _objGeometryB.Area;

                return Math.Abs(dArea - dAreaResult) <= _dTolerance;
            }

            return false;
        }

        protected virtual bool TestBoundary()
        {
            Trace.Assert(_objResult != null, "The result object cannot be null");

            var geoResult = (Geometry) _objResult;

            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var boundary = (Geometry) _objGeometryA.Boundary;
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
                        return AreEqual(boundary, geoResult);
                    }
                }
            }
            else if (_objGeometryB != null)
            {
                var boundary = (Geometry) _objGeometryB.Boundary;
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
                        return AreEqual(boundary, geoResult);
                    }
                }
            }

            return false;
        }

        protected virtual bool TestBoundaryDimension()
        {
            int nResult = (int)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                double dArea = _objGeometryA.Area;

                return Math.Abs(dArea - nResult) <= _dTolerance;
            }
            else if (_objGeometryB != null)
            {
                double dArea = _objGeometryB.Area;

                return Math.Abs(dArea - nResult) <= _dTolerance;
            }

            return false;
        }

        protected virtual bool TestBuffer()
        {
            var geoResult = (Geometry)_objResult;
            double dArg;
            if (_objArgument1 is Geometry)
                double.TryParse((string)_objArgument2, NumberStyles.Any, GetNumberFormatInfo(), out dArg);
            else
                double.TryParse((string)_objArgument1, NumberStyles.Any, GetNumberFormatInfo(), out dArg);

            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var buffer = (Geometry) _objGeometryA.Buffer(dArg);
                if (buffer != null)
                {
                    if (_resultMatcher is IResultMatcher<GeometryResult>)
                    {
                        var exp = new GeometryResult(geoResult);
                        var res = new GeometryResult(buffer);
                        return ((IResultMatcher<GeometryResult>)_resultMatcher).IsMatch(
                            _objGeometryA, "buffer", new[] { _objArgument1 }, res, exp, _dTolerance);

                    }

                    if (buffer.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }

                    if (geoResult.GetType().Name == "GeometryCollection")
                    {
                        return CompareGeometries(geoResult, buffer);
                    }

                    return AreEqual(buffer, geoResult);
                }
            }
            else if (_objGeometryB != null)
            {
                var buffer = (Geometry) _objGeometryB.Buffer(dArg);
                if (buffer != null)
                {
                    if (_resultMatcher is IResultMatcher<GeometryResult>)
                    {
                        var exp = new GeometryResult(geoResult);
                        var res = new GeometryResult(buffer);
                        return ((IResultMatcher<GeometryResult>)_resultMatcher).IsMatch(
                            _objGeometryB, "buffer", new[] { _objArgument1 }, res, exp, _dTolerance);

                    }

                    if (buffer.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }

                    if (geoResult.GetType().Name == "GeometryCollection")
                    {
                        return CompareGeometries(geoResult, buffer);
                    }

                    return AreEqual(buffer, geoResult);
                }
            }

            return false;
        }

        protected virtual bool TestBufferMitredJoin()
        {
            var geoResult = (Geometry)_objResult;
            double dArg;
            double.TryParse((string)_objArgument1, NumberStyles.Any, GetNumberFormatInfo(), out dArg);

            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var bp = new BufferParameters {JoinStyle = JoinStyle.Mitre};
                var buffer = (Geometry) _objGeometryA.Buffer(dArg, bp);
                if (buffer != null)
                {
                    if (_resultMatcher is IResultMatcher<GeometryResult>)
                    {
                        var exp = new GeometryResult(geoResult);
                        var res = new GeometryResult(buffer);
                        return ((IResultMatcher<GeometryResult>) _resultMatcher).IsMatch(
                            _objGeometryA, "buffer", new[] {_objArgument1}, res, exp, _dTolerance);

                    }

                    return AreEqual(buffer, geoResult);
                }
            }
            return false;
        }

        protected virtual bool TestCentroid()
        {
            Trace.Assert(_objResult != null, "The result object cannot be null");

            var geoResult = (Geometry)_objResult;

            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var centroid = (Geometry) _objGeometryA.Centroid;
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

                    return AreEqual(centroid, geoResult);
                }
            }
            else if (_objGeometryB != null)
            {
                var centroid = (Geometry) _objGeometryB.Centroid;
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

                    return AreEqual(centroid, geoResult);
                }
            }

            return false;
        }

        protected virtual bool TestContains()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.Contains(_objGeometryB) == bResult;
                }
                else
                {
                    return _objGeometryA.Contains((Geometry)_objArgument1) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.Contains(_objGeometryA) == bResult;
                }
                else
                {
                    return _objGeometryB.Contains((Geometry)_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestConvexHull()
        {
            Trace.Assert(_objResult != null, "The result object cannot be null");

            var geoResult = (Geometry)_objResult;

            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var convexhall = (Geometry) _objGeometryA.ConvexHull();
                if (convexhall != null)
                {
                    if (convexhall.IsEmpty && geoResult.IsEmpty)
                        return true;

                    bool bResult = CompareGeometries(geoResult, convexhall);
                    if (!bResult)
                    {
                        Console.WriteLine(_objGeometryA.ToString());
                        Console.WriteLine(convexhall.ToString());

                        Console.WriteLine(geoResult.ToString());
                    }

                    return bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                var convexhall = (Geometry) _objGeometryB.ConvexHull();
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
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.Crosses(_objGeometryB) == bResult;
                }
                else
                {
                    return _objGeometryA.Crosses((Geometry)_objArgument1) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.Crosses(_objGeometryA) == bResult;
                }
                else
                {
                    return _objGeometryB.Crosses((Geometry)_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestDensify()
        {
            var geoResult = _objResult as Geometry;

            double dArg = GetDoubleArgument();

            var geom = _bIsDefaultTarget && _objGeometryA != null ? _objGeometryA : _objGeometryB;

            if (geom != null)
            {
                var res = NetTopologySuite.Densify.Densifier.Densify(geom, dArg);
                return AreEqual(res, geoResult);
            }

            return false;
        }

        private double GetDoubleArgument()
        {
            if (_objArgument1 is Geometry)
                return double.Parse((string) _objArgument2, NumberStyles.Any, GetNumberFormatInfo());

            return double.Parse((string)_objArgument1, NumberStyles.Any, GetNumberFormatInfo());
        }

        protected virtual bool TestDifference()
        {
            Trace.Assert(_objResult != null, "The result object cannot be null");

            var geoResult = (Geometry)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    var difference = (Geometry) _objGeometryA.Difference(_objGeometryB);
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
                            return AreEqual(difference, geoResult);
                        }
                    }
                }
                else
                {
                    var difference = (Geometry) _objGeometryA.Difference((Geometry)_objArgument1);
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
                            return AreEqual(difference, geoResult);
                        }
                    }
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    var difference = (Geometry) _objGeometryB.Difference(_objGeometryA);
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
                            return AreEqual(difference, geoResult);
                        }
                    }
                }
                else
                {
                    var difference = (Geometry) _objGeometryB.Difference((Geometry)_objArgument1);
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
                            return AreEqual(difference, geoResult);
                        }
                    }
                }
            }

            return false;
        }

        protected virtual bool TestDimension()
        {
            int nResult = (int)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                int nDim = (int) _objGeometryA.Dimension;

                return Math.Abs(nDim - nResult) <= (int)_dTolerance;
            }
            else if (_objGeometryB != null)
            {
                int nDim = (int)_objGeometryB.Dimension;

                return Math.Abs(nDim - nResult) <= (int)_dTolerance;
            }

            return false;
        }

        protected virtual bool TestDisjoint()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.Disjoint(_objGeometryB) == bResult;
                }
                else
                {
                    return _objGeometryA.Disjoint((Geometry)_objArgument1) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.Disjoint(_objGeometryA) == bResult;
                }
                else
                {
                    return _objGeometryB.Disjoint((Geometry)_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestDistance()
        {
            double dResult = (double)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                double dDistance = 0;
                if (_objArgument1 == null)
                {
                    dDistance = _objGeometryA.Distance(_objGeometryB);
                }
                else
                {
                    dDistance = _objGeometryA.Distance((Geometry)_objArgument1);
                }

                return Math.Abs(dDistance - dResult) <= _dTolerance;
            }
            else if (_objGeometryB != null)
            {
                double dDistance = 0;
                if (_objArgument1 == null)
                {
                    dDistance = _objGeometryB.Distance(_objGeometryA);
                }
                else
                {
                    dDistance = _objGeometryB.Distance((Geometry)_objArgument1);
                }

                return Math.Abs(dDistance - dResult) <= _dTolerance;
            }

            return false;
        }

        protected virtual bool TestEnvelope()
        {
            Trace.Assert(_objResult != null, "The result object cannot be null");

            var geoResult = (Geometry)_objResult;

            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var envelope = (Geometry) _objGeometryA.Envelope;
                if (envelope != null)
                    return envelope.Equals(geoResult);
            }
            else if (_objGeometryB != null)
            {
                var envelope = (Geometry) _objGeometryB.Envelope;
                if (envelope != null)
                    return envelope.Equals(geoResult);
            }

            return false;
        }

        protected virtual bool TestEquals()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.Equals(_objGeometryB) == bResult;
                }
                else
                {
                    return _objGeometryA.Equals((Geometry)_objArgument1) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.Equals(_objGeometryA) == bResult;
                }
                else
                {
                    return _objGeometryB.Equals((Geometry)_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestInteriorPoint()
        {
            Trace.Assert(_objResult != null, "The result object cannot be null");

            var geoResult = (Geometry) _objResult;

            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var interiorpoint = (Geometry) _objGeometryA.InteriorPoint;
                if (interiorpoint != null)
                {
                    if (interiorpoint.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }

                    return interiorpoint.Equals(geoResult);
                }
            }
            else if (_objGeometryB != null)
            {
                var interiorpoint = (Geometry) _objGeometryB.InteriorPoint;
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
            Trace.Assert(_objResult != null, "The result object cannot be null");

            var geoResult = (Geometry)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    var intersection = (Geometry) _objGeometryA.Intersection(_objGeometryB);
                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                            return true;

                        if (geoResult.GetType().Name == "GeometryCollection")
                             return CompareGeometries(geoResult, intersection);
                        else
                            return AreEqual(intersection, geoResult);
                    }
                }
                else
                {
                    var intersection = (Geometry) _objGeometryA.Intersection((Geometry) _objArgument1);
                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                            return true;

                        if (geoResult.GetType().Name == "GeometryCollection")
                             return CompareGeometries(geoResult, intersection);
                        else
                            return AreEqual(intersection, geoResult);
                    }
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    var intersection = (Geometry) _objGeometryB.Intersection(_objGeometryA);
                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                            return true;

                        if (geoResult.GetType().Name == "GeometryCollection")
                             return CompareGeometries(geoResult, intersection);
                        else
                            return AreEqual(intersection, geoResult);
                    }
                }
                else
                {
                    var intersection = (Geometry) _objGeometryB.Intersection((Geometry)_objArgument1);
                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                            return true;

                        if (geoResult.GetType().Name == "GeometryCollection")
                             return CompareGeometries(geoResult, intersection);
                        else
                            return AreEqual(intersection, geoResult);
                    }
                }
            }

            return false;
        }

        protected virtual bool TestIntersects()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.Intersects(_objGeometryB) == bResult;
                }
                else
                {
                    return _objGeometryA.Intersects((Geometry)_objArgument1) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.Intersects(_objGeometryA) == bResult;
                }
                else
                {
                    return _objGeometryB.Intersects((Geometry)_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestIsEmpty()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                bool bState = _objGeometryA.IsEmpty;

                return bState == bResult;
            }
            else if (_objGeometryB != null)
            {
                bool bState = _objGeometryB.IsEmpty;

                return bState == bResult;
            }

            return false;
        }

        protected virtual bool TestIsSimple()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                bool bState = _objGeometryA.IsSimple;

                return bState == bResult;
            }
            else if (_objGeometryB != null)
            {
                bool bState = _objGeometryB.IsSimple;

                return bState == bResult;
            }

            return false;
        }

        protected virtual bool TestIsValid()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                bool bState = _objGeometryA.IsValid;
                return bState == bResult;
            }
            else if (_objGeometryB != null)
            {
                bool bState = _objGeometryB.IsValid;

                return bState == bResult;
            }

            return false;
        }

        protected virtual bool TestIsWithinDistance()
        {
            bool bResult = (bool)_objResult;
            double dArg = double.Parse((string)_objArgument2, GetNumberFormatInfo());

            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.IsWithinDistance(_objGeometryB, dArg) == bResult;
                }
                else
                {
                    return _objGeometryA.IsWithinDistance((Geometry)_objArgument1,
                        dArg) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.IsWithinDistance(_objGeometryA, dArg) == bResult;
                }
                else
                {
                    return _objGeometryB.IsWithinDistance((Geometry)_objArgument1,
                        dArg) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestLength()
        {
            double dLengthResult = (double)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                double dLength = _objGeometryA.Area;

                return Math.Abs(dLength - dLengthResult) <= _dTolerance;
            }
            else if (_objGeometryB != null)
            {
                double dLength = _objGeometryB.Area;

                return Math.Abs(dLength - dLengthResult) <= _dTolerance;
            }

            return false;
        }

        protected virtual bool TestNumPoints()
        {
            int nResult = (int)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                int nPoints = _objGeometryA.NumPoints;

                return Math.Abs(nPoints - nResult) <= (int)_dTolerance;
            }
            else if (_objGeometryB != null)
            {
                int nPoints = _objGeometryB.NumPoints;

                return Math.Abs(nPoints - nResult) <= (int)_dTolerance;
            }

            return false;
        }

        protected virtual bool TestOverlaps()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.Overlaps(_objGeometryB) == bResult;
                }
                else
                {
                    return _objGeometryA.Overlaps((Geometry)_objArgument1) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.Overlaps(_objGeometryA) == bResult;
                }
                else
                {
                    return _objGeometryB.Overlaps((Geometry)_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestRelate()
        {
            bool bResult = (bool)_objResult;
            string arg   = (string)_objArgument2;

            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var matrix = _objGeometryA.Relate(_objGeometryB);

                string strMatrix = matrix.ToString();

                return (strMatrix == arg) == bResult;
            }
            else if (_objGeometryB != null)
            {
                var matrix = _objGeometryB.Relate(_objGeometryA);

                string strMatrix = matrix.ToString();

                return (strMatrix == arg) == bResult;
            }

            return false;
        }

        protected virtual bool TestSRID()
        {
            int nResult = (int)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                int nSRID = _objGeometryA.SRID;

                return Math.Abs(nSRID - nResult) <= (int)_dTolerance;
            }
            else if (_objGeometryB != null)
            {
                int nSRID = _objGeometryB.SRID;

                return Math.Abs(nSRID - nResult) <= (int)_dTolerance;
            }

            return false;
        }

        protected virtual bool TestSymDifference()
        {
            Trace.Assert(_objResult != null, "The result object cannot be null");

            var geoResult = (Geometry)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    var difference = (Geometry) _objGeometryA.SymmetricDifference(_objGeometryB);
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
                            return AreEqual(difference, geoResult);
                        }
                    }
                }
                else
                {
                    var difference = (Geometry) _objGeometryA.SymmetricDifference((Geometry)_objArgument1);
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
                            return AreEqual(difference, geoResult);
                        }
                    }
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    var difference = (Geometry) _objGeometryB.SymmetricDifference(_objGeometryA);
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
                            return AreEqual(difference, geoResult);
                        }
                    }
                }
                else
                {
                    var difference = (Geometry) _objGeometryB.SymmetricDifference((Geometry)_objArgument1);
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
                            return AreEqual(difference, geoResult);
                        }
                    }
                }
            }

            return false;
        }

        protected virtual bool TestTouches()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.Touches(_objGeometryB) == bResult;
                }
                else
                {
                    return _objGeometryA.Touches((Geometry)_objArgument1) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.Touches(_objGeometryA) == bResult;
                }
                else
                {
                    return _objGeometryB.Touches((Geometry)_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestUnion()
        {
            Trace.Assert(_objResult != null, "The result object cannot be null");

            var geoResult = (Geometry)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    var union = (Geometry)_objGeometryA.Union();

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
                            return AreEqual(union, geoResult);
                        }
                    }
                }
                else
                {
                    var union = (Geometry) _objGeometryA.Union((Geometry)_objArgument1);
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
                            return AreEqual(union, geoResult);
                        }
                    }
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    var union = (Geometry) _objGeometryB.Union(_objGeometryA);
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
                            return AreEqual(union, geoResult);
                        }
                    }
                }
                else
                {
                    var union = (Geometry) _objGeometryB.Union((Geometry)_objArgument1);
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
                            return AreEqual(union, geoResult);
                        }
                    }
                }
            }

            return false;
        }

        protected virtual bool TestWithin()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.Within(_objGeometryB) == bResult;
                }
                else
                {
                    return _objGeometryA.Within((Geometry)_objArgument1) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.Within(_objGeometryA) == bResult;
                }
                else
                {
                    return _objGeometryB.Within((Geometry)_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestCovers()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                     return _objGeometryA.Covers(_objGeometryB) == bResult;
                else return _objGeometryA.Covers((Geometry)_objArgument1) == bResult;
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                     return _objGeometryB.Covers(_objGeometryA) == bResult;
                else return _objGeometryB.Covers((Geometry)_objArgument1) == bResult;
            }

            return false;
        }

        protected virtual bool TestCoveredBy()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                     return _objGeometryA.CoveredBy(_objGeometryB) == bResult;
                else return _objGeometryA.CoveredBy((Geometry)_objArgument1) == bResult;
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                     return _objGeometryB.CoveredBy(_objGeometryA) == bResult;
                else return _objGeometryB.CoveredBy((Geometry)_objArgument1) == bResult;
            }

            return false;
        }

        protected virtual bool TestEqualsExact()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.EqualsExact(_objGeometryB) == bResult;
                }
                else
                {
                    return _objGeometryA.EqualsExact((Geometry)_objArgument1) == bResult;
                }
            }
            else if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryB.EqualsExact(_objGeometryA) == bResult;
                }
                else
                {
                    return _objGeometryB.EqualsExact((Geometry)_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestEqualsNorm()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                {
                    return _objGeometryA.EqualsNormalized(_objGeometryB);
                }
                var g = (Geometry)_objArgument1;
                return _objGeometryA.EqualsNormalized(g) == bResult;
            }
            if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                    return _objGeometryB.EqualsNormalized(_objGeometryA) == bResult;
                var g = (Geometry)_objArgument1;
                return _objGeometryB.EqualsNormalized(g) == bResult;
            }

            return false;
        }

        protected virtual bool TestMinClearance()
        {
            double dResult = (double)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var c = new MinimumClearance(_objGeometryA);
                double dClearance = c.GetDistance();
                return Math.Abs(dClearance - dResult) <= _dTolerance;
            }
            return false;
        }

        protected virtual bool TestMinClearanceLine()
        {
            var gResult = (Geometry)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                var c = new MinimumClearance(_objGeometryA);
                Geometry gClearance = c.GetLine();
                return gResult.EqualsNormalized(gClearance);
            }
            return false;
        }

        protected virtual bool TestEqualsTopo()
        {
            bool bResult = (bool)_objResult;
            if (_bIsDefaultTarget && _objGeometryA != null)
            {
                if (_objArgument1 == null)
                    return _objGeometryA.EqualsTopologically(_objGeometryB) == bResult;
                var g = (Geometry)_objArgument1;
                return _objGeometryA.EqualsTopologically(g) == bResult;
            }
            if (_objGeometryB != null)
            {
                if (_objArgument1 == null)
                    return _objGeometryB.EqualsTopologically(_objGeometryA) == bResult;
                var g = (Geometry)_objArgument1;
                return _objGeometryB.EqualsTopologically(g) == bResult;
            }

            return false;
        }

        #endregion

        #region Private Members

        private bool CompareGeometries(Geometry a, Geometry b)
        {
            if (a != null && b != null && a.GetType().Name == b.GetType().Name)
            {
                var aClone = (Geometry)a.Copy();
                var bClone = (Geometry)b.Copy();

                aClone.Normalize();
                bClone.Normalize();

                return aClone.EqualsExact(bClone, _dTolerance);
            }

            return false;
        }

        private static bool AreEqual(Geometry g0, Geometry g1)
        {
            if (g0.EqualsExact(g1))
                return true;
            return g0.EqualsTopologically(g1);
        }

        #endregion
    }
}
