using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// Implements the <see cref="Geometry{TCoordinate}.Relate"/> operations
    /// on <see cref="Geometry{TCoordinate}"/>s.
    /// </summary>
    public class RelateOp<TCoordinate> : GeometryGraphOperation<TCoordinate>
         where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                             IComparable<TCoordinate>, IConvertible, 
                             IComputable<Double, TCoordinate>
    {
        public static IntersectionMatrix Relate(IGeometry<TCoordinate> a, IGeometry<TCoordinate> b)
        {
            RelateOp<TCoordinate> relOp = new RelateOp<TCoordinate>(a, b);
            IntersectionMatrix im = relOp.IntersectionMatrix;
            return im;
        }

        private readonly RelateComputer<TCoordinate> _relate;

        /// <summary>
        /// Creates a new <see cref="RelateOp{TCoordinate}"/> for the given input
        /// geometries.
        /// </summary>
        /// <param name="g0">The first geometry to relate.</param>
        /// <param name="g1">The second geometry to relate.</param>
        public RelateOp(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
            : base(g0, g1)
        {
            _relate = new RelateComputer<TCoordinate>(Argument1, Argument2);
        }

        /// <summary>
        /// Gets the computed intersection matrix for the input geometries.
        /// </summary>
        public IntersectionMatrix IntersectionMatrix
        {
            get { return _relate.ComputeIntersectionMatrix(); }
        }
    }
}