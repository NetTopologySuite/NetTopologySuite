using System;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// A contiguous portion of 1D-space. Used internally by SIRtree.
    /// </summary>
    public class Interval : IIntersectable<Interval>, IExpandable<Interval>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        public Interval(Interval other) : this(other._min, other._max) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public Interval(double min, double max)
        {
            Assert.IsTrue(min <= max);
            _min = min;
            _max = max;
        }

        private double _min;
        private double _max;
        /// <summary>
        ///
        /// </summary>
        public double Centre => (_min + _max) / 2;

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns><c>this</c></returns>
        public void ExpandToInclude(Interval other)
        {
            _max = Math.Max(_max, other._max);
            _min = Math.Min(_min, other._min);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns><c>this</c></returns>
        public Interval ExpandedBy(Interval other)
        {
            _max = Math.Max(_max, other._max);
            _min = Math.Min(_min, other._min);
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Intersects(Interval other)
        {
            return !(other._min > _max || other._max < _min);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            if (!(o is Interval))
                return false;
            var other = (Interval) o;
            return _min == other._min && _max == other._max;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
