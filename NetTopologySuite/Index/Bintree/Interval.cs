using System;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    // DESIGN_NOTE: Looks like a value type
    /// <summary> 
    /// Represents an (1-dimensional) closed interval on the Real number line.
    /// </summary>
    public class Interval
    {
        private Double min, max;

        public Double Min
        {
            get { return min; }
            set { min = value; }
        }

        public Double Max
        {
            get { return max; }
            set { max = value; }
        }

        public Double Width
        {
            get { return Max - Min; }
        }

        public Interval()
        {
            min = 0.0;
            max = 0.0;
        }

        public Interval(Double min, Double max)
        {
            Init(min, max);
        }

        public Interval(Interval interval)
        {
            Init(interval.Min, interval.Max);
        }

        public void Init(Double min, Double max)
        {
            Min = min;
            Max = max;

            if (min > max)
            {
                Min = max;
                Max = min;
            }
        }

        public void ExpandToInclude(Interval interval)
        {
            if (interval.Max > Max)
            {
                Max = interval.Max;
            }
            if (interval.Min < Min)
            {
                Min = interval.Min;
            }
        }

        public Boolean Overlaps(Interval interval)
        {
            return Overlaps(interval.Min, interval.Max);
        }

        public Boolean Overlaps(Double min, Double max)
        {
            if (Min > max || Max < min)
            {
                return false;
            }
            return true;
        }

        public Boolean Contains(Interval interval)
        {
            return Contains(interval.Min, interval.Max);
        }

        public Boolean Contains(Double min, Double max)
        {
            return (min >= Min && max <= Max);
        }

        public Boolean Contains(Double p)
        {
            return (p >= Min && p <= Max);
        }
    }
}