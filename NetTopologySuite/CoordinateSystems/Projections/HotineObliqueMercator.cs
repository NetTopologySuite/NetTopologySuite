// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.


using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.DataStructures;
using GeoAPI.Units;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Projections
{

    internal class InverseHotineObliqueMercator<TCoordinate> : HotineObliqueMercator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public InverseHotineObliqueMercator(IEnumerable<ProjectionParameter> parameters, ICoordinateFactory<TCoordinate> factory, bool naturalOriginOffsets, HotineObliqueMercator<TCoordinate> transform)
            : base(parameters, factory, naturalOriginOffsets)
        {
            Inverse = transform;
        }

        public override string Name
        {
            get { return "Inverse_" + base.Name; }
        }

        public override bool IsInverse
        {
            get { return true; }
        }

        public override TCoordinate Transform(TCoordinate point)
        {
            // Inverse equations
            // -----------------
            Double x = point[Ordinates.X] * MetersPerUnit - false_easting;
            Double y = point[Ordinates.Y] * MetersPerUnit - false_northing;
            Double vs = x * cosgrid - y * singrid;
            Double us = y * cosgrid + x * singrid;
            if (!_naturalOriginOffsets) us = us + u;
            Double q = Math.Exp(-bl * vs / al);
            Double s = .5 * (q - 1.0 / q);
            Double t = .5 * (q + 1.0 / q);
            Double vl = Math.Sin(bl * us / al);
            Double ul = (vl * cosgam + s * singam) / t;
            if (Math.Abs(Math.Abs(ul) - 1.0) <= Epsilon)
                return CreateCoordinate((Degrees) new Radians(lon_origin), (Degrees) new Radians(HalfPI)*Sign(ul), point);

            Double con = 1.0 / bl;
            Double ts1 = Math.Pow((el / Math.Sqrt((1.0 + ul) / (1.0 - ul))), con);
            Double lat = ComputePhi2(E, ts1);
            con = Math.Cos(bl * us / al);
            Double theta = lon_origin - Math.Atan2((s * cosgam - vl * singam), con) / bl;
            Double lon = AdjustLongitude(theta);

            return CreateCoordinate((Degrees)new Radians(lon), (Degrees)new Radians(lat), point);
        }
    }
    
    /// <summary>
    /// Summary description for HotineObliqueMercator.
    /// </summary>
    /// <remarks>
    /// <para>Transforms input longitude and latitude to easting and northing
    /// for the Oblique Mercator projection using Hotine's simplified
    /// calculations of hyperbolic functions.  The ellipsoid is conformally
    /// projected onto a sphere of constant total curvature, called a "aposphere".
    /// The sphere is then projected to a cylinder around the sphere so that it
    /// touches the surface along the great circle path chosen for the central line,
    /// instead of along the Earth's Equator.  As a result, the Hotine is perfectly
    /// conformal, but the scale is true only along the central line.  The natural
    /// origin of the projection is approximately at the intersection of the
    /// central line with the equator of the "aposphere."</para>
    ///
    /// <para>Reference: John P. Snyder (Map Projections - A Working Manual,
    ///            U.S. Geological Survey Professional Paper 1395, 1987)</para>
    /// </remarks>
    internal class HotineObliqueMercator<TCoordinate> : MapProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        protected Boolean _naturalOriginOffsets;

        readonly Radians azimuth;
        readonly Double scale_factor;        /* scale factor                */
        protected Radians lon_origin;            /* center longitude            */
        Radians lat_origin;            /* center latitude            */
        //double e, es;                /* eccentricity constants    */
        protected Double false_northing;        /* y offset in meters        */
        protected Double false_easting;        /* x offset in meters        */
        readonly double sin_p20;    /* sin and Math.cos values        */
        readonly double cos_p20;    /* sin and Math.cos values        */
        protected Double bl;
        protected Double al;
        double d;
        protected Double el, u;
        protected Double singam, cosgam;
        double sinaz, cosaz;
        protected Double singrid, cosgrid;

        /// <summary>
        /// Creates an instance of an HotineObliqueMercator projection object.
        /// </summary>
        /// <param name="parameters">List of parameters to initialize the projection.</param>
        /// <param name="naturalOriginOffsets">Flag indicating whether the false easting and northing values are relative to the projections "natural" origin (true) or to the center of the projection (false)</param>
        /// <param name="inverse">Flag indicating whether is a forward/projection (false) or an inverse projection (true).</param>
        /// <remarks>
        /// <para>The parameters this projection expects are listed below.</para>
        /// <list type="bullet">
        /// <listheader><term>Items</term><description>Descriptions</description></listheader>
        /// <item><term>scale_factor</term><description>The factor by which the map grid is reduced or enlarged during the projection process, defined by its value along the central line.</description></item>
        /// <item><term>azimuth</term><description>The angle of azimuth east of north, for the central line as it passes through the center of the map.</description></item>
        /// <item><term>longitude_of_center</term><description>The longitude of the selected center of the map, falling on the central line.</description></item>
        /// <item><term>latitude_of_center</term><description>The latitude of the selected center of the map, falling on the central line.</description></item>
        /// <item><term>rectified_grid_angle</term><description>The angle used to rotate/rectify the coordinates back from their oblique orientation.  This is generally equal to or very close to the azimuth.</description></item>
        /// <item><term>false_easting</term><description>The desired east value of the natural origin OR the center of projection depending on naturalOriginOffsets's value</description></item>
        /// <item><term>false_northing</term><description>The desired north value of the natural origin OR the center of projection depending on naturalOriginOffsets's value</description></item>
        /// </list>
        /// </remarks>
        public HotineObliqueMercator(IEnumerable<ProjectionParameter> parameters, ICoordinateFactory<TCoordinate> factory, bool naturalOriginOffsets)
            : base(parameters, factory)
        {
            _naturalOriginOffsets = naturalOriginOffsets;
            Authority = "EPSG";
            AuthorityCode = naturalOriginOffsets ? "9812" : "9815";
            ProjectionParameter par_scale_factor = GetParameter("scale_factor");
            ProjectionParameter par_azimuth = GetParameter("azimuth");
            ProjectionParameter par_longitude_of_center = GetParameter("longitude_of_center");
            ProjectionParameter par_latitude_of_center = GetParameter("latitude_of_center");
            ProjectionParameter par_rectified_grid_angle = GetParameter("rectified_grid_angle");
            ProjectionParameter par_false_easting = GetParameter("false_easting");
            ProjectionParameter par_false_northing = GetParameter("false_northing");
            //Check for missing parameters
            if ( par_scale_factor == null )
                throw new ArgumentException("Missing projection parameter 'scale_factor'");
            if ( par_azimuth == null )
                throw new ArgumentException("Missing projection parameter 'azimuth'");
            if ( par_longitude_of_center == null )
                throw new ArgumentException("Missing projection parameter 'longitude_of_center'");
            if ( par_latitude_of_center == null )
                throw new ArgumentException("Missing projection parameter 'latitude_of_center'");
            if ( par_rectified_grid_angle == null )
                throw new ArgumentException("Missing projection parameter 'rectified_grid_angle'");
            if ( par_false_easting == null )
                throw new ArgumentException("Missing projection parameter 'false_easting'");
            if ( par_false_northing == null )
                throw new ArgumentException("Missing projection parameter 'false_northing'");

            double ts;
            double f, g;

            scale_factor = par_scale_factor.Value;
            azimuth = (Radians)new Degrees(par_azimuth.Value);
            lon_origin = (Radians)new Degrees(par_longitude_of_center.Value);
            lat_origin = (Radians)new Degrees(par_latitude_of_center.Value);
            false_easting = par_false_easting.Value * MetersPerUnit;
            false_northing = par_false_northing.Value * MetersPerUnit;

            SinCos(lat_origin, out sin_p20, out cos_p20);
            double con = 1.0 - E2 * Math.Pow(sin_p20, 2);
            double com = Math.Sqrt(1.0 - E2);
            bl = Math.Sqrt(1.0 + E2 * Math.Pow(cos_p20, 4.0) / ( 1.0 - E2 ));
            al = SemiMajor * bl * scale_factor * com / con;
            if ( Math.Abs(lat_origin) < Epsilon )
            {
                ts = 1.0;
                d = 1.0;
                el = 1.0;
                f = 1.0;
            }
            else
            {
                ts = ComputeSmallT(E, lat_origin, sin_p20);
                con = Math.Sqrt(con);
                d = bl * com / ( cos_p20 * con );
                if ( ( d * d - 1.0 ) > 0.0 )
                {
                    if ( lat_origin >= 0.0 )
                        f = d + Math.Sqrt(d * d - 1.0);
                    else
                        f = d - Math.Sqrt(d * d - 1.0);
                }
                else
                    f = d;
                el = f * Math.Pow(ts, bl);
            }

            g = .5 * ( f - 1.0 / f );
            Double gama = Asin(Math.Sin(azimuth) / d);
            lon_origin = new Radians(lon_origin - Asin(g * Math.Tan(gama)) / bl);

            con = Math.Abs(lat_origin);
            if ( ( con > Epsilon ) && ( Math.Abs(con - HalfPI) > Epsilon ) )
            {
                SinCos(gama, out singam, out cosgam);
                SinCos(azimuth, out sinaz, out cosaz);
                if ( lat_origin >= 0 )
                    u = ( al / bl ) * Math.Atan(Math.Sqrt(d * d - 1.0) / cosaz);
                else
                    u = -( al / bl ) * Math.Atan(Math.Sqrt(d * d - 1.0) / cosaz);
            }
            else
            {
                throw new ArgumentException("Input data error");
            }

            SinCos((Radians)new Degrees(par_rectified_grid_angle.Value), out singrid, out cosgrid);
        }
        /*
        /// <summary>
        /// Converts coordinates in decimal degrees to projected meters.
        /// </summary>
        /// <param name="lonlat">The point in decimal degrees.</param>
        /// <returns>Point in projected meters</returns>
        public override TCoordinate DegreesToMeters(TCoordinate lonlat)
        {
            double lon = (Radians)new Degrees(lonlat[Ordinates.Lon]);
            double lat = (Radians)new Degrees(lonlat[Ordinates.Lat]);

            double us;
            double ul;
            double s;
            double dlon;
            double ts1;

            // Forward equations
            // -----------------
            double sin_phi = Math.Sin(lat);
            dlon = AdjustLongitude(lon - lon_origin);
            double vl = Math.Sin(bl * dlon);
            if ( Math.Abs(Math.Abs(lat) - HalfPI) > Epsilon )
            {
                ts1 = ComputeSmallT(E, lat, sin_phi);
                double q = el / ( Math.Pow(ts1, bl) );
                s = .5 * ( q - 1.0 / q );
                double t = .5 * ( q + 1.0 / q );
                ul = ( s * singam - vl * cosgam ) / t;
                double con = Math.Cos(bl * dlon);
                if ( Math.Abs(con) < .0000001 )
                {
                    us = al * bl * dlon;
                }
                else
                {
                    us = al * Math.Atan(( s * cosgam + vl * singam ) / con) / bl;
                    if ( con < 0 )
                        us = us + PI * al / bl;
                }
            }
            else
            {
                if ( lat >= 0 )
                    ul = singam;
                else
                    ul = -singam;
                us = al * lat / bl;
            }
            if ( Math.Abs(Math.Abs(ul) - 1.0) <= Epsilon )
            {
                throw new ApplicationException("Point projects into infinity");
            }

            double vs = .5 * al * Math.Log(( 1.0 - ul ) / ( 1.0 + ul )) / bl;
            if ( !_naturalOriginOffsets ) us = us - u;
            double x = false_easting + vs * cosgrid + us * singrid;
            double y = false_northing + us * cosgrid - vs * singrid;
            if ( lonlat.ComponentCount < 3 )
                return CoordinateFactory.Create( x * UnitsPerMeter, y * UnitsPerMeter );

            return CoordinateFactory.Create(x * UnitsPerMeter, y * UnitsPerMeter, (Double)lonlat[2]);
        }
        */
        /*
        /// <summary>
        /// Converts coordinates in projected meters to decimal degrees.
        /// </summary>
        /// <param name="p">Point in meters</param>
        /// <returns>Transformed point in decimal degrees</returns>
        public override TCoordinate MetersToDegrees(TCoordinate p)
        {
            // Inverse equations
            // -----------------
            Double x = p[Ordinates.X] * MetersPerUnit - false_easting;
            Double y = p[Ordinates.Y] * MetersPerUnit - false_northing;
            Double vs = x * cosgrid - y * singrid;
            Double us = y * cosgrid + x * singrid;
            if ( !_naturalOriginOffsets ) us = us + u;
            Double q = Math.Exp(-bl * vs / al);
            Double s = .5 * ( q - 1.0 / q );
            Double t = .5 * ( q + 1.0 / q );
            Double vl = Math.Sin(bl * us / al);
            Double ul = ( vl * cosgam + s * singam ) / t;
            if ( Math.Abs(Math.Abs(ul) - 1.0) <= Epsilon )
            {
                if (p.ComponentCount < 3)
                    return CoordinateFactory.Create((Degrees)new Radians(lon_origin),
                                                    (Degrees)new Radians(HalfPI) * Sign(ul));

                return CoordinateFactory.Create((Degrees) new Radians(lon_origin),
                                                (Degrees) new Radians(HalfPI)*Sign(ul), (Double) p[2]);
            }

            Double con = 1.0 / bl;
            Double ts1 = Math.Pow(( el / Math.Sqrt(( 1.0 + ul ) / ( 1.0 - ul )) ), con);
            Double lat = ComputePhi2(E, ts1);
            con = Math.Cos(bl * us / al);
            Double theta = lon_origin - Math.Atan2(( s * cosgam - vl * singam ), con) / bl;
            Double lon = AdjustLongitude(theta);

            if ( p.ComponentCount < 3 )
                return CoordinateFactory.Create((Degrees)new Radians(lon), (Degrees)new Radians(lat));

            return CoordinateFactory.Create((Degrees)new Radians(lon), (Degrees)new Radians(lat), (Double)p[2]);
        }
        */
        public override string ProjectionClassName
        {
            get {
                return _naturalOriginOffsets
                           ?
                               "Hotine_Oblique_Mercator"
                           :
                               "Oblique_Mercator";
            }
        }

        public override string Name
        {
            get { return ProjectionClassName; }
        }

        public override bool IsInverse
        {
            get { return false; }
        }

        public override int SourceDimension
        {
            get { throw new NotImplementedException(); }
        }

        public override int TargetDimension
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns the inverse of this projection.
        /// </summary>
        /// <returns>IMathTransform that is the reverse of the current projection.</returns>
        protected override IMathTransform  ComputeInverse(IMathTransform setAsInverse)
        {
            IEnumerable<ProjectionParameter> parameters = 
                Caster.Downcast<ProjectionParameter, Parameter>(Parameters);
            return new InverseHotineObliqueMercator<TCoordinate>(parameters, CoordinateFactory , _naturalOriginOffsets, this);
        }

        public override TCoordinate Transform(TCoordinate point)
        {
            Radians lon = (Radians)new Degrees(point[Ordinates.Lon]);
            Radians lat = (Radians)new Degrees(point[Ordinates.Lat]);

            Double us, ul;

            // Forward equations
            // -----------------
            double sin_phi = Math.Sin(lat);
            double dlon = AdjustLongitude(lon - lon_origin);
            double vl = Math.Sin(bl * dlon);
            if (Math.Abs(Math.Abs(lat) - HalfPI) > Epsilon)
            {
                double ts1 = ComputeSmallT(E, lat, sin_phi);
                double q = el / (Math.Pow(ts1, bl));
                double s = .5 * (q - 1.0 / q);
                double t = .5 * (q + 1.0 / q);
                ul = (s * singam - vl * cosgam) / t;
                double con = Math.Cos(bl * dlon);
                if (Math.Abs(con) < .0000001)
                {
                    us = al * bl * dlon;
                }
                else
                {
                    us = al * Math.Atan((s * cosgam + vl * singam) / con) / bl;
                    if (con < 0)
                        us = us + PI * al / bl;
                }
            }
            else
            {
                if (lat >= 0)
                    ul = singam;
                else
                    ul = -singam;
                us = al * lat / bl;
            }
            if (Math.Abs(Math.Abs(ul) - 1.0) <= Epsilon)
            {
                throw new ApplicationException("Point projects into infinity");
            }

            double vs = .5 * al * Math.Log((1.0 - ul) / (1.0 + ul)) / bl;
            if (!_naturalOriginOffsets) us = us - u;
            double x = false_easting + vs * cosgrid + us * singrid;
            double y = false_northing + us * cosgrid - vs * singrid;

            return CreateCoordinate(x * UnitsPerMeter, y * UnitsPerMeter, point);
        }
    }
}
