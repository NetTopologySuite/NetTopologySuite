// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
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

using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.DataStructures;
using NPack.Interfaces;
using GeoAPI.Units;

namespace NetTopologySuite.CoordinateSystems.Transformations
{
    internal class InverseGeocentricTransform<TCoordinate> : GeocentricTransform<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        /// <summary>
        /// Initializes an inverse geocentric projection object
        /// </summary>
        /// <param name="parameters">List of parameters to initialize the projection.</param>
        public InverseGeocentricTransform(IEnumerable<ProjectionParameter> parameters,
                                          ICoordinateFactory<TCoordinate> coordinateFactory,
                                          GeocentricTransform<TCoordinate> transform)
            : base(parameters, coordinateFactory)
        {
            Inverse = transform;
        }

        protected override String Name
        {
            get
            {
                return "Inverse Geocentric";
            }
        }

        public override Boolean IsInverse
        {
            get
            {
                return true;
            }
        }
    }

    /// <summary>
    /// Implements a geocentric transformation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Latitude, Longitude and ellipsoidal height in terms of a 3-dimensional geographic system
    /// may by expressed in terms of a geocentric (earth centered) Cartesian coordinate reference system
    /// X, Y, Z with the Z axis corresponding to the earth's rotation axis positive northwards, the X
    /// axis through the intersection of the prime meridian and equator, and the Y axis through
    /// the intersection of the equator with longitude 90 degrees east. The geographic and geocentric
    /// systems are based on the same geodetic datum.
    /// </para>
    /// <para>
    /// Geocentric coordinate reference systems are conventionally taken to be defined with the X
    /// axis through the intersection of the Greenwich meridian and equator. This requires that the equivalent
    /// geographic coordinate reference systems based on a non-Greenwich prime meridian should first be
    /// transformed to their Greenwich equivalent. Geocentric coordinates X, Y and Z take their units from
    /// the units of the ellipsoid axes (a and b). As it is conventional for X, Y and Z to be in metres,
    /// if the ellipsoid axis dimensions are given in another linear unit they should first be converted
    /// to metres.
    /// </para>
    /// </remarks>
    internal class GeocentricTransform<TCoordinate> : GeoMathTransform<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private const Double COS_67P5 = 0.38268343236508977; /* cosine of 67.5 degrees */
        private const Double AD_C = 1.0026000; /* Toms region 1 constant */

        private readonly Double _ses; // Second eccentricity squared : (a^2 - b^2)/b^2
        //private Double ab; // Semi_major / semi_minor
        //private Double ba; // Semi_minor / semi_major

        /// <summary>
        /// Initializes a geocentric projection object
        /// </summary>
        /// <param name="parameters">List of parameters to initialize the projection.</param>
        public GeocentricTransform(IEnumerable<ProjectionParameter> parameters,
                                   ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(Caster.Upcast<Parameter, ProjectionParameter>(parameters), coordinateFactory)
        {
            Double semiMinor = SemiMinor;
            _ses = (Math.Pow(SemiMajor, 2) - Math.Pow(semiMinor, 2)) / Math.Pow(semiMinor, 2);
        }

        /// <summary>
        /// Returns the inverse of this conversion.
        /// </summary>
        /// <returns>IMathTransform that is the reverse of the current conversion.</returns>
        protected override IMathTransform ComputeInverse(IMathTransform setAsInverse)
        {
            IEnumerable<ProjectionParameter> parameters =
                Caster.Downcast<ProjectionParameter, Parameter>(Parameters);

            return new InverseGeocentricTransform<TCoordinate>(parameters,
                                                               CoordinateFactory,
                                                               this);
        }

        /// <summary>
        /// Converts coordinates in decimal degrees to projected meters.
        /// </summary>
        /// <param name="lonlat">The point in decimal degrees.</param>
        /// <returns>Point in projected meters</returns>
        private TCoordinate degreesToMeters(TCoordinate lonlat)
        {
            Double lon = (Radians)new Degrees((Double)lonlat[0]);
            Double lat = (Radians)new Degrees((Double)lonlat[1]);
            Double h = lonlat.ComponentCount < 3
                           ? 0
                           : ((Double)lonlat[2]).Equals(Double.NaN)
                                 ? 0
                                 : (Double)lonlat[2];

            Double e2 = E2;
            Double v = SemiMajor / Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(lat), 2));
            Double x = (v + h) * Math.Cos(lat) * Math.Cos(lon);
            Double y = (v + h) * Math.Cos(lat) * Math.Sin(lon);
            Double z = ((1 - e2) * v + h) * Math.Sin(lat);
            return CreateCoordinate3D(x, y, z);
        }

        /// <summary>
        /// Converts coordinates in projected meters to decimal degrees.
        /// </summary>
        /// <param name="pnt">Point in meters</param>
        /// <returns>Transformed point in decimal degrees</returns>		
        private TCoordinate metersToDegrees(TCoordinate pnt)
        {
            Boolean atPole = false; // indicates whether location is in polar region */
            Double z = pnt.ComponentCount < 3
                           ? 0
                           : ((Double)pnt[2]).Equals(Double.NaN)
                                 ? 0
                                 : (Double)pnt[2];

            Radians lon;
            Radians lat = new Radians(0);
            Double height;

            const Double HalfPI = Math.PI * 0.5;

            Double x = (Double)pnt[0];
            Double y = (Double)pnt[1];

            if (x != 0.0)
            {
                lon = new Radians(Math.Atan2(y, x));
            }
            else
            {
                if (y > 0)
                {
                    lon = new Radians(HalfPI);
                }
                else if (y < 0)
                {
                    lon = new Radians(-HalfPI);
                }
                else
                {
                    atPole = true;
                    lon = new Radians(0);

                    if (z > 0.0)
                    {
                        /* north pole */
                        lat = new Radians(HalfPI);
                    }
                    else if (z < 0.0)
                    {
                        /* south pole */
                        lat = new Radians(-HalfPI);
                    }
                    else
                    {
                        /* center of earth */
                        return CreateCoordinate3D(new Degrees(lon),
                                                  (Degrees)new Radians(HalfPI),
                                                  -SemiMajor);
                    }
                }
            }

            Double semiMinor = SemiMinor;
            Double semiMajor = SemiMajor;

            Double w2 = x * x + y * y; // Square of distance from Z axis
            Double w = Math.Sqrt(w2); // distance from Z axis
            Double t0 = z * AD_C; // initial estimate of vertical component
            Double s0 = Math.Sqrt(t0 * t0 + w2); //initial estimate of horizontal component
            Double sinB0 = t0 / s0; //sin(B0), B0 is estimate of Bowring aux variable
            Double cosB0 = w / s0; //cos(B0)
            Double sin3B0 = Math.Pow(sinB0, 3);
            Double t1 = z + semiMinor * _ses * sin3B0; //corrected estimate of vertical component
            Double sum = w - semiMajor * E2 * cosB0 * cosB0 * cosB0; //numerator of cos(phi1)
            Double s1 = Math.Sqrt(t1 * t1 + sum * sum); //corrected estimate of horizontal component
            Double sinP1 = t1 / s1; //sin(phi1), phi1 is estimated latitude
            Double cosP1 = sum / s1; //cos(phi1)
            Double rn = semiMajor / Math.Sqrt(1.0 - E2 * sinP1 * sinP1); //Earth radius at location

            if (cosP1 >= COS_67P5)
            {
                height = w / cosP1 - rn;
            }
            else if (cosP1 <= -COS_67P5)
            {
                height = w / -cosP1 - rn;
            }
            else
            {
                height = z / sinP1 + rn * (E2 - 1.0);
            }

            if (!atPole)
            {
                lat = new Radians(Math.Atan(sinP1 / cosP1));
            }

            return CreateCoordinate3D((Degrees)lon, (Degrees)lat, height);
        }

        /// <summary>
        /// Transforms a coordinate point. The passed parameter point should not be modified.
        /// </summary>
        /// <param name="point">The coordinate to transform.</param>
        /// <returns>
        /// A new <typeparamref name="TCoordinate"/> which represents the transformed coordinate.
        /// </returns>
        public override TCoordinate Transform(TCoordinate point)
        {
            return !IsInverse
                       ? degreesToMeters(point)
                       : metersToDegrees(point);
        }

        /// <summary>
        /// Transforms a set of coordinates.
        /// </summary>
        /// <param name="points"></param>
        /// <returns>The enumeration of coordinate values after tranformation.</returns>
        /// <remarks>
        /// This method is provided for efficiently transforming many points.
        /// </remarks>
        public override IEnumerable<TCoordinate> Transform(IEnumerable<TCoordinate> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (!IsInverse)
            {
                foreach (TCoordinate point in points)
                    yield return degreesToMeters(point);
            }
            else
            {
                foreach (TCoordinate point in points)
                    yield return metersToDegrees(point);
            }
        }

        public override ICoordinateSequence<TCoordinate> Transform(ICoordinateSequence<TCoordinate> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            return points.CoordinateSequenceFactory.Create(Transform((IEnumerable<TCoordinate>)points));
        }

        public override Int32 SourceDimension
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Int32 TargetDimension
        {
            get { throw new System.NotImplementedException(); }
        }

        public override ICoordinate Transform(ICoordinate coordinate)
        {
            return !IsInverse
                       ? degreesToMeters(CoordinateFactory.Create3D(coordinate))
                       : metersToDegrees(CoordinateFactory.Create3D(coordinate));
        }

        public override IEnumerable<ICoordinate> Transform(IEnumerable<ICoordinate> points)
        {
            throw new System.NotImplementedException();
        }

        public override ICoordinateSequence Transform(ICoordinateSequence points)
        {
            throw new System.NotImplementedException();
        }

        public override Boolean IsInverse
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a Well-Known Text representation of this object.
        /// </summary>
        /// <value></value>
        public override String Wkt
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets an XML representation of this object.
        /// </summary>
        /// <value></value>
        public override String Xml
        {
            get { throw new NotImplementedException(); }
        }
    }
}