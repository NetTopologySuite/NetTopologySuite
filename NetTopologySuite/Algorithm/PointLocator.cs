using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the topological relationship (Location) of a single point to a Geometry.
    /// The algorithm obeys the SFS boundaryDetermination rule to correctly determine
    /// whether the point lies on the boundary or not.
    /// Note that instances of this class are not reentrant.
    /// </summary>
    public class PointLocator
    {
        private bool isIn;            // true if the point lies in or on any Geometry element
        private int numBoundaries;    // the number of sub-elements whose boundaries the point lies in

        /// <summary>
        /// Initializes a new instance of the <see cref="PointLocator"/> class.
        /// </summary>
        public PointLocator() { }

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
            else if (geom is IPolygon) 
                return Locate(p, (IPolygon) geom);
        
            isIn = false;
            numBoundaries = 0;
            ComputeLocation(p, geom);
            if(GeometryGraph.IsInBoundary(numBoundaries))
                return Locations.Boundary;
            if(numBoundaries > 0 || isIn)
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
                isIn = true;
            if(loc == Locations.Boundary) 
                numBoundaries++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        private Locations Locate(ICoordinate p, ILineString l)
        {
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
        private Locations LocateInPolygonRing(ICoordinate p, ILinearRing ring)
        {
            // can this test be folded into IsPointInRing?
            if (CGAlgorithms.IsOnLine(p, ring.Coordinates))
                return Locations.Boundary;
            if (CGAlgorithms.IsPointInRing(p, ring.Coordinates))
                return Locations.Interior;
            return Locations.Exterior;
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
