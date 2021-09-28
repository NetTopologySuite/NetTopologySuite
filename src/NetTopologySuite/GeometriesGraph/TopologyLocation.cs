using System;
using System.Text;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A TopologyLocation is the labelling of a
    /// GraphComponent's topological relationship to a single Geometry.
    /// </summary>
    /// <remarks>
    /// If the parent component is an area edge, each side and the edge itself
    /// have a topological location.  These locations are named:
    /// <list type="table">
    /// <item><term>On</term><description>on the edge</description></item>
    /// <item><term>Left</term><description>left-hand side of the edge</description></item>
    /// <item><term>Right</term><description>right-hand side</description></item>
    /// </list>
    /// <para>
    /// If the parent component is a line edge or node, there is a single
    /// topological relationship attribute, On.</para>
    /// <para>
    /// The possible values of a topological location are
    /// { <see cref="Location.Null"/>, <see cref="Location.Exterior"/>, <see cref="Location.Boundary"/>, <see cref="Location.Interior"/> }</para>
    /// <para>
    /// The labelling is stored in an array _location[j] where
    /// where j has the values On, Left, Right.
    /// </para>
    /// </remarks>
    public class TopologyLocation
    {
        private Location[] _location;

        /// <summary>
        ///
        /// </summary>
        /// <param name="location"></param>
        public TopologyLocation(Location[] location)
        {
            Init(location.Length);
        }

        /// <summary>
        /// Constructs a TopologyLocation specifying how points on, to the left of, and to the
        /// right of some GraphComponent relate to some Geometry. Possible values for the
        /// parameters are Location.Null, Location.Exterior, Location.Boundary,
        /// and Location.Interior.
        /// </summary>
        /// <param name="on"><c>Location</c> for <b>On</b> position</param>
        /// <param name="left"><c>Location</c> for <b>Left</b> position</param>
        /// <param name="right"><c>Location</c> for <b>Right</b> position</param>
        public TopologyLocation(Location on, Location left, Location right)
        {
            Init(3);
            _location[(int)Geometries.Position.On] = on;
            _location[(int)Geometries.Position.Left] = left;
            _location[(int)Geometries.Position.Right] = right;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="on"></param>
        public TopologyLocation(Location on)
        {
            Init(1);
            _location[(int)Geometries.Position.On] = on;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gl"></param>
        public TopologyLocation(TopologyLocation gl)
        {
            if (gl == null)
                throw new ArgumentNullException("gl", "null topology location specified");

            Init(gl._location.Length);
            for (int i = 0; i < _location.Length; i++)
                _location[i] = gl._location[i];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="size"></param>
        private void Init(int size)
        {
            _location = new Location[size];
            SetAllLocations(Location.Null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        [Obsolete("Use Get(Geometries.Position)")]
        public Location Get(Positions posIndex) =>
            Get(new Geometries.Position((int)posIndex));

        /// <summary>
        ///
        /// </summary>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public Location Get(Geometries.Position posIndex)
        {
            int index = (int)posIndex;
            if (index < _location.Length)
                return _location[index];
            return Location.Null;
        }

        /// <summary>
        /// Get calls Get(Positions posIndex),
        /// Set calls SetLocation(Positions locIndex, Location locValue)
        /// </summary>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        [Obsolete("Use the Geometries.Position indexer instead")]
        public  Location this[Positions posIndex]
        {
            get => this[new Geometries.Position((int)posIndex)];
            set => this[new Geometries.Position((int)posIndex)] = value;
        }

        /// <summary>
        /// Get calls Get(Positions posIndex),
        /// Set calls SetLocation(Positions locIndex, Location locValue)
        /// </summary>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public Location this[Geometries.Position posIndex]
        {
            get => Get(posIndex);
            set => SetLocation(posIndex, value);
        }

        /// <returns>
        /// <c>true</c> if all locations are Null.
        /// </returns>
        public  bool IsNull
        {
            get
            {
                for (int i = 0; i < _location.Length; i++)
                    if (_location[i] != Location.Null)
                        return false;
                return true;
            }
        }

        /// <returns>
        /// <c>true</c> if any locations are Null.
        /// </returns>
        public  bool IsAnyNull
        {
            get
            {
                for (int i = 0; i < _location.Length; i++)
                    if (_location[i] == Location.Null)
                        return true;
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="le"></param>
        /// <param name="locIndex"></param>
        /// <returns></returns>
        public  bool IsEqualOnSide(TopologyLocation le, int locIndex)
        {
            return _location[locIndex] == le._location[locIndex];
        }

        /// <summary>
        ///
        /// </summary>
        public  bool IsArea => _location.Length > 1;

        /// <summary>
        ///
        /// </summary>
        public  bool IsLine => _location.Length == 1;

        /// <summary>
        ///
        /// </summary>
        public  void Flip()
        {
            if (_location.Length <= 1)
                return;
            var temp = _location[(int)Geometries.Position.Left];
            _location[Geometries.Position.Left] = _location[(int)Geometries.Position.Right];
            _location[Geometries.Position.Right] = temp;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="locValue"></param>
        public  void SetAllLocations(Location locValue)
        {
            for (int i = 0; i < _location.Length; i++)
                _location[i] = locValue;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="locValue"></param>
        public  void SetAllLocationsIfNull(Location locValue)
        {
            for (int i = 0; i < _location.Length; i++)
                if (_location[i] == Location.Null)
                    _location[i] = locValue;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="locIndex"></param>
        /// <param name="locValue"></param>
        [Obsolete("Use SetLocation(Geometries.Position, Location)")]
        public void SetLocation(Positions locIndex, Location locValue) =>
            SetLocation(new Geometries.Position((int)locIndex), locValue);

        /// <summary>
        ///
        /// </summary>
        /// <param name="locIndex"></param>
        /// <param name="locValue"></param>
        public void SetLocation(Geometries.Position locIndex, Location locValue)
        {
            _location[(int)locIndex] = locValue;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="locValue"></param>
        public  void SetLocation(Location locValue)
        {
            SetLocation(Geometries.Position.On, locValue);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public  Location[] GetLocations()
        {
            return _location;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="on"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public  void SetLocations(Location on, Location left, Location right)
        {
            _location[Geometries.Position.On] = on;
            _location[Geometries.Position.Left] = left;
            _location[Geometries.Position.Right] = right;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gl"></param>
        public  void SetLocations(TopologyLocation gl)
        {
            for (int i = 0; i < gl._location.Length; i++)
                _location[i] = gl._location[i];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public  bool AllPositionsEqual(Location loc)
        {
            for (int i = 0; i < _location.Length; i++)
                if (_location[i] != loc)
                    return false;
            return true;
        }

        /// <summary>
        /// Merge updates only the Null attributes of this object
        /// with the attributes of another.
        /// </summary>
        public  void Merge(TopologyLocation gl)
        {
            // if the src is an Area label & and the dest is not, increase the dest to be an Area
            if (gl._location.Length > _location.Length)
            {
                var newLoc = new Location[3];
                newLoc[(int)Geometries.Position.On] = _location[(int)Geometries.Position.On];
                newLoc[(int)Geometries.Position.Left] = Location.Null;
                newLoc[(int)Geometries.Position.Right] = Location.Null;
                _location = newLoc;
            }
            for (int i = 0; i < _location.Length; i++)
                if (_location[i] == Location.Null && i < gl._location.Length)
                    _location[i] = gl._location[i];
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (_location.Length > 1)
                sb.Append(LocationUtility.ToLocationSymbol(_location[(int)Geometries.Position.Left]));
            sb.Append(LocationUtility.ToLocationSymbol(_location[(int)Geometries.Position.On]));
            if (_location.Length > 1)
                sb.Append(LocationUtility.ToLocationSymbol(_location[(int)Geometries.Position.Right]));
            return sb.ToString();
        }
    }
}
