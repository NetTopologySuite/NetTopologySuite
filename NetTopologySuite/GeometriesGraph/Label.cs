using System;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A <see cref="Label"/> indicates the topological relationship of a component
    /// of a topology graph to a given <see cref="Geometry{TCoordinate}"/> or pair of 
    /// <see cref="Geometry{TCoordinate}"/> instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class supports labels for relationships to two <see cref="Geometry{TCoordinate}"/>s,
    /// which is sufficient for algorithms for binary operations.
    /// </para>
    /// <para>
    /// Topology graphs support the concept of labeling nodes and edges in the graph.
    /// The label of a node or edge specifies its topological relationship to one or
    /// more geometries.  (In fact, since NTS operations have only two arguments labels
    /// are required for only two geometries).  A label for a node or edge has one or
    /// two elements, depending on whether the node or edge occurs in one or both of 
    /// the input <see cref="Geometry{TCoordinate}"/>s.  Elements contain attributes 
    /// which categorize the topological location of the node or edge relative to the 
    /// parent <see cref="Geometry{TCoordinate}"/>; that is, whether the node or edge 
    /// is in the interior, boundary or exterior of the <see cref="Geometry{TCoordinate}"/>.
    /// Attributes have a value from the set <c>{Interior, Boundary, Exterior}</c>.  
    /// In a node each element has a single attribute <c>On</c>. For an edge each element 
    /// has a triplet of attributes <c>Left, On, Right</c>.
    /// It is up to the client code to associate the 0 and 1 <see cref="TopologyLocation"/>s
    /// with specific geometries.
    /// </para>
    /// </remarks>
    public struct Label
    {
        /// <summary>
        /// Converts a Label to a Line label (that is, one with no side Locations).
        /// </summary>
        /// <param name="label">Label to convert.</param>
        /// <returns>Label as Line label.</returns>
        public static Label ToLineLabel(Label label)
        {
            Label lineLabel = new Label(Locations.None);

            for (Int32 i = 0; i < 2; i++)
            {
                lineLabel = new Label(lineLabel, i, label[i].On);
            }

            return lineLabel;
        }

        private readonly TopologyLocation _g0;
        private readonly TopologyLocation _g1;

        /// <summary>
        /// Construct a Label with a single location for both Geometries.
        /// Initialize the locations to Null.
        /// </summary>
        public Label(Locations on)
        {
            _g0 = new TopologyLocation(on);
            _g1 = new TopologyLocation(on);
        }

        /// <summary> 
        /// Construct a Label with a single location for both Geometries.
        /// Initialize the location for the Geometry index.
        /// </summary>
        public Label(Locations onGeometry1, Locations onGeometry2)
        {
            _g0 = new TopologyLocation(onGeometry1);
            _g1 = new TopologyLocation(onGeometry2);
        }

        /// <summary>
        /// Construct a Label with On, Left and Right locations for both Geometries.
        /// Initialize the locations for both Geometries to the given values.
        /// </summary>
        public Label(Locations on, Locations left, Locations right)
        {
            _g0 = new TopologyLocation(on, left, right);
            _g1 = new TopologyLocation(on, left, right);
        }

        /// <summary>
        /// Construct a Label with On, Left and Right locations for both Geometries.
        /// Initialize the locations for the given Geometry index.
        /// </summary>
        public Label(Locations onGeometry1, Locations leftGeometry1, Locations rightGeometry1, 
            Locations onGeometry2, Locations leftGeometry2, Locations rightGeometry2)
        {
            _g0 = new TopologyLocation(onGeometry1, leftGeometry1, rightGeometry1);
            _g1 = new TopologyLocation(onGeometry2, leftGeometry2, rightGeometry2);
        }

        public Label(Int32 geometryIndex, Locations on)
            : this(geometryIndex, on, Locations.None, Locations.None)
        { }

        public Label(Int32 geometryIndex, Locations on, Locations left, Locations right)
        {
            checkIndex(geometryIndex);

            if (geometryIndex == 0)
            {
                _g0 = new TopologyLocation(on, left, right);
                _g1 = TopologyLocation.None;
            }
            else
            {
                _g0 = TopologyLocation.None;
                _g1 = new TopologyLocation(on, left, right);
            }
        }

        public Label(TopologyLocation geometry1Label, TopologyLocation geometry2Label)
        {
            _g0 = geometry1Label;
            _g1 = geometry2Label;
        }

        public Label(Label other, Int32 geometryIndex, Locations on)
        {
            TopologyLocation newLocation = new TopologyLocation(on);

            if(geometryIndex == 0)
            {
                _g0 = newLocation;
                _g1 = other._g1;
            }
            else
            {
                _g1 = newLocation;
                _g0 = other._g0;
            }
        }

        public Label(Label other, Int32 geometryIndex, Locations on, Locations left, Locations right)
        {
            checkIndex(geometryIndex);

            TopologyLocation newLocation = new TopologyLocation(on, left, right);

            if (geometryIndex == 0)
            {
                _g0 = newLocation;
                _g1 = other._g1;
            }
            else
            {
                _g1 = newLocation;
                _g0 = other._g0;
            }
        }

        public Label(Label other, Int32 geometryIndex, Positions side, Locations location)
        {
            checkIndex(geometryIndex);

            TopologyLocation newLocation = new TopologyLocation(other[geometryIndex], side, location);

            if (geometryIndex == 0)
            {
                _g0 = newLocation;
                _g1 = other._g1;
            }
            else
            {
                _g1 = newLocation;
                _g0 = other._g0;
            }
        }

        public Label Flip()
        {
            Label flipped = new Label(_g0.Flip(), _g1.Flip());
            return flipped;
        }

        public Locations this[Int32 geometryIndex, Positions position]
        {
            get
            {
                switch (geometryIndex)
                {
                    case 0 :
                        return _g0[position];
                    case 1:
                        return _g1[position];
                    default:
                        checkIndex(geometryIndex);
                        Assert.ShouldNeverReachHere();
                        return Locations.None;
                }
            }
        }

        public TopologyLocation this[Int32 geometryIndex]
        {
            get
            {
                switch (geometryIndex)
                {
                    case 0:
                        return _g0;
                    case 1:
                        return _g1;
                    default:
                        break;
                }

                checkIndex(geometryIndex);
                Assert.ShouldNeverReachHere();
                return TopologyLocation.None;
            }
        }

        //public void SetLocation(Int32 geomIndex, Positions posIndex, Locations location)
        //{
        //    _elt[geomIndex].setLocation(posIndex, location);
        //}

        //public void SetLocation(Int32 geomIndex, Locations location)
        //{
        //    _elt[geomIndex].setLocation(Positions.On, location);
        //}

        public static Label SetAllLocations(Label label, Int32 geometryIndex, Locations location)
        {
            checkIndex(geometryIndex);

            // Real estate!
            TopologyLocation newLocation = new TopologyLocation(location, location, location);

            if (geometryIndex == 0)
            {
                return new Label(newLocation, label._g1);
            }
            else
            {
                return new Label(label._g0, newLocation);
            }
        }

        public static Label SetAllLocationsIfNull(Label label, Int32 geometryIndex, Locations location)
        {
            checkIndex(geometryIndex);

            TopologyLocation labelLocation = label[geometryIndex];

            TopologyLocation newLocation = new TopologyLocation(
                labelLocation.On == Locations.None ? location : labelLocation.On, 
                labelLocation.Left == Locations.None ? location : labelLocation.Left, 
                labelLocation.Right == Locations.None ? location : labelLocation.Right);

            if (geometryIndex == 0)
	        {
	            return new Label(newLocation, label._g1);
	        }
            else
            {
                return new Label(label._g0, newLocation);
            }
        }

        public static Label SetAllLocationsIfNull(Label label, Locations location)
        {
            label = SetAllLocationsIfNull(label, 0, location);
            label = SetAllLocationsIfNull(label, 1, location);
            return label;
        }

        /// <summary> 
        /// Merge this label with another one.
        /// Merging updates any null attributes of this label with the 
        /// attributes from <paramref name="label"/>.
        /// </summary>
        /// <param name="label">The <see cref="Label"/> to merge with.</param>
        public Label Merge(Label label)
        {
            TopologyLocation g0 = _g0, g1 = _g1;

            if (g0.IsNull && !label._g0.IsNull)
            {
                g0 = label._g0;
            }
            else
            {
                g0 = g0.Merge(label._g0);
            }

            if (g1.IsNull && !label._g1.IsNull)
            {
                g1 = label._g1;
            }
            else
            {
                g1 = g1.Merge(label._g1);
            }

            return new Label(g0, g1);
        }

        public Int32 GeometryCount
        {
            get
            {
                Int32 count = 0;

                if (!_g0.IsNull)
                {
                    count++;
                }

                if (!_g1.IsNull)
                {
                    count++;
                }

                return count;
            }
        }

        public Boolean IsNull(Int32 geometryIndex)
        {
            switch (geometryIndex)
            {
                case 0 :
                    return _g0.IsNull;
                case 1:
                    return _g1.IsNull;
                default:
                    checkIndex(geometryIndex);
                    Assert.ShouldNeverReachHere();
                    return false;
            }
        }

        public Boolean AreAnyNull(Int32 geometryIndex)
        {
            switch (geometryIndex)
            {
                case 0:
                    return _g0.AreAnyNull;
                case 1:
                    return _g1.AreAnyNull;
                default:
                    checkIndex(geometryIndex);
                    Assert.ShouldNeverReachHere();
                    return false;
            }
        }

        public Boolean IsArea()
        {
            return _g0.IsArea || _g1.IsArea;
        }

        public Boolean IsArea(Int32 geometryIndex)
        {
            switch (geometryIndex)
            {
                case 0 :
                    return _g0.IsArea;
                case 1:
                    return _g1.IsArea;
                default:
                    checkIndex(geometryIndex);
                    Assert.ShouldNeverReachHere();
                    return false;
            }
        }

        public Boolean IsLine(Int32 geometryIndex)
        {
            switch (geometryIndex)
            {
                case 0:
                    return _g0.IsLine;
                case 1:
                    return _g1.IsLine;
                default:
                    checkIndex(geometryIndex);
                    Assert.ShouldNeverReachHere();
                    return false;
            }
        }

        public Boolean IsEqualOnSide(Label label, Positions side)
        {
            return _g0.IsEqualOnSide(label._g0, side)
                   && _g1.IsEqualOnSide(label._g1, side);
        }

        public Boolean AllPositionsEqual(Int32 geometryIndex, Locations location)
        {
            switch (geometryIndex)
            {
                case 0:
                    return _g0.AllPositionsEqual(location);
                case 1:
                    return _g1.AllPositionsEqual(location);
                default:
                    checkIndex(geometryIndex);
                    Assert.ShouldNeverReachHere();
                    return false;
            }
        }

        /// <summary> 
        /// Converts one TopologyLocation to a Line location.
        /// </summary>
        public Label ToLine(Int32 geometryIndex)
        {
            switch (geometryIndex)
            {
                case 0:
                    if (_g0.IsArea)
                    {
                        return new Label(new TopologyLocation(_g0.On), _g1);
                    }
                    break;
                case 1:
                    if (_g1.IsArea)
                    {
                        return new Label(_g0, new TopologyLocation(_g1.On));
                    }
                    break;
                default:
                    checkIndex(geometryIndex);
                    Assert.ShouldNeverReachHere();
                    break;
            }

            return this;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("a:");
            sb.Append(_g0.ToString());

            sb.Append(" b:");
            sb.Append(_g1.ToString());

            return sb.ToString();
        }

        //private void SetGeometryLocation(Int32 geomIndex, TopologyLocation tl)
        //{
        //    if (tl == null)
        //    {
        //        return;
        //    }

        //    _elt[geomIndex].setLocations(tl);
        //}

        private static void checkIndex(Int32 geometryIndex)
        {
            if (geometryIndex != 0 && geometryIndex != 1)
            {
                throw new ArgumentOutOfRangeException("geometryIndex", geometryIndex,
                                                      "Geometry index must be 0 or 1.");
            }
        }
    }
}