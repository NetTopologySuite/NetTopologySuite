using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.SnapRound
{
    /// <summary>
    /// Nodes the linework in a list of <see cref="IGeometry"/>s using Snap-Rounding
    /// to a given <see cref="IPrecisionModel"/>.
    /// <para/>
    /// The input coordinates are expected to be rounded
    /// to the given precision model.
    /// This class does not perform that function.
    /// <c>GeometryPrecisionReducer</c> may be used to do this.
    /// <para/>
    /// This class does <b>not</b> dissolve the output linework,
    /// so there may be duplicate linestrings in the output.  
    /// Subsequent processing (e.g. polygonization) may require
    /// the linework to be unique.  Using <c>UnaryUnion</c> is one way
    /// to do this (although this is an inefficient approach).
    /// </summary>
    public class GeometrySnapRounder
    {
        private IGeometryFactory _geomFact;
        private readonly IPrecisionModel _pm;

        /// <summary>
        /// Creates a new noder which snap-rounds to a grid specified
        /// by the given <see cref="IPrecisionModel"/>.
        /// </summary>
        /// <param name="pm">The precision model for the grid to snap-round to</param>
        public GeometrySnapRounder(IPrecisionModel pm)
        {
            _pm = pm;
        }

        /// <summary> 
        /// Nodes the linework of a set of Geometrys using SnapRounding.
        /// </summary>
        /// <param name="geoms">A collection of geometries</param>
        /// <returns>A collection of LineString geometries representing the noded linework of the input</returns>
        public ICollection<IGeometry> Node(ICollection<IGeometry> geoms)
        {
            // get geometry factory
            _geomFact = FunctionsUtil.GetFactoryOrDefault(geoms);

            var segStrings = ExtractSegmentStrings(geoms);
            //Noder sr = new SimpleSnapRounder(pm);
            var sr = new MCIndexSnapRounder(_pm);
            sr.ComputeNodes(segStrings);

            var nodedLines = GetNodedLines(segStrings);
            return nodedLines;

        }

        public IGeometry Node(IGeometry geom)
        {

            var geomList = new List<IGeometry>();
            geomList.Add(geom);
            var segStrings = ExtractSegmentStrings(geomList);
            //Noder sr = new SimpleSnapRounder(pm);
            var sr = new MCIndexSnapRounder(_pm);
            sr.ComputeNodes(segStrings);


            var nodedPtsMap = GetNodedPtsMap(segStrings);
            var lineReplacer = new GeometryLineReplacer(nodedPtsMap);
            var geomEditor = new GeometryEditor();
            var snapped = geomEditor.Edit(geom, lineReplacer);
            return snapped;

        }

        private static IList<ISegmentString> ExtractSegmentStrings(IEnumerable<IGeometry> geoms)
        {
            var segStrings = new List<ISegmentString>();
            var filter = new GeometryComponentFilter(
                g => { if (g is ILineString) segStrings.Add(new NodedSegmentString(g.Coordinates, g)); });

            foreach (var geom in geoms)
                geom.Apply(filter);
            return segStrings;
        }

        private List<IGeometry> GetNodedLines(IEnumerable<ISegmentString> segStrings)
        {
            var lines = new List<IGeometry>();
            foreach (NodedSegmentString nss in segStrings)
            {
                // skip collapsed lines
                if (nss.Count < 2)
                    continue;
                //Coordinate[] pts = getCoords(nss);
                var pts = nss.NodeList.GetSplitCoordinates();

                lines.Add(_geomFact.CreateLineString(pts));
            }
            return lines;
        }

        private static IDictionary<IGeometry, Coordinate[]> GetNodedPtsMap(IEnumerable<ISegmentString> segStrings)
        {
            var ptsMap = new Dictionary<IGeometry, Coordinate[]>();
            foreach (NodedSegmentString nss in segStrings)
            {
                // skip collapsed lines
                if (nss.Count < 2)
                    continue;
                //Coordinate[] pts = getCoords(nss);
                Coordinate[] pts = nss.NodeList.GetSplitCoordinates();

                ptsMap.Add((IGeometry)nss.Context, pts);
            }
            return ptsMap;
        }

        /*
  private Coordinate[] getCoords(NodedSegmentString nss) {
    List edges = new ArrayList();
    nss.getNodeList().addSplitEdges(edges);
    CoordinateList coordList = new CoordinateList();
    for (Iterator it = edges.iterator(); it.hasNext(); ) {
      SegmentString ss = (SegmentString) it.next();
      Coordinate[] coords = ss.getCoordinates();
      coordList.add(coords, false);
    }    
    
    Coordinate[] pts = coordList.toCoordinateArray();
    return pts;
  }
*/

    }

    internal class GeometryLineReplacer : GeometryEditor.CoordinateSequenceOperation
    {
        private readonly IDictionary<IGeometry, Coordinate[]> _geometryPtsMap;

        public GeometryLineReplacer(IDictionary<IGeometry, Coordinate[]> geometryPtsMap)
        {
            _geometryPtsMap = geometryPtsMap;
            EditSequence = DoEditSequence;
        }

        private ICoordinateSequence DoEditSequence(ICoordinateSequence coordSeq, IGeometry geometry)
        {
            Coordinate[] res;
            if (_geometryPtsMap.TryGetValue(geometry, out res))
                return geometry.Factory.CoordinateSequenceFactory.Create(res);
            return coordSeq;
        }

    }
}