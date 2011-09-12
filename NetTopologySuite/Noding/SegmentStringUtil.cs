using System.Collections.Generic;
#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IEnvelope = NetTopologySuite.Geometries.Envelope;
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IPoint = NetTopologySuite.Geometries.Point;
using ILineString = NetTopologySuite.Geometries.LineString;
using ILinearRing = NetTopologySuite.Geometries.LinearRing;
using IPolygon = NetTopologySuite.Geometries.Polygon;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using IGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using IPrecisionModel = NetTopologySuite.Geometries.PrecisionModel;
#endif
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Utility methods for processing <see cref="ISegmentString"/>s
    ///</summary>
    /// <author>Martin Davis</author>
    public class SegmentStringUtil
    {
        ///<summary>
        /// Extracts all linear components from a given <see cref="IGeometry"/> to <see cref="ISegmentString"/>s.
        ///</summary>
        /// <param name="geom">the geometry to extract from</param>
        /// <returns>a List of SegmentStrings
        /// </returns>
        public static IList<ISegmentString> ExtractSegmentStrings(IGeometry geom)
        {
            IList<ISegmentString> segStr = new List<ISegmentString>();
            ICollection<ILineString> lines = LinearComponentExtracter.GetLines(geom);
            foreach (ILineString line in lines)
            {
                ICoordinate[] pts = line.Coordinates;
                segStr.Add(new NodedSegmentString(pts, geom));
            }
            return segStr;
        }

    }
}