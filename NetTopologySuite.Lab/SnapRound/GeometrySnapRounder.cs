using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;

namespace NetTopologySuite.SnapRound
{
    /// <summary>
    /// Nodes a <see cref="IGeometry"/>s using Snap-Rounding
    /// to a given <see cref="IPrecisionModel"/>.
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
        private readonly IPrecisionModel _pm;
        private bool _isLineworkOnly;

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
            get => _isLineworkOnly;
            set => _isLineworkOnly = value;
        }

        /// <summary>
        /// Snap-rounds the given geometry.
        /// </summary>
        /// <param name="geom">The geometry to snap-round.</param>
        /// <returns>The snap-rounded geometry</returns>
        public IGeometry Execute(IGeometry geom)
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

        private static IList<ISegmentString> ExtractTaggedSegmentStrings(IGeometry geom, IPrecisionModel pm)
        {
            var segStrings = new List<ISegmentString>();
            var filter = new GeometryComponentFilter(
                delegate (IGeometry fgeom)
                {
                    // Extract linework for lineal components only
                    if (!(fgeom is ILineString))
                        return;
                    // skip empty lines
                    if (geom.NumPoints <= 0)
                        return;
                    var roundPts = Round(((ILineString)fgeom).CoordinateSequence, pm);
                    segStrings.Add(new NodedSegmentString(roundPts, fgeom));
                });

            geom.Apply(filter);
            return segStrings;
        }

        private static Coordinate[] Round(ICoordinateSequence seq, IPrecisionModel pm)
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
            if (geom.IsValid)
                return geom;
            return CleanPolygonal(geom);
        }

        private static IGeometry CleanPolygonal(IGeometry geom)
        {
            return PolygonCleaner.Clean(geom);
        }
    }
}