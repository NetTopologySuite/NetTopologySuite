using System;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary> 
    /// A contiguous portion of 1D-space. Used internally by SIRtree.
    /// </summary>
    public class Interval
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        public Interval(Interval other) : this(other.min, other.max) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public Interval(double min, double max)
        {
            Assert.IsTrue(min <= max);
            this.min = min;
            this.max = max;
        }

        private double min;
        private double max;

        /// <summary>
        /// 
        /// </summary>
        public double Centre
        {
            get
            {
                return (min + max) / 2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns><c>this</c></returns>
        public Interval ExpandToInclude(Interval other)
        {
            max = Math.Max(max, other.max);
            min = Math.Min(min, other.min);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Intersects(Interval other)
        {
            return !(other.min > max || other.max < min);
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
            Interval other = (Interval) o;
            return min == other.min && max == other.max;
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
