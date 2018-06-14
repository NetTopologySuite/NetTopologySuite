using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Overlay.Validate
{
    ///<summary>
    /// Finds the most likely <see cref="Location"/> of a point relative to
    /// the polygonal components of a geometry, using a tolerance value.
    ///</summary>
    ///<remarks>
    /// If a point is not clearly in the Interior or Exterior,
    /// it is considered to be on the Boundary.
    /// In other words, if the point is within the tolerance of the Boundary,
    /// it is considered to be on the Boundary; otherwise,
    /// whether it is Interior or Exterior is determined directly.
    ///</remarks>
    /// <author>Martin Davis</author>
    public class FuzzyPointLocator
    {
        private readonly IGeometry _g;
        private readonly double _boundaryDistanceTolerance;
        private readonly IMultiLineString _linework;
        private readonly PointLocator _ptLocator = new PointLocator();
        private readonly LineSegment _seg = new LineSegment();

        public FuzzyPointLocator(IGeometry g, double boundaryDistanceTolerance)
        {
            _g = g;
            _boundaryDistanceTolerance = boundaryDistanceTolerance;
            _linework = ExtractLinework(g);
        }

        public Location GetLocation(Coordinate pt)
        {
            if (IsWithinToleranceOfBoundary(pt))
                return Location.Boundary;
            /*
            double dist = linework.distance(point);

            // if point is close to boundary, it is considered to be on the boundary
            if (dist < tolerance)
              return Location.BOUNDARY;
             */

            // now we know point must be clearly inside or outside geometry, so return actual location value
            return _ptLocator.Locate(pt, _g);
        }

        ///<summary>
        /// Extracts linework for polygonal components.
        ///</summary>
        ///<param name="g">The geometry from which to extract</param>
        ///<returns>A lineal geometry containing the extracted linework</returns>
        private static IMultiLineString ExtractLinework(IGeometry g)
        {
            var extracter = new PolygonalLineworkExtracter();
            g.Apply(extracter);
            var linework = extracter.Linework;
            return g.Factory.CreateMultiLineString(linework.ToArray());
        }

        private bool IsWithinToleranceOfBoundary(Coordinate pt)
        {
            for (int i = 0; i < _linework.NumGeometries; i++)
            {
                var line = (ILineString)_linework.GetGeometryN(i);
                var seq = line.CoordinateSequence;
                for (int j = 0; j < seq.Count - 1; j++)
                {
                    seq.GetCoordinate(j, _seg.P0);
                    seq.GetCoordinate(j + 1, _seg.P1);
                    double dist = _seg.Distance(pt);
                    if (dist <= _boundaryDistanceTolerance)
                        return true;
                }
            }
            return false;
        }
    }

    ///<summary>
    /// Extracts the LineStrings in the boundaries of all the polygonal elements in the target <see cref="IGeometry"/>.
    ///</summary>
    ///<author>Martin Davis</author>
    class PolygonalLineworkExtracter : IGeometryFilter
    {
        private readonly List<ILineString> _linework;

        public PolygonalLineworkExtracter()
        {
            _linework = new List<ILineString>();
        }

        ///<summary>
        /// Filters out all linework for polygonal elements
        /// </summary>
        public void Filter(IGeometry g)
        {
            if (g is IPolygon)
            {
                var poly = (IPolygon)g;
                _linework.Add(poly.ExteriorRing);
                for (int i = 0; i < poly.NumInteriorRings; i++)
                {
                    _linework.Add(poly.InteriorRings[i]);
                }
            }
        }

        ///<summary>
        /// Gets the list of polygonal linework.
        ///</summary>
        public List<ILineString> Linework => _linework;
    }
}