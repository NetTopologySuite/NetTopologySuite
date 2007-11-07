using System;

namespace GisSharpBlog.NetTopologySuite.Index.Sweepline
{
    // DESIGN_NOTE: Looks like a value type
    public class SweepLineInterval
    {
        private Double min, max;
        private Object item;

        public SweepLineInterval(Double min, Double max) : this(min, max, null) {}

        public SweepLineInterval(Double min, Double max, object item)
        {
            this.min = min < max ? min : max;
            this.max = max > min ? max : min;
            this.item = item;
        }

        public Double Min
        {
            get { return min; }
        }

        public Double Max
        {
            get { return max; }
        }

        public Object Item
        {
            get { return item; }
        }
    }
}