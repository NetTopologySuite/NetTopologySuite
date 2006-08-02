using System;
using System.Collections;
using System.Text;

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
        public virtual double Min
        {
            get 
            { 
                return min; 
            }
            set 
            { 
                min = value; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual double Max
        {
            get 
            { 
                return max; 
            }
            set 
            { 
                max = value; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual double Width
        {
            get
            {
                return Max - Min;
            }
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
        public virtual void Init(double min, double max)
        {
            this.Min = min;
            this.Max = max;

            if (min > max)
            {
                this.Min = max;
                this.Max = min;
            }
        }
               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        public virtual void ExpandToInclude(Interval interval)
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
        public virtual bool Overlaps(Interval interval)
        {
            return Overlaps(interval.Min, interval.Max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public virtual bool Overlaps(double min, double max)
        {
            if (this.Min > max || this.Max < min) 
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public virtual bool Contains(Interval interval)
        {
            return Contains(interval.Min, interval.Max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public virtual bool Contains(double min, double max)
        {
            return (min >= this.Min && max <= this.Max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual bool Contains(double p)
        {
            return (p >= this.Min && p <= this.Max);
        }
    }
}
