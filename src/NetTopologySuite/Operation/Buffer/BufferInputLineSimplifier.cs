using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Simplifies a buffer input line to remove concavities with shallow depth.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The most important benefit of doing this
    /// is to reduce the number of points and the complexity of
    /// shape which will be buffered.
    /// It also reduces the risk of gores created by
    /// the quantized fillet arcs (although this issue
    /// should be eliminated in any case by the
    /// offset curve generation logic).
    /// </para>
    /// <para>
    /// A key aspect of the simplification is that it
    /// affects inside (concave or inward) corners only.
    /// Convex (outward) corners are preserved, since they
    /// are required to ensure that the generated buffer curve
    /// lies at the correct distance from the input geometry.
    /// </para>
    /// <para>
    /// Another important heuristic used is that the end segments
    /// of the input are never simplified.  This ensures that
    /// the client buffer code is able to generate end caps faithfully.
    /// </para>
    /// <para>
    /// No attempt is made to avoid self-intersections in the output.
    /// This is acceptable for use for generating a buffer offset curve,
    /// since the buffer algorithm is insensitive to invalid polygonal
    /// geometry.  However,
    /// this means that this algorithm
    /// cannot be used as a general-purpose polygon simplification technique.
    /// </para>
    /// </remarks>
    /// <author> Martin Davis</author>
    public class BufferInputLineSimplifier
    {
        /// <summary>
        /// Simplify the input coordinate list.
        /// If the distance tolerance is positive,
        /// concavities on the LEFT side of the line are simplified.
        /// If the supplied distance tolerance is negative,
        /// concavities on the RIGHT side of the line are simplified.
        /// </summary>
        /// <param name="inputLine">The coordinate list to simplify</param>
        /// <param name="distanceTol">simplification distance tolerance to use</param>
        /// <returns>The simplified coordinate list</returns>
        public static Coordinate[] Simplify(Coordinate[] inputLine, double distanceTol)
        {
            var simp = new BufferInputLineSimplifier(inputLine);
            return simp.Simplify(distanceTol);
        }

        private const int NumPtsToCheck = 10;

        // private const int Init = 0;
        private const int Delete = 1;
        // private const int Keep = 2;

        private readonly Coordinate[] _inputLine;
        private double _distanceTol;
        private byte[] _isDeleted;
        private OrientationIndex _angleOrientation = OrientationIndex.CounterClockwise;

        public BufferInputLineSimplifier(Coordinate[] inputLine)
        {
            _inputLine = inputLine;
        }

        /// <summary>
        /// Simplify the input coordinate list.
        /// </summary>
        /// <remarks>
        /// If the distance tolerance is positive, concavities on the LEFT side of the line are simplified.
        /// If the supplied distance tolerance is negative, concavities on the RIGHT side of the line are simplified.
        /// </remarks>
        /// <param name="distanceTol">Simplification distance tolerance to use</param>
        /// <returns>
        /// The simplified coordinates list
        /// </returns>
        public Coordinate[] Simplify(double distanceTol)
        {
            _distanceTol = System.Math.Abs(distanceTol);
            if (distanceTol < 0)
                _angleOrientation = OrientationIndex.Clockwise;

            // rely on fact that boolean array is filled with false value
            _isDeleted = new byte[_inputLine.Length];

            bool isChanged;
            do
            {
                isChanged = DeleteShallowConcavities();
            } while (isChanged);

            return CollapseLine();
        }

        /// <summary>
        /// Uses a sliding window containing 3 vertices to detect shallow angles
        /// in which the middle vertex can be deleted, since it does not
        /// affect the shape of the resulting buffer in a significant way.
        /// </summary>
        /// <returns></returns>
        private bool DeleteShallowConcavities()
        {
            /*
             * Do not simplify end line segments of the line string.
             * This ensures that end caps are generated consistently.
             */
            int index = 1;

            int midIndex = FindNextNonDeletedIndex(index);
            int lastIndex = FindNextNonDeletedIndex(midIndex);

            bool isChanged = false;
            while (lastIndex < _inputLine.Length)
            {
                // test triple for shallow concavity
                bool isMiddleVertexDeleted = false;
                if (IsDeletable(index, midIndex, lastIndex, _distanceTol))
                {
                    _isDeleted[midIndex] = Delete;
                    isMiddleVertexDeleted = true;
                    isChanged = true;
                }

                // move simplification window forward
                if (isMiddleVertexDeleted)
                    index = lastIndex;
                else
                    index = midIndex;

                midIndex = FindNextNonDeletedIndex(index);
                lastIndex = FindNextNonDeletedIndex(midIndex);
            }
            return isChanged;
        }

        /// <summary>
        /// Finds the next non-deleted index, or the end of the point array if none
        /// </summary>
        /// <param name="index">The start index to search from</param>
        /// <returns>The next non-deleted index, if any <br/>
        /// or <c>_inputLine.Length</c> if there are no more non-deleted indices
        /// </returns>
        private int FindNextNonDeletedIndex(int index)
        {
            int next = index + 1;
            while (next < _inputLine.Length - 1 && _isDeleted[next] == Delete)
                next++;
            return next;
        }

        private Coordinate[] CollapseLine()
        {
            var coordList = new CoordinateList(_inputLine.Length);
            for (int i = 0; i < _inputLine.Length; i++)
            {
                if (_isDeleted[i] != Delete)
                    coordList.Add(_inputLine[i]);
            }
            // if (coordList.size() < inputLine.length)      System.out.println("Simplified " + (inputLine.length - coordList.size()) + " pts");
            return coordList.ToCoordinateArray();
        }

        private bool IsDeletable(int i0, int i1, int i2, double distanceTol)
        {
            var p0 = _inputLine[i0];
            var p1 = _inputLine[i1];
            var p2 = _inputLine[i2];

            if (!IsConcave(p0, p1, p2)) return false;
            if (!IsShallow(p0, p1, p2, distanceTol)) return false;

            // MD - don't use this heuristic - it's too restricting
            // if (p0.distance(p2) > distanceTol) return false;

            return IsShallowSampled(p0, p1, i0, i2, distanceTol);
        }

        /*
        private bool IsShallowConcavity(Coordinate p0, Coordinate p1, Coordinate p2, double distanceTol)
        {
            int orientation = CGAlgorithms.ComputeOrientation(p0, p1, p2);
            bool isAngleToSimplify = (orientation == _angleOrientation);
            if (!isAngleToSimplify)
                return false;

            double dist = CGAlgorithms.DistancePointLine(p1, p0, p2);
            return dist < distanceTol;
        }
         */

        /// <summary>
        /// Checks for shallowness over a sample of points in the given section.
        /// This helps to prevent the simplification from incrementally
        /// "skipping" over points which are in fact non-shallow.
        /// </summary>
        /// <param name="p0">A coordinate of section</param>
        /// <param name="p2">A coordinate of section</param>
        /// <param name="i0">The start index of section</param>
        /// <param name="i2">The end index of section</param>
        /// <param name="distanceTol">The tolerated distance</param>
        private bool IsShallowSampled(Coordinate p0, Coordinate p2, int i0, int i2, double distanceTol)
        {
            // check every n'th point to see if it is within tolerance
            int inc = (i2 - i0) / NumPtsToCheck;
            if (inc <= 0) inc = 1;

            for (int i = i0; i < i2; i += inc)
            {
                if (!IsShallow(p0, p2, _inputLine[i], distanceTol)) return false;
            }
            return true;
        }

        private static bool IsShallow(Coordinate p0, Coordinate p1, Coordinate p2, double distanceTol)
        {
            double dist = DistanceComputer.PointToSegment(p1, p0, p2);
            return dist < distanceTol;
        }

        private bool IsConcave(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            var orientation = Orientation.Index(p0, p1, p2);
            bool isConcave = (orientation == _angleOrientation);
            return isConcave;
        }
    }
}
