using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;

namespace NetTopologySuite.Noding.Snap
{
    public class SnapVertexIndex
    {

        private readonly double _snapTolerance;

        private readonly KdTree<Coordinate> _snapVertexIndex;

        public SnapVertexIndex(double snapTolerance)
        {
            _snapTolerance = snapTolerance;
            _snapVertexIndex = new KdTree<Coordinate>(snapTolerance);
        }

        public Coordinate Snap(Coordinate p)
        {
            /*
             * Inserting the coordinate snaps it to any existing
             * one within tolerance, or adds it.
             */
            var node = _snapVertexIndex.Insert(p);
            return node.Coordinate;
        }

        public double Tolerance { get =>_snapTolerance; }

    }
}
