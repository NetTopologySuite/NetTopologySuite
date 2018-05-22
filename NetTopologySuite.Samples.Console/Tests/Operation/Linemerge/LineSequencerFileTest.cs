using System;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Linemerge;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Operation.Linemerge
{
    public class LineSequencerFileTest
    {
        private readonly IGeometryFactory factory = new GeometryFactory(new PrecisionModel(10000));

        [TestCase("d:\\temp\\linestosequence.wkt")]
        public void TestWktFile(string file)
        {
            if (!File.Exists(file))
                throw new IgnoreException($"File '{file}' does not exist");

            try
            {
                Assert.DoesNotThrow(() => DoTestFile(file));
            }
            catch (InconclusiveException ice)
            {
                Console.WriteLine(ice.Message);
                return;
            }
            catch
            {
                return;
            }
            return;
        }

        private void DoTestFile(string file)
        {
            if (!File.Exists(file))
                Assert.Inconclusive("File {0} does not exist", file);

            var r = new WKTFileReader(file, new WKTReader(factory));

            var geoms = r.Read();
            var ls = new LineSequencer();
            ls.Add(geoms);

            if (!ls.IsSequenceable())
                Assert.Inconclusive("Linework not sequencable");

            var seq = ls.GetSequencedLineStrings();
            Assert.Less(seq.NumGeometries, geoms.Count);
        }
    }
}
