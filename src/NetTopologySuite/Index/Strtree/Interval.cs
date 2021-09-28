using System;
using NetTopologySuite.Geometries;
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
        /// Gets the centre of the interval.
        /// </summary>
        public double Centre => (_min + _max) * 0.5d;

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

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            const int prime = 31;
            // ReSharper disable NonReadonlyMemberInGetHashCode
            long temp = BitConverter.DoubleToInt64Bits(_max);
            int result = prime + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(_min);
            result = prime * result + (int)(temp ^ (temp >> 32));
            // ReSharper restore NonReadonlyMemberInGetHashCode
            return result;
        }

    }
}
