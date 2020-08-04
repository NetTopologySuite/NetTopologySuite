using System;

namespace NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// Represents an (1-dimensional) closed interval on the Real number line.
    /// </summary>
    [Serializable]
    public class Interval
    {
        private double _min;
        private double _max;

        /// <summary>
        /// Gets or sets a value indicating the minimum value of the closed interval.
        /// </summary>
        public double Min
        {
            get => _min;
            set => _min = value;
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum value of the closed interval.
        /// </summary>
        public double Max
        {
            get => _max;
            set => _max = value;
        }

        /// <summary>
        /// Gets the width of the interval (<see cref="Max"/> - <see cref="Min"/>)
        /// </summary>
        public double Width => Max - Min;

        /// <summary>
        /// Gets the centre of the interval (<see cref="Min"/> + <see cref="Width"/> * 0.5d)
        /// </summary>
        public double Centre => (Min + Max) * 0.5d;

        /// <summary>
        /// Creates a new interval instance, setting <see cref="Min"/>=<see cref="Max"/>=0d;
        /// </summary>
        public Interval()
        {
            _min = 0.0;
            _max = 0.0;
        }

        /// <summary>
        /// Creates a new interval instance, setting <see cref="Min"/>=<paramref name="min"/> and <see cref="Max"/>=<paramref name="max"/>;
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        public Interval(double min, double max)
        {
            Init(min, max);
        }

        /// <summary>
        /// Creates a new interval instance, setting <see cref="Min"/>=<paramref name="interval.Min"/> and <see cref="Max"/>=<paramref name="interval.Max"/>.
        /// </summary>
        /// <param name="interval"></param>
        public Interval(Interval interval)
        {
            Init(interval.Min, interval.Max);
        }

        /// <summary>
        /// Method to initialize the interval with the given <paramref name="min"/> and <paramref name="max"/> values. <br/>
        /// If <paramref name="max"/> &lt; <paramref name="min"/>, their values are exchanged.
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
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
        /// Method to expand this interval to contain <paramref name="interval"/>.
        /// </summary>
        /// <param name="interval">The interval to contain.</param>
        public void ExpandToInclude(Interval interval)
        {
            if (interval.Max > Max)
                Max = interval.Max;
            if (interval.Min < Min)
                Min = interval.Min;
        }

        /// <summary>
        /// Function to test if this <see cref="Interval"/> overlaps <paramref name="interval"/>.
        /// </summary>
        /// <param name="interval">The interval to test</param>
        /// <returns><c>true</c> if this interval overlaps <paramref name="interval"/></returns>
        public bool Overlaps(Interval interval)
        {
            return Overlaps(interval.Min, interval.Max);
        }

        /// <summary>
        /// Function to test if this <see cref="Interval"/> overlaps the interval R[<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        /// <param name="min">The mimimum value of the interval</param>
        /// <param name="max">The maximum value of the interval</param>
        /// <returns><c>true</c> if this interval overlaps the interval R[<paramref name="min"/>, <paramref name="max"/>]</returns>
        public bool Overlaps(double min, double max)
        {
            if (Min > max || Max < min)
                return false;
            return true;
        }

        /// <summary>
        /// Function to test if this <see cref="Interval"/> contains <paramref name="interval"/>.
        /// </summary>
        /// <remarks>This is more rigid than <see cref="Overlaps(Interval)"/></remarks>
        /// <param name="interval">The interval to test</param>
        /// <returns><c>true</c> if this interval contains <paramref name="interval"/></returns>
        public bool Contains(Interval interval)
        {
            return Contains(interval.Min, interval.Max);
        }

        /// <summary>
        /// Function to test if this <see cref="Interval"/> contains the interval R[<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        /// <remarks>This is more rigid than <see cref="Overlaps(double, double)"/></remarks>
        /// <param name="min">The mimimum value of the interval</param>
        /// <param name="max">The maximum value of the interval</param>
        /// <returns><c>true</c> if this interval contains the interval R[<paramref name="min"/>, <paramref name="max"/>]</returns>
        public bool Contains(double min, double max)
        {
            return (min >= Min && max <= Max);
        }

        /// <summary>
        /// Function to test if this <see cref="Interval"/> contains the value <paramref name="p"/>.
        /// </summary>
        /// <param name="p">The value to test</param>
        /// <returns><c>true</c> if this interval contains the value <paramref name="p"/></returns>
        public bool Contains(double p)
        {
            return (p >= Min && p <= Max);
        }
    }
}
