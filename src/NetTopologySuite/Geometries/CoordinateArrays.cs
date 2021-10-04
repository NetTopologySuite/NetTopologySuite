using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Useful utility functions for handling Coordinate arrays.
    /// </summary>
    public static class CoordinateArrays
    {
        /// <summary>
        /// Determine dimension based on subclass of <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="pts">pts supplied coordinates</param>
        /// <returns>number of ordinates recorded</returns>
        public static int Dimension(Coordinate[] pts)
        {
            if (pts == null || pts.Length == 0)
            {
                return 2; // unknown, assume default
            }
            int dimension = 0;
            foreach (var coordinate in pts)
            {
                dimension = Math.Max(dimension, Coordinates.Dimension(coordinate));
            }
            return dimension;
        }

        /// <summary>
        /// Determine number of measures based on subclass of <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="pts">supplied coordinates</param>
        /// <returns>number of measures recorded</returns>
        public static int Measures(Coordinate[] pts)
        {
            if (pts == null || pts.Length == 0)
            {
                return 0; // unknown, assume default
            }
            int measures = 0;
            foreach (var coordinate in pts)
            {
                measures = Math.Max(measures, Coordinates.Measures(coordinate));
            }
            return measures;
        }


        /// <summary>
        /// Utility method ensuring array contents are of consistent dimension and measures.
        /// <para/>
        /// Array is modified in place if required, coordinates are replaced in the array as required
        /// to ensure all coordinates have the same dimension and measures. The final dimension and
        /// measures used are the maximum found when checking the array.
        /// </summary>
        /// <param name="array">Modified in place to coordinates of consistent dimension and measures.</param>
        public static void EnforceConsistency(Coordinate[] array)
        {
            if (array == null)
            {
                return;
            }
            // step one check
            int maxSpatialDimension = -1;
            int maxMeasures = -1;
            bool isConsistent = true;
            for (int i = 0; i < array.Length; i++)
            {
                var coordinate = array[i];
                if (coordinate != null)
                {
                    int m = Coordinates.Measures(coordinate);
                    int sd = Coordinates.Dimension(coordinate) - m;
                    if (maxSpatialDimension == -1)
                    {
                        maxMeasures = m;
                        maxSpatialDimension = sd;
                        continue;
                    }
                    if (sd != maxSpatialDimension || m != maxMeasures)
                    {
                        isConsistent = false;
                        maxSpatialDimension = Math.Max(maxSpatialDimension, sd);
                        maxMeasures = Math.Max(maxMeasures, m);
                    }
                }
            }
            if (!isConsistent)
            {
                // step two fix
                int maxDimension = maxSpatialDimension + maxMeasures;
                var sample = Coordinates.Create(maxDimension, maxMeasures);
                var type = sample.GetType();
                for (int i = 0; i < array.Length; i++)
                {
                    var coordinate = array[i];
                    if (coordinate != null && coordinate.GetType() != type)
                    {
                        var duplicate = Coordinates.Create(maxDimension, maxMeasures);
                        duplicate.CoordinateValue = coordinate;
                        array[i] = duplicate;
                    }
                }
            }
        }

        /// <summary>
        /// Utility method ensuring array contents are of the specified dimension and measures.
        /// <para/>
        /// Array is returned unmodified if consistent, or a copy of the array is made with
        /// each inconsistent coordinate duplicated into an instance of the correct dimension and measures.
        /// </summary>
        /// <param name="array">A coordinate array</param>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        /// <returns>Input array or copy created if required to enforce consistency.</returns>
        public static Coordinate[] EnforceConsistency(Coordinate[] array, int dimension, int measures)
        {
            var sample = Coordinates.Create(dimension, measures);
            var type = sample.GetType();
            bool isConsistent = true;
            for (int i = 0; i < array.Length; i++)
            {
                var coordinate = array[i];
                if (coordinate != null && coordinate.GetType() != type)
                {
                    isConsistent = false;
                    break;
                }
            }
            if (isConsistent)
            {
                return array;
            }
            else
            {
                var coordinateType = sample.GetType();
                var copy = (Coordinate[])Array.CreateInstance(coordinateType, array.Length);
                for (int i = 0; i < copy.Length; i++)
                {
                    var coordinate = array[i];
                    if (coordinate != null && coordinate.GetType() != type)
                    {
                        var duplicate = Coordinates.Create(dimension, measures);
                        duplicate.CoordinateValue = coordinate;
                        copy.SetValue(duplicate, i);
                    }
                    else
                    {
                        copy.SetValue(coordinate, i);
                    }
                }
                return copy;
            }
        }

        /// <summary>
        /// Tests whether an array of <see cref="Coordinate"/>s forms a ring, by checking length and closure.
        /// Self-intersection is not checked.
        /// </summary>
        /// <param name="pts">An array of Coordinates</param>
        /// <returns>true if the coordinate form a ring.</returns>
        public static bool IsRing(Coordinate[] pts)
        {
            if (pts.Length < 4)
                return false;
            if (!pts[0].Equals2D(pts[pts.Length - 1]))
                return false;
            return true;
        }

        /// <summary>
        /// Finds a <see cref="Coordinate "/> in a list of <see cref="Coordinate "/>s
        /// which is not contained in another list of <see cref="Coordinate "/>s.
        /// </summary>
        /// <param name="testPts">The <see cref="Coordinate" />s to test.</param>
        /// <param name="pts">An array of <see cref="Coordinate" />s to test the input points against.</param>
        /// <returns>
        /// A <see cref="Coordinate" /> from <paramref name="testPts" />
        /// which is not in <paramref name="pts" />, or <c>null</c>.
        /// </returns>
        public static Coordinate PointNotInList(Coordinate[] testPts, Coordinate[] pts)
        {
            for (int i = 0; i < testPts.Length; i++)
            {
                var testPt = testPts[i];
                if (IndexOf(testPt, pts) < 0)
                    return testPt;
            }
            return null;
        }

        /// <summary>
        /// Compares two <see cref="Coordinate" /> arrays
        /// in the forward direction of their coordinates,
        /// using lexicographic ordering.
        /// </summary>
        /// <param name="pts1"></param>
        /// <param name="pts2"></param>
        /// <returns></returns>
        public static int Compare(Coordinate[] pts1, Coordinate[] pts2)
        {
            int i = 0;
            while (i < pts1.Length && i < pts2.Length)
            {
                int compare = pts1[i].CompareTo(pts2[i]);
                if (compare != 0)
                    return compare;
                i++;
            }

            // handle situation when arrays are of different length
            if (i < pts2.Length)
                return -1;
            if (i < pts1.Length)
                return 1;

            return 0;
        }

        /// <summary>
        /// Determines which orientation of the <see cref="Coordinate" /> array is (overall) increasing.
        /// In other words, determines which end of the array is "smaller"
        /// (using the standard ordering on <see cref="Coordinate" />).
        /// Returns an integer indicating the increasing direction.
        /// If the sequence is a palindrome, it is defined to be
        /// oriented in a positive direction.
        /// </summary>
        /// <param name="pts">The array of Coordinates to test.</param>
        /// <returns>
        /// <c>1</c> if the array is smaller at the start or is a palindrome,
        /// <c>-1</c> if smaller at the end.
        /// </returns>
        public static int IncreasingDirection(Coordinate[] pts)
        {
            for (int i = 0; i < pts.Length / 2; i++)
            {
                int j = pts.Length - 1 - i;
                // skip equal points on both ends
                int comp = pts[i].CompareTo(pts[j]);
                if (comp != 0)
                    return comp;
            }
            // array must be a palindrome - defined to be in positive direction
            return 1;
        }

        /// <summary>
        /// Determines whether two <see cref="Coordinate" /> arrays of equal length
        /// are equal in opposite directions.
        /// </summary>
        /// <param name="pts1"></param>
        /// <param name="pts2"></param>
        /// <returns></returns>
        private static bool IsEqualReversed(Coordinate[] pts1, Coordinate[] pts2)
        {
            for (int i = 0; i < pts1.Length; i++)
            {
                var p1 = pts1[i];
                var p2 = pts2[pts1.Length - i - 1];
                if (p1.CompareTo(p2) != 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Creates a deep copy of the argument <c>Coordinate</c> array.
        /// </summary>
        /// <param name="coordinates">Array of Coordinates.</param>
        /// <returns>Deep copy of the input.</returns>
        public static Coordinate[] CopyDeep(Coordinate[] coordinates)
        {
            var copy = new Coordinate[coordinates.Length];
            for (int i = 0; i < coordinates.Length; i++)
            {
                var c = coordinates[i].Copy();
                copy[i] = c;
            }
            return copy;
        }

        /// <summary>
        /// Creates a deep copy of a given section of a source <see cref="Coordinate"/> array into a destination Coordinate array.
        /// The destination array must be an appropriate size to receive the copied coordinates.
        /// </summary>
        /// <param name="src">An array of Coordinates</param>
        /// <param name="srcStart">The index to start copying from</param>
        /// <param name="dest">The array to receive the deep-copied coordinates</param>
        /// <param name="destStart">The destination index to start copying to</param>
        /// <param name="length">The number of items to copy</param>
        public static void CopyDeep(Coordinate[] src, int srcStart, Coordinate[] dest, int destStart, int length)
        {
            for (int i = 0; i < length; i++)
            {
                var c = src[srcStart + i].Copy();
                dest[destStart + i] = c;
            }
        }

        /// <summary>
        /// Converts the given <see cref="IEnumerable{T}" /> of
        /// <see cref="Coordinate" />s into a <see cref="Coordinate" /> array.
        /// </summary>
        /// <param name="coordList"><see cref="IEnumerable{T}"/> of coordinates.</param>
        /// <returns></returns>
        public static Coordinate[] ToCoordinateArray(IEnumerable<Coordinate> coordList)
        {
            return coordList.ToArray();
        }

        /// <summary>
        /// Returns whether <see cref="Coordinate.Equals(object)"/> returns true
        /// for any two consecutive coordinates in the given array.
        /// </summary>
        /// <param name="coord">An array of <c>Coordinate</c>s.</param>
        /// <returns>true if coord has repeated points; false otherwise.</returns>
        public static bool HasRepeatedPoints(Coordinate[] coord)
        {
            for (int i = 1; i < coord.Length; i++)
            {
                var prev = coord[i - 1];
                var curr = coord[i];
                if (prev.Equals(curr))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns either the given coordinate array if its length is greater than
        /// the given amount, or an empty coordinate array.
        /// </summary>
        /// <param name="n">Length amount.</param>
        /// <param name="c">Array of Coordinates.</param>
        /// <returns>New Coordinate array.</returns>
        public static Coordinate[] AtLeastNCoordinatesOrNothing(int n, Coordinate[] c)
        {
            return c.Length >= n ? c : new Coordinate[] { };
        }

        /// <summary>
        /// If the coordinate array argument has repeated points,
        /// constructs a new array containing no repeated points.
        /// Otherwise, returns the argument.
        /// </summary>
        /// <param name="coord">An array of <c>Coordinate</c>s</param>
        /// <returns>The array with repeated coordinates removed</returns>
        public static Coordinate[] RemoveRepeatedPoints(Coordinate[] coord)
        {
            if (!HasRepeatedPoints(coord))
                return coord;
            var coordList = new CoordinateList(coord, false);
            return coordList.ToCoordinateArray();
        }

        /// <summary>
        /// Tests whether an array has any repeated or invalid coordinates.
        /// </summary>
        /// <param name="coord">An array of coordinates</param>
        /// <returns><c>true</c> if the array contains repeated or invalid coordinates</returns>
        /// <see cref="Coordinate.IsValid"/>
        public static bool HasRepeatedOrInvalidPoints(Coordinate[] coord)
        {
            if (!coord[0].IsValid)
                return true;

            for (int i = 1; i < coord.Length; i++)
            {
                if (!coord[i].IsValid)
                    return true;
                if (coord[i - 1].Equals(coord[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If the coordinate array argument has repeated or invalid points,
        /// constructs a new array containing no repeated points.
        /// Otherwise, returns the argument.
        /// </summary>
        /// <param name="coord">An array of coordinates</param>
        /// <returns>The array with repeated or invalid coordinates removed.</returns>
        /// <see cref="HasRepeatedOrInvalidPoints"/>
        /// <see cref="Coordinate.IsValid"/>
        public static Coordinate[] RemoveRepeatedOrInvalidPoints(Coordinate[] coord)
        {
            if (!HasRepeatedOrInvalidPoints(coord)) return coord;
            var coordList = new CoordinateList(coord.Length);
            for (int i = 0; i < coord.Length; i++) {
                if (!coord[i].IsValid) continue;
                coordList.Add(coord[i], false);
            }
            return coordList.ToCoordinateArray();
        }

        /// <summary>
        /// Collapses a coordinate array to remove all null elements.
        /// </summary>
        /// <param name="coord">The coordinate array to collapse</param>
        /// <returns>An Array containing only non-null elements</returns>
        public static Coordinate[] RemoveNull(Coordinate[] coord)
        {
            var coordinateList = new List<Coordinate>(coord.Length);
            foreach (var coordinate in coord)
            {
                if (coordinate != null)
                    coordinateList.Add(coordinate);
            }
            return coordinateList.ToArray();
        }

        /// <summary>
        /// Reverses the coordinates in an array in-place.
        /// </summary>
        /// <param name="coord">Array of Coordinates.</param>
        public static void Reverse(Coordinate[] coord)
        {
            Array.Reverse(coord);
        }

        /// <summary>
        /// Returns <c>true</c> if the two arrays are identical, both <c>null</c>, or pointwise
        /// equal (as compared using Coordinate.Equals).
        /// </summary>
        /// <param name="coord1">First array of Coordinates.</param>
        /// <param name="coord2">Second array of Coordinates.</param>
        /// <returns><c>true</c> if two Coordinates array are equals; false otherwise</returns>
        public static bool Equals(Coordinate[] coord1, Coordinate[] coord2)
        {
            if (coord1 == coord2)
                return true;
            if (coord1 == null || coord2 == null)
                return false;
            if (coord1.Length != coord2.Length)
                return false;
            for (int i = 0; i < coord1.Length; i++)
                if (!coord1[i].Equals(coord2[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// Compares two <see cref="Coordinate" /> arrays
        /// in the forward direction of their coordinates,
        /// using lexicographic ordering.
        /// </summary>
        public class ForwardComparator : IComparer<Coordinate[]>
        {
            /// <summary>
            /// Compares the specified <see cref="Coordinate" />s arrays.
            /// </summary>
            /// <param name="pts1">An array of coordinates</param>
            /// <param name="pts2">An array of coordinates</param>
            public int Compare(Coordinate[] pts1, Coordinate[] pts2)
            {
                return CoordinateArrays.Compare(pts1, pts2);
            }
        }

        /// <summary>
        /// A comparator for <see cref="Coordinate" /> arrays modulo their directionality.
        /// E.g. if two coordinate arrays are identical but reversed
        /// they will compare as equal under this ordering.
        /// If the arrays are not equal, the ordering returned
        /// is the ordering in the forward direction.
        /// </summary>
        public class BidirectionalComparator : IComparer<Coordinate[]>
        {
            /// <summary>
            /// Compares the specified <see cref="Coordinate" />s arrays.
            /// </summary>
            /// <param name="pts1">An array of coordinates</param>
            /// <param name="pts2">An array of coordinates</param>
            public int Compare(Coordinate[] pts1, Coordinate[] pts2)
            {
                if (pts1.Length < pts2.Length)
                    return -1;
                if (pts1.Length > pts2.Length)
                    return 1;

                if (pts1.Length == 0)
                    return 0;

                int forwardComp = CoordinateArrays.Compare(pts1, pts2);
                bool isEqualRev = IsEqualReversed(pts1, pts2);
                if (isEqualRev)
                    return 0;

                return forwardComp;
            }

            /// <summary/>
            [Obsolete("Old")]
            public int OldCompare(Coordinate[] pts1, Coordinate[] pts2)
            {
                if (pts1.Length < pts2.Length)
                    return -1;
                if (pts1.Length > pts2.Length)
                    return 1;

                if (pts1.Length == 0)
                    return 0;

                int dir1 = IncreasingDirection(pts1);
                int dir2 = IncreasingDirection(pts2);

                int i1 = dir1 > 0 ? 0 : pts1.Length - 1;
                int i2 = dir2 > 0 ? 0 : pts1.Length - 1;

                for (int i = 0; i < pts1.Length; i++)
                {
                    int comparePt = pts1[i1].CompareTo(pts2[i2]);
                    if (comparePt != 0)
                        return comparePt;
                    i1 += dir1;
                    i2 += dir2;
                }
                return 0;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the two arrays are identical, both <c>null</c>, or pointwise
        /// equal, using a user-defined <see cref="IComparer" />
        /// for <see cref="Coordinate" />s.
        /// </summary>
        /// <param name="coord1">An array of <see cref="Coordinate" />s.</param>
        /// <param name="coord2">Another array of <see cref="Coordinate" />s.</param>
        /// <param name="coordinateComparer">
        /// A <see cref="IComparer" /> for <see cref="Coordinate" />s.
        /// </param>
        /// <returns></returns>
        public static bool Equals(Coordinate[] coord1, Coordinate[] coord2,
                IComparer<Coordinate[]> coordinateComparer)
        {
            if (coord1 == coord2)
                return true;
            if (coord1 == null || coord2 == null)
                return false;
            if (coord1.Length != coord2.Length)
                return false;
            if (coordinateComparer.Compare(coord1, coord2) != 0)
                return false;
            return true;
        }

        /// <summary>
        /// Returns the minimum coordinate, using the usual lexicographic comparison.
        /// </summary>
        /// <param name="coordinates">Array to search.</param>
        /// <returns>The minimum coordinate in the array, found using <c>CompareTo</c>.</returns>
        public static Coordinate MinCoordinate(Coordinate[] coordinates)
        {
            Coordinate minCoord = null;
            for (int i = 0; i < coordinates.Length; i++)
                if (minCoord == null || minCoord.CompareTo(coordinates[i]) > 0)
                    minCoord = coordinates[i];
            return minCoord;
        }

        /// <summary>
        /// Shifts the positions of the coordinates until <c>firstCoordinate</c> is first.
        /// </summary>
        /// <param name="coordinates">Array to rearrange.</param>
        /// <param name="firstCoordinate">Coordinate to make first.</param>
        public static void Scroll(Coordinate[] coordinates, Coordinate firstCoordinate)
        {
            int i = IndexOf(firstCoordinate, coordinates);
            Scroll(coordinates, i);
        }

        /// <summary>
        /// Shifts the positions of the coordinates until the coordinate
        /// at <c>indexOfFirstCoordinate</c> is first.
        /// </summary>
        /// <param name="coordinates">The array of coordinates to arrange</param>
        /// <param name="indexOfFirstCoordinate">The index of the coordinate to make first</param>
        public static void Scroll(Coordinate[] coordinates, int indexOfFirstCoordinate)
        {
            Scroll(coordinates, indexOfFirstCoordinate, IsRing(coordinates));
        }

        /// <summary>
        /// Shifts the positions of the coordinates until the coordinate
        /// at <c>indexOfFirstCoordinate</c> is first.
        /// </summary>
        /// <remarks>
        /// If <paramref name="ensureRing"/> is <c>true</c>, first and last
        /// coordinate of the returned array are equal.
        /// </remarks>
        /// <param name="coordinates">The array of coordinates to arrange</param>
        /// <param name="indexOfFirstCoordinate">The index of the coordinate to make first</param>
        /// <param name="ensureRing">A flag indicating if returned array should form a ring.</param>
        public static void Scroll(Coordinate[] coordinates, int indexOfFirstCoordinate, bool ensureRing)
        {
            int i = indexOfFirstCoordinate;
            if (i <= 0) return;

            var newCoordinates = new Coordinate[coordinates.Length];
            if (!ensureRing)
            {
                Array.Copy(coordinates, i, newCoordinates, 0, coordinates.Length - i);
                Array.Copy(coordinates, 0, newCoordinates, coordinates.Length - i, i);
            }
            else
            {
                int last = coordinates.Length - 1;

                // fill in values
                int j;
                for (j = 0; j < last; j++)
                    newCoordinates[j] = coordinates[(i + j) % last];

                // Fix the ring (first == last)
                newCoordinates[j] = newCoordinates[0].Copy();
            }
            Array.Copy(newCoordinates, 0, coordinates, 0, coordinates.Length);
        }

        /// <summary>
        /// Returns the index of <paramref name="coordinate" /> in <paramref name="coordinates" />.
        /// The first position is 0; the second is 1; etc.
        /// </summary>
        /// <param name="coordinate">A <see cref="Coordinate" /> to search for.</param>
        /// <param name="coordinates">A <see cref="Coordinate" /> array to search.</param>
        /// <returns>The position of <c>coordinate</c>, or -1 if it is not found.</returns>
        public static int IndexOf(Coordinate coordinate, Coordinate[] coordinates)
        {
            for (int i = 0; i < coordinates.Length; i++)
            {
                var c = coordinates[i];
                if (coordinate.Equals(c))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Extracts a subsequence of the input <see cref="Coordinate" /> array
        /// from indices <paramref name="start" /> to <paramref name="end"/> (inclusive).
        /// The input indices are clamped to the array size;
        /// If the end index is less than the start index,
        /// the extracted array will be empty.
        /// </summary>
        /// <param name="pts">The input array.</param>
        /// <param name="start">The index of the start of the subsequence to extract.</param>
        /// <param name="end">The index of the end of the subsequence to extract.</param>
        /// <returns>A subsequence of the input array.</returns>
        public static Coordinate[] Extract(Coordinate[] pts, int start, int end)
        {
            start = MathUtil.Clamp(start, 0, pts.Length);
            end = MathUtil.Clamp(end, -1, pts.Length);

            int npts = end - start + 1;
            if (end < 0)
                npts = 0;
            if (start >= pts.Length)
                npts = 0;
            if (end < start)
                npts = 0;

            var extractPts = new Coordinate[npts];
            if (npts == 0)
                return extractPts;

            Array.Copy(pts, start, extractPts, 0, npts);
            return extractPts;
        }

        /// <summary>
        /// Computes the <see cref="Envelope"/> of the coordinates.
        /// </summary>
        /// <param name="coordinates">the <see cref="Coordinate"/> array to scan.</param>
        /// <returns>the <see cref="Envelope"/> of the <paramref name="coordinates"/>.</returns>
        public static Envelope Envelope(Coordinate[] coordinates)
        {
            var env = new Envelope();
            for (int i = 0; i < coordinates.Length; i++)
            {
                var c = coordinates[i];
                env.ExpandToInclude(c);
            }
            return env;
        }

        /// <summary>
        /// Extracts the coordinates which intersect an <see cref="Envelope"/>.
        /// </summary>
        /// <param name="coordinates">The coordinates to scan</param>
        /// <param name="env">The envelope to intersect with</param>
        /// <returns>An array of coordinates which intersect with the envelope</returns>
        public static Coordinate[] Intersection(Coordinate[] coordinates, Envelope env)
        {
            var coordList = new CoordinateList(coordinates.Length);
            for (int i = 0; i < coordinates.Length; i++)
            {
                var c = coordinates[i];
                if (env.Intersects(c))
                    coordList.Add(c, true);
            }
            return coordList.ToCoordinateArray();
        }
    }
}
