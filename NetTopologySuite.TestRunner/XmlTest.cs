using System;
using System.Diagnostics;
using System.Globalization;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner
{
    #region Test Event Definitions

    public class XmlTestEventArgs : EventArgs
    {
        private int m_nIndex      = -1;
        private bool m_bSuccess   = false;
        private XmlTest m_objTest = null;

        public XmlTestEventArgs(int index, bool success, XmlTest testItem) : base()
        {
            m_nIndex   = index;
            m_bSuccess = success;
            m_objTest  = testItem;
        }

        public int Index 
        {
            get
            {
                return m_nIndex;
            }
        }

        public bool Success
        {
            get
            {
                return m_bSuccess;
            }
        }

        public XmlTest Test
        {
            get
            {
                return m_objTest;
            }
        }
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
    }
 
    #endregion

	/// <summary>
	/// Summary description for XmlTest.
	/// </summary>
	public class XmlTest
	{
        private static NumberFormatInfo nfi = null;

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

        private static int m_nCount        = 1;

        private bool    m_bIsDefaultTarget = true;
        
        private Exception m_objException   = null;

        private bool      m_bSuccess       = false;
        private object    m_objResult      = null;

        private IGeometry  m_objGeometryA   = null;
        private IGeometry  m_objGeometryB   = null;

        private object    m_objArgument1   = null;
        private object    m_objArgument2   = null;

        private XmlTestType m_enumTestType = XmlTestType.None;

        private string    m_strDescription = null;

        private double    m_dTolerance     = Double.Epsilon;

        #endregion

        #region Constructors and Destructor
		
        public XmlTest(string description, bool bIsDefaultTarget, double tolerance)
		{
            if (description != null && description.Length != 0)
            {
                m_strDescription = description;
            }
            else
            {
                m_strDescription = "Untitled" + m_nCount.ToString();

                ++m_nCount;
            }

            m_bIsDefaultTarget = bIsDefaultTarget;
            m_dTolerance       = tolerance;
		}

        #endregion

        #region Public Properties

        public string Description
        {
            get
            {
                return m_strDescription;
            }

            set
            {
                m_strDescription = value;
            }
        }

        public Exception Thrown
        {
            get
            {
                return m_objException;
            }

            set
            {
                m_objException = value;
            }
        }

        public bool Success
        {
            get
            {
                return m_bSuccess;
            }
        }

        public IGeometry A
        {
            get
            {
                return m_objGeometryA;
            }

            set 
            {
                m_objGeometryA = value;
            }
        }

        public IGeometry B
        {
            get
            {
                return m_objGeometryB;
            }

            set 
            {
                m_objGeometryB = value;
            }
        }

        public XmlTestType TestType
        {
            get
            {
                return m_enumTestType;
            }

            set
            {
                m_enumTestType = value;
            }
        }

        public object Result
        {
            get
            {
                return m_objResult;
            }

            set
            {
                m_objResult = value;
            }
        }

        public object Argument1
        {
            get
            {
                return m_objArgument1;
            }

            set
            {
                m_objArgument1 = value;
            }
        }

        public object Argument2
        {
            get
            {
                return m_objArgument2;
            }

            set
            {
                m_objArgument2 = value;
            }
        }

		public bool IsDefaultTarget
		{
            get
            {
                return m_bIsDefaultTarget;
            }

            set
            {
				m_bIsDefaultTarget = value;
            }
		}

        #endregion

        #region Public Methods
        
        public bool Run()
        {
            try
            {
                m_bSuccess = this.RunTest();
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
                    Console.WriteLine(String.Format("Result expected is {0}, but was {1}", true, m_bSuccess));
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

        protected virtual bool RunTest()
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

        protected virtual bool TestArea()
        {
            double dAreaResult = (double)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                double dArea = m_objGeometryA.Area;

                return Math.Abs(dArea - dAreaResult) <= m_dTolerance;
            }
            else if (m_objGeometryB != null)
            {
                double dArea = m_objGeometryB.Area;

                return Math.Abs(dArea - dAreaResult) <= m_dTolerance;
            }

            return false;
        }

        protected virtual bool TestBoundary()          
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry geoResult = (Geometry) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry boundary = (Geometry) m_objGeometryA.Boundary;
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
            else if (m_objGeometryB != null)
            {
                Geometry boundary = (Geometry) m_objGeometryB.Boundary;
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
            int nResult = (int)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                double dArea = m_objGeometryA.Area;

                return Math.Abs(dArea - nResult) <= m_dTolerance;
            }
            else if (m_objGeometryB != null)
            {
                double dArea = m_objGeometryB.Area;

                return Math.Abs(dArea - nResult) <= m_dTolerance;
            }

            return false;
        }

        protected virtual bool TestBuffer()            
        {
            Geometry geoResult = (Geometry)m_objResult;
            double dArg        = Double.Parse((string)m_objArgument2, GetNumberFormatInfo());

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry buffer = (Geometry) m_objGeometryA.Buffer(dArg);
                if (buffer != null)
                {
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
            else if (m_objGeometryB != null)
            {
                Geometry buffer = (Geometry) m_objGeometryB.Buffer(dArg);
                if (buffer != null)
                {
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

        protected virtual bool TestCentroid()          
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry geoResult = (Geometry)m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry centroid = (Geometry) m_objGeometryA.Centroid;
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
            else if (m_objGeometryB != null)
            {
                Geometry centroid = (Geometry) m_objGeometryB.Centroid;
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
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryA.Contains(m_objGeometryB) == bResult;
                }
                else
                {
                    return m_objGeometryA.Contains((Geometry)m_objArgument1) == bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryB.Contains(m_objGeometryA) == bResult;
                }
                else
                {
                    return m_objGeometryB.Contains((Geometry)m_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestConvexHull()        
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry geoResult = (Geometry)m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry convexhall = (Geometry) m_objGeometryA.ConvexHull();
                if (convexhall != null)
                {
                    if (convexhall.IsEmpty && geoResult.IsEmpty)
                        return true;
    
                    bool bResult = CompareGeometries(geoResult, convexhall);  
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
                Geometry convexhall = (Geometry) m_objGeometryB.ConvexHull();
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
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryA.Crosses(m_objGeometryB) == bResult;
                }
                else
                {
                    return m_objGeometryA.Crosses((Geometry)m_objArgument1) == bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryB.Crosses(m_objGeometryA) == bResult;
                }
                else
                {
                    return m_objGeometryB.Crosses((Geometry)m_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestDifference()        
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry geoResult = (Geometry)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry difference = (Geometry) m_objGeometryA.Difference(m_objGeometryB);
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
                    Geometry difference = (Geometry) m_objGeometryA.Difference((Geometry)m_objArgument1);
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
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry difference = (Geometry) m_objGeometryB.Difference(m_objGeometryA);
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
                    Geometry difference = (Geometry) m_objGeometryB.Difference((Geometry)m_objArgument1);
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
            int nResult = (int)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                int nDim = (int) m_objGeometryA.Dimension;

                return Math.Abs(nDim - nResult) <= (int)m_dTolerance;
            }
            else if (m_objGeometryB != null)
            {
                int nDim = (int)m_objGeometryB.Dimension;

                return Math.Abs(nDim - nResult) <= (int)m_dTolerance;
            }

            return false;
        }

        protected virtual bool TestDisjoint()          
        {
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryA.Disjoint(m_objGeometryB) == bResult;
                }
                else
                {
                    return m_objGeometryA.Disjoint((Geometry)m_objArgument1) == bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryB.Disjoint(m_objGeometryA) == bResult;
                }
                else
                {
                    return m_objGeometryB.Disjoint((Geometry)m_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestDistance()          
        {
            double dResult = (double)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                double dDistance = 0;
                if (m_objArgument1 == null)
                {
                    dDistance = m_objGeometryA.Distance(m_objGeometryB);
                }
                else
                {
                    dDistance = m_objGeometryA.Distance((Geometry)m_objArgument1);
                }

                return Math.Abs(dDistance - dResult) <= m_dTolerance;
            }
            else if (m_objGeometryB != null)
            {
                double dDistance = 0;
                if (m_objArgument1 == null)
                {
                    dDistance = m_objGeometryB.Distance(m_objGeometryA);
                }
                else
                {
                    dDistance = m_objGeometryB.Distance((Geometry)m_objArgument1);
                }

                return Math.Abs(dDistance - dResult) <= m_dTolerance;
            }

            return false;
        }

        protected virtual bool TestEnvelope()          
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry geoResult = (Geometry)m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry envelope = (Geometry) m_objGeometryA.Envelope;
                if (envelope != null)
                    return envelope.Equals(geoResult);
            }
            else if (m_objGeometryB != null)
            {
                Geometry envelope = (Geometry) m_objGeometryB.Envelope;
                if (envelope != null)
                    return envelope.Equals(geoResult);
            }

            return false;
        }

        protected virtual bool TestEquals()             
        {
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryA.Equals(m_objGeometryB) == bResult;
                }
                else
                {
                    return m_objGeometryA.Equals((Geometry)m_objArgument1) == bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryB.Equals(m_objGeometryA) == bResult;
                }
                else
                {
                    return m_objGeometryB.Equals((Geometry)m_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestInteriorPoint()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry geoResult = (Geometry) m_objResult;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                Geometry interiorpoint = (Geometry) m_objGeometryA.InteriorPoint;
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
                Geometry interiorpoint = (Geometry) m_objGeometryB.InteriorPoint;
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
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry geoResult = (Geometry)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)                
                {
                    Geometry intersection = (Geometry) m_objGeometryA.Intersection(m_objGeometryB);                    
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
                    Geometry intersection = (Geometry) m_objGeometryA.Intersection((Geometry) m_objArgument1);
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
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry intersection = (Geometry) m_objGeometryB.Intersection(m_objGeometryA);
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
                    Geometry intersection = (Geometry) m_objGeometryB.Intersection((Geometry)m_objArgument1);
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
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryA.Intersects(m_objGeometryB) == bResult;
                }
                else
                {
                    return m_objGeometryA.Intersects((Geometry)m_objArgument1) == bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryB.Intersects(m_objGeometryA) == bResult;
                }
                else
                {
                    return m_objGeometryB.Intersects((Geometry)m_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestIsEmpty()           
        {
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                bool bState = m_objGeometryA.IsEmpty;

                return bState == bResult;
            }
            else if (m_objGeometryB != null)
            {
                bool bState = m_objGeometryB.IsEmpty;

                return bState == bResult;
            }

            return false;
        }

        protected virtual bool TestIsSimple()          
        {
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                bool bState = m_objGeometryA.IsSimple;

                return bState == bResult;
            }
            else if (m_objGeometryB != null)
            {
                bool bState = m_objGeometryB.IsSimple;

                return bState == bResult;
            }

            return false;
        }

        protected virtual bool TestIsValid()           
        {
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                bool bState = m_objGeometryA.IsValid;
                return bState == bResult;
            }
            else if (m_objGeometryB != null)
            {
                bool bState = m_objGeometryB.IsValid;

                return bState == bResult;
            }

            return false;
        }

        protected virtual bool TestIsWithinDistance()
        {
            bool bResult = (bool)m_objResult;
            double dArg = Double.Parse((string)m_objArgument2, GetNumberFormatInfo());

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryA.IsWithinDistance(m_objGeometryB, dArg) == bResult;
                }
                else
                {
                    return m_objGeometryA.IsWithinDistance((Geometry)m_objArgument1, 
                        dArg) == bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryB.IsWithinDistance(m_objGeometryA, dArg) == bResult;
                }
                else
                {
                    return m_objGeometryB.IsWithinDistance((Geometry)m_objArgument1, 
                        dArg) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestLength()            
        {
            double dLengthResult = (double)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                double dLength = m_objGeometryA.Area;

                return Math.Abs(dLength - dLengthResult) <= m_dTolerance;
            }
            else if (m_objGeometryB != null)
            {
                double dLength = m_objGeometryB.Area;

                return Math.Abs(dLength - dLengthResult) <= m_dTolerance;
            }

            return false;
        }

        protected virtual bool TestNumPoints()
        {
            int nResult = (int)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                int nPoints = m_objGeometryA.NumPoints;

                return Math.Abs(nPoints - nResult) <= (int)m_dTolerance;
            }
            else if (m_objGeometryB != null)
            {
                int nPoints = m_objGeometryB.NumPoints;

                return Math.Abs(nPoints - nResult) <= (int)m_dTolerance;
            }

            return false;
        }

        protected virtual bool TestOverlaps()          
        {
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryA.Overlaps(m_objGeometryB) == bResult;
                }
                else
                {
                    return m_objGeometryA.Overlaps((Geometry)m_objArgument1) == bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryB.Overlaps(m_objGeometryA) == bResult;
                }
                else
                {
                    return m_objGeometryB.Overlaps((Geometry)m_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestRelate()            
        {
            bool bResult = (bool)m_objResult;
            string arg   = (string)m_objArgument2;

            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                IntersectionMatrix matrix = m_objGeometryA.Relate(m_objGeometryB);

                string strMatrix = matrix.ToString();

                return (strMatrix == arg) == bResult;
            }
            else if (m_objGeometryB != null)
            {
                IntersectionMatrix matrix = m_objGeometryB.Relate(m_objGeometryA);

                string strMatrix = matrix.ToString();

                return (strMatrix == arg) == bResult;
            }

            return false;
        }

        protected virtual bool TestSRID()              
        {
            int nResult = (int)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                int nSRID = m_objGeometryA.SRID;

                return Math.Abs(nSRID - nResult) <= (int)m_dTolerance;
            }
            else if (m_objGeometryB != null)
            {
                int nSRID = m_objGeometryB.SRID;

                return Math.Abs(nSRID - nResult) <= (int)m_dTolerance;
            }

            return false;
        }

        protected virtual bool TestSymDifference()
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry geoResult = (Geometry)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry difference = (Geometry) m_objGeometryA.SymmetricDifference(m_objGeometryB);
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
                    Geometry difference = (Geometry) m_objGeometryA.SymmetricDifference((Geometry)m_objArgument1);
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
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry difference = (Geometry) m_objGeometryB.SymmetricDifference(m_objGeometryA);
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
                    Geometry difference = (Geometry) m_objGeometryB.SymmetricDifference((Geometry)m_objArgument1);
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
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryA.Touches(m_objGeometryB) == bResult;
                }
                else
                {
                    return m_objGeometryA.Touches((Geometry)m_objArgument1) == bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryB.Touches(m_objGeometryA) == bResult;
                }
                else
                {
                    return m_objGeometryB.Touches((Geometry)m_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestUnion()             
        {
            Trace.Assert(m_objResult != null, "The result object cannot be null");

            Geometry geoResult = (Geometry)m_objResult;            
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry union = (Geometry) m_objGeometryA.Union(m_objGeometryB);
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
                    Geometry union = (Geometry) m_objGeometryA.Union((Geometry)m_objArgument1);
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
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    Geometry union = (Geometry) m_objGeometryB.Union(m_objGeometryA);
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
                    Geometry union = (Geometry) m_objGeometryB.Union((Geometry)m_objArgument1);
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
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryA.Within(m_objGeometryB) == bResult;
                }
                else
                {
                    return m_objGeometryA.Within((Geometry)m_objArgument1) == bResult;
                }
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                {
                    return m_objGeometryB.Within(m_objGeometryA) == bResult;
                }
                else
                {
                    return m_objGeometryB.Within((Geometry)m_objArgument1) == bResult;
                }
            }

            return false;
        }

        protected virtual bool TestCovers()
        {
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                     return m_objGeometryA.Covers(m_objGeometryB) == bResult;
                else return m_objGeometryA.Covers((Geometry)m_objArgument1) == bResult;                
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                     return m_objGeometryB.Covers(m_objGeometryA) == bResult;
                else return m_objGeometryB.Covers((Geometry)m_objArgument1) == bResult;
            }

            return false;
        }

        protected virtual bool TestCoveredBy()
        {
            bool bResult = (bool)m_objResult;
            if (m_bIsDefaultTarget && m_objGeometryA != null)
            {
                if (m_objArgument1 == null)
                     return m_objGeometryA.CoveredBy(m_objGeometryB) == bResult;
                else return m_objGeometryA.CoveredBy((Geometry)m_objArgument1) == bResult;
            }
            else if (m_objGeometryB != null)
            {
                if (m_objArgument1 == null)
                     return m_objGeometryB.CoveredBy(m_objGeometryA) == bResult;
                else return m_objGeometryB.CoveredBy((Geometry)m_objArgument1) == bResult;
            }

            return false;
        }

        #endregion

        #region Private Members

        private bool CompareGeometries(Geometry a, Geometry b)
        {
            if (a != null && b != null && a.GetType().Name == b.GetType().Name)
            {
                Geometry aClone = (Geometry)a.Clone();
                Geometry bClone = (Geometry)b.Clone();

                aClone.Normalize();
                bClone.Normalize();

                return aClone.EqualsExact(bClone, m_dTolerance);
            }

            return false;
        }

        #endregion
	}
}
