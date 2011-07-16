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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// A 3D coordinate system, with its origin at the center of the Earth.
    /// </summary>
    public class GeocentricCoordinateSystem<TCoordinate> : CoordinateSystem<TCoordinate>,
                                                           IGeocentricCoordinateSystem<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly IHorizontalDatum _horizontalDatum;
        private readonly ILinearUnit _linearUnit;
        private readonly IPrimeMeridian _primeMeridan;

        protected internal GeocentricCoordinateSystem(IExtents<TCoordinate> extents,
                                                      IHorizontalDatum datum, ILinearUnit linearUnit,
                                                      IPrimeMeridian primeMeridian,
                                                      IEnumerable<IAxisInfo> axisinfo, String name, String authority,
                                                      String authorityCode, String alias, String remarks, String abbreviation)
            : base(extents, name, authority, authorityCode, alias, abbreviation, remarks)
        {
            _horizontalDatum = datum;
            _linearUnit = linearUnit;
            _primeMeridan = primeMeridian;

            foreach (AxisInfo info in axisinfo)
            {
                AxisInfo.Add(info);
            }

            if (AxisInfo.Count != 3)
            {
                throw new ArgumentException("Axis info should contain three axes for geocentric coordinate systems");
            }
        }

        #region IGeocentricCoordinateSystem Members

        /// <summary>
        /// Returns the HorizontalDatum. The horizontal datum is used to determine where
        /// the center of the Earth is considered to be. All coordinate points will be 
        /// measured from the center of the Earth, and not the surface.
        /// </summary>
        public IHorizontalDatum HorizontalDatum
        {
            get { return _horizontalDatum; }
        }

        /// <summary>
        /// Gets the units used along all the axes.
        /// </summary>
        public ILinearUnit LinearUnit
        {
            get { return _linearUnit; }
        }

        /// <summary>
        /// Gets units for dimension within coordinate system. Each dimension in 
        /// the coordinate system has corresponding units.
        /// </summary>
        /// <param name="dimension">Dimension</param>
        /// <returns>Unit</returns>
        public override IUnit GetUnits(Int32 dimension)
        {
            return _linearUnit;
        }

        /// <summary>
        /// Returns the PrimeMeridian.
        /// </summary>
        public IPrimeMeridian PrimeMeridian
        {
            get { return _primeMeridan; }
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
                sb.AppendFormat("GEOCCS[\"{0}\", {1}, {2}, {3}", Name, HorizontalDatum.Wkt, PrimeMeridian.Wkt,
                                LinearUnit.Wkt);

                // Skip axis info if they contain default values				
                if (AxisInfo.Count != 3 ||
                    AxisInfo[0].Name != "X" || AxisInfo[0].Orientation != AxisOrientation.Other ||
                    AxisInfo[1].Name != "Y" || AxisInfo[1].Orientation != AxisOrientation.East ||
                    AxisInfo[2].Name != "Z" || AxisInfo[2].Orientation != AxisOrientation.North)
                {
                    foreach (AxisInfo axis in AxisInfo)
                    {
                        sb.AppendFormat(", {0}", axis.Wkt);
                    }
                }

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
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat,
                                "<CS_CoordinateSystem Dimension=\"{0}\"><CS_GeocentricCoordinateSystem>{1}",
                                Dimension, InfoXml);

                foreach (AxisInfo ai in AxisInfo)
                {
                    sb.Append(ai.Xml);
                }

                sb.AppendFormat("{0}{1}{2}</CS_GeocentricCoordinateSystem></CS_CoordinateSystem>",
                                HorizontalDatum.Xml, LinearUnit.Xml, PrimeMeridian.Xml);

                return sb.ToString();
            }
        }

        public override Boolean EqualParams(IInfo other)
        {
            GeocentricCoordinateSystem<TCoordinate> g =
                other as GeocentricCoordinateSystem<TCoordinate>;

            if (ReferenceEquals(g, null))
            {
                return false;
            }

            return g.HorizontalDatum.EqualParams(HorizontalDatum) &&
                   g.LinearUnit.EqualParams(LinearUnit) &&
                   g.PrimeMeridian.EqualParams(PrimeMeridian);
        }

        #endregion
    }
}