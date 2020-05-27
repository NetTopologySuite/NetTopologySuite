using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Manages the input geometries for an overlay operation.
     * The second geometry is allowed to be null, 
     * to support for instance precision reduction.
     * 
     * @author Martin Davis
     *
     */
    class InputGeometry
    {

        //private static final PointLocator ptLocator = new PointLocator();

        private readonly Geometry[] geom;
        private IPointOnGeometryLocator ptLocatorA;
        private IPointOnGeometryLocator ptLocatorB;
        private readonly bool[] isCollapsed = new bool[2];

        public InputGeometry(Geometry geomA, Geometry geomB)
        {
            geom = new [] { geomA, geomB };
        }

        public bool isSingle()
        {
            return geom[1] == null;
        }

        public Dimension getDimension(int index)
        {
            if (geom[index] == null) return Dimension.False;
            return geom[index].Dimension;
        }

        public Geometry getGeometry(int geomIndex)
        {
            return geom[geomIndex];
        }

        public Envelope getEnvelope(int geomIndex)
        {
            return geom[geomIndex].EnvelopeInternal;
        }

        public bool isEmpty(int geomIndex)
        {
            return geom[geomIndex].IsEmpty;
        }

        public bool isArea(int geomIndex)
        {
            return geom[geomIndex] != null && geom[geomIndex].Dimension == Dimension.Surface;
        }

        /**
         * Gets the index of an input which is an area,
         * if one exists.
         * Otherwise returns -1.
         * If both inputs are areas, returns the index of the first one (0).
         * 
         * @return the index of an area input, or -1
         */
        public int getAreaIndex()
        {
            if (getDimension(0) == Dimension.Surface) return 0;
            if (getDimension(1) == Dimension.Surface) return 1;
            return -1;
        }

        public bool isLine(int geomIndex)
        {
            return geom[geomIndex].Dimension == Dimension.Curve;
        }

        public bool isAllPoints()
        {
            return getDimension(0) == Dimension.Point
                && geom[1] != null && getDimension(1) == Dimension.Point;
        }

        public bool hasPoints()
        {
            return getDimension(0) == Dimension.Point || getDimension(1) == Dimension.Point;
        }

        /**
         * Tests if an input geometry has edges.
         * This indicates that topology needs to be computed for them.
         * 
         * @param geomIndex
         * @return true if the input geometry has edges
         */
        public bool hasEdges(int geomIndex)
        {
            return geom[geomIndex] != null && geom[geomIndex].Dimension > Dimension.Point;
        }

        /**
         * Determines the location within an area geometry.
         * This allows disconnected edges to be fully 
         * located.  
         * 
         * @param geomIndex the index of the geometry
         * @param pt the coordinate to locate
         * @return the location of the coordinate
         * 
         * @see Location
         */
        public Location locatePointInArea(int geomIndex, Coordinate pt)
        {
            // Assert: only called if dimension(geomIndex) = 2

            if (isCollapsed[geomIndex])
                return Location.Exterior;


            //return ptLocator.locate(pt, geom[geomIndex]);

            //*
            // this check is required because IndexedPointInAreaLocator can't handle empty polygons
            if (getGeometry(geomIndex).IsEmpty
                || isCollapsed[geomIndex])
                return Location.Exterior;

            var ptLocator = getLocator(geomIndex);
            return ptLocator.Locate(pt);
            //*/
        }

        private IPointOnGeometryLocator getLocator(int geomIndex)
        {
            if (geomIndex == 0)
            {
                if (ptLocatorA == null)
                    ptLocatorA = new IndexedPointInAreaLocator(getGeometry(geomIndex));
                return ptLocatorA;
            }
            else
            {
                if (ptLocatorB == null)
                    ptLocatorB = new IndexedPointInAreaLocator(getGeometry(geomIndex));
                return ptLocatorB;
            }
        }

        public void setCollapsed(int geomIndex, bool isGeomCollapsed)
        {
            isCollapsed[geomIndex] = isGeomCollapsed;
        }


    }

}
