using System;
using System.Collections.Generic;
using System.Text;

namespace GeoAPI.Geometries
{
    public interface ICoordinate : ICloneable, IComparable, IComparable<ICoordinate>, IEquatable<ICoordinate>
    {
        double X { get; set; }

        double Y { get; set; }
        
        double Z { get; set; }

        ICoordinate CoordinateValue { get; set; }

        double Distance(ICoordinate p);
        
        bool Equals2D(ICoordinate other);
        
        bool Equals3D(ICoordinate other);        
    }
}
