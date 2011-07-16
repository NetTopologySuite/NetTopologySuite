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

namespace NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// Definition of linear units.
    /// </summary>
    public class LinearUnit : Info, ILinearUnit
    {
        private readonly Double _metersPerUnit;

        /// <summary>
        /// Creates an instance of a linear unit
        /// </summary>
        /// <param name="metersPerUnit">Number of meters per <see cref="LinearUnit" /></param>
        /// <param name="name">Name</param>
        /// <param name="authority">Authority name</param>
        /// <param name="authorityCode">Authority-specific identification code.</param>
        /// <param name="alias">Alias</param>
        /// <param name="abbreviation">Abbreviation</param>
        /// <param name="remarks">Provider-supplied remarks</param>
        public LinearUnit(Double metersPerUnit, String name, String authority, String authorityCode, String alias,
                          String abbreviation, String remarks)
            : base(name, authority, authorityCode, alias, abbreviation, remarks)
        {
            _metersPerUnit = metersPerUnit;
        }

        #region Predefined units

        /// <summary>
        /// Returns the meters linear unit.
        /// Also known as International metre. SI standard unit.
        /// </summary>
        public static ILinearUnit Meter
        {
            get
            {
                return new LinearUnit(1.0, "metre", "EPSG", "9001", "m", String.Empty,
                                      "Also known as International metre. SI standard unit.");
            }
        }

        /// <summary>
        /// Returns the foot linear unit (1ft = 0.3048m).
        /// </summary>
        public static ILinearUnit Foot
        {
            get { return new LinearUnit(0.3048, "foot", "EPSG", "9002", "ft", String.Empty, String.Empty); }
        }

        /// <summary>
        /// Returns the US Survey foot linear unit (1ftUS = 0.304800609601219m).
        /// </summary>
        public static ILinearUnit USSurveyFoot
        {
            get
            {
                return
                    new LinearUnit(0.304800609601219, "US survey foot", "EPSG", "9003", "American foot", "ftUS",
                                   "Used in USA.");
            }
        }

        /// <summary>
        /// Returns the Nautical Mile linear unit (1NM = 1852m).
        /// </summary>
        public static ILinearUnit NauticalMile
        {
            get { return new LinearUnit(1852, "nautical mile", "EPSG", "9030", "NM", String.Empty, String.Empty); }
        }

        /// <summary>
        /// Returns Clarke's foot.
        /// </summary>
        /// <remarks>
        /// Assumes Clarke's 1865 ratio of 1 British foot = 0.3047972654 French legal metres applies to the international metre. 
        /// Used in older Australian, southern African &amp; British West Indian mapping.
        /// </remarks>
        public static ILinearUnit ClarkesFoot
        {
            get
            {
                return
                    new LinearUnit(0.3047972654, "Clarke's foot", "EPSG", "9005", "Clarke's foot", String.Empty,
                                   "Assumes Clarke's 1865 ratio of 1 British foot = 0.3047972654 French legal metres applies to the international metre. Used in older Australian, southern African & British West Indian mapping.");
            }
        }

        #endregion

        #region ILinearUnit Members

        /// <summary>
        /// Gets or sets the number of meters per <see cref="LinearUnit"/>.
        /// </summary>
        public Double MetersPerUnit
        {
            get { return _metersPerUnit; }
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
                sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "UNIT[\"{0}\", {1}", Name, MetersPerUnit);
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
                return
                    String.Format(CultureInfo.InvariantCulture.NumberFormat,
                                  "<CS_LinearUnit MetersPerUnit=\"{0}\">{1}</CS_LinearUnit>", MetersPerUnit, InfoXml);
            }
        }

        #endregion

        public override Boolean EqualParams(IInfo other)
        {
            LinearUnit l = other as LinearUnit;

            if (ReferenceEquals(l, null))
            {
                return false;
            }

            return l.MetersPerUnit == MetersPerUnit;
        }
    }
}