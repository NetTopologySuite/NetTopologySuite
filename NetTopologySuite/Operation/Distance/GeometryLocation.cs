using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Represents the location of a point on a Geometry.
    /// Maintains both the actual point location (which of course
    /// may not be exact) as well as information about the component
    /// and segment index where the point occurs.
    /// </summary>
    /// <remarks>
    /// Locations inside area Geometries will not have an associated segment index,
    /// so in this case the segment index will be null.
    /// </remarks>
    public struct GeometryLocation<TCoordinate> : IEquatable<GeometryLocation<TCoordinate>>,
                                                  IComparable<GeometryLocation<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometry<TCoordinate> _component;
        private readonly TCoordinate _coordinate;
        private readonly Int32? _segIndex;

        /// <summary>
        /// Constructs a GeometryLocation specifying a point on a point, as well as the 
        /// segment that the point is on (or InsideArea if the point is not on a segment).
        /// </summary>
        public GeometryLocation(IGeometry<TCoordinate> component, Int32? segIndex, TCoordinate pt)
        {
            _component = component;
            _segIndex = segIndex;
            _coordinate = pt;
        }

        /// <summary> 
        /// Constructs a GeometryLocation specifying a point inside an area point.
        /// </summary>
        public GeometryLocation(IGeometry<TCoordinate> component, TCoordinate pt)
            : this(component, null, pt)
        {
        }

        /// <summary>
        /// Gets the point associated with this location.
        /// </summary>
        public IGeometry<TCoordinate> GeometryComponent
        {
            get { return _component; }
        }

        /// <summary>
        /// Gets the segment index for this location. If the location is inside an
        /// area, the index will be null.
        /// </summary>
        public Int32? SegmentIndex
        {
            get { return _segIndex; }
        }

        /// <summary>
        /// Returns the location.
        /// </summary>
        public TCoordinate Coordinate
        {
            get { return _coordinate; }
        }

        /// <summary>
        /// Returns whether this GeometryLocation represents a point inside an area point.
        /// </summary>
        public Boolean IsInsideArea
        {
            get { return !_segIndex.HasValue; }
        }

        #region IComparable<GeometryLocation<TCoordinate>> Members

        public Int32 CompareTo(GeometryLocation<TCoordinate> other)
        {
            if (Equals(other))
            {
                return 0;
            }
            else
            {
                Int32 comparison;

                // First compare the geometries
                comparison = other._component.CompareTo(_component);

                if (comparison != 0)
                {
                    return comparison;
                }

                // If the geometries are equal, check the segment indexes.
                // If either has a null segment index (they aren't equal here),
                // it's less.
                if (!other._segIndex.HasValue || !_segIndex.HasValue)
                {
                    if (_segIndex.HasValue)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }

                comparison = other._segIndex.Value.CompareTo(_segIndex.Value);

                if (comparison != 0)
                {
                    return comparison;
                }

                comparison = other._coordinate.CompareTo(_coordinate);

                Debug.Assert(comparison != 0);

                return comparison;
            }
        }

        #endregion

        #region IEquatable<GeometryLocation<TCoordinate>> Members

        public Boolean Equals(GeometryLocation<TCoordinate> other)
        {
            return other._segIndex == _segIndex &&
                   other._coordinate.Equals(_coordinate) &&
                   other._component == _component;
        }

        #endregion

        public override Boolean Equals(Object obj)
        {
            if (!(obj is GeometryLocation<TCoordinate>))
            {
                return false;
            }

            return Equals((GeometryLocation<TCoordinate>) obj);
        }
    }
}