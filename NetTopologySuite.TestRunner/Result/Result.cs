using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace Open.Topology.TestRunner.Result
{
    public interface IResult
    {
        bool Equals(IResult other, double tolerance);

        String ToShortString();

        String ToLongString();

        String ToFormattedString();
         
    }

    public interface IResult<T> : IResult
    {
        T Value { get; }
    }
    
    public class BooleanResult : IResult<Boolean>
    {
        public BooleanResult(bool result)
        {
            Value = result;
        }
        
        public Boolean Value { get; private set; }

        public bool Equals(IResult other, double tolerance)
        {
            if (!(other is IResult<Boolean>))
                return false;
            return Value == ((IResult<Boolean>)other).Value;
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

    public class DoubleResult : IResult<Double>
    {
        public DoubleResult(Double result)
        {
            Value = result;
        }

        public Double Value { get; private set; }

        public bool Equals(IResult other, double tolerance)
        {
            if (!(other is IResult<Double>))
                return false;
            return Math.Abs(Value - ((IResult<Double>)other).Value) <= tolerance;
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

    public class IntegerResult : IResult<Int32>
    {
        public IntegerResult(int result)
        {
            Value = result;
        }

        public Int32 Value { get; private set; }

        public bool Equals(IResult other, double tolerance)
        {
            if (!(other is IResult<int>))
                return false;
            return Math.Abs(Value - ((IResult<Int32>)other).Value) <= tolerance;
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

            var thisGeometryClone = (IGeometry)Value.Clone();
            var otherGeometryClone = (IGeometry)otherGeometry.Clone();
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