//#define C5
using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

#if C5
using C5;
#else
using System.Collections.Generic;
#endif
namespace GisSharpBlog.NetTopologySuite.Geometries
{
#if C5
    public class CoordinateList<TCoordinate> : ArrayList<TCoordinate>
#else
    public class CoordinateList<TCoordinate> : List<TCoordinate>
#endif
    where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        ///<summary>
        /// Constructs a new list without any coordinates
        ///</summary>
        public CoordinateList()
        {
        }

        ///<summary>
        /// Constructs a new list from an array of Coordinates, allowing repeated points.
        /// (I.e. this constructor produces a {@link CoordinateList} with exactly the same set of points
        /// as the input array.)
        ///</summary>
        ///<param name="coord">the initial coordinates</param>
        public CoordinateList(ICoordinateSequence<TCoordinate> coord)
            : base(coord)
        {
        }

        ///<summary>
        /// Constructs a new list from an array of Coordinates, allowing caller to specify if repeated points are to be removed.
        ///</summary>
        ///<param name="coord">the array of coordinates to load into the list</param>
        ///<param name="allowRepeated">if <see langword="false"/>, repeated points are removed</param>
        public CoordinateList(ICoordinateSequence<TCoordinate> coord, Boolean allowRepeated)
            : base(coord.Count)
        {
            AddRange(coord, allowRepeated);
        }

        ///<summary>
        /// Add an enumeration of coordinates
        ///</summary>
        ///<param name="coords">the coordinates</param>
        ///<param name="allowRepeated">if set to false, repeated coordinates are collapsed</param>
        ///<param name="direction">if false, the array is added in reverse order</param>
        public void AddRange(IEnumerable<TCoordinate> coords, Boolean allowRepeated, Boolean direction)
        {
            if (direction)
            {
                foreach (TCoordinate coord in coords)
                    Add(coord, allowRepeated);
            }
            else
            {
                foreach (TCoordinate coord in coords)
                    Insert(0, coord, allowRepeated);
            }
        }

        ///<summary>
        /// Add an enumeration of coordinates
        ///</summary>
        ///<param name="coords">The coordinates</param>
        ///<param name="allowRepeated">if set to false, repeated coordinates are collapsed</param>
        public void AddRange(IEnumerable<TCoordinate> coords, Boolean allowRepeated)
        {
            AddRange(coords, allowRepeated, true);
        }

        ///<summary>
        /// Add a coordinate
        ///</summary>
        ///<param name="obj">the coordinate to add</param>
        ///<param name="allowRepeated">if set to <see langword="false"/>, repeated coordinates are collapsed</param>
        public void Add(Object obj, Boolean allowRepeated)
        {
            if ( obj is TCoordinate)
                Add((TCoordinate)obj, allowRepeated);
        }

        ///<summary>
        /// Adds a coordinate to the end of the list.
        ///</summary>
        ///<param name="coord">the coordinate</param>
        ///<param name="allowRepeated">if set to <see langword="false"/>, repeated coordinates are collapsed</param>
        public void Add(TCoordinate coord, Boolean allowRepeated)
        {
            // don't add duplicate coordinates
            if (!allowRepeated)
            {
                if (Count >= 1)
                {
                    TCoordinate last = this[Count-1];
                    if (last.Equals(coord)) return;
                }
            }
            Add(coord);
        }

        ///<summary>
        /// Inserts the specified coordinate at the specified position in this list.
        ///</summary>
        ///<param name="i">the position at which to insert</param>
        ///<param name="coord"></param>
        ///<param name="allowRepeated">if set to false, repeated coordinates are collapsed</param>
        public void Insert(Int32 i, TCoordinate coord, Boolean allowRepeated)
        {
            // don't add duplicate coordinates
            if (!allowRepeated)
            {
                Int32 count = Count;
                if (count > 0)
                {
                    if (i > 0)
                    {
                        TCoordinate prev = this[i - 1];
                        if (prev.Equals(coord)) return;
                    }
                    if (i < count)
                    {
                        TCoordinate next = this[i];
                        if (next.Equals(coord)) return;
                    }
                }
            }
            Insert(i, coord);
        }

        ///<summary>
        /// Add an array of coordinates
        ///</summary>
        ///<param name="coll">the coordinates</param>
        ///<param name="allowRepeated">if set to <see langword="false"/>, repeated coordinates are collapsed</param>
        public void AddAll(IEnumerable<TCoordinate> coll, Boolean allowRepeated)
        {
            AddRange(coll, allowRepeated);
        }

        ///<summary>
        /// Ensure this coordList is a ring, by adding the start point if necessary
        ///</summary>
        public void CloseRing()
        {
            if (Count > 0)
                Add(this[0], false);
        }

        /** Returns the Coordinates in this collection.
         *
         * @return the coordinates
         */
        ///<summary>
        /// Returns the Coordinates in this collection as a <see cref="ICoordinateSequenceFactory{TCoordinate}"/>.
        ///</summary>
        ///<param name="factory">factory to create the sequence</param>
        ///<returns></returns>
        public ICoordinateSequence<TCoordinate> ToCoordinateSequence(ICoordinateSequenceFactory<TCoordinate> factory)
        {
            if (factory == null)
                return null;

            return factory.Create(this);
        }

        ///<summary>
        /// Returns a deep copy of this <tt>CoordinateList</tt> instance.
        ///</summary>
        ///<returns>clone of this <tt>CoordinateList</tt> instance</returns>
        public CoordinateList<TCoordinate> Clone()
        {
            CoordinateList<TCoordinate> clone = new CoordinateList<TCoordinate>();
            foreach (TCoordinate coord in this)
                clone.Add(coord.Clone());

            return clone;
        }
    }
}
