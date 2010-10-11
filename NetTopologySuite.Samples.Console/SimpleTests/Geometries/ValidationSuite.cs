using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Samples.SimpleTests.Geometries
{
    public class ValidationSuite : BaseSamples
    {
        public override void Start()
        {
            IGeometry a = Reader.Read("POLYGON ((340 320, 340 200, 200 280, 200 80, " +
                                      "340 200, 340 20, 60 20, 60 340, 340 320))");
            Boolean result = a.IsValid;

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