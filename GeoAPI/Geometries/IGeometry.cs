using System;
using System.Collections.Generic;
using System.Text;

using GeoAPI.Operations.Buffer;

namespace GeoAPI.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGeometry : ICloneable, IComparable, IComparable<IGeometry>, IEquatable<IGeometry>
    {
        int SRID { get; set; }

        string GeometryType { get; } 

        /// <summary>
        /// A ISurface method moved in IGeometry 
        /// </summary>
        double Area { get; }

        /// <summary>
        /// A ICurve method moved in IGeometry
        /// </summary>
        double Length { get; }        

        /// <summary>
        /// A IGeometryCollection method moved in IGeometry
        /// </summary>
        int NumGeometries { get; }

        /// <summary>
        /// A ILineString method moved to IGeometry
        /// </summary>
        int NumPoints { get; }        

        IGeometry Boundary { get; set; }

        Dimensions BoundaryDimension { get; set; }

        /// <summary>
        /// A ISurface method moved in IGeometry 
        /// </summary>
        IPoint Centroid { get; }                        
        
        ICoordinate Coordinate { get; }
        
        ICoordinate[] Coordinates { get; }       
                        
        Dimensions Dimension { get; set; }
                
        IGeometry Envelope { get; }

        IEnvelope EnvelopeInternal { get; }

        IPoint InteriorPoint { get; }

        /// <summary>
        /// A ISurface method moved in IGeometry 
        /// </summary>        
        IPoint PointOnSurface { get; }        

        /// <summary>
        /// A IGeometryCollection method moved in IGeometry
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        IGeometry GetGeometryN(int n);   
                               
        void Normalize();

        byte[] AsBinary();
        
        string AsText();
        
        object UserData { get; set; }

        IGeometry ConvexHull();

        IntersectionMatrix Relate(IGeometry g);

        IGeometry Difference(IGeometry other);

        IGeometry SymmetricDifference(IGeometry other);

        IGeometry Buffer(double distance);

        IGeometry Buffer(double distance, int quadrantSegments);

        IGeometry Buffer(double distance, BufferStyle endCapStyle);

        IGeometry Buffer(double distance, int quadrantSegments, BufferStyle endCapStyle);

        IGeometry Intersection(IGeometry other);

        IGeometry Union(IGeometry other);

        bool EqualsExact(IGeometry other);

        bool EqualsExact(IGeometry other, double tolerance);

        bool IsEmpty { get; }

        bool IsRectangle { get; }

        bool IsSimple { get; }

        bool IsValid { get; }

        bool Within(IGeometry g);

        bool Contains(IGeometry g);

        bool IsWithinDistance(IGeometry geom, double distance);

        bool CoveredBy(IGeometry g);

        bool Covers(IGeometry g);

        bool Crosses(IGeometry g);

        bool Intersects(IGeometry g);

        bool Overlaps(IGeometry g);

        bool Relate(IGeometry g, string intersectionPattern);

        bool Touches(IGeometry g);

        bool Disjoint(IGeometry g);

        double Distance(IGeometry g);

        void Apply(ICoordinateFilter filter);

        void Apply(IGeometryFilter filter);

        void Apply(IGeometryComponentFilter filter);

        void GeometryChanged();

        void GeometryChangedAction();
    }
}
