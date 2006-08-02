using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;
using GisSharpBlog.NetTopologySuite.CoordinateTransformations;
using GisSharpBlog.NetTopologySuite.Positioning;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests
{

    /// <summary>
    /// 
    /// </summary>
    public class CoordSystem : BaseSamples
    {
        private CoordinateSystemEPSGFactory factory = null;
        private CoordinateTransformationEPSGFactory ctfactory = null;

        /// <summary>
        /// 
        /// </summary>
        public CoordSystem() : base()
        {
            Environment.CurrentDirectory = @"..\..\..\";
            factory = new CoordinateSystemEPSGFactory(Global.GetEPSGDataSet());
            ctfactory = new CoordinateTransformationEPSGFactory(Global.GetEPSGDataSet());
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            TestLinearUnit("9001");
            TestLinearUnit("1");
            TestLinearUnit(null);
            TestGeographicCoordinateSystem("27700");
            TestCreateVerticalCoordinateSystem("5701");            
            TestCreateProjectedCoordinateSystem("4326");
            TestAngularUnit("9101");
            IAxisInfo[] axisinfos = factory.GetAxisInfo("4400");
            ParameterList parameterList = factory.GetParameters("19916");
            TestCreateFromTransformationCode1();
            TestCreateFromTransformationCode2();
        }

        private void TestCreateProjectedCoordinateSystem(string code)
        {
            try
            {
                factory.CreateProjectedCoordinateSystem(code);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void TestAngularUnit(string code)
        {
            try
            {
                IAngularUnit angularUnit = factory.CreateAngularUnit(code);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void TestLinearUnit(string code)
        {
            try
            {
                ILinearUnit linearUnit = factory.CreateLinearUnit(code);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void TestGeographicCoordinateSystem(string code)
        {
            try
            {
                IGeographicCoordinateSystem geographicCoordSystem = factory.CreateGeographicCoordinateSystem(code);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void TestCreateVerticalCoordinateSystem(string code)
        {
            try
            {
                IVerticalCoordinateSystem vcs = factory.CreateVerticalCoordinateSystem(code);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
       
        public void TestCreateFromTransformationCode1()
		{
            try
            {
                ICoordinateTransformation UKNationalGrid1 = ctfactory.CreateFromTransformationCode("1036");

                double long1 = -2;
                double lat1 = 49;
                CoordinatePoint pt = new CoordinatePoint();
                pt.Ord = new Double[2];
                pt.Ord[0] = long1;
                pt.Ord[1] = lat1;

                CoordinatePoint result1 = UKNationalGrid1.MathTransform.Transform(pt);

                double metersX = (double)result1.Ord[0];
                double metersY = (double)result1.Ord[1];

                Assert.IsTrue("400000" == metersX.ToString());
                Assert.IsTrue("-100000" == metersY.ToString());

                CoordinatePoint result2 = UKNationalGrid1.MathTransform.GetInverse().Transform(result1);

                double long2 = (double)result2.Ord[0];
                double lat2 = (double)result2.Ord[1];

                Assert.IsTrue("-2" == long2.ToString());
                Assert.IsTrue("49" == lat2.ToString());
            }
            catch (Exception ex)
            {                
                throw ex;
            }
		}

		public void TestCreateFromTransformationCode2()
		{
            try
            {
                ICoordinateTransformation UKNationalGrid1 = ctfactory.CreateFromTransformationCode("1681");
                double long1 = 2.5;
                double lat1 = 53.2;
                CoordinatePoint pt = new CoordinatePoint();
                pt.Ord = new Double[2];
                pt.Ord[0] = long1;
                pt.Ord[1] = lat1;

                CoordinatePoint result1 = UKNationalGrid1.MathTransform.Transform(pt);

                double metersX = (double)result1.Ord[0];
                double metersY = (double)result1.Ord[1];
            }
            catch (Exception ex)
            {                
                throw ex;
            }
		}
    }
}
