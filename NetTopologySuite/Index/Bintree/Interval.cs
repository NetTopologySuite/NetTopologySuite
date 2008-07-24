namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary> 
    /// Represents an (1-dimensional) closed interval on the Real number line.
    /// </summary>
    public class Interval
    {
        private double min, max;

        /// <summary>
        /// 
        /// </summary>
        public double Min
        {
            get { return min;  }
            set { min = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Max
        {
            get { return max;  }
            set { max = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Width
        {
            get { return Max - Min; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Interval()
        {
            min = 0.0;
            max = 0.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public Interval(double min, double max)
        {
            Init(min, max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        public Interval(Interval interval)
        {
            Init(interval.Min, interval.Max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Init(double min, double max)
        {
            Min = min;
            Max = max;

            if (min > max)
            {
                Min = max;
                Max = min;
            }
        }
               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        public void ExpandToInclude(Interval interval)
        {
            if (interval.Max > Max) 
                Max = interval.Max;
            if (interval.Min < Min) 
                Min = interval.Min;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public bool Overlaps(Interval interval)
        {
            return Overlaps(interval.Min, interval.Max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public bool Overlaps(double min, double max)
        {
            if (Min > max || Max < min) 
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public bool Contains(Interval interval)
        {
            return Contains(interval.Min, interval.Max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public bool Contains(double min, double max)
        {
            return (min >= Min && max <= Max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Contains(double p)
        {
            return (p >= Min && p <= Max);
        }
    }
}
