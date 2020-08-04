using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NetTopologySuite.Clip;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Tests.NUnit.TestData;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Clip
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class RectangleClipPolygonPerformanceTest
    {

        static readonly GeometryFactory Factory = new GeometryFactory();
        private readonly bool _clip;

        public RectangleClipPolygonPerformanceTest(bool clip)
        {
            _clip = clip;
        }

        public static List<Geometry> readWKTFile(Stream fileStream)
        {
            var fileRdr = new WKTFileReader(fileStream, new WKTReader());
            return (List<Geometry>) fileRdr.Read();
        }

        [Test, Category("Stress")]
        public void Test()
        {
            Geometry data = LoadData();

            Console.WriteLine($"Dataset: # geometries = {data.NumGeometries}   # pts = {data.NumPoints}");

            var sw = new Stopwatch();
            sw.Start();
            RunClip(data);
            sw.Stop();

            Console.WriteLine("Time: {0}ms", sw.ElapsedMilliseconds);
        }

        private GeometryCollection LoadData()
        {
            List<Geometry> data = null;
            try
            {
                data = readWKTFile(EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.world.wkt"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return Factory.CreateGeometryCollection(data.ToArray());
        }

        private void RunClip(Geometry data)
        {
            var dataEnv = data.EnvelopeInternal;

            const int gridSize = 20;
            for (int x = -180; x < 180; x += gridSize)
            {
                for (int y = -90; y < 90; y += gridSize)
                {
                    var env = new Envelope(x, x + gridSize, y, y + gridSize);
                    var rect = Factory.ToGeometry(env);
                    RunClip(rect, data);
                }
            }
        }

        private void RunClip(Geometry rect, Geometry data)
        {
            for (int i = 0; i < data.NumGeometries; i++)
            {
                var geom = data.GetGeometryN(i);
                if (_clip) Clip(rect, geom);
                else RectangleIntersection(rect, geom);
            }
        }

        private Geometry Clip(Geometry rect, Geometry geom)
        {
            var clipper = new RectangleClipPolygon(rect);
            return clipper.Clip(geom);
        }

        private Geometry RectangleIntersection(Geometry rect, Geometry geom)
        {
            var env = rect.EnvelopeInternal;
            //Geometry result;
            if (env.Contains(geom.EnvelopeInternal))
            {
                return geom.Copy();
            }

            // Use intersects check first as that is faster
            if (!rect.Intersects(geom)) return null;

            return rect.Intersection(geom);
        }

        private Envelope Envelope(List<Geometry> world)
        {
            var env = new Envelope();
            foreach (var geom in world)
            {
                env.ExpandToInclude(geom.EnvelopeInternal);
            }

            return env;
        }
    }
}
