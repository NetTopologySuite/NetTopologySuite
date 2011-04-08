using System;
using GeoAPI;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries
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
    public class LinearRing<TCoordinate> : LineString<TCoordinate>,
                                           ILinearRing<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// The minimum number of vertices allowed in a valid non-empty ring (= 4).
        /// Empty rings with 0 vertices are also valid.
        ///</summary>
        public const Int32 MinimumValidSize = 4;
        
        /// <summary>
        /// Constructs a <see cref="LinearRing{TCoordinate}" /> with the given coordinates.
        /// </summary>
        /// <param name="coordinates">
        /// Points forming a closed and simple linestring, or
        /// <see langword="null" /> or an empty array to create the empty point.
        /// This array must not contain <see langword="null" /> elements.
        /// </param>
        /// <param name="factory"></param>
        public LinearRing(ICoordinateSequence<TCoordinate> coordinates, IGeometryFactory<TCoordinate> factory)
            : base(coordinates, factory)
        {
            ValidateConstruction();
        }
		
		public override ILineString<TCoordinate> Reverse()
        {
            ILineString<TCoordinate> reverse = base.Reverse();
            return Factory.CreateLinearRing(reverse.Coordinates);
        }

        #region ILinearRing<TCoordinate> Members

        public override IGeometry<TCoordinate> Clone()
        {
            return Factory.CreateLinearRing(Coordinates);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the geometry has no anomalous geometric 
        /// points, such as self intersection or self tangency. The description of each 
        /// instantiable geometric class will include the specific conditions that cause 
        /// an instance of that class to be classified as not simple.
        /// </summary>
        public override Boolean IsSimple
        {
            get { return true; }
        }

        public override OgcGeometryType GeometryType
        {
            // NOTE: this was "LinearRing", but this value should suffice
            // There is no "LinearRing" type in the OGC SF spec
            get { return OgcGeometryType.LineString; }
        }

        ///<summary>
        /// Returns <see langword="true"/> if the first and last coordinate describe the same location.
        ///</summary>
        public override Boolean IsClosed
        {
            get
            {
                // Empty Linear Rings are closed by definition
                if (IsEmpty)
                    return true;
                return base.IsClosed;
            }
        }

        public Boolean IsCcw
        {
            get { return CGAlgorithms<TCoordinate>.IsCCW(Coordinates); }
        }

        #endregion

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        ///// <summary>
        ///// Initializes a new instance of the <see cref="LinearRing{TCoordinate}"/> class.
        ///// </summary>
        ///// <param name="points">The points used for create this instance.</param>
        ///// <remarks>
        ///// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        ///// with <see cref="IPrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModelType.Floating"/>.
        ///// </remarks>
        //public LinearRing(IEnumerable<TCoordinate> points) :
        //    this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory) {}

        /* END ADDED BY MPAUL42: monoGIS team */

        private void ValidateConstruction()
        {
            if (!IsEmpty && !base.IsClosed)
            {
                throw new GeometryInvalidException("Coordinates must form a closed linestring");
            }

            if (Coordinates.Count >= 1 && Coordinates.Count < MinimumValidSize)
            {
                throw new GeometryInvalidException("Number of coordinates must equal 0 " +
                                                   "or be greater than 3");
            }
        }
    }
}