using System;
using System.Text;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A <see cref="TopologyLocation"/> is the labeling of a
    /// <see cref="GraphComponent{TCoordinate}"/>'s topological relationship 
    /// to a single <see cref="IGeometry"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the labeled component is an area edge, each side and the edge itself
    /// have a topological location.  These locations are named:
    ///  On: on the edge
    ///  Left: left-hand side of the edge
    ///  Right: right-hand side
    /// </para>
    /// <para>
    /// If the labeled component is a line edge or node, there is a single
    /// topological relationship attribute: <see cref="Positions.On"/>.
    /// </para>
    /// <para>
    /// The possible values of a topological location are
    /// { <see cref="Locations.None"/>, <see cref="Locations.Exterior"/>, 
    /// <see cref="Locations.Boundary"/>, <see cref="Locations.Interior"/> }.
    /// </para>
    /// <para>
    /// The labeling is efficiently stored in an <see cref="Int32"/> 
    /// where the lower 3 bytes represent the values of 
    /// <see cref="Positions.On"/>, <see cref="Positions.Left"/> and 
    /// <see cref="Positions.Right"/>, respectively.
    /// </para>
    /// </remarks>
    public struct TopologyLocation : IEquatable<TopologyLocation>
    {
        private const Int32 AllLocationsNone =
            ((SByte)Locations.None) << 16 |
            ((SByte)Locations.None) << 8 |
            (SByte)Locations.None;

        private const Int32 OnOffset = 16;
        private const Int32 LeftOffset = 8;
        private const Int32 RightOffset = 0;

        private const UInt32 OnMask = 0xFF00FFFF;
        private const UInt32 LeftMask = 0xFFFF00FF;
        private const UInt32 RightMask = 0xFFFFFF00;

        public static readonly TopologyLocation None = new TopologyLocation(AllLocationsNone);

        private Int32 _locations;

        /// <summary> 
        /// Constructs a <see cref="TopologyLocation"/> specifying how points on, 
        /// to the left of, and to the right of some 
        /// <see cref="GraphComponent{TCoordinate}"/> relate
        /// to some Geometry.
        /// </summary>
        public TopologyLocation(Locations on, Locations left, Locations right)
        {
            _locations = AllLocationsNone;
            setLocation(Positions.On, on);
            setLocation(Positions.Left, left);
            setLocation(Positions.Right, right);
        }

        /// <summary> 
        /// Constructs a <see cref="TopologyLocation"/> specifying how points on
        /// some <see cref="GraphComponent{TCoordinate}"/> relate
        /// to some Geometry.
        /// </summary>
        /// <param name="on">
        /// The location of points on the graph component relative to 
        /// some geometry.
        /// </param>
        public TopologyLocation(Locations on)
        {
            _locations = AllLocationsNone;
            setLocation(Positions.On, on);
        }

        /// <summary> 
        /// Constructs a <see cref="TopologyLocation"/> which is a copy of the 
        /// given <paramref name="other"/> topology location.
        /// </summary>
        /// <param name="other">The <see cref="TopologyLocation"/> to copy.</param>
        public TopologyLocation(TopologyLocation other)
            : this(other._locations) { }

        /// <summary> 
        /// Constructs a <see cref="TopologyLocation"/> which is a copy of the 
        /// given <paramref name="other"/> topology location, but with
        /// the given <paramref name="position"/> set to the given 
        /// <paramref name="location"/>.
        /// </summary>
        /// <param name="other">The <see cref="TopologyLocation"/> to copy.</param>
        /// <param name="position">
        /// The position within this <see cref="TopologyLocation"/> to set to a new
        /// <see cref="Locations"/> value.
        /// </param>
        /// <param name="location">
        /// The location to use for the given <paramref name="position"/>.
        /// </param>
        public TopologyLocation(TopologyLocation other,
                                Positions position,
                                Locations location)
        {
            _locations = other._locations;
            setLocation(position, location);
        }

        private TopologyLocation(Int32 locations)
        {
            _locations = locations;
        }

        /// <summary>
        /// Gets the <see cref="Locations"/> value at the 
        /// given <see cref="Positions"/> value.
        /// </summary>
        public Locations this[Positions positionIndex]
        {
            get
            {
                switch (positionIndex)
                {
                    case Positions.On:
                        return On;
                    case Positions.Left:
                        return Left;
                    case Positions.Right:
                        return Right;
                    //case Positions.Parallel:
                    default:
                        return Locations.None;
                }
            }
        }

        /// <summary>
        /// Gets <see langword="true"/> if all locations are 
        /// <see cref="Locations.None"/>.
        /// </summary>
        public Boolean IsNone
        {
            get
            {
                return _locations == AllLocationsNone;
            }
        }

        /// <summary>
        /// Gets <see langword="true"/> if any locations are 
        /// <see cref="Locations.None"/>.
        /// </summary>
        public Boolean AreAnyNone
        {
            get
            {
                return On == Locations.None ||
                       Left == Locations.None ||
                       Right == Locations.None;
            }
        }

        /// <summary>
        /// Gets a value indicating if the given <see cref="TopologyLocation"/>
        /// has the same <see cref="Locations"/> value on the side specified by 
        /// <paramref name="position"/>.
        /// </summary>
        /// <param name="other">
        /// The other <see cref="TopologyLocation"/> to compare.
        /// </param>
        /// <param name="position">
        /// The side to compare.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the values at <paramref name="position"/>
        /// are equal; false otherwise.
        /// </returns>
        public Boolean IsEqualOnSide(TopologyLocation other, Positions position)
        {
            return this[position] == other[position];
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="TopologyLocation"/>
        /// represents an area, by having a value other than 
        /// <see cref="Locations.None"/> for <see cref="Left"/> and <see cref="Right"/>.
        /// </summary>
        public Boolean IsArea
        {
            get
            {
                return Left != Locations.None ||
                       Right != Locations.None;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="TopologyLocation"/>
        /// represents a line, by having a value <see cref="Locations.None"/> 
        /// for <see cref="Left"/> and <see cref="Right"/>.
        /// </summary>
        public Boolean IsLine
        {
            get
            {
                return Left == Locations.None &&
                       Right == Locations.None;
            }
        }

        /// <summary>
        /// Computes a <see cref="TopologyLocation"/> which has the
        /// <see cref="Left"/> and <see cref="Right"/> <see cref="Locations"/>
        /// values swapped.
        /// </summary>
        /// <returns>The flipped <see cref="TopologyLocation"/>.</returns>
        public TopologyLocation Flip()
        {
            if (IsLine)
            {
                return this;
            }

            Locations flippedRight = Left;
            Locations flippedLeft = Right;
            return new TopologyLocation(On, flippedLeft, flippedRight);
        }

        /// <summary>
        /// Gets the location of points on this <see cref="TopologyLocation"/>
        /// relative to some geometry as a <see cref="Locations"/> value.
        /// </summary>
        public Locations On
        {
            get
            {
                return (Locations)(SByte)(_locations >> OnOffset);
            }
        }

        /// <summary>
        /// Gets the location of points to the left of this 
        /// <see cref="TopologyLocation"/> relative to some geometry 
        /// as a <see cref="Locations"/> value.
        /// </summary>
        public Locations Left
        {
            get
            {
                return (Locations)(SByte)(_locations >> LeftOffset);
            }
        }

        /// <summary>
        /// Gets the location of points to the right of this 
        /// <see cref="TopologyLocation"/> relative to some geometry 
        /// as a <see cref="Locations"/> value.
        /// </summary>
        public Locations Right
        {
            get
            {
                return (Locations)(SByte)(_locations >> RightOffset);
            }
        }

        /// <summary>
        /// Computes whether all locations in this <see cref="TopologyLocation"/>
        /// are equal to the given <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location to test.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="On"/>, 
        /// <see cref="Left"/> and <see cref="Right"/> are all equal to
        /// <paramref name="location"/>.
        /// </returns>
        public Boolean AllPositionsEqual(Locations location)
        {
            return On == location &&
                   Left == location &&
                   Right == location;
        }

        /// <summary>
        /// Merge updates only the attributes of this 
        /// <see cref="TopologyLocation"/> which are <see cref="Locations.None"/>
        /// with the attributes of another, and returns the result.
        /// </summary>
        /// <remarks>
        /// If one of the the <see cref="TopologyLocation"/> is a line, and  
        /// other is an area, the destination will be an area.
        /// </remarks>
        public TopologyLocation Merge(TopologyLocation other)
        {
            // if the src is an Area location & and the dest is not, 
            // increase the dest to be an Area
            Locations on = On == Locations.None ? other.On : On;
            Locations left = Left == Locations.None ? other.Left : Left;
            Locations right = Right == Locations.None ? other.Right : Right;

            return new TopologyLocation(on, left, right);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            Boolean isArea = IsArea;

            if (isArea)
            {
                sb.Append(LocationTypeConverter.ToLocationSymbol(Left));
            }

            sb.Append(LocationTypeConverter.ToLocationSymbol(On));

            if (isArea)
            {
                sb.Append(LocationTypeConverter.ToLocationSymbol(Right));
            }

            return sb.ToString();
        }

        #region IEquatable<TopologyLocation> Members

        public Boolean Equals(TopologyLocation other)
        {
            return other._locations == _locations;
        }

        #endregion

        private void setLocation(Positions locIndex, Locations locValue)
        {
            Int32 locations = _locations;

            unchecked
            {
                switch (locIndex)
                {
                    case Positions.On:
                        locations &= (Int32)OnMask;
                        locations |= ((SByte)locValue) << OnOffset;
                        break;
                    case Positions.Left:
                        locations &= (Int32)LeftMask;
                        locations |= ((SByte)locValue) << LeftOffset;
                        break;
                    case Positions.Right:
                        locations &= (Int32)RightMask;
                        locations |= ((SByte)locValue) << RightOffset;
                        break;
                    //case Positions.Parallel:
                    default:
                        break;
                }
            }

            _locations = locations;
        }
    }
}