using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// A wrapper for <see cref="INoder"/>s which validates
    /// the output arrangement is correctly noded.
    /// An arrangement of line segments is fully noded if
    /// there is no line segment
    /// which has another segment intersecting its interior.
    /// If the noding is not correct, a <see cref="TopologyException"/> is thrown
    /// with details of the first invalid location found.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="FastNodingValidator"/>
    public sealed class ValidatingNoder : INoder
    {

        private readonly INoder _noder;
        private IList<ISegmentString> _nodedSs;

        /// <summary>
        /// Creates a noding validator wrapping the given <paramref name="noder"/>
        /// </summary>
        /// <param name="noder">The noder to validate</param>
        public ValidatingNoder(INoder noder)
        {
            this._noder = noder;
        }

        /// <summary>
        /// Checks whether the output of the wrapped noder is fully noded.
        /// Throws an exception if it is not.
        /// </summary>
        /// <exception cref="TopologyException"></exception>
        public void ComputeNodes(IList<ISegmentString> segStrings)
        {
            _noder.ComputeNodes(segStrings);
            _nodedSs = _noder.GetNodedSubstrings();
            Validate();
        }

        private void Validate()
        {
            var nv = new FastNodingValidator(_nodedSs);
            nv.CheckValid();
        }

        /// <inheritdoc cref="INoder.GetNodedSubstrings"/>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return _nodedSs;
        }

    }

}
