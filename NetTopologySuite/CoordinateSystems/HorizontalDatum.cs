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
using System.Globalization;
using System.Text;
using GeoAPI.CoordinateSystems;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems
{
    /// <summary>
    /// Horizontal datum defining the standard datum information.
    /// </summary>
    public class HorizontalDatum : Datum, IHorizontalDatum
    {
        private readonly IEllipsoid _ellipsoid;
        private readonly Wgs84ConversionInfo _wgs84ConversionInfo;

        /// <summary>
        /// Initializes a new instance of a horizontal datum
        /// </summary>
        /// <param name="ellipsoid">Ellipsoid</param>
        /// <param name="toWgs84">Parameters for a Bursa Wolf transformation into WGS84</param>
        /// <param name="type">Datum type</param>
        /// <param name="name">Name</param>
        /// <param name="authority">Authority name</param>
        /// <param name="authorityCode">Authority-specific identification code.</param>
        /// <param name="alias">Alias</param>
        /// <param name="abbreviation">Abbreviation</param>
        /// <param name="remarks">Provider-supplied remarks</param>
        internal HorizontalDatum(IEllipsoid ellipsoid, 
                                 Wgs84ConversionInfo toWgs84, 
                                 DatumType type,
                                 String name, 
                                 String authority, 
                                 String authorityCode, 
                                 String alias, 
                                 String remarks, 
                                 String abbreviation)
            : base(type, name, authority, authorityCode, alias, remarks, abbreviation)
        {
            _ellipsoid = ellipsoid;
            _wgs84ConversionInfo = toWgs84;
        }

        #region Predefined datums

        /// <summary>
        /// World Geodetic System 1984. Kept current through present day.
        /// </summary>
        /// <remarks>
        /// <para>Area of use: World</para>
        /// <para>
        /// Origin description: Defined through a consistent set of station coordinates. 
        /// These have changed with time: by 0.7m on 29/6/1994 [WGS 84 (G730)], 
        /// a further 0.2m on 29/1/1997 [WGS 84 (G873)] and a further 0.06m on 
        /// 20/1/2002 [WGS 84 (G1150)].
        /// </para>
        /// <para>
        /// EPSG's WGS 84 datum has been the then current realisation. 
        /// No distinction is made between the original WGS 84 frame, WGS 84 (G730), 
        /// WGS 84 (G873) and WGS 84 (G1150). Since 1997, WGS 84 has been maintained 
        /// within 10cm of the then current ITRF.
        /// </para>
        /// </remarks>
        public static HorizontalDatum Wgs84
        {
            get
            {
                return new HorizontalDatum(CoordinateSystems.Ellipsoid.Wgs84,
                                           null, 
                                           DatumType.HorizontalGeocentric, 
                                           "World Geodetic System 1984", "EPSG",
                                           "6326",
                                           String.Empty,
                                           "EPSG's WGS 84 datum has been the then "+
                                           "current realisation. No distinction is made "+
                                           "between the original WGS 84 frame, "+
                                           "WGS 84 (G730), WGS 84 (G873) and WGS 84 (G1150). "+
                                           "Since 1997, WGS 84 has been maintained within 10cm "+
                                           "of the then current ITRF.",
                                           String.Empty);
            }
        }

        /// <summary>
        /// World Geodetic System 1972.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used by GPS before 1987. For Transit satellite positioning see also WGS 72BE. 
        /// Datum code 6323 reserved for southern hemisphere projected coordinate systems.
        /// </para>
        /// <para>Area of use: World</para>
        /// <para>
        /// Origin description: Developed from a worldwide distribution of terrestrial and
        /// geodetic satellite observations and defined through a set of station coordinates.
        /// </para>
        /// </remarks>
        public static HorizontalDatum Wgs72
        {
            get
            {
                Wgs84ConversionInfo wgsConversion = new Wgs84ConversionInfo(0, 0, 4.5, 0, 0, 0.554, 0.219);
                HorizontalDatum datum = new HorizontalDatum(CoordinateSystems.Ellipsoid.Wgs72,
                                                            wgsConversion, 
                                                            DatumType.HorizontalGeocentric,
                                                            "World Geodetic System 1972", 
                                                            "EPSG", 
                                                            "6322", 
                                                            String.Empty,
                                                            "Used by GPS before 1987. For TRANSIT "+
                                                            "satellite positioning " +
                                                            "see also WGS 72BE. Datum code 6323 reserved "+
                                                            "for southern hemisphere projected coordinate "+
                                                            "systems.", 
                                                            String.Empty);
                return datum;
            }
        }


        /// <summary>
        /// European Terrestrial Reference System 1989.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Area of use: 
        /// Europe: Albania; Andorra; Austria; Belgium; Bosnia and Herzegovina; 
        /// Bulgaria; Croatia; Cyprus; Czech Republic; Denmark; Estonia; Finland; 
        /// Faroe Islands; France; Germany; Greece; Hungary; Ireland; Italy; 
        /// Latvia; Liechtenstein; Lithuania; Luxembourg; Malta; Netherlands; 
        /// Norway; Poland; Portugal; Romania; San Marino; Serbia and Montenegro; 
        /// Slovakia; Slovenia; Spain; Svalbard; Sweden; Switzerland; 
        /// United Kingdom (UK) including Channel Islands and 
        /// Isle of Man; Vatican City State.
        /// </para>
        /// <para>
        /// Origin description: Fixed to the stable part of the Eurasian 
        /// continental plate and consistent with ITRS at the epoch 1989.0.
        /// </para>
        /// </remarks>
        public static HorizontalDatum Etrf89
        {
            get
            {
                Wgs84ConversionInfo wgsConversion = new Wgs84ConversionInfo();
                HorizontalDatum datum = new HorizontalDatum(CoordinateSystems.Ellipsoid.Grs80, 
                                                            wgsConversion,
                                                            DatumType.HorizontalGeocentric,
                                                            "European Terrestrial Reference System 1989",
                                                            "EPSG", 
                                                            "6258", 
                                                            "ETRF89",
                                                            "The distinction in usage between ETRF89 and ETRS89 is confused: " +
                                                            "although in principle conceptually different in practice both are " +
                                                            "used for the realisation.", 
                                                            String.Empty);
                return datum;
            }
        }

        /// <summary>
        /// European Datum 1950.
        /// </summary>
        /// <remarks>
        /// <para>Area of use:
        /// Europe - west - Denmark; Faroe Islands; France offshore; 
        /// Israel offshore; Italy including San Marino and Vatican City State; 
        /// Ireland offshore; Netherlands offshore; Germany; Greece (offshore);
        /// North Sea; Norway; Spain; Svalbard; Turkey; United Kingdom UKCS offshore;
        /// Egypt - Western Desert.
        /// </para>
        /// <para>
        /// Origin description: Fundamental point: Potsdam (Helmert Tower). 
        /// Latitude: 52 deg 22 min 51.4456 sec N; 
        /// Longitude: 13 deg  3 min 58.9283 sec E (of Greenwich).
        /// </para>
        /// </remarks>
        public static HorizontalDatum ED50
        {
            get
            {
                return
                    new HorizontalDatum(CoordinateSystems.Ellipsoid.International1924,
                                        new Wgs84ConversionInfo(-87, -98, -121, 0, 0, 0, 0),
                                        DatumType.HorizontalClassic, 
                                        "European Datum 1950",
                                        "EPSG", 
                                        "6230", 
                                        "ED50", 
                                        String.Empty, 
                                        String.Empty);
            }
        }

        #endregion

        #region IHorizontalDatum Members

        /// <summary>
        /// Gets or sets the ellipsoid of the datum
        /// </summary>
        public IEllipsoid Ellipsoid
        {
            get { return _ellipsoid; }
        }

        /// <summary>
        /// Gets preferred parameters for a Bursa Wolf transformation into WGS84
        /// </summary>
        public Wgs84ConversionInfo Wgs84Parameters
        {
            get { return _wgs84ConversionInfo; }
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
                sb.AppendFormat("DATUM[\"{0}\", {1}", Name, _ellipsoid.Wkt);

                if (_wgs84ConversionInfo != null)
                {
                    sb.AppendFormat(", {0}", _wgs84ConversionInfo.Wkt);
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
                return String.Format(CultureInfo.InvariantCulture.NumberFormat,
                                     "<CS_HorizontalDatum DatumType=\"{0}\">{1}{2}{3}</CS_HorizontalDatum>",
                                     (Int32) DatumType, 
                                     InfoXml, 
                                     Ellipsoid.Xml,
                                     (Wgs84Parameters == null ? String.Empty : Wgs84Parameters.Xml));
            }
        }

        #endregion

        public override Boolean EqualParams(IInfo obj)
        {
            HorizontalDatum other = obj as HorizontalDatum;

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (other.Wgs84Parameters == null && Wgs84Parameters != null)
            {
                return false;
            }

            if (other.Wgs84Parameters != null && !other.Wgs84Parameters.Equals(Wgs84Parameters))
            {
                return false;
            }

            return other.Ellipsoid.EqualParams(Ellipsoid) && other.DatumType == DatumType;
        }
    }
}