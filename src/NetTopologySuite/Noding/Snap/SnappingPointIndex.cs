using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;

namespace NetTopologySuite.Noding.Snap
{
    public class SnappingPointIndex
    {
        private readonly double _snapTolerance;
        private readonly KdTree<Coordinate> _snapPointIndex;

        public SnappingPointIndex(double snapTolerance)
        {
            _snapTolerance = snapTolerance;
            _snapPointIndex = new KdTree<Coordinate>(snapTolerance);
        }

        public Coordinate Snap(Coordinate p)
        {
            /*
             * Inserting the coordinate snaps it to any existing
             * one within tolerance, or adds it if not.
             */
            var node = _snapPointIndex.Insert(p);
            return node.Coordinate;
        }

        public double Tolerance { get =>_snapTolerance; }

    }
}
