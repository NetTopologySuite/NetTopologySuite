
using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Samples.Tests.IO
{
    [TestFixture]
    public class Issue146Fixtures
    {
        // see https://code.google.com/p/nettopologysuite/issues/detail?id=146
        [Test, ExpectedException(typeof(ArgumentException))]
        public void ShapeCreationWithInvalidAttributeName()
        {
            Coordinate[] points = new Coordinate[3];
            points[0] = new Coordinate(0, 0);
            points[1] = new Coordinate(1, 0);
            points[2] = new Coordinate(1, 1);
            LineString ls = new LineString(points);
            IMultiLineString mls = GeometryFactory.Default.CreateMultiLineString(new ILineString[] { ls });

            AttributesTable attrs = new AttributesTable();
            attrs.AddAttribute("Simulation name", "FOO");
            
            Feature[] features = new[] { new Feature(mls, attrs)};            
            ShapefileDataWriter shp_writer = new ShapefileDataWriter("invalid_line_string")
            {
                Header = ShapefileDataWriter.GetHeader(features[0], features.Length)
            };
            shp_writer.Write(features);
        }
    }
}
