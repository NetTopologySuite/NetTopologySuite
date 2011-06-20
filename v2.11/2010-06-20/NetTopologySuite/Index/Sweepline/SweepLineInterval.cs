using System;

namespace GisSharpBlog.NetTopologySuite.Index.Sweepline
{
    // DESIGN_NOTE: Looks like a value type
    public class SweepLineInterval
    {
        private readonly Object _item;
        private readonly Double _max;
        private readonly Double _min;

        public SweepLineInterval(Double min, Double max) : this(min, max, null)
        {
        }

        public SweepLineInterval(Double min, Double max, object item)
        {
            _min = min < max ? min : max;
            _max = max > min ? max : min;
            _item = item;
        }

        public Double Min
        {
            get { return _min; }
        }

        public Double Max
        {
            get { return _max; }
        }

        public Object Item
        {
            get { return _item; }
        }
    }
}