using System;
using System.Collections;

using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    interface IGeometryFactory
    {
        ICoordinateSequenceFactory CoordinateSequenceFactory { get; }

        int SRID { get; }
        PrecisionModel PrecisionModel { get; }
                
        IGeometry BuildGeometry(ICollection geomList);
        IGeometry CreateGeometry(IGeometry g);
        
        Point CreatePoint(ICoordinate coordinate);
        Point CreatePoint(ICoordinateSequence coordinates);        

        ILineString CreateLineString(ICoordinate[] coordinates);
        ILineString CreateLineString(ICoordinateSequence coordinates);

        ILinearRing CreateLinearRing(ICoordinate[] coordinates);
        ILinearRing CreateLinearRing(ICoordinateSequence coordinates);

        IPolygon CreatePolygon(ILinearRing shell, ILinearRing[] holes);

        IMultiPoint CreateMultiPoint(ICoordinate[] coordinates);
        IMultiPoint CreateMultiPoint(IPoint[] point);
        IMultiPoint CreateMultiPoint(ICoordinateSequence coordinates);

        IMultiLineString CreateMultiLineString(ILineString[] lineStrings);
        
        IMultiPolygon CreateMultiPolygon(IPolygon[] polygons);
        
        IGeometryCollection CreateGeometryCollection(IGeometry[] geometries);
    }
}
