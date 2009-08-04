using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Dissolves a noded collection of <see cref="NodedSegmentString{TCoordinate}" />s to produce
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
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ISegmentStringMerger _merger;

        private readonly SortedDictionary<ICoordinateSequence<TCoordinate>, NodedSegmentString<TCoordinate>>
            _orientedCoordinateMap =
                new SortedDictionary<ICoordinateSequence<TCoordinate>, NodedSegmentString<TCoordinate>>();

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
            : this(null)
        {
        }

        /// <summary>
        /// Gets the collection of dissolved (i.e. unique) <see cref="NodedSegmentString{TCoordinate}" />s
        /// </summary>
        public IEnumerable<NodedSegmentString<TCoordinate>> Dissolved
        {
            get { return _orientedCoordinateMap.Values; }
        }

        /// <summary>
        /// Dissolve all <see cref="NodedSegmentString{TCoordinate}" />s 
        /// in the input set.
        /// </summary>
        public void Dissolve(IEnumerable<NodedSegmentString<TCoordinate>> segStrings)
        {
            foreach (NodedSegmentString<TCoordinate> segmentString in segStrings)
            {
                Dissolve(segmentString);
            }
        }

        /// <summary>
        /// Dissolve the given <see cref="NodedSegmentString{TCoordinate}" />.
        /// </summary>
        /// <param name="segString"></param>
        public void Dissolve(NodedSegmentString<TCoordinate> segString)
        {
            ICoordinateSequence<TCoordinate> orientedSequence = segString.Coordinates;
            //OrientedCoordinateArray<TCoordinate> oca = new OrientedCoordinateArray<TCoordinate>(segString.Coordinates);

            NodedSegmentString<TCoordinate> existing;
            _orientedCoordinateMap.TryGetValue(orientedSequence, out existing);

            if (existing == null)
            {
                add(orientedSequence, segString);
            }
            else
            {
                if (_merger != null)
                {
                    Boolean isSameOrientation = existing.Coordinates.Equals(segString.Coordinates);
                    _merger.Merge(existing, segString, isSameOrientation);
                }
            }
        }

        private void add(ICoordinateSequence<TCoordinate> oca, NodedSegmentString<TCoordinate> segString)
        {
            _orientedCoordinateMap.Add(oca, segString);
        }

        #region Nested type: ISegmentStringMerger

        public interface ISegmentStringMerger
        {
            /// <summary>
            /// Updates the context data of a <see cref="NodedSegmentString{TCoordinate}" />
            /// when an identical (up to orientation) one is found during dissolving.
            /// </summary>
            /// <param name="mergeTarget">The segment string to update.</param>
            /// <param name="ssToMerge">The segment string being dissolved.</param>
            /// <param name="isSameOrientation">
            /// <see langword="true"/> if the strings are in the same direction,
            /// <c>false</c> if they are opposite.
            /// </param>
            void Merge(NodedSegmentString<TCoordinate> mergeTarget, NodedSegmentString<TCoordinate> ssToMerge,
                       Boolean isSameOrientation);
        }

        #endregion
    }
}