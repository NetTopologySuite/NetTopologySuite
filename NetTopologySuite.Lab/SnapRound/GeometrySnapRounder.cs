using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;

namespace NetTopologySuite.SnapRound
{
    /// <summary>
    /// Nodes a <see cref="Geometry"/>s using Snap-Rounding
    /// to a given <see cref="PrecisionModel"/>.
    /// <list type="Bullet">
    /// <item>Point geometries are not handled.They are skipped if present in the input.</item>
    /// <item>Linestrings which collapse to a point due to snapping are removed.</item>
    /// <item>Polygonal output may not be valid.</item>
    /// </list>
    /// Invalid output is due to the introduction of topology collapses.
    /// This should be straightforward to clean using standard heuristics(e.g.buffer(0) ).
    /// The geometry input coordinates are expected to be rounded
    /// to the given precision model.
    /// This class does not perform that function.
    /// <c>GeometryPrecisionReducer</c> may be used to do this.
    /// </summary>
    public class GeometrySnapRounder
    {
        private readonly PrecisionModel _pm;
        private bool _isLineworkOnly;

        /// <summary>
        /// Creates a new snap-rounder which snap-rounds to a grid specified
        /// by the given <see cref="PrecisionModel"/>.
        /// </summary>
        /// <param name="pm">The precision model for the grid to snap-round to</param>
        public GeometrySnapRounder(PrecisionModel pm)
        {
            _pm = pm;
        }

        /// <summary>
        /// Gets or sets a value indicating if only the linework should be treated.
        /// </summary>
        public bool LineworkOnly
        {
            get => _isLineworkOnly;
            set => _isLineworkOnly = value;
        }

        /// <summary>
        /// Snap-rounds the given geometry.
        /// </summary>
        /// <param name="geom">The geometry to snap-round.</param>
        /// <returns>The snap-rounded geometry</returns>
        public Geometry Execute(Geometry geom)
        {

            // TODO: reduce precision of input automatically
            // TODO: add switch to GeometryPrecisionReducer to NOT check & clean invalid polygonal geometry (not needed here)
            // TODO: OR just do precision reduction with custom code here

            var segStrings = ExtractTaggedSegmentStrings(geom, _pm);
            SnapRound(segStrings);

            if (_isLineworkOnly)
            {
                return ToNodedLines(segStrings, geom.Factory);
            }

            var geomSnapped = ReplaceLines(geom, segStrings);
            var geomClean = EnsureValid(geomSnapped);
            return geomClean;
        }

        private Geometry ToNodedLines(ICollection<ISegmentString> segStrings, GeometryFactory geomFact)
        {
            var lines = new List<Geometry>();
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

        private Geometry ReplaceLines(Geometry geom, IList<ISegmentString> segStrings)
        {
            var nodedLinesMap = NodedLinesMap(segStrings);
            var lineReplacer = new GeometryCoordinateReplacer(nodedLinesMap);
            var geomEditor = new GeometryEditorEx(lineReplacer);
            var snapped = geomEditor.Edit(geom);
            return snapped;
        }

        private void SnapRound(IList<ISegmentString> segStrings)
        {
            //Noder sr = new SimpleSnapRounder(pm);
            var sr = new MCIndexSnapRounder(_pm);
            sr.ComputeNodes(segStrings);
        }

        private Dictionary<Geometry, Coordinate[]> NodedLinesMap(ICollection<ISegmentString> segStrings)
        {
            var ptsMap = new Dictionary<Geometry, Coordinate[]>();
            foreach (NodedSegmentString nss in segStrings)
            {
                // skip collapsed lines
                if (nss.Count < 2)
                    continue;
                //Coordinate[] pts = getCoords(nss);
                var pts = nss.NodeList.GetSplitCoordinates();

                ptsMap.Add((Geometry)nss.Context, pts);
            }
            return ptsMap;
        }

        private static IList<ISegmentString> ExtractTaggedSegmentStrings(Geometry geom, PrecisionModel pm)
        {
            var segStrings = new List<ISegmentString>();
            var filter = new GeometryComponentFilter(
                delegate (Geometry fgeom)
                {
                    // Extract linework for lineal components only
                    if (!(fgeom is LineString))
                        return;
                    // skip empty lines
                    if (geom.NumPoints <= 0)
                        return;
                    var roundPts = Round(((LineString)fgeom).CoordinateSequence, pm);
                    segStrings.Add(new NodedSegmentString(roundPts, fgeom));
                });

            geom.Apply(filter);
            return segStrings;
        }

        private static Coordinate[] Round(ICoordinateSequence seq, PrecisionModel pm)
        {
            if (seq.Count == 0) return new Coordinate[0];

            var coordList = new CoordinateList();
            // copy coordinates and reduce
            for (int i = 0; i < seq.Count; i++)
            {
                var coord = new Coordinate(
                    seq.GetOrdinate(i, Ordinate.X),
                    seq.GetOrdinate(i, Ordinate.Y));
                pm.MakePrecise(coord);
                coordList.Add(coord, false);
            }
            var coords = coordList.ToCoordinateArray();

            //TODO: what if seq is too short?
            return coords;
        }

        private static Geometry EnsureValid(Geometry geom)
        {
            if (geom.IsValid)
                return geom;
            return CleanPolygonal(geom);
        }

        private static Geometry CleanPolygonal(Geometry geom)
        {
            return PolygonCleaner.Clean(geom);
        }
    }
}