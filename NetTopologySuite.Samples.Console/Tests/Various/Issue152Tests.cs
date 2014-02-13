using DotSpatial.Projections;
using NUnit.Framework;
using NetTopologySuite.CoordinateSystems.Transformation.DotSpatial.Projections;
using NetTopologySuite.CoordinateSystems.Transformations;

namespace NetTopologySuite.Tests.Various
{
    public class Issue152Tests
    {
        //The standard DotSpatial definition, KnownCoordinateSystems.Projected.NationalGrids.BritishNationalGridOSGB36, is incorrect so needed defined string below
        const string BritishNationalGridOsgb36String = "+proj=tmerc +lat_0=49 +lon_0=-2 +k=0.9996012717 +x_0=400000 +y_0=-100000 +ellps=airy +towgs84=446.448,-125.157,542.060,0.1502,0.2470,0.8421,-20.4894 +units=m +no_defs";

        readonly ProjectionInfo _britishNationalGridOsgb36 = ProjectionInfo.FromProj4String(BritishNationalGridOsgb36String);
        readonly ProjectionInfo _wgs84 = KnownCoordinateSystems.Geographic.World.WGS1984;

        [Test]
        public void ConvertAPointUsingDotSpatialReproject()
        {
            //SETUP
            var xy = new double[] { 532248.29992272425, 181560.30052819476 };
            var z = new double[] { 0 };

            //ATTEMPT
            Reproject.ReprojectPoints(xy, z,
                                      _britishNationalGridOsgb36, _wgs84, 0, z.Length);

            //VERIFY            
            Assert.AreEqual(xy[0], -0.095399303, 0.001);
            Assert.AreEqual(xy[1], 51.517489, 0.001);
        }


        [Test, Category("Issue152")]
        public void ConvertAPointUsingNetTopologySuiteTransformGeometry()
        {
            //SETUP
            var factory = NetTopologySuite.Geometries.GeometryFactory.Default;
            var pointNatGrid = new NetTopologySuite.Geometries.Point(532248.29992272425, 181560.30052819476);

            //ATTEMPT
            var transform = new DotSpatialMathTransform(
                _britishNationalGridOsgb36, _wgs84);
            var result = GeometryTransform.TransformGeometry(
                factory, pointNatGrid, transform);

            //VERIFY            
            Assert.AreEqual("Point", result.GeometryType);
            Assert.AreEqual(1, result.Coordinates.Length);
            Assert.AreEqual(-0.095399303,result.Coordinate.X,  0.001);
            Assert.AreEqual(51.517489, result.Coordinates[0].Y, 0.001);
        }

    }
}