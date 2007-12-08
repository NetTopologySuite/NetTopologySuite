using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Dissolves a noded collection of <see cref="SegmentString{TCoordinate}" />s to produce
    /// a set of merged linework with unique segments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A custom merging strategy can be applied when two identical (up to orientation)
    /// strings are dissolved together.
    /// The default merging strategy is simply to discard the merged string.
    /// </para>
    /// <para>
    /// A common use for this class is to merge noded edges
    /// while preserving topological labeling.
    /// </para>
    /// </remarks>
    public class SegmentStringDissolver<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public interface ISegmentStringMerger
        {
            /// <summary>
            /// Updates the context data of a <see cref="SegmentString{TCoordinate}" />
            /// when an identical (up to orientation) one is found during dissolving.
            /// </summary>
            /// <param name="mergeTarget">The segment string to update.</param>
            /// <param name="ssToMerge">The segment string being dissolved.</param>
            /// <param name="isSameOrientation">
            /// <see langword="true"/> if the strings are in the same direction,
            /// <c>false</c> if they are opposite.
            /// </param>
            void Merge(SegmentString<TCoordinate> mergeTarget, SegmentString<TCoordinate> ssToMerge, Boolean isSameOrientation);
        }

        private readonly ISegmentStringMerger _merger;

        private readonly SortedDictionary<OrientedCoordinateArray<TCoordinate>, SegmentString<TCoordinate>> _ocaMap =
            new SortedDictionary<OrientedCoordinateArray<TCoordinate>, SegmentString<TCoordinate>>();

        /// <summary>
        /// Creates a dissolver with a user-defined merge strategy.
        /// </summary>
        public SegmentStringDissolver(ISegmentStringMerger merger)
        {
            _merger = merger;
        }

        /// <summary>
        /// Creates a dissolver with the default merging strategy.
        /// </summary>
        public SegmentStringDissolver()
            : this(null) {}

        /// <summary>
        /// Dissolve all <see cref="SegmentString{TCoordinate}" />s 
        /// in the input set.
        /// </summary>
        public void Dissolve(IEnumerable<SegmentString<TCoordinate>> segStrings)
        {
            foreach (SegmentString<TCoordinate> segmentString in segStrings)
            {
                Dissolve(segmentString);
            }
        }

        /// <summary>
        /// Dissolve the given <see cref="SegmentString{TCoordinate}" />.
        /// </summary>
        /// <param name="segString"></param>
        public void Dissolve(SegmentString<TCoordinate> segString)
        {
            OrientedCoordinateArray<TCoordinate> oca = new OrientedCoordinateArray<TCoordinate>(segString.Coordinates);

            SegmentString<TCoordinate> existing;
            _ocaMap.TryGetValue(oca, out existing);

            if (existing == null)
            {
                add(oca, segString);
            }
            else
            {
                if (_merger != null)
                {
                    Boolean isSameOrientation = CoordinateArrays.Equals(existing.Coordinates, segString.Coordinates);
                    _merger.Merge(existing, segString, isSameOrientation);
                }
            }
        }

        /// <summary>
        /// Gets the collection of dissolved (i.e. unique) <see cref="SegmentString{TCoordinate}" />s
        /// </summary>
        public IEnumerable<SegmentString<TCoordinate>> Dissolved
        {
            get { return _ocaMap.Values; }
        }

        private void add(OrientedCoordinateArray<TCoordinate> oca, SegmentString<TCoordinate> segString)
        {
            _ocaMap.Add(oca, segString);
        }
    }
}