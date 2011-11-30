using System.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.IO
{
    [TestFixture]
    [Ignore("Sample file(s) not published")]
    public class ShapeFileInvalidHeaderTest
    {
        private readonly string _invalidPath = CombinePaths("..", "..", "..", "NetTopologySuite.Samples.Shapefiles", "invalidheader.shp");

        [Test]
        public void TestInvalidShapeFile()
        {
            /*
            var s = new NetTopologySuite.IO.ShapefileReader(_invalidPath);
            var sh = s.Header;
            var g = s.ReadAll();
            */
            var dbf = Path.ChangeExtension(_invalidPath, ".dbf");
            var d = new NetTopologySuite.IO.DbaseFileReader(dbf);

            var de = d.GetEnumerator();
            Assert.IsNull(de.Current);
            de.MoveNext();
            Assert.IsNotNull(de.Current);
        }
        
        public static string CombinePaths(params string[] pathComponents)
        {
            if (pathComponents == null)
                return string.Empty;

            var ret = pathComponents[0];
            for (var i = 1; i < pathComponents.Length; i++)
                ret = Path.Combine(ret, pathComponents[i]);
            return ret;
        }

    }
}