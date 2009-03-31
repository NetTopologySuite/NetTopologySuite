using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    ///	Useful utility functions for handling Coordinate arrays.
    /// </summary>
    public static class CoordinateArrays
    {
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
        public static ICoordinate PointNotInList(ICoordinate[] testPts, ICoordinate[] pts)
        {
            for (var i = 0; i < testPts.Length; i++)
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
        public static int Compare(ICoordinate[] pts1, ICoordinate[] pts2)
        {
            var i = 0;
            while (i < pts1.Length && i < pts2.Length)
            {
                var compare = pts1[i].CompareTo(pts2[i]);
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
        public static int IncreasingDirection(ICoordinate[] pts)
        {
            for (var i = 0; i < pts.Length / 2; i++)
            {
                var j = pts.Length - 1 - i;
                // skip equal points on both ends
                var comp = pts[i].CompareTo(pts[j]);
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
        private static bool IsEqualReversed(ICoordinate[] pts1, ICoordinate[] pts2)
        {
            for (var i = 0; i < pts1.Length; i++)
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
        public static ICoordinate[] CopyDeep(ICoordinate[] coordinates)
        {
            var copy = new ICoordinate[coordinates.Length];            
	        for(var i = 0; i < coordinates.Length; i++)            
            	copy[i] = new Coordinate(coordinates[i]);            
            return copy;
        }

        /// <summary>
        /// Converts the given <see cref="IList" /> of 
        /// <see cref="Coordinate" />s into a <see cref="Coordinate" /> array.
        /// </summary>
        /// <param name="coordList"><see cref="IList" /> of coordinates.</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">
        /// If <paramref name="coordList"/> contains not only <see cref="Coordinate" />s.
        /// </exception>
        [Obsolete("Use generic method instead")]
        public static ICoordinate[] ToCoordinateArray(ICollection coordList)
        {
            var tempList = new List<ICoordinate>(coordList.Count);
            foreach (ICoordinate coord in coordList)
                tempList.Add(coord);
            return tempList.ToArray();
        }

        /// <summary>
        /// Converts the given <see cref="IList" /> of 
        /// <see cref="Coordinate" />s into a <see cref="Coordinate" /> array.
        /// </summary>
        /// <param name="coordList"><see cref="IList" /> of coordinates.</param>
        /// <returns></returns>
        public static ICoordinate[] ToCoordinateArray(ICollection<ICoordinate> coordList)
        {
            var tempList = new List<ICoordinate>(coordList.Count);
            foreach (var coord in coordList)
                tempList.Add(coord);
            return tempList.ToArray();
        }

        /// <summary>
        /// Returns whether Equals returns true for any two consecutive
        /// coordinates in the given array.
        /// </summary>
        /// <param name="coord">Array of Coordinates.</param>
        /// <returns>true if coord has repeated points; false otherwise.</returns>
      	public static bool HasRepeatedPoints(ICoordinate[] coord)
      	{
            for(var i = 1; i < coord.Length; i++)            
            	if(coord[i - 1].Equals(coord[i]))                
                    return true;            	
	        return false;
      	}

        /// <summary>
        /// Returns either the given coordinate array if its length is greater than
        /// the given amount, or an empty coordinate array.
        /// </summary>
        /// <param name="n">Length amount.</param>
        /// <param name="c">Array of Coordinates.</param>
        /// <returns>New Coordinate array.</returns>
        public static ICoordinate[] AtLeastNCoordinatesOrNothing(int n, ICoordinate[] c)
      	{
            return (c.Length >= n) ? (c) : (new ICoordinate[] { });
        }

        /// <summary>
        /// If the coordinate array argument has repeated points,
        /// constructs a new array containing no repeated points.
        /// Otherwise, returns the argument.
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>        
      	public static ICoordinate[] RemoveRepeatedPoints(ICoordinate[] coord)
      	{
            if (!HasRepeatedPoints(coord))
                return coord;
            var coordList = new CoordinateList(coord, false);
            return coordList.ToCoordinateArray();
      	}

        /// <summary>
        /// Reverses the coordinates in an array in-place.
        /// </summary>
        /// <param name="coord">Array of Coordinates.</param>
      	public static void Reverse(ICoordinate[] coord)
      	{
            // This implementation uses FCL capabilities
            Array.Reverse(coord);

            /* Old code from JTS
            int last = coord.Length - 1;
            int mid = last / 2;
            for (int i = 0; i <= mid; i++)
            {
                Coordinate tmp = coord[i];
                coord[i] = coord[last - i];
                coord[last - i] = tmp;
            }
            */
      	}

        /// <summary>
        /// Returns <c>true</c> if the two arrays are identical, both <c>null</c>, or pointwise
        /// equal (as compared using Coordinate.Equals).
        /// </summary>
        /// <param name="coord1">First array of Coordinates.</param>
        /// <param name="coord2">Second array of Coordinates.</param>
        /// <returns><c>true</c> if two Coordinates array are equals; false otherwise</returns>
        public static bool Equals(ICoordinate[] coord1, ICoordinate[] coord2)
      	{
            if (coord1 == coord2) 
                return true;
            if (coord1 == null || coord2 == null) 
                return false;
            if (coord1.Length != coord2.Length) 
                return false;
            for (var i = 0; i < coord1.Length; i++)
            	if (!coord1[i].Equals(coord2[i])) 
                    return false;            
            return true;
        }

        /// <summary>
        /// Compares two <see cref="Coordinate" /> arrays
        /// in the forward direction of their coordinates,
        /// using lexicographic ordering.
        /// </summary>        
        public class ForwardComparator : IComparer<ICoordinate[]>
        {
            /// <summary>
            /// Compares the specified <see cref="Coordinate" />s arrays.
            /// </summary>
            /// <param name="pts1"></param>
            /// <param name="pts2"></param>
            /// <returns></returns>
            public int Compare(ICoordinate[] pts1, ICoordinate[] pts2)
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
        public class BidirectionalComparator : IComparer<ICoordinate[]>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pts1"></param>
            /// <param name="pts2"></param>
            /// <returns></returns>
            public int Compare(ICoordinate[] pts1, ICoordinate[] pts2)
            {
                if (pts1.Length < pts2.Length)
                    return -1;
                if (pts1.Length > pts2.Length)
                    return 1;

                if (pts1.Length == 0)
                    return 0;

                var forwardComp = CoordinateArrays.Compare(pts1, pts2);
                var isEqualRev = IsEqualReversed(pts1, pts2);
                if (isEqualRev)
                    return 0;

                return forwardComp;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pts1"></param>
            /// <param name="pts2"></param>
            /// <returns></returns>
            public int OLDcompare(ICoordinate[] pts1, ICoordinate[] pts2)
            {
                if (pts1.Length < pts2.Length)
                    return -1;
                if (pts1.Length > pts2.Length)
                    return 1;

                if (pts1.Length == 0)
                    return 0;

                var dir1 = IncreasingDirection(pts1);
                var dir2 = IncreasingDirection(pts2);

                var i1 = dir1 > 0 ? 0 : pts1.Length - 1;
                var i2 = dir2 > 0 ? 0 : pts1.Length - 1;

                for (var i = 0; i < pts1.Length; i++)
                {
                    var comparePt = pts1[i1].CompareTo(pts2[i2]);
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
        ///  A <see cref="IComparer" /> for <see cref="Coordinate" />s.
        /// </param>
        /// <returns></returns>
        public static bool Equals(ICoordinate[] coord1, ICoordinate[] coord2,
                IComparer<ICoordinate[]> coordinateComparer)
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
      	public static ICoordinate MinCoordinate(ICoordinate[] coordinates)
        {
            ICoordinate minCoord = null;
            for (var i = 0; i < coordinates.Length; i++)
            	if (minCoord == null || minCoord.CompareTo(coordinates[i]) > 0)
                    minCoord = coordinates[i];
	        return minCoord;
        }

        /// <summary>
        /// Shifts the positions of the coordinates until <c>firstCoordinate</c> is first.
        /// </summary>
        /// <param name="coordinates">Array to rearrange.</param>
        /// <param name="firstCoordinate">Coordinate to make first.</param>
        public static void Scroll(ICoordinate[] coordinates, ICoordinate firstCoordinate)
        {
            var i = IndexOf(firstCoordinate, coordinates);
            if (i < 0) 
                return;
            var newCoordinates = new ICoordinate[coordinates.Length];
            Array.Copy(coordinates, i, newCoordinates, 0, coordinates.Length - i);
            Array.Copy(coordinates, 0, newCoordinates, coordinates.Length - i, i);
            Array.Copy(newCoordinates, 0, coordinates, 0, coordinates.Length);
        }

        /// <summary>
        /// Returns the index of <paramref name="coordinate" /> in <paramref name="coordinates" />.
        /// The first position is 0; the second is 1; etc.
        /// </summary>
        /// <param name="coordinate">A <see cref="Coordinate" /> to search for.</param>
        /// <param name="coordinates">A <see cref="Coordinate" /> array to search.</param>
        /// <returns>The position of <c>coordinate</c>, or -1 if it is not found.</returns>
        public static int IndexOf(ICoordinate coordinate, ICoordinate[] coordinates)
        {
            for (var i = 0; i < coordinates.Length; i++)
        	    if (coordinate.Equals(coordinates[i]))
                    return i;
            return -1;
      	}

        /// <summary>
        /// Extracts a subsequence of the input <see cref="Coordinate" /> array
        /// from indices <paramref name="start" /> to <paramref name="end"/> (inclusive).
        /// </summary>
        /// <param name="pts">The input array.</param>
        /// <param name="start">The index of the start of the subsequence to extract.</param>
        /// <param name="end">The index of the end of the subsequence to extract.</param>
        /// <returns>A subsequence of the input array.</returns>
        public static ICoordinate[] Extract(ICoordinate[] pts, int start, int end)
        {
            // Code using FLC features
            var len = end - start + 1;
            var extractPts = new ICoordinate[len];
            Array.Copy(pts, start, extractPts, 0, len);

            /* Original JTS code
            int iPts = 0;
            for (int i = start; i <= end; i++)
                extractPts[iPts++] = pts[i];            
            */
            return extractPts;             
        }
    } 
} 
