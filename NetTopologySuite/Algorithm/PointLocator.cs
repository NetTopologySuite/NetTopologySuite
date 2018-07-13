using System;
using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the topological relationship (<see cref="Location"/>) of a single point to a Geometry.
    /// </summary>
    /// <remarks>
    /// A <see cref="IBoundaryNodeRule"/> may be specified to control the evaluation of whether the point lies on the boundary or not
    /// The default rule is to use the <i>SFS Boundary Determination Rule</i>
    /// <para>
    /// Notes:
    /// <list Type="Bullet">
    /// <item><see cref="ILinearRing"/>s do not enclose any area - points inside the ring are still in the EXTERIOR of the ring.</item>
    /// </list>
    /// Instances of this class are not reentrant.
    /// </para>
    /// </remarks>
    public class PointLocator
    {
        // default is to use OGC SFS rule
        private readonly IBoundaryNodeRule _boundaryRule = BoundaryNodeRules.EndpointBoundaryRule; //OGC_SFS_BOUNDARY_RULE;

        private bool _isIn;            // true if the point lies in or on any Geometry element
        private int _numBoundaries;    // the number of sub-elements whose boundaries the point lies in

        /// <summary>
        /// Initializes a new instance of the <see cref="PointLocator"/> class.<para/>
        /// The default boundary rule is <see cref="BoundaryNodeRules.EndpointBoundaryRule"/>.
        /// </summary>
        public PointLocator() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointLocator"/> class using the provided
        /// <paramref name="boundaryRule">boundary rule</paramref>.
        /// </summary>
        /// <param name="boundaryRule">The boundary rule to use.</param>
        public PointLocator(IBoundaryNodeRule boundaryRule)
        {
            if (boundaryRule == null)
                throw new ArgumentException("Rule must be non-null");
            _boundaryRule = boundaryRule;
        }

        /// <summary>
        /// Convenience method to test a point for intersection with a Geometry
        /// </summary>
        /// <param name="p">The coordinate to test.</param>
        /// <param name="geom">The Geometry to test.</param>
        /// <returns><c>true</c> if the point is in the interior or boundary of the Geometry.</returns>
        public bool Intersects(Coordinate p, IGeometry geom)
        {
            return Locate(p, geom) != Location.Exterior;
        }

        /// <summary>
        /// Computes the topological relationship ({Location}) of a single point to a Geometry.
        /// It handles both single-element and multi-element Geometries.
        /// The algorithm for multi-part Geometries takes into account the boundaryDetermination rule.
        /// </summary>
        /// <returns>The Location of the point relative to the input Geometry.</returns>
        public Location Locate(Coordinate p, IGeometry geom)
        {
            if (geom.IsEmpty)
                return Location.Exterior;
            if (geom is ILineString)
                return LocateOnLineString(p, (ILineString) geom);
            if (geom is IPolygon)
                return LocateInPolygon(p, (IPolygon) geom);

            _isIn = false;
            _numBoundaries = 0;
            ComputeLocation(p, geom);
            if (_boundaryRule.IsInBoundary(_numBoundaries))
                return Location.Boundary;
            if (_numBoundaries > 0 || _isIn)
                return Location.Interior;

            return Location.Exterior;
        }

        private void ComputeLocation(Coordinate p, IGeometry geom)
        {
            if (geom is IPoint)
                UpdateLocationInfo(LocateOnPoint(p, (IPoint) geom));
            if (geom is ILineString)
                UpdateLocationInfo(LocateOnLineString(p, (ILineString) geom));
            else if(geom is IPolygon)
                UpdateLocationInfo(LocateInPolygon(p, (IPolygon) geom));
            else if(geom is IMultiLineString)
            {
                var ml = (IMultiLineString) geom;
                foreach (ILineString l in ml.Geometries)
                    UpdateLocationInfo(LocateOnLineString(p, l));
            }
            else if(geom is IMultiPolygon)
            {
                var mpoly = (IMultiPolygon) geom;
                foreach (IPolygon poly in mpoly.Geometries)
                    UpdateLocationInfo(LocateInPolygon(p, poly));
            }
            else if (geom is IGeometryCollection)
            {
                var geomi = new GeometryCollectionEnumerator((IGeometryCollection) geom);
                while(geomi.MoveNext())
                {
                    var g2 = (IGeometry) geomi.Current;
                    if (g2 != geom)
                        ComputeLocation(p, g2);
                }
            }
        }

        private void UpdateLocationInfo(Location loc)
        {
            if(loc == Location.Interior)
                _isIn = true;
            if(loc == Location.Boundary)
                _numBoundaries++;
        }

        private static Location LocateOnPoint(Coordinate p, IPoint pt)
        {
            // no point in doing envelope test, since equality test is just as fast

            var ptCoord = pt.Coordinate;
            if (ptCoord.Equals2D(p))
                return Location.Interior;
            return Location.Exterior;
        }

        private static Location LocateOnLineString(Coordinate p, ILineString l)
        {
            // bounding-box check
            if (!l.EnvelopeInternal.Intersects(p))
                return Location.Exterior;

            var pt = l.Coordinates;
            if(!l.IsClosed)
                if(p.Equals(pt[0]) || p.Equals(pt[pt.Length - 1]))
                    return Location.Boundary;
            if (PointLocation.IsOnLine(p, pt))
                return Location.Interior;
            return Location.Exterior;
        }

        private static Location LocateInPolygonRing(Coordinate p, ILinearRing ring)
        {
            // bounding-box check
            if (! ring.EnvelopeInternal.Intersects(p)) return Location.Exterior;

            return PointLocation.LocateInRing(p, ring.CoordinateSequence);
        }

        private Location LocateInPolygon(Coordinate p, IPolygon poly)
        {
            if (poly.IsEmpty)
                return Location.Exterior;
            var shell = poly.Shell;
            var shellLoc = LocateInPolygonRing(p, shell);
            if (shellLoc == Location.Exterior)
                return Location.Exterior;
            if (shellLoc == Location.Boundary)
                return Location.Boundary;
            // now test if the point lies in or on the holes
            foreach (ILinearRing hole in poly.InteriorRings)
            {
                var holeLoc = LocateInPolygonRing(p, hole);
                if (holeLoc == Location.Interior)
                    return Location.Exterior;
                if (holeLoc == Location.Boundary)
                    return Location.Boundary;
            }
            return Location.Interior;
        }
    }
}
