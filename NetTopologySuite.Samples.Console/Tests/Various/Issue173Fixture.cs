using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    public class Issue173Fixture
    {
        [Test, Category("Issue173"), Description("The NetTopologySuite.IO.GeoTools class method ShapeFile.GetGeometryType(IGeometry geom) will always returns ShapeGeometryType.PointZM making all shapefile geometry GeometryZM.")]
        public void Test()
        {
            var features = new List<Features.IFeature>();
            var seq = DotSpatialAffineCoordinateSequenceFactory.Instance.Create(1, Ordinates.XY);
            seq.SetOrdinate(0, Ordinate.X, -91.0454);
            seq.SetOrdinate(0, Ordinate.Y, 32.5907);
            var pt = new GeometryFactory(DotSpatialAffineCoordinateSequenceFactory.Instance).CreatePoint(seq);
            var attr = new Features.AttributesTable();
            attr.AddAttribute("FirstName", "John");
            attr.AddAttribute("LastName", "Doe");
            features.Add(new Features.Feature(pt, attr));

            var fileName = Path.GetTempFileName();
            fileName = fileName.Substring(0, fileName.Length - 4);
            var shpWriter = new IO.ShapefileDataWriter(fileName, features[0].Geometry.Factory)
            {
                Header = IO.ShapefileDataWriter.GetHeader(features[0], features.Count)
            };
            shpWriter.Write(features);

            bool isTrue;
            using (var reader = new IO.ShapefileDataReader(fileName, pt.Factory))
                @isTrue = reader.ShapeHeader.ShapeType.ToString() == "Point";
           
            foreach (var file in Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(fileName) + ".*" ))
            {
                File.Delete(file);
            }

            Assert.IsTrue(@isTrue);
        }
    }
}