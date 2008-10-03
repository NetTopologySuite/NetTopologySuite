#region Namespace Imports

using System;
using System.Diagnostics;
using System.Globalization;
using GisSharpBlog.NetTopologySuite.Geometries;
using GeoAPI.Geometries;
using NetTopologySuite.Coordinates;
using GeoAPI.Coordinates;

#endregion

namespace GisSharpBlog.NetTopologySuite
{
    public class XmlTestEventArgs : EventArgs
    {
        private readonly Int32 m_nIndex = -1;
        private readonly Boolean m_bSuccess;
        private readonly XmlTest m_objTest;

        public XmlTestEventArgs(Int32 index, Boolean success, XmlTest testItem)
        {
            m_nIndex = index;
            m_bSuccess = success;
            m_objTest = testItem;
        }

        public Int32 Index
        {
            get { return m_nIndex; }
        }

        public Boolean Success
        {
            get { return m_bSuccess; }
        }

        public XmlTest Test
        {
            get { return m_objTest; }
        }
    }

    public delegate void XmlTextEventHandler(object sender, XmlTestEventArgs args);

    public enum XmlTestType
    {
        None = 0,
        Area = 1,
        Boundary = 2,
        BoundaryDimension = 3,
        Buffer = 4,
        Centroid = 5,
        Contains = 6,
        ConvexHull = 7,
        Crosses = 8,
        Difference = 9,
        Dimension = 10,
        Disjoint = 11,
        Distance = 12,
        Envelope = 13,
        Equals = 14,
        InteriorPoint = 15,
        Intersection = 16,
        Intersects = 17,
        IsEmpty = 18,
        IsSimple = 19,
        IsValid = 20,
        IsWithinDistance = 21,
        Length = 22,
        NumPoints = 23,
        Overlaps = 24,
        Relate = 25,
        SRID = 26,
        SymmetricDifference = 27,
        Touches = 28,
        Union = 29,
        Within = 30,
        Covers = 31,
        CoveredBy = 32,
    }

    /// <summary>
    /// Summary description for XmlTest.
    /// </summary>
    public class XmlTest
    {
        private static NumberFormatInfo nfi;

        protected static IFormatProvider GetNumberFormatInfo()
        {
            if (nfi == null)
            {
                nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
            }
            return nfi;
        }

        #region Private Members

        private static Int32 m_nCount = 1;

        private Boolean m_bIsDefaultTarget = true;

        private Exception m_objException;

        private Boolean m_bSuccess;
        private object m_objResult;

        private IGeometry m_objGeometryA;
        private IGeometry m_objGeometryB;

        private object m_objArgument1;
        private object m_objArgument2;

        private XmlTestType m_enumTestType = XmlTestType.None;

        private String m_strDescription;

        private readonly Double m_dTolerance = Double.Epsilon;

        #endregion

        #region Constructors and Destructor

        public XmlTest(String description, Boolean bIsDefaultTarget, Double tolerance)
        {
            if (description != null && description.Length != 0)
            {
                m_strDescription = description;
            }
            else
            {
                m_strDescription = "Untitled" + m_nCount;

                ++m_nCount;
            }

            m_bIsDefaultTarget = bIsDefaultTarget;
            m_dTolerance = tolerance;
        }

        #endregion

        #region Public Properties

        public String Description
        {
            get { return m_strDescription; }

            set { m_strDescription = value; }
        }

        public Exception Thrown
        {
            get { return m_objException; }

            set { m_objException = value; }
        }

        public Boolean Success
        {
            get { return m_bSuccess; }
        }

        public IGeometry A
        {
            get { return m_objGeometryA; }

            set { m_objGeometryA = value; }
        }

        public IGeometry B
        {
            get { return m_objGeometryB; }

            set { m_objGeometryB = value; }
        }

        public XmlTestType TestType
        {
            get { return m_enumTestType; }

            set { m_enumTestType = value; }
        }

        public object Result
        {
            get { return m_objResult; }

            set { m_objResult = value; }
        }

        public object Argument1
        {
            get { return m_objArgument1; }

            set { m_objArgument1 = value; }
        }

        public object Argument2
        {
            get { return m_objArgument2; }

            set { m_objArgument2 = value; }
        }

        public Boolean IsDefaultTarget
        {
            get { return m_bIsDefaultTarget; }

            set { m_bIsDefaultTarget = value; }
        }

        #endregion

        #region Public Methods

        public Boolean Run()
        {
            try
            {
                m_bSuccess = RunTest();
                if (!m_bSuccess)
                {
                    // DEBUG ERRORS: retry to launch the test and analyze...                                       
                    Console.WriteLine();
                    Console.WriteLine("Retry failed test: " + Description);
                    Console.WriteLine(Argument1);
                    Console.WriteLine(Argument2);
                    Console.WriteLine(A);
                    Console.WriteLine(B);
                    Console.WriteLine("Test type: " + TestType);
                    m_bSuccess = RunTest();
                    Console.WriteLine(String.Format("Result expected is {0}, but was {1}",
                                                    true,
                                                    m_bSuccess));
                    Console.WriteLine();
                }
                return m_bSuccess;
            }
            catch (Exception ex)
            {
                m_objException = ex;
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                XmlTestExceptionManager.Publish(ex);
                return false;
            }
        }

        #endregion

        #region Protected Methods

        protected virtual Boolean RunTest()
        {
            switch (m_enumTestType)
            {
                case XmlTestType.Area:
                    return TestArea();

                case XmlTestType.Boundary:
                    return TestBoundary();

                case XmlTestType.BoundaryDimension:
                    return TestBoundaryDimension();

                case XmlTestType.Buffer:
                    return TestBuffer();

                case XmlTestType.Centroid:
                    return TestCentroid();

                case XmlTestType.Contains:
                    return TestContains();

                case XmlTestType.ConvexHull:
                    return TestConvexHull();

                case XmlTestType.Crosses:
                    return TestCrosses();

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

                default:
                    break;
            }

            return false;
        }

        protected virtual Boolean TestArea()
        {
            Double dAreaResult = (Double) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Double dArea = (m_objGeometryA as ISurface).Area;

                return Math.Abs(dArea - dAreaResult) <= m_dTolerance;
            }

            if (m_objGeometryB != null)
            {
                Double dArea = (m_objGeometryB as ISurface).Area;

                return Math.Abs(dArea - dAreaResult) <= m_dTolerance;
            }

            return false;
        }

        protected virtual Boolean TestBoundary()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry<BufferedCoordinate> boundary =
                    (Geometry<BufferedCoordinate>) m_objGeometryA.Boundary;

