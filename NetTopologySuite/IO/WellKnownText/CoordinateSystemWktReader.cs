// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of GeoAPI.Net.
// GeoAPI.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// GeoAPI.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeoAPI.Net; if not, write to the Free Software
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
using System.IO;
using GeoAPI.CoordinateSystems;

namespace GeoAPI.IO.WellKnownText
{
    /// <summary>
    /// Creates an <see cref="IInfo"/> instance based on the supplied Well Known Text (WKT).
    /// </summary>
    internal static class CoordinateSystemWktReader
    {
        /// <summary>
        /// Reads and parses a WKT-formatted projection String.
        /// </summary>
        /// <param name="wkt">String containing WKT.</param>
        /// <returns>Object representation of the WKT.</returns>
        /// <exception cref="ParseException">
        /// If a token is not recognised.
        /// </exception>
        public static IInfo Parse(String wkt, ICoordinateSystemFactory factory)
        {
            return Parse(new StringReader(wkt), factory);
        }

        public static IInfo Parse(TextReader reader, ICoordinateSystemFactory factory)
        {
            IInfo returnObject;
            WktTokenizer tokenizer = new WktTokenizer(reader);
            tokenizer.Read();
            String objectName = tokenizer.CurrentToken;

            switch (objectName)
            {
                case "UNIT":
                    returnObject = readUnit(tokenizer, factory);
                    break;
                //case "VERT_DATUM":
                //    IVerticalDatum verticalDatum = ReadVerticalDatum(tokenizer);
                //    returnObject = verticalDatum;
                //    break;
                case "SPHEROID":
                    returnObject = readEllipsoid(tokenizer, factory);
                    break;
                case "DATUM":
                    returnObject = readHorizontalDatum(tokenizer, factory);
                    ;
                    break;
                case "PRIMEM":
                    returnObject = readPrimeMeridian(tokenizer, factory);
                    break;
                case "VERT_CS":
                case "GEOGCS":
                case "PROJCS":
                case "COMPD_CS":
                case "GEOCCS":
                case "FITTED_CS":
                case "LOCAL_CS":
                    returnObject = readCoordinateSystem(tokenizer, factory);
                    break;
                default:
                    throw new ArgumentException(String.Format("'{0}' is not recongnized.", objectName));
            }

            reader.Close();
            return returnObject;
        }

        /// <summary>
        /// Returns an <see cref="IUnit"/> given a string of WKT.
        /// </summary>
        /// <param name="tokenizer">WktStreamTokenizer that has the WKT.</param>
        /// <returns>An object that implements the <see cref="IUnit"/> interface.</returns>
        private static IUnit readUnit(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            tokenizer.ReadToken("[");
            String unitName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.Read();
            Double conversionFactor = tokenizer.CurrentTokenAsNumber;
            String authority = String.Empty;
            String authorityCode = String.Empty;
            tokenizer.Read();

            if (tokenizer.CurrentToken == ",")
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            return factory.CreateUnit(conversionFactor, unitName, authority,
                authorityCode, String.Empty, String.Empty, String.Empty);
        }

        /// <summary>
        /// Returns a <see cref="ILinearUnit"/> given a string of WKT.
        /// </summary>
        /// <param name="tokenizer">WktStreamTokenizer that has the WKT.</param>
        /// <returns>An Object that implements the ILinearUnit interface.</returns>
        private static ILinearUnit readLinearUnit(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            //if (tokenizer.CurrentToken == "[") tokenizer.Read(); // Token("[");
            //if (tokenizer.CurrentToken != "UNIT")
            //    return null;

            tokenizer.ReadToken("[");
            String unitName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.Read();
            Double conversionFactor = tokenizer.CurrentTokenAsNumber;
            String authority = String.Empty;
            String authorityCode = String.Empty;
            tokenizer.Read();

            if (tokenizer.CurrentToken == ",")
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            return factory.CreateLinearUnit(conversionFactor, unitName, authority,
                authorityCode, String.Empty, String.Empty, String.Empty);
        }

        /// <summary>
        /// Returns an <see cref="IAngularUnit"/> given a string of WKT.
        /// </summary>
        /// <param name="tokenizer">WktStreamTokenizer that has the WKT.</param>
        /// <returns>An Object that implements the IUnit interface.</returns>
        private static IAngularUnit readAngularUnit(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            tokenizer.ReadToken("[");
            String unitName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.Read();
            Double conversionFactor = tokenizer.CurrentTokenAsNumber;
            String authority = String.Empty;
            String authorityCode = String.Empty;
            tokenizer.Read();

            if (tokenizer.CurrentToken == ",")
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            return factory.CreateAngularUnit(conversionFactor, unitName, authority,
                authorityCode, String.Empty, String.Empty, String.Empty);
        }

        /// <summary>
        /// Returns an <see cref="IAxisInfo"/> given a string of WKT.
        /// </summary>
        /// <param name="tokenizer">WktStreamTokenizer that has the WKT.</param>
        /// <returns>An AxisInfo Object.</returns>
        private static IAxisInfo readAxis(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            if (tokenizer.CurrentToken != "AXIS")
            {
                tokenizer.ReadToken("AXIS");
            }

            tokenizer.ReadToken("[");
            String axisName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.Read();
            String unitname = tokenizer.CurrentToken;
            tokenizer.ReadToken("]");

            switch (unitname.ToUpper())
            {
                case "DOWN":
                    return factory.CreateAxisInfo(AxisOrientation.Down, axisName);
                case "EAST":
                    return factory.CreateAxisInfo(AxisOrientation.East, axisName);
                case "NORTH":
                    return factory.CreateAxisInfo(AxisOrientation.North, axisName);
                case "OTHER":
                    return factory.CreateAxisInfo(AxisOrientation.Other, axisName);
                case "SOUTH":
                    return factory.CreateAxisInfo(AxisOrientation.South, axisName);
                case "UP":
                    return factory.CreateAxisInfo(AxisOrientation.Up, axisName);
                case "WEST":
                    return factory.CreateAxisInfo(AxisOrientation.West, axisName);
                default:
                    throw new ArgumentException("Invalid axis name '" + unitname + "' in WKT");
            }
        }

        private static ICoordinateSystem readCoordinateSystem(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            switch (tokenizer.CurrentToken)
            {
                case "GEOGCS":
                    return readGeographicCoordinateSystem(tokenizer, factory);
                case "PROJCS":
                    return readProjectedCoordinateSystem(tokenizer, factory);
                case "COMPD_CS":
                /*	ICompoundCoordinateSystem compoundCS = ReadCompoundCoordinateSystem(tokenizer);
                returnCS = compoundCS;
                break;*/
                case "VERT_CS":
                /*	IVerticalCoordinateSystem verticalCS = ReadVerticalCoordinateSystem(tokenizer);
                returnCS = verticalCS;
                break;*/
                case "GEOCCS":
                case "FITTED_CS":
                case "LOCAL_CS":
                    throw new NotSupportedException(
                        "Coordinate system is not supported.");
                default:
                    throw new InvalidOperationException(
                        "Coordinate system is not recognized.");
            }
        }

        /// <summary>
        /// Reads either 3, 6 or 7 parameter Bursa-Wolf values from TOWGS84 token
        /// </summary>
        private static Wgs84ConversionInfo readWgs84ConversionInfo(WktTokenizer tokenizer)
        {
            //TOWGS84[0,0,0,0,0,0,0]
            tokenizer.ReadToken("[");
            tokenizer.Read();
            Double dx = tokenizer.CurrentTokenAsNumber;
            tokenizer.ReadToken(",");

            tokenizer.Read();
            Double dy = tokenizer.CurrentTokenAsNumber;
            tokenizer.ReadToken(",");

            tokenizer.Read();
            Double dz = tokenizer.CurrentTokenAsNumber;
            tokenizer.Read();

            Double ex = 0, ey = 0, ez = 0, ppm = 0;

            if (tokenizer.CurrentToken == ",")
            {
                tokenizer.Read();
                ex = tokenizer.CurrentTokenAsNumber;

                tokenizer.ReadToken(",");
                tokenizer.Read();
                ey = tokenizer.CurrentTokenAsNumber;

                tokenizer.ReadToken(",");
                tokenizer.Read();
                ez = tokenizer.CurrentTokenAsNumber;

                tokenizer.Read();

                if (tokenizer.CurrentToken == ",")
                {
                    tokenizer.Read();
                    ppm = tokenizer.CurrentTokenAsNumber;
                }
            }

            if (tokenizer.CurrentToken != "]")
            {
                tokenizer.ReadToken("]");
            }

            return new Wgs84ConversionInfo(dx, dy, dz, ex, ey, ez, ppm);
        }

        private static IEllipsoid readEllipsoid(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            //SPHEROID["Airy 1830",6377563.396,299.3249646,AUTHORITY["EPSG","7001"]]
            tokenizer.ReadToken("[");
            String name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.Read();
            Double majorAxis = tokenizer.CurrentTokenAsNumber;
            tokenizer.ReadToken(",");
            tokenizer.Read();
            Double e = tokenizer.CurrentTokenAsNumber;
            //

            //tokenizer.ReadToken(",");
            tokenizer.Read();
            String authority = String.Empty;
            String authorityCode = String.Empty;

            if (tokenizer.CurrentToken == ",") //Read authority
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            ILinearUnit meter = factory.CreateLinearUnit(CommonLinearUnits.Meter);

            IEllipsoid ellipsoid = factory.CreateFlattenedSphere(
                majorAxis, e, meter, name, authority, authorityCode, String.Empty,
                String.Empty, String.Empty);

            return ellipsoid;
        }

        private static IProjection readProjection(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            //tokenizer.Read();// PROJECTION
            tokenizer.ReadToken("PROJECTION");
            tokenizer.ReadToken("["); //[
            String projectionName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken("]"); //]
            tokenizer.ReadToken(","); //,
            tokenizer.ReadToken("PARAMETER");
            List<ProjectionParameter> paramList = new List<ProjectionParameter>();

            while (tokenizer.CurrentToken == "PARAMETER")
            {
                tokenizer.ReadToken("[");
                String paramName = tokenizer.ReadDoubleQuotedWord();
                tokenizer.ReadToken(",");
                tokenizer.Read();
                Double paramValue = tokenizer.CurrentTokenAsNumber;
                tokenizer.ReadToken("]");
                tokenizer.ReadToken(",");
                paramList.Add(new ProjectionParameter(paramName, paramValue));
                tokenizer.Read();
            }

            String authority = String.Empty;
            String authorityCode = String.Empty;
            IProjection projection = factory.CreateProjection(projectionName, paramList,
                projectionName, authority, authorityCode, String.Empty,
                String.Empty, String.Empty);

            return projection;
        }

        private static IProjectedCoordinateSystem readProjectedCoordinateSystem(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            /*PROJCS[
				"OSGB 1936 / British National Grid",
				GEOGCS[
					"OSGB 1936",
					DATUM[...]
					PRIMEM[...]
					AXIS["Geodetic latitude","NORTH"]
					AXIS["Geodetic longitude","EAST"]
					AUTHORITY["EPSG","4277"]
				],
                UNIT[...],
				PROJECTION["Transverse Mercator"],
				PARAMETER["latitude_of_natural_origin",49],
				PARAMETER["longitude_of_natural_origin",-2],
				PARAMETER["scale_factor_at_natural_origin",0.999601272],
				PARAMETER["false_easting",400000],
				PARAMETER["false_northing",-100000],
                UNIT[...],
				AXIS["Easting","EAST"],
				AXIS["Northing","NORTH"],
				AUTHORITY["EPSG","27700"]
			]
			*/

            tokenizer.ReadToken("[");
            String name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("GEOGCS");
            IGeographicCoordinateSystem geographic = readGeographicCoordinateSystem(tokenizer, factory);
            tokenizer.ReadToken(",");

            IUnit unit = null;
            String projectionName = "";
            List<ProjectionParameter> paramList = new List<ProjectionParameter>();
            IAxisInfo axis1 = null, axis2 = null;
            String authority = String.Empty;
            String authorityCode = String.Empty;

            while (true)
            {
                tokenizer.Read();
                switch (tokenizer.CurrentToken)
                {
                    case "UNIT":
                        unit = readLinearUnit(tokenizer, factory);
                        break;
                    case "AUTHORITY":
                        tokenizer.ReadAuthority(ref authority, ref authorityCode);
                        break;
                    case "AXIS":
                        if (axis1 == null)
                            axis1 = readAxis(tokenizer, factory);
                        else
                            axis2 = readAxis(tokenizer, factory);
                        break;
                    case "PROJECTION":
                        projectionName = readProjectionName(tokenizer);
                        break;
                    case "PARAMETER":
                        paramList.Add(readProjectionParameter(tokenizer));
                        break;
                    case "EXTENSION":
                        readExtension(tokenizer);//ignore for now;
                        break;
                }
                if (tokenizer.NextToken == "]") break;
                tokenizer.Read();
            }

            IProjection projection = factory.CreateProjection(projectionName, paramList, projectionName);
            //IUnit unit = readLinearUnit(tokenizer, factory);
            //IProjection projection = readProjection(tokenizer, factory);
            //if (unit == null ) unit = readLinearUnit(tokenizer, factory);

            //String authority = String.Empty;
            //String authorityCode = String.Empty;
            //tokenizer.Read();
            //IAxisInfo axis1 = null, axis2 = null;

            //if (tokenizer.CurrentToken == ",")
            //{
            //    tokenizer.Read();

            //    if (tokenizer.CurrentToken == "AXIS")
            //    {
            //        axis1 = readAxis(tokenizer, factory);
            //        tokenizer.Read();
            //        if (tokenizer.CurrentToken == ",") tokenizer.Read();
            //    }

            //    if (tokenizer.CurrentToken == "AXIS")
            //    {
            //        axis2 = readAxis(tokenizer, factory);
            //        tokenizer.Read();
            //        if (tokenizer.CurrentToken == ",") tokenizer.Read();
            //    }

            //    if (tokenizer.CurrentToken == "AUTHORITY")
            //    {
            //        tokenizer.ReadAuthority(ref authority, ref authorityCode);
            //        tokenizer.ReadToken("]");
            //    }
            //}

            //This is default axis values if not specified.
            if (ReferenceEquals(axis1, null) && ReferenceEquals(axis2, null))
            {
                axis1 = factory.CreateAxisInfo(AxisOrientation.East, "X");
                axis2 = factory.CreateAxisInfo(AxisOrientation.North, "Y");
            }

            IProjectedCoordinateSystem projected = factory.CreateProjectedCoordinateSystem(
                geographic, projection, unit as ILinearUnit, axis1, axis2,
                name, authority, authorityCode, String.Empty,
                String.Empty, String.Empty);

            return projected;
        }

        private static void readExtension(WktTokenizer tokenizer)
        {
            tokenizer.ReadToken("[");
            tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken("]");
        }

        private static ProjectionParameter readProjectionParameter(WktTokenizer tokenizer)
        {
            tokenizer.ReadToken("[");
            String paramName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.Read();
            Double paramValue = tokenizer.CurrentTokenAsNumber;
            tokenizer.ReadToken("]");
            return new ProjectionParameter(paramName, paramValue);
        }

        private static string readProjectionName(WktTokenizer tokenizer)
        {
            tokenizer.ReadToken("["); //[
            String projectionName = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken("]"); //]

            return projectionName;
        }

        private static IGeographicCoordinateSystem readGeographicCoordinateSystem(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            /*
			GEOGCS["OSGB 1936",
			DATUM["OSGB 1936",SPHEROID["Airy 1830",6377563.396,299.3249646,AUTHORITY["EPSG","7001"]]TOWGS84[0,0,0,0,0,0,0],AUTHORITY["EPSG","6277"]]
			PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]]
			AXIS["Geodetic latitude","NORTH"]
			AXIS["Geodetic longitude","EAST"]
			AUTHORITY["EPSG","4277"]
			]
			*/
            tokenizer.ReadToken("[");
            String name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("DATUM");
            IHorizontalDatum horizontalDatum = readHorizontalDatum(tokenizer, factory);
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("PRIMEM");
            IPrimeMeridian primeMeridian = readPrimeMeridian(tokenizer, factory);
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("UNIT");
            IAngularUnit angularUnit = readAngularUnit(tokenizer, factory);

            String authority = String.Empty;
            String authorityCode = String.Empty;
            tokenizer.Read();
            IAxisInfo axis1 = null, axis2 = null;

            if (tokenizer.CurrentToken == ",")
            {
                tokenizer.Read();

                if (tokenizer.CurrentToken == "AXIS")
                {
                    axis1 = readAxis(tokenizer, factory);
                    tokenizer.Read();
                    if (tokenizer.CurrentToken == ",") tokenizer.Read();
                }

                if (tokenizer.CurrentToken == "AXIS")
                {
                    axis2 = readAxis(tokenizer, factory);
                    tokenizer.Read();
                    if (tokenizer.CurrentToken == ",") tokenizer.Read();
                }

                if (tokenizer.CurrentToken == "AUTHORITY")
                {
                    tokenizer.ReadAuthority(ref authority, ref authorityCode);
                    tokenizer.ReadToken("]");
                }
            }

            //This is default axis values if not specified.
            if (ReferenceEquals(axis1, null) && ReferenceEquals(axis2, null))
            {
                axis1 = factory.CreateAxisInfo(AxisOrientation.East, "Lon");
                axis2 = factory.CreateAxisInfo(AxisOrientation.North, "Lat");
            }

            // TODO: The DefaultEnvelope shouldn't be null, apparently. 
            // The OGC spec notes that it should be an extents that should be as
            // large as is possibly needed. See the CoordinateSystem.DefaultEnvelope
            // member XmlDoc for more info, or the OGC CTS spec: 12.3.5.1.
            IGeographicCoordinateSystem geographic = factory.CreateGeographicCoordinateSystem(
                null, angularUnit, horizontalDatum, primeMeridian, axis1, axis2, name, authority,
                authorityCode, String.Empty, String.Empty, String.Empty);

            return geographic;
        }

        private static IHorizontalDatum readHorizontalDatum(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            //DATUM["OSGB 1936",SPHEROID["Airy 1830",6377563.396,299.3249646,AUTHORITY["EPSG","7001"]]TOWGS84[0,0,0,0,0,0,0],AUTHORITY["EPSG","6277"]]
            Wgs84ConversionInfo wgsInfo = null;
            String authority = String.Empty;
            String authorityCode = String.Empty;

            tokenizer.ReadToken("[");
            String name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.ReadToken("SPHEROID");
            IEllipsoid ellipsoid = readEllipsoid(tokenizer, factory);
            tokenizer.Read();

            while (tokenizer.CurrentToken == ",")
            {
                tokenizer.Read();

                if (tokenizer.CurrentToken == "TOWGS84")
                {
                    wgsInfo = readWgs84ConversionInfo(tokenizer);
                    tokenizer.Read();
                }
                else if (tokenizer.CurrentToken == "AUTHORITY")
                {
                    tokenizer.ReadAuthority(ref authority, ref authorityCode);
                    tokenizer.ReadToken("]");
                }
            }

            // make an assumption about the datum type.
            IHorizontalDatum horizontalDatum = factory.CreateHorizontalDatum(
                DatumType.HorizontalGeocentric, ellipsoid, wgsInfo,
                name, authority, authorityCode, String.Empty, String.Empty,
                String.Empty);

            return horizontalDatum;
        }

        private static IPrimeMeridian readPrimeMeridian(WktTokenizer tokenizer, ICoordinateSystemFactory factory)
        {
            //PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]]
            tokenizer.ReadToken("[");
            String name = tokenizer.ReadDoubleQuotedWord();
            tokenizer.ReadToken(",");
            tokenizer.Read();
            Double longitude = tokenizer.CurrentTokenAsNumber;

            tokenizer.Read();
            String authority = String.Empty;
            String authorityCode = String.Empty;

            if (tokenizer.CurrentToken == ",")
            {
                tokenizer.ReadAuthority(ref authority, ref authorityCode);
                tokenizer.ReadToken("]");
            }

            // make an assumption about the Angular units - degrees.
            IAngularUnit degrees = factory.CreateAngularUnit(CommonAngularUnits.Degree);

            IPrimeMeridian primeMeridian = factory.CreatePrimeMeridian(degrees,
                longitude, name, authority, authorityCode, String.Empty,
                String.Empty, String.Empty);

            return primeMeridian;
        }

