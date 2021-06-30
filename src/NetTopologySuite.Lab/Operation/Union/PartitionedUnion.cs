using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// Unions a set of polygonal geometries by partitioning them
    /// into connected sets of polygons.<br/>
    /// This works best for a <i>sparse</i> set of polygons.
    /// Sparse means that if the geometries are partioned
    /// into connected sets, the number of sets
    /// is a significant fraction of the total number of geometries.<br/>
    /// The algorithm used provides performance and memory advantages
    /// over the <see cref="CascadedPolygonUnion"/> algorithm.<br/>
    /// It also has the advantage that it does not alter input geometries
    /// which do not intersect any other input geometry.
    /// <para/>
    /// Non-sparse sets are computed correctly, but may be slower than using cascaded union.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PartitionedUnion
    {
        public static Geometry Union(Geometry geoms)
        {
            var polys = PolygonExtracter.GetPolygons(geoms);
            var op = new PartitionedUnion(polys);
            return op.Union();
        }

        private readonly Geometry[] inputPolys;

        public PartitionedUnion(ICollection<Geometry> polys)
        {
            this.inputPolys = polys?.ToArray() ?? new Geometry[0];
        }

        public Geometry Union()
        {
            if (inputPolys.Length == 0)
                return null;

            var part = new SpatialPartition(inputPolys, new SPRelation(inputPolys));
    
            //--- compute union of each set
            var unionGeoms = new List<Geometry>(part.Count);
            int numSets = part.Count;
            for (int i = 0; i < numSets; i++) {
                var geom = Union(part, i);
                unionGeoms.Add(geom);
            }
            var geomFactory = inputPolys[0].Factory;
            return geomFactory.BuildGeometry(unionGeoms);
        }

        private Geometry Union(SpatialPartition part, int s)
        {
            //--- one geom in partition, so just copy it
            if (part.GetSize(s) == 1)
                return part.GetGeometry(s, 0);

            var setGeoms = new List<Geometry>();
            for (int i = 0; i < part.GetSize(s); i++)
            {
                setGeoms.Add(part.GetGeometry(s, i));
            }
            return CascadedPolygonUnion.Union(setGeoms);
        }

        private class SPRelation : SpatialPartition.IEquivalenceRelation
        {
            private readonly Geometry[] _inputPolys;

            public SPRelation(Geometry[] inputPolys)
            {
                _inputPolys = inputPolys;
            }

            public bool IsEquivalent(int i, int j)
            {
                var pg = PreparedGeometryFactory.Prepare(_inputPolys[i]);
                return pg.Intersects(_inputPolys[j]);
            }
        }
    }
}
