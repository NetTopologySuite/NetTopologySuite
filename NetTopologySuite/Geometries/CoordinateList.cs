using System;
using System.Collections;
using System.Diagnostics;

namespace GisSharpBlog.NetTopologySuite.Geometries
{       
    /// <summary>
    /// A list of Coordinates, which may
    /// be set to prevent repeated coordinates from occuring in the list.
    /// </summary>
    public class CoordinateList : ArrayList
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
        public CoordinateList(Coordinate[] coord)
        {
            Add(coord, true);
        }

        /// <summary>
        /// Constructs a new list from a collection of Coordinates,
        /// allows repeated points.
        /// </summary>
        /// <param name="coordList">Collection of coordinates to load into the list.</param>
        public CoordinateList(IList coordList)
        {
            AddAll(coordList, true);
        }

        /// <summary>
        /// Constructs a new list from a collection of Coordinates,
        /// allowing caller to specify if repeated points are to be removed.
        /// </summary>
        /// <param name="coordList">Collection of coordinates to load into the list.</param>
        /// <param name="allowRepeated">If <c>false</c>, repeated points are removed.</param>
        public CoordinateList(IList coordList, bool allowRepeated)
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
        /// Returns the number of elements in CoordinateList collection.
        /// </summary>
        public override int Count
        {
            get
            {
                return base.Count;
            }
        }

        /// <summary>
        /// Returns the coordinate at specified index.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <return>Coordinate specified.</return>
        public virtual Coordinate GetCoordinate(int i)
        {
            return (Coordinate)base[i];
        }

        /// <summary>
        /// Returns the coordinate at specified index.
        /// </summary>
        /// <param name="i">Coordinate index.</param>
        /// <return>Coordinate specified.</return>
        public override object this[int i]
        {
            get
            {
                return base[i];
            }
        }

        /// <summary>
        /// Add an array of coordinates.
        /// </summary>
        /// <param name="coord">Coordinates to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <param name="direction">If false, the array is added in reverse order.</param>
        /// <returns>Return true.</returns>
        public virtual bool Add(Coordinate[] coord, bool allowRepeated, bool direction)
        {
            if (direction)
                for(int i = 0; i < coord.Length; i++)
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
        public virtual bool Add(Coordinate[] coord, bool allowRepeated)
        {
            return Add(coord, allowRepeated, true);
        }

        /// <summary>
        /// Add a coordinate.
        /// </summary>
        /// <param name="obj">Coordinate to be inserted, as object.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true.</returns>
        public virtual bool Add(object obj, bool allowRepeated)
        {
            return Add((Coordinate)obj, allowRepeated);
        }

        /// <summary>
        /// Add a coordinate.
        /// </summary>
        /// <param name="coord">Coordinate to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true if all ok.</returns>
        public virtual bool Add(Coordinate coord, bool allowRepeated)
        {
            // don't add duplicate coordinates
            if (!allowRepeated)
            {
                if (this.Count >= 1)
                {
                    Coordinate last = (Coordinate) this[this.Count - 1];
                    if(last.Equals2D(coord)) 
                        return false;
                }
            }
            this.Add(coord);
            return true;
        }

        /// <summary>
        /// Add an array of coordinates.
        /// </summary>
        /// <param name="coll">Coordinates collection to be inserted.</param>
        /// <param name="allowRepeated">If set to false, repeated coordinates are collapsed.</param>
        /// <returns>Return true if at least one element has added (IList not empty).</returns>
        public virtual bool AddAll(IList coll, bool allowRepeated)
        {
            bool isChanged = false;
            foreach(Coordinate c in coll)
            {
                Add(c, allowRepeated);
                isChanged = true;
            }
            return isChanged;
        }

        /// <summary>
        /// Ensure this coordList is a ring, by adding the start point if necessary.
        /// </summary>
        public virtual void CloseRing()
        {
            if(this.Count > 0)
                Add(this[0], false);
        }

        /// <summary>
        /// Returns the Coordinates in this collection.
        /// </summary>
        /// <returns>Coordinater as <c>Coordinate[]</c> array.</returns>
        public virtual Coordinate[] ToCoordinateArray()
        {
            return (Coordinate[])this.ToArray(typeof(Coordinate));
        }

        /// <summary>
        /// Returns a deep copy of this collection.
        /// </summary>
        /// <returns>The copied object.</returns>
        public override object Clone()
        {
            CoordinateList copy = (CoordinateList)base.Clone();
            foreach (Coordinate c in this)
                copy.Add(c.Clone());  
            return copy;
        }
    } 
} 