                if (boundary != null)
                {
                    return boundary.IsEmpty && geoResult.IsEmpty ||
                           (geoResult.GetType().Name == "GeometryCollection"
                                ? CompareGeometries(geoResult, boundary)
                                : boundary.Equals(geoResult));
                }
            }
            else if (m_objGeometryB != null)
            {
                Geometry<BufferedCoordinate> boundary =
                    (Geometry<BufferedCoordinate>) m_objGeometryB.Boundary;

                if (boundary != null)
                {
                    return boundary.IsEmpty && geoResult.IsEmpty ||
                           (geoResult.GetType().Name == "GeometryCollection"
                                ? CompareGeometries(geoResult, boundary)
                                : boundary.Equals(geoResult));
                }
            }

            return false;
        }

        protected virtual Boolean TestBoundaryDimension()
        {
            Int32 nResult = (Int32) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Double dArea = (m_objGeometryA as ISurface).Area;

                return Math.Abs(dArea - nResult) <= m_dTolerance;
            }

            if (m_objGeometryB != null)
            {
                Double dArea = (m_objGeometryB as ISurface).Area;

                return Math.Abs(dArea - nResult) <= m_dTolerance;
            }

            return false;
        }

        protected virtual Boolean TestBuffer()
        {
            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;
            Double dArg = Double.Parse((String) m_objArgument2, GetNumberFormatInfo());

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry<BufferedCoordinate> buffer =
                    (Geometry<BufferedCoordinate>) m_objGeometryA.Buffer(dArg);

                if (buffer != null)
                {
                    return buffer.IsEmpty && geoResult.IsEmpty ||
                           (geoResult.GetType().Name == "GeometryCollection"
                                ? CompareGeometries(geoResult, buffer)
                                : buffer.Equals(geoResult));
                }
            }
            else if (m_objGeometryB != null)
            {
                Geometry<BufferedCoordinate> buffer =
                    (Geometry<BufferedCoordinate>) m_objGeometryB.Buffer(dArg);

                if (buffer != null)
                {
                    return buffer.IsEmpty && geoResult.IsEmpty ||
                           (geoResult.GetType().Name == "GeometryCollection"
                                ? CompareGeometries(geoResult, buffer)
                                : buffer.Equals(geoResult));
                }
            }

            return false;
        }

        protected virtual Boolean TestCentroid()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry<BufferedCoordinate> centroid =
                    (Geometry<BufferedCoordinate>) m_objGeometryA.Centroid;

                if (centroid != null)
                {
                    if (centroid.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }

                    return geoResult.GetType().Name == "GeometryCollection"
                               ? CompareGeometries(geoResult, centroid)
                               : centroid.Equals(geoResult);
                }
            }
            else if (m_objGeometryB != null)
            {
                Geometry<BufferedCoordinate> centroid =
                    (Geometry<BufferedCoordinate>) m_objGeometryB.Centroid;
                if (centroid != null)
                {
                    if (centroid.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }

                    return geoResult.GetType().Name == "GeometryCollection"
                               ? CompareGeometries(geoResult, centroid)
                               : centroid.Equals(geoResult);
                }
            }

            return false;
        }

        protected virtual Boolean TestContains()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.Contains(m_objGeometryB) == bResult
                           : m_objGeometryA.Contains((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.Contains(m_objGeometryA) == bResult
                           : m_objGeometryB.Contains((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            return false;
        }

        protected virtual Boolean TestConvexHull()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry<BufferedCoordinate> convexhall =
                    (Geometry<BufferedCoordinate>) m_objGeometryA.ConvexHull();

                if (convexhall != null)
                {
                    if (convexhall.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }

                    Boolean bResult = CompareGeometries(geoResult, convexhall);

                    if (!bResult)
                    {
                        Console.WriteLine(m_objGeometryA.ToString());
                        Console.WriteLine(convexhall.ToString());

                        Console.WriteLine(geoResult.ToString());
                    }

                    return bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                Geometry<BufferedCoordinate> convexhall =
                    (Geometry<BufferedCoordinate>) m_objGeometryB.ConvexHull();

                if (convexhall != null)
                {
                    if (convexhall.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }

                    return CompareGeometries(geoResult, convexhall);
                }
            }

            return false;
        }

        protected virtual Boolean TestCrosses()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.Crosses(m_objGeometryB) == bResult
                           : m_objGeometryA.Crosses((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.Crosses(m_objGeometryA) == bResult
                           : m_objGeometryB.Crosses((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            return false;
        }

        protected virtual Boolean TestDifference()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry<BufferedCoordinate> difference =
                        (Geometry<BufferedCoordinate>) m_objGeometryA.Difference(m_objGeometryB);

                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, difference)
                                   : difference.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry<BufferedCoordinate> difference =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryA.Difference((Geometry<BufferedCoordinate>) m_objArgument1);

                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, difference)
                                   : difference.Equals(geoResult);
                    }
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry<BufferedCoordinate> difference =
                        (Geometry<BufferedCoordinate>) m_objGeometryB.Difference(m_objGeometryA);

                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, difference)
                                   : difference.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry<BufferedCoordinate> difference =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryB.Difference((Geometry<BufferedCoordinate>) m_objArgument1);

                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, difference)
                                   : difference.Equals(geoResult);
                    }
                }
            }

            return false;
        }

        protected virtual Boolean TestDimension()
        {
            Int32 nResult = (Int32) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Int32 nDim = (Int32) m_objGeometryA.Dimension;

                return Math.Abs(nDim - nResult) <= (Int32) m_dTolerance;
            }

            if (m_objGeometryB != null)
            {
                Int32 nDim = (Int32) m_objGeometryB.Dimension;

                return Math.Abs(nDim - nResult) <= (Int32) m_dTolerance;
            }

            return false;
        }

        protected virtual Boolean TestDisjoint()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.Disjoint(m_objGeometryB) == bResult
                           : m_objGeometryA.Disjoint((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.Disjoint(m_objGeometryA) == bResult
                           : m_objGeometryB.Disjoint((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            return false;
        }

        protected virtual Boolean TestDistance()
        {
            Double dResult = (Double) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Double dDistance = 0;

                dDistance = m_objArgument1 == null
                                ? m_objGeometryA.Distance(m_objGeometryB)
                                : m_objGeometryA.Distance(
                                      (Geometry<BufferedCoordinate>) m_objArgument1);

                return Math.Abs(dDistance - dResult) <= m_dTolerance;
            }

            if (m_objGeometryB != null)
            {
                Double dDistance = 0;

                dDistance = m_objArgument1 == null
                                ? m_objGeometryB.Distance(m_objGeometryA)
                                : m_objGeometryB.Distance(
                                      (Geometry<BufferedCoordinate>) m_objArgument1);

                return Math.Abs(dDistance - dResult) <= m_dTolerance;
            }

            return false;
        }

        protected virtual Boolean TestEnvelope()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry<BufferedCoordinate> envelope =
                    (Geometry<BufferedCoordinate>) m_objGeometryA.Envelope;

                if (envelope != null)
                {
                    return envelope.Equals(geoResult);
                }
            }
            else if (m_objGeometryB != null)
            {
                Geometry<BufferedCoordinate> envelope =
                    (Geometry<BufferedCoordinate>) m_objGeometryB.Envelope;

                if (envelope != null)
                {
                    return envelope.Equals(geoResult);
                }
            }

            return false;
        }

        protected virtual Boolean TestEquals()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.Equals(m_objGeometryB) == bResult
                           : m_objGeometryA.Equals((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.Equals(m_objGeometryA) == bResult
                           : m_objGeometryB.Equals((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            return false;
        }

        protected virtual Boolean TestInteriorPoint()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry<BufferedCoordinate> interiorpoint =
                    (Geometry<BufferedCoordinate>) (m_objGeometryA as ISurface).PointOnSurface;

                if (interiorpoint != null)
                {
                    if (interiorpoint.IsEmpty && geoResult.IsEmpty)
                    {
                        return true;
                    }

                    return interiorpoint.Equals(geoResult);
                }
            }
            else if (m_objGeometryB != null)
            {
                Geometry<BufferedCoordinate> interiorpoint =
                    (Geometry<BufferedCoordinate>) (m_objGeometryB as ISurface).PointOnSurface;

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

        protected virtual Boolean TestIntersection()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry<BufferedCoordinate> intersection =
                        (Geometry<BufferedCoordinate>) m_objGeometryA.Intersection(m_objGeometryB);

                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType() ==
                               typeof (GeometryCollection<BufferedCoordinate>)
                                   ? CompareGeometries(geoResult, intersection)
                                   : intersection.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry<BufferedCoordinate> intersection =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryA.Intersection((Geometry<BufferedCoordinate>) m_objArgument1);

                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType() ==
                               typeof (GeometryCollection<BufferedCoordinate>)
                                   ? CompareGeometries(geoResult, intersection)
                                   : intersection.Equals(geoResult);
                    }
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry<BufferedCoordinate> intersection =
                        (Geometry<BufferedCoordinate>) m_objGeometryB.Intersection(m_objGeometryA);

                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType() ==
                               typeof (GeometryCollection<BufferedCoordinate>)
                                   ? CompareGeometries(geoResult, intersection)
                                   : intersection.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry<BufferedCoordinate> intersection =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryB.Intersection((Geometry<BufferedCoordinate>) m_objArgument1);

                    if (intersection != null)
                    {
                        if (intersection.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType() ==
                               typeof (GeometryCollection<BufferedCoordinate>)
                                   ? CompareGeometries(geoResult, intersection)
                                   : intersection.Equals(geoResult);
                    }
                }
            }

            return false;
        }

        protected virtual Boolean TestIntersects()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.Intersects(m_objGeometryB) == bResult
                           : m_objGeometryA.Intersects(
                                 (Geometry<BufferedCoordinate>) m_objArgument1) == bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.Intersects(m_objGeometryA) == bResult
                           : m_objGeometryB.Intersects(
                                 (Geometry<BufferedCoordinate>) m_objArgument1) == bResult;
            }

            return false;
        }

        protected virtual Boolean TestIsEmpty()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Boolean bState = m_objGeometryA.IsEmpty;

                return bState == bResult;
            }

            if (m_objGeometryB != null)
            {
                Boolean bState = m_objGeometryB.IsEmpty;

                return bState == bResult;
            }

            return false;
        }

        protected virtual Boolean TestIsSimple()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Boolean bState = m_objGeometryA.IsSimple;

                return bState == bResult;
            }

            if (m_objGeometryB != null)
            {
                Boolean bState = m_objGeometryB.IsSimple;

                return bState == bResult;
            }

            return false;
        }

        protected virtual Boolean TestIsValid()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Boolean bState = m_objGeometryA.IsValid;
                return bState == bResult;
            }

            if (m_objGeometryB != null)
            {
                Boolean bState = m_objGeometryB.IsValid;

                return bState == bResult;
            }

            return false;
        }

        protected virtual Boolean TestIsWithinDistance()
        {
            Boolean bResult = (Boolean) m_objResult;
            Double dArg = Double.Parse((String) m_objArgument2, GetNumberFormatInfo());

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.IsWithinDistance(m_objGeometryB, dArg) == bResult
                           : m_objGeometryA.IsWithinDistance(
                                 (Geometry<BufferedCoordinate>) m_objArgument1,
                                 dArg) == bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.IsWithinDistance(m_objGeometryA, dArg) == bResult
                           : m_objGeometryB.IsWithinDistance(
                                 (Geometry<BufferedCoordinate>) m_objArgument1,
                                 dArg) == bResult;
            }

            return false;
        }

        protected virtual Boolean TestLength()
        {
            Double dLengthResult = (Double) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Double dLength = (m_objGeometryA as ISurface).Area;

                return Math.Abs(dLength - dLengthResult) <= m_dTolerance;
            }

            if (m_objGeometryB != null)
            {
                Double dLength = (m_objGeometryB as ISurface).Area;

                return Math.Abs(dLength - dLengthResult) <= m_dTolerance;
            }

            return false;
        }

        protected virtual Boolean TestNumPoints()
        {
            Int32 nResult = (Int32) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Int32 nPoints = m_objGeometryA.PointCount;

                return Math.Abs(nPoints - nResult) <= (Int32) m_dTolerance;
            }

            if (m_objGeometryB != null)
            {
                Int32 nPoints = m_objGeometryB.PointCount;

                return Math.Abs(nPoints - nResult) <= (Int32) m_dTolerance;
            }

            return false;
        }

        protected virtual Boolean TestOverlaps()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.Overlaps(m_objGeometryB) == bResult
                           : m_objGeometryA.Overlaps((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.Overlaps(m_objGeometryA) == bResult
                           : m_objGeometryB.Overlaps((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            return false;
        }

        protected virtual Boolean TestRelate()
        {
            Boolean bResult = (Boolean) m_objResult;
            String arg = (String) m_objArgument2;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                IntersectionMatrix matrix = m_objGeometryA.Relate(m_objGeometryB);

                String strMatrix = matrix.ToString();

                return (strMatrix == arg) == bResult;
            }

            if (m_objGeometryB != null)
            {
                IntersectionMatrix matrix = m_objGeometryB.Relate(m_objGeometryA);

                String strMatrix = matrix.ToString();

                return (strMatrix == arg) == bResult;
            }

            return false;
        }

        protected virtual Boolean TestSRID()
        {
            Int32 nResult = (Int32) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Int32 nSRID = m_objGeometryA.Srid.Value;

                return Math.Abs(nSRID - nResult) <= (Int32) m_dTolerance;
            }

            if (m_objGeometryB != null)
            {
                Int32 nSRID = m_objGeometryB.Srid.Value;

                return Math.Abs(nSRID - nResult) <= (Int32) m_dTolerance;
            }

            return false;
        }

        protected virtual Boolean TestSymDifference()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry<BufferedCoordinate> difference =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryA.SymmetricDifference(m_objGeometryB);

                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, difference)
                                   : difference.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry<BufferedCoordinate> difference =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryA.SymmetricDifference(
                            (Geometry<BufferedCoordinate>) m_objArgument1);

                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, difference)
                                   : difference.Equals(geoResult);
                    }
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry<BufferedCoordinate> difference =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryB.SymmetricDifference(m_objGeometryA);

                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, difference)
                                   : difference.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry<BufferedCoordinate> difference =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryB.SymmetricDifference(
                            (Geometry<BufferedCoordinate>) m_objArgument1);

                    if (difference != null)
                    {
                        if (difference.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, difference)
                                   : difference.Equals(geoResult);
                    }
                }
            }

            return false;
        }

        protected virtual Boolean TestTouches()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.Touches(m_objGeometryB) == bResult
                           : m_objGeometryA.Touches((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.Touches(m_objGeometryA) == bResult
                           : m_objGeometryB.Touches((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            return false;
        }

        protected virtual Boolean TestUnion()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry<BufferedCoordinate> geoResult = (Geometry<BufferedCoordinate>) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry<BufferedCoordinate> union =
                        (Geometry<BufferedCoordinate>) m_objGeometryA.Union(m_objGeometryB);

                    if (union != null)
                    {
                        if (union.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, union)
                                   : union.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry<BufferedCoordinate> union =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryA.Union((Geometry<BufferedCoordinate>) m_objArgument1);

                    if (union != null)
                    {
                        if (union.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, union)
                                   : union.Equals(geoResult);
                    }
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry<BufferedCoordinate> union =
                        (Geometry<BufferedCoordinate>) m_objGeometryB.Union(m_objGeometryA);

                    if (union != null)
                    {
                        if (union.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, union)
                                   : union.Equals(geoResult);
                    }
                }
                else
                {
                    Geometry<BufferedCoordinate> union =
                        (Geometry<BufferedCoordinate>)
                        m_objGeometryB.Union((Geometry<BufferedCoordinate>) m_objArgument1);

                    if (union != null)
                    {
                        if (union.IsEmpty && geoResult.IsEmpty)
                        {
                            return true;
                        }

                        return geoResult.GetType().Name == "GeometryCollection"
                                   ? CompareGeometries(geoResult, union)
                                   : union.Equals(geoResult);
                    }
                }
            }

            return false;
        }

        protected virtual Boolean TestWithin()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.Within(m_objGeometryB) == bResult
                           : m_objGeometryA.Within((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.Within(m_objGeometryA) == bResult
                           : m_objGeometryB.Within((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            return false;
        }

        protected virtual Boolean TestCovers()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.Covers(m_objGeometryB) == bResult
                           : m_objGeometryA.Covers((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.Covers(m_objGeometryA) == bResult
                           : m_objGeometryB.Covers((Geometry<BufferedCoordinate>) m_objArgument1) ==
                             bResult;
            }

            return false;
        }

        protected virtual Boolean TestCoveredBy()
        {
            Boolean bResult = (Boolean) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryA.CoveredBy(m_objGeometryB) == bResult
                           : m_objGeometryA.CoveredBy(
                                 (Geometry<BufferedCoordinate>) m_objArgument1) == bResult;
            }

            if (m_objGeometryB != null)
            {
                return m_objArgument1 == null
                           ? m_objGeometryB.CoveredBy(m_objGeometryA) == bResult
                           : m_objGeometryB.CoveredBy(
                                 (Geometry<BufferedCoordinate>) m_objArgument1) == bResult;
            }

            return false;
        }

        #endregion

        #region Private Members

        private Boolean CompareGeometries(Geometry<BufferedCoordinate> a,
                                          Geometry<BufferedCoordinate> b)
        {
            if (a != null && b != null && a.GetType().Name == b.GetType().Name)
            {
                Geometry<BufferedCoordinate> aClone = (Geometry<BufferedCoordinate>) a.Clone();
                Geometry<BufferedCoordinate> bClone = (Geometry<BufferedCoordinate>) b.Clone();

                aClone.Normalize();
                bClone.Normalize();

                return aClone.Equals(bClone, new Tolerance(m_dTolerance));
            }

            return false;
        }

        #endregion
    }
}