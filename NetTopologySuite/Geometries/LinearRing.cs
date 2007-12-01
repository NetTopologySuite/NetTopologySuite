using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Basic implementation of <see cref="LinearRing{TCoordinate}" />.
    /// </summary>
    /// <remarks>
    /// The first and last point in the coordinate sequence must be equal.
    /// Either orientation of the ring is allowed.
    /// A valid ring must not self-intersect.
    /// </remarks>
    [Serializable]
    public class LinearRing<TCoordinate> : LineString<TCoordinate>, ILinearRing<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Constructs a <see cref="LinearRing{TCoordinate}" /> with the given points.
        /// </summary>
        /// <param name="points">
        /// Points forming a closed and simple linestring, or
        /// <see langword="null" /> or an empty array to create the empty point.
        /// This array must not contain <see langword="null" /> elements.
        /// </param>
        /// <param name="factory"></param>
        public LinearRing(ICoordinateSequence points, IGeometryFactory factory) : base(points, factory)
        {
            ValidateConstruction();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ValidateConstruction()
        {
            if (!IsEmpty && !base.IsClosed)
            {
                throw new ArgumentException("points must form a closed linestring");
            }
            if (CoordinateSequence.Count >= 1 && CoordinateSequence.Count <= 3)
            {
                throw new ArgumentException("Number of points must be 0 or >3");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Boolean IsSimple
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GeometryType
        {
            get { return "LinearRing"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Boolean IsClosed
        {
            get { return true; }
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearRing"/> class.
        /// </summary>
        /// <param name="points">The points used for create this instance.</param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public LinearRing(IEnumerable<TCoordinate> points) :
            this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory) {}

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}