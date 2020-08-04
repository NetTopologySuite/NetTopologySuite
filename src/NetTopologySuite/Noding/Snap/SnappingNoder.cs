using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding.Snap
{
    /// <summary>
    /// Nodes a set of segment strings,
    /// using a snap tolerance distance to snap vertices and node points together.
    /// This produces a much more robust noded output.
    /// <para/>
    /// The snap tolerance should be chosen to be as small as possible.
    /// It probably only needs to be a factor of 10e-12
    /// smaller than the magnitude of the segment coordinates. 
    /// </summary>
    /// <version>1.17</version>
    public class SnappingNoder : INoder
    {
        private readonly SnappingPointIndex snapIndex;
        private readonly double _snapTolerance;
        private IList<ISegmentString> _nodedResult;

        public SnappingNoder(double snapTolerance)
        {
            _snapTolerance = snapTolerance;
            snapIndex = new SnappingPointIndex(snapTolerance);
        }

        /// <inheritdoc cref="INoder.GetNodedSubstrings"/>>
        /// <returns>A collection of <see cref="NodedSegmentString"/>s representing the substrings</returns>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return _nodedResult;
        }

        /// <param name="inputSegmentStrings">A Collection of <see cref="ISegmentString"/>s</param>
        public void ComputeNodes(IList<ISegmentString> inputSegmentStrings)
        {
            var snappedSS = SnapVertices(inputSegmentStrings);
            _nodedResult = ComputeIntersections(snappedSS);
        }

        private IList<ISegmentString> SnapVertices(IEnumerable<ISegmentString> segStrings)
        {
            var nodedStrings = new List<ISegmentString>();
            foreach (var ss in segStrings)
                nodedStrings.Add(SnapVertices(ss));

            return nodedStrings;
        }

        private NodedSegmentString SnapVertices(ISegmentString ss)
        {
            var snapCoords = Snap(ss.Coordinates);
            return new NodedSegmentString(snapCoords, ss.Context);
        }

        private Coordinate[] Snap(Coordinate[] coords)
        {
            var snapCoords = new CoordinateList();
            for (int i = 0; i < coords.Length; i++)
            {
                var pt = snapIndex.Snap(coords[i]);
                snapCoords.Add(pt, false);
            }
            return snapCoords.ToCoordinateArray();
        }

        /// <summary>
        /// Computes all interior intersections in the collection of <see cref="ISegmentString"/>s,
        /// and returns their <see cref="NodedSegmentString"/>s.
        /// <para/>
        /// Also adds the intersection nodes to the segments.
        /// </summary>
        /// <returns>A list of noded substrings</returns>
        private IList<ISegmentString> ComputeIntersections(IList<ISegmentString> inputSS)
        {
            var intAdder = new SnappingIntersectionAdder(_snapTolerance, snapIndex);
            /*
             * Use an overlap tolerance to ensure all 
             * possible snapped intersections are found
             */
            var noder = new MCIndexNoder(intAdder, 2 * _snapTolerance);
            noder.ComputeNodes(inputSS);
            return noder.GetNodedSubstrings();
        }

    }
}
