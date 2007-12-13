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
    /// topological relationship attribute, On.
    /// The possible values of a topological location are
    /// { Location.Null, Location.Exterior, Location.Boundary, Location.Interior } 
    /// The labeling is stored in an array location[j] where
    /// where j has the values On, Left, Right.
    /// </para>
    /// </remarks>
    public struct TopologyLocation : IEquatable<TopologyLocation>
    {
        private static readonly Int32 AllLocationsNull =
            ((SByte) Locations.None) << 16 |
            ((SByte)Locations.None) << 8 |
            (SByte)Locations.None;

        private static readonly Int32 OnOffset = 16;
        private static readonly Int32 LeftOffset = 8;
        private static readonly Int32 RightOffset = 0;

        private static readonly UInt32 OnMask = 0xFF00FFFF;
        private static readonly UInt32 LeftMask = 0xFFFF00FF;
        private static readonly UInt32 RightMask = 0xFFFFFF00;

        public static readonly TopologyLocation None = new TopologyLocation(AllLocationsNull);

        private Int32 _locations;

        /// <summary> 
        /// Constructs a TopologyLocation specifying how points on, to the left of, and to the
        /// right of some GraphComponent relate to some Geometry.
        /// </summary>
        public TopologyLocation(Locations on, Locations left, Locations right)
        {
            _locations = AllLocationsNull;
            setLocation(Positions.On, on);
            setLocation(Positions.Left, left);
            setLocation(Positions.Right, right);
        }

        public TopologyLocation(Locations on)
        {
            _locations = AllLocationsNull;
            setLocation(Positions.On, on);
        }

        public TopologyLocation(TopologyLocation other)
        {
            _locations = other._locations;
        }

        public TopologyLocation(TopologyLocation other, Positions position, Locations location)
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
                    case Positions.Parallel:
                    default:
                        return Locations.None;
                }
            }
        }

        /// <summary>
        /// Gets <see langword="true"/> if all locations are 
        /// <see cref="Locations.None"/>.
        /// </summary>
        public Boolean IsNull
        {
            get
            {
                return _locations == AllLocationsNull;
            }
        }

        /// <summary>
        /// Gets <see langword="true"/> if any locations are 
        /// <see cref="Locations.None"/>.
        /// </summary>
        public Boolean AreAnyNull
        {
            get
            {
                return On == Locations.None ||
                       Left == Locations.None ||
                       Right == Locations.None;
            }
        }

        public Boolean IsEqualOnSide(TopologyLocation location, Positions position)
        {
            return this[position] == location[position];
        }

        public Boolean IsArea
        {
            get
            {
                return Left != Locations.None || 
                       Right != Locations.None;
            }
        }

        public Boolean IsLine
        {
            get
            {
                return Left == Locations.None &&
                       Right == Locations.None;
            }
        }

        public TopologyLocation Flip()
        {
            if (IsLine)
            {
                return this;
            }

            Locations left = Left;
            Locations right = Right;
            return new TopologyLocation(On, right, left);
        }

        public Locations On
        {
            get
            {
                return (Locations)(SByte)(_locations >> OnOffset);
            }
        }

        public Locations Left
        {
            get
            {
                return (Locations)(SByte)(_locations >> LeftOffset);
            }
        }

        public Locations Right
        {
            get
            {
                return (Locations)(SByte)(_locations >> RightOffset);
            }
        }

        public Boolean AllPositionsEqual(Locations loc)
        {
            return On == loc &&
                Left == loc &&
                Right == loc;
        }

        /// <summary>
        /// Merge updates only the Null attributes of this <see cref="TopologyLocation"/>
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

            switch (locIndex)
            {
                case Positions.On:
                    locations &= (Int32)OnMask;
                    locations |= ((SByte) locValue) << OnOffset;
                    break;
                case Positions.Left:
                    locations &= (Int32)LeftMask;
                    locations |= ((SByte)locValue) << LeftOffset;
                    break;
                case Positions.Right:
                    locations &= (Int32)RightMask;
                    locations |= ((SByte)locValue) << OnOffset;
                    break;
                case Positions.Parallel:
                default:
                    break;
            }

            _locations = locations;
        }
    }
}