// This code has lifted from ProjNet project code base, and the namespaces 
// updated to fit into NetTopologySuit. This is an interim measure, so that 
// ProjNet can be removed from Sharpmap. This code is to be refactor / written
//  to use the DotSpiatial project library.

// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of GeoAPI.
// GeoAPI is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// GeoAPI is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeoAPI; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Globalization;
using GeoAPI.CoordinateSystems;

namespace NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// Details of axis. This is used to label axes, and indicate the orientation.
    /// </summary>
    // TODO: why doesn't this implement IInfo?
    public class AxisInfo : IAxisInfo
    {
        private readonly String _name;
        private readonly AxisOrientation _orientation;

        /// <summary>
        /// Initializes a new instance of an AxisInfo.
        /// </summary>
        /// <param name="name">Name of axis</param>
        /// <param name="orientation">Axis orientation</param>
        public AxisInfo(AxisOrientation orientation, String name)
        {
            _name = name;
            _orientation = orientation;
        }

        /// <summary>
        /// Human readable name for axis. 
        /// Possible values are X, Y, Long, Lat or any other short string.
        /// </summary>
        public String Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets enumerated value for orientation.
        /// </summary>
        public AxisOrientation Orientation
        {
            get { return _orientation; }
        }

        /// <summary>
        /// Returns the Well-Known Text for this object
        /// as defined in the simple features specification.
        /// </summary>
        public String Wkt
        {
            get
            {
                return String.Format("AXIS[\"{0}\", {1}]", Name,
                                     Orientation.ToString().ToUpper(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets an XML representation of this object.
        /// </summary>
        public String Xml
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture.NumberFormat,
                                     "<CS_AxisInfo Name=\"{0}\" Orientation=\"{1}\"/>", Name,
                                     Orientation.ToString().ToUpper(CultureInfo.InvariantCulture));
            }
        }
    }
}