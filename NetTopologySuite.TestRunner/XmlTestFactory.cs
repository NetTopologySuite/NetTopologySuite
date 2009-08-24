using System;
using System.Globalization;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite
{
    public class XmlTestFactory
    {
        private static NumberFormatInfo _numberFormapInfo;

        protected static IFormatProvider GetNumberFormatInfo()
        {
            if (_numberFormapInfo == null)
            {
                _numberFormapInfo = new NumberFormatInfo();
                _numberFormapInfo.NumberDecimalSeparator = ".";
            }

            return _numberFormapInfo;
        }

        protected enum Target
        {
            A = 1,
            B = 2,
            C = 3
        }

        protected IGeometryFactory<BufferedCoordinate> _geometryFactory;
        protected IWktGeometryReader _wktReader;

        public XmlTestFactory(ICoordinateSequenceFactory<BufferedCoordinate> seqFactory)
        {
            _geometryFactory = new GeometryFactory<BufferedCoordinate>(seqFactory);
            _wktReader = new WktReader<BufferedCoordinate>(_geometryFactory, null);
        }

        public XmlTest Create(Int32 index, XmlTestInfo testInfo, Double tolerance)
        {
            XmlTest xmlTest = new XmlTest(index, testInfo.GetValue("desc"),
                                          testInfo.IsDefaultTarget(),
                                          tolerance);

            // Handle test type or name.
            String testType = testInfo.GetValue("name");
            
            if (String.IsNullOrEmpty(testType))
            {
                return null;
            }

            ParseType(testType, xmlTest);

            // Handle the Geometry A:
            String wktA = testInfo.GetValue("a");
            
            if (!String.IsNullOrEmpty(wktA))
            {
                ParseGeometry(Target.A, wktA, xmlTest);
            }

            // Handle the Geometry B:
            String wktB = testInfo.GetValue("b");
            
            if (!String.IsNullOrEmpty(wktB))
            {
                ParseGeometry(Target.B, wktB, xmlTest);
            }

            // Handle the arguments
            String arg2 = testInfo.GetValue("arg2");

            if (!String.IsNullOrEmpty(arg2))
            {
                switch (arg2[0])
                {
                    case 'A':
                    case 'a':
                        xmlTest.Argument1 = xmlTest.A;
                        break;
                    case 'B':
                    case 'b':
                        xmlTest.Argument1 = xmlTest.B;
                        break;
                }
            }

            String arg3 = testInfo.GetValue("arg3");
            
            if (!String.IsNullOrEmpty(arg3))
            {
                xmlTest.Argument2 = arg3;
            }

            String result = testInfo.GetValue("result");
            
            if (String.IsNullOrEmpty(result))
            {
                return null;
            }

            ParseResult(result, xmlTest);

            return xmlTest;
        }

        protected Boolean ParseType(String testType, XmlTest xmlTestItem)
        {
            testType = testType.ToLower();

            switch (testType.ToLower())
            {
                case "getarea":
                    xmlTestItem.TestType = XmlTestType.Area;
                    break;
                case "getboundary":
                    xmlTestItem.TestType = XmlTestType.Boundary;
                    break;
                case "getboundarydimension":
                    xmlTestItem.TestType = XmlTestType.BoundaryDimension;
                    break;
                case "buffer":
                    xmlTestItem.TestType = XmlTestType.Buffer;
                    break;
                case "getcentroid":
                    xmlTestItem.TestType = XmlTestType.Centroid;
                    break;
                case "contains":
                    xmlTestItem.TestType = XmlTestType.Contains;
                    break;
                case "convexhull":
                    xmlTestItem.TestType = XmlTestType.ConvexHull;
                    break;
                case "crosses":
                    xmlTestItem.TestType = XmlTestType.Crosses;
                    break;
                case "difference":
                    xmlTestItem.TestType = XmlTestType.Difference;
                    break;
                case "getdimension":
                    xmlTestItem.TestType = XmlTestType.Dimension;
                    break;
                case "disjoint":
                    xmlTestItem.TestType = XmlTestType.Disjoint;
                    break;
                case "distance":
                    xmlTestItem.TestType = XmlTestType.Distance;
                    break;
                case "getenvelope":
                    xmlTestItem.TestType = XmlTestType.Envelope;
                    break;
                case "equals":
                    xmlTestItem.TestType = XmlTestType.Equals;
                    break;
                case "getinteriorpoint":
                    xmlTestItem.TestType = XmlTestType.InteriorPoint;
                    break;
                case "intersection":
                    xmlTestItem.TestType = XmlTestType.Intersection;
                    break;
                case "intersects":
                    xmlTestItem.TestType = XmlTestType.Intersects;
                    break;
                case "isempty":
                    xmlTestItem.TestType = XmlTestType.IsEmpty;
                    break;
                case "issimple":
                    xmlTestItem.TestType = XmlTestType.IsSimple;
                    break;
                case "isvalid":
                    xmlTestItem.TestType = XmlTestType.IsValid;
                    break;
                case "iswithindistance":
                    xmlTestItem.TestType = XmlTestType.IsWithinDistance;
                    break;
                case "getlength":
                    xmlTestItem.TestType = XmlTestType.Length;
                    break;
                case "getnumpoints":
                    xmlTestItem.TestType = XmlTestType.NumPoints;
                    break;
                case "overlaps":
                    xmlTestItem.TestType = XmlTestType.Overlaps;
                    break;
                case "relate":
                    xmlTestItem.TestType = XmlTestType.Relate;
                    break;
                case "getsrid":
                    xmlTestItem.TestType = XmlTestType.SRID;
                    break;
                case "symmetricdifference":
                    xmlTestItem.TestType = XmlTestType.SymmetricDifference;
                    break;
                case "symdifference":
                    xmlTestItem.TestType = XmlTestType.SymmetricDifference;
                    break;
                case "touches":
                    xmlTestItem.TestType = XmlTestType.Touches;
                    break;
                case "union":
                    xmlTestItem.TestType = XmlTestType.Union;
                    break;
                case "within":
                    xmlTestItem.TestType = XmlTestType.Within;
                    break;
                case "covers":
                    xmlTestItem.TestType = XmlTestType.Covers;
                    break;
                case "coveredby":
                    xmlTestItem.TestType = XmlTestType.CoveredBy;
                    break;
                default:
                    throw new ArgumentException(
                        String.Format("The operation type \"{0}\" is not valid: ", testType));
            }

            return true;
        }

        protected Boolean ParseResult(String result, XmlTest xmlTestItem)
        {
            switch (xmlTestItem.TestType)
            {
                    // Here we expect Double
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
                        xmlTestItem.Result = _wktReader.Read(result);
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

        protected Boolean ParseGeometry(Target targetType, String targetText, XmlTest xmlTestItem)
        {
            IGeometry geom;

            try
            {
                geom = _wktReader.Read(targetText);
            }
            catch (Exception ex)
            {
                xmlTestItem.Thrown = ex;
                XmlTestExceptionManager.Publish(ex);
                return false;
            }

            if (geom == null)
            {
                return false;
            }

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