using System;
using System.Text;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph
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
        /// Converts a Label to a Line label (that is, one with no side Location).
        /// </summary>
        /// <param name="label">Label to convert.</param>
        /// <returns>Label as Line label.</returns>
        public static Label ToLineLabel(Label label)
        {
            var lineLabel = new Label(Location.Null);
            for (int i = 0; i < 2; i++)
                lineLabel.SetLocation(i, label.GetLocation(i));
            return lineLabel;
        }

        private readonly TopologyLocation[] elt = new TopologyLocation[2];

        /// <summary>
        /// Construct a Label with a single location for both Geometries.
        /// Initialize the locations to Null.
        /// </summary>
        /// <param name="onLoc">A location value</param>
        public Label(Location onLoc)
        {
            elt[0] = new TopologyLocation(onLoc);
            elt[1] = new TopologyLocation(onLoc);
        }

        /// <summary>
        /// Construct a Label with a single location for both Geometries.
        /// Initialize the location for the Geometry index.
        /// </summary>
        /// <param name="geomIndex">A geometry index, <c>0</c>, or <c>1</c>.</param>
        /// <param name="onLoc">A location value for <b>On</b></param>
        public Label(int geomIndex, Location onLoc)
        {
            elt[0] = new TopologyLocation(Location.Null);
            elt[1] = new TopologyLocation(Location.Null);
            elt[geomIndex].SetLocation(onLoc);
        }

        /// <summary>
        /// Construct a Label with On, Left and Right locations for both Geometries.
        /// Initialize the locations for both Geometries to the given values.
        /// </summary>
        /// <param name="onLoc">A location value for <b>On</b></param>
        /// <param name="leftLoc">A location value for <b>Left</b></param>
        /// <param name="rightLoc">A location value for <b>Right</b></param>
        public Label(Location onLoc, Location leftLoc, Location rightLoc)
        {
            elt[0] = new TopologyLocation(onLoc, leftLoc, rightLoc);
            elt[1] = new TopologyLocation(onLoc, leftLoc, rightLoc);
        }

        /// <summary>
        /// Construct a Label with On, Left and Right locations for both Geometries.
        /// Initialize the locations for the given Geometry index.
        /// </summary>
        /// <param name="geomIndex">A geometry index, <c>0</c>, or <c>1</c>.</param>
        /// <param name="onLoc">A location value for <b>On</b></param>
        /// <param name="leftLoc">A location value for <b>Left</b></param>
        /// <param name="rightLoc">A location value for <b>Right</b></param>
        public Label(int geomIndex, Location onLoc, Location leftLoc, Location rightLoc)
        {
            elt[0] = new TopologyLocation(Location.Null, Location.Null, Location.Null);
            elt[1] = new TopologyLocation(Location.Null, Location.Null, Location.Null);
            elt[geomIndex].SetLocations(onLoc, leftLoc, rightLoc);
        }

        /// <summary>
        /// Construct a Label with the same values as the argument Label.
        /// </summary>
        /// <param name="lbl">A <c>Label</c></param>
        public Label(Label lbl)
        {
            elt[0] = new TopologyLocation(lbl.elt[0]);
            elt[1] = new TopologyLocation(lbl.elt[1]);
        }

        /// <summary>
        /// Performs <see cref="TopologyLocation.Flip"/> on both
        /// <see cref="TopologyLocation"/>s of this <c>Label</c>
        /// </summary>
        public  void Flip()
        {
            elt[0].Flip();
            elt[1].Flip();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        [Obsolete("Use GetLocation(int, Topology.Location)")]
        public Location GetLocation(int geomIndex, Positions posIndex)
            => GetLocation(geomIndex, new Geometries.Position((int) posIndex));

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <returns></returns>
        public Location GetLocation(int geomIndex, Geometries.Position posIndex)
        {
            return elt[geomIndex].Get(posIndex);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public Location GetLocation(int geomIndex)
        {
            return elt[geomIndex].Get(Geometries.Position.On);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <param name="_location"></param>
        [Obsolete("Use SetLocation(int, Geometries.Position, Location)")]
        public void SetLocation(int geomIndex, Positions posIndex, Location _location) =>
            SetLocation(geomIndex, new Geometries.Position((int)posIndex), _location);
        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="posIndex"></param>
        /// <param name="_location"></param>
        public void SetLocation(int geomIndex, Geometries.Position posIndex, Location _location)
        {
            elt[geomIndex].SetLocation(posIndex, _location);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="_location"></param>
        public void SetLocation(int geomIndex, Location _location)
        {
            elt[geomIndex].SetLocation(Geometries.Position.On, _location);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="_location"></param>
        public  void SetAllLocations(int geomIndex, Location _location)
        {
            elt[geomIndex].SetAllLocations(_location);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="_location"></param>
        public  void SetAllLocationsIfNull(int geomIndex, Location _location)
        {
            elt[geomIndex].SetAllLocationsIfNull(_location);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="_location"></param>
        public  void SetAllLocationsIfNull(Location _location)
        {
            SetAllLocationsIfNull(0, _location);
            SetAllLocationsIfNull(1, _location);
        }

        /// <summary>
        /// Merge this label with another one.
        /// Merging updates any null attributes of this label with the attributes from <paramref name="lbl"/>.
        /// </summary>
        /// <param name="lbl">The <c>Label</c> to merge</param>
        public  void Merge(Label lbl)
        {
            for (int i = 0; i < 2; i++)
            {
                if (elt[i] == null && lbl.elt[i] != null)
                     elt[i] = new TopologyLocation(lbl.elt[i]);
                else elt[i].Merge(lbl.elt[i]);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="tl"></param>
        private void SetGeometryLocation(int geomIndex, TopologyLocation tl)
        {
            if (tl == null)
                return;
            elt[geomIndex].SetLocations(tl);
        }

        /// <summary>
        ///
        /// </summary>
        public  int GeometryCount
        {
            get
            {
                int count = 0;
                if (!elt[0].IsNull)
                    count++;
                if (!elt[1].IsNull)
                    count++;
                return count;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public  bool IsNull(int geomIndex)
        {
            return elt[geomIndex].IsNull;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public  bool IsAnyNull(int geomIndex)
        {
            return elt[geomIndex].IsAnyNull;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public  bool IsArea()
        {
            return elt[0].IsArea || elt[1].IsArea;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public bool IsArea(int geomIndex)
        {
            /*  Testing
            if (elt[0].getLocations().length != elt[1].getLocations().length) {
                System.out.println(this);
            }
                */
            return elt[geomIndex].IsArea;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public  bool IsLine(int geomIndex)
        {
            return elt[geomIndex].IsLine;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lbl"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public  bool IsEqualOnSide(Label lbl, int side)
        {
            return  this.elt[0].IsEqualOnSide(lbl.elt[0], side)
                &&  this.elt[1].IsEqualOnSide(lbl.elt[1], side);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        public  bool AllPositionsEqual(int geomIndex, Location loc)
        {
            return elt[geomIndex].AllPositionsEqual(loc);
        }

        /// <summary>
        /// Converts one GeometryLocation to a Line location.
        /// </summary>
        /// <param name="geomIndex">The index of the <c>TopologyLocation</c> to convert (<c>0</c> or <c>1</c>)</param>
        public  void ToLine(int geomIndex)
        {
            if (elt[geomIndex].IsArea)
                elt[geomIndex] = new TopologyLocation(elt[geomIndex].GetLocations()[0]);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (elt[0] != null)
            {
                sb.Append("A:");
                sb.Append(this.elt[0]);
            }
            if (elt[1] != null)
            {
                sb.Append(" B:");
                sb.Append(this.elt[1]);
            }
            return sb.ToString();
        }
    }
}
