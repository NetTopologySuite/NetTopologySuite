using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Buffer
{
    ///<summary>
    /// Simplifies a buffer input line to remove concavities with shallow depth.
    ///</summary>
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
    /// are required to ensure that the eventual buffer curve
    /// lies at the correct distance from the input geometry.
    /// </para>
    /// <para>
    /// Another important heuristic used is that the end segments
    /// of the input are never simplified.  This ensures that
    /// the client buffer code is able to generate end caps consistently.
    /// </para>
    /// </remarks>
    /// <author> Martin Davis</author>
    public class BufferInputLineSimplifier
    {
        public static ICoordinate[] Simplify(ICoordinate[] inputLine, double distanceTol)
        {
            var simp = new BufferInputLineSimplifier(inputLine);
            return simp.Simplify(distanceTol);
        }

        private readonly ICoordinate[] _inputLine;
        private double _distanceTol;
        private bool[] _isDeleted;
        private int _angleOrientation = CGAlgorithms.CounterClockwise;

        public BufferInputLineSimplifier(ICoordinate[] inputLine)
        {
            _inputLine = inputLine;
        }

        ///<summary>
        /// Simplify the input geometry.
        ///</summary>
        /// <remarks>
        /// If the distance tolerance is positive, concavities on the LEFT side of the line are simplified.
        /// If the supplied distance tolerance is negative, concavities on the RIGHT side of the line are simplified.
        /// </remarks>
        /// <param name="distanceTol">Simplification distance tolerance to use</param>
        /// <returns>
        /// Simplified coordinates
        /// </returns>
        public ICoordinate[] Simplify(double distanceTol)
        {
            _distanceTol = Math.Abs(distanceTol);
            if (distanceTol < 0)
                _angleOrientation = CGAlgorithms.Clockwise;

            // rely on fact that boolean array is filled with false value
            _isDeleted = new bool[_inputLine.Length];

            bool isChanged;
            do
            {
                isChanged = DeleteShallowConcavities();
            } while (isChanged);

            return CollapseLine();
        }

        private bool DeleteShallowConcavities()
        {
            /**
             * Do not simplify end line segments of the line string.
             * This ensures that end caps are generated consistently.
             */
            int index = 1;
            int maxIndex = _inputLine.Length - 1;

            int midIndex = FindNextValidIndex(index);
            int lastIndex = FindNextValidIndex(midIndex);

            bool isChanged = false;
            while (lastIndex < maxIndex)
            {
                // test triple for shallow concavity
                if (IsShallowConcavity(_inputLine[index], _inputLine[midIndex], _inputLine[lastIndex],
                    _distanceTol))
                {
                    _isDeleted[midIndex] = true;
                    isChanged = true;
                }
                // this needs to be replaced by scanning for next valid pts
                index = lastIndex;
                midIndex = FindNextValidIndex(index);
                lastIndex = FindNextValidIndex(midIndex);
            }
            return isChanged;
        }

        private int FindNextValidIndex(int index)
        {
            int next = index + 1;
            while (next < _inputLine.Length - 1
                && _isDeleted[next])
                next++;
            return next;
        }

        private ICoordinate[] CollapseLine()
        {
            var coordList = new CoordinateList();
            for (int i = 0; i < _inputLine.Length; i++)
            {
                if (!_isDeleted[i])
                    coordList.Add(_inputLine[i]);
            }
            //    if (coordList.size() < inputLine.length)      System.out.println("Simplified " + (inputLine.length - coordList.size()) + " pts");
            return coordList.ToCoordinateArray();
        }

        private bool IsShallowConcavity(ICoordinate p0, ICoordinate p1, ICoordinate p2, double distanceTol)
        {
            int orientation = CGAlgorithms.ComputeOrientation(p0, p1, p2);
            bool isAngleToSimplify = (orientation == _angleOrientation);
            if (!isAngleToSimplify)
                return false;

            double dist = CGAlgorithms.DistancePointLine(p1, p0, p2);
            return dist < distanceTol;
        }
    }
}