using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Basic implementation of <c>LinearRing</c>.
    /// The first and last point in the coordinate sequence must be equal.
    /// Either orientation of the ring is allowed.
    /// A valid ring must not self-intersect.
    /// </summary>
    [Serializable]
    public class LinearRing : LineString, ILinearRing
    {
        /// <summary>
        /// Constructs a <c>LinearRing</c> with the given points.
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
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="Geometry{TCoordinate}Factory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public LinearRing(ICoordinate[] points) :
            this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory) {}

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}