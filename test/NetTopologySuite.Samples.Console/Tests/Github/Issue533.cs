using NetTopologySuite.IO;
using NetTopologySuite.Operation.Valid;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue533
    {
        [Test, Explicit]
        public void Test()
        {
            var rdr = new WKTReader(NtsGeometryServices.Instance);
            var polygon1 = rdr.Read("POLYGON ((-1.426955 -1.9145, -1.426954 -1.9145, -3.65313 -1.9145, -3.65313 1.9145, -1.426954 1.9145, -1.426955 1.9145, -1.426955 -1.9145))");
            var op = new IsValidOp(polygon1);
            Assert.That(op.IsValid, op.ValidationError.ToString);
            var polygon2 = rdr.Read("POLYGON ((-1.426955 1.9145, -1.426954 1.9145, -1.426955 1.9145, -3.65313 1.9145, -3.65313 -1.9145, -1.426954 -1.9145, -1.426955 -1.9145, -1.426955 -1.9145, -1.426955 1.9145))");
            op = new IsValidOp(polygon2);
            Assert.That(op.IsValid, op.ValidationError.ToString);

            Assert.That(() => polygon1.EqualsTopologically(polygon2), Throws.Nothing);
        }
    }
}
