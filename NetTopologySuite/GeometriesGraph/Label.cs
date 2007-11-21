using System;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A <see cref="Label"/> indicates the topological relationship of a component
    /// of a topology graph to a given <see cref="Geometry{TCoordinate}"/>.
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
    public class Label
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
                lineLabel.SetLocation(i, label.GetLocation(i));
            }

            return lineLabel;
        }

        private TopologyLocation[] _elt = new TopologyLocation[2];

        /// <summary>
        /// Construct a Label with a single location for both Geometries.
        /// Initialize the locations to Null.
        /// </summary>
        public Label(Locations onLoc)
        {
            _elt[0] = new TopologyLocation(onLoc);
            _elt[1] = new TopologyLocation(onLoc);
        }

        /// <summary> 
        /// Construct a Label with a single location for both Geometries.
        /// Initialize the location for the Geometry index.
        /// </summary>
        public Label(Int32 geomIndex, Locations onLoc)
        {
            _elt[0] = new TopologyLocation(Locations.None);
            _elt[1] = new TopologyLocation(Locations.None);
            _elt[geomIndex].setLocation(onLoc);
        }

        /// <summary>
        /// Construct a Label with On, Left and Right locations for both Geometries.
        /// Initialize the locations for both Geometries to the given values.
        /// </summary>
        public Label(Locations onLoc, Locations leftLoc, Locations rightLoc)
        {
            _elt[0] = new TopologyLocation(onLoc, leftLoc, rightLoc);
            _elt[1] = new TopologyLocation(onLoc, leftLoc, rightLoc);
        }

        /// <summary>
        /// Construct a Label with On, Left and Right locations for both Geometries.
        /// Initialize the locations for the given Geometry index.
        /// </summary>
        public Label(Int32 geomIndex, Locations onLoc, Locations leftLoc, Locations rightLoc)
        {
            _elt[0] = new TopologyLocation(Locations.None, Locations.None, Locations.None);
            _elt[1] = new TopologyLocation(Locations.None, Locations.None, Locations.None);
            _elt[geomIndex].setLocations(onLoc, leftLoc, rightLoc);
        }

        /// <summary> 
        /// Construct a Label with the same values as the argument for the
        /// given Geometry index.
        /// </summary>
        public Label(Int32 geomIndex, TopologyLocation gl)
        {
            _elt[0] = new TopologyLocation(gl.GetLocations());
            _elt[1] = new TopologyLocation(gl.GetLocations());
            _elt[geomIndex].setLocations(gl);
        }

        /// <summary> 
        /// Construct a Label with the same values as the argument Label.
        /// </summary>
        public Label(Label lbl)
        {
            _elt[0] = new TopologyLocation(lbl._elt[0]);
            _elt[1] = new TopologyLocation(lbl._elt[1]);
        }

        public void Flip()
        {
            _elt[0].Flip();
            _elt[1].Flip();
        }

        public Locations GetLocation(Int32 geomIndex, Positions posIndex)
        {
            return _elt[geomIndex].Get(posIndex);
        }

        public Locations GetLocation(Int32 geomIndex)
        {
            return _elt[geomIndex].Get(Positions.On);
        }

        public void SetLocation(Int32 geomIndex, Positions posIndex, Locations location)
        {
            _elt[geomIndex].setLocation(posIndex, location);
        }

        public void SetLocation(Int32 geomIndex, Locations location)
        {
            _elt[geomIndex].setLocation(Positions.On, location);
        }

        public void SetAllLocations(Int32 geomIndex, Locations location)
        {
            _elt[geomIndex].setAllLocations(location);
        }

        public void SetAllLocationsIfNull(Int32 geomIndex, Locations location)
        {
            _elt[geomIndex].setAllLocationsIfNull(location);
        }

        public void SetAllLocationsIfNull(Locations location)
        {
            SetAllLocationsIfNull(0, location);
            SetAllLocationsIfNull(1, location);
        }

        /// <summary> 
        /// Merge this label with another one.
        /// Merging updates any null attributes of this label with the 
        /// attributes from <paramref name="label"/>.
        /// </summary>
        /// <param name="label">The <see cref="Label"/> to merge with.</param>
        public void Merge(Label label)
        {
            for (Int32 i = 0; i < 2; i++)
            {
                if (_elt[i] == null && label._elt[i] != null)
                {
                    _elt[i] = new TopologyLocation(label._elt[i]);
                }
                else
                {
                    _elt[i].Merge(label._elt[i]);
                }
            }
        }

        public Int32 GeometryCount
        {
            get
            {
                Int32 count = 0;

                if (!_elt[0].IsNull)
                {
                    count++;
                }

                if (!_elt[1].IsNull)
                {
                    count++;
                }

                return count;
            }
        }

        public Boolean IsNull(Int32 geomIndex)
        {
            return _elt[geomIndex].IsNull;
        }

        public Boolean IsAnyNull(Int32 geomIndex)
        {
            return _elt[geomIndex].IsAnyNull;
        }

        public Boolean IsArea()
        {
            return _elt[0].IsArea || _elt[1].IsArea;
        }

        public Boolean IsArea(Int32 geomIndex)
        {
            return _elt[geomIndex].IsArea;
        }

        public Boolean IsLine(Int32 geomIndex)
        {
            return _elt[geomIndex].IsLine;
        }

        public Boolean IsEqualOnSide(Label lbl, Int32 side)
        {
            return _elt[0].IsEqualOnSide(lbl._elt[0], side)
                   && _elt[1].IsEqualOnSide(lbl._elt[1], side);
        }

        public Boolean AllPositionsEqual(Int32 geomIndex, Locations loc)
        {
            return _elt[geomIndex].AllPositionsEqual(loc);
        }

        /// <summary> 
        /// Converts one GeometryLocation to a Line location.
        /// </summary>
        public void ToLine(Int32 geomIndex)
        {
            if (_elt[geomIndex].IsArea)
            {
                _elt[geomIndex] = new TopologyLocation(_elt[geomIndex].GetLocations()[0]);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (_elt[0] != null)
            {
                sb.Append("a:");
                sb.Append(_elt[0].ToString());
            }

            if (_elt[1] != null)
            {
                sb.Append(" b:");
                sb.Append(_elt[1].ToString());
            }

            return sb.ToString();
        }

        private void SetGeometryLocation(Int32 geomIndex, TopologyLocation tl)
        {
            if (tl == null)
            {
                return;
            }

            _elt[geomIndex].setLocations(tl);
        }
    }
}