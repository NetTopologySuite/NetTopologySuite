using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.CoordinateSystems.Transformations;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace NetTopologySuite.Samples.Tests.Various
{
    [TestFixture]
    public class Issue156Tests
    {
        [Test, Category("Issue156")]
        public void TestTransform()
        {
            CoordinateSystemFactory csFactory = new CoordinateSystemFactory();
            const string sourceCsWkt = "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";                                    
            ICoordinateSystem sourceCs = csFactory.CreateFromWkt(sourceCsWkt);
            Assert.That(sourceCs, Is.Not.Null);
            const string targetCsWkt = "PROJCS[\"WGS 84 / Australian Antarctic Lambert\",GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]],PROJECTION[\"Lambert_Conformal_Conic_2SP\"],PARAMETER[\"standard_parallel_1\",-68.5],PARAMETER[\"standard_parallel_2\",-74.5],PARAMETER[\"latitude_of_origin\",-50],PARAMETER[\"central_meridian\",70],PARAMETER[\"false_easting\",6000000],PARAMETER[\"false_northing\",6000000],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AUTHORITY[\"EPSG\",\"3033\"]]";
            ICoordinateSystem targetCs = csFactory.CreateFromWkt(targetCsWkt);
            Assert.That(targetCs, Is.Not.Null);
            CoordinateTransformationFactory ctFactory = new CoordinateTransformationFactory();
            ICoordinateTransformation coordTransformation = ctFactory.CreateFromCoordinateSystems(sourceCs, targetCs);

            IGeometryFactory gf = GeometryFactory.Default;            
            const string geomWkt = "MULTIPOINT (152.83949210500001 -42.14413555,152.83910355899999 -42.129844618)";
            WKTReader reader = new WKTReader(gf);
            IGeometry geom = reader.Read(geomWkt);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom.IsValid, Is.True);
            Assert.That(geom, Is.InstanceOf<IMultiPoint>());

            IMultiPoint mp = (IMultiPoint)geom;
            foreach (IPoint pt in mp.Geometries)
            {
                IGeometry tp = GeometryTransform.TransformGeometry(gf, pt, coordTransformation.MathTransform);
                Assert.That(tp, Is.Not.Null);
                Assert.That(tp.IsValid, Is.True);
            }

            IGeometry transformed = GeometryTransform.TransformGeometry(gf, mp, coordTransformation.MathTransform);
            Assert.That(transformed, Is.Not.Null);
            Assert.That(transformed.IsValid, Is.True);
        }
    }
}