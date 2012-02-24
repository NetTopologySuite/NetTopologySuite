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
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.CoordinateSystems.Projections
{
    internal class InverseTransverseMercator<TCoordinate> : TransverseMercator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public InverseTransverseMercator(IEnumerable<ProjectionParameter> parameters,
                                         ICoordinateFactory<TCoordinate> coordinateFactory,
                                         TransverseMercator<TCoordinate> transform)
            : base(parameters, coordinateFactory)
        {
            Inverse = transform;
        }

        public override Boolean IsInverse
        {
            get
            {
                return true;
            }
        }

        public override TCoordinate Transform(TCoordinate coordinate)
        {
            Int64 i; // counter variable
            Double semiMajor = SemiMajor;

            Double x = ((Double)coordinate[0]) * MetersPerUnit - _falseEasting;
            Double y = ((Double)coordinate[1]) * MetersPerUnit - _falseNorthing;

            Double con = (_ml0 + y / _scaleFactor) / semiMajor;
            Double phi = DamTool.Phi1(con);

            TCoordinate transformed;

            if (Math.Abs(phi) < HalfPI)
            {
                Double sinPhi, cosPhi; // sin cos and tangent values
                SinCos(phi, out sinPhi, out cosPhi);
                Double tanPhi = Math.Tan(phi);
                Double c = _esp * Math.Pow(cosPhi, 2);
                Double cs = Math.Pow(c, 2);
                Double t = Math.Pow(tanPhi, 2);
                Double ts = Math.Pow(t, 2);
                con = 1.0 - E2 * Math.Pow(sinPhi, 2);
                Double n = semiMajor / Math.Sqrt(con);
                Double r = n * (1.0 - E2) / con;
                Double d = x / (n * _scaleFactor);
                Double ds = Math.Pow(d, 2);

                Radians lat = (Radians)(phi -
                                        (n * tanPhi * ds / r) *
                                        (0.5 - ds / 24.0 *
                                            (5.0 + 3.0 * t + 10.0 * c - 4.0 * cs - 9.0 * _esp - ds / 30.0 *
                                                (61.0 + 90.0 * t + 298.0 * c + 45.0 * ts - 252.0 * _esp - 3.0 * cs))));

                Radians lon = (Radians)AdjustLongitude(_centralMeridian +
                                                       (d *
                                                            (1.0 - ds / 6.0 *
                                                                (1.0 + 2.0 * t + c - ds / 20.0 *
                                                                    (5.0 - 2.0 * c + 28.0 * t -
                                                                     3.0 * cs + 8.0 * _esp + 24.0 * ts))) / cosPhi));

                return CreateCoordinate((Degrees)lon, (Degrees)lat, coordinate);
            }

            return CreateCoordinate((Degrees)new Radians(HalfPI * Sign(y)), new Degrees(_centralMeridian), coordinate);
        }
    }

    /// <summary>
    /// Implements the Universal (UTM) and Modified (MTM) Transverses Mercator projections.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a cylindrical projection, in which the cylinder has been rotated 90°.
    /// Instead of being tangent to the equator (or to an other standard latitude),
    /// it is tangent to a central meridian. Deformation are more important as we
    /// are going futher from the central meridian. The Transverse Mercator
    /// projection is appropriate for region wich have a greater extent north-south
    /// than east-west.
    /// </para>
    /// <para>
    /// Reference: John P. Snyder (Map Projections - A Working Manual,
    ///            U.S. Geological Survey Professional Paper 1395, 1987)
    /// </para>
    /// </remarks>
    internal class TransverseMercator<TCoordinate> : MapProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        protected readonly Double _scaleFactor; /* scale factor				*/
        protected readonly Radians _centralMeridian; /* Center longitude (projection center) */
        private readonly Radians _latOrigin; /* center latitude			*/
        private readonly Double _e0, _e1, _e2, _e3; /* eccentricity constants	*/
        protected readonly Double _esp; /* eccentricity constants	*/
        protected readonly Double _ml0; /* small value m			*/
        protected readonly Double _falseNorthing; /* y offset in meters		*/
        protected readonly Double _falseEasting; /* x offset in meters		*/
        //static Double ind;		               /* spherical flag			*/

        /// <summary>
        /// Initializes a <see cref="TransverseMercator{TCoordinate}"/> projection
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
        /// <item><term>semi_major</term><description>Semi major radius</description></item>
        /// <item><term>semi_minor</term><description>Semi minor radius</description></item>
        /// <item><term>scale_factor</term><description></description></item>
        /// <item><term>central meridian</term><description></description></item>
        /// <item><term>latitude_origin</term><description></description></item>
        /// <item><term>false_easting</term><description></description></item>
        /// <item><term>false_northing</term><description></description></item>
        /// </list>
        /// </remarks>
        public TransverseMercator(IEnumerable<ProjectionParameter> parameters,
                                  ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {
            Authority = "EPSG";
            AuthorityCode = "9807";

            ProjectionParameter scaleFactor = GetParameter("scale_factor");
            ProjectionParameter centralMeridian = GetParameter("central_meridian");
            ProjectionParameter latitudeOfOrigin = GetParameter("latitude_of_origin");
            ProjectionParameter falseEasting = GetParameter("false_easting");
            ProjectionParameter falseNorthing = GetParameter("false_northing");

            //Check for missing parameters
            if (scaleFactor == null)
            {
                throw new ArgumentException("Missing projection parameter 'scale_factor'");
            }

            if (centralMeridian == null)
            {
                throw new ArgumentException("Missing projection parameter 'central_meridian'");
            }

            if (latitudeOfOrigin == null)
            {
                throw new ArgumentException("Missing projection parameter 'latitude_of_origin'");
            }

            if (falseEasting == null)
            {
                throw new ArgumentException("Missing projection parameter 'false_easting'");
            }

            if (falseNorthing == null)
            {
                throw new ArgumentException("Missing projection parameter 'false_northing'");
            }

            _scaleFactor = scaleFactor.Value;
            _centralMeridian = (Radians)new Degrees(centralMeridian.Value);
            _latOrigin = (Radians)new Degrees(latitudeOfOrigin.Value);
            _falseEasting = falseEasting.Value * MetersPerUnit;
            _falseNorthing = falseNorthing.Value * MetersPerUnit;

            Double semiMajor = SemiMajor;
            //_e0 = ComputeE0(E2);
            //_e1 = ComputeE1(E2);
            //_e2 = ComputeE2(E2);
            //_e3 = ComputeE3(E2);
            _ml0 = semiMajor * MeridianLength(_latOrigin);
            _esp = E2 / (1.0 - E2);
        }

        /*
        /// <summary>
        /// Converts coordinates in decimal degrees to projected meters.
        /// </summary>
        /// <param name="lonlat">The point in decimal degrees.</param>
        /// <returns>Point in projected meters</returns>
        public override TCoordinate DegreesToMeters(TCoordinate lonlat)
        {
            DoubleComponent lonVal, latVal;
            lonlat.GetComponents(out lonVal, out latVal);
            Radians lon = (Radians)new Degrees((Double)lonVal);
            Radians lat = (Radians)new Degrees((Double)latVal);
            Double semiMajor = SemiMajor;

            Double deltaLon = AdjustLongitude(lon - _centralMeridian);

            Double sinPhi, cosPhi; // sin and cos value
            SinCos(lat, out sinPhi, out cosPhi);

            Double al = cosPhi * deltaLon;
            Double als = Math.Pow(al, 2);
            Double c = _esp * Math.Pow(cosPhi, 2);
            Double tq = Math.Tan(lat);
            Double t = Math.Pow(tq, 2);
            Double con = 1.0 - E2 * Math.Pow(sinPhi, 2);
            Double n = semiMajor / Math.Sqrt(con);
            Double ml = semiMajor * MeridianLength(lat);

            Double x = _scaleFactor * n * al *
                       (1.0 + als / 6.0 *
                            (1.0 - t + c + als / 20.0 *
                                (5.0 - 18.0 * t + Math.Pow(t, 2) + 72.0 * c - 58.0 * _esp)))
                       + _falseEasting;

            Double y = _scaleFactor *
                       (ml - _ml0 + n * tq *
                            (als *
                                (0.5 + als / 24.0 *
                                    (5.0 - t + 9.0 * c + 4.0 * Math.Pow(c, 2) + als / 30.0 *
                                        (61.0 - 58.0 * t + Math.Pow(t, 2) + 600.0 * c - 330.0 * _esp)))))
                       + _falseNorthing;

            return lonlat.ComponentCount < 3
                       ? CreateCoordinate(x / MetersPerUnit, y / MetersPerUnit)
                       : CreateCoordinate(x / MetersPerUnit, y / MetersPerUnit, (Double)lonlat[2]);
        }
        */

        public override string ProjectionClassName
        {
            get { return "Transverse_Mercator"; }
        }

        public override string Name
        {
            get { return "Transverse_Mercator"; }
        }

        /*
        /// <summary>
        /// Converts coordinates in projected meters to decimal degrees.
        /// </summary>
        /// <param name="p">Point in meters</param>
        /// <returns>Transformed point in decimal degrees</returns>
        public override TCoordinate MetersToDegrees(TCoordinate p)
        {
            Int64 i; // counter variable
            Double semiMajor = SemiMajor;

            Double x = ((Double)p[0]) * MetersPerUnit - _falseEasting;
            Double y = ((Double)p[1]) * MetersPerUnit - _falseNorthing;

            Double con = (_ml0 + y / _scaleFactor) / semiMajor;
            Double phi = DamTool.Phi1(con);

            //Double con = (_ml0 + y / _scaleFactor) / semiMajor;
            //Double phi = con;

            //for (i = 0; ; i++)
            //{
            //    Double deltaPhi; /* difference between longitudes

            //    deltaPhi = -phi +
            //          ((con + _e1 * Math.Sin(2.0 * phi) -
            //                  _e2 * Math.Sin(4.0 * phi) +
            //                  _e3 * Math.Sin(6.0 * phi)) / _e0);

            //    phi += deltaPhi;

            //    if (Math.Abs(deltaPhi) <= Epsilon)
            //    {
            //        break;
            //    }

            //    if (i >= MaxIterationCount)
            //    {
            //        throw ComputationalConvergenceError();
            //    }
            //}

            TCoordinate transformed;

            if (Math.Abs(phi) < HalfPI)
            {
                Double sinPhi, cosPhi; // sin cos and tangent values
                SinCos(phi, out sinPhi, out cosPhi);
                Double tanPhi = Math.Tan(phi);
                Double c = _esp * Math.Pow(cosPhi, 2);
                Double cs = Math.Pow(c, 2);
                Double t = Math.Pow(tanPhi, 2);
                Double ts = Math.Pow(t, 2);
                con = 1.0 - E2 * Math.Pow(sinPhi, 2);
                Double n = semiMajor / Math.Sqrt(con);
                Double r = n * (1.0 - E2) / con;
                Double d = x / (n * _scaleFactor);
                Double ds = Math.Pow(d, 2);

                Radians lat = (Radians)(phi -
                                        (n * tanPhi * ds / r) *
                                        (0.5 - ds / 24.0 *
                                            (5.0 + 3.0 * t + 10.0 * c - 4.0 * cs - 9.0 * _esp - ds / 30.0 *
                                                (61.0 + 90.0 * t + 298.0 * c + 45.0 * ts - 252.0 * _esp - 3.0 * cs))));

                Radians lon = (Radians)AdjustLongitude(_centralMeridian +
                                                       (d *
                                                            (1.0 - ds / 6.0 *
                                                                (1.0 + 2.0 * t + c - ds / 20.0 *
                                                                    (5.0 - 2.0 * c + 28.0 * t -
                                                                     3.0 * cs + 8.0 * _esp + 24.0 * ts))) / cosPhi));

                transformed = p.ComponentCount < 3
                                   ? CreateCoordinate((Degrees)lon,
                                                      (Degrees)lat)
                                   : CreateCoordinate((Degrees)lon,
                                                      (Degrees)lat,
                                                      (Double)p[2]);
            }
            else
            {
                transformed = p.ComponentCount < 3
                                   ? CreateCoordinate((Degrees)new Radians(HalfPI * Sign(y)),
                                                      new Degrees(_centralMeridian))
                                   : CreateCoordinate((Degrees)new Radians(HalfPI * Sign(y)),
                                                      new Degrees(_centralMeridian),
                                                      (Double)p[2]);
            }

            return transformed;
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
            return new InverseTransverseMercator<TCoordinate>(parameters, CoordinateFactory, this);
        }

        public override TCoordinate Transform(TCoordinate point)
        {
            DoubleComponent lonVal, latVal;
            ((IVector<DoubleComponent>)point).GetComponents(out lonVal, out latVal);
            Radians lon = (Radians)new Degrees((Double)lonVal);
            Radians lat = (Radians)new Degrees((Double)latVal);
            Double semiMajor = SemiMajor;

            Double deltaLon = AdjustLongitude(lon - _centralMeridian);

            Double sinPhi, cosPhi; // sin and cos value
            SinCos(lat, out sinPhi, out cosPhi);

            Double al = cosPhi * deltaLon;
            Double als = Math.Pow(al, 2);
            Double c = _esp * Math.Pow(cosPhi, 2);
            Double tq = Math.Tan(lat);
            Double t = Math.Pow(tq, 2);
            Double con = 1.0 - E2 * Math.Pow(sinPhi, 2);
            Double n = semiMajor / Math.Sqrt(con);
            Double ml = semiMajor * MeridianLength(lat);

            Double x = _scaleFactor * n * al *
                       (1.0 + als / 6.0 *
                            (1.0 - t + c + als / 20.0 *
                                (5.0 - 18.0 * t + Math.Pow(t, 2) + 72.0 * c - 58.0 * _esp)))
                       + _falseEasting;

            Double y = _scaleFactor *
                       (ml - _ml0 + n * tq *
                            (als *
                                (0.5 + als / 24.0 *
                                    (5.0 - t + 9.0 * c + 4.0 * Math.Pow(c, 2) + als / 30.0 *
                                        (61.0 - 58.0 * t + Math.Pow(t, 2) + 600.0 * c - 330.0 * _esp)))))
                       + _falseNorthing;

            return CreateCoordinate(x * UnitsPerMeter, y * UnitsPerMeter, point);
        }
    }
}