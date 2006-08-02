using System;
using System.CodeDom.Compiler;
using System.IO;

using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;	

namespace GisSharpBlog.NetTopologySuite.IO
{
	/// <summary>
	/// Writes a coordinate system object out as Well Known Text (WKT).
	/// </summary>
	public class CoordinateSystemWktWriter
	{		
		/// <summary>
		/// Converts a given coordinate system object to a WKT string.
		/// </summary>
		/// <param name="obj">The coordinate system object to convert.</param>
		/// <returns>A string containing WKT.</returns>
		public static string Write(object obj)
		{
			TextWriter textwriter = new StringWriter();
			IndentedTextWriter indentedWriter = new IndentedTextWriter(textwriter);
			Write(obj, indentedWriter);
			return textwriter.ToString();
		}

        /// <summary>
		/// Converts a given coordinate system object to a IndentedTextWriter.
		/// </summary>
		/// <param name="obj">The coordinate system to convert.</param>
		/// <param name="writer"></param>
		/// <remarks>
		/// <list type="bullet">
		/// <listheader><term>Items</term><description>Descriptions</description></listheader>
		/// <item><term>ICoordinateSystem</term><description>Your Description</description></item>
		/// <item><term>IDatum</term><description>Your Description</description></item>
		/// <item><term>IEllipsoid</term><description>Your Description</description></item>
		/// <item><term>IAxisInfo</term><description>Your Description</description></item>
		/// <item><term>IWGS84ConversionInfo</term><description>Your Description</description></item>
		/// <item><term>IUnit</term><description>Your Description</description></item>
		/// <item><term>IPrimeMeridian</term><description>Your Description</description></item>
		/// <item><term>ICompoundCoordinateSystem</term><description>Your Description</description></item>
		/// <item><term>IGeographicCoordinateSystem</term><description>Your Description</description></item>
		/// <item><term>IProjectedCoordinateSystem</term><description>Your Description</description></item>
		/// <item><term>IVerticalCoordinateSystem</term><description>Your Description</description></item>
		/// </list>
		/// </remarks>
		public static void Write(object obj, IndentedTextWriter writer)
		{
			if (obj is ICoordinateSystem)			
				WriteCoordinateSystem(obj as ICoordinateSystem, writer);
			else if (obj is IDatum)
				WriteDatum(obj as IDatum, writer);
			else if (obj is IEllipsoid)
				WriteEllipsoid(obj as IEllipsoid, writer);
			else if (obj is IAxisInfo)
            {
				IAxisInfo info = (IAxisInfo)obj;
				WriteAxis(info, writer);
            }
            else if (obj is WGS84ConversionInfo)
            {
                WGS84ConversionInfo info = (WGS84ConversionInfo)obj;
                WriteWGS84ConversionInfo(info, writer);
            }
            else if (obj is IUnit)
                WriteUnit(obj as IUnit, writer);
            else if (obj is IPrimeMeridian)
                WritePrimeMeridian(obj as IPrimeMeridian, writer);
            else throw new NotImplementedException(String.Format("Cannot convert {0} to WKT.", obj.GetType().FullName));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="coordinateSystem"></param>
		/// <param name="writer"></param>
		private static void WriteCoordinateSystem(ICoordinateSystem coordinateSystem, IndentedTextWriter writer)
		{
			if (coordinateSystem is ICompoundCoordinateSystem)
				WriteCompoundCoordinateSystem(coordinateSystem as ICompoundCoordinateSystem, writer);		
			else if (coordinateSystem is IGeographicCoordinateSystem)
				WriteGeographicCoordinateSystem( coordinateSystem as IGeographicCoordinateSystem, writer);
			else if (coordinateSystem is IProjectedCoordinateSystem)
				WriteProjectedCoordinateSystem( coordinateSystem as IProjectedCoordinateSystem, writer);
			else if (coordinateSystem is ILocalCoordinateSystem)
				WriteLocalCoordinateSystem( coordinateSystem as ILocalCoordinateSystem, writer);
			else if (coordinateSystem is IFittedCoordinateSystem)
				WriteFittedCoordinateSystem( coordinateSystem as IFittedCoordinateSystem, writer);
			else if (coordinateSystem is IGeocentricCoordinateSystem)
				WriteGeocentricCoordinateSystem(coordinateSystem as IGeocentricCoordinateSystem, writer);
			else if (coordinateSystem is IVerticalCoordinateSystem)
				WriteVerticalCoordinateSystem(coordinateSystem as IVerticalCoordinateSystem, writer);
			else if (coordinateSystem is IHorizontalCoordinateSystem)
				WriteHorizontalCoordinateSystem( coordinateSystem as IHorizontalCoordinateSystem, writer);
			else throw new InvalidOperationException("this coordinate system is recongized");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="writer"></param>
		private static void WriteUnit(IUnit unit, IndentedTextWriter writer)
		{
			if (unit is IAngularUnit)
				WriteAngularUnit(unit as IAngularUnit, writer);
			else if (unit is ILinearUnit)
				WriteLinearUnit(unit as ILinearUnit, writer);
			else throw new InvalidOperationException("this unit is not recognized");
		}

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="angularUnit"></param>
	    /// <param name="writer"></param>
		private static void WriteAngularUnit(IAngularUnit angularUnit, IndentedTextWriter writer)
		{
			writer.WriteLine("UNIT[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",{1:r},", angularUnit.Name, angularUnit.RadiansPerUnit));
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", angularUnit.Authority, angularUnit.AuthorityCode));
			writer.Indent=writer.Indent-1;
			writer.WriteLine("]");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compoundCoordinateSystem"></param>
        /// <param name="writer"></param>
		private static void WriteCompoundCoordinateSystem(ICompoundCoordinateSystem compoundCoordinateSystem, IndentedTextWriter writer)
		{
			writer.WriteLine("COMPD_CS[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",", compoundCoordinateSystem.Name));
			WriteCoordinateSystem(compoundCoordinateSystem.HeadCS, writer);
			writer.WriteLine(",");
			WriteCoordinateSystem(compoundCoordinateSystem.TailCS, writer);
			writer.WriteLine(",");
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", compoundCoordinateSystem.Authority, compoundCoordinateSystem.AuthorityCode));
			writer.Indent=writer.Indent-1;
			writer.WriteLine("]");
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="geographicCoordinateSystem"></param>
		/// <param name="writer"></param>
		private static void WriteGeographicCoordinateSystem(IGeographicCoordinateSystem geographicCoordinateSystem, IndentedTextWriter writer)
		{
			writer.WriteLine("GEOGCS[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",", geographicCoordinateSystem.Name));
			WriteHorizontalDatum(geographicCoordinateSystem.HorizontalDatum, writer);
			WritePrimeMeridian(geographicCoordinateSystem.PrimeMeridian, writer);
			//TODO:WriteAngularUnit(geocentricCoordinateSystem.get_Units
			for(int dimension=0;  dimension<geographicCoordinateSystem.Dimension; dimension++)
				WriteAxis( geographicCoordinateSystem.GetAxis(dimension), writer );			
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", geographicCoordinateSystem.Authority, geographicCoordinateSystem.AuthorityCode));
			writer.Indent=writer.Indent-1;
			writer.Write("]");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectedCoordinateSystem"></param>
        /// <param name="writer"></param>
		private static void WriteProjectedCoordinateSystem(IProjectedCoordinateSystem projectedCoordinateSystem, IndentedTextWriter writer)
		{
			writer.WriteLine("PROJCS[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",",projectedCoordinateSystem.Name));
			WriteGeographicCoordinateSystem(projectedCoordinateSystem.GeographicCoordinateSystem, writer);
			writer.WriteLine(",");
			WriteProjection( projectedCoordinateSystem.Projection, writer);
			for(int dimension=0;  dimension<projectedCoordinateSystem.Dimension; dimension++)
				WriteAxis( projectedCoordinateSystem.GetAxis(dimension), writer );
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", projectedCoordinateSystem.Authority, projectedCoordinateSystem.AuthorityCode));			
			writer.Indent=writer.Indent-1;
			writer.WriteLine("]");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="datum"></param>
		/// <param name="writer"></param>
		private static void WriteDatum(IDatum datum, IndentedTextWriter writer)
		{
			if (datum is IVerticalDatum)
				WriteVerticalDatum(datum as IVerticalDatum, writer);
			else if (datum is IHorizontalDatum)
				WriteHorizontalDatum(datum as IHorizontalDatum, writer);
			else throw new NotImplementedException("This datum is not supported.");			
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="horizontalDatum"></param>
        /// <param name="writer"></param>
		private static void WriteHorizontalDatum(IHorizontalDatum horizontalDatum, IndentedTextWriter writer)
		{
			writer.WriteLine("DATUM[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",", horizontalDatum.Name));			
			WriteEllipsoid(horizontalDatum.Ellipsoid, writer);
			WriteWGS84ConversionInfo(horizontalDatum.WGS84Parameters, writer);
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", horizontalDatum.Authority, horizontalDatum.AuthorityCode));
			writer.Indent=writer.Indent-1;
			writer.WriteLine("]");
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ellipsoid"></param>
        /// <param name="writer"></param>
		private static void WriteEllipsoid(IEllipsoid ellipsoid, IndentedTextWriter writer)
		{
			writer.WriteLine("SPHEROID[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",{1},{2},", ellipsoid.Name,ellipsoid.SemiMajorAxis, ellipsoid.InverseFlattening ));
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", ellipsoid.Authority, ellipsoid.AuthorityCode));
			writer.Indent=writer.Indent-1;
			writer.WriteLine("]");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="writer"></param>
		private static void WriteAxis(IAxisInfo axis, IndentedTextWriter writer)
		{
			string axisOrientation=String.Empty;
			switch(axis.Orientation)
			{
				case AxisOrientation.Down:
					axisOrientation="DOWN";
					break;
				case AxisOrientation.East:
					axisOrientation="EAST";
					break;
				case AxisOrientation.North:
					axisOrientation="NORTH";
					break;
				case AxisOrientation.Other:
					axisOrientation="OTHER";
					break;
				case AxisOrientation.South:
					axisOrientation="SOUTH";
					break;
				case AxisOrientation.Up:
					axisOrientation="UP";
					break;
				case AxisOrientation.West:
					axisOrientation="WEST";
					break;
				default:
					throw new InvalidOperationException("This  should not exist");
			}
			writer.WriteLine(String.Format("AXIS[\"{0}\",\"{1}\"],", axis.Name, axisOrientation));	
		} 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversionInfo"></param>
        /// <param name="writer"></param>
		private static void WriteWGS84ConversionInfo(WGS84ConversionInfo conversionInfo,  IndentedTextWriter writer)
		{	 
			writer.WriteLine(String.Format("TOWGS84[{0},{1},{2},{3},{4},{5},{6}],",
					conversionInfo.Dx,conversionInfo.Dy,conversionInfo.Dz,
					conversionInfo.Ex,conversionInfo.Ey,conversionInfo.Ez,
					conversionInfo.Ppm));
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="linearUnit"></param>
        /// <param name="writer"></param>
		private static void WriteLinearUnit(ILinearUnit linearUnit, IndentedTextWriter writer)
		{
			writer.WriteLine("UNIT[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",{1},", linearUnit.Name, linearUnit.MetersPerUnit));
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", linearUnit.Authority, linearUnit.AuthorityCode));
			writer.Indent=writer.Indent-1;
			writer.WriteLine("]");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="primeMeridian"></param>
        /// <param name="writer"></param>
		private static void WritePrimeMeridian(IPrimeMeridian primeMeridian, IndentedTextWriter writer)
		{
			writer.WriteLine("PRIMEM[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",{1},", primeMeridian.Name, primeMeridian.Longitude));
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", primeMeridian.Authority, primeMeridian.AuthorityCode));
			writer.Indent=writer.Indent-1;
			writer.WriteLine("]");
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="projection"></param>
		/// <param name="writer"></param>
		private static void WriteProjection(IProjection projection, IndentedTextWriter writer)
		{
			writer.WriteLine(String.Format("PROJECTION[\"{0}\"],",projection.Name));
			for (int i=0;i<projection.NumParameters; i++)
			{
				string paramName = projection.GetParameter(i).Name;
				double paramValue = projection.GetParameter(i).Value;
				writer.WriteLine(String.Format("PARAMETER[\"{0}\",{1}],",paramName,paramValue));
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verticalCoordinateSystem"></param>
        /// <param name="writer"></param>
		private static void WriteVerticalCoordinateSystem(IVerticalCoordinateSystem verticalCoordinateSystem, IndentedTextWriter writer)
		{
			writer.WriteLine("VERT_CS[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",", verticalCoordinateSystem.Name));
			WriteDatum( verticalCoordinateSystem.VerticalDatum, writer );
			WriteUnit( verticalCoordinateSystem.VerticalUnit, writer );
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", verticalCoordinateSystem.Authority, verticalCoordinateSystem.AuthorityCode));
			writer.Indent=writer.Indent-1;
			writer.WriteLine("]");
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="verticalDatum"></param>
        /// <param name="writer"></param>
		private static void WriteVerticalDatum(IVerticalDatum verticalDatum, IndentedTextWriter writer)
		{
			writer.WriteLine("VERT_DATUM[");
			writer.Indent=writer.Indent+1;
			writer.WriteLine(String.Format("\"{0}\",{1},", verticalDatum.Name, DatumTypeAsCode(verticalDatum.DatumType)));
			writer.WriteLine(String.Format("AUTHORITY[\"{0}\",\"{1}\"]", verticalDatum.Authority, verticalDatum.AuthorityCode));
			writer.Indent=writer.Indent-1;
			writer.WriteLine("]");
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datumtype"></param>
        /// <returns></returns>
		public static string DatumTypeAsCode(DatumType datumtype)
		{
			string datumCode = Enum.Format(typeof(DatumType),datumtype, "d");
			return datumCode;
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fiitedCoordinateSystem"></param>
        /// <param name="writer"></param>
        [Obsolete("Since the related objects have not been implemented")]
		public static void WriteFittedCoordinateSystem(IFittedCoordinateSystem fiitedCoordinateSystem, IndentedTextWriter writer)
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geocentricCoordinateSystem"></param>
        /// <param name="writer"></param>
        [Obsolete("Since the related objects have not been implemented")]
		public static void WriteGeocentricCoordinateSystem(IGeocentricCoordinateSystem geocentricCoordinateSystem, IndentedTextWriter writer)
		{
			throw new NotImplementedException();		
        }
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="horizontalCoordinateSystem"></param>
        /// <param name="writer"></param>
        [Obsolete("Since the related objects have not been implemented")]
        public static void WriteHorizontalCoordinateSystem(IHorizontalCoordinateSystem horizontalCoordinateSystem, IndentedTextWriter writer)
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localCoordinateSystem"></param>
        /// <param name="writer"></param>
        [Obsolete("Since the related objects have not been implemented")]
		private static void WriteLocalCoordinateSystem(ILocalCoordinateSystem localCoordinateSystem, IndentedTextWriter writer)
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localDatum"></param>
        /// <param name="writer"></param>
        [Obsolete("Since the related objects have not been implemented")]
		private static void WriteLocalDatum(LocalDatum localDatum, IndentedTextWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
