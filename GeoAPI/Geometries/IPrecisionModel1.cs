using System;
namespace GeoAPI.Geometries
{
    public interface IPrecisionModel : IComparable, IComparable<IPrecisionModel>, IEquatable<IPrecisionModel>
    {        
        double MakePrecise(double val);
        void MakePrecise(GeoAPI.Geometries.ICoordinate coord);

        bool IsFloating { get; }
        int MaximumSignificantDigits { get; }
    }
}
