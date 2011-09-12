using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

#region geoapi vs nts
#if useFullGeoAPI
using ICoordinate = GeoAPI.Geometries.ICoordinate;
using IGeometry = GeoAPI.Geometries.IGeometry;
using IPoint = GeoAPI.Geometries.IPoint;
using ILineString = GeoAPI.Geometries.ILineString;
using ILinearRing = GeoAPI.Geometries.ILinearRing;
using IPolygon = GeoAPI.Geometries.IPolygon;
using IGeometryCollection = GeoAPI.Geometries.IGeometryCollection;
using IMultiPoint = GeoAPI.Geometries.IMultiPoint;
using IMultiLineString = GeoAPI.Geometries.IMultiLineString;
using IMultiPolygon = GeoAPI.Geometries.IMultiPolygon;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IPoint = NetTopologySuite.Geometries.Point;
using ILineString = NetTopologySuite.Geometries.LineString;
using ILinearRing = NetTopologySuite.Geometries.LinearRing;
using IPolygon = NetTopologySuite.Geometries.Polygon;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using ICoordinateSequenceFilter = NetTopologySuite.Geometries.ICoordinateSequenceFilter;
using ICoordinateSequence = NetTopologySuite.Geometries.ICoordinateSequence;
#endif
#endregion

namespace NetTopologySuite.Precision
{
    ///<summary>
    /// Reduces the precision of the {@link Coordinate}s in a
    /// <see cref="ICoordinateSequence"/> to match the supplied <see cref="PrecisionModel"/>.
    ///</summary>
    /// <remarks>
    /// Uses <see cref="PrecisionModel.MakePrecise(double)"/>.
    /// The input is modified in-place, so
    /// it should be cloned beforehand if the
    /// original should not be modified.
    /// </remarks>
    /// <author>mbdavis</author>
    public class CoordinatePrecisionReducerFilter : ICoordinateSequenceFilter
    {
        private readonly PrecisionModel _precModel;

        ///<summary>
        /// Creates a new precision reducer filter.
        ///</summary>
        /// <param name="precModel">The PrecisionModel to use</param>
        public CoordinatePrecisionReducerFilter(PrecisionModel precModel)
        {
            _precModel = precModel;
        }

        ///<summary>
        /// Rounds the Coordinates in the sequence to match the PrecisionModel
        ///</summary>
        public void Filter(ICoordinateSequence seq, int i)
        {
            seq.SetOrdinate(i, Ordinate.X, _precModel.MakePrecise(seq.GetOrdinate(i, Ordinate.X)));
            seq.SetOrdinate(i, Ordinate.Y, _precModel.MakePrecise(seq.GetOrdinate(i, Ordinate.Y)));
        }

        ///<summary>
        /// Always runs over all geometry components.
        ///</summary>
        public bool Done { get { return false; } }

        ///<summary>
        /// Always reports that the geometry has changed
        ///</summary>
        public bool GeometryChanged { get { return true; } }
    }
}