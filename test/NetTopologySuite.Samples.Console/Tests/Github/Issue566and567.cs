using System.Collections.Generic;
using System.Reflection;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue566and567
    {
        private readonly List<Geometry> _geometries = new List<Geometry>();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            
            var strm = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("NetTopologySuite.Samples.Tests.Github.Issue566and567.wkt");
            if (strm == null)
                Assert.Inconclusive("Resource stream not found");

            var rdr = new WKTFileReader(strm, new WKTReader());
            _geometries.AddRange(rdr.Read());
        }

        [Test, Explicit("Known to fail, need to check with JTS")]
        public void TestIssue566()
        {
            for (int i = 0; i < _geometries.Count; i += 2)
            {
                Assert.That(_geometries[i].IsValid, Is.Not.True);
                var geom = GeometryFixer.Fix(_geometries[i]);
                Assert.That(geom.GeometryType, Is.EqualTo(_geometries[i].GeometryType));
                Assert.That(geom.IsValid, Is.True);
            }
        }

        [Test, Ignore("More information needed")]
        public void TestIssue567()
        {
            for (int i = 0; i < _geometries.Count; i += 2)
            {
                Assert.That(_geometries[i].IsValid, Is.Not.True);
                var geom = GeometryFixer.Fix(_geometries[i]);
                Assert.That(CoordinateArrays.Dimension(geom.Coordinates), Is.EqualTo(CoordinateArrays.Dimension(_geometries[i].Coordinates)));
            }
        }
    }
}
