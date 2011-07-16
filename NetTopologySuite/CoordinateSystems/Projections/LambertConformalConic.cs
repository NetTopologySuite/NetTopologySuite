// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Proj.Net.
// Proj.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Proj.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Proj.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// SOURCECODE IS MODIFIED FROM ANOTHER WORK AND IS ORIGINALLY BASED ON GeoTools.NET:
/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. 
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.DataStructures;
using NPack.Interfaces;
using GeoAPI.Units;

namespace NetTopologySuite.CoordinateSystems.Projections
{
    internal class InverseLambertConformalConic2SP<TCoordinate> : LambertConformalConic2SP<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public InverseLambertConformalConic2SP(IEnumerable<ProjectionParameter> parameters,
                                               ICoordinateFactory<TCoordinate> coordinateFactory,
                                               LambertConformalConic2SP<TCoordinate> transform)
            : base(parameters, coordinateFactory)
        {
            Inverse = transform;
        }

        public override Boolean IsInverse
        {
            get { return true; }
        }

        public override TCoordinate Transform(TCoordinate point)
        {
            Radians lat;

            Double rh1; // height above ellipsoid
            Double con; // sign variable

            Double dX = ((Double)point[0]) * MetersPerUnit - _falseEasting;
            Double dY = _rh - ((Double)point[1]) * MetersPerUnit + _falseNorthing;

            if (_ns > 0)
            {
                rh1 = Math.Sqrt(dX * dX + dY * dY);
                con = 1.0;
            }
            else
            {
                rh1 = -Math.Sqrt(dX * dX + dY * dY);
                con = -1.0;
            }

            Double theta = 0.0;

            if (rh1 != 0)
            {
                theta = Math.Atan2((con * dX), (con * dY));
            }

            if ((rh1 != 0) || (_ns > 0.0))
            {
                con = 1.0 / _ns;
                Double smallT = Math.Pow((rh1 / (SemiMajor * _f0)), con);
                lat = (Radians)ComputePhi2(E, smallT);
            }
            else
            {
                lat = new Radians(-HalfPI);
            }

            Radians lon = new Radians(AdjustLongitude(theta / _ns + _centerLon));

            return CreateCoordinate((Degrees) lon, (Degrees) lat, point);
        }
    }

    /// <summary>
    /// Implemetns the Lambert Conformal Conic 2SP Projection.
    /// </summary>
    /// <remarks>
    /// <para>The Lambert Conformal Conic projection is a standard projection for presenting maps
    /// of land areas whose East-West extent is large compared with their North-South extent.
    /// This projection is "conformal" in the sense that lines of latitude and longitude, 
    /// which are perpendicular to one another on the earth's surface, are also perpendicular
    /// to one another in the projected domain.</para>
    /// </remarks>
    internal class LambertConformalConic2SP<TCoordinate> : MapProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        protected readonly Double _falseEasting;
        protected readonly Double _falseNorthing;
        protected readonly Radians _centerLon; /* center longituted            */
        private readonly Radians _centerLat; /* cetner latitude              */
        protected readonly Double _ns; /* ratio of angle between meridian*/
        protected readonly Double _f0; /* flattening of ellipsoid      */
        protected readonly Double _rh; /* height above ellipsoid       */

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="LambertConformalConic2SP{TCoordinate}"/> projection 
        /// with the specified parameters. 
        /// </summary>
        /// <param name="parameters">Parameters of the projection.</param>
        /// <param name="coordinateFactory">Coordinate factory to use.</param>
        /// <remarks>
        /// <para>The parameters this projection expects are listed below.</para>
        /// <list type="table">
        /// <listheader><term>Parameter</term><description>Description</description></listheader>
        /// <item><term>latitude_of_origin</term><description>The latitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
        /// <item><term>central_meridian</term><description>The longitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
        /// <item><term>standard_parallel_1</term><description>For a conic projection with two standard parallels, this is the latitude of intersection of the cone with the ellipsoid that is nearest the pole.  Scale is true along this parallel.</description></item>
        /// <item><term>standard_parallel_2</term><description>For a conic projection with two standard parallels, this is the latitude of intersection of the cone with the ellipsoid that is furthest from the pole.  Scale is true along this parallel.</description></item>
        /// <item><term>false_easting</term><description>The easting value assigned to the false origin.</description></item>
        /// <item><term>false_northing</term><description>The northing value assigned to the false origin.</description></item>
        /// </list>
        /// </remarks>
        public LambertConformalConic2SP(IEnumerable<ProjectionParameter> parameters,
                                        ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {
            Authority = "EPSG";
            AuthorityCode = "9802";

            ProjectionParameter latitudeOfOrigin = GetParameter("latitude_of_origin");
            ProjectionParameter centralMeridian = GetParameter("central_meridian");
            ProjectionParameter standardParallel1 = GetParameter("standard_parallel_1");
            ProjectionParameter standardParallel2 = GetParameter("standard_parallel_2");
            ProjectionParameter falseEasting = GetParameter("false_easting");
            ProjectionParameter falseNorthing = GetParameter("false_northing");

            //Check for missing parameters
            if (latitudeOfOrigin == null)
            {
                throw new ArgumentException("Missing projection parameter 'latitude_of_origin'");
            }

            if (centralMeridian == null)
            {
                throw new ArgumentException("Missing projection parameter 'central_meridian'");
            }

            if (standardParallel1 == null)
            {
                throw new ArgumentException("Missing projection parameter 'standard_parallel_1'");
            }

            if (standardParallel2 == null)
            {
                throw new ArgumentException("Missing projection parameter 'standard_parallel_2'");
            }

            if (falseEasting == null)
            {
                throw new ArgumentException("Missing projection parameter 'false_easting'");
            }

            if (falseNorthing == null)
            {
                throw new ArgumentException("Missing projection parameter 'false_northing'");
            }

            Radians centralLat = (Radians)new Degrees(latitudeOfOrigin.Value);
            Radians centralLon = (Radians)new Degrees(centralMeridian.Value);
            Radians lat1 = (Radians)new Degrees(standardParallel1.Value);
            Radians lat2 = (Radians)new Degrees(standardParallel2.Value);
            _falseEasting = falseEasting.Value * MetersPerUnit;
            _falseNorthing = falseNorthing.Value * MetersPerUnit;

            Double sinPo; /* sin value                            */
            Double cosPo; /* cos value                            */
            Double con; /* temporary variable                   */
            Double ms1; /* small m 1                            */
            Double ms2; /* small m 2                            */
            Double ts0; /* small t 0                            */
            Double ts1; /* small t 1                            */
            Double ts2; /* small t 2                            */


            /* Standard Parallels cannot be equal and on opposite sides of the equator
            ------------------------------------------------------------------------*/
            if (Math.Abs(lat1 + lat2) < Epsilon)
            {
                throw new ArgumentException("Latitudes for standard parallels cannot be equal " +
                                            "and on opposite sides of equator.");
            }

            _centerLon = centralLon;
            _centerLat = centralLat;
            SinCos(lat1, out sinPo, out cosPo);
            con = sinPo;
            ms1 = ComputeSmallM(E, sinPo, cosPo);
            ts1 = ComputeSmallT(E, lat1, sinPo);
            SinCos(lat2, out sinPo, out cosPo);
            ms2 = ComputeSmallM(E, sinPo, cosPo);
            ts2 = ComputeSmallT(E, lat2, sinPo);
            sinPo = Math.Sin(_centerLat);
            ts0 = ComputeSmallT(E, _centerLat, sinPo);

            if (Math.Abs(lat1 - lat2) > Epsilon)
            {
                _ns = Math.Log(ms1 / ms2) / Math.Log(ts1 / ts2);
            }
            else
            {
                _ns = con;
            }

            _f0 = ms1 / (_ns * Math.Pow(ts1, _ns));
            _rh = SemiMajor * _f0 * Math.Pow(ts0, _ns);
        }

        #endregion
        /*
        /// <summary>
        /// Converts coordinates in decimal degrees to projected meters.
        /// </summary>
        /// <param name="lonlat">The point in decimal degrees.</param>
        /// <returns>Point in projected meters</returns>
        public override TCoordinate DegreesToMeters(TCoordinate lonlat)
        {
            Radians lon = (Radians)new Degrees((Double)lonlat[0]);
            Radians lat = (Radians)new Degrees((Double)lonlat[1]);

            Double rh1; // height above ellipsoid

            Double con = Math.Abs(Math.Abs(lat) - HalfPI);

            if (con > Epsilon)
            {
                Double sinPhi = Math.Sin(lat);
                Double smallT = ComputeSmallT(E, lat, sinPhi);
                rh1 = SemiMajor * _f0 * Math.Pow(smallT, _ns);
            }
            else
            {
                con = lat * _ns;

                if (con <= 0)
                {
                    throw new ArgumentOutOfRangeException("lonlat",
                                                          lonlat,
                                                          "Latitude must be less than 90°.");
                }

                rh1 = 0;
            }

            Double theta = _ns * AdjustLongitude(lon - _centerLon);
            lon = (Radians)(rh1 * Math.Sin(theta) + _falseEasting);
            lat = (Radians)(_rh - rh1 * Math.Cos(theta) + _falseNorthing);

            return lonlat.ComponentCount == 2
                       ? CreateCoordinate(lon / MetersPerUnit, lat / MetersPerUnit)
                       : CreateCoordinate(lon / MetersPerUnit, lat / MetersPerUnit, (Double)lonlat[2]);
        }
        */
        public override string ProjectionClassName
        {
            get { return "Lambert_Conformal_Conic_2SP"; }
        }

        public override string Name
        {
            get { return "Lambert_Conformal_Conic_2SP"; }
        }
        /*
        /// <summary>
        /// Converts coordinates in projected meters to decimal degrees.
        /// </summary>
        /// <param name="p">Point in meters</param>
        /// <returns>Transformed point in decimal degrees</returns>
        public override TCoordinate MetersToDegrees(TCoordinate p)
        {
            Radians lat;

            Double rh1; // height above ellipsoid
            Double con; // sign variable

            Double dX = ((Double)p[0]) * MetersPerUnit - _falseEasting;
            Double dY = _rh - ((Double)p[1]) * MetersPerUnit + _falseNorthing;

            if (_ns > 0)
            {
                rh1 = Math.Sqrt(dX * dX + dY * dY);
                con = 1.0;
            }
            else
            {
                rh1 = -Math.Sqrt(dX * dX + dY * dY);
                con = -1.0;
            }

            Double theta = 0.0;

            if (rh1 != 0)
            {
                theta = Math.Atan2((con * dX), (con * dY));
            }

            if ((rh1 != 0) || (_ns > 0.0))
            {
                con = 1.0 / _ns;
                Double smallT = Math.Pow((rh1 / (SemiMajor * _f0)), con);
                lat = (Radians)ComputePhi2(E, smallT);
            }
            else
            {
                lat = new Radians(-HalfPI);
            }

            Radians lon = new Radians(AdjustLongitude(theta / _ns + _centerLon));

            return p.ComponentCount == 2
                       ? CreateCoordinate((Degrees)lon, (Degrees)lat)
                       : CreateCoordinate((Degrees)lon, (Degrees)lat, (Double)p[2]);
        }
        */
        public override Int32 SourceDimension
        {
            get { throw new NotImplementedException(); }
        }

        public override Int32 TargetDimension
        {
            get { throw new NotImplementedException(); }
        }

        public override Boolean IsInverse
        {
            get { return false; }
        }

        protected override IMathTransform ComputeInverse(IMathTransform setAsInverse)
        {
            IEnumerable<ProjectionParameter> parameters =
                Caster.Downcast<ProjectionParameter, Parameter>(Parameters);

            return new InverseLambertConformalConic2SP<TCoordinate>(parameters,
                                                                    CoordinateFactory,
                                                                    this);
        }

        public override TCoordinate Transform(TCoordinate lonlat)
        {
            Radians lon = (Radians)new Degrees((Double)lonlat[0]);
            Radians lat = (Radians)new Degrees((Double)lonlat[1]);

            Double rh1; // height above ellipsoid

            Double con = Math.Abs(Math.Abs(lat) - HalfPI);

            if (con > Epsilon)
            {
                Double sinPhi = Math.Sin(lat);
                Double smallT = ComputeSmallT(E, lat, sinPhi);
                rh1 = SemiMajor * _f0 * Math.Pow(smallT, _ns);
            }
            else
            {
                con = lat * _ns;

                if (con <= 0)
                {
                    throw new ArgumentOutOfRangeException("lonlat",
                                                          lonlat,
                                                          "Latitude must be less than 90?.");
                }

                rh1 = 0;
            }

            Double theta = _ns * AdjustLongitude(lon - _centerLon);
            lon = (Radians)(rh1 * Math.Sin(theta) + _falseEasting);
            lat = (Radians)(_rh - rh1 * Math.Cos(theta) + _falseNorthing);

            return CreateCoordinate(UnitsPerMeter * lon, UnitsPerMeter * lat, lonlat);
        }
    }
}