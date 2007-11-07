using System;
using System.Text;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A TopologyLocation is the labelling of a
    /// GraphComponent's topological relationship to a single Geometry.
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
    /// </summary>
    public class TopologyLocation
    {
        private Locations[] location;

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
            location[(Int32) Positions.On] = on;
            location[(Int32) Positions.Left] = left;
            location[(Int32) Positions.Right] = right;
        }

        public TopologyLocation(Locations on)
        {
            Init(1);
            location[(Int32) Positions.On] = on;
        }

        public TopologyLocation(TopologyLocation gl)
        {
            Init(gl.location.Length);

            if (gl != null)
            {
                for (Int32 i = 0; i < location.Length; i++)
                {
                    location[i] = gl.location[i];
                }
            }
        }

        private void Init(Int32 size)
        {
            location = new Locations[size];
            SetAllLocations(Locations.Null);
        }

        public Locations Get(Positions posIndex)
        {
            Int32 index = (Int32) posIndex;
            if (index < location.Length)
            {
                return location[index];
            }
            return Locations.Null;
        }

        /// <summary>
        /// Get calls Get(Positions posIndex),
        /// Set calls SetLocation(Positions locIndex, Locations locValue)
        /// </summary>
        public Locations this[Positions posIndex]
        {
            get { return Get(posIndex); }
            set { SetLocation(posIndex, value); }
        }

        /// <returns>
        /// <c>true</c> if all locations are Null.
        /// </returns>
        public Boolean IsNull
        {
            get
            {
                for (Int32 i = 0; i < location.Length; i++)
                {
                    if (location[i] != Locations.Null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <returns> 
        /// <c>true</c> if any locations are Null.
        /// </returns>
        public Boolean IsAnyNull
        {
            get
            {
                for (Int32 i = 0; i < location.Length; i++)
                {
                    if (location[i] == Locations.Null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public Boolean IsEqualOnSide(TopologyLocation le, Int32 locIndex)
        {
            return location[locIndex] == le.location[locIndex];
        }

        public Boolean IsArea
        {
            get { return location.Length > 1; }
        }

        public Boolean IsLine
        {
            get { return location.Length == 1; }
        }

        public void Flip()
        {
            if (location.Length <= 1)
            {
                return;
            }

            Locations temp = location[(Int32) Positions.Left];
            location[(Int32) Positions.Left] = location[(Int32) Positions.Right];
            location[(Int32) Positions.Right] = temp;
        }

        public void SetAllLocations(Locations locValue)
        {
            for (Int32 i = 0; i < location.Length; i++)
            {
                location[i] = locValue;
            }
        }

        public void SetAllLocationsIfNull(Locations locValue)
        {
            for (Int32 i = 0; i < location.Length; i++)
            {
                if (location[i] == Locations.Null)
                {
                    location[i] = locValue;
                }
            }
        }

        public void SetLocation(Positions locIndex, Locations locValue)
        {
            location[(Int32) locIndex] = locValue;
        }

        public void SetLocation(Locations locValue)
        {
            SetLocation(Positions.On, locValue);
        }

        public Locations[] GetLocations()
        {
            return location;
        }

        public void SetLocations(Locations on, Locations left, Locations right)
        {
            location[(Int32) Positions.On] = on;
            location[(Int32) Positions.Left] = left;
            location[(Int32) Positions.Right] = right;
        }

        public void SetLocations(TopologyLocation gl)
        {
            for (Int32 i = 0; i < gl.location.Length; i++)
            {
                location[i] = gl.location[i];
            }
        }

        public Boolean AllPositionsEqual(Locations loc)
        {
            for (Int32 i = 0; i < location.Length; i++)
            {
                if (location[i] != loc)
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
            if (gl.location.Length > location.Length)
            {
                Locations[] newLoc = new Locations[3];
                newLoc[(Int32) Positions.On] = location[(Int32) Positions.On];
                newLoc[(Int32) Positions.Left] = Locations.Null;
                newLoc[(Int32) Positions.Right] = Locations.Null;
                location = newLoc;
            }

            for (Int32 i = 0; i < location.Length; i++)
            {
                if (location[i] == Locations.Null && i < gl.location.Length)
                {
                    location[i] = gl.location[i];
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (location.Length > 1)
            {
                sb.Append(Location.ToLocationSymbol(location[(Int32) Positions.Left]));
            }

            sb.Append(Location.ToLocationSymbol(location[(Int32) Positions.On]));
            
            if (location.Length > 1)
            {
                sb.Append(Location.ToLocationSymbol(location[(Int32) Positions.Right]));
            }

            return sb.ToString();
        }
    }
}