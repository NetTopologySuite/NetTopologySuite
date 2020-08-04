using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Noding.Snap
{
    /// <summary>
    /// Uses regular noding but with snapping vertices
    /// to nearby segments.
    /// <para/>
    /// EXPERIMENTAL
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

        /// <param name="inputSegmentStrings">A Collection of <see cref="NodedSegmentString"/>s</param>
        public void ComputeNodes(IList<ISegmentString> inputSegmentStrings)
        {
            var snappedSS = SnapVertices(inputSegmentStrings);
            /*
             * Determine hot pixels for intersections and vertices.
             * This is done BEFORE the input lines are rounded,
             * to avoid distorting the line arrangement 
             * (rounding can cause vertices to move across edges).
             */
            _nodedResult = ComputeIntersections(snappedSS);

            // testing purposes only - remove in final version
            //checkCorrectness(inputSegmentStrings);
            //if (Debug.isDebugging()) dumpNodedLines(inputSegmentStrings);
            //if (Debug.isDebugging()) dumpNodedLines(snappedResult);
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

        private void dumpNodedLines(IEnumerable<ISegmentString> segStrings)
        {
            foreach (NodedSegmentString nss in segStrings)
            {
                Debug.WriteLine(WKTWriter.ToLineString(nss.NodeList.GetSplitCoordinates()));
            }
        }

        private void checkValidNoding(ICollection<ISegmentString> inputSegmentStrings)
        {
            var resultSegStrings = NodedSegmentString.GetNodedSubstrings(inputSegmentStrings);
            var nv = new NodingValidator(resultSegStrings);
            try
            {
                nv.CheckValid();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Computes all interior intersections in the collection of {@link SegmentString}s,
        /// and returns their {@link Coordinate}s.
        /// <para/>
        /// Also adds the intersection nodes to the segments.
        /// </summary>
        /// <returns>A list of noded substrings</returns>
        private IList<ISegmentString> ComputeIntersections(IList<ISegmentString> inputSS)
        {
            var intAdder = new SnappingIntersectionAdder(snapIndex);
            var noder = new MCIndexNoder(intAdder, 2 * _snapTolerance);
            noder.ComputeNodes(inputSS);
            return noder.GetNodedSubstrings();
        }

    }
}
