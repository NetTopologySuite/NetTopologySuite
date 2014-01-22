using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    public class Issue173Fixture
    {
        [Test]
        public void Test()
        {
            var features = new List<Features.Feature>();
            var seq = DotSpatialAffineCoordinateSequenceFactory.Instance.Create(1, Ordinates.XY);
            seq.SetOrdinate(0, Ordinate.X, -91.0454);
            seq.SetOrdinate(0, Ordinate.Y, 32.5907);
            var pt = new GeometryFactory(DotSpatialAffineCoordinateSequenceFactory.Instance).CreatePoint(seq);
            var attr = new Features.AttributesTable();
            attr.AddAttribute("FirstName", "John");
            attr.AddAttribute("LastName", "Doe");
            features.Add(new Features.Feature(pt, attr));

            var fileName = Path.ChangeExtension(Path.GetTempFileName(), "shp");
            var shpWriter = new IO.ShapefileDataWriter(fileName, features[0].Geometry.Factory);
            shpWriter.Header = IO.ShapefileDataWriter.GetHeader(features[0], features.Count);
            shpWriter.Write(features);


            foreach (var file in Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(fileName) + ".*" ))
            {
                File.Delete(file);
            }
        }
    }
}