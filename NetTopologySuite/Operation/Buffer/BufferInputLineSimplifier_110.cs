using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Buffer
{
    ///<summary>
    /// Simplifies a buffer input line to 
    /// remove concavities with shallow depth.
    /// 
    /// The most important benefit of doing this
    /// is to reduce the number of points and the complexity of
    /// shape which will be buffered.
    /// It also reduces the risk of gores created by
    /// the quantized fillet arcs (although this issue
    /// should be eliminated in any case by the 
    /// offset curve generation logic).
    /// 
    /// A key aspect of the simplification is that it
    /// affects inside (concave or inward) corners only.  
    /// Convex (outward) corners are preserved, since they
    /// are required to ensure that the eventual buffer curve
    /// lies at the correct distance from the input geometry.
    /// 
    /// Another important heuristic used is that the end segments
    /// of the input are never simplified.  This ensures that
    /// the client buffer code is able to generate end caps consistently.
    ///</summary>
    public class BufferInputLineSimplifier_110<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
        public static IEnumerable<TCoordinate> Simplify(IEnumerable<TCoordinate> inputLine, Double distanceTol)
        {
            BufferInputLineSimplifier_110<TCoordinate> simp = new BufferInputLineSimplifier_110<TCoordinate>(inputLine);
            return simp.Simplify(distanceTol);
        }

        private TCoordinate[] _inputLine;
        private Double _distanceTol;
        private Boolean[] _isDeleted;
        private Int32 _numRemaining;
        private Orientation _angleOrientation = Orientation.CounterClockwise;

        public BufferInputLineSimplifier_110(IEnumerable<TCoordinate> inputLine)
        {
            List<TCoordinate> tmp = new List<TCoordinate>();
            tmp.AddRange(inputLine);
            _inputLine = tmp.ToArray();
        }

        ///<summary>
        /// Simplify the input geometry.
        /// If the distance tolerance is positive, 
        /// concavities on the LEFT side of the line are simplified.
        /// If the supplied distance tolerance is negative,
        /// concavities on the RIGHT side of the line are simplified.
        ///</summary>
        ///<param name="distanceTol">simplification distance tolerance to use</param>
        ///<returns></returns>
        public IEnumerable<TCoordinate> Simplify(Double distanceTol)
        {
            _distanceTol = Math.Abs(distanceTol);
            if (distanceTol < 0)
                _angleOrientation = Orientation.Clockwise;

            // rely on fact that boolean array is filled with false value
            _isDeleted = new Boolean[_inputLine.Length];
            _numRemaining = _inputLine.Length;

            Boolean isChanged = false;
            do
            {
                isChanged = DeleteShallowConcavities();
            } while (isChanged);

            return CollapseLine();
        }

        private Boolean DeleteShallowConcavities()
        {
            /**
             * Do not simplify end line segments of the line string.
             * This ensures that end caps are generated consistently.
             */
            Int32 index = 1;
            Int32 maxIndex = _inputLine.Length - 1;

            Int32 midIndex = FindNextValidIndex(index);
            Int32 lastIndex = FindNextValidIndex(midIndex);

            Boolean isChanged = false;
            while (lastIndex < maxIndex)
            {
                // test triple for shallow concavity
                if (IsShallowConcavity(_inputLine[index], _inputLine[midIndex], _inputLine[lastIndex],
                    _distanceTol))
                {
                    _isDeleted[midIndex] = true;
                    _numRemaining--;
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

        private IEnumerable<TCoordinate> CollapseLine()
        {
            List<TCoordinate> coords = new List<TCoordinate>();
            for (int i = 0; i < _inputLine.Length; i++)
            {
                if (!_isDeleted[i])
                    coords.Add(_inputLine[i]);
                    //yield return _inputLine[i];
            }
            return coords;
        }

        private Boolean IsShallowConcavity(TCoordinate p0, TCoordinate p1, TCoordinate p2, Double distanceTol)
        {
            Orientation orientation = CGAlgorithms<TCoordinate>.ComputeOrientation(p0, p1, p2);
            Boolean isAngleToSimplify = (orientation == _angleOrientation);
            if (!isAngleToSimplify)
                return false;

            double dist = CGAlgorithms<TCoordinate>.DistancePointLine(p1, p0, p2);
            return dist < distanceTol;
        }
    }
}
