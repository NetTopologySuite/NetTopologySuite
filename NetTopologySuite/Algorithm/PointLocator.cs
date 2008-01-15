using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the topological relationship (Location) of a single point to a Geometry.
    /// The algorithm obeys the SFS boundaryDetermination rule to correctly determine
    /// whether the point lies on the boundary or not.
    /// Note that instances of this class are not reentrant.
    /// </summary>
    public class PointLocator<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        // true if the point lies in or on any Geometry element
        private Boolean _isIn; 
        // the number of sub-elements whose boundaries the point lies in
        private Int32 _boundaryCount; 

        /// <summary> 
        /// Convenience method to test a point for intersection with a Geometry
        /// </summary>
        /// <param name="p">The coordinate to test.</param>
        /// <param name="geom">The Geometry to test.</param>
        /// <returns><see langword="true"/> if the point is in the interior or boundary of the Geometry.</returns>
        public Boolean Intersects(TCoordinate p, IGeometry<TCoordinate> geom)
        {
            return Locate(p, geom) != Locations.Exterior;
        }

        /// <summary> 
        /// Computes the topological relationship (<see cref="Locations"/>) of a single point to a Geometry.
        /// It handles both single-element and multi-element Geometries.
        /// The algorithm for multi-part Geometries takes into account the boundaryDetermination rule.
        /// </summary>
        /// <returns>The Location of the point relative to the input Geometry.</returns>
        public Locations Locate(TCoordinate p, IGeometry<TCoordinate> geom)
        {
            if (geom.IsEmpty)
            {
                return Locations.Exterior;
            }

            if (geom is ILineString<TCoordinate>)
            {
                return Locate(p, geom as ILineString<TCoordinate>);
            }
            else if (geom is IPolygon<TCoordinate>)
            {
                return locate(p, geom as IPolygon<TCoordinate>);
            }

            _isIn = false;
            _boundaryCount = 0;

            computeLocation(p, geom);

            if (GeometryGraph<TCoordinate>.IsInBoundary(_boundaryCount))
            {
                return Locations.Boundary;
            }

            if (_boundaryCount > 0 || _isIn)
            {
                return Locations.Interior;
            }

            return Locations.Exterior;
        }

        private void computeLocation(TCoordinate p, IGeometry<TCoordinate> geom)
        {
            if (geom is ILineString<TCoordinate>)
            {
                updateLocationInfo(locate(p, geom as ILineString<TCoordinate>));
            }
            else if (geom is IPolygon<TCoordinate>)
            {
                updateLocationInfo(locate(p, geom as IPolygon<TCoordinate>));
            }
            else if (geom is IMultiLineString<TCoordinate>)
            {
                IMultiLineString<TCoordinate> ml = geom as IMultiLineString<TCoordinate>;

                foreach (ILineString<TCoordinate> l in (ml as IEnumerable<ILineString<TCoordinate>>))
                {
                    updateLocationInfo(Locate(p, l));
                }
            }
            else if (geom is IMultiPolygon<TCoordinate>)
            {
                IMultiPolygon<TCoordinate> mpoly = geom as IMultiPolygon<TCoordinate>;
                
                foreach (IPolygon<TCoordinate> poly in mpoly)
                {
                    updateLocationInfo(locate(p, poly));
                }
            }
            else if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> collection = geom as IGeometryCollection<TCoordinate>;

                IEnumerator<IGeometry<TCoordinate>> geomi 
                    = new GeometryCollectionEnumerator<TCoordinate>(collection);
               
                while (geomi.MoveNext())
                {
                    IGeometry<TCoordinate> computeGeometry = geomi.Current;

                    if (computeGeometry != geom)
                    {
                        computeLocation(p, computeGeometry);
                    }
                }
            }
        }

        private void updateLocationInfo(Locations loc)
        {
            if (loc == Locations.Interior)
            {
                _isIn = true;
            }

            if (loc == Locations.Boundary)
            {
                _boundaryCount++;
            }
        }

        private Locations locate(TCoordinate p, ILineString<TCoordinate> l)
        {
            IEnumerable<TCoordinate> line = l.Coordinates;

            if (!l.IsClosed)
            {
                TCoordinate start = Slice.GetFirst(line);
                TCoordinate end = Slice.GetLast(line);

                if (p.Equals(start) || p.Equals(end))
                {
                    return Locations.Boundary;
                }
            }

            if (CGAlgorithms<TCoordinate>.IsOnLine(p, line))
            {
                return Locations.Interior;
            }

            return Locations.Exterior;
        }

        private Locations locateInPolygonRing(TCoordinate p, ILinearRing<TCoordinate> ring)
        {
            // can this test be folded into IsPointInRing?
            if (CGAlgorithms<TCoordinate>.IsOnLine(p, ring.Coordinates))
            {
                return Locations.Boundary;
            }

            if (CGAlgorithms<TCoordinate>.IsPointInRing(p, ring.Coordinates))
            {
                return Locations.Interior;
            }

            return Locations.Exterior;
        }

        private Locations locate(TCoordinate p, IPolygon<TCoordinate> poly)
        {
            if (poly.IsEmpty)
            {
                return Locations.Exterior;
            }

            ILinearRing<TCoordinate> shell = poly.ExteriorRing as ILinearRing<TCoordinate>;
            Debug.Assert(shell != null);
            Locations shellLoc = locateInPolygonRing(p, shell);
           
            if (shellLoc == Locations.Exterior)
            {
                return Locations.Exterior;
            }

            if (shellLoc == Locations.Boundary)
            {
                return Locations.Boundary;
            }

            // now test if the point lies in or on the holes
            foreach (ILinearRing<TCoordinate> hole in poly.InteriorRings)
            {
                Locations holeLoc = locateInPolygonRing(p, hole);

                if (holeLoc == Locations.Interior)
                {
                    return Locations.Exterior;
                }

                if (holeLoc == Locations.Boundary)
                {
                    return Locations.Boundary;
                }
            }

            return Locations.Interior;
        }
    }
}