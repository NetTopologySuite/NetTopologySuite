using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

#if !HAS_SYSTEM_ICLONEABLE
using ICloneable = GeoAPI.ICloneable;
#endif

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A list of Coordinates, which may
    /// be set to prevent repeated coordinates from occuring in the list.
    /// </summary>
    public class CoordinateList : List<Coordinate>, ICloneable
    {
        /// <summary>
        /// Constructs a new list without any coordinates
        /// </summary>
        public CoordinateList() { }

        /// <summary>
        /// Constructs a new list from an array of Coordinates, allowing repeated points.
        /// (I.e. this constructor produces a <see cref="CoordinateList"/> with exactly the same set of points
        /// as the input array.)
        /// </summary>
        /// <param name="coord">Initial coordinates</param>
        public CoordinateList(Coordinate[] coord)
            : base(coord.Length)
        {
            //EnsureCapacity(coord.Length);
            Add(coord, true);
        }

        /// <summary>
        /// Constructs a new list from a collection of Coordinates,
        /// allows repeated points.
        /// </summary>
        /// <param name="coordList">Collection of coordinates to load into the list.</param>
        public CoordinateList(IList<Coordinate> coordList)
            : base(coordList.Count)
        {
            //EnsureCapacity(coordList.Count);
            AddAll(coordList, true);
        }

        /// <summary>
        /// Constructs a new list from a collection of Coordinates,
        /// allowing caller to specify if repeated points are to be removed.
        /// </summary>
        /// <param name="coordList">Collection of coordinates to load into the list.</param>
        /// <param name="allowRepeated">If <c>false</c>, repeated points are removed.</param>
        public CoordinateList(IList<Coordinate> coordList, bool allowRepeated)
        {
            AddAll(coordList, allowRepeated);
        }

        /// <summary>
        /// Constructs a new list from an array of Coordinates,
        /// allowing caller to specify if repeated points are to be removed.
        /// </summary>
        /// <param name="coord">Array of coordinates to load into the list.</param>
        /// <param name="allowRepeated">If <c>false</c>, repeated points are removed.</param>
        public CoordinateList(Coordinate[] coord, bool allowRepeated)
        {
            Add(coord, allowRepeated);
        }

        /// <summary>
        /// Returns the coordinate at specified index.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <return>Coordinate specified.</return>
        public Coordinate GetCoordinate(int i)
        {
            return base[i];
        }

        /// <summary>
        /// Adds a section of an array of coordinates to the list.
        /// </summary>
        /// <param name="coord">The coordinates</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed</param>
        /// <param name="start">The index to start from</param>
        /// <param name="end">The index to add up to but not including</param>
        /// <returns>true (as by general collection contract)</returns>
        public bool Add(Coordinate[] coord, bool allowRepeated, int start, int end)
        {
            int inc = 1;
            if (start > end) inc = -1;

            for (int i = start; i != end; i += inc)
            {
                Add(coord[i], allowRepeated);
            }
            return true;
        }


        /// <summary>
        /// Adds an array of coordinates to the list.
        /// </summary>
        /// <param name="coord">Coordinates to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <param name="direction">If false, the array is added in reverse order.</param>
        /// <returns>Return true.</returns>
        public bool Add(Coordinate[] coord, bool allowRepeated, bool direction)
        {
            if (direction)
                for (int i = 0; i < coord.Length; i++)
                    Add(coord[i], allowRepeated);
            else
                for (int i = coord.Length - 1; i >= 0; i--)
                    Add(coord[i], allowRepeated);
            return true;
        }

        /// <summary>
        /// Adds an array of coordinates to the list.
        /// </summary>
        /// <param name="coord">Coordinates to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true.</returns>
        public bool Add(Coordinate[] coord, bool allowRepeated)
        {
            return Add(coord, allowRepeated, true);
        }

        /// <summary>
        /// Adds a coordinate to the list.
        /// </summary>
        /// <param name="obj">Coordinate to be inserted, as object.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true.</returns>
        public bool Add(object obj, bool allowRepeated)
        {
            return Add((Coordinate)obj, allowRepeated);
        }

        /// <summary>
        /// Adds a coordinate to the end of this list.
        /// </summary>
        /// <param name="coord">Coordinate to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true if all ok.</returns>
        public bool Add(Coordinate coord, bool allowRepeated)
        {
            // don't add duplicate coordinates
            if (!allowRepeated)
            {
                if (Count >= 1)
                {
                    Coordinate last = this[Count - 1];
                    if (last.Equals2D(coord))
                        return false;
                }
            }
            Add(coord);
            return true;
        }

        /// <summary>
        /// Inserts the specified coordinate at the specified position in this list.
        /// </summary>
        /// <param name="i">The position at which to insert</param>
        /// <param name="coord">the coordinate to insert</param>
        /// <param name="allowRepeated">if set to false, repeated coordinates are collapsed</param>
        public void Add(int i, Coordinate coord, bool allowRepeated)
        {
            // don't add duplicate coordinates
            if (!allowRepeated)
            {
                int size = Count;
                if (size > 0)
                {
                    if (i > 0)
                    {
                        Coordinate prev = this[i - 1];
                        if (prev.Equals2D(coord)) return;
                    }
                    if (i < size)
                    {
                        Coordinate next = this[i];
                        if (next.Equals2D(coord)) return;
                    }
                }
            }
            Insert(i, coord);
        }

        /// <summary>
        /// Add an array of coordinates.
        /// </summary>
        /// <param name="coll">Coordinates collection to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true if at least one element has added (IList not empty).</returns>
        public bool AddAll(IList<Coordinate> coll, bool allowRepeated)
        {
            bool isChanged = false;
            foreach (Coordinate c in coll)
            {
                Add(c, allowRepeated);
                isChanged = true;
            }
            return isChanged;
        }

        /// <summary>
        /// Ensure this coordList is a ring, by adding the start point if necessary.
        /// </summary>
        public void CloseRing()
        {
            if (Count > 0)
                Add(new Coordinate(this[0]), false);
        }

        /// <summary>
        /// Returns the Coordinates in this collection.
        /// </summary>
        /// <returns>Coordinater as <c>Coordinate[]</c> array.</returns>
        public Coordinate[] ToCoordinateArray()
        {
            return ToArray();
        }

        /// <summary>
        /// Returns a deep copy of this collection.
        /// </summary>
        /// <returns>The copied object.</returns>
        public object Clone()
        {
            CoordinateList copy = new CoordinateList();
            foreach (Coordinate c in this)
                copy.Add((Coordinate)c.Clone());
            return copy;
        }
    }
}
