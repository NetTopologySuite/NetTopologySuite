using System;
using System.Text;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A TopologyLocation is the labelling of a
    /// GraphComponent's topological relationship to a single Geometry.
    /// </summary>
    /// <remarks>
    /// If the parent component is an area edge, each side and the edge itself
    /// have a topological location.  These locations are named:
    ///  On: on the edge
    ///  Left: left-hand side of the edge
    ///  Right: right-hand side
    /// If the parent component is a line edge or node, there is a single
    /// topological relationship attribute, On.
    /// The possible values of a topological location are
    /// { Location.Null, Location.Exterior, Location.Boundary, Location.Interior } 
    /// The labelling is stored in an array location[j] where
    /// where j has the values On, Left, Right.
    /// </remarks>
    public class TopologyLocation
    {
        private Locations[] _location;

        public TopologyLocation(Locations[] location)
        {
            Init(location.Length);
        }

        /// <summary> 
        /// Constructs a TopologyLocation specifying how points on, to the left of, and to the
        /// right of some GraphComponent relate to some Geometry. Possible values for the
        /// parameters are Location.Null, Location.Exterior, Location.Boundary, 
        /// and Location.Interior.
        /// </summary>
        public TopologyLocation(Locations on, Locations left, Locations right)
        {
            Init(3);
            _location[(Int32) Positions.On] = on;
            _location[(Int32) Positions.Left] = left;
            _location[(Int32) Positions.Right] = right;
        }

        public TopologyLocation(Locations on)
        {
            Init(1);
            _location[(Int32) Positions.On] = on;
        }

        public TopologyLocation(TopologyLocation gl)
        {
            if (gl == null)
            {
                throw new ArgumentNullException("gl");
            }

            Init(gl._location.Length);

            for (Int32 i = 0; i < _location.Length; i++)
            {
                _location[i] = gl._location[i];
            }
        }

        private void Init(Int32 size)
        {
            _location = new Locations[size];
            SetAllLocations(Locations.Null);
        }

        public Locations Get(Positions posIndex)
        {
            Int32 index = (Int32) posIndex;

            if (index < _location.Length)
            {
                return _location[index];
            }

            return Locations.Null;
        }

        /// <summary>
        /// Get calls Get(Positions posIndex),
        /// Set calls SetLocation(Positions locIndex, Locations locValue)
        /// </summary>
        public Locations this[Positions positionIndex]
        {
            get { return Get(positionIndex); }
            set { SetLocation(positionIndex, value); }
        }

        /// <returns>
        /// <see langword="true"/> if all locations are Null.
        /// </returns>
        public Boolean IsNull
        {
            get
            {
                foreach (Locations location in _location)
                {
                    if (location != Locations.Null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <returns> 
        /// <see langword="true"/> if any locations are Null.
        /// </returns>
        public Boolean IsAnyNull
        {
            get
            {
                foreach (Locations location in _location)
                {
                    if (location == Locations.Null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public Boolean IsEqualOnSide(TopologyLocation le, Int32 locIndex)
        {
            return _location[locIndex] == le._location[locIndex];
        }

        public Boolean IsArea
        {
            get { return _location.Length > 1; }
        }

        public Boolean IsLine
        {
            get { return _location.Length == 1; }
        }

        public void Flip()
        {
            if (_location.Length <= 1)
            {
                return;
            }

            Locations temp = _location[(Int32) Positions.Left];
            _location[(Int32) Positions.Left] = _location[(Int32) Positions.Right];
            _location[(Int32) Positions.Right] = temp;
        }

        public void SetAllLocations(Locations locValue)
        {
            for (Int32 i = 0; i < _location.Length; i++)
            {
                _location[i] = locValue;
            }
        }

        public void SetAllLocationsIfNull(Locations locValue)
        {
            for (Int32 i = 0; i < _location.Length; i++)
            {
                if (_location[i] == Locations.Null)
                {
                    _location[i] = locValue;
                }
            }
        }

        public void SetLocation(Positions locIndex, Locations locValue)
        {
            _location[(Int32) locIndex] = locValue;
        }

        public void SetLocation(Locations locValue)
        {
            SetLocation(Positions.On, locValue);
        }

        public Locations[] GetLocations()
        {
            return _location;
        }

        public void SetLocations(Locations on, Locations left, Locations right)
        {
            _location[(Int32) Positions.On] = on;
            _location[(Int32) Positions.Left] = left;
            _location[(Int32) Positions.Right] = right;
        }

        public void SetLocations(TopologyLocation gl)
        {
            for (Int32 i = 0; i < gl._location.Length; i++)
            {
                _location[i] = gl._location[i];
            }
        }

        public Boolean AllPositionsEqual(Locations loc)
        {
            for (Int32 i = 0; i < _location.Length; i++)
            {
                if (_location[i] != loc)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Merge updates only the Null attributes of this object
        /// with the attributes of another.
        /// </summary>
        public void Merge(TopologyLocation gl)
        {
            // if the src is an Area label & and the dest is not, increase the dest to be an Area
            if (gl._location.Length > _location.Length)
            {
                Locations[] newLoc = new Locations[3];
                newLoc[(Int32) Positions.On] = _location[(Int32) Positions.On];
                newLoc[(Int32) Positions.Left] = Locations.Null;
                newLoc[(Int32) Positions.Right] = Locations.Null;
                _location = newLoc;
            }

            for (Int32 i = 0; i < _location.Length; i++)
            {
                if (_location[i] == Locations.Null && i < gl._location.Length)
                {
                    _location[i] = gl._location[i];
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (_location.Length > 1)
            {
                sb.Append(LocationTypeConverter.ToLocationSymbol(_location[(Int32)Positions.Left]));
            }

            sb.Append(LocationTypeConverter.ToLocationSymbol(_location[(Int32)Positions.On]));
            
            if (_location.Length > 1)
            {
                sb.Append(LocationTypeConverter.ToLocationSymbol(_location[(Int32)Positions.Right]));
            }

            return sb.ToString();
        }
    }
}