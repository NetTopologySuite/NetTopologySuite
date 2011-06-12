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

using System;
using System.Globalization;
using System.Text;
using GeoAPI.CoordinateSystems;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// The IEllipsoid interface defines the standard information stored with ellipsoid objects.
    /// </summary>
    public class Ellipsoid : Info, IEllipsoid
    {
        private readonly Boolean _isIvfDefinitive;
        private readonly ILinearUnit _axisUnit;
        private readonly Double _inverseFlattening;
        private readonly Double _semiMinorAxis;
        private readonly Double _semiMajorAxis;

        /// <summary>
        /// Initializes a new instance of an <see cref="Ellipsoid"/>.
        /// </summary>
        /// <param name="semiMajorAxis">Semi major axis.</param>
        /// <param name="semiMinorAxis">Semi minor axis.</param>
        /// <param name="inverseFlattening">Inverse flattening.</param>
        /// <param name="isIvfDefinitive">
        /// Inverse Flattening is definitive for this ellipsoid 
        /// (Semi-minor axis will be overridden).
        /// </param>
        /// <param name="axisUnit">Axis unit.</param>
        /// <param name="name">Name of the ellipsoid.</param>
        /// <param name="authority">Authority name.</param>
        /// <param name="authorityCode">Authority-specific identification code.</param>
        /// <param name="alias">Alias.</param>
        /// <param name="abbreviation">Abbreviation.</param>
        /// <param name="remarks">Provider-supplied remarks.</param>
        internal Ellipsoid(Double semiMajorAxis, Double semiMinorAxis, Double inverseFlattening,
                           Boolean isIvfDefinitive, ILinearUnit axisUnit, String name, String authority,
                           String authorityCode, String alias, String abbreviation, String remarks)
            : base(name, authority, authorityCode, alias, abbreviation, remarks)
        {
            _semiMajorAxis = semiMajorAxis;
            _inverseFlattening = inverseFlattening;
            _axisUnit = axisUnit;
            _isIvfDefinitive = isIvfDefinitive;

            if (isIvfDefinitive && (inverseFlattening == 0 || Double.IsInfinity(inverseFlattening)))
            {
                _semiMinorAxis = semiMajorAxis;
            }
            else if (isIvfDefinitive)
            {
                _semiMinorAxis = (1.0 - (1.0 / _inverseFlattening)) * semiMajorAxis;
            }
            else
            {
                _semiMinorAxis = semiMinorAxis;
            }
        }

        #region Predefined ellipsoids

        /// <summary>
        /// WGS 84 ellipsoid
        /// </summary>
        /// <remarks>
        /// Inverse flattening derived from four defining parameters 
        /// (semi-major axis;
        /// C20 = -484.16685*10e-6;
        /// earth's angular velocity w = 7292115e11 rad/sec;
        /// gravitational constant GM = 3986005e8 m*m*m/s/s).
        /// </remarks>
        public static Ellipsoid Wgs84
        {
            get
            {
                return new Ellipsoid(6378137, 0, 298.257223563, 
                                     true, LinearUnit.Meter, "WGS 84", 
                                     "EPSG", "7030", "WGS84", String.Empty,
                                     "Inverse flattening derived from four defining "+
                                     "parameters (semi-major axis; "+
                                     "C20 = -484.16685*10e-6; earth's angular velocity "+
                                     "w = 7292115e11 rad/sec; gravitational constant "+
                                     "GM = 3986005e8 m*m*m/s/s).");
            }
        }

        /// <summary>
        /// WGS 72 Ellipsoid
        /// </summary>
        public static Ellipsoid Wgs72
        {
            get
            {
                return new Ellipsoid(6378135.0, 0, 298.26, 
                                     true, LinearUnit.Meter, 
                                     "WGS 72", "EPSG", "7043", "WGS 72",
                                     String.Empty, String.Empty);
            }
        }

        /// <summary>
        /// GRS 1980 / International 1979 ellipsoid
        /// </summary>
        /// <remarks>
        /// Adopted by IUGG 1979 Canberra.
        /// Inverse flattening is derived from
        /// geocentric gravitational constant GM = 3986005e8 m*m*m/s/s;
        /// dynamic form factor J2 = 108263e8 and Earth's angular velocity = 7292115e-11 rad/s.")
        /// </remarks>
        public static Ellipsoid Grs80
        {
            get
            {
                return new Ellipsoid(6378137, 0, 298.257222101,
                                     true, LinearUnit.Meter,
                                     "GRS 1980", "EPSG", "7019",
                                     "International 1979", String.Empty,
                                     "Adopted by IUGG 1979 Canberra.  " +
                                     "Inverse flattening is derived from geocentric " +
                                     "gravitational constant GM = 3986005e8 m*m*m/s/s; " +
                                     "dynamic form factor J2 = 108263e8 and Earth's " +
                                     "angular velocity = 7292115e-11 rad/s.");
            }
        }

        /// <summary>
        /// International 1924 / Hayford 1909 ellipsoid
        /// </summary>
        /// <remarks>
        /// Described as a=6378388 m. and b=6356909m. from which 1/f derived to be 296.95926. 
        /// The figure was adopted as the International ellipsoid in 1924 but with 1/f taken as
        /// 297 exactly from which b is derived as 6356911.946m.
        /// </remarks>
        public static Ellipsoid International1924
        {
            get
            {
                return new Ellipsoid(6378388, 0, 297,
                                     true, LinearUnit.Meter,
                                     "International 1924", "EPSG", "7022",
                                     "Hayford 1909", String.Empty,
                                     "Described as a=6378388 m. and b=6356909 m. " +
                                     "from which 1/f derived to be 296.95926. The " +
                                     "figure was adopted as the International ellipsoid " +
                                     "in 1924 but with 1/f taken as 297 exactly from " +
                                     "which b is derived as 6356911.946m.");
            }
        }

        /// <summary>
        /// Clarke 1880
        /// </summary>
        /// <remarks>
        /// Clarke gave a and b and also 1/f=293.465 (to 3 decimal places).  1/f derived from a and b = 293.4663077
        /// </remarks>
        public static Ellipsoid Clarke1880
        {
            get
            {
                return new Ellipsoid(20926202, 0, 297,
                                     true, LinearUnit.ClarkesFoot,
                                     "Clarke 1880", "EPSG", "7034",
                                     "Clarke 1880", String.Empty,
                                     "Clarke gave a and b and also 1/f=293.465 " +
                                     "(to 3 decimal places).  1/f derived from a " +
                                     "and b = 293.4663077…");
            }
        }

        /// <summary>
        /// Clarke 1866
        /// </summary>
        /// <remarks>
        /// Original definition a=20926062 and b=20855121 (British) feet. Uses Clarke's 1865 inch-metre ratio of 39.370432 to obtain metres. (Metric value then converted to US survey feet for use in the United States using 39.37 exactly giving a=20925832.16 ft US).
        /// </remarks>
        public static Ellipsoid Clarke1866
        {
            get
            {
                return new Ellipsoid(6378206.4, 6356583.8, Double.PositiveInfinity,
                                     false, LinearUnit.Meter, "Clarke 1866",
                                     "EPSG", "7008", "Clarke 1866", String.Empty,
                                     "Original definition a=20926062 and b=20855121 " +
                                     "(British) feet. Uses Clarke's 1865 inch-metre " +
                                     "ratio of 39.370432 to obtain metres. (Metric " +
                                     "value then converted to US survey feet for " +
                                     "use in the United States using 39.37 exactly " +
                                     "giving a=20925832.16 ft US).");
            }
        }

        /// <summary>
        /// Sphere
        /// </summary>
        /// <remarks>
        /// Authalic sphere derived from GRS 1980 ellipsoid (code 7019).  (An authalic sphere is
        /// one with a surface area equal to the surface area of the ellipsoid). 1/f is infinite.
        /// </remarks>
        public static Ellipsoid Sphere
        {
            get
            {
                return new Ellipsoid(6370997.0, 6370997.0, Double.PositiveInfinity,
                                     false, LinearUnit.Meter, "GRS 1980 Authalic Sphere",
                                     "EPSG", "7048", "Sphere", String.Empty,
                                     "Authalic sphere derived from GRS 1980 ellipsoid " +
                                     "(code 7019).  (An authalic sphere is one with a " +
                                     "surface area equal to the surface area of the " +
                                     "ellipsoid). 1/f is infinite.");
            }
        }

        #endregion

        #region IEllipsoid Members

        /// <summary>
        /// Gets or sets the value of the semi-major axis.
        /// </summary>
        public Double SemiMajorAxis
        {
            get { return _semiMajorAxis; }
        }

        /// <summary>
        /// Gets or sets the value of the semi-minor axis.
        /// </summary>
        public Double SemiMinorAxis
        {
            get { return _semiMinorAxis; }
        }

        /// <summary>
        /// Gets or sets the value of the inverse of the flattening constant of the ellipsoid.
        /// </summary>
        public Double InverseFlattening
        {
            get { return _inverseFlattening; }
        }

        /// <summary>
        /// Gets or sets the value of the axis unit.
        /// </summary>
        public ILinearUnit AxisUnit
        {
            get { return _axisUnit; }
        }

        /// <summary>
        /// Tells if the Inverse Flattening is definitive for this ellipsoid. Some ellipsoids use 
        /// the IVF as the defining value, and calculate the polar radius whenever asked. Other
        /// ellipsoids use the polar radius to calculate the IVF whenever asked. This 
        /// distinction can be important to avoid floating-point rounding errors.
        /// </summary>
        public Boolean IsIvfDefinitive
        {
            get { return _isIvfDefinitive; }
        }

        /// <summary>
        /// Returns the Well-Known Text for this object
        /// as defined in the simple features specification.
        /// </summary>
        public override String Wkt
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat,
                                "SPHEROID[\"{0}\", {1}, {2}", Name, SemiMajorAxis, 
                                InverseFlattening);

                if (!String.IsNullOrEmpty(Authority) && !String.IsNullOrEmpty(AuthorityCode))
                {
                    sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);
                }

                sb.Append("]");
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets an XML representation of this object
        /// </summary>
        public override String Xml
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture.NumberFormat,
                                     "<CS_Ellipsoid SemiMajorAxis=\"{0}\" SemiMinorAxis=\"{1}\" " +
                                     "InverseFlattening=\"{2}\" IvfDefinitive=\"{3}\">{4}{5}</CS_Ellipsoid>",
                                     SemiMajorAxis, SemiMinorAxis, InverseFlattening, (IsIvfDefinitive ? 1 : 0),
                                     InfoXml, AxisUnit.Xml);
            }
        }

        #endregion

        public override Boolean EqualParams(IInfo other)
        {
            Ellipsoid e = other as Ellipsoid;

            if (ReferenceEquals(e, null))
            {
                return false;
            }

            return (e.InverseFlattening == InverseFlattening &&
                    e.IsIvfDefinitive == IsIvfDefinitive &&
                    e.SemiMajorAxis == SemiMajorAxis &&
                    e.SemiMinorAxis == SemiMinorAxis &&
                    e.AxisUnit.EqualParams(AxisUnit));
        }
    }
}