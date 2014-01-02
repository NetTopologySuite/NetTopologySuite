using System;
using System.Diagnostics;
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

        [Test, Explicit("missing input file 'd:\\temp\\linestosequence.wkt'")]
        public void TestFiles()
        {
            var allPass = true;
            allPass &= TestFile("d:\\temp\\linestosequence.wkt");

            Assert.IsTrue(allPass);
        }

        private bool TestFile(string file)
        {
            try
            {
                Assert.DoesNotThrow(() => DoTestFile(file));
            }
            catch (InconclusiveException ice)
            {
                Console.WriteLine(ice.Message);
                return true;
            }
            catch
            {
                return false;
            }
            return true;
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