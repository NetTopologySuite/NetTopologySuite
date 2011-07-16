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

    internal class InverseKrovakProjection<TCoordinate> : KrovakProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public InverseKrovakProjection(IEnumerable<ProjectionParameter> parameters,
                               ICoordinateFactory<TCoordinate> coordinateFactory,
                               KrovakProjection<TCoordinate> transform )
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
            Double x = (Double)point[0].Divide(SemiMajor);
            Double y = (Double)point[1].Divide(SemiMajor);

            // x -> southing, y -> westing
            Double ro = Math.Sqrt(x * x + y * y);
            Double eps = Math.Atan2(-x, -y);
            Double d = eps / _n;
            Double s = 2 * (Math.Atan(Math.Pow(_ro0 / ro, 1 / _n) * _tanS2) - S45);
            Double cs = Math.Cos(s);
            Double u = Math.Asin((_cosAzim * Math.Sin(s)) - (_sinAzim * cs * Math.Cos(d)));
            Double kau = _ka * Math.Pow(Math.Tan((u / 2.0) + S45), 1 / _alfa);
            Double deltav = Math.Asin((cs * Math.Sin(d)) / Math.Cos(u));
            Double lambda = -deltav / _alfa;
            Double phi = 0;
            Double fi1 = u;

            // iteration calculation
            for (int i = MaximumIterations; ; )
            {
                fi1 = phi;
                Double esf = E * Math.Sin(fi1);
                phi = 2.0 * (Math.Atan(kau * Math.Pow((1.0 + esf) / (1.0 - esf), E / 2.0)) - S45);
                if (Math.Abs(fi1 - phi) <= IterationTolerance)
                {
                    break;
                }

                if (--i < 0)
                {
                    break;
                    //throw new ProjectionException(Errors.format(ErrorKeys.NO_CONVERGENCE));
                }
            }

            return CreateCoordinate((Degrees)new Radians(lambda + _centralMeridian), (Degrees)new Radians(phi), point);
        }
    }

    /// <summary>
    /// Implemetns the Krovak Projection.
    /// </summary>
    /// <remarks>
    /// <para>The normal case of the Lambert Conformal conic is for the axis of the cone 
    /// to be coincident with the minor axis of the ellipsoid, that is the axis of the cone 
    /// is normal to the ellipsoid at a pole. For the Oblique Conformal Conic the axis 
    /// of the cone is normal to the ellipsoid at a defined location and its extension 
    /// cuts the minor axis at a defined angle. This projection is used in the Czech Republic 
    /// and Slovakia under the name "Krovak" projection.</para>
    /// </remarks>
    internal class KrovakProjection<TCoordinate> : MapProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly Double _falseEasting, _falseNorthing;
        /**
         * Maximum number of iterations for iterative computations.
         */
        protected const int MaximumIterations = 15;

        /**
         * When to stop the iteration.
         */
        protected const double IterationTolerance = 1E-11;

        /**
         * Azimuth of the centre line passing through the centre of the projection.
         * This is equals to the co-latitude of the cone axis at point of intersection
         * with the ellipsoid.
         */
        protected Radians _azimuth;

        /**
         * Latitude of pseudo standard parallel.
         */
        protected Radians _pseudoStandardParallel;

        /**
         * Useful variables calculated from parameters defined by user.
         */
        protected double _sinAzim, _cosAzim, _n, _tanS2, _alfa, _hae, _k1, _ka, _ro0, _rop;

        protected Radians _centralMeridian;
        protected Radians _latitudeOfOrigin;
        protected double _scaleFactor;

        /**
         * Useful constant - 45° in radians.
         */
        protected const Double S45 = 0.785398163397448;

        #region Constructors

        /// <summary>
        /// Creates an instance of an Krovac projection object.
        /// </summary>
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
        /// <param name="parameters">List of parameters to initialize the projection.</param>
        /// <param name="isInverse">Indicates whether the projection forward (meters to degrees or degrees to meters).</param>
        public KrovakProjection(IEnumerable<ProjectionParameter> parameters, ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {
            Authority = "EPSG";
            AuthorityCode = "9819";

            //PROJCS["S-JTSK (Ferro) / Krovak",
            //GEOGCS["S-JTSK (Ferro)",
            //    DATUM["D_S_JTSK_Ferro",
            //        SPHEROID["Bessel 1841",6377397.155,299.1528128]],
            //    PRIMEM["Ferro",-17.66666666666667],
            //    UNIT["degree",0.0174532925199433]],
            //PROJECTION["Krovak"],
            //PARAMETER["latitude_of_center",49.5],
            //PARAMETER["longitude_of_center",42.5],
            //PARAMETER["azimuth",30.28813972222222],
            //PARAMETER["pseudo_standard_parallel_1",78.5],
            //PARAMETER["scale_factor",0.9999],
            //PARAMETER["false_easting",0],
            //PARAMETER["false_northing",0],
            //UNIT["metre",1]]

            ProjectionParameter parLatitudeOfCenter = GetParameter("latitude_of_center");
            ProjectionParameter parLongitudeOfCenter = GetParameter("longitude_of_center");
            ProjectionParameter parAzimuth = GetParameter("azimuth");
            ProjectionParameter parPseudoStandardParallel1 = GetParameter("pseudo_standard_parallel_1");
            ProjectionParameter parScaleFactor = GetParameter("scale_factor");
            ProjectionParameter parFalseEasting = GetParameter("false_easting");
            ProjectionParameter parFalseNorthing = GetParameter("false_northing");

            //Check for missing parameters
            if (parLatitudeOfCenter == null)
                throw new ArgumentException("Missing projection parameter 'latitude_of_center'");
            if (parLongitudeOfCenter == null)
                throw new ArgumentException("Missing projection parameter 'longitude_of_center'");
            if (parAzimuth == null)
                throw new ArgumentException("Missing projection parameter 'azimuth'");
            if (parPseudoStandardParallel1 == null)
                throw new ArgumentException("Missing projection parameter 'pseudo_standard_parallel_1'");
            if (parFalseEasting == null)
                throw new ArgumentException("Missing projection parameter 'false_easting'");
            if (parFalseNorthing == null)
                throw new ArgumentException("Missing projection parameter 'false_northing'");

            _latitudeOfOrigin = (Radians) new Degrees(parLatitudeOfCenter.Value);
            _centralMeridian = (Radians) new Degrees((24 + (50.0 / 60)));// par_longitude_of_center.Value);
            _azimuth = (Radians)new Degrees((Double)parAzimuth.Value);
            _pseudoStandardParallel = (Radians)new Degrees(parPseudoStandardParallel1.Value);
            _scaleFactor = parScaleFactor.Value;

            _falseEasting = parFalseEasting.Value * MetersPerUnit;
            _falseNorthing = parFalseNorthing.Value * MetersPerUnit;

            //_excentricitySquared = 1.0 - (SemiMinor * SemiMinor) / (SemiMajor * SemiMajor);
            //_excentricity = Math.Sqrt(_excentricitySquared);

            // Calculates useful constants.
            _sinAzim = Math.Sin(_azimuth);
            _cosAzim = Math.Cos(_azimuth);
            _n = Math.Sin(_pseudoStandardParallel);
            _tanS2 = Math.Tan(_pseudoStandardParallel / 2 + S45);

            Double sinLat = Math.Sin(_latitudeOfOrigin);
            Double cosLat = Math.Cos(_latitudeOfOrigin);
            Double cosL2 = cosLat * cosLat;
            _alfa = Math.Sqrt(1 + ((E2 * (cosL2 * cosL2)) / (1 - E2))); // parameter B
            _hae = _alfa * E / 2;
            Double u0 = Math.Asin(sinLat / _alfa);

            Double esl = E * sinLat;
            Double g = Math.Pow((1 - esl) / (1 + esl), (_alfa * E) / 2);
            _k1 = Math.Pow(Math.Tan(_latitudeOfOrigin / 2 + S45), _alfa) * g / Math.Tan(u0 / 2 + S45);
            _ka = Math.Pow(1 / _k1, -1 / _alfa);

            Double radius = Math.Sqrt(1 - E2) / (1 - (E2 * (sinLat * sinLat)));

            _ro0 = _scaleFactor * radius / Math.Tan(_pseudoStandardParallel);
            _rop = _ro0 * Math.Pow(_tanS2, _n);
        }
        #endregion

        public override string ProjectionClassName
        {
            get { return Name; }
        }

        public override string Name
        {
            get { return "Krovak"; }
        }
        /*
        /// <summary>
        /// Converts coordinates in decimal degrees to projected meters.
        /// </summary>
        /// <param name="lonlat">The point in decimal degrees.</param>
        /// <returns>Point in projected meters</returns>
        public override TCoordinate DegreesToMeters(TCoordinate lonlat)
        {
            Radians lambda = (Radians) new Degrees((Double)lonlat[0]) - _centralMeridian;
            Radians phi = (Radians) new Degrees((Double)lonlat[1]);

            double esp = E * Math.Sin(phi);
            double gfi = Math.Pow(((1.0 - esp) / (1.0 + esp)), _hae);
            double u = 2 * (Math.Atan(Math.Pow(Math.Tan(phi / 2 + S45), _alfa) / _k1 * gfi) - S45);
            double deltav = -lambda * _alfa;
            double cosU = Math.Cos(u);
            double s = Math.Asin((_cosAzim * Math.Sin(u)) + (_sinAzim * cosU * Math.Cos(deltav)));
            double d = Math.Asin(cosU * Math.Sin(deltav) / Math.Cos(s));
            double eps = _n * d;
            double ro = _rop / Math.Pow(Math.Tan(s / 2 + S45), _n);

            //x and y are reverted
            double y = -(ro * Math.Cos(eps)) * SemiMajor;
            double x = -(ro * Math.Sin(eps)) * SemiMajor;

            return CreateCoordinate( x, y, lonlat );
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
            throw new NotSupportedException();
            Double x = (Double)p[0].Divide(SemiMajor);
            Double y = (Double)p[1].Divide(SemiMajor);

            //x -> southing, y -> westing
            Double ro = Math.Sqrt(x * x + y * y);
            Double eps = Math.Atan2(-x, -y);
            Double d = eps / _n;
            Double s = 2 * (Math.Atan(Math.Pow(_ro0 / ro, 1 / _n) * _tanS2) - S45);
            Double cs = Math.Cos(s);
            Double u = Math.Asin((_cosAzim * Math.Sin(s)) - (_sinAzim * cs * Math.Cos(d)));
            Double kau = _ka * Math.Pow(Math.Tan((u / 2.0) + S45), 1 / _alfa);
            Double deltav = Math.Asin((cs * Math.Sin(d)) / Math.Cos(u));
            Double lambda = -deltav / _alfa;
            Double phi = 0;
            Double fi1 = u;

            // iteration calculation
            for (int i = MaximumIterations; ; )
            {
                fi1 = phi;
                Double esf = E * Math.Sin(fi1);
                phi = 2.0 * (Math.Atan(kau * Math.Pow((1.0 + esf) / (1.0 - esf), E / 2.0)) - S45);
                if (Math.Abs(fi1 - phi) <= IterationTolerance)
                {
                    break;
                }

                if (--i < 0)
                {
                    break;
                    //throw new ProjectionException(Errors.format(ErrorKeys.NO_CONVERGENCE));
                }
            }

            return CreateCoordinate((Degrees) new Radians(lambda + _centralMeridian), (Degrees)new Radians(phi), p);
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

            return new InverseKrovakProjection<TCoordinate>(parameters, CoordinateFactory, this);
        }

        public override TCoordinate Transform(TCoordinate lonlat)
        {
            Radians lambda = (Radians)new Degrees((Double)lonlat[0]) - _centralMeridian;
            Radians phi = (Radians)new Degrees((Double)lonlat[1]);

            double esp = E * Math.Sin(phi);
            double gfi = Math.Pow(((1.0 - esp) / (1.0 + esp)), _hae);
            double u = 2 * (Math.Atan(Math.Pow(Math.Tan(phi / 2 + S45), _alfa) / _k1 * gfi) - S45);
            double deltav = -lambda * _alfa;
            double cosU = Math.Cos(u);
            double s = Math.Asin((_cosAzim * Math.Sin(u)) + (_sinAzim * cosU * Math.Cos(deltav)));
            double d = Math.Asin(cosU * Math.Sin(deltav) / Math.Cos(s));
            double eps = _n * d;
            double ro = _rop / Math.Pow(Math.Tan(s / 2 + S45), _n);

            /* x and y are reverted  */
            double y = -(ro * Math.Cos(eps)) * SemiMajor;
            double x = -(ro * Math.Sin(eps)) * SemiMajor;

            return CreateCoordinate(x, y, lonlat);
        }

    }
}
