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
using GeoAPI.Units;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Projections
{

    internal class InverseMercator<TCoordinate> : Mercator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public InverseMercator(IEnumerable<ProjectionParameter> parameters,
                               ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {
        }

        public override string Name
        {
            get
            {
                return "Inverse_" + base.Name;
            }
        }

        public override Boolean IsInverse
        {
            get { return true; }
        }

        public override TCoordinate Transform(TCoordinate point)
        {
            Double semiMajor = SemiMajor;

            // Inverse equations
            Double dX = point[Ordinates.X] * MetersPerUnit - _falseEasting;
            Double dY = point[Ordinates.Y] * MetersPerUnit - _falseNorthing;
            Double smallT = Math.Exp(-dY / (semiMajor * _k0)); // t

            Double chi = HalfPI - 2 * Math.Atan(smallT);
            Double e2 = E2;
            Double e4 = e2 * e2;
            Double e6 = e4 * e2;
            Double e8 = e4 * e4;

            Radians lat = (Radians)(chi + (e2 * 0.5 + 5 * e4 / 24 + e6 / 12 + 13 * e8 / 360) * Math.Sin(2 * chi) +
                                    (7 * e4 / 48 + 29 * e6 / 240 + 811 * e8 / 11520) * Math.Sin(4 * chi) +
                                    (7 * e6 / 120 + 81 * e8 / 1120) * Math.Sin(6 * chi) +
                                    (4279 * e8 / 161280) * Math.Sin(8 * chi));

            Radians lon = (Radians)(dX / (semiMajor * _k0) + _lon_center);

            return CreateCoordinate((Degrees) lon, (Degrees) lat, point);
        }
    }

    /// <summary>
    /// Implements the Mercator projection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This map projection introduced in 1569 by Gerardus Mercator. 
    /// It is often described as a cylindrical projection, but it must be derived
    /// mathematically. The meridians are equally spaced, parallel vertical lines, 
    /// and the parallels of latitude are parallel, horizontal straight lines, 
    /// spaced farther and farther apart as their distance from the Equator 
    /// increases. This projection is widely used for navigation charts, because 
    /// any straight line on a Mercator-projection map is a line of constant true 
    /// bearing that enables a navigator to plot a straight-line course. 
    /// It is less practical for world maps because the scale is distorted; 
    /// areas farther away from the equator appear disproportionately large. 
    /// On a Mercator projection, for example, the landmass of Greenland appears 
    /// to be greater than that of the continent of South America; in actual area, 
    /// Greenland is smaller than the Arabian Peninsula.
    /// </para>
    /// </remarks>
    internal class Mercator<TCoordinate> : MapProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly String _name;   
        protected readonly Double _falseEasting;
        protected readonly Double _falseNorthing;
        protected readonly Radians _lon_center; //Center longitude (projection center)
        private readonly Radians _lat_origin; //center latitude
        protected readonly Double _k0; //small value m

        /// <summary>
        /// Initializes a <see cref="Mercator{TCoordinate}"/> projection 
        /// with the specified parameters. 
        /// </summary>
        /// <param name="parameters">Parameters of the projection.</param>
        /// <param name="coordinateFactory">Coordinate factory to use.</param>
        /// <remarks>
        ///   <para>The parameters this projection expects are listed below.</para>
        ///   <list type="table">
        ///     <listheader>
        ///      <term>Parameter</term>
        ///      <description>Description</description>
        ///    </listheader>
        ///     <item>
        ///      <term>central_meridian</term>
        ///      <description>
        ///         The longitude of the point from which the values of both the 
        ///         geographical coordinates on the ellipsoid and the grid coordinates 
        ///         on the projection are deemed to increment or decrement for computational purposes. 
        ///         Alternatively it may be considered as the longitude of the point which in the 
        ///         absence of application of false coordinates has grid coordinates of (0, 0).
        ///       </description>
        ///    </item>
        ///     <item>
        ///      <term>latitude_of_origin</term>
        ///      <description>
        ///         The latitude of the point from which the values of both the 
        ///         geographical coordinates on the ellipsoid and the grid coordinates 
        ///         on the projection are deemed to increment or decrement for computational purposes. 
        ///         Alternatively it may be considered as the latitude of the point which in the 
        ///         absence of application of false coordinates has grid coordinates of (0, 0). 
        ///      </description>
        ///    </item>
        ///     <item>
        ///      <term>scale_factor</term>
        ///      <description>
        ///         The factor by which the map grid is reduced or enlarged during the projection process, 
        ///         defined by its value at the natural origin.
        ///      </description>
        ///    </item>
        ///     <item>
        ///      <term>false_easting</term>
        ///      <description>
        ///         Since the natural origin may be at or near the center of the projection and under 
        ///         normal coordinate circumstances would thus give rise to negative coordinates over 
        ///         parts of the mapped area, this origin is usually given false coordinates which are 
        ///         large enough to avoid this inconvenience. The False Easting, FE, is the easting 
        ///         value assigned to the abscissa (east).
        ///      </description>
        ///    </item>
        ///     <item>
        ///      <term>false_northing</term>
        ///      <description>
        ///         Since the natural origin may be at or near the center of the projection and under 
        ///         normal coordinate circumstances would thus give rise to negative coordinates over 
        ///         parts of the mapped area, this origin is usually given false coordinates which are 
        ///         large enough to avoid this inconvenience. The False Northing, FN, is the northing 
        ///         value assigned to the ordinate.
        ///      </description>
        ///    </item>
        ///  </list>
        /// </remarks>
        public Mercator(IEnumerable<ProjectionParameter> parameters,
                        ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {
            ProjectionParameter central_meridian = GetParameter("central_meridian");
            ProjectionParameter latitude_of_origin = GetParameter("latitude_of_origin");
            ProjectionParameter scale_factor = GetParameter("scale_factor");
            ProjectionParameter false_easting = GetParameter("false_easting");
            ProjectionParameter false_northing = GetParameter("false_northing");

            //Check for missing parameters
            if (central_meridian == null)
            {
                throw new ArgumentException("Missing projection parameter 'central_meridian'");
            }

            if (latitude_of_origin == null)
            {
                throw new ArgumentException("Missing projection parameter 'latitude_of_origin'");
            }

            if (false_easting == null)
            {
                throw new ArgumentException("Missing projection parameter 'false_easting'");
            }

            if (false_northing == null)
            {
                throw new ArgumentException("Missing projection parameter 'false_northing'");
            }

            _lon_center = (Radians)new Degrees(central_meridian.Value);
            _lat_origin = (Radians)new Degrees(latitude_of_origin.Value);
            _falseEasting = false_easting.Value * MetersPerUnit;
            _falseNorthing = false_northing.Value * MetersPerUnit;

            if (scale_factor == null) // This is a two standard parallel Mercator projection (2SP)
            {
                _k0 = Math.Cos(_lat_origin) /
                      Math.Sqrt(1.0 - E2 *
                                Math.Sin(_lat_origin) *
                                Math.Sin(_lat_origin));
                AuthorityCode = "9805";
                _name = "Mercator_2SP";
            }
            else // This is a one standard parallel Mercator projection (1SP)
            {
                _k0 = scale_factor.Value;
                _name = "Mercator_1SP";
            }

            Authority = "EPSG";
        }
        /*
        /// <summary>
        /// Converts coordinates in decimal degrees to projected meters.
        /// </summary>
        /// <remarks>
        /// <para>The parameters this projection expects are listed below.</para>
        /// <list type="table">
        /// <listheader><term>Items</term><description>Descriptions</description></listheader>
        /// <item><term>longitude_of_natural_origin</term><description>The longitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the longitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).  Sometimes known as ""central meridian""."</description></item>
        /// <item><term>latitude_of_natural_origin</term><description>The latitude of the point from which the values of both the geographical coordinates on the ellipsoid and the grid coordinates on the projection are deemed to increment or decrement for computational purposes. Alternatively it may be considered as the latitude of the point which in the absence of application of false coordinates has grid coordinates of (0,0).</description></item>
        /// <item><term>scale_factor_at_natural_origin</term><description>The factor by which the map grid is reduced or enlarged during the projection process, defined by its value at the natural origin.</description></item>
        /// <item><term>false_easting</term><description>Since the natural origin may be at or near the center of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Easting, FE, is the easting value assigned to the abscissa (east).</description></item>
        /// <item><term>false_northing</term><description>Since the natural origin may be at or near the center of the projection and under normal coordinate circumstances would thus give rise to negative coordinates over parts of the mapped area, this origin is usually given false coordinates which are large enough to avoid this inconvenience. The False Northing, FN, is the northing value assigned to the ordinate .</description></item>
        /// </list>
        /// </remarks>
        /// <param name="lonlat">The point in decimal degrees.</param>
        /// <returns>Point in projected meters</returns>
        public override TCoordinate DegreesToMeters(TCoordinate lonlat)
        {
            if (Double.IsNaN((Double)lonlat[0]) || Double.IsNaN((Double)lonlat[1]))
            {
                return CreateCoordinate(Double.NaN, Double.NaN);
            }

            Radians lon = (Radians)(Degrees)(Double)lonlat[0];
            Radians lat = (Radians)(Degrees)(Double)lonlat[1];

            // Forward equations
            if (Math.Abs(Math.Abs(lat) - HalfPI) <= Epsilon)
            {
                throw new ComputationException("Transformation cannot be computed at the poles.");
            }

            Double esinphi = E * Math.Sin(lat);
            Double semiMajor = SemiMajor;

            Double x = _falseEasting + semiMajor * _k0 * (lon - _lon_center);
            Double y = _falseNorthing +
                       semiMajor * _k0 *
                       Math.Log(Math.Tan(PI * 0.25 + lat * 0.5) *
                                Math.Pow((1 - esinphi) / (1 + esinphi), E * 0.5));

            return lonlat.ComponentCount < 3
                       ? CreateCoordinate(x / MetersPerUnit, y / MetersPerUnit)
                       : CreateCoordinate(x / MetersPerUnit, y / MetersPerUnit, (Double)lonlat[2]);
        }
        */
        public override string ProjectionClassName
        {
            get { return _name; }
        }

        public override string Name
        {
            get { return _name; }
        }
        /*
        /// <summary>
        /// Converts coordinates in projected meters to decimal degrees.
        /// </summary>
        /// <param name="p">Point in meters</param>
        /// <returns>Transformed point in decimal degrees</returns>
        public override TCoordinate MetersToDegrees(TCoordinate p)
        {
            Double semiMajor = SemiMajor;

            // Inverse equations
            Double dX = p[Ordinates.X] * MetersPerUnit - _falseEasting;
            Double dY = p[Ordinates.Y] * MetersPerUnit - _falseNorthing;
            Double smallT = Math.Exp(-dY / (semiMajor * _k0)); // t

            Double chi = HalfPI - 2 * Math.Atan(smallT);
            Double e2 = E2;
            Double e4 = e2 * e2;
            Double e6 = e4 * e2;
            Double e8 = e4 * e4;

            Radians lat = (Radians)(chi + (e2 * 0.5 + 5 * e4 / 24 + e6 / 12 + 13 * e8 / 360) * Math.Sin(2 * chi) +
                                    (7 * e4 / 48 + 29 * e6 / 240 + 811 * e8 / 11520) * Math.Sin(4 * chi) +
                                    (7 * e6 / 120 + 81 * e8 / 1120) * Math.Sin(6 * chi) +
                                    (4279 * e8 / 161280) * Math.Sin(8 * chi));

            Radians lon = (Radians)(dX / (semiMajor * _k0) + _lon_center);

            return p.ComponentCount < 3
                       ? CreateCoordinate((Degrees)lon, (Degrees)lat)
                       : CreateCoordinate((Degrees)lon, (Degrees)lat, (Double)p[2]);
        }
        */
        public override Int32 SourceDimension
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Int32 TargetDimension
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Boolean IsInverse
        {
            get { return false; }
        }

        public override IEnumerable<ICoordinate> Transform(IEnumerable<ICoordinate> points)
        {
            throw new System.NotImplementedException();
        }

        public override ICoordinateSequence Transform(ICoordinateSequence points)
        {
            throw new System.NotImplementedException();
        }

        protected override IMathTransform ComputeInverse(IMathTransform setAsInverse)
        {
            IEnumerable<ProjectionParameter> parameters =
                Caster.Downcast<ProjectionParameter, Parameter>(Parameters);

            return new InverseMercator<TCoordinate>(parameters, CoordinateFactory);
        }

        public override TCoordinate Transform(TCoordinate lonlat)
        {
            if (Double.IsNaN((Double)lonlat[0]) || Double.IsNaN((Double)lonlat[1]))
            {
                return CreateCoordinate(Double.NaN, Double.NaN, lonlat);
            }

            Radians lon = (Radians)(Degrees)(Double)lonlat[0];
            Radians lat = (Radians)(Degrees)(Double)lonlat[1];

            // Forward equations
            if (Math.Abs(Math.Abs(lat) - HalfPI) <= Epsilon)
            {
                throw new ComputationException("Transformation cannot be computed at the poles.");
            }

            Double esinphi = E * Math.Sin(lat);
            Double semiMajor = SemiMajor;

            Double x = _falseEasting + semiMajor * _k0 * (lon - _lon_center);
            Double y = _falseNorthing +
                       semiMajor * _k0 *
                       Math.Log(Math.Tan(PI * 0.25 + lat * 0.5) *
                                Math.Pow((1 - esinphi) / (1 + esinphi), E * 0.5));

            return CreateCoordinate(x*UnitsPerMeter, y*UnitsPerMeter, lonlat);
        }
    }
}