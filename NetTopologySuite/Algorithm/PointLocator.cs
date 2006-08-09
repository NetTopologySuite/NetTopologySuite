using System;
using System.Collections;
using System.Text;
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
        public virtual bool Intersects(Coordinate p, Geometry geom)
        {
            return Locate(p, geom) != Locations.Exterior;
        }

        /// <summary> 
        /// Computes the topological relationship ({Location}) of a single point to a Geometry.
        /// It handles both single-element and multi-element Geometries.
        /// The algorithm for multi-part Geometries takes into account the boundaryDetermination rule.
        /// </summary>
        /// <returns>The Location of the point relative to the input Geometry.</returns>
        public virtual Locations Locate(Coordinate p, Geometry geom)
        {
            if(geom.IsEmpty)
                return Locations.Exterior;
            if(geom is LineString) 
                return Locate(p, (LineString)geom);                        
            else if (geom is Polygon) 
                return Locate(p, (Polygon)geom);
        
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
        private void ComputeLocation(Coordinate p, Geometry geom)
        {
            if(geom is LineString) 
                UpdateLocationInfo(Locate(p, (LineString)geom));                                  
            else if(geom is Polygon) 
                UpdateLocationInfo(Locate(p, (Polygon)geom));            
            else if(geom is MultiLineString) 
            {
                MultiLineString ml = (MultiLineString)geom;
                foreach(LineString l in ml.Geometries)                     
                    UpdateLocationInfo(Locate(p, l));                
            }
            else if(geom is MultiPolygon)
            {
                MultiPolygon mpoly = (MultiPolygon)geom;
                foreach(Polygon poly in mpoly.Geometries) 
                    UpdateLocationInfo(Locate(p, poly));
            }
            else if (geom is GeometryCollection) 
            {
                IEnumerator geomi = new GeometryCollectionEnumerator((GeometryCollection)geom);
                while(geomi.MoveNext()) 
                {
                    Geometry g2 = (Geometry)geomi.Current;
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
        private Locations Locate(Coordinate p, LineString l)
        {
            Coordinate[] pt = l.Coordinates;
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
        private Locations LocateInPolygonRing(Coordinate p, LinearRing ring)
        {
            // can this test be folded into IsPointInRing?
            if(CGAlgorithms.IsOnLine(p, ring.Coordinates))
                return Locations.Boundary;
            if(CGAlgorithms.IsPointInRing(p, ring.Coordinates))
                return Locations.Interior;
            return Locations.Exterior;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="poly"></param>
        /// <returns></returns>
        private Locations Locate(Coordinate p, Polygon poly)
        {
            if (poly.IsEmpty) 
                return Locations.Exterior;
            LinearRing shell = (LinearRing)poly.ExteriorRing;
            Locations shellLoc = LocateInPolygonRing(p, shell);
            if (shellLoc == Locations.Exterior) 
                return Locations.Exterior;
            if (shellLoc == Locations.Boundary) 
                return Locations.Boundary;
            // now test if the point lies in or on the holes
            foreach(LinearRing hole in poly.InteriorRings)
            {
                Locations holeLoc = LocateInPolygonRing(p, hole);
                if(holeLoc == Locations.Interior) 
                    return Locations.Exterior;
                if(holeLoc == Locations.Boundary) 
                    return Locations.Boundary;
            }
            return Locations.Interior;
        }
    }
}
