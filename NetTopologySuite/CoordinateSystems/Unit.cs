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
    /// Class for defining units.
    /// </summary>
    public class Unit : Info, IUnit
    {
        private readonly Double _conversionFactor;

        /// <summary>
        /// Initializes a new unit.
        /// </summary>
        /// <param name="conversionFactor">Conversion factor to base unit.</param>
        /// <param name="name">Name of unit.</param>
        /// <param name="authority">Authority name.</param>
        /// <param name="authorityCode">Authority-specific identification code.</param>
        /// <param name="alias">Alias.</param>
        /// <param name="abbreviation">Abbreviation.</param>
        /// <param name="remarks">Provider-supplied remarks.</param>
        protected internal Unit(Double conversionFactor, String name, String authority, String authorityCode,
                                String alias,
                                String abbreviation, String remarks)
            : base(name, authority, authorityCode, alias, abbreviation, remarks)
        {
            _conversionFactor = conversionFactor;
        }

        /// <summary>
        /// Initializes a new unit
        /// </summary>
        /// <param name="name">Name of unit</param>
        /// <param name="conversionFactor">Conversion factor to base unit</param>
        protected internal Unit(Double conversionFactor, String name)
            : this(conversionFactor, name, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty) {}

        /// <summary>
        /// Gets or sets the number of units per base-unit.
        /// </summary>
        public Double ConversionFactor
        {
            get { return _conversionFactor; }
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
                sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "UNIT[\"{0}\", {1}", Name, _conversionFactor);
                
                if (!String.IsNullOrEmpty(Authority) && !String.IsNullOrEmpty(AuthorityCode))
                {
                    sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);
                }
                
                sb.Append("]");
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets an XML representation of this object [NOT IMPLEMENTED].
        /// </summary>
        public override String Xml
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Tests if two instances of unit contain the same values
        /// </summary>
        public override Boolean EqualParams(IInfo other)
        {
            Unit u = other as Unit;

            if (ReferenceEquals(u, null))
            {
                return false;
            }

            return u.ConversionFactor == ConversionFactor;
        }
    }
}