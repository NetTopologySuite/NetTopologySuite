using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a collection of TaggedLineStrings, preserving topology
    /// (in the sense that no new intersections are introduced).
    /// </summary>
    public class TaggedLinesSimplifier<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly LineSegmentIndex<TCoordinate> _inputIndex = new LineSegmentIndex<TCoordinate>();
        private readonly LineSegmentIndex<TCoordinate> _outputIndex = new LineSegmentIndex<TCoordinate>();
        private Double _distanceTolerance = 0.0;

        /// <summary>
        /// Gets or sets the distance tolerance for the simplification.
        /// Points closer than this tolerance to a simplified segment may
        /// be removed.
        /// </summary>        
        public Double DistanceTolerance
        {
            get { return _distanceTolerance; }
            set { _distanceTolerance = value; }
        }

        /// <summary>
        /// Simplify a collection of <see cref="TaggedLineString{TCoordinate}"/>s.
        /// </summary>
        /// <param name="taggedLines">The collection of lines to simplify.</param>
        public void Simplify(IEnumerable<TaggedLineString<TCoordinate>> taggedLines)
        {
            foreach (TaggedLineString<TCoordinate> taggedLine in taggedLines)
            {
                _inputIndex.Add(taggedLine);
            }

            foreach (TaggedLineString<TCoordinate> taggedLine in taggedLines)
            {
                TaggedLineStringSimplifier<TCoordinate> tlss
                    = new TaggedLineStringSimplifier<TCoordinate>(_inputIndex, _outputIndex);
                tlss.DistanceTolerance = _distanceTolerance;
                tlss.Simplify(taggedLine);
            }
        }
    }
}