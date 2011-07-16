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
    /// A coordinate system based on latitude and longitude. 
    /// </summary>
    /// <remarks>
    /// Some geographic coordinate systems are Lat/Lon, and some are Lon/Lat. 
    /// You can find out which this is by examining the axes. You should also 
    /// check the angular units, since not all geographic coordinate systems 
    /// use degrees.
    /// </remarks>
    public class GeographicCoordinateSystem<TCoordinate> : HorizontalCoordinateSystem<TCoordinate>,
                                                           IGeographicCoordinateSystem<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly IAngularUnit _angularUnit;
        private readonly IPrimeMeridian _primeMeridian;
        private readonly List<Wgs84ConversionInfo> _wgs84ConversionInfo;

        /// <summary>
        /// Creates an instance of a Geographic Coordinate System
        /// </summary>
        /// <param name="angularUnit">Angular units</param>
        /// <param name="horizontalDatum">Horizontal datum</param>
        /// <param name="primeMeridian">Prime meridian</param>
        /// <param name="axisInfo">Axis info</param>
        /// <param name="name">Name</param>
        /// <param name="authority">Authority name</param>
        /// <param name="authorityCode">Authority-specific identification code.</param>
        /// <param name="alias">Alias</param>
        /// <param name="abbreviation">Abbreviation</param>
        /// <param name="remarks">Provider-supplied remarks</param>
        protected internal GeographicCoordinateSystem(IExtents<TCoordinate> extents,
                                                      IAngularUnit angularUnit, IHorizontalDatum horizontalDatum,
                                                      IPrimeMeridian primeMeridian, IEnumerable<IAxisInfo> axisInfo,
                                                      String name, String authority, String authorityCode, String alias,
                                                      String abbreviation, String remarks)
            : base(extents, horizontalDatum, axisInfo, name, authority, authorityCode, alias, abbreviation, remarks)
        {
            _angularUnit = angularUnit;
            _primeMeridian = primeMeridian;
        }

        #region Predefined geographic coordinate systems

        /// <summary>
        /// Creates a decimal degrees geographic coordinate system based on the WGS84 ellipsoid, suitable for GPS measurements
        /// </summary>
        public static GeographicCoordinateSystem<TCoordinate> GetWgs84(IGeometryFactory<TCoordinate> geoFactory)
        {
            AxisInfo[] axes = new AxisInfo[]
                {
                    new AxisInfo(AxisOrientation.East, "Lon"),
                    new AxisInfo(AxisOrientation.North, "Lat")
                };

            IExtents<TCoordinate> defaultExtents =
                (IExtents<TCoordinate>) geoFactory.CreateExtents2D(-180, -90, 180, 90);

            // TODO: DefaultEnvelope should be (-180, 180; -90, 90)
            return new GeographicCoordinateSystem<TCoordinate>(
                                            defaultExtents, 
                                            CoordinateSystems.AngularUnit.Degrees, 
                                            CoordinateSystems.HorizontalDatum.Wgs84, 
                                            CoordinateSystems.PrimeMeridian.Greenwich, 
                                            axes, "WGS 84", "EPSG", "4326", String.Empty, 
                                            String.Empty, String.Empty);
        
        }

        #endregion

        #region IGeographicCoordinateSystem Members

        /// <summary>
        /// Gets or sets the angular units of the geographic coordinate system.
        /// </summary>
        public IAngularUnit AngularUnit
        {
            get { return _angularUnit; }
        }

        /// <summary>
        /// Gets units for dimension within coordinate system. Each dimension in 
        /// the coordinate system has corresponding units.
        /// </summary>
        /// <param name="dimension">Dimension</param>
        /// <returns>Unit</returns>
        public override IUnit GetUnits(Int32 dimension)
        {
            return _angularUnit;
        }

        /// <summary>
        /// Gets or sets the prime meridian of the geographic coordinate system.
        /// </summary>
        public IPrimeMeridian PrimeMeridian
        {
            get { return _primeMeridian; }
        }

        /// <summary>
        /// Gets the number of available conversions to WGS84 coordinates.
        /// </summary>
        public Int32 ConversionToWgs84Count
        {
            get { return _wgs84ConversionInfo.Count; }
        }

        /// <summary>
        /// Gets details on a conversion to WGS84.
        /// </summary>
        public Wgs84ConversionInfo GetWgs84ConversionInfo(Int32 index)
        {
            return _wgs84ConversionInfo[index];
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
                sb.AppendFormat("GEOGCS[\"{0}\", {1}, {2}, {3}", Name, HorizontalDatum.Wkt, PrimeMeridian.Wkt,
                                AngularUnit.Wkt);
                //Skip axis info if they contain default values
                if (AxisInfo.Count != 2 ||
                    AxisInfo[0].Name != "Lon" || AxisInfo[0].Orientation != AxisOrientation.East ||
                    AxisInfo[1].Name != "Lat" || AxisInfo[1].Orientation != AxisOrientation.North)
                {
                    foreach (AxisInfo info in AxisInfo)
                    {
                        sb.AppendFormat(", {0}", info.Wkt);
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
                                "<CS_CoordinateSystem Dimension=\"{0}\"><CS_GeographicCoordinateSystem>{1}",
                                Dimension, InfoXml);

                foreach (AxisInfo ai in AxisInfo)
                {
                    sb.Append(ai.Xml);
                }

                sb.AppendFormat("{0}{1}{2}</CS_GeographicCoordinateSystem></CS_CoordinateSystem>",
                                HorizontalDatum.Xml, AngularUnit.Xml, PrimeMeridian.Xml);

                return sb.ToString();
            }
        }

        public override Boolean EqualParams(IInfo other)
        {
            GeographicCoordinateSystem<TCoordinate> gcs =
                other as GeographicCoordinateSystem<TCoordinate>;

            if (ReferenceEquals(gcs, null))
            {
                return false;
            }

            if (gcs.Dimension != Dimension)
            {
                return false;
            }

            if (Wgs84ConversionInfo != null && gcs.Wgs84ConversionInfo == null)
            {
                return false;
            }

            if (Wgs84ConversionInfo == null && gcs.Wgs84ConversionInfo != null)
            {
                return false;
            }

            if (Wgs84ConversionInfo != null && gcs.Wgs84ConversionInfo != null)
            {
                if (Wgs84ConversionInfo.Count != gcs.Wgs84ConversionInfo.Count)
                {
                    return false;
                }

                for (Int32 i = 0; i < Wgs84ConversionInfo.Count; i++)
                {
                    if (!gcs.Wgs84ConversionInfo[i].Equals(Wgs84ConversionInfo[i]))
                    {
                        return false;
                    }
                }
            }

            if (AxisInfo.Count != gcs.AxisInfo.Count)
            {
                return false;
            }

            for (Int32 i = 0; i < gcs.AxisInfo.Count; i++)
            {
                if (gcs.AxisInfo[i].Orientation != AxisInfo[i].Orientation)
                {
                    return false;
                }
            }

            return gcs.AngularUnit.EqualParams(AngularUnit) &&
                   gcs.HorizontalDatum.EqualParams(HorizontalDatum) &&
                   gcs.PrimeMeridian.EqualParams(PrimeMeridian);
        }

        #endregion

        internal List<Wgs84ConversionInfo> Wgs84ConversionInfo
        {
            get { return _wgs84ConversionInfo; }
        }
    }
}