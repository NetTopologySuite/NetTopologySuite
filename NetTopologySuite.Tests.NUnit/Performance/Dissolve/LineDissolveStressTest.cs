using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Dissolve;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Tests.NUnit.TestData;
using NetTopologySuite.Utilities;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Performance.Dissolve
{
    [TestFixtureAttribute, CategoryAttribute("Stress")]
    public class LineDissolveStressTest
    {
        [TestAttribute, Ignore("takes ages to complete")]
        public void Test()
        {
            Trace.WriteLine("Loading data...");
            var data = GeometryUtils.ReadWKTFile(EmbeddedResourceManager.GetResourceStream(
                "NetTopologySuite.Tests.NUnit.TestData.world.wkt"));
            const int maxTimes = 5;
            for (var i = 1; i <= maxTimes; i++)
            {
                Trace.WriteLine(string.Format("Iteration {0} of {1} started", i, maxTimes));
                RunDissolverWorld(data);
                RunBruteForceWorld(data);
                Trace.WriteLine(string.Format("Iteration {0} of {1} terminated", i, maxTimes));
                Trace.WriteLine(Environment.NewLine);
            }
            Trace.WriteLine("Test terminated");
        }
        private void RunDissolverWorld(IList<IGeometry> data)
        {
            var dis = new LineDissolver();
            dis.Add(data);
            var result = dis.GetResult();
            Trace.WriteLine("RunDissolverWorld");
            Trace.WriteLine(Memory.TotalString);
            // Trace.WriteLine(String.Format("Result: {0}", result));
        }
        private void RunBruteForceWorld(IList<IGeometry> data)
        {
            var result = DissolveLines(data);
            Trace.WriteLine("RunBruteForceWorld");
            Trace.WriteLine(Memory.TotalString);
            // Trace.WriteLine(String.Format("Result: {0}", result));
        }
        private IGeometry DissolveLines(IList<IGeometry> lines)
        {
            var linesGeom = ExtractLines(lines);
            return DissolveLines(linesGeom);
        }
        private static IGeometry DissolveLines(IGeometry lines)
        {
            var dissolved = lines.Union();
            var merger = new LineMerger();
            merger.Add(dissolved);
            var mergedColl = merger.GetMergedLineStrings();
            var merged = lines.Factory.BuildGeometry(mergedColl);
            return merged;
        }
        private static IGeometry ExtractLines(ICollection<IGeometry> geoms)
        {
            IGeometryFactory factory = null;
            var lines = new List<IGeometry>();
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
