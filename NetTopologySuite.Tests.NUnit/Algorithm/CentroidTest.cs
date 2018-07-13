using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class CentroidTest
    {

        private const double Tolerance = 1e-10;

        /// <summary>
        /// Compute the centroid of a geometry as an area-weighted average of the centroids
        /// of its components.
        /// </summary>
        /// <param name="g">A polygonal geometry</param>
        /// <returns>Coordinate of the geometry's centroid</returns>
        private static Coordinate AreaWeightedCentroid(IGeometry g)
        {
            double totalArea = g.Area;
            double cx = 0d;
            double cy = 0d;

            for (int i = 0; i < g.NumGeometries; i++)
            {
                var component = g.GetGeometryN(i);
                double areaFraction = component.Area / totalArea;

                var componentCentroid = component.Centroid.Coordinate;

                cx += areaFraction * componentCentroid.X;
                cy += areaFraction * componentCentroid.Y;
            }

            return new Coordinate(cx, cy);
        }

        [Test]
        public void TestCentroidMultiPolygon()
        {
            // Verify that the computed centroid of a MultiPolygon is equivalent to the
            // area-weighted average of its components.
            var g = new WKTReader().Read(
                "MULTIPOLYGON ((( -92.661322 36.58994900000003, -92.66132199999993 36.58994900000005, " +
                "-92.66132199999993 36.589949000000004, -92.661322 36.589949, -92.661322 36.58994900000003)), " +
                "(( -92.65560500000008 36.58708800000005, -92.65560499999992 36.58708800000005, " +
                "-92.65560499998745 36.587087999992576, -92.655605 36.587088, -92.65560500000008 36.58708800000005 )), " +
                "(( -92.65512450000065 36.586800000000466, -92.65512449999994 36.58680000000004, " +
                "-92.65512449998666 36.5867999999905, -92.65512450000065 36.586800000000466 )))");

            Assert.IsTrue(AreaWeightedCentroid(g).Equals2D(g.Centroid.Coordinate, Tolerance));
        }

    }
}