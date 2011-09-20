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
    /// The default rule is to use the the <i>SFS Boundary Determination Rule</i>
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
        /// Initializes a new instance of the <see cref="PointLocator"/> class.
        /// </summary>
        public PointLocator() { }

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
                return Locate(p, (ILineString) geom);
            if (geom is IPolygon) 
                return Locate(p, (IPolygon) geom);

            _isIn = false;
            _numBoundaries = 0;
            ComputeLocation(p, geom);
            if (_boundaryRule.IsInBoundary(_numBoundaries))
                return Location.Boundary;
            if (_numBoundaries > 0 || _isIn)
                return Location.Interior;

            return Location.Exterior;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="geom"></param>
        private void ComputeLocation(Coordinate p, IGeometry geom)
        {
            if (geom is IPoint)
                UpdateLocationInfo(Locate(p, (IPoint) geom));
            if (geom is ILineString) 
                UpdateLocationInfo(Locate(p, (ILineString) geom));                                  
            else if(geom is Polygon) 
                UpdateLocationInfo(Locate(p, (IPolygon) geom));            
            else if(geom is IMultiLineString) 
            {
                IMultiLineString ml = (IMultiLineString) geom;
                foreach (ILineString l in ml.Geometries)                     
                    UpdateLocationInfo(Locate(p, l));                
            }
            else if(geom is IMultiPolygon)
            {
                IMultiPolygon mpoly = (IMultiPolygon) geom;
                foreach (IPolygon poly in mpoly.Geometries) 
                    UpdateLocationInfo(Locate(p, poly));
            }
            else if (geom is IGeometryCollection) 
            {
                IEnumerator geomi = new GeometryCollectionEnumerator((IGeometryCollection) geom);
                while(geomi.MoveNext()) 
                {
                    IGeometry g2 = (IGeometry) geomi.Current;
                    if (g2 != geom)
                        ComputeLocation(p, g2);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loc"></param>
        private void UpdateLocationInfo(Location loc)
        {
            if(loc == Location.Interior) 
                _isIn = true;
            if(loc == Location.Boundary) 
                _numBoundaries++;
        }

        private static Location Locate(Coordinate p, IPoint pt)
        {
            // no point in doing envelope test, since equality test is just as fast

            Coordinate ptCoord = pt.Coordinate;
            if (ptCoord.Equals2D(p))
                return Location.Interior;
            return Location.Exterior;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        private static Location Locate(Coordinate p, ILineString l)
        {
            // bounding-box check
            if (!l.EnvelopeInternal.Intersects(p)) 
                return Location.Exterior;
  	

            Coordinate[] pt = l.Coordinates;
            if(!l.IsClosed)
                if(p.Equals(pt[0]) || p.Equals(pt[pt.Length - 1]))
                    return Location.Boundary;                            
            if (CGAlgorithms.IsOnLine(p, pt))
                return Location.Interior;
            return Location.Exterior;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ring"></param>
        /// <returns></returns>
        private static Location LocateInPolygonRing(Coordinate p, ILinearRing ring)
        {
  	        // bounding-box check
  	        if (! ring.EnvelopeInternal.Intersects(p)) return Location.Exterior;

  	        return CGAlgorithms.LocatePointInRing(p, ring.Coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="poly"></param>
        /// <returns></returns>
        private Location Locate(Coordinate p, IPolygon poly)
        {
            if (poly.IsEmpty) 
                return Location.Exterior;
            ILinearRing shell = poly.Shell;
            Location shellLoc = LocateInPolygonRing(p, shell);
            if (shellLoc == Location.Exterior) 
                return Location.Exterior;
            if (shellLoc == Location.Boundary) 
                return Location.Boundary;
            // now test if the point lies in or on the holes
            foreach (ILinearRing hole in poly.InteriorRings)
            {
                Location holeLoc = LocateInPolygonRing(p, hole);
                if (holeLoc == Location.Interior) 
                    return Location.Exterior;
                if (holeLoc == Location.Boundary) 
                    return Location.Boundary;
            }
            return Location.Interior;
        }
    }
}
