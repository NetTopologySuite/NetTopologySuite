using System;
using System.Globalization;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace Open.Topology.TestRunner
{
	/// <summary>
	/// Summary description for XmlTestFactory.
	/// </summary>
	public class XmlTestFactory
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

        protected enum Target
        {
            A = 1,
            B = 2,
            C = 3
        }

        protected GeometryFactory m_objGeometryFactory = null;
        protected WKTReader m_objReader = null;

        public XmlTestFactory(PrecisionModel pm)
		{
            m_objGeometryFactory = new GeometryFactory(pm);
            m_objReader = new WKTReader(m_objGeometryFactory);
        }

        public XmlTest Create(XmlTestInfo testInfo, double tolerance)
        {
            XmlTest xmlTest = new XmlTest(testInfo.GetValue("desc"), 
                testInfo.IsDefaultTarget(), tolerance);

            // Handle test type or name.
            string strTestType = testInfo.GetValue("name");
            if (strTestType == null || strTestType.Length == 0)
                return null;
            
            ParseType(strTestType, xmlTest);

            // Handle the Geometry A:
            string wktA = testInfo.GetValue("a");
            if (wktA != null && wktA.Length > 0)
                ParseGeometry(Target.A, wktA, xmlTest);
            
            // Handle the Geometry B:
            string wktB = testInfo.GetValue("b");
            if (wktB != null && wktB.Length > 0)           
                ParseGeometry(Target.B, wktB, xmlTest);            

            // Handle the arguments
            string arg2 = testInfo.GetValue("arg2");
            if (arg2 != null && arg2.Length > 0)
            {
                if (arg2 == "a")
                    xmlTest.Argument1 = xmlTest.A;
                else if (arg2 == "b")
                    xmlTest.Argument1 = xmlTest.B;
            }

            string arg3 = testInfo.GetValue("arg3");
            if (arg3 != null && arg3.Length > 0)            
                xmlTest.Argument2 = arg3;

            string strResult = testInfo.GetValue("result");
            if (strResult == null || strResult.Length == 0)
                return null;

            ParseResult(strResult, xmlTest);

            return xmlTest;
        }

        protected bool ParseType(string testType, XmlTest xmlTestItem)
        {
            testType = testType.ToLower();

            if (testType == "getarea")
                xmlTestItem.TestType = XmlTestType.Area;
            
            else if (testType == "getboundary")
                xmlTestItem.TestType = XmlTestType.Boundary;
            
            else if (testType == "getboundarydimension")
                xmlTestItem.TestType = XmlTestType.BoundaryDimension;
            
            else if (testType == "buffer")
                xmlTestItem.TestType = XmlTestType.Buffer;
            
            else if (testType == "getcentroid")
                xmlTestItem.TestType = XmlTestType.Centroid;
            
            else if (testType == "contains")
                xmlTestItem.TestType = XmlTestType.Contains;
            
            else if (testType == "convexhull")
                xmlTestItem.TestType = XmlTestType.ConvexHull;
            
            else if (testType == "crosses")
                xmlTestItem.TestType = XmlTestType.Crosses;
            
            else if (testType == "difference")
                xmlTestItem.TestType = XmlTestType.Difference;
            
            else if (testType == "getdimension")
                xmlTestItem.TestType = XmlTestType.Dimension;
            
            else if (testType == "disjoint")
                xmlTestItem.TestType = XmlTestType.Disjoint;
            
            else if (testType == "distance")
                xmlTestItem.TestType = XmlTestType.Distance;
            
            else if (testType == "getenvelope")
                xmlTestItem.TestType = XmlTestType.Envelope;
            
            else if (testType == "equals")
                xmlTestItem.TestType = XmlTestType.Equals;
            
            else if (testType == "getinteriorpoint")
                xmlTestItem.TestType = XmlTestType.InteriorPoint;
            
            else if (testType == "intersection")
                xmlTestItem.TestType = XmlTestType.Intersection;
            
            else if (testType == "intersects")
                xmlTestItem.TestType = XmlTestType.Intersects;
            
            else if (testType == "isempty")
                xmlTestItem.TestType = XmlTestType.IsEmpty;
            
            else if (testType == "issimple")
                xmlTestItem.TestType = XmlTestType.IsSimple;
            
            else if (testType == "isvalid")
                xmlTestItem.TestType = XmlTestType.IsValid;
            
            else if (testType == "iswithindistance")
                xmlTestItem.TestType = XmlTestType.IsWithinDistance;
            
            else if (testType == "getlength")
                xmlTestItem.TestType = XmlTestType.Length;
            
            else if (testType == "getnumpoints")
                xmlTestItem.TestType = XmlTestType.NumPoints;
            
            else if (testType == "overlaps")
                xmlTestItem.TestType = XmlTestType.Overlaps;
            
            else if (testType == "relate")
                xmlTestItem.TestType = XmlTestType.Relate;
            
            else if (testType == "getsrid")
                xmlTestItem.TestType = XmlTestType.SRID;
            
            else if (testType == "symmetricdifference")
                xmlTestItem.TestType = XmlTestType.SymmetricDifference;
            
            else if (testType == "symdifference")
                xmlTestItem.TestType = XmlTestType.SymmetricDifference;
            
            else if (testType == "touches")
                xmlTestItem.TestType = XmlTestType.Touches;
            
            else if (testType == "union")
                xmlTestItem.TestType = XmlTestType.Union;
            
            else if (testType == "within")
                xmlTestItem.TestType = XmlTestType.Within;

            else if (testType == "covers")
                xmlTestItem.TestType = XmlTestType.Covers;

            else if (testType == "coveredby")
                xmlTestItem.TestType = XmlTestType.CoveredBy;
            
            else throw new ArgumentException(String.Format("The operation type \"{0}\" is not valid: ", testType));
            
            return true;
        }

        protected bool ParseResult(string result, XmlTest xmlTestItem)
        {
            switch (xmlTestItem.TestType) 
            {
                // Here we expect double
                case XmlTestType.Area:
                case XmlTestType.Distance:
                case XmlTestType.Length:
                {
                    try
                    {
                        xmlTestItem.Result = Double.Parse(result, GetNumberFormatInfo());
                        return true;
                    }
                    catch (Exception ex)
                    {
                        XmlTestExceptionManager.Publish(ex);
                        return false;
                    }
                }

                // Here we expect integer
                case XmlTestType.BoundaryDimension:
                case XmlTestType.Dimension:
                case XmlTestType.NumPoints:
                case XmlTestType.SRID:
                {
                    try
                    {
                        xmlTestItem.Result = Int32.Parse(result);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        XmlTestExceptionManager.Publish(ex);
                        return false;
                    }
                }

                // Here we expect a point
                case XmlTestType.Boundary:
                case XmlTestType.Buffer:
                case XmlTestType.Centroid:
                case XmlTestType.ConvexHull:
                case XmlTestType.Difference:
                case XmlTestType.Envelope:
                case XmlTestType.InteriorPoint:
                case XmlTestType.Intersection:
                case XmlTestType.SymmetricDifference:
                case XmlTestType.Union:
                {
                    try
                    {
                        xmlTestItem.Result = m_objReader.Read(result);                        
                        return true;
                    }
                    catch (Exception ex)
                    {
                        XmlTestExceptionManager.Publish(ex);
                        return false;
                    }
                }

                // Here we expect boolean
                case XmlTestType.Contains:
                case XmlTestType.Crosses:
                case XmlTestType.Disjoint:
                case XmlTestType.Equals:
                case XmlTestType.Intersects:
                case XmlTestType.IsEmpty:
                case XmlTestType.IsSimple:
                case XmlTestType.IsValid:
                case XmlTestType.IsWithinDistance:
                case XmlTestType.Overlaps:
                case XmlTestType.Relate:
                case XmlTestType.Touches:
                case XmlTestType.Within:
                case XmlTestType.Covers:
                case XmlTestType.CoveredBy:
                {
                    try
                    {
                        xmlTestItem.Result = Boolean.Parse(result);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        XmlTestExceptionManager.Publish(ex);
                        return false;
                    }
                }

                default:
                    break;
            }
            return false;
        }

        protected bool ParseGeometry(Target targetType, string targetText, XmlTest xmlTestItem)
        {   
            IGeometry geom = null;
            try
            {
                geom = m_objReader.Read(targetText);
            }
            catch (Exception ex)
            {
                xmlTestItem.Thrown = ex;
                XmlTestExceptionManager.Publish(ex);
                return false;
            }

            if (geom == null)
                return false;

            switch (targetType) 
            {
                case Target.A:
                    xmlTestItem.A = geom;
                    break;

                case Target.B:
                    xmlTestItem.B = geom;
                    break;
            }

            return true;
        }
    }
}
