using System;
using GeoAPI.DataStructures;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    public struct DoubleBinTreeValue : IAddable<DoubleBinTreeValue>,
                                       IAddable<Double, DoubleBinTreeValue>, 
                                       IDivisible<Double, DoubleBinTreeValue>
    {
        private readonly Double _value;

        public DoubleBinTreeValue(Double value)
        {
            _value = value;
        }

        #region IAddable<DoubleBinTreeValue> Members

        public DoubleBinTreeValue Add(DoubleBinTreeValue other)
        {
            return new DoubleBinTreeValue(_value + other._value);
        }

        #endregion

        #region IAddable<DoubleBinTreeValue> Members

        public DoubleBinTreeValue Add(Double other)
        {
            return new DoubleBinTreeValue(_value + other);
        }

        #endregion

        #region IDivisible<Double,DoubleBinTreeValue> Members

        public DoubleBinTreeValue Divide(Double other)
        {
            return new DoubleBinTreeValue(_value / other);
        }

        #endregion

        public static implicit operator Double(DoubleBinTreeValue value)
        {
            return value._value;
        }
    }

    /// <summary>
    /// A <see cref="BinTreeKey"/> is a unique identifier for a node in a tree.
    /// </summary>
    /// <remarks>
    /// It contains a lower-left point and a level number. The level number
    /// is the power of two for the size of the node envelope.
    /// </remarks>
    public class BinTreeKey : AbstractNodeKey<Interval, DoubleBinTreeValue>
    {
        protected override Int32 ComputeLevel(Interval interval)
        {
            Double dx = interval.Width;
            Int32 level = DoubleBits.GetExponent(dx) + 1;
            return level;
        }

        public BinTreeKey(Interval interval)
            : base(interval) { }

        //private void computeInterval(Int32 level, Interval itemInterval)
        //{
        //    Double size = DoubleBits.PowerOf2(level);
        //    Value = Math.Floor(itemInterval.Min / size) * size;
        //    Bounds = new Interval(_location, _location + size);
        //}

        protected override DoubleBinTreeValue GetBoundsMax()
        {
            return new DoubleBinTreeValue(Bounds.Max);
        }

        protected override DoubleBinTreeValue GetBoundsMin()
        {
            return new DoubleBinTreeValue(Bounds.Min);
        }

        protected override Interval CreateBounds(DoubleBinTreeValue min, Double nodeSize)
        {
            return new Interval(min, min + nodeSize);
        }

        protected override DoubleBinTreeValue CreateLocation(Interval bounds, Double nodeSize)
        {
            throw new NotImplementedException();
        }

        //protected override void ComputeKey(Interval bounds)
        //{
        //    Level = ComputeLevel(bounds);
        //    Bounds = new Interval();
        //    computeInterval(Level, bounds);

        //    // TODO: MD - would be nice to have a non-iterative form of this algorithm
        //    while (!Bounds.Contains(bounds))
        //    {
        //        Level += 1;
        //        computeInterval(Level, bounds);
        //    }
        //}
    }
}