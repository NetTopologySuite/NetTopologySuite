using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Manages the input geometries for an overlay operation.
    /// The second geometry is allowed to be null, 
    /// to support for instance precision reduction.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class InputGeometry
    {

        //private static final PointLocator ptLocator = new PointLocator();

        private readonly Geometry[] _geom;
        private IPointOnGeometryLocator _ptLocatorA;
        private IPointOnGeometryLocator _ptLocatorB;
        private readonly bool[] _isCollapsed = new bool[2];

        public InputGeometry(Geometry geomA, Geometry geomB)
        {
            _geom = new [] { geomA, geomB };
        }

        public bool IsSingle
        {
            get => _geom[1] == null;
        }

        public Dimension GetDimension(int index)
        {
            if (_geom[index] == null) return Dimension.False;
            return _geom[index].Dimension;
        }

        public Geometry GetGeometry(int geomIndex)
        {
            return _geom[geomIndex];
        }

        public Envelope GetEnvelope(int geomIndex)
        {
            return _geom[geomIndex].EnvelopeInternal;
        }

        public bool IsEmpty(int geomIndex)
        {
            return _geom[geomIndex].IsEmpty;
        }

        public bool IsArea(int geomIndex)
        {
            return _geom[geomIndex] != null && _geom[geomIndex].Dimension == Dimension.Surface;
        }

        /// <summary>
        /// Gets the index of an input which is an area,
        /// if one exists.
        /// Otherwise returns -1.
        /// </summary>
        /// <returns>The index of an area input, or -1</returns>
        public int GetAreaIndex()
        {
            if (GetDimension(0) == Dimension.Surface) return 0;
            if (GetDimension(1) == Dimension.Surface) return 1;
            return -1;
        }

        public bool IsLine(int geomIndex)
        {
            return GetDimension(geomIndex) == Dimension.Curve;
        }

        public bool IsAllPoints
        {
            get => GetDimension(0) == Dimension.Point
                && _geom[1] != null && GetDimension(1) == Dimension.Point;
        }

        public bool HasPoints
        {
            get => GetDimension(0) == Dimension.Point || GetDimension(1) == Dimension.Point;
        }

        /// <summary>
        /// Tests if an input geometry has edges.
        /// This indicates that topology needs to be computed for them.
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns><c>true</c> if the input geometry has edges</returns>
        public bool HasEdges(int geomIndex)
        {
            return _geom[geomIndex] != null && _geom[geomIndex].Dimension > Dimension.Point;
        }

        /// <summary>
        /// Determines the location within an area geometry.
        /// This allows disconnected edges to be fully 
        /// located. 
        /// </summary>
        /// <param name="geomIndex">The index of the geometry</param>
        /// <param name="pt">The coordinate to locate</param>
        /// <returns>The location of the coordinate</returns>
        /// <seealso cref="Location"/>
        public Location LocatePointInArea(int geomIndex, Coordinate pt)
        {
            // Assert: only called if dimension(geomIndex) = 2

            if (_isCollapsed[geomIndex])
                return Location.Exterior;


            //return ptLocator.locate(pt, geom[geomIndex]);

            //*
            // this check is required because IndexedPointInAreaLocator can't handle empty polygons
            if (GetGeometry(geomIndex).IsEmpty
                || _isCollapsed[geomIndex])
                return Location.Exterior;

            var ptLocator = GetLocator(geomIndex);
            return ptLocator.Locate(pt);
            //*/
        }

        private IPointOnGeometryLocator GetLocator(int geomIndex)
        {
            if (geomIndex == 0)
            {
                if (_ptLocatorA == null)
                    _ptLocatorA = new IndexedPointInAreaLocator(GetGeometry(geomIndex));
                return _ptLocatorA;
            }
            else
            {
                if (_ptLocatorB == null)
                    _ptLocatorB = new IndexedPointInAreaLocator(GetGeometry(geomIndex));
                return _ptLocatorB;
            }
        }

        public void SetCollapsed(int geomIndex, bool isGeomCollapsed)
        {
            _isCollapsed[geomIndex] = isGeomCollapsed;
        }


    }

}
