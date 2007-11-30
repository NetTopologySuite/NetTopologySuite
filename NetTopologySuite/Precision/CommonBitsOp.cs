using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Precision
{
    /// <summary> 
    /// Provides versions of Geometry spatial functions which use
    /// common bit removal to reduce the likelihood of robustness problems.
    /// </summary>
    /// <remarks>
    /// In the current implementation no rounding is performed on the
    /// reshifted result point, which means that it is possible
    /// that the returned Geometry is invalid.
    /// Client classes should check the validity of the returned result themselves.
    /// </remarks>
    public class CommonBitsOp<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly Boolean _returnToOriginalPrecision = true;
        private CommonBitsRemover<TCoordinate> _commonBitsRemover;

        /// <summary>
        /// Creates a new instance of class, which reshifts result 
        /// <see cref="Geometry{TCoordinate}"/>s.
        /// </summary>
        public CommonBitsOp() : this(true) {}

        /// <summary>
        /// Creates a new instance of class, specifying whether
        /// the result <see cref="Geometry{TCoordinate}"/>s should be reshifted.
        /// </summary>
        public CommonBitsOp(Boolean returnToOriginalPrecision)
        {
            _returnToOriginalPrecision = returnToOriginalPrecision;
        }

        /// <summary>
        /// Computes the set-theoretic intersection of two <see cref="Geometry{TCoordinate}"/>s, 
        /// using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>The Geometry representing the set-theoretic intersection of the input Geometries.</returns>
        public IGeometry<TCoordinate> Intersection(IGeometry<TCoordinate> geom0, IGeometry<TCoordinate> geom1)
        {
            IGeometry<TCoordinate> geom0Output;
            IGeometry<TCoordinate> geom1Output;
            geom0Output = removeCommonBits(geom0, geom1, out geom1Output);
            return computeResultPrecision(geom0Output.Intersection(geom1Output));
        }

        /// <summary>
        /// Computes the set-theoretic union of two <see cref="Geometry{TCoordinate}"/>s, 
        /// using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>
        /// The Geometry representing the set-theoretic union of the input Geometries.
        /// </returns>
        public IGeometry<TCoordinate> Union(IGeometry<TCoordinate> geom0, IGeometry<TCoordinate> geom1)
        {
            IGeometry<TCoordinate> geom0Output;
            IGeometry<TCoordinate> geom1Output;
            geom0Output = removeCommonBits(geom0, geom1, out geom1Output);
            return computeResultPrecision(geom0Output.Union(geom1Output));
        }

        /// <summary>
        /// Computes the set-theoretic difference of two <see cref="Geometry{TCoordinate}"/>s, 
        /// using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry, to be subtracted from the first.</param>
        /// <returns>
        /// The Geometry representing the set-theoretic difference of the input Geometries.
        /// </returns>
        public IGeometry<TCoordinate> Difference(IGeometry<TCoordinate> geom0, IGeometry<TCoordinate> geom1)
        {
            IGeometry<TCoordinate> geom0Output;
            IGeometry<TCoordinate> geom1Output;
            geom0Output = removeCommonBits(geom0, geom1, out geom1Output);
            return computeResultPrecision(geom0Output.Difference(geom1Output));
        }

        /// <summary
        /// > Computes the set-theoretic symmetric difference of two geometries,
        /// using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>The Geometry representing the set-theoretic symmetric difference of the input Geometries.</returns>
        public IGeometry<TCoordinate> SymDifference(IGeometry<TCoordinate> geom0, IGeometry<TCoordinate> geom1)
        {
            IGeometry<TCoordinate> geom0Output;
            IGeometry<TCoordinate> geom1Output;
            geom0Output = removeCommonBits(geom0, geom1, out geom1Output);
            return computeResultPrecision(geom0Output.SymmetricDifference(geom1Output));
        }

        /// <summary>
        /// Computes the buffer a point, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The Geometry to buffer.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <returns>The Geometry representing the buffer of the input Geometry.</returns>
        public IGeometry<TCoordinate> Buffer(IGeometry<TCoordinate> geom0, Double distance)
        {
            IGeometry<TCoordinate> geom = removeCommonBits(geom0);
            return computeResultPrecision(geom.Buffer(distance));
        }

        /// <summary>
        /// If required, returning the result to the orginal precision if required.
        /// In this current implementation, no rounding is performed on the
        /// reshifted result point, which means that it is possible
        /// that the returned Geometry is invalid.
        /// </summary>
        /// <param name="result">The result Geometry to modify.</param>
        /// <returns>The result Geometry with the required precision.</returns>
        private IGeometry<TCoordinate> computeResultPrecision(IGeometry<TCoordinate> result)
        {
            if (_returnToOriginalPrecision)
            {
                _commonBitsRemover.AddCommonBits(result);
            }

            return result;
        }

        /// <summary>
        /// Computes a copy of the input <see cref="Geometry{TCoordinate}"/> with the calculated common bits
        /// removed from each coordinate.
        /// </summary>
        /// <param name="geom0">The Geometry to remove common bits from.</param>
        /// <returns>A copy of the input Geometry with common bits removed.</returns>
        private IGeometry<TCoordinate> removeCommonBits(IGeometry<TCoordinate> geom0)
        {
            _commonBitsRemover = new CommonBitsRemover<TCoordinate>();
            _commonBitsRemover.Add(geom0);
            IGeometry<TCoordinate> geom = _commonBitsRemover.RemoveCommonBits(geom0.Clone());
            return geom;
        }

        /// <summary>
        /// Computes a copy of each input <see cref="Geometry{TCoordinate}"/>s with the calculated common bits
        /// removed from each coordinate.
        /// </summary>
        /// <param name="geom0">A Geometry to remove common bits from.</param>
        /// <param name="geom1">A Geometry to remove common bits from.</param>
        /// <returns>
        /// An array containing copies
        /// of the input Geometry's with common bits removed.
        /// </returns>
        private IGeometry<TCoordinate> removeCommonBits(IGeometry<TCoordinate> geom0, IGeometry<TCoordinate> geom1,
                                                        out IGeometry<TCoordinate> geom1Output)
        {
            _commonBitsRemover = new CommonBitsRemover<TCoordinate>();
            _commonBitsRemover.Add(geom0);
            _commonBitsRemover.Add(geom1);

            geom1Output = _commonBitsRemover.RemoveCommonBits(geom1.Clone());
            return _commonBitsRemover.RemoveCommonBits(geom0.Clone());
        }
    }
}