using System;
using System.Text;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A <see cref="Label"/> indicates the topological relationship of a component
    /// of a topology graph to a given <see cref="Geometry{TCoordinate}"/> or pair of 
    /// <see cref="Geometry{TCoordinate}"/> instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class supports labels for relationships to two 
    /// <see cref="Geometry{TCoordinate}"/>s, which is sufficient for 
    /// algorithms for binary operations.
    /// </para>
    /// <para>
    /// Topology graphs support the concept of labeling nodes and edges in the graph.
    /// The label of a node or edge specifies its topological relationship to one or
    /// more geometries.  (In fact, since NTS operations have only two arguments, labels
    /// are required for only two geometries).  A label for a node or edge has one or
    /// two elements, known as <see cref="TopologyLocation"/>s, depending on 
    /// whether the node or edge occurs in one or both of the input 
    /// <see cref="Geometry{TCoordinate}"/>s.  <see cref="TopologyLocation"/>s contain 
    /// attributes of type <see cref="Locations"/> which categorize the 
    /// topological location of the node or edge relative to the parent 
    /// <see cref="Geometry{TCoordinate}"/>; that is, whether 
    /// the node or edge is in the interior, boundary or exterior of the 
    /// <see cref="Geometry{TCoordinate}"/>.
    /// </para>
    /// <para>
    /// Attributes have a value from the set <c>{Interior, Boundary, Exterior}</c>.  
    /// In a node each element has a single attribute <c>On</c>. For an edge each element 
    /// has a triplet of attributes <c>Left, On, Right</c>.
    /// It is up to the client code to associate the 0 and 1 
    /// <see cref="TopologyLocation"/>s with specific geometries.
    /// </para>
    /// </remarks>
    public struct Label
    {
        /// <summary>
        /// Converts a label to a line label (that is, one with no side Locations).
        /// </summary>
        /// <param name="label">Label to convert.</param>
        /// <returns>Label as line label.</returns>
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
        /// Construct a <see cref="Label"/> with a single location for 
        /// both geometries.
        /// </summary>
        public Label(Locations on)
        {
            _g0 = new TopologyLocation(on);
            _g1 = new TopologyLocation(on);
        }

        /// <summary> 
        /// Construct a <see cref="Label"/> with the given 
        /// <see cref="Positions.On"/> location 
        /// for the respective geometries.
        /// </summary>
        public Label(Locations onGeometry1, Locations onGeometry2)
        {
            _g0 = new TopologyLocation(onGeometry1);
            _g1 = new TopologyLocation(onGeometry2);
        }

        /// <summary>
        /// Construct a <see cref="Label"/> with <see cref="Positions.On"/>, 
        /// <see cref="Positions.Left"/> and <see cref="Positions.Right"/>
        /// locations for both geometries.
        /// </summary>
        public Label(Locations on, Locations left, Locations right)
        {
            _g0 = new TopologyLocation(on, left, right);
            _g1 = new TopologyLocation(on, left, right);
        }

        /// <summary>
        /// Construct a <see cref="Label"/> with <see cref="Positions.On"/>, 
        /// <see cref="Positions.Left"/> and <see cref="Positions.Right"/> 
        /// locations for each respective geometry.
        /// </summary>
        public Label(Locations onGeometry1, Locations leftGeometry1, Locations rightGeometry1,
                     Locations onGeometry2, Locations leftGeometry2, Locations rightGeometry2)
        {
            _g0 = new TopologyLocation(onGeometry1, leftGeometry1, rightGeometry1);
            _g1 = new TopologyLocation(onGeometry2, leftGeometry2, rightGeometry2);
        }

        /// <summary>
        /// Construct a <see cref="Label"/> with <see cref="TopologyLocation.On"/>
        /// location for the geometry at <paramref name="geometryIndex"/>.
        /// </summary>
        /// <param name="geometryIndex">
        /// The geometry to label the <see cref="TopologyLocation.On"/> with the value
        /// <paramref name="on"/>.
        /// </param>
        /// <param name="on">The <see cref="Locations"/> value to label with.</param>
        public Label(Int32 geometryIndex, Locations on)
            : this(geometryIndex, on, Locations.None, Locations.None) { }

        /// <summary>
        /// Construct a <see cref="Label"/> with <see cref="TopologyLocation.On"/>,
        /// <see cref="TopologyLocation.Left"/>, and <see cref="TopologyLocation.Right"/>
        /// locations set to <paramref name="on"/>, <paramref name="left"/> and 
        /// <paramref name="right"/>, respectively, for the geometry at 
        /// <paramref name="geometryIndex"/>.
        /// </summary>
        /// <param name="geometryIndex">
        /// The geometry to label the <see cref="Positions.On"/> with the value
        /// <paramref name="on"/>.
        /// </param>
        /// <param name="on">
        /// The <see cref="Locations"/> value to set the corresponding 
        /// <see cref="TopologyLocation.On"/> to.
        /// </param>
        /// <param name="left">
        /// The <see cref="Locations"/> value to set the corresponding 
        /// <see cref="TopologyLocation.Left"/> to.
        /// </param>
        /// <param name="right">
        /// The <see cref="Locations"/> value to set the corresponding 
        /// <see cref="TopologyLocation.Right"/> to.
        /// </param>
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

        /// <summary>
        /// Construct a <see cref="Label"/> with the given labels for 
        /// each respective geometry.
        /// </summary>
        /// <param name="geometry1Label">The label for the first geometry.</param>
        /// <param name="geometry2Label">The label for the second geometry.</param>
        public Label(TopologyLocation geometry1Label, TopologyLocation geometry2Label)
        {
            _g0 = geometry1Label;
            _g1 = geometry2Label;
        }

        /// <summary>
        /// Construct a <see cref="Label"/> with the same values as 
        /// <paramref name="other"/>, except for the geometry at 
        /// <paramref name="geometryIndex"/>, where <see cref="TopologyLocation.On"/>
        /// will be set to <paramref name="on"/>.
        /// </summary>
        /// <param name="other">The <see cref="Label"/> to copy.</param>
        /// <param name="geometryIndex">
        /// The geometry to set a new <see cref="TopologyLocation.On"/> value for.
        /// </param>
        /// <param name="on">
        /// The value to set the <see cref="TopologyLocation.On"/> position of 
        /// <paramref name="geometryIndex"/> to.
        /// </param>
        public Label(Label other, Int32 geometryIndex, Locations on)
        {
            TopologyLocation newLocation = new TopologyLocation(on);

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

        /// <summary>
        /// Construct a <see cref="Label"/> with the same values as 
        /// <paramref name="other"/>, except for the geometry at 
        /// <paramref name="geometryIndex"/>, where the positions 
        /// <see cref="TopologyLocation.On"/>, <see cref="TopologyLocation.Left"/>
        /// and <see cref="TopologyLocation.Right"/> will be set to
        /// <paramref name="on"/>, <paramref name="left"/> and <paramref name="right"/>,
        /// respectively.
        /// </summary>
        /// <param name="other">The <see cref="Label"/> to copy.</param>
        /// <param name="geometryIndex">
        /// The geometry to set a new <see cref="TopologyLocation.On"/> value for.
        /// </param>
        /// <param name="on">
        /// The value to set the <see cref="TopologyLocation.On"/> position of 
        /// <paramref name="geometryIndex"/> to.
        /// </param>
        /// <param name="left">
        /// The value to set the <see cref="TopologyLocation.Left"/> position of 
        /// <paramref name="geometryIndex"/> to.
        /// </param>
        /// <param name="right">
        /// The value to set the <see cref="TopologyLocation.Right"/> position of 
        /// <paramref name="geometryIndex"/> to.
        /// </param>
        public Label(Label other, Int32 geometryIndex,
                     Locations on, Locations left, Locations right)
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

        /// <summary>
        /// Construct a <see cref="Label"/> with the same values as 
        /// <paramref name="other"/>, except for the geometry at 
        /// <paramref name="geometryIndex"/>, where the position
        /// <paramref name="side"/> will be set to
        /// <paramref name="location"/> respectively.
        /// </summary>
        /// <param name="other">The <see cref="Label"/> to copy.</param>
        /// <param name="geometryIndex">
        /// The geometry to set a new value at <paramref name="side"/> for.
        /// </param>
        /// <param name="side">
        /// The position of the <see cref="TopologyLocation"/> to set to 
        /// <paramref name="location"/>.
        /// </param>
        /// <param name="location">
        /// The value to set the given <paramref name="side"/> to.
        /// </param>
        public Label(Label other, Int32 geometryIndex, Positions side, Locations location)
        {
            checkIndex(geometryIndex);

            TopologyLocation newLocation = new TopologyLocation(other[geometryIndex],
                                                                side,
                                                                location);

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

        /// <summary>
        /// Computes a <see cref="Label"/> which has each of the 
        /// <see cref="TopologyLocation"/>s flipped.
        /// </summary>
        /// <returns>
        /// A <see cref="Label"/> which has each <see cref="TopologyLocation"/>
        /// flipped.
        /// </returns>
        /// <seealso cref="TopologyLocation.Flip"/>
        public Label Flip()
        {
            Label flipped = new Label(_g0.Flip(), _g1.Flip());
            return flipped;
        }

        /// <summary>
        /// Gets the <see cref="Locations"/> value for the given 
        /// <paramref name="position"/> for the geometry at
        /// <paramref name="geometryIndex"/>.
        /// </summary>
        /// <param name="geometryIndex">The index of the geometry to lookup.</param>
        /// <param name="position">T
        /// he <see cref="Positions"/> to get the <see cref="Locations"/> value at.
        /// </param>
        /// <returns>
        /// The <see cref="Locations"/> value stored for the geometry at 
        /// the given position.
        /// </returns>
        public Locations this[Int32 geometryIndex, Positions position]
        {
            get
            {
                switch (geometryIndex)
                {
                    case 0:
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

        /// <summary>
        /// Gets the <see cref="TopologyLocation"/> for the geometry
        /// at <paramref name="geometryIndex"/>.
        /// </summary>
        /// <param name="geometryIndex">
        /// The index of the geometry to get the corresponding 
        /// <see cref="TopologyLocation"/> for.
        /// </param>
        /// <returns>
        /// The <see cref="TopologyLocation"/> for the geometry
        /// at <paramref name="geometryIndex"/>.
        /// </returns>
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

        /// <summary>
        /// Makes a copy of a <see cref="Label"/> with all the positions
        /// for the geometry at <paramref name="geometryIndex"/> set to 
        /// <paramref name="location"/>.
        /// </summary>
        /// <param name="label">The <see cref="label"/> to copy.</param>
        /// <param name="geometryIndex">
        /// The index of the geometry to set all locations.
        /// </param>
        /// <param name="location">
        /// The <see cref="Locations"/> value to set all the positions to.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="label"/> with all positions set to 
        /// <paramref name="location"/>.
        /// </returns>
        public static Label SetAllPositions(Label label,
                                            Int32 geometryIndex,
                                            Locations location)
        {
            checkIndex(geometryIndex);

            TopologyLocation newLocation = label.IsArea()
                            ? new TopologyLocation(location, location, location)
                            : new TopologyLocation(location);

            return geometryIndex == 0
                ? new Label(newLocation, label._g1)
                : new Label(label._g0, newLocation);
        }

        /// <summary>
        /// Makes a copy of a <see cref="Label"/> with all the positions
        /// for the geometry at <paramref name="geometryIndex"/> set to 
        /// <paramref name="location"/> if they are equal to 
        /// <see cref="Locations.None"/>.
        /// </summary>
        /// <param name="label">The <see cref="label"/> to copy.</param>
        /// <param name="geometryIndex">
        /// The index of the geometry to set all locations.
        /// </param>
        /// <param name="location">
        /// The <see cref="Locations"/> value to set any positions 
        /// equal to <see cref="Locations.None"/> to.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="label"/> with any positions
        /// which had been <see cref="Locations.None"/> set to 
        /// <paramref name="location"/> for the geometry at 
        /// <paramref name="geometryIndex"/>.
        /// </returns>
        public static Label SetAllPositionsIfNone(Label label,
                                                  Int32 geometryIndex,
                                                  Locations location)
        {
            checkIndex(geometryIndex);

            TopologyLocation labelLocation = label[geometryIndex];

            // TODO: rewrite using bitwise operators
            TopologyLocation newLocation = new TopologyLocation(
                labelLocation.On == Locations.None ? location : labelLocation.On,
                labelLocation.Left == Locations.None ? location : labelLocation.Left,
                labelLocation.Right == Locations.None ? location : labelLocation.Right);

            return geometryIndex == 0
                ? new Label(newLocation, label._g1)
                : new Label(label._g0, newLocation);
        }

        /// <summary>
        /// Makes a copy of a <see cref="Label"/> with all the positions
        /// for both geometries, if they are equal to <see cref="Locations.None"/>,
        /// set to <paramref name="location"/>.
        /// </summary>
        /// <param name="label">The <see cref="label"/> to copy.</param>
        /// <param name="location">
        /// The <see cref="Locations"/> value to set any positions 
        /// equal to <see cref="Locations.None"/> to.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="label"/> with any positions
        /// which had been <see cref="Locations.None"/> set to 
        /// <paramref name="location"/>.
        /// </returns>
        public static Label SetAllPositionsIfNone(Label label, Locations location)
        {
            label = SetAllPositionsIfNone(label, 0, location);
            label = SetAllPositionsIfNone(label, 1, location);
            return label;
        }

        /// <summary> 
        /// Merge this label with another one.
        /// Merging updates any positions of this label which equal 
        /// <see cref="Locations.None"/> with the 
        /// positions from <paramref name="label"/>.
        /// </summary>
        /// <param name="label">The <see cref="Label"/> to merge with.</param>
        public Label Merge(Label? label)
        {
            if (label == null)
            {
                return this;
            }

            Label other = label.Value;

            TopologyLocation l0 = _g0, l1 = _g1;

            if (l0.IsNone && !other._g0.IsNone)
            {
                l0 = other._g0;
            }
            else
            {
                l0 = l0.Merge(other._g0);
            }

            if (l1.IsNone && !other._g1.IsNone)
            {
                l1 = other._g1;
            }
            else
            {
                l1 = l1.Merge(other._g1);
            }

            return new Label(l0, l1);
        }

        /// <summary>
        /// The number of geometries represented by this <see cref="Label"/>.
        /// </summary>
        /// <remarks>
        /// Each <see cref="Label"/> might represent 0, 1 or 2 geometries.
        /// </remarks>
        public Int32 GeometryCount
        {
            get
            {
                Int32 count = 0;

                if (!_g0.IsNone)
                {
                    count++;
                }

                if (!_g1.IsNone)
                {
                    count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the geometry at
        /// <paramref name="geometryIndex"/> has no set value.
        /// </summary>
        /// <param name="geometryIndex">
        /// The index of geometry to check.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the geometry represented at 
        /// <paramref name="geometryIndex"/> has no value; 
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TopologyLocation.IsNone"/>
        public Boolean IsNone(Int32 geometryIndex)
        {
            switch (geometryIndex)
            {
                case 0:
                    return _g0.IsNone;
                case 1:
                    return _g1.IsNone;
                default:
                    checkIndex(geometryIndex);
                    Assert.ShouldNeverReachHere();
                    return false;
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if any position geometry at
        /// <paramref name="geometryIndex"/> has no set value.
        /// </summary>
        /// <param name="geometryIndex">
        /// The index of geometry to check.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if any position of the geometry represented at 
        /// <paramref name="geometryIndex"/> has no value; 
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TopologyLocation.AreAnyNone"/>
        public Boolean AreAnyNone(Int32 geometryIndex)
        {
            switch (geometryIndex)
            {
                case 0:
                    return _g0.AreAnyNone;
                case 1:
                    return _g1.AreAnyNone;
                default:
                    checkIndex(geometryIndex);
                    Assert.ShouldNeverReachHere();
                    return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any <see cref="TopologyLocation"/>
        /// represents an area.
        /// </summary>
        /// <seealso cref="TopologyLocation.IsArea"/>
        public Boolean IsArea()
        {
            return _g0.IsArea || _g1.IsArea;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TopologyLocation"/>
        /// of the geometry at <paramref name="geometryIndex"/>
        /// represents an area.
        /// </summary>
        /// <param name="geometryIndex">
        /// The index of the represented geometry to check.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the geometry at <paramref name="geometryIndex"/> 
        /// is an area; <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TopologyLocation.IsArea"/>
        public Boolean IsArea(Int32 geometryIndex)
        {
            switch (geometryIndex)
            {
                case 0:
                    return _g0.IsArea;
                case 1:
                    return _g1.IsArea;
                default:
                    checkIndex(geometryIndex);
                    Assert.ShouldNeverReachHere();
                    return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TopologyLocation"/>
        /// of the geometry at <paramref name="geometryIndex"/>
        /// represents a line.
        /// </summary>
        /// <param name="geometryIndex">
        /// The index of the represented geometry to check.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the geometry at <paramref name="geometryIndex"/> 
        /// is a line; <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso cref="TopologyLocation.IsLine"/>
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

        /// <summary>
        /// Compares the <see cref="Label"/> and another Label to determine
        /// if all the <see cref="Locations"/> values are the same on the given
        /// <paramref name="side"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Label"/> to compare.</param>
        /// <param name="side">
        /// The <see cref="Positions"/> value indicating the side to compare.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the labels share the same values on the given 
        /// <paramref name="side"/>; <see langword="false"/> otherwise.
        /// </returns>
        public Boolean IsEqualOnSide(Label other, Positions side)
        {
            return _g0.IsEqualOnSide(other._g0, side) &&
                   _g1.IsEqualOnSide(other._g1, side);
        }

        /// <summary>
        /// Compares the <see cref="Locations"/> values of all the positions
        /// for the geometry represented at <paramref name="geometryIndex"/>
        /// are equal to <paramref name="location"/>.
        /// </summary>
        /// <param name="geometryIndex">
        /// The index of the represented geometry to check.
        /// </param>
        /// <param name="location">The <see cref="Locations"/> value to compare to.</param>
        /// <returns>
        /// <see langword="true"/> if the positions equal the given 
        /// <paramref name="location"/>; <see langword="false"/> otherwise.
        /// </returns>
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
        /// Converts the <see cref="TopologyLocation"/> at 
        /// <paramref name="geometryIndex"/> to a line location.
        /// </summary>
        /// <param name="geometryIndex">
        /// The index of the geometry location to convert.
        /// </param>
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

        public override String ToString()
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