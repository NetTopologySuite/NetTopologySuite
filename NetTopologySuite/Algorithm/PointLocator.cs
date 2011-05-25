using System;
using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the topological relationship (Location) of a single point to a Geometry.
    /// The algorithm obeys the SFS boundaryDetermination rule to correctly determine
    /// whether the point lies on the boundary or not.
    /// Note that instances of this class are not reentrant.
    /// </summary>
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
        public bool Intersects(ICoordinate p, IGeometry geom)
        {
            return Locate(p, geom) != Locations.Exterior;
        }

        /// <summary> 
        /// Computes the topological relationship ({Location}) of a single point to a Geometry.
        /// It handles both single-element and multi-element Geometries.
        /// The algorithm for multi-part Geometries takes into account the boundaryDetermination rule.
        /// </summary>
        /// <returns>The Location of the point relative to the input Geometry.</returns>
        public Locations Locate(ICoordinate p, IGeometry geom)
        {
            if (geom.IsEmpty)
                return Locations.Exterior;
            if (geom is ILineString) 
                return Locate(p, (ILineString) geom);
            if (geom is IPolygon) 
                return Locate(p, (IPolygon) geom);

            _isIn = false;
            _numBoundaries = 0;
            ComputeLocation(p, geom);
            if (_boundaryRule.IsInBoundary(_numBoundaries))
                return Locations.Boundary;
            if (_numBoundaries > 0 || _isIn)
                return Locations.Interior;

            return Locations.Exterior;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="geom"></param>
        private void ComputeLocation(ICoordinate p, IGeometry geom)
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
        private void UpdateLocationInfo(Locations loc)
        {
            if(loc == Locations.Interior) 
                _isIn = true;
            if(loc == Locations.Boundary) 
                _numBoundaries++;
        }

        private static Locations Locate(ICoordinate p, IPoint pt)
        {
            // no point in doing envelope test, since equality test is just as fast

            ICoordinate ptCoord = pt.Coordinate;
            if (ptCoord.Equals2D(p))
                return Locations.Interior;
            return Locations.Exterior;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        private static Locations Locate(ICoordinate p, ILineString l)
        {
            // bounding-box check
            if (!l.EnvelopeInternal.Intersects(p)) 
                return Locations.Exterior;
  	

            ICoordinate[] pt = l.Coordinates;
            if(!l.IsClosed)
                if(p.Equals(pt[0]) || p.Equals(pt[pt.Length - 1]))
                    return Locations.Boundary;                            
            if (CGAlgorithms.IsOnLine(p, pt))
                return Locations.Interior;
            return Locations.Exterior;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ring"></param>
        /// <returns></returns>
        private static Locations LocateInPolygonRing(ICoordinate p, ILinearRing ring)
        {
  	        // bounding-box check
  	        if (! ring.EnvelopeInternal.Intersects(p)) return Locations.Exterior;

  	        return CGAlgorithms.LocatePointInRing(p, ring.Coordinates);
  	
          	/*
            // can this test be folded into IsPointInRing?
            if (CGAlgorithms.IsOnLine(p, ring.Coordinates))
                return Locations.Boundary;
            if (CGAlgorithms.IsPointInRing(p, ring.Coordinates))
                return Locations.Interior;
            return Locations.Exterior;
            */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="poly"></param>
        /// <returns></returns>
        private Locations Locate(ICoordinate p, IPolygon poly)
        {
            if (poly.IsEmpty) 
                return Locations.Exterior;
            ILinearRing shell = poly.Shell;
            Locations shellLoc = LocateInPolygonRing(p, shell);
            if (shellLoc == Locations.Exterior) 
                return Locations.Exterior;
            if (shellLoc == Locations.Boundary) 
                return Locations.Boundary;
            // now test if the point lies in or on the holes
            foreach (ILinearRing hole in poly.InteriorRings)
            {
                Locations holeLoc = LocateInPolygonRing(p, hole);
                if (holeLoc == Locations.Interior) 
                    return Locations.Exterior;
                if (holeLoc == Locations.Boundary) 
                    return Locations.Boundary;
            }
            return Locations.Interior;
        }
    }
}
