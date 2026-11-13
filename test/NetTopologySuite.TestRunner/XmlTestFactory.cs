using System;
using System.Globalization;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Open.Topology.TestRunner.Operations;
using Open.Topology.TestRunner.Result;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner
{
    /// <summary>
    /// Summary description for XmlTestFactory.
    /// </summary>
    public class XmlTestFactory
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

        protected enum Target
        {
            A = 1,
            B = 2,
            C = 3
        }

        private readonly PrecisionModel _pm;
        private readonly IGeometryOperation _geometryOperation;
        private readonly IResultMatcher _resultMatcher;

        private MultiFormatReader _objReader;

        public XmlTestFactory(PrecisionModel pm, IGeometryOperation geometryOperation, IResultMatcher resultMatcher)
        {
            _pm = pm;
            //ObjGeometryFactory = gs.CreateGeometryFactory();

            // Refused here rather than tolerated: without an operation there is nothing to ask
            // for a return type, so every case would be dropped as unreadable and whole files
            // would come back empty without a word about why.
            _geometryOperation = geometryOperation
                                 ?? throw new ArgumentNullException(nameof(geometryOperation));
            _resultMatcher = resultMatcher;            
        }

        public XmlTest Create(XmlTestInfo testInfo, double tolerance)
        {
            // Handle test type or name.
            string strTestType = testInfo.GetValue("name");
            if (string.IsNullOrEmpty(strTestType))
                return null;

            var xmlTest = new XmlTest(testInfo.GetValue("desc"),
                testInfo.IsDefaultTarget(), tolerance, _geometryOperation, _resultMatcher);

            try
            {
                if (!ParseType(strTestType, xmlTest))
                    return null;
            }
            catch (Exception)
            {
                return null;
            }

            // Handle the Geometry A:
            string wkt = testInfo.GetValue("a");
            if (!string.IsNullOrEmpty(wkt))
                ParseGeometry(Target.A, wkt, xmlTest);

            // Handle the Geometry B:
            wkt = testInfo.GetValue("b");
            if (!string.IsNullOrEmpty(wkt))
                ParseGeometry(Target.B, wkt, xmlTest);

            string arg2 = testInfo.GetValue("arg2");
            if (!string.IsNullOrEmpty(arg2))
            {
                if (arg2 == "a" || arg2 == "A")
                    xmlTest.Argument1 = xmlTest.A;
                else if (arg2 == "b" || arg2 == "B")
                    xmlTest.Argument1 = xmlTest.B;
                else
                    xmlTest.Argument1 = arg2;
            }

            string arg3 = testInfo.GetValue("arg3");
            if (!string.IsNullOrEmpty(arg3))
                xmlTest.Argument2 = arg3;

            // A case is allowed to state no expected result, and eight in the corpus do. Upstream
            // keeps those: it runs the operation and counts the case as passed, which leaves them
            // as a check that the operation does not throw. Dropping them here lost eight cases
            // without a word.
            string strResult = testInfo.GetValue("result")?.Trim();
            if (!string.IsNullOrEmpty(strResult))
            {
                // A case whose expected value could not be read, or whose operation nothing
                // answers to, has nothing to compare against: handing it back would report it
                // later as a wrong answer rather than as a case this runner cannot run.
                if (!ParseResult(strResult, xmlTest))
                    return null;
            }

            return xmlTest;
        }

        /// <summary>
        /// Records the operation the case names, and prepares the reader its geometries are read
        /// with.
        /// <para/>
        /// The name is carried through as written: which member or function answers to it is the
        /// operation's business, so a name nothing knows is reported when the case is built
        /// rather than rejected here.
        /// </summary>
        protected bool ParseType(string testType, XmlTest xmlTestItem)
        {
            var gs = new NtsGeometryServices(
                CoordinateArraySequenceFactory.Instance,
                _pm,
                -1,
                GeometryOverlay.Legacy,
                new CoordinateEqualityComparer());
            _objReader = new MultiFormatReader(gs);

            xmlTestItem.Operation = testType;
            return true;
        }

        /// <summary>
        /// Reads the value the case expects into a result of the shape the operation returns.
        /// </summary>
        /// <returns>
        /// <c>false</c> when the value cannot be read, and when no operation answers to the name
        /// the case gave, which is how a case the runner cannot run comes to contribute nothing.
        /// </returns>
        protected bool ParseResult(string result, XmlTest xmlTestItem)
        {
            var returnType = _geometryOperation.GetReturnType(xmlTestItem.Operation);
            if (returnType == null)
            {
                // Nothing answers to this operation name, so there is no shape to read the
                // expected value into. The case is still built and still run: the operation then
                // reports itself missing when it is invoked, which is louder than the case
                // vanishing from a file that goes on contributing its other cases.
                return true;
            }

            try
            {
                if (returnType == typeof(bool))
                    xmlTestItem.ExpectedResult = new BooleanResult(bool.Parse(result.Trim()));
                else if (returnType == typeof(int))
                    xmlTestItem.ExpectedResult = new IntegerResult(int.Parse(result.Trim()));
                else if (returnType == typeof(double))
                    xmlTestItem.ExpectedResult = new DoubleResult(double.Parse(result.Trim(), GetNumberFormatInfo()));
                else if (typeof(Geometry).IsAssignableFrom(returnType))
                    xmlTestItem.ExpectedResult = new GeometryResult(_objReader.Read(result));
                else
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                XmlTestExceptionManager.Publish(ex);
                return false;
            }
        }

        protected bool ParseGeometry(Target targetType, string targetText, XmlTest xmlTestItem)
        {
            Geometry geom;
            try
            {
                geom = _objReader.Read(targetText);
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
