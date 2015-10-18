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
    /// Nodes a <see cref="IGeometry"/>s using Snap-Rounding
    /// to a given <see cref="IPrecisionModel"/>.
    /// <list type="Bullet">
    /// <item>Point geometries are not handled.They are skipped if present in the input.
    /// <item>Linestrings which collapse to a point due to snapping are removed.
    /// <item>Polygonal output may not be valid.  
    /// </list>
    /// Invalid output is due to the introduction of topology collapses.
    /// This should be straightforward to clean using standard heuristics(e.g.buffer(0) ).
    /// The geometryinput coordinates are expected to be rounded
    /// to the given precision model.
    /// This class does not perform that function.
    /// <c>GeometryPrecisionReducer</c> may be used to do this.
    /// </summary>
    public class GeometrySnapRounder
    {
        private readonly IPrecisionModel _pm;
        private bool _isLineworkOnly = false;

        /// <summary>
        /// Creates a new snap-rounder which snap-rounds to a grid specified
        /// by the given <see cref="IPrecisionModel"/>.
        /// </summary>
        /// <param name="pm">The precision model for the grid to snap-round to</param>
        public GeometrySnapRounder(IPrecisionModel pm)
        {
            _pm = pm;
        }

        /// <summary>
        /// Gets or sets a value indicating if only the linework should be treated.
        /// </summary>
        public bool LineworkOnly
        {
            get { return _isLineworkOnly; }
            set { _isLineworkOnly = value; }
        }


        /// <summary>
        /// Snap-rounds the given geometry.
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public IGeometry Execute(IGeometry geom)
        {

            // TODO: reduce precision of input automatically
            // TODO: add switch to GeometryPrecisionReducer to NOT check & clean invalid polygonal geometry (not needed here)
            // TODO: OR just do precision reduction with custom code here 

            var segStrings = ExtractTaggedSegmentStrings(geom, _pm);
            snapRound(segStrings);

            if (_isLineworkOnly)
            {
                return ToNodedLines(segStrings, geom.Factory);
            }

            var geomSnapped = ReplaceLines(geom, segStrings);
            var geomClean = EnsureValid(geomSnapped);
            return geomClean;
        }

        private IGeometry ToNodedLines(ICollection<ISegmentString> segStrings, IGeometryFactory geomFact)
        {
            var lines = new List<IGeometry>();
            foreach (NodedSegmentString nss in segStrings)
            {
                // skip collapsed lines
                if (nss.Count < 2)
                    continue;
                //Coordinate[] pts = getCoords(nss);
                var pts = nss.NodeList.GetSplitCoordinates();

                lines.Add(geomFact.CreateLineString(pts));
            }
            return geomFact.BuildGeometry(lines);
        }


        private IGeometry ReplaceLines(IGeometry geom, IList<ISegmentString> segStrings)
        {
            var nodedLinesMap = NodedLinesMap(segStrings);
            var lineReplacer = new GeometryCoordinateReplacer(nodedLinesMap);
            var geomEditor = new GeometryEditor();
            var snapped = geomEditor.Edit(geom, lineReplacer);
            return snapped;
        }

        private void snapRound(IList<ISegmentString> segStrings)
        {
            //Noder sr = new SimpleSnapRounder(pm);
            var sr = new MCIndexSnapRounder(_pm);
            sr.ComputeNodes(segStrings);
        }

        private Dictionary<IGeometry, Coordinate[]> NodedLinesMap(ICollection<ISegmentString> segStrings)
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

        static IList<ISegmentString> ExtractTaggedSegmentStrings(IGeometry geom, IPrecisionModel pm)
        {
            var segStrings = new List<ISegmentString>();
            var filter = new GeometryComponentFilter(
                delegate (IGeometry fgeom)
                {
                    // Extract linework for lineal components only
                    if (!(fgeom is ILineString)) return;
                    // skip empty lines
                    if (geom.NumPoints <= 0) return;
                    var roundPts = Round(((ILineString)geom).CoordinateSequence, pm);
                    segStrings.Add(new NodedSegmentString(roundPts, geom));
                });

            geom.Apply(filter);
            return segStrings;
        }

        static Coordinate[] Round(ICoordinateSequence seq, IPrecisionModel pm)
        {
            if (seq.Count == 0) return new Coordinate[0];

            CoordinateList coordList = new CoordinateList();
            // copy coordinates and reduce
            for (int i = 0; i < seq.Count; i++)
            {
                var coord = new Coordinate(
                    seq.GetOrdinate(i, Ordinate.X),
                    seq.GetOrdinate(i, Ordinate.Y));
                pm.MakePrecise(coord);
                coordList.Add(coord, false);
            }
            Coordinate[] coords = coordList.ToCoordinateArray();

            //TODO: what if seq is too short?
            return coords;
        }

        private static IGeometry EnsureValid(IGeometry geom)
        {
            // TODO: need to ensure all polygonal components are valid
            if (!(geom is IPolygonal)) return geom;
            if (geom.IsValid) return geom;

            return CleanPolygonal(geom);
        }

        private static IGeometry CleanPolygonal(IGeometry geom)
        {
            // TODO: use a better method of removing collapsed topology 
            return geom.Buffer(0);
        }

    }

    internal class GeometryCoordinateReplacer : GeometryEditor.CoordinateSequenceOperation
    {
        private readonly IDictionary<IGeometry, Coordinate[]> _geometryPtsMap;

        public GeometryCoordinateReplacer(IDictionary<IGeometry, Coordinate[]> geometryPtsMap)
        {
            _geometryPtsMap = geometryPtsMap;
            EditSequence = DoEditSequence;
        }

        /// <summary>
        /// Gets the snapped coordinate array for an atomic geometry,
        /// or null if it has collapsed.
        /// </summary>
        /// <param name="coordSeq">The sequence to edit</param>
        /// <param name="geometry">The geometry, the sequence belongs to.</param>
        /// <returns>The snapped coordinate array for this geometry</returns>
        /// <returns><value>null</value> if the snapped coordinates have collapsed, or are missing</returns>
        private ICoordinateSequence DoEditSequence(ICoordinateSequence coordSeq, IGeometry geometry)
        {
            Coordinate[] pts;

            if (_geometryPtsMap.TryGetValue(geometry, out pts))
            {
                // Assert: pts should always have length > 0
                var isValidPts = IsValidSize(pts, geometry);
                if (!isValidPts) return null;

                return geometry.Factory.CoordinateSequenceFactory.Create(pts);
            }

            // TODO: should this return null if no matching snapped line is found
            // probably should never reach here?
            return coordSeq;
        }

        /// <summary>
        /// Tests if a coordinate array has a size which is 
        /// valid for the containing geometry.
        /// </summary>
        /// <param name="pts">The point list to validate</param>
        /// <param name="geom">The atomic geometry containing the point list</param>
        /// <returns><value>true</value> if the coordinate array is a valid size</returns>
        private static bool IsValidSize(Coordinate[] pts, IGeometry geom)
        {
            if (pts.Length == 0) return true;
            int minSize = MinimumNonEmptyCoordinatesSize(geom);
            if (pts.Length < minSize)
            {
                return false;
            }
            return true;
        }

        private static int MinimumNonEmptyCoordinatesSize(IGeometry geom)
        {
            if (geom is ILinearRing)
                return 4;
            if (geom is ILineString)
                return 2;
            return 0;
        }

    }
}