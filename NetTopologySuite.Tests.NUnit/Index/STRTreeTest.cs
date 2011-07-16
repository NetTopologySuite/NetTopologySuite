
using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    public class STRTreeTest : SpatialIndexTest
    {

        [Test]
        public void TestCreateParentsFromVerticalSlice()
        {
            DoTestCreateParentsFromVerticalSlice(3, 2, 2, 1);
            DoTestCreateParentsFromVerticalSlice(4, 2, 2, 2);
            DoTestCreateParentsFromVerticalSlice(5, 2, 2, 1);
        }

        public override ISpatialIndex<IExtents<Coordinate>, IGeometry<Coordinate>> CreateSpatialIndex()
        {
            return new StrTree<Coordinate, IGeometry<Coordinate>>(GeometryUtils.GeometryFactory);
        }


        [Test]
        public void TestDisallowedInserts()
        {
            StrTree<Coordinate, IGeometry<Coordinate>> t =
                new StrTree<Coordinate, IGeometry<Coordinate>>(GeometryUtils.GeometryFactory);
            t.Insert(GeometryUtils.GeometryFactory.CreatePoint2D(0, 0) as IGeometry<Coordinate>);
            t.Insert(GeometryUtils.GeometryFactory.CreatePoint2D(0, 0) as IGeometry<Coordinate>);
            t.Query(GeometryUtils.GeometryFactory.CreateGeometryCollection().Extents);
            try
            {
                t.Insert(GeometryUtils.GeometryFactory.CreatePoint2D(0, 0) as IGeometry<Coordinate>);
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }

        public void TestQuery()
        {
            List<IGeometry<Coordinate>> geometries = new List<IGeometry<Coordinate>>();
            geometries.Add(GeometryUtils.GeometryFactory.CreateLineString(new Coordinate[]
                                                        {
                                                            GeometryUtils.CoordFac.Create(0, 0), GeometryUtils.CoordFac.Create(10, 10)
                                                        }));
            geometries.Add(GeometryUtils.GeometryFactory.CreateLineString(new Coordinate[]
                                                        {
                                                            GeometryUtils.CoordFac.Create(20, 20), GeometryUtils.CoordFac.Create(30, 30)
                                                        }));
            geometries.Add(GeometryUtils.GeometryFactory.CreateLineString(new Coordinate[]
                                                        {
                                                            GeometryUtils.CoordFac.Create(20, 20), GeometryUtils.CoordFac.Create(30, 30)
                                                        }));
            STRTreeDemo.TestTree t = new STRTreeDemo.TestTree(4);
            foreach (IGeometry<Coordinate> g in geometries)
            {
                t.Insert(g);
            }

            try
            {
                Assert.AreEqual(1, t.Query((IExtents<Coordinate>)GeometryUtils.GeometryFactory.CreatePoint2D(5, 6).Extents));
                Assert.AreEqual(0, t.Query(GeometryUtils.GeometryFactory.CreatePolygon(
                    GeometryUtils.CoordSeqFac.Create(new Coordinate[]
                                                         {
                                                             GeometryUtils.CoordFac.Create(20,30), 
                                                             GeometryUtils.CoordFac.Create(20,10), 
                                                             GeometryUtils.CoordFac.Create(0,10), 
                                                             GeometryUtils.CoordFac.Create(0,30), 
                                                             GeometryUtils.CoordFac.Create(20,30)})
                    ).Extents));
                //Envelope(20, 30, 0, 10)).size());
                Assert.AreEqual(2, t.Query((IExtents<Coordinate>)GeometryUtils.GeometryFactory.CreatePoint2D(25, 26).Extents));
                Assert.AreEqual(3, t.Query((IExtents<Coordinate>)GeometryUtils.GeometryFactory.CreatePoint2D(0, 100).Extents));
            }
            catch (Exception x)
            {
                STRTreeDemo.PrintSourceData(geometries, Console.Out);
                STRTreeDemo.PrintLevels(t, Console.Out);
                throw x;
            }
        }

        [Test]
        public void TestVerticalSlices()
        {
            DoTestVerticalSlices(3, 2, 2, 1);
            DoTestVerticalSlices(4, 2, 2, 2);
            DoTestVerticalSlices(5, 3, 2, 1);
        }

        private void DoTestCreateParentsFromVerticalSlice(int childCount, int nodeCapacity, int expectedChildrenPerParentBoundable, int expectedChildrenOfLastParent)
        {
            STRTreeDemo.TestTree t = new STRTreeDemo.TestTree(nodeCapacity);
            IList<IBoundable<IExtents<Coordinate>>> parentBoundables
                = t.CreateParentBoundablesFromVerticalSlice(ItemWrappers(childCount), 0);
            List<IBoundable<IExtents<Coordinate>>> childBoundables;
            for (int i = 0; i < parentBoundables.Count - 1; i++)
            {
                //-1
                ISpatialIndexNode<IExtents<Coordinate>, IGeometry<Coordinate>> parentBoundable = parentBoundables[i] as ISpatialIndexNode<IExtents<Coordinate>, IGeometry<Coordinate>>;
                childBoundables = new List<IBoundable<IExtents<Coordinate>>>(parentBoundable.ChildBoundables);
                Assert.AreEqual(expectedChildrenPerParentBoundable, childBoundables.Count);
            }
            ISpatialIndexNode<IExtents<Coordinate>, IGeometry<Coordinate>> lastParent =
                parentBoundables[parentBoundables.Count - 1] as ISpatialIndexNode<IExtents<Coordinate>, IGeometry<Coordinate>>;
            childBoundables = new List<IBoundable<IExtents<Coordinate>>>(lastParent.ChildBoundables);
            Assert.AreEqual(expectedChildrenOfLastParent, childBoundables.Count);
        }

        private void DoTestVerticalSlices(int itemCount, int sliceCount, int expectedBoundablesPerSlice, int expectedBoundablesOnLastSlice)
        {
            STRTreeDemo.TestTree t = new STRTreeDemo.TestTree(2);
            IList<IList<IBoundable<IExtents<Coordinate>>>> slices =
                t.VerticalSlices(ItemWrappers(itemCount), sliceCount);
            Assert.AreEqual(sliceCount, slices.Count);
            for (int i = 0; i < sliceCount - 1; i++)
            {
                //-1
                Assert.AreEqual(expectedBoundablesPerSlice, slices[i].Count);
            }
            Assert.AreEqual(expectedBoundablesOnLastSlice, slices[sliceCount - 1].Count);
        }

        private IList<IBoundable<IExtents<Coordinate>>> ItemWrappers(int size)
        {
            List<IBoundable<IExtents<Coordinate>>> itemWrappers = new List<IBoundable<IExtents<Coordinate>>>();
            for (int i = 0; i < size; i++)
            {
                IGeometry<Coordinate> tmp = GeometryUtils.GeometryFactory.CreatePoint(
                    GeometryUtils.CoordFac.Create(0, 0));
                itemWrappers.Add(tmp);
            }
            return itemWrappers;
        }
    }

    public static class STRTreeDemo
    {

        static STRTreeDemo()
        {
        }

        public class TestTree : StrTree<Coordinate, IGeometry<Coordinate>>
        {

            public TestTree(int nodeCapacity)
                : base(GeometryUtils.GeometryFactory, nodeCapacity)
            {
            }

            public new IEnumerable<IBoundable<IExtents<Coordinate>>> BoundablesAtLevel(int level)
            {
                return base.BoundablesAtLevel(level);
            }

            public new ISpatialIndexNode<IExtents<Coordinate>, IGeometry<Coordinate>> Root
            {
                get { return base.Root; }
            }

            public new IList<IBoundable<IExtents<Coordinate>>> CreateParentBoundables(IList<IBoundable<IExtents<Coordinate>>> verticalSlice, int newLevel)
            {
                return base.CreateParentBoundables(verticalSlice, newLevel);
            }

            public new IList<IList<IBoundable<IExtents<Coordinate>>>> VerticalSlices(ICollection<IBoundable<IExtents<Coordinate>>> childBoundables, int size)
            {
                return base.VerticalSlices(childBoundables, size);
            }

            public new IList<IBoundable<IExtents<Coordinate>>> CreateParentBoundablesFromVerticalSlice(IList<IBoundable<IExtents<Coordinate>>> childBoundables, int newLevel)
            {
                return base.CreateParentBoundablesFromVerticalSlice(childBoundables, newLevel);
            }
        }

        /*

      private static void initTree(TestTree t, List sourceEnvelopes) {
        for (Iterator i = sourceEnvelopes.iterator(); i.hasNext(); ) {
          Envelope sourceEnvelope = (Envelope) i.next();
          t.insert(sourceEnvelope, sourceEnvelope);
        }
        t.build();
      }

      public static void main(String[] args) throws Exception {
        List envelopes = sourceData();
        TestTree t = new TestTree(NODE_CAPACITY);
        initTree(t, envelopes);
        PrintStream printStream = System.out;
        printSourceData(envelopes, printStream);
        printLevels(t, printStream);
      }
         */
        public static void PrintSourceData(IEnumerable<IGeometry<Coordinate>> sourceEnvelopes, TextWriter o)
        {
            o.WriteLine("============ Source Data ============\n");
            o.Write("GEOMETRYCOLLECTION(");
            Boolean first = true;
            foreach (IGeometry<Coordinate> geom in sourceEnvelopes)
            {
                IGeometry<Coordinate> g = geom.Extents.ToGeometry();
                if (first)
                {
                    first = false;
                }
                else
                {
                    o.Write(",");
                }
                o.Write(g);
            }
            o.WriteLine(")\n");
        }

        /*
          private static List sourceData() {
            ArrayList envelopes = new ArrayList();
            for (int i = 0; i < ItemCount; i++) {
              envelopes.add(RandomRectangle().getEnvelopeInternal());
            }
            return envelopes;
          }

          private const double Extent = 100;
          private const double MaxItemExtent = 15;
          private const double MinItemExtent = 3;
          private const int ItemCount = 20;
          private const int NODE_CAPACITY = 4;
          //private static GeometryFactory factory = new GeometryFactory();

          private static IPolygon<Coordinate> RandomRectangle() {
            double width = MinItemExtent + ((MaxItemExtent-MinItemExtent) * Math.random());
            double height = MinItemExtent + ((MaxItemExtent-MinItemExtent) * Math.random());
            double bottom = Extent * Math.random();
            double left = Extent * Math.random();
            double top = bottom + height;
            double right = left + width;
            return factory.createPolygon(factory.createLinearRing(new Coordinate[]{
                  new Coordinate(left, bottom), new Coordinate(right, bottom),
                  new Coordinate(right, top), new Coordinate(left, top),
                  new Coordinate(left, bottom) }), null);
          }
            */
        public static void PrintLevels(TestTree t, TextWriter o)
        {
            for (int i = 0; i <= t.Root.Level; i++)
            {
                PrintBoundables(t.BoundablesAtLevel(i), "Level " + i, o);
            }
        }

        public static void PrintBoundables(IEnumerable<IBoundable<IExtents<Coordinate>>> boundables, String title, TextWriter o)
        {
            o.WriteLine("============ " + title + " ============\n");
            o.Write("GEOMETRYCOLLECTION(");
            Boolean first = true;
            foreach (IBoundable<IExtents<Coordinate>> boundable in boundables)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    o.Write(",");
                }
                o.Write(ToString(boundable));
            }
            o.WriteLine(")\n");
        }

        private static String ToString(IBoundable<IExtents<Coordinate>> b)
        {
            IExtents<Coordinate> envelope = Extents(b);
            return "POLYGON(("
             + envelope.GetMin(Ordinates.X) + " "
             + envelope.GetMin(Ordinates.Y) + ", "
             + envelope.GetMin(Ordinates.X) + " "
             + envelope.GetMax(Ordinates.Y) + ", "
             + envelope.GetMax(Ordinates.X) + " "
             + envelope.GetMax(Ordinates.Y) + ", "
             + envelope.GetMax(Ordinates.X) + " "
             + envelope.GetMin(Ordinates.Y) + ","
             + envelope.GetMin(Ordinates.X) + " "
             + envelope.GetMin(Ordinates.Y) + "))";
        }

        private static IExtents<Coordinate> Extents(IBoundable<IExtents<Coordinate>> b)
        {
            return b.Bounds;
        }


    }
}

