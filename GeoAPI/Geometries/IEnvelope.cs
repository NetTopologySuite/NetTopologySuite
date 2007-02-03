using System;
using System.Collections.Generic;
using System.Text;

namespace GeoAPI.Geometries
{
    public interface IEnvelope : ICloneable, IComparable, IComparable<IEnvelope>, IEquatable<IEnvelope>
    {
        double Area { get; }

        double Width { get; }

        double Height { get; }

        double MaxX { get; }

        double MaxY { get; }

        double MinX { get; }

        double MinY { get; }

        ICoordinate Centre { get; }
        
        bool Contains(double x, double y);
        
        bool Contains(ICoordinate p);
        
        bool Contains(IEnvelope other);
        
        double Distance(IEnvelope env);
        
        void ExpandBy(double distance);
        
        void ExpandBy(double deltaX, double deltaY);
        
        void ExpandToInclude(ICoordinate p);
        
        void ExpandToInclude(double x, double y);
        
        void ExpandToInclude(IEnvelope other);

        void Init();

        void Init(ICoordinate p);

        void Init(IEnvelope env);

        void Init(ICoordinate p1, ICoordinate p2);
        
        void Init(double x1, double x2, double y1, double y2);

        IEnvelope Intersection(IEnvelope env);

        void Translate(double transX, double transY);

        IEnvelope Union(IPoint point);
        
        IEnvelope Union(ICoordinate coord);
        
        IEnvelope Union(IEnvelope box);        

        bool Intersects(ICoordinate p);
        
        bool Intersects(double x, double y);
        
        bool Intersects(IEnvelope other);
        
        bool IsNull { get; }

        void SetToNull();

        void Zoom(double perCent);
                
        bool Overlaps(IEnvelope other);

        bool Overlaps(ICoordinate p);
        
        bool Overlaps(double x, double y);
        
        void SetCentre(double width, double height);
        
        void SetCentre(IPoint centre, double width, double height);
        
        void SetCentre(ICoordinate centre);
        
        void SetCentre(IPoint centre);
        
        void SetCentre(ICoordinate centre, double width, double height);                
    }
}
