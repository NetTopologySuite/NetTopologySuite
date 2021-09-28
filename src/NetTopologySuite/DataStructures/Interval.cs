#define picky
using System;
using System.Globalization;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.DataStructures
{
    /// <summary>
    /// Structure for a closed 1-dimensional &#x211d;-interval
    /// </summary>
    [Serializable]
    public struct Interval : IEquatable<Interval>
    {
        /// <summary>
        /// The lower bound of the interval
        /// </summary>
        public readonly double Min;

        /// <summary>
        /// The upper bound of the interval
        /// </summary>
        public double Max;

        /// <summary>
        /// Initializes this structure with <see cref="Min"/> = <see cref="Max"/> = <paramref name="value"/>
        /// </summary>
        /// <param name="value">The value for min and max</param>
        private Interval(double value)
        {
            Min = value;
            Max = value;
        }

        /// <summary>
        /// Initializes this structure with <paramref name="min"/> and <paramref name="max"/> values
        /// </summary>
        /// <param name="min">The minimum interval values</param>
        /// <param name="max">The maximum interval values</param>
        private Interval(double min, double max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Method to expand 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Interval ExpandedByValue(double p)
        {
#if picky
            // This is not a valid value, ignore it
            if (p.Equals(Coordinate.NullOrdinate))
                return this;

            // This interval has not seen a valid ordinate
            if (Min.Equals(Coordinate.NullOrdinate))
                return new Interval(p, p);
#endif
            double min = p < Min ? p : Min;
            double max = p > Max ? p : Max;
            return new Interval(min, max);
        }

        /// <summary>
        /// Gets a value if this interval is empty/undefined
        /// </summary>
        bool IsEmpty { get { return Min.Equals(Coordinate.NullOrdinate); } }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            const int prime = 31;
            // ReSharper disable NonReadonlyMemberInGetHashCode
            long temp = BitConverter.DoubleToInt64Bits(Max);
            int result = prime + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(Min);
            result = prime * result + (int)(temp ^ (temp >> 32));
            // ReSharper restore NonReadonlyMemberInGetHashCode
            return result;
        }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Interval))
                return false;
            return Equals((Interval)obj);
        }

        ///<inheritdoc/>
        public bool Equals(Interval other)
        {
            if (IsEmpty ^ other.IsEmpty)
                return false;

            if (IsEmpty && other.IsEmpty)
                return true;

            return Min == other.Min &&
                   Max == other.Max;
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return !IsEmpty
                       ? string.Format(NumberFormatInfo.InvariantInfo, "Interval \u211D[{0}, {1}] (Width={2}) ", Min,
                                       Max, Width)
                       : "Interval \u211D[Uninitialized]";
        }

        /// <summary>
        /// Gets a value indicating the width of the <see cref="Interval"/>
        /// </summary>
        public double Width { get { return Max - Min; } }

        /// <summary>
        /// Gets a value indicating the centre of the interval (Min + Width * 0.5)
        /// </summary>
        public double Centre { get { return Min + Width * 0.5; } }

        /// <summary>
        /// Function to compute an interval that contains this and <paramref name="interval"/> <see cref="Interval"/>
        /// </summary>
        /// <param name="interval">The interval</param>
        /// <returns>An interval</returns>
        public Interval ExpandedByInterval(Interval interval)
        {
#if picky
            if (IsEmpty && interval.IsEmpty)
                return Create();

            if (!IsEmpty && interval.IsEmpty)
                return this;

            if (IsEmpty && !interval.IsEmpty)
                return interval;
#endif
            double min = Min < interval.Min ? Min : interval.Min;
            double max = Max > interval.Max ? Max : interval.Max;
            return new Interval(min, max);
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
        /// Function to test if this <see cref="Interval"/> overlaps the interval &#x211d;[<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        /// <param name="min">The minimum value of the interval</param>
        /// <param name="max">The maximum value of the interval</param>
        /// <returns><c>true</c> if this interval overlaps the interval &#x211d;[<paramref name="min"/>, <paramref name="max"/>]</returns>
        public bool Overlaps(double min, double max)
        {
            return !(Min > max) && !(Max < min);
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
        /// Function to test if this <see cref="Interval"/> contains the interval &#x211d;[<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        /// <remarks>This is more rigid than <see cref="Overlaps(double, double)"/></remarks>
        /// <param name="min">The minimum value of the interval</param>
        /// <param name="max">The maximum value of the interval</param>
        /// <returns><c>true</c> if this interval contains the interval &#x211d;[<paramref name="min"/>, <paramref name="max"/>]</returns>
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

        /// <summary>
        /// Function to test if this <see cref="Interval"/> intersects the interval <paramref name="other"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <returns><c>true</c> if this interval intersects <paramref name="other"/></returns>
        public bool Intersects(Interval other)
        {
            return Intersects(other.Min, other.Max);
        }

        /// <summary>
        /// Function to test if this <see cref="Interval"/> intersects the interval &#x211d;[<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        /// <param name="min">The minimum value of the interval</param>
        /// <param name="max">The maximum value of the interval</param>
        /// <returns><c>true</c> if this interval intersects the interval &#x211d;[<paramref name="min"/>, <paramref name="max"/>].</returns>
        public bool Intersects(double min, double max)
        {
            return !(min > Max || max < Min);
        }

        /// <summary>
        /// Creates an empty or uninitialized Interval
        /// </summary>
        /// <returns>An empty or uninitialized <see cref="Interval"/></returns>
        public static Interval Create()
        {
            return new Interval(Coordinate.NullOrdinate);
        }

        /// <summary>
        /// Creates an interval with the range &#x211d;[<paramref name="value"/>,<paramref name="value"/>]
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>An <see cref="Interval"/></returns>
        public static Interval Create(double value)
        {
            return new Interval(value);
        }

        /// <summary>
        /// Creates an interval with the range &#x211d;[<paramref name="val1"/>,<paramref name="val2"/>]. <br/>
        /// If necessary, val1 and val2 are exchanged.
        /// </summary>
        /// <param name="val1">The minimum value</param>
        /// <param name="val2">The maximum value</param>
        /// <returns>An <see cref="Interval"/></returns>
        public static Interval Create(double val1, double val2)
        {
            return val1 < val2
                ? new Interval(val1, val2)
                : new Interval(val2, val1);
        }

        /// <summary>
        /// Creates an interval with the range &#x211d;[<see cref="Min"/>,<see cref="Max"/>].
        /// </summary>
        /// <param name="interval">The template interval</param>
        /// <returns>An <see cref="Interval"/></returns>
        public static Interval Create(Interval interval)
        {
            return new Interval(interval.Min, interval.Max);
        }

        /// <summary>
        /// Equality operator for <see cref="Interval"/>s
        /// </summary>
        /// <param name="lhs">The left-hand-side <see cref="Interval"/></param>
        /// <param name="rhs">The right-hand-side <see cref="Interval"/></param>
        /// <returns><c>true</c> if the <see cref="Interval"/>s are equal.</returns>
        public static bool operator ==(Interval lhs, Interval rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Inequality operator for <see cref="Interval"/>s
        /// </summary>
        /// <param name="lhs">The left-hand-side <see cref="Interval"/></param>
        /// <param name="rhs">The right-hand-side <see cref="Interval"/></param>
        /// <returns><c>true</c> if the <see cref="Interval"/>s are <b>not</b> equal.</returns>
        public static bool operator !=(Interval lhs, Interval rhs)
        {
            return !lhs.Equals(rhs);
        }


    }
}
