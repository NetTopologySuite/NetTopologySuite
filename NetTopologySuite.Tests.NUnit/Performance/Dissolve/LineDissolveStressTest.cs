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
            string filePath = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile(
                "NetTopologySuite.Tests.NUnit.TestData.world.wkt");
            IList<IGeometry> data = GeometryUtils.ReadWKTFile(filePath);

            const int maxTimes = 5;
            for (int i = 1; i <= maxTimes; i++)
            {
                Trace.WriteLine(String.Format("Iteration {0} of {1} started", i, maxTimes));
                RunDissolverWorld(data);
                RunBruteForceWorld(data);
                Trace.WriteLine(String.Format("Iteration {0} of {1} terminated", i, maxTimes));
                Trace.WriteLine(Environment.NewLine);
            }

            EmbeddedResourceManager.CleanUpTempFile(filePath);
            Trace.WriteLine("Test terminated");
        }

        private void RunDissolverWorld(IList<IGeometry> data)
        {
            LineDissolver dis = new LineDissolver();
            dis.Add(data);
            IGeometry result = dis.GetResult();
            Trace.WriteLine("RunDissolverWorld");
            Trace.WriteLine(Memory.TotalString);
            // Trace.WriteLine(String.Format("Result: {0}", result));
        }

        private void RunBruteForceWorld(IList<IGeometry> data)
        {
            IGeometry result = DissolveLines(data);
            Trace.WriteLine("RunBruteForceWorld");
            Trace.WriteLine(Memory.TotalString);
            // Trace.WriteLine(String.Format("Result: {0}", result));
        }

        private IGeometry DissolveLines(IList<IGeometry> lines)
        {
            IGeometry linesGeom = ExtractLines(lines);
            return DissolveLines(linesGeom);
        }

        private static IGeometry DissolveLines(IGeometry lines)
        {
            IGeometry dissolved = lines.Union();
            LineMerger merger = new LineMerger();
            merger.Add(dissolved);
            IList<IGeometry> mergedColl = merger.GetMergedLineStrings();
            IGeometry merged = lines.Factory.BuildGeometry(mergedColl);
            return merged;
        }

        private static IGeometry ExtractLines(ICollection<IGeometry> geoms)
        {
            IGeometryFactory factory = null;
            List<IGeometry> lines = new List<IGeometry>();
            foreach (IGeometry g in geoms)
            {
                if (factory == null)
                    factory = g.Factory;
                ICollection<IGeometry> coll = LinearComponentExtracter.GetLines(g);
                lines.AddRange(coll);
            }

            if (factory == null)
                return null;
            return factory.BuildGeometry(geoms);
        }
    }
}
