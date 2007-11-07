using System;
using System.Text;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A <c>Label</c> indicates the topological relationship of a component
    /// of a topology graph to a given <c>Geometry</c>.
    /// This class supports labels for relationships to two <c>Geometry</c>s,
    /// which is sufficient for algorithms for binary operations.
    /// Topology graphs support the concept of labeling nodes and edges in the graph.
    /// The label of a node or edge specifies its topological relationship to one or
    /// more geometries.  (In fact, since NTS operations have only two arguments labels
    /// are required for only two geometries).  A label for a node or edge has one or
    /// two elements, depending on whether the node or edge occurs in one or both of the
    /// input <c>Geometry</c>s.  Elements contain attributes which categorize the
    /// topological location of the node or edge relative to the parent
    /// <c>Geometry</c>; that is, whether the node or edge is in the interior,
    /// boundary or exterior of the <c>Geometry</c>.  Attributes have a value
    /// from the set <c>{Interior, Boundary, Exterior}</c>.  In a node each
    /// element has a single attribute <c>On</c>. For an edge each element has a
    /// triplet of attributes <c>Left, On, Right</c>.
    /// It is up to the client code to associate the 0 and 1 <c>TopologyLocation</c>s
    /// with specific geometries.
    /// </summary>
    public class Label
    {
        /// <summary>
        /// Converts a Label to a Line label (that is, one with no side Locations).
        /// </summary>
        /// <param name="label">Label to convert.</param>
        /// <returns>Label as Line label.</returns>
        public static Label ToLineLabel(Label label)
        {
            Label lineLabel = new Label(Locations.Null);

            for (Int32 i = 0; i < 2; i++)
            {
                lineLabel.SetLocation(i, label.GetLocation(i));
            }

            return lineLabel;
        }

        private TopologyLocation[] elt = new TopologyLocation[2];

        /// <summary>
        /// Construct a Label with a single location for both Geometries.
        /// Initialize the locations to Null.
        /// </summary>
        public Label(Locations onLoc)
        {
            elt[0] = new TopologyLocation(onLoc);
            elt[1] = new TopologyLocation(onLoc);
        }

        /// <summary> 
        /// Construct a Label with a single location for both Geometries.
        /// Initialize the location for the Geometry index.
        /// </summary>
        public Label(Int32 geomIndex, Locations onLoc)
        {
            elt[0] = new TopologyLocation(Locations.Null);
            elt[1] = new TopologyLocation(Locations.Null);
            elt[geomIndex].SetLocation(onLoc);
        }

        /// <summary>
        /// Construct a Label with On, Left and Right locations for both Geometries.
        /// Initialize the locations for both Geometries to the given values.
        /// </summary>
        public Label(Locations onLoc, Locations leftLoc, Locations rightLoc)
        {
            elt[0] = new TopologyLocation(onLoc, leftLoc, rightLoc);
            elt[1] = new TopologyLocation(onLoc, leftLoc, rightLoc);
        }

        /// <summary>
        /// Construct a Label with On, Left and Right locations for both Geometries.
        /// Initialize the locations for the given Geometry index.
        /// </summary>
        public Label(Int32 geomIndex, Locations onLoc, Locations leftLoc, Locations rightLoc)
        {
            elt[0] = new TopologyLocation(Locations.Null, Locations.Null, Locations.Null);
            elt[1] = new TopologyLocation(Locations.Null, Locations.Null, Locations.Null);
            elt[geomIndex].SetLocations(onLoc, leftLoc, rightLoc);
        }

        /// <summary> 
        /// Construct a Label with the same values as the argument for the
        /// given Geometry index.
        /// </summary>
        public Label(Int32 geomIndex, TopologyLocation gl)
        {
            elt[0] = new TopologyLocation(gl.GetLocations());
            elt[1] = new TopologyLocation(gl.GetLocations());
            elt[geomIndex].SetLocations(gl);
        }

        /// <summary> 
        /// Construct a Label with the same values as the argument Label.
        /// </summary>
        public Label(Label lbl)
        {
            elt[0] = new TopologyLocation(lbl.elt[0]);
            elt[1] = new TopologyLocation(lbl.elt[1]);
        }

        public void Flip()
        {
            elt[0].Flip();
            elt[1].Flip();
        }

        public Locations GetLocation(Int32 geomIndex, Positions posIndex)
        {
            return elt[geomIndex].Get(posIndex);
        }

        public Locations GetLocation(Int32 geomIndex)
        {
            return elt[geomIndex].Get(Positions.On);
        }

        public void SetLocation(Int32 geomIndex, Positions posIndex, Locations location)
        {
            elt[geomIndex].SetLocation(posIndex, location);
        }

        public void SetLocation(Int32 geomIndex, Locations location)
        {
            elt[geomIndex].SetLocation(Positions.On, location);
        }

        public void SetAllLocations(Int32 geomIndex, Locations location)
        {
            elt[geomIndex].SetAllLocations(location);
        }

        public void SetAllLocationsIfNull(Int32 geomIndex, Locations location)
        {
            elt[geomIndex].SetAllLocationsIfNull(location);
        }

        public void SetAllLocationsIfNull(Locations location)
        {
            SetAllLocationsIfNull(0, location);
            SetAllLocationsIfNull(1, location);
        }

        /// <summary> 
        /// Merge this label with another one.
        /// Merging updates any null attributes of this label with the attributes from lbl.
        /// </summary>
        public void Merge(Label lbl)
        {
            for (Int32 i = 0; i < 2; i++)
            {
                if (elt[i] == null && lbl.elt[i] != null)
                {
                    elt[i] = new TopologyLocation(lbl.elt[i]);
                }
                else
                {
                    elt[i].Merge(lbl.elt[i]);
                }
            }
        }
        private void SetGeometryLocation(Int32 geomIndex, TopologyLocation tl)
        {
            if (tl == null)
            {
                return;
            }
            elt[geomIndex].SetLocations(tl);
        }

        public Int32 GeometryCount
        {
            get
            {
                Int32 count = 0;

                if (!elt[0].IsNull)
                {
                    count++;
                }

                if (!elt[1].IsNull)
                {
                    count++;
                }

                return count;
            }
        }

        public Boolean IsNull(Int32 geomIndex)
        {
            return elt[geomIndex].IsNull;
        }

        public Boolean IsAnyNull(Int32 geomIndex)
        {
            return elt[geomIndex].IsAnyNull;
        }

        public Boolean IsArea()
        {
            return elt[0].IsArea || elt[1].IsArea;
        }

        public Boolean IsArea(Int32 geomIndex)
        {
            return elt[geomIndex].IsArea;
        }

        public Boolean IsLine(Int32 geomIndex)
        {
            return elt[geomIndex].IsLine;
        }

        public Boolean IsEqualOnSide(Label lbl, Int32 side)
        {
            return elt[0].IsEqualOnSide(lbl.elt[0], side)
                   && elt[1].IsEqualOnSide(lbl.elt[1], side);
        }

        public Boolean AllPositionsEqual(Int32 geomIndex, Locations loc)
        {
            return elt[geomIndex].AllPositionsEqual(loc);
        }

        /// <summary> 
        /// Converts one GeometryLocation to a Line location.
        /// </summary>
        public void ToLine(Int32 geomIndex)
        {
            if (elt[geomIndex].IsArea)
            {
                elt[geomIndex] = new TopologyLocation(elt[geomIndex].GetLocations()[0]);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (elt[0] != null)
            {
                sb.Append("a:");
                sb.Append(elt[0].ToString());
            }

            if (elt[1] != null)
            {
                sb.Append(" b:");
                sb.Append(elt[1].ToString());
            }

            return sb.ToString();
        }
    }
}