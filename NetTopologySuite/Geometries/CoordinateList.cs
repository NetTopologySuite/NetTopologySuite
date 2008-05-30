using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{       
    /// <summary>
    /// A list of Coordinates, which may
    /// be set to prevent repeated coordinates from occuring in the list.
    /// </summary>
    public class CoordinateList : List<ICoordinate>, ICloneable
    {
        /// <summary>
        /// Constructs a new list without any coordinates
        /// </summary>
        public CoordinateList() : base() {}

        /// <summary>
        /// The basic constructor for a CoordinateArray allows repeated points
        /// (i.e produces a CoordinateList with exactly the same set of points).
        /// </summary>
        /// <param name="coord">Initial coordinates</param>
        public CoordinateList(ICoordinate[] coord)
        {
            Add(coord, true);
        }

        /// <summary>
        /// Constructs a new list from a collection of Coordinates,
        /// allows repeated points.
        /// </summary>
        /// <param name="coordList">Collection of coordinates to load into the list.</param>
        public CoordinateList(List<ICoordinate> coordList)
        {
            AddAll(coordList, true);
        }

        /// <summary>
        /// Constructs a new list from a collection of Coordinates,
        /// allowing caller to specify if repeated points are to be removed.
        /// </summary>
        /// <param name="coordList">Collection of coordinates to load into the list.</param>
        /// <param name="allowRepeated">If <c>false</c>, repeated points are removed.</param>
        public CoordinateList(List<ICoordinate> coordList, bool allowRepeated)
        {
            AddAll(coordList, allowRepeated);
        }

        /// <summary>
        /// Constructs a new list from an array of Coordinates,
        /// allowing caller to specify if repeated points are to be removed.
        /// </summary>
        /// <param name="coord">Array of coordinates to load into the list.</param>
        /// <param name="allowRepeated">If <c>false</c>, repeated points are removed.</param>
        public CoordinateList(ICoordinate[] coord, bool allowRepeated)
        {
            Add(coord, allowRepeated);
        }        

        /// <summary>
        /// Returns the coordinate at specified index.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <return>Coordinate specified.</return>
        public ICoordinate GetCoordinate(int i)
        {
            return base[i];
        }        

        /// <summary>
        /// Add an array of coordinates.
        /// </summary>
        /// <param name="coord">Coordinates to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <param name="direction">If false, the array is added in reverse order.</param>
        /// <returns>Return true.</returns>
        public bool Add(ICoordinate[] coord, bool allowRepeated, bool direction)
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
        /// Add an array of coordinates.
        /// </summary>
        /// <param name="coord">Coordinates to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true.</returns>
        public bool Add(ICoordinate[] coord, bool allowRepeated)
        {
            return Add(coord, allowRepeated, true);
        }

        /// <summary>
        /// Add a coordinate.
        /// </summary>
        /// <param name="obj">Coordinate to be inserted, as object.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true.</returns>
        public bool Add(object obj, bool allowRepeated)
        {
            return Add((ICoordinate) obj, allowRepeated);
        }

        /// <summary>
        /// Add a coordinate.
        /// </summary>
        /// <param name="coord">Coordinate to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true if all ok.</returns>
        public bool Add(ICoordinate coord, bool allowRepeated)
        {
            // don't add duplicate coordinates
            if (!allowRepeated)
            {
                if (Count >= 1)
                {
                    ICoordinate last = this[Count - 1];
                    if (last.Equals2D(coord)) 
                        return false;
                }
            }
            Add(coord);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="coord"></param>
        /// <param name="allowRepeated"></param>
        public void Add(int i, ICoordinate coord, bool allowRepeated)
        {
            // don't add duplicate coordinates
            if (!allowRepeated)
            {
                int size = Count;
                if (size > 0)
                {
                    if (i > 0)
                    {
                        ICoordinate prev = this[i - 1];
                        if (prev.Equals2D(coord)) return;
                    }
                    if (i < size)
                    {
                        ICoordinate next = this[i];
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
        public bool AddAll(List<ICoordinate> coll, bool allowRepeated)
        {
            bool isChanged = false;
            foreach (ICoordinate c in coll)
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
                Add(this[0], false);
        }

        /// <summary>
        /// Returns the Coordinates in this collection.
        /// </summary>
        /// <returns>Coordinater as <c>Coordinate[]</c> array.</returns>
        public ICoordinate[] ToCoordinateArray()
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
            foreach (ICoordinate c in this)
                copy.Add((ICoordinate) c.Clone());
            return copy;
        }        
    } 
} 
