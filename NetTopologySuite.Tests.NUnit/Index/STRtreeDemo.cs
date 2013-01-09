using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    public class STRtreeDemo
    {
        private static double EXTENT = 100;
        private static double MAX_ITEM_EXTENT = 15;
        private static double MIN_ITEM_EXTENT = 3;
        private static int ITEM_COUNT = 20;
        private static int NODE_CAPACITY = 4;
        private static GeometryFactory factory = new GeometryFactory();

        public STRtreeDemo()
        {
            var envelopes = SourceData();
            TestTree t = new TestTree(NODE_CAPACITY);
            InitTree(t, envelopes);
            PrintSourceData(envelopes);
            PrintLevels(t);
        }

        public class TestTree : STRtree<object>
        {
            public TestTree(int nodeCapacity)
                : base(nodeCapacity)
            {
            }

            public IList<IBoundable<Envelope, object>> BoundablesAtLevel(int level) { return base.BoundablesAtLevel(level); }

            public AbstractNode<Envelope, object> Root { get { return base.Root; } }

            public IList<IBoundable<Envelope, object>> CreateParentBoundables(IList<IBoundable<Envelope, object>> verticalSlice, int newLevel)
            {
                return base.CreateParentBoundables(verticalSlice, newLevel);
            }
            public IList<IBoundable<Envelope, object>>[] VerticalSlices(IList<IBoundable<Envelope, object>> childBoundables, int size)
            {
                return base.VerticalSlices(childBoundables, size);
            }
            public IList<IBoundable<Envelope, object>> CreateParentBoundablesFromVerticalSlice(IList<IBoundable<Envelope, object>> childBoundables, int newLevel)
            {
                return base.CreateParentBoundablesFromVerticalSlice(childBoundables, newLevel);
            }
        }

        private static void InitTree(TestTree t, IList<Envelope> sourceEnvelopes)
        {
            foreach (var sourceEnvelope in sourceEnvelopes)
            {
                t.Insert(sourceEnvelope, sourceEnvelope);
            }
            t.Build();
        }

        public static void PrintSourceData(IList<Envelope> sourceEnvelopes)
        {
            Console.WriteLine("============ Source Data ============\n");
            Console.Write("GEOMETRYCOLLECTION(");
            bool first = true;
            foreach (var e in sourceEnvelopes)
            {
                IGeometry g = factory.CreatePolygon(factory.CreateLinearRing(new Coordinate[] {
                new Coordinate(e.MinX, e.MinY), new Coordinate(e.MinX, e.MaxY),
                new Coordinate(e.MaxX, e.MaxY), new Coordinate(e.MaxX, e.MinY),
                new Coordinate(e.MinX, e.MinY) }), null);
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.Write(",");
                }
                Console.Write(g);
            }
            Console.WriteLine(")\n");
        }

        private static IList<Envelope> SourceData()
        {
            var envelopes = new List<Envelope>();
            for (int i = 0; i < ITEM_COUNT; i++)
            {
                envelopes.Add(RandomRectangle().EnvelopeInternal);
            }
            return envelopes;
        }

        private static IPolygon RandomRectangle()
        {
            var random = new Random();

            double width = MIN_ITEM_EXTENT + ((MAX_ITEM_EXTENT - MIN_ITEM_EXTENT) * random.NextDouble());
            double height = MIN_ITEM_EXTENT + ((MAX_ITEM_EXTENT - MIN_ITEM_EXTENT) * random.NextDouble());
            double bottom = EXTENT * random.NextDouble();
            double left = EXTENT * random.NextDouble();
            double top = bottom + height;
            double right = left + width;
            return factory.CreatePolygon(factory.CreateLinearRing(new Coordinate[]{
                new Coordinate(left, bottom), new Coordinate(right, bottom),
                new Coordinate(right, top), new Coordinate(left, top),
                new Coordinate(left, bottom) }), null);
        }

        public static void PrintLevels(TestTree t)
        {
            for (int i = 0; i <= t.Root.Level; i++)
            {
                PrintBoundables(t.BoundablesAtLevel(i), "Level " + i);
            }
        }

        public static void PrintBoundables(IList<IBoundable<Envelope, object>> boundables, string title)
        {
            Console.WriteLine("============ " + title + " ============\n");
            Console.Write("GEOMETRYCOLLECTION(");
            bool first = true;
            foreach (IBoundable<Envelope, object> boundable in boundables)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Console.Write(",");
                }
                Console.Write(ToString(boundable));
            }
            Console.WriteLine(")\n");
        }

        private static String ToString(IBoundable<Envelope, object> b)
        {
            return "POLYGON(("
                    + Envelope(b).MinX + " "
                    + Envelope(b).MinY + ", "
                    + Envelope(b).MinX + " "
                    + Envelope(b).MaxY + ", "
                    + Envelope(b).MaxX + " "
                    + Envelope(b).MaxY + ", "
                    + Envelope(b).MaxX + " "
                    + Envelope(b).MinY + ","
                    + Envelope(b).MinX + " "
                    + Envelope(b).MinY + "))";
        }

        private static Envelope Envelope(IBoundable<Envelope, object> b)
        {
            return (Envelope)b.Bounds;
        }
    }
}
