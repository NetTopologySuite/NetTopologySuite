using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    /// <summary>
    /// see: https://github.com/NetTopologySuite/NetTopologySuite/issues/36
    /// </summary>
    [TestFixture]
    public class Issue36Tests : BaseSamples
    {
        [SetUp]
        public void SetUp()
        {
            // Set current dir to shapefiles dir
            Environment.CurrentDirectory =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    String.Format("..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles", Path.DirectorySeparatorChar));
        }
        [Test]
        public void ok_when_writing_shapefile_with_features()
        {
            DbaseFileHeader header = new DbaseFileHeader();
            header.AddColumn("X", 'C', 10, 0);
            ShapefileDataWriter writer = new ShapefileDataWriter(@"issue36") { Header = header };

            IAttributesTable attributesTable = new AttributesTable();
            attributesTable.AddAttribute("X", "y");
            IFeature feature = new Feature(new Point(1, 2), attributesTable);

            IList<IFeature> features = new List<IFeature>();            
            features.Add(feature);
            writer.Write(features);
        }

        [Test]
        public void ok_when_writing_shapefile_with_no_features()
        {
            DbaseFileHeader header = new DbaseFileHeader();
            header.AddColumn("X", 'C', 10, 0);
            ShapefileDataWriter writer = new ShapefileDataWriter(@"issue36") { Header = header };

            IList<IFeature> features = new List<IFeature>();
            writer.Write(features);            
        }
    }
}