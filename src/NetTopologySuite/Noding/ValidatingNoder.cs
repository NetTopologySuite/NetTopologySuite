using System.Collections.Generic;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// A wrapper for <see cref="INoder"/>s which validates
    /// the noding is correct.
    /// </summary>
    /// <author>Martin Davis</author>
    public class ValidatingNoder : INoder
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

        /// <inheritdoc cref="INoder.ComputeNodes"/>
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