        /*
		private static ICompoundCoordinateSystem readCompoundCoordinateSystem(WktStreamTokenizer tokenizer)
		{
			
			//COMPD_CS[
			//"OSGB36 / British National Grid + ODN",
			//PROJCS[]
			//VERT_CS[]
			//AUTHORITY["EPSG","7405"]
			//]

			tokenizer.ReadToken("[");
			String name=tokenizer.ReadDoubleQuotedWord();
			tokenizer.ReadToken(",");
			tokenizer.Read();
			String headCSCode =  tokenizer.CurrentToken;
			ICoordinateSystem headCS = ReadCoordinateSystem(headCSCode,tokenizer);
			tokenizer.ReadToken(",");
			tokenizer.Read();
			String tailCSCode =  tokenizer.CurrentToken;
			ICoordinateSystem tailCS = ReadCoordinateSystem(tailCSCode,tokenizer);
			tokenizer.ReadToken(",");
			String authority=String.Empty;
			String authorityCode=String.Empty; 
			tokenizer.ReadAuthority(ref authority, ref authorityCode);
			tokenizer.ReadToken("]");
			ICompoundCoordinateSystem compoundCS = new CompoundCoordinateSystem(headCS,tailCS,String.Empty,authority,authorityCode,name,String.Empty,String.Empty); 
			return compoundCS;
			
		}
        
		private static IVerticalCoordinateSystem readVerticalCoordinateSystem(WktStreamTokenizer tokenizer)
		{
			//VERT_CS["Newlyn",
			//VERT_DATUM["Ordnance Datum Newlyn",2005,AUTHORITY["EPSG","5101"]]
			//UNIT["metre",1,AUTHORITY["EPSG","9001"]]
			//AUTHORITY["EPSG","5701"]
			
			tokenizer.ReadToken("[");
			String name=tokenizer.ReadDoubleQuotedWord();
			tokenizer.ReadToken(",");
			tokenizer.ReadToken("VERT_DATUM");
			IVerticalDatum verticalDatum = ReadVerticalDatum(tokenizer);
			tokenizer.ReadToken("UNIT");
			IUnit unit = ReadUnit(tokenizer);
			String authority=String.Empty;
			String authorityCode=String.Empty; 
			tokenizer.ReadAuthority(ref authority, ref authorityCode);
			tokenizer.ReadToken("]");

			IVerticalCoordinateSystem verticalCS = new VerticalCoordinateSystem(name,verticalDatum,String.Empty,authority,authorityCode,String.Empty,String.Empty);
			return verticalCS;
		}
        
		private static IVerticalDatum readVerticalDatum(WktStreamTokenizer tokenizer)
		{
			//VERT_DATUM["Ordnance Datum Newlyn",2005,AUTHORITY["5101","EPSG"]]
			tokenizer.ReadToken("[");
			String datumName=tokenizer.ReadDoubleQuotedWord();
			tokenizer.ReadToken(",");
			tokenizer.Read();
			String datumTypeNumber = tokenizer.CurrentToken;
			tokenizer.ReadToken(",");
			String authority=String.Empty;
			String authorityCode=String.Empty; 
			tokenizer.ReadAuthority(ref authority, ref authorityCode);
			DatumType datumType = (DatumType)Enum.Parse(typeof(DatumType),datumTypeNumber);
			IVerticalDatum verticalDatum = new VerticalDatum(datumType,String.Empty,authorityCode,authority,datumName,String.Empty,String.Empty);
			tokenizer.ReadToken("]");
			return verticalDatum;
		}
        
		[Obsolete("Since the related objects have not been implemented")]
		private static IGeocentricCoordinateSystem readGeocentricCoordinateSystem(WktStreamTokenizer tokenizer)
		{
			throw new NotImplementedException("IGeocentricCoordinateSystem is not implemented");
		}
         
         */
    }
}