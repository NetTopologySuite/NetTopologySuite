using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace Open.Topology.TestRunner.Result
{
    public interface IResult
    {
        bool Equals(IResult other, double tolerance);

        string ToShortString();

        string ToLongString();

        string ToFormattedString();

    }

    public interface IResult<T> : IResult
    {
        T Value { get; }
    }

    public class BooleanResult : IResult<bool>
    {
        public BooleanResult(bool result)
        {
            Value = result;
        }

        public bool Value { get; private set; }

        public bool Equals(IResult other, double tolerance)
        {
            if (!(other is IResult<bool>))
                return false;
            return Value == ((IResult<bool>)other).Value;
        }

        public string ToShortString()
        {
            return Value ? "true" : "false";
        }

        public string ToLongString()
        {
            return ToShortString();
        }

        public string ToFormattedString()
        {
            return ToShortString();
        }
    }

    public class DoubleResult : IResult<double>
    {
        public DoubleResult(double result)
        {
            Value = result;
        }

        public double Value { get; private set; }

        public bool Equals(IResult other, double tolerance)
        {
            if (!(other is IResult<double>))
                return false;
            return Math.Abs(Value - ((IResult<double>)other).Value) <= tolerance;
        }

        public string ToShortString()
        {
            return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public string ToLongString()
        {
            return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public string ToFormattedString()
        {
            return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public class IntegerResult : IResult<int>
    {
        public IntegerResult(int result)
        {
            Value = result;
        }

        public int Value { get; private set; }

        public bool Equals(IResult other, double tolerance)
        {
            if (!(other is IResult<int>))
                return false;
            return Math.Abs(Value - ((IResult<int>)other).Value) <= tolerance;
        }

        public string ToShortString()
        {
            return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public string ToLongString()
        {
            return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public string ToFormattedString()
        {
            return Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public class GeometryResult : IResult<IGeometry>
    {
        public GeometryResult(IGeometry result)
        {
            Value = result;
        }

        public IGeometry Value { get; private set; }

        public bool Equals(IResult other, double tolerance)
        {
            if (!(other is IResult<IGeometry>))
                return false;
            var otherGeometryResult = (IResult<IGeometry>)other;
            var otherGeometry = otherGeometryResult.Value;

            var thisGeometryClone = (IGeometry)Value.Copy();
            var otherGeometryClone = (IGeometry)otherGeometry.Copy();
            thisGeometryClone.Normalize();
            otherGeometryClone.Normalize();

            return thisGeometryClone.EqualsExact(otherGeometryClone, tolerance);

        }

        public string ToShortString()
        {
            return Value.GeometryType;
        }

        public string ToLongString()
        {
            return Value.AsText();
        }

        public string ToFormattedString()
        {
            var wktWriter = new WKTWriter {Formatted = true};
            return wktWriter.WriteFormatted(Value);
        }
    }

}