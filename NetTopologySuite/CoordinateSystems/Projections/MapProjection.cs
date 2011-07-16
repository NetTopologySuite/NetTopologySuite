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
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.Units;
using NPack.Interfaces;
using NetTopologySuite.CoordinateSystems.Transformations;
using GeoAPI.DataStructures;

namespace NetTopologySuite.CoordinateSystems.Projections
{
    /// <summary>
    /// Base class for concrete map projections.
    /// </summary>
    internal abstract class MapProjection<TCoordinate> : GeoMathTransform<TCoordinate>, IProjection
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly Double _metersPerUnit;
        private readonly Double _unitsPerMeter;

        // TODO: can these fields / properties get factored out and shared with CoordinateTransformation<TCoordinate>?
        private String _abbreviation;
        private String _alias;
        private String _authority;
        private String _authorityCode;
        //private String _name;
        private String _remarks;

        private DistanceAlongMeridianTool _damTool;
        public DistanceAlongMeridianTool DamTool
        {
            get 
            {
                if ( _damTool == null )
                    _damTool = new DistanceAlongMeridianTool(E2);
                return _damTool;
            }
        }

        protected MapProjection(IEnumerable<ProjectionParameter> parameters,
                                ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(Caster.Upcast<Parameter, ProjectionParameter>(parameters),
                   coordinateFactory)
        {
            ProjectionParameter unit = GetParameter("unit");
            _metersPerUnit = unit != null ? unit.Value : 1.0d;
            _unitsPerMeter = 1d/_metersPerUnit;
        }

        #region IProjection Members

        public ProjectionParameter this[Int32 index]
        {
            get { return GetParameter(index); }
        }

        public ProjectionParameter this[String name]
        {
            get { return GetParameter(name); }
        }

        public abstract String ProjectionClassName { get; }

        /// <summary>
        /// Gets an named parameter of the projection.
        /// </summary>
        /// <remarks>The parameter name is case insensitive.</remarks>
        /// <param name="name">Name of parameter.</param>
        /// <returns>
        /// The parameter with the given name or 
        /// <see langword="null"/> if not found.
        /// </returns>
        public ProjectionParameter GetParameter(String name)
        {
            return GetParameterInternal(name) as ProjectionParameter;
        }

        /// <summary>
        /// Gets an named parameter of the projection.
        /// </summary>
        /// <param name="index">Index of parameter.</param>
        /// <returns>
        /// The parameter at the given <paramref name="index"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="index"/> is less than 0 or greater than
        /// or equal to <see cref="MathTransform.ParameterCount"/>.
        /// </exception>
        public ProjectionParameter GetParameter(Int32 index)
        {
            return GetParameterInternal(index) as ProjectionParameter;
        }

        /// <summary>
        /// Gets or sets the abbreviation of the object.
        /// </summary>
        public String Abbreviation
        {
            get { return _abbreviation; }
            set { _abbreviation = value; }
        }

        /// <summary>
        /// Gets or sets the alias of the object.
        /// </summary>
        public String Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        /// <summary>
        /// Gets or sets the authority name for this object, e.g., "EPSG",
        /// if this is a standard object with an authority specific
        /// identity code. Returns "CUSTOM" if this is a custom object.
        /// </summary>
        public String Authority
        {
            get { return _authority; }
            set { _authority = value; }
        }

        /// <summary>
        /// Gets or sets the authority specific identification code of the object.
        /// </summary>
        public String AuthorityCode
        {
            get { return _authorityCode; }
            set { _authorityCode = value; }
        }

        /// <summary>
        /// Gets the name of the object.
        /// </summary>
        public abstract String Name { get; }

        /// <summary>
        /// Gets or sets the provider-supplied remarks for the object.
        /// </summary>
        public String Remarks
        {
            get { return _remarks; }
            set { _remarks = value; }
        }

        #endregion

        #region IEnumerable<ProjectionParameter> Members

        public IEnumerator<ProjectionParameter> GetEnumerator()
        {
            foreach (ProjectionParameter parameter in Parameters)
            {
                yield return parameter;
            }
        }

        #endregion

        //public abstract TCoordinate MetersToDegrees(TCoordinate coordinate);
        //public abstract TCoordinate DegreesToMeters(TCoordinate coordinate);

        #region IMathTransform

        protected internal Double MetersPerUnit
        {
            get { return _metersPerUnit; }
        }

        protected internal Double UnitsPerMeter
        {
            get { return _unitsPerMeter; }
        }

        /// <summary>
        /// Transforms the given point.
        /// </summary>
        public override TCoordinate Transform(TCoordinate point)
        {
            throw new NotImplementedException();
            //return !IsInverse
            //           ? DegreesToMeters(point)
            //           : MetersToDegrees(point);
        }

        public override IEnumerable<TCoordinate> Transform(IEnumerable<TCoordinate> points)
        {
            foreach (TCoordinate point in points)
                yield return Transform(point);

            //if (!IsInverse)
            //{
            //    foreach (TCoordinate point in points)
            //        yield return Transform(point);
            //}
            //else
            //{
            //    foreach (TCoordinate point in points)
            //        yield return MetersToDegrees(point);
            //}
        }

        /// <summary>
        /// Transforms the specified <see cref="ICoordinate"/> according 
        /// to the specific projection.
        /// </summary>
        /// <param name="coordinate">The coordinate to transform.</param>
        /// <returns>
        /// A new <see cref="ICoordinate{t}"/> which results from transforming
        /// the given <paramref name="coordinate"/> with this algorithm.
        /// </returns>
        public override ICoordinate Transform(ICoordinate coordinate)
        {
            return coordinate.ContainsOrdinate(Ordinates.Z)
                       ? Transform(CoordinateFactory.Create3D(coordinate))
                       : Transform(CoordinateFactory.Create(coordinate));
        }

        public override IEnumerable<ICoordinate> Transform(IEnumerable<ICoordinate> points)
        {
            foreach (ICoordinate coordinate in points)
            {
                yield return Transform(coordinate);
            }
        }

        public override ICoordinateSequence Transform(ICoordinateSequence points)
        {
            ICoordinateSequence<TCoordinate> typedPoints = points as ICoordinateSequence<TCoordinate>;

            if (points != null)
            {
                return Transform(typedPoints);
            }

            ICoordinateSequenceFactory factory =
                points.CoordinateSequenceFactory;

            return factory.Create(Transform(points as IEnumerable<ICoordinate>));
        }

        public override ICoordinateSequence<TCoordinate> Transform(ICoordinateSequence<TCoordinate> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            ICoordinateSequenceFactory<TCoordinate> factory =
                points.CoordinateSequenceFactory;

            return factory.Create(Transform(points as IEnumerable<TCoordinate>));
        }

        public Boolean EqualParams(IInfo obj)
        {
            MapProjection<TCoordinate> m = obj as MapProjection<TCoordinate>;

            if (ReferenceEquals(m, null))
            {
                return true;
            }

            if (m.ParameterCount != ParameterCount)
            {
                return false;
            }

            foreach (ProjectionParameter parameter in m)
            {
                ProjectionParameter found = null;

                foreach (Parameter localParam in Parameters)
                {
                    if (localParam.Name.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        found = localParam as ProjectionParameter;
                        break;
                    }
                }

                if (found == null)
                {
                    return false;
                }

                if (found.Value != parameter.Value)
                {
                    return false;
                }
            }

            return IsInverse == m.IsInverse;
        }

        #endregion

        #region Helper mathematical functions

        // The following functions are largely ported from CPROJ.C, which came from GCTP:
        // the General Cartographic Transformation Package from the NOAA.
        //
        // From CPROJ.C:
        //
        /*******************************************************************************
            NAME                Projection support routines listed below.

            PURPOSE:	The following functions are included in CPROJ.C.

		            SINCOS:	  Calculates the sine and cosine.
		            ASINZ:	  Eliminates roundoff errors.
		            MSFNZ:	  Computes the constant small m for Oblique Equal Area.
		            QSFNZ:	  Computes the constant small q for Oblique Equal Area.
		            PHI1Z:	  Computes phi1 for Albers Conical Equal-Area.
		            PHI2Z:	  Computes the latitude angle, phi2, for Lambert
			              Conformal Conic and Polar Stereographic.
		            PHI3Z:	  Computes the latitude, phi3, for Equidistant Conic.
		            PHI4Z:	  Computes the latitude, phi4, for Polyconic.
		            PAKCZ:	  Converts a 2 digit alternate packed DMS format to
			              standard packed DMS format.
		            PAKR2DM:  Converts radians to 3 digit packed DMS format.
		            TSFNZ:	  Computes the small t for Lambert Conformal Conic and
			              Polar Stereographic.
		            SIGN:	  Returns the sign of an argument.
		            ADJUST_LON:  Adjusts a longitude angle to range -180 to 180.
		            E0FN, E1FN, E2FN, E3FN:
			              Computes the constants e0,e1,e2,and e3 for
			              calculating the distance along a meridian.
		            E4FN:	  Computes e4 used for Polar Stereographic.
		            MLFN:	  Computes M, the distance along a meridian.
		            CALC_UTM_ZONE:	Calculates the UTM zone number.

            PROGRAMMER              DATE		REASON
            ----------              ----		------
            D. Steinwand, EROS      July, 1991	Initial development
            T. Mittan, EROS		    May, 1993	Modified from Fortran GCTP for C GCTP
            S. Nelson, EROS		    June, 1993	Added inline comments
            S. Nelson, EROS		    Nov, 1993	Added loop counter in ADJUST_LON
            S. Nelson, EROS		    Jan, 1998	Changed misspelled error message

        *******************************************************************************/


        // defines some usefull constants that are used in the projection routines
        /// <summary>
        /// Shortcut constant for π, the ratio of the circumferance of a circle to its diameter.
        /// </summary>
        protected const Double PI = Math.PI;

        /// <summary>
        /// π * 0.5 
        /// </summary>
        protected const Double HalfPI = (PI * 0.5);

        /// <summary>
        /// π * 2
        /// </summary>
        protected const Double TwoPI = (PI * 2.0);

        /// <summary>
        /// The smallest tolerable difference between two real numbers. Differences
        /// smaller than this number are considered to be equal to zero.
        /// </summary>
        protected const Double Epsilon = 1.0e-10;

        /// <summary>
        /// Number of radians in one second.
        /// </summary>
        protected const Double SecondsToRadians = 4.848136811095359e-6;

        /// <summary>
        /// The maximum number of iterations for normalizing longitude.
        /// </summary>
        protected const Double MaxIterationCount = 20;

        /// <summary>
        /// The maximum positive value of an <see cref="Int32"/>, as a <see cref="Double"/>.
        /// </summary>
        protected const Double MaxInt32 = 2147483647;

        /// <summary>
        /// Approximately <see cref="Int64.MaxValue"/> / 2, or 4611686018427387903L, as a 
        /// <see cref="Double"/>.
        /// </summary>
        /// <value><see cref="Int64.MaxValue"/> / 2</value>
        protected const Double HalfMaxInt64 = 4.61168601e18;

        /// <summary>
        /// Returns the cube of a number.
        /// </summary>
        /// <param name="x">The number to raise to the power of 3.</param>
        protected static Double Cube(Double x)
        {
            return Math.Pow(x, 3); /* x^3 */
        }

        /// <summary>
        /// Returns the quad of a number.
        /// </summary>
        /// <param name="x">The number to raise to the power of 4.</param>
        protected static Double Quad(Double x)
        {
            return Math.Pow(x, 4); /* x^4 */
        }

        protected static Double GMax(ref Double a, ref Double b)
        {
            // NOTE: why are the parameters "ref" here?
            return Math.Max(a, b); /* assign maximum of a and b */
        }

        protected static Double GMin(ref Double a, ref Double b)
        {
            // NOTE: why are the parameters "ref" here?
            return ((a) < (b) ? (a) : (b)); /* assign minimum of a and b */
        }

        protected static Double IMod(Double a, Double b)
        {
            return (a) - (((a) / (b)) * (b)); /* Integer mod function */
        }

        /// <summary>
        /// Function to return the sign of an argument.
        /// </summary>
        protected static Double Sign(Double x)
        {
            return x < 0.0
                       ? -1
                       : 1;
        }

        protected static Double AdjustLongitude(Double x)
        {
            long count = 0;

            while (count <= MaxIterationCount && Math.Abs(x) > PI)
            {
                if (((Int64)Math.Abs(x / Math.PI)) < 2)
                {
                    x = x - (Sign(x) * TwoPI);
                }
                else if (((Int64)Math.Abs(x / TwoPI)) < MaxInt32)
                {
                    x = x - (((Int64)(x / TwoPI)) * TwoPI);
                }
                else if (((Int64)Math.Abs(x / (MaxInt32 * TwoPI))) < MaxInt32)
                {
                    x = x - (((Int64)(x / (MaxInt32 * TwoPI))) * (TwoPI * MaxInt32));
                }
                else if (((Int64)Math.Abs(x / (HalfMaxInt64 * TwoPI))) < MaxInt32)
                {
                    x = x - (((Int64)(x / (HalfMaxInt64 * TwoPI))) * (TwoPI * HalfMaxInt64));
                }
                else
                {
                    x = x - (Sign(x) * TwoPI);
                }

                count++;
            }

            return (x);
        }

        /// <summary>
        /// Function to compute the constant 'small m' which is the radius of
        /// a parallel of latitude, φ, divided by the semi-major axis.
        /// </summary>
        protected static Double ComputeSmallM(Double eccentricity, Double sinPhi, Double cosPhi)
        {
            Double con = eccentricity * sinPhi;

            return ((cosPhi / (Math.Sqrt(1.0 - con * con))));
        }

        /// <summary>
        /// Function to compute constant 'small q', used in the
        /// forward computation for Albers Conical Equal-Area projection.
        /// </summary>
        protected static Double ComputeSmallQ(Double eccentricity, Double sinPhi)
        {
            Double q;

            if (eccentricity > 1.0e-7)
            {
                Double con = eccentricity * sinPhi;

                q = (1.0 - eccentricity * eccentricity) *
                    (sinPhi / (1.0 - con * con) -
                    (0.5 / eccentricity) *
                    Math.Log((1.0 - con) / (1.0 + con)));
            }
            else
            {
                q = 2.0 * sinPhi;
            }

            return q;
        }

        /// <summary>
        /// Function to calculate the sine and cosine in one call.
        /// </summary>
        /// <param name="radians">The radians to compute the sine and cosine of.</param>
        /// <param name="sinValue">The sine value of <paramref name="radians"/>.</param>
        /// <param name="cosValue">The cosine value of <paramref name="radians"/>.</param>
        /// <remarks>
        /// <para>
        /// From CPROJ.C:
        /// </para>
        /// <blockquote>
        /// Some computer systems have implemented this function, resulting in a faster implementation
        /// than calling each function separately.  It is provided here for those
        /// computer systems which don't implement this function.
        /// </blockquote>
        /// <para>
        /// The current CLR has obviously not done this yet, but it may at some point...
        /// </para>
        /// </remarks>
        protected static void SinCos(Double radians, out Double sinValue, out Double cosValue)
        {
            sinValue = Math.Sin(radians);
            cosValue = Math.Cos(radians);
        }

        /// <summary>
        /// Function to compute the constant 'small t' for use in the forward
        /// computations in the Lambert Conformal Conic and the Polar
        /// Stereographic projections.
        /// </summary>
        protected static Double ComputeSmallT(Double eccentricity, Double phi, Double sinphi)
        {
            Double con = eccentricity * sinphi;
            Double com = 0.5 * eccentricity;
            con = Math.Pow(((1.0 - con) / (1.0 + con)), com);
            return Math.Tan(0.5 * (HalfPI - phi)) / con;
        }

        /// <summary>
        /// Computes latitude in inverse of Albers Equal-Area.
        /// </summary>
        /// <param name="eccentricity">The eccentricity of the ellipsoid.</param>
        /// <param name="qs">The input angle in radians as computed by <see cref="ComputeSmallQ"/>.</param>
        /// <remarks>
        /// Through an iterative procedure, this function computes the latitude 
        /// angle PHI1. PHI1 is the equivalent of the latitude PHI for the 
        /// inverse of the Albers Conical Equal-Area projection.
        /// All values are <see cref="Double"/> and all angular values are in radians.
        /// </remarks>
        /// <returns>The inverse of the Albers Conical Equal-Area projection</returns>
        protected static Double ComputePhi1(Double eccentricity, Double qs)
        {
            Int64 i;

            Double phi = Asin(0.5 * qs);

            if (eccentricity < Epsilon)
            {
                return phi;
            }

            Double e2 = eccentricity * eccentricity;

            for (i = 1; i <= MaxIterationCount; i++)
            {
                Double sinpi;
                Double cospi;

                SinCos(phi, out sinpi, out cospi);
                Double con = eccentricity * sinpi;
                Double com = 1.0 - con * con;
                Double dphi = 0.5 * com * com / cospi * (qs / (1.0 - e2) - sinpi / com +
                                                         0.5 / eccentricity * Math.Log((1.0 - con) / (1.0 + con)));
                phi = phi + dphi;

                if (Math.Abs(dphi) <= 1e-7)
                {
                    return phi;
                }
            }

            throw ComputationalConvergenceError();
        }

        /// <summary>
        /// Function to eliminate roundoff errors in an arcsine computation.
        /// </summary>
        protected static Double Asin(Double con)
        {
            if (Math.Abs(con) > 1.0)
            {
                if (con > 1.0)
                {
                    con = 1.0;
                }
                else
                {
                    con = -1.0;
                }
            }

            return Math.Asin(con);
        }

        /// <summary>
        /// Function to compute the latitude angle, phi2, for the inverse of the
        /// Lambert Conformal Conic and Polar Stereographic projections.
        /// </summary>
        /// <param name="eccentricity">The eccentricity of the ellipsoid.</param>
        /// <param name="ts">The constant "t" as computed by <see cref="ComputeSmallT"/>.</param>
        /// <remarks>
        /// The latitude PHI2 is computed using an iterative procedure. PHI2 is 
        /// PHI for the inverse of the Lambert Conformal Conic and Polar 
        /// Stereographic projections.
        /// All real variables are <see cref="Double"/>.
        /// </remarks>
        protected static Double ComputePhi2(Double eccentricity, Double ts)
        {
            Int64 i;

            Double halfEccentricity = 0.5 * eccentricity;
            Double chi = HalfPI - 2 * Math.Atan(ts);

            for (i = 0; i <= MaxIterationCount; i++)
            {
                Double con;
                Double dphi;
                Double sinpi;
                sinpi = Math.Sin(chi);
                con = eccentricity * sinpi;
                dphi = HalfPI - 2 *
                       Math.Atan(ts * Math.Pow(((1.0 - con) / (1.0 + con)), halfEccentricity)) -
                       chi;

                chi += dphi;

                if (Math.Abs(dphi) <= .0000000001)
                {
                    return chi;
                }
            }

            throw ComputationalConvergenceError();
        }

        // Functions to compute the constants e0, e1, e2, and e3 which are used
        // in a series for calculating the distance along a meridian.

        /// <summary>
        /// Computes constant "e0" for distance along meridian.
        /// </summary>
        /// <returns>
        /// The value "e0" which is used in a series to calculate a distance along a meridian.
        /// </returns>
        protected static Double ComputeE0(Double eccSquared)
        {
            return (1.0 - 0.25 * eccSquared * (1.0 + eccSquared / 16.0 * (3.0 + 1.25 * eccSquared)));
        }

        /// <summary>
        /// Computes constant "e1" for distance along meridian.
        /// </summary>
        /// <param name="eccSquared">The eccentricity of an ellipsoid, squared.</param>
        /// <returns>
        /// The value "e1" which is used in a series to calculate a distance along a meridian.
        /// </returns>
        protected static Double ComputeE1(Double eccSquared)
        {
            return (0.375 * eccSquared * (1.0 + 0.25 * eccSquared * (1.0 + 0.46875 * eccSquared)));
        }

        /// <summary>
        /// Computes constant "e2" for distance along meridian.
        /// </summary>
        /// <param name="eccSquared">The eccentricity of an ellipsoid, squared.</param>
        /// <returns>
        /// The value "e2" which is used in a series to calculate a distance along a meridian.
        /// </returns>
        protected static Double ComputeE2(Double eccSquared)
        {
            return (0.05859375 * eccSquared * eccSquared * (1.0 + 0.75 * eccSquared));
        }

        /// <summary>
        /// Computes constant "e3" for distance along meridian.
        /// </summary>
        /// <param name="eccSquared">The eccentricity of an ellipsoid, squared.</param>
        /// <returns>
        /// The value "e3" which is used in a series to calculate a distance along a meridian.
        /// </returns>
        protected static Double ComputeE3(Double eccSquared)
        {
            return (eccSquared * eccSquared * eccSquared * (35.0 / 3072.0));
        }

        /// <summary>
        /// Function to compute the constant e4 from the input of the eccentricity
        /// of the ellipsoid, x.  This constant is used in the Polar Stereographic
        /// projection.
        /// </summary>
        protected static Double ComputeE4(Double x)
        {
            Double con = 1.0 + x;
            Double com = 1.0 - x;
            return (Math.Sqrt((Math.Pow(con, con)) * (Math.Pow(com, com))));
        }

        /// <summary>
        /// Function computes the value of M which is the distance along a meridian
        /// from the Equator to latitude <paramref name="phi"/>.
        /// </summary>
        /// <param name="e0">The first eccentricity computation in the series, as computed by <see cref="ComputeE0"/>.</param>
        /// <param name="e1">The second eccentricity computation in the series, as computed by <see cref="ComputeE1"/>.</param>
        /// <param name="e2">The third eccentricity computation in the series, as computed by <see cref="ComputeE2"/>.</param>
        /// <param name="e3">The fourth eccentricity computation in the series, as computed by <see cref="ComputeE3"/>.</param>
        /// <param name="phi">The measure of the latitude to measure to, in radians.</param>
        protected static Double MeridianLength(Double e0, Double e1, Double e2, Double e3, Double phi)
        {
            return e0 * phi -
                   e1 * Math.Sin(2.0 * phi) +
                   e2 * Math.Sin(4.0 * phi) -
                   e3 * Math.Sin(6.0 * phi);
        }

        /// <summary>
        /// Function computes the value of M which is the distance along a meridian
        /// from the Equator to latitude <paramref name="phi"/>.
        /// </summary>
        /// <param name="phi">The measure of the latitude to measure to, in radians.</param>
        protected Double MeridianLength(Radians phi)
        {
            if ( _damTool == null )
                _damTool = new DistanceAlongMeridianTool(E2);

            return _damTool.Length(phi);
        }

        /// <summary>
        /// Function to calculate UTM zone number from degrees longitude.
        /// </summary>
        /// <param name="degreesLongitude">Longitude to find UTM zone for in degrees.</param>
        protected static long UtmZoneFromDegreesLongitude(Double degreesLongitude)
        {
            return (Int64)(((degreesLongitude + 180.0) / 6.0) + 1.0);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Converts a longitude value in degrees to radians.
        /// </summary>
        /// <param name="x">The value in degrees to convert to radians.</param>
        /// <param name="edge">If true, -180 and +180 are valid, otherwise they are considered out of range.</param>
        /// <returns></returns>
        protected static Radians LongitudeToRadians(Degrees x, Boolean edge)
        {
            if (edge ? ((Double)x >= -180 && (Double)x <= 180) : ((Double)x > -180 && (Double)x < 180))
            {
                return (Radians)x;
            }

            throw new ArgumentOutOfRangeException("x", x, "Not a valid longitude in degrees.");
        }

        /// <summary>
        /// Converts a latitude value in degrees to radians.
        /// </summary>
        /// <param name="y">The value in degrees to to radians.</param>
        /// <param name="edge">
        /// If true, -90 and +90 are valid, otherwise they are considered out of range.
        /// </param>
        /// <returns></returns>
        protected static Radians LatitudeToRadians(Degrees y, Boolean edge)
        {
            if (edge ? ((Double)y >= -90 && (Double)y <= 90) : ((Double)y > -90 && (Double)y < 90))
            {
                return (Radians)y;
            }

            throw new ArgumentOutOfRangeException("y", y, "Not a valid longitude in degrees.");
        }

        protected static Exception ComputationalConvergenceError()
        {
            return new ComputationException(String.Format("Failed to converge " +
                                                                     "after {0} iterations.",
                                                                     MaxIterationCount));
        }
        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}