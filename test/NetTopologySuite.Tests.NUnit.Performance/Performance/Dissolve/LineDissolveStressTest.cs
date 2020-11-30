using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NetTopologySuite.Dissolve;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Tests.NUnit.TestData;
using NetTopologySuite.Tests.NUnit.Utilities;
using NetTopologySuite.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Dissolve
{
    [TestFixture, Category("Stress")]
    public class LineDissolveStressTest
    {
        [Test, Ignore("takes ages to complete")]
        public void Test()
        {
            Trace.WriteLine("Loading data...");
            var data = IOUtil.ReadWKTFile(new StreamReader(EmbeddedResourceManager.GetResourceStream(
                "NetTopologySuite.Tests.NUnit.TestData.world.wkt")));

            const int maxTimes = 5;
            for (int i = 1; i <= maxTimes; i++)
            {
                Trace.WriteLine(string.Format("Iteration {0} of {1} started", i, maxTimes));
                RunDissolverWorld(data);
                RunBruteForceWorld(data);
                Trace.WriteLine(string.Format("Iteration {0} of {1} terminated", i, maxTimes));
                Trace.WriteLine(Environment.NewLine);
            }

            Trace.WriteLine("Test terminated");
        }

        private void RunDissolverWorld(IList<Geometry> data)
        {
            var dis = new LineDissolver();
            dis.Add(data);
            var result = dis.GetResult();
            Trace.WriteLine("RunDissolverWorld");
            Trace.WriteLine(Memory.TotalString);
            // Trace.WriteLine(String.Format("Result: {0}", result));
        }

        private void RunBruteForceWorld(IList<Geometry> data)
        {
            var result = DissolveLines(data);
            Trace.WriteLine("RunBruteForceWorld");
            Trace.WriteLine(Memory.TotalString);
            // Trace.WriteLine(String.Format("Result: {0}", result));
        }

        private Geometry DissolveLines(IList<Geometry> lines)
        {
            var linesGeom = ExtractLines(lines);
            return DissolveLines(linesGeom);
        }

        private static Geometry DissolveLines(Geometry lines)
        {
            var dissolved = lines.Union();
            var merger = new LineMerger();
            merger.Add(dissolved);
            var mergedColl = merger.GetMergedLineStrings();
            var merged = lines.Factory.BuildGeometry(mergedColl);
            return merged;
        }

        private static Geometry ExtractLines(ICollection<Geometry> geoms)
        {
            GeometryFactory factory = null;
            var lines = new List<Geometry>();
            foreach (var g in geoms)
            {
                if (factory == null)
                    factory = g.Factory;
                var coll = LinearComponentExtracter.GetLines(g);
                lines.AddRange(coll);
            }

            if (factory == null)
                return null;
            return factory.BuildGeometry(geoms);
        }
    }
}
