using System;
using GeoAPI.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    //Tests are exposed by SpatialIndexTestCase type
    public class QuadtreeTestCase : SpatialIndexTestCase
    {
        protected override ISpatialIndex<object> CreateSpatialIndex()
        {
            return new Quadtree<object>();
        }
    }
}
