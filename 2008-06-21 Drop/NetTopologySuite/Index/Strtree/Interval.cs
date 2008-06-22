using System;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    // DESIGN_NOTE: Looks like a value type
    /// <summary> 
    /// A contiguous portion of 1D-space. Used internally by SIRtree.
    /// </summary>
    public class Interval
    {
        public Interval(Interval other) : this(other.min, other.max) {}

        public Interval(Double min, Double max)
        {
            Assert.IsTrue(min <= max);
            this.min = min;
            this.max = max;
        }

        private Double min;
        private Double max;

        public Double Centre
        {
            get { return (min + max)/2; }
        }

        public Interval ExpandToInclude(Interval other)
        {
            max = Math.Max(max, other.max);
            min = Math.Min(min, other.min);
            return this;
        }

        public Boolean Intersects(Interval other)
        {
            return !(other.min > max || other.max < min);
        }

        public override Boolean Equals(object o)
        {
            if (!(o is Interval))
            {
                return false;
            }
            Interval other = (Interval) o;
            return min == other.min && max == other.max;
        }
    }
}