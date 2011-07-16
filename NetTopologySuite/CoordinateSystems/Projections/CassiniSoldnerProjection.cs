// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Copyright 2008
//
// This file is part of ProjNet.
// ProjNet is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// ProjNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with ProjNet; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

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
using GeoAPI.Units;
using GeoAPI.DataStructures;
using NPack.Interfaces;

namespace NetTopologySuite.CoordinateSystems.Projections
{

    internal class InverseCassiniSoldnerProjection<TCoordinate> : CassiniSoldnerProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {

        private const Double one3rd = 0.33333333333333333333d;      //C4
        private const Double one15th = 0.06666666666666666666d;     //C5

        public InverseCassiniSoldnerProjection(IEnumerable<ProjectionParameter> parameters,
                               ICoordinateFactory<TCoordinate> coordinateFactory,
                               CassiniSoldnerProjection<TCoordinate> transform)
            : base(parameters, coordinateFactory)
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
            Double rsmajor = ReciprocalSemiMajor;
            Double x = ((point[Ordinates.X] * MetersPerUnit) - _falseEasting) * rsmajor;
            Double y = ((point[Ordinates.Y] * MetersPerUnit) - _falseNorthing) * rsmajor;
            Radians phi1 = DamTool.Phi1(_m0 + y);

            Double tn = Math.Tan(phi1);
            Double t = tn * tn;
            Double n = Math.Sin(phi1);
            Double r = 1.0d / (1.0d - E2 * n * n);
            n = Math.Sqrt(r);
            r *= (1.0d - E2) * n;
            Double dd = x / n;
            Double d2 = dd * dd;

            Radians phi = new Radians(phi1 - (n * tn / r) * d2 * (.5 - (1.0 + 3.0 * t) * d2 * one24th));
            Radians lambda = new Radians(dd * (1.0 + t * d2 * (-one3rd + (1.0 + 3.0 * t) * d2 * one15th)) / Math.Cos(phi1));
            lambda = new Radians(AdjustLongitude(lambda + _centralMeridian));

            return CreateCoordinate((Degrees)lambda, (Degrees)phi, point);
        }
    }

    /// <summary>
    /// Implements the Cassini-Soldner Projection.
    /// </summary>
    /// <remarks>
    /// <para>The Cassini-Soldner projection is the ellipsoidal version of the
    /// Cassini projection for the sphere. It is not conformal but as it is
    /// relatively simple to construct it was extensively used in the last century
    /// and is still useful for mapping areas with limited longitudinal extent.
    /// It has now largely been replaced by the conformal Transverse Mercator
    /// which it resembles. Like this, it has a straight central meridian along
    /// which the scale is true, all other meridians and parallels are curved, and
    /// the scale distortion increases rapidly with increasing distance from the
    /// central meridian.</para>
    /// </remarks>
    internal class CassiniSoldnerProjection<TCoordinate> : MapProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {

        private const Double one6th = 0.16666666666666666666d;      //C1
        private const Double one120th = 0.00833333333333333333d;    //C2
        protected const Double one24th = 0.04166666666666666666d;     //C3

        /**
         * Values of necessary ProjectionParameters.
         */
        protected readonly Double _falseEasting;
        protected readonly Double _falseNorthing;
        protected Radians _centralMeridian;
        protected Radians _latitudeOfOrigin;

        /**
         * Useful variables calculated from parameters defined by user.
         */
        private readonly Double _cFactor;
        protected readonly Double _m0;

        #region Constructors

        /// <summary>
        /// Creates an instance of an Cassini-Soldner projection object.
        /// </summary>
        /// <remarks>
        /// <para>The parameters this projection expects are listed below.</para>
        /// <list type="table">
        /// <listheader><term>Parameter</term><description>Description</description></listheader>
        /// <item><term>latitude_of_origin</term><description>The latitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
        /// <item><term>central_meridian</term><description>The longitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
        /// <item><term>false_easting</term><description>The easting value assigned to the false origin.</description></item>
        /// <item><term>false_northing</term><description>The northing value assigned to the false origin.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="parameters">List of parameters to initialize the projection.</param>
        /// <param name="coordinateFactory">Factory to create projected coordinates</param>
        public CassiniSoldnerProjection(IEnumerable<ProjectionParameter> parameters, ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {
            Authority = "EPSG";
            AuthorityCode = "9806";

            /* PROJCS["Kertau / Singapore Grid",
             *      GEOGCS["Kertau",
             *          DATUM["Kertau",
             *              SPHEROID["Everest 1830 Modified",
             *                       6377304.063,
             *                       300.8017,
             *                       AUTHORITY["EPSG","7018"]],
             *              TOWGS84[-11,851,5,0,0,0,0],
             *              AUTHORITY["EPSG","6245"]],
             *          PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]],
             *          UNIT["degree",0.01745329251994328,AUTHORITY["EPSG","9122"]],
             *          AUTHORITY["EPSG","4245"]],
             *      PROJECTION["Cassini_Soldner"],
             *      PARAMETER["latitude_of_origin",1.287646666666667],
             *      PARAMETER["central_meridian",103.8530022222222],
             *      PARAMETER["false_easting",30000],
             *      PARAMETER["false_northing",30000],
             *      UNIT["metre",1,AUTHORITY["EPSG","9001"]],
             *      AUTHORITY["EPSG","24500"]]
             */

            ProjectionParameter parLatitudeOfOrigin = GetParameter("latitude_of_origin");
            ProjectionParameter parCentralMeridian = GetParameter("central_meridian");
            ProjectionParameter parFalseEasting = GetParameter("false_easting");
            ProjectionParameter parFalseNorthing = GetParameter("false_northing");

            //Check for missing parameters
            if (parLatitudeOfOrigin == null)
                throw new ArgumentException("Missing projection parameter 'latitude_of_center'");
            if (parCentralMeridian == null)
                throw new ArgumentException("Missing projection parameter 'central_meridian'");
            if (parFalseEasting == null)
                throw new ArgumentException("Missing projection parameter 'false_easting'");
            if (parFalseNorthing == null)
                throw new ArgumentException("Missing projection parameter 'false_northing'");

            _latitudeOfOrigin = (Radians)new Degrees((Double)parLatitudeOfOrigin.Value);
            _centralMeridian = (Radians)new Degrees((Double)parCentralMeridian.Value);// par_longitude_of_center.Value);
            _falseEasting = parFalseEasting.Value * MetersPerUnit;
            _falseNorthing = parFalseNorthing.Value * MetersPerUnit;

            _cFactor = E2/(1 - E2);
            _m0 = MeridianLength(_latitudeOfOrigin);

        }
        #endregion

        public override string ProjectionClassName
        {
            get { return "Cassini_Soldner"; }
        }

        public override string Name
        {
            get { return ProjectionClassName; }
        }
        /*
        /// <summary>
        /// Converts coordinates in decimal degrees to projected meters.
        /// </summary>
        /// <param name="lonlat">The point in decimal degrees.</param>
        /// <returns>Point in projected meters</returns>
        public override TCoordinate DegreesToMeters(TCoordinate lonlat)
        {

            Radians lambda = (Radians)new Degrees(lonlat[Ordinates.Lon]) - _centralMeridian;
            Radians phi = (Radians)new Degrees(lonlat[Ordinates.Lat]);

            Double sinPhi, cosPhi; // sin and cos value
            SinCos(phi, out sinPhi, out cosPhi);

            Double y = DamTool.Length(phi, sinPhi, cosPhi);
            Double n = 1.0d / Math.Sqrt(1 - E2 * sinPhi * sinPhi);
            Double tn = Math.Tan(phi);
            Double t = tn*tn;
            Double a1 = lambda * cosPhi;
            Double a2 = a1*a1;
            Double c = _cFactor * Math.Pow(cosPhi,2.0d);

            Double x = n * a1 * (1.0d - a2 * t * (one6th - (8.0d - t + 8.0d * c) * a2 * one120th));
            y -= _m0  - n * tn * a2 * (0.5d + (5.0d - t + 6.0d * c) * a2 * one24th);

            Double semiMajor = SemiMajor;
            x = UnitsPerMeter*(semiMajor*x + _falseEasting);
            y = UnitsPerMeter*(semiMajor*y + _falseNorthing);

            return CreateCoordinate(x, y, lonlat);
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

            Double rsmajor = ReciprocalSemiMajor;
            Double x = ((p[Ordinates.X] * MetersPerUnit) - _falseEasting) * rsmajor;
            Double y = ((p[Ordinates.Y] * MetersPerUnit) - _falseNorthing) * rsmajor;
            Radians phi1 = DamTool.Phi1(_m0+y);

            Double tn = Math.Tan(phi1);
            Double t = tn*tn;
            Double n = Math.Sin(phi1);
            Double r = 1.0d/(1.0d-E2*n*n);
            n = Math.Sqrt(r);
            r *= (1.0d - E2) * n;
            Double dd = x/n;
            Double d2 = dd*dd;

            Radians phi = new Radians(phi1 - (n * tn / r) * d2 * (.5 - (1.0 + 3.0 * t) * d2 * one24th));
            Radians lambda = new Radians(dd * (1.0 + t * d2 * (-one3rd + (1.0 + 3.0 * t) * d2 * one15th)) / Math.Cos(phi1));
            lambda = new Radians(AdjustLongitude(lambda + _centralMeridian));

            return CreateCoordinate((Degrees)lambda, (Degrees)phi, p);
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

            return new InverseCassiniSoldnerProjection<TCoordinate>(parameters, CoordinateFactory, this);
        }
        public override TCoordinate Transform(TCoordinate point)
        {
            Radians lambda = (Radians)new Degrees(point[Ordinates.Lon]) - _centralMeridian;
            Radians phi = (Radians)new Degrees(point[Ordinates.Lat]);

            Double sinPhi, cosPhi; // sin and cos value
            SinCos(phi, out sinPhi, out cosPhi);

            Double y = DamTool.Length(phi, sinPhi, cosPhi);
            Double n = 1.0d / Math.Sqrt(1 - E2 * sinPhi * sinPhi);
            Double tn = Math.Tan(phi);
            Double t = tn * tn;
            Double a1 = lambda * cosPhi;
            Double a2 = a1 * a1;
            Double c = _cFactor * Math.Pow(cosPhi, 2.0d);

            Double x = n * a1 * (1.0d - a2 * t * (one6th - (8.0d - t + 8.0d * c) * a2 * one120th));
            y -= _m0 - n * tn * a2 * (0.5d + (5.0d - t + 6.0d * c) * a2 * one24th);

            Double semiMajor = SemiMajor;
            x = UnitsPerMeter * (semiMajor * x + _falseEasting);
            y = UnitsPerMeter * (semiMajor * y + _falseNorthing);

            return CreateCoordinate(x, y, point);
        }
    }
}
