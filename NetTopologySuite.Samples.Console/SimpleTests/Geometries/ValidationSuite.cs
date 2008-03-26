using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidationSuite : BaseSamples
    {
        /// <summary>
        /// 
        /// </summary>
        public ValidationSuite()
        {
            IPrecisionModel<BufferedCoordinate2D> pm
                = new PrecisionModel<BufferedCoordinate2D>(PrecisionModelType.Fixed);
        }

        /// <summary>
        /// 
        /// </summary>
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