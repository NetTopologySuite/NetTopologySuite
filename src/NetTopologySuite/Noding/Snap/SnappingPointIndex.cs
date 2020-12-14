using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;

namespace NetTopologySuite.Noding.Snap
{
    /// <summary>
    /// An index providing fast creation and lookup of snap points.
    /// </summary>
    public sealed class SnappingPointIndex
    {
        /// <summary>
        /// Since points are added incrementally, this index needs to be dynamic.
        /// This class also makes use of the KdTree support for a tolerance distance
        /// for point equality.
        /// </summary>
        private readonly KdTree<Coordinate> _snapPointIndex;

        /// <summary>
        /// Creates a snap point index using a specified distance tolerance.
        /// </summary>
        /// <param name="snapTolerance">Points are snapped if within this distance</param>
        public SnappingPointIndex(double snapTolerance)
        {
            Tolerance = snapTolerance;
            _snapPointIndex = new KdTree<Coordinate>(snapTolerance);
        }

        /// <summary>
        /// Snaps a coordinate to an existing snap point,
        /// if it is within the snap tolerance distance.
        /// Otherwise adds the coordinate to the snap point index.
        /// </summary>
        /// <param name="p">The point to snap</param>
        /// <returns>The point it snapped to, or the input point</returns>
        public Coordinate Snap(Coordinate p)
        {
            /*
             * Inserting the coordinate snaps it to any existing
             * one within tolerance, or adds it if not.
             */
            var node = _snapPointIndex.Insert(p);
            return node.Coordinate;
        }

        public double Tolerance { get; }

    }
}
