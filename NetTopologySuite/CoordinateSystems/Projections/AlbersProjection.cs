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
using GeoAPI.Units;
using NPack.Interfaces;
using GeoAPI.DataStructures;

namespace NetTopologySuite.CoordinateSystems.Projections
{
    internal class InverseAlbersProjection<TCoordinate> : AlbersProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {

        public InverseAlbersProjection(IEnumerable<ProjectionParameter> parameters,
                                       ICoordinateFactory<TCoordinate> coordinateFactory,
                                       AlbersProjection<TCoordinate> transform)
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
            Double p0 = (Double)point[0];
            Double theta = Math.Atan((p0 * MetersPerUnit - _falseEasting) /
                                     (_rho0 - (point[1].Multiply(MetersPerUnit) - _falseNorthing)));

            Double rho = Math.Sqrt(Math.Pow(p0 * MetersPerUnit - _falseEasting, 2) +
                                   Math.Pow(_rho0 - (point[1].Multiply(MetersPerUnit) - _falseNorthing), 2));

            Double q = (_c - Math.Pow(rho, 2) * Math.Pow(_n, 2) / Math.Pow(SemiMajor, 2)) / _n;
            //Double b = Math.Sin(q / (1 - ((1 - e_sq) / (2 * e)) * Math.Log((1 - e) / (1 + e))));

            Radians lat = (Radians)Math.Asin(q * 0.5);
            Double preLat = Double.MaxValue;
            Int32 iterationCounter = 0;

            while (Math.Abs(lat - preLat) > 0.000001)
            {
                preLat = lat;
                Double sin = Math.Sin(lat);
                Double e2sin2 = E2 * Math.Pow(sin, 2);

                lat = (Radians)
                      ((Math.Pow(1 - e2sin2, 2) / (2 * Math.Cos(lat))) *
                        ((q / (1 - E2)) - sin / (1 - e2sin2) + 1 / (2 * E) *
                            Math.Log((1 - E * sin) / (1 + E * sin))) +
                                lat);

                iterationCounter++;

                if (iterationCounter > 25)
                {
                    throw new ComputationException("Transformation failed to converge " +
                                                              "in Albers backwards transformation.");
                }
            }

            Radians lon = (Radians)(_centerLongitude + (theta / _n));

            return CreateCoordinate((Degrees) lon, (Degrees) lat, point);
        }
    }

    /// <summary>
    ///	Implements the Albers projection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implements the Albers projection. The Albers projection is most commonly
    /// used to project the United States of America. It gives the northern
    /// border with Canada a curved appearance.
    /// </para>
    ///	<para>
    /// The <a href="http://www.geog.mcgill.ca/courses/geo201/mapproj/naaeana.gif">
    /// Albers Equal Area</a> projection has the property that the area bounded
    ///	by any pair of parallels and meridians is exactly reproduced between the 
    ///	image of those parallels and meridians in the projected domain, that is,
    ///	the projection preserves the correct area of the earth though distorts
    ///	direction, distance and shape somewhat.
    /// </para>
    /// </remarks>
    internal class AlbersProjection<TCoordinate> : MapProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        protected readonly Double _falseEasting;
        protected readonly Double _falseNorthing;
        protected readonly Double _c; //constant c
        protected readonly Double _rho0;
        protected readonly Double _n;
        protected readonly Radians _centerLongitude; //center longitude   

        #region Constructors
        /// <summary>
        /// Initializes a <see cref="AlbersProjection{TCoordinate}"/> projection 
        /// with the specified parameters. 
        /// </summary>
        /// <param name="parameters">Parameters of the projection.</param>
        /// <param name="coordinateFactory">Coordinate factory to use.</param>
        /// <remarks>
        /// <list type="table">
        ///     <listheader>
        ///      <term>Parameter</term>
        ///      <description>Description</description>
        ///    </listheader>
        /// <item>
        ///     <term>latitude_of_false_origin</term>
        ///     <description>
        ///         The latitude of the point which is not the natural origin and 
        ///         at which grid coordinate values false easting and false northing are defined.
        ///     </description>
        /// </item>
        /// <item>
        ///     <term>longitude_of_false_origin</term>
        ///     <description>
        ///         The longitude of the point which is not the natural origin and at 
        ///         which grid coordinate values false easting and false northing are defined.
        ///     </description>
        /// </item>
        /// <item>
        ///     <term>latitude_of_1st_standard_parallel</term>
        ///     <description>
        ///         For a conic projection with two standard parallels, this is the latitude of 
        ///         intersection of the cone with the ellipsoid that is nearest the pole.  
        ///         Scale is true along this parallel.
        ///     </description>
        /// </item>
        /// <item>
        ///     <term>latitude_of_2nd_standard_parallel</term>
        ///     <description>
        ///         For a conic projection with two standard parallels, this is the latitude of 
        ///         intersection of the cone with the ellipsoid that is furthest from the pole.  
        ///         Scale is true along this parallel.
        ///     </description>
        /// </item>
        /// <item>
        ///     <term>easting_at_false_origin</term>
        ///     <description>The easting value assigned to the false origin.</description>
        /// </item>
        /// <item>
        ///     <term>northing_at_false_origin</term>
        ///     <description>The northing value assigned to the false origin.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public AlbersProjection(IEnumerable<ProjectionParameter> parameters,
                                ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {
            //Retrieve parameters
            ProjectionParameter longitudeOfCenter = GetParameter("longitude_of_center");
            ProjectionParameter latitudeOfCenter = GetParameter("latitude_of_center");
            ProjectionParameter standardParallel1 = GetParameter("standard_parallel_1");
            ProjectionParameter standardParallel2 = GetParameter("standard_parallel_2");
            ProjectionParameter falseEasting = GetParameter("false_easting");
            ProjectionParameter falseNorthing = GetParameter("false_northing");

            //Check for missing parameters
            if (longitudeOfCenter == null)
            {
                longitudeOfCenter = GetParameter("central_meridian"); //Allow for altenative name

                if (longitudeOfCenter == null)
                {
                    throw new ArgumentException("Missing projection parameter 'longitude_of_center'");
                }
            }

            if (latitudeOfCenter == null)
            {
                latitudeOfCenter = GetParameter("latitude_of_origin"); //Allow for altenative name

                if (latitudeOfCenter == null)
                {
                    throw new ArgumentException("Missing projection parameter 'latitude_of_center'");
                }
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

            _centerLongitude = (Radians)new Degrees(longitudeOfCenter.Value);
            Radians lat0 = (Radians)new Degrees(latitudeOfCenter.Value);
            Radians lat1 = (Radians)new Degrees(standardParallel1.Value);
            Radians lat2 = (Radians)new Degrees(standardParallel2.Value);
            _falseEasting = falseEasting.Value * MetersPerUnit;
            _falseNorthing = falseNorthing.Value * MetersPerUnit;

            if (Math.Abs(lat1 + lat2) < Double.Epsilon)
            {
                throw new ArgumentException("Equal latitudes for standard parallels on opposite sides of Equator.");
            }

            Double alpha1 = computeAlpha(lat1);
            Double alpha2 = computeAlpha(lat2);

            Double m1 = Math.Cos(lat1) / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(lat1), 2));
            Double m2 = Math.Cos(lat2) / Math.Sqrt(1 - E2 * Math.Pow(Math.Sin(lat2), 2));

            _n = (Math.Pow(m1, 2) - Math.Pow(m2, 2)) / (alpha2 - alpha1);
            _c = Math.Pow(m1, 2) + (_n * alpha1);

            _rho0 = computeRho(computeAlpha(lat0));
            /*
            Double sin_p0 = Math.Sin(lat0);
            Double cos_p0 = Math.Cos(lat0);
            Double q0 = qsfnz(e, sin_p0, cos_p0);

            Double sin_p1 = Math.Sin(lat1);
            Double cos_p1 = Math.Cos(lat1);
            Double m1 = msfnz(e,sin_p1,cos_p1);
            Double q1 = qsfnz(e,sin_p1,cos_p1);


            Double sin_p2 = Math.Sin(lat2);
            Double cos_p2 = Math.Cos(lat2);
            Double m2 = msfnz(e,sin_p2,cos_p2);
            Double q2 = qsfnz(e,sin_p2,cos_p2);

            if (Math.Abs(lat1 - lat2) > EPSLN)
                ns0 = (m1 * m1 - m2 * m2)/ (q2 - q1);
            else
                ns0 = sin_p1;
            C = m1 * m1 + ns0 * q1;
            rh = this._semiMajor * Math.Sqrt(C - ns0 * q0)/ns0;
            */
        }

        #endregion

        #region Public methods
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

            Double a = computeAlpha(lat);
            Double rho = computeRho(a);
            Double theta = _n * (lon - _centerLongitude);
            Double x = _falseEasting + rho * Math.Sin(theta);
            Double y = _falseNorthing + _rho0 - (rho * Math.Cos(theta));

            return lonlat.ComponentCount == 2
                       ? CreateCoordinate(x / MetersPerUnit, y / MetersPerUnit)
                       : CreateCoordinate(x / MetersPerUnit, y / MetersPerUnit, (Double)lonlat[2]);
        }
        */
        public override string ProjectionClassName
        {
            get { return "Albers_Conic_Equal_Area"; }
        }

        public override string Name
        {
            get { return "Albers_Conic_Equal_Area"; }
        }
        /*
        /// <summary>
        /// Converts coordinates in projected meters to decimal degrees.
        /// </summary>
        /// <param name="p">Point in meters</param>
        /// <returns>Transformed point in decimal degrees</returns>
        public override TCoordinate MetersToDegrees(TCoordinate p)
        {
            Double p0 = (Double)p[0];
            Double theta = Math.Atan((p0 * MetersPerUnit - _falseEasting) /
                                     (_rho0 - (p[1].Multiply(MetersPerUnit) - _falseNorthing)));

            Double rho = Math.Sqrt(Math.Pow(p0 * MetersPerUnit - _falseEasting, 2) +
                                   Math.Pow(_rho0 - (p[1].Multiply(MetersPerUnit) - _falseNorthing), 2));

            Double q = (_c - Math.Pow(rho, 2) * Math.Pow(_n, 2) / Math.Pow(SemiMajor, 2)) / _n;
            //Double b = Math.Sin(q / (1 - ((1 - e_sq) / (2 * e)) * Math.Log((1 - e) / (1 + e))));

            Radians lat = (Radians)Math.Asin(q * 0.5);
            Double preLat = Double.MaxValue;
            Int32 iterationCounter = 0;

            while (Math.Abs(lat - preLat) > 0.000001)
            {
                preLat = lat;
                Double sin = Math.Sin(lat);
                Double e2sin2 = E2 * Math.Pow(sin, 2);

                lat = (Radians)
                      ((Math.Pow(1 - e2sin2, 2) / (2 * Math.Cos(lat))) *
                        ((q / (1 - E2)) - sin / (1 - e2sin2) + 1 / (2 * E) *
                            Math.Log((1 - E * sin) / (1 + E * sin))) + 
                                lat);

                iterationCounter++;

                if (iterationCounter > 25)
                {
                    throw new ComputationConvergenceException("Transformation failed to converge " +
                                                              "in Albers backwards transformation.");
                }
            }

            Radians lon = (Radians)(_centerLongitude + (theta / _n));

            return p.ComponentCount == 2
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

        protected override IMathTransform ComputeInverse(IMathTransform setAsInverse)
        {
            IEnumerable<ProjectionParameter> parameters =
                Caster.Downcast<ProjectionParameter, Parameter>(Parameters);
            return new InverseAlbersProjection<TCoordinate>(parameters, CoordinateFactory, this);
        }

        #endregion

        #region Math helper functions

        //private Double ToAuthalic(Double lat)
        //{
        //    return Math.Atan(Q(lat) / Q(Math.PI * 0.5));
        //}

        //private Double Q(Double angle)
        //{
        //    Double sin = Math.Sin(angle);
        //    Double esin = e * sin;
        //    return Math.Abs(sin / (1 - Math.Pow(esin, 2)) - 0.5 * e) * Math.Log((1 - esin) / (1 + esin)));
        //}

        private Double computeAlpha(Double lat)
        {
            Double sin = Math.Sin(lat);
            Double sinsq = Math.Pow(sin, 2);
            return (1 - E2) * (((sin / (1 - E2 * sinsq)) - 1
                                                     / (2 * E) * Math.Log((1 - E * sin) / (1 + E * sin))));
        }

        private Double computeRho(Double a)
        {
            return SemiMajor * Math.Sqrt((_c - _n * a)) / _n;
        }

        #endregion

        public override TCoordinate Transform(TCoordinate lonlat)
        {
            Radians lon = (Radians)new Degrees((Double)lonlat[0]);
            Radians lat = (Radians)new Degrees((Double)lonlat[1]);

            Double a = computeAlpha(lat);
            Double rho = computeRho(a);
            Double theta = _n * (lon - _centerLongitude);
            Double x = _falseEasting + rho * Math.Sin(theta);
            Double y = _falseNorthing + _rho0 - (rho * Math.Cos(theta));

            return CreateCoordinate(x * UnitsPerMeter, y * UnitsPerMeter, lonlat);
        }
    }
}