using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Union;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Union
{
    public class UnionPerfTester
    {
        public const int CASCADED = 1;
        public const int ITERATED = 2;
        public const int BUFFER0 = 3;
        public const int ORDERED = 4;

        public static void run(String testName, int testType, IList<IGeometry> polys)
        {
            UnionPerfTester test = new UnionPerfTester(polys);
            test.run(testName, testType);
        }

        public static void runAll(IList<IGeometry> polys)
        {
            UnionPerfTester test = new UnionPerfTester(polys);
            test.runAll();
        }

        private const int MAX_ITER = 1;

        private readonly IGeometryFactory _factory = new GeometryFactory();

        private readonly IList<IGeometry> _polys;

        public UnionPerfTester(IList<IGeometry> polys)
        {
            _polys = polys;
        }

        public void runAll()
        {
            Console.WriteLine("# items: " + _polys.Count);
            run("Cascaded", CASCADED, _polys);
//    run("Buffer-0", BUFFER0, polys);

            run("Iterated", ITERATED, _polys);

        }

        public void run(String testName, int testType)
        {
            Console.WriteLine();
            Console.WriteLine("======= Union Algorithm: " + testName + " ===========");

            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < MAX_ITER; i++)
            {
                IGeometry union = null;
                switch (testType)
                {
                    case CASCADED:
                        union = unionCascaded(_polys);
                        break;
                    case ITERATED:
                        union = unionAllSimple(_polys);
                        break;
                    case BUFFER0:
                        union = unionAllBuffer(_polys);
                        break;
                }

//    	printFormatted(union);

            }
            Console.WriteLine("Finished in " + sw.ElapsedMilliseconds);
        }

        private void printFormatted(IGeometry geom)
        {
            WKTWriter writer = new WKTWriter();
            Console.WriteLine(writer.WriteFormatted(geom));
        }

        public IGeometry unionAllSimple(IList<IGeometry> geoms)
        {
            IGeometry unionAll = null;
            int count = 0;
            foreach (var geom in geoms)
            {

                if (unionAll == null)
                {
                    unionAll = (IGeometry) geom.Copy();
                }
                else
                {
                    unionAll = unionAll.Union(geom);
                }

                count++;
                if (count%100 == 0)
                {
                    Console.Write(".");
//        System.out.println("Adding geom #" + count);
                }
            }
            return unionAll;
        }

        public IGeometry unionAllBuffer(IList<IGeometry> geoms)
        {

            var gColl = _factory.BuildGeometry(geoms);
            var unionAll = gColl.Buffer(0.0);
            return unionAll;
        }

        public IGeometry unionCascaded(IList<IGeometry> geoms)
        {
            return CascadedPolygonUnion.Union(geoms);
        }

        private void printItemEnvelopes(IList tree)
        {
            Envelope itemEnv = new Envelope();
            foreach (object o in tree)
            {
                if (o is IList)
                {
                    printItemEnvelopes((IList) o);
                }
                else if (o is IGeometry)
                {
                    itemEnv.ExpandToInclude(((IGeometry) o).EnvelopeInternal);
                }
            }
            Console.WriteLine(_factory.ToGeometry(itemEnv).ToString());
        }
    }
}