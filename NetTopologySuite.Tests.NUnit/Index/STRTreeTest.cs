/*
using System;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    public class STRTreeTest : SpatialIndexTest
    {

        [Test]
        public void TestCreateParentsFromVerticalSlice() {
    doTestCreateParentsFromVerticalSlice(3, 2, 2, 1);
    doTestCreateParentsFromVerticalSlice(4, 2, 2, 2);
    doTestCreateParentsFromVerticalSlice(5, 2, 2, 1);
  }

  public override ISpatialIndex<IExtents<Coordinate>, IGeometry<Coordinate>> CreateSpatialIndex() {
    return new StrTree<Coordinate, IGeometry<Coordinate>>(GeometryUtils.GeometryFactory);
  }

        [Test]
  public void TestDisallowedInserts() {
    StrTree<Coordinate, IGeometry<Coordinate>> t = new StrTree<Coordinate, IGeometry<Coordinate>>(GeometryUtils.GeometryFactory);
    t.Insert(new Envelope(0, 0, 0, 0), new Object());
    t.Insert(new Envelope(0, 0, 0, 0), new Object());
    t.Query(new Envelope());
    try {
      t.insert(new Envelope(0, 0, 0, 0), new Object());
      assertTrue(false);
    }
    catch (AssertionFailedException e) {
      assertTrue(true);
    }
  }

  public void TestQuery() {
    ArrayList geometries = new ArrayList();
    geometries.add(factory.createLineString(new Coordinate[]{
        new Coordinate(0, 0), new Coordinate(10, 10)}));
    geometries.add(factory.createLineString(new Coordinate[]{
        new Coordinate(20, 20), new Coordinate(30, 30)}));
    geometries.add(factory.createLineString(new Coordinate[]{
        new Coordinate(20, 20), new Coordinate(30, 30)}));
    STRtreeDemo.TestTree t = new STRtreeDemo.TestTree(4);
    for (Iterator i = geometries.iterator(); i.hasNext(); ) {
      Geometry<Coordinate> g = (Geometry<Coordinate>) i.next();
      t.Insert(g.getEnvelopeInternal(), new Object());
    }
    t.build();
    try {
      assertEquals(1, t.query(new Envelope(5, 6, 5, 6)).size());
      assertEquals(0, t.query(new Envelope(20, 30, 0, 10)).size());
      assertEquals(2, t.query(new Envelope(25, 26, 25, 26)).size());
      assertEquals(3, t.query(new Envelope(0, 100, 0, 100)).size());
    }
    catch (Throwable x) {
      STRtreeDemo.printSourceData(geometries, System.out);
      STRtreeDemo.printLevels(t, System.out);
      throw x;
    }
  }
        [Test]
  public void TestVerticalSlices() {
    doTestVerticalSlices(3, 2, 2, 1);
    doTestVerticalSlices(4, 2, 2, 2);
    doTestVerticalSlices(5, 3, 2, 1);
  }

  private void doTestCreateParentsFromVerticalSlice(int childCount,
      int nodeCapacity, int expectedChildrenPerParentBoundable,
      int expectedChildrenOfLastParent) {
    STRtreeDemo.TestTree t = new STRtreeDemo.TestTree(nodeCapacity);
    List parentBoundables
         = t.createParentBoundablesFromVerticalSlice(itemWrappers(childCount), 0);
    for (int i = 0; i < parentBoundables.size() - 1; i++) {//-1
      AbstractNode<Coordinate,IGeometry<Coordinate>> parentBoundable = (AbstractNode) parentBoundables.get(i);
      assertEquals(expectedChildrenPerParentBoundable, parentBoundable.getChildBoundables().size());
    }
    AbstractNode lastParent = (AbstractNode) parentBoundables.get(parentBoundables.size() - 1);
    assertEquals(expectedChildrenOfLastParent, lastParent.getChildBoundables().size());
  }

  private void doTestVerticalSlices(int itemCount, int sliceCount,
      int expectedBoundablesPerSlice, int expectedBoundablesOnLastSlice) {
    STRtreeDemo.TestTree t = new STRtreeDemo.TestTree(2);
    List[] slices =
        t.verticalSlices(itemWrappers(itemCount), sliceCount);
    assertEquals(sliceCount, slices.length);
    for (int i = 0; i < sliceCount - 1; i++) {//-1
      assertEquals(expectedBoundablesPerSlice, slices[i].size());
    }
    assertEquals(expectedBoundablesOnLastSlice, slices[sliceCount - 1].size());
  }

  private List itemWrappers(int size) {
    ArrayList itemWrappers = new ArrayList();
    for (int i = 0; i < size; i++) {
      itemWrappers.add(new ItemBoundable(new Envelope(0, 0, 0, 0), new Object()));
    }
    return itemWrappers;
  }
    }
}
*/