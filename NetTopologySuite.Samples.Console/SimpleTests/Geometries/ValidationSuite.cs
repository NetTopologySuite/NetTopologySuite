using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Geometries
{
    public class ValidationSuite : BaseSamples
    {
        public ValidationSuite()
        {
            //ICoordinateFactory<BufferedCoordinate2D> coordinateFactory =
            //    new BufferedCoordinate2DFactory();
            //IPrecisionModel<BufferedCoordinate2D> pm
            //    = new PrecisionModel<BufferedCoordinate2D>(coordinateFactory, PrecisionModelType.Fixed);
        }

        public override void Start()
        {
            IGeometry a = Reader.Read("POLYGON ((340 320, 340 200, 200 280, 200 80, " +
                                      "340 200, 340 20, 60 20, 60 340, " +
                                      "340 320))");
            bool result = a.IsValid;

            if (result)
            {
                Console.WriteLine("Error!");
            }
            else
            {
                Console.WriteLine("Work completed!");
            }
        }
    }
}