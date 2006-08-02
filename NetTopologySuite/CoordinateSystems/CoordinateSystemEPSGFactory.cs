using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using GisSharpBlog.NetTopologySuite.CoordinateTransformations;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// A factory class that creates objects using codes defined by the <a href="http://www.epsg.org/">EPSG</a>.
	/// </summary>
	/// <remarks>
	/// EPSG, through its geodesy working group, maintains and publishes a data set of parameters for coordinate system and coordinate transformation description. The data is supported through formulae given in Guidance Note number 7. The EPSG Geodetic Parameters have been included as reference data in the GeoTIFF data exchange specifications, in the Iris21 data model and in Epicentre (the POSC data model). 
	/// </remarks>
	public class CoordinateSystemEPSGFactory : ICoordinateSystemAuthorityFactory
	{			
		private DataSet dataSet;

		/// <summary>
		/// Initializes a new instance of the CoordinateSystemEPSGFactory class.
		/// </summary>        
		public CoordinateSystemEPSGFactory(DataSet dataSet)
		{
			if (dataSet == null)
				throw new ArgumentNullException("dataSet");
            this.dataSet = dataSet;            
		}		

		/// <summary>
		/// Returns a GeographicCoordinateSystem object from a code.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>An object that implements the IGeographicCoordinateSystem interface.</returns>
		public IGeographicCoordinateSystem CreateGeographicCoordinateSystem(string code)
		{
			if (code == null)			
				throw new ArgumentNullException("code");

            DataRow[] rows = dataSet.Tables["Coordinate Reference System"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["COORD_REF_SYS_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
            
			string coordSysCode = reader["COORD_SYS_CODE"].ToString().ToLower();
			string coordSysName = reader["COORD_REF_SYS_NAME"].ToString();
			string name = reader["COORD_REF_SYS_NAME"].ToString();
			string horizontalDatumCode = reader["DATUM_CODE"].ToString();
			string coordRefKind = reader["COORD_REF_SYS_KIND"].ToString();
			string datasource = reader["DATA_SOURCE"].ToString(); // should always be EPSG??
			string remarks = reader["REMARKS"].ToString();
			
			if (coordRefKind.ToLower() != "geographic 2d")			
				throw new ArgumentException(String.Format("CRS code {0} is not a geographic coordinate system but a {1}.",code,coordRefKind));			

			string primeMeridianCode = String.Empty;
			IPrimeMeridian primeMeridian = null;
			IHorizontalDatum horizontalDatum= null;
			if (horizontalDatumCode == String.Empty)
			{
				horizontalDatum = HorizontalDatum.WGS84;
				primeMeridianCode = this.CreatePrimeMeridianCodeFromDatum(horizontalDatumCode);
				primeMeridian  = this.CreatePrimeMeridian( primeMeridianCode );
			}
			else
			{
				horizontalDatum = this.CreateHorizontalDatum( horizontalDatumCode );
				primeMeridianCode = this.CreatePrimeMeridianCodeFromDatum(horizontalDatumCode);
				primeMeridian  = this.CreatePrimeMeridian( primeMeridianCode );
			}

			// we get the information for the axis 
			IAxisInfo[] axisInfos = GetAxisInfo(coordSysCode);
			IAngularUnit angularUnit = new AngularUnit(1);
			
			
			IAxisInfo axisInfo1 = axisInfos[0];
			IAxisInfo axisInfo2 = axisInfos[1];
			IGeographicCoordinateSystem geographicCoordSys = new GeographicCoordinateSystem(angularUnit, horizontalDatum, primeMeridian, axisInfo1, axisInfo2,remarks,datasource,code,name,String.Empty,String.Empty);
			return geographicCoordSys;
			
		}

		/// <summary>
		/// Creates a 3D coordinate system from a code.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>An object that implements the ICompoundCoordinateSystem interface.</returns>
		public ICompoundCoordinateSystem CreateCompoundCoordinateSystem(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");			
                        
            DataRow[] rows = dataSet.Tables["Coordinate Reference System"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["COORD_REF_SYS_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);

			string coordSysCode = reader["COORD_SYS_CODE"].ToString().ToLower();
			string coordSysName = reader["COORD_REF_SYS_NAME"].ToString();
			string name = reader["COORD_REF_SYS_NAME"].ToString();
			string verticalCRSCode = reader["CMPD_VERTCRS_CODE"].ToString();
			string horizontalCRSCode = reader["CMPD_HORIZCRS_CODE"].ToString();
			string coordRefKind = reader["COORD_REF_SYS_KIND"].ToString();
			string remarks = reader["REMARKS"].ToString();
			string datasource = reader["DATA_SOURCE"].ToString(); // should always be EPSG??
			
			if (coordRefKind.ToLower() != "compound")
				throw new ArgumentException(String.Format("CRS code {0} is not a projected coordinate system but a {1}.",code,coordRefKind));
		
			ICoordinateSystem   headCRS = this.CreateCoordinateSystem( horizontalCRSCode);
			ICoordinateSystem   tailCRS = this.CreateCoordinateSystem( verticalCRSCode  );									

			ICompoundCoordinateSystem compoundCRS = new CompoundCoordinateSystem(headCRS, tailCRS, remarks, datasource, code, name,String.Empty,String.Empty);
			return compoundCRS;	
		}
			
		/// <summary>
		/// Returns a LinearUnit object from a code.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>An object that implements the ILinearUnit interface.</returns>
		public ILinearUnit CreateLinearUnit(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");            			
                                    
            DataRow[] rows = dataSet.Tables["Unit of Measure"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["UOM_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
			
			string unitOfMeasureType = reader["UNIT_OF_MEAS_TYPE"].ToString();
			if (unitOfMeasureType.ToLower() != "length")
				throw new ArgumentException(String.Format("Requested unit ({0}) is not a linear unit.",unitOfMeasureType));

			double metersPerUnit = Convert.ToDouble(reader["FACTOR_B"], CultureInfo.InvariantCulture);
			double factor = Convert.ToDouble(reader["FACTOR_C"], CultureInfo.InvariantCulture);
			string remarks = reader["REMARKS"].ToString();
			string name = reader["UNIT_OF_MEAS_NAME"].ToString();
            
            return new LinearUnit(metersPerUnit * factor, remarks, "EPSG", code, name, String.Empty, String.Empty);            
		}

		/// <summary>
		/// Gets the Geoid code from a WKT name. In the OGC definition of WKT horizontal datums, the geoid is referenced by a quoted string, which is used as a key value.  This method converts the key value string into a code recognized by this authority.
		/// </summary>
		/// <param name="wkt">WKT text name.</param>
		/// <returns>String containing the Geoid.</returns>
		public string GeoidFromWKTName(string wkt)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates a new vertical coordinate system object from a code.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>An object that implements the IVerticalCoordinateSystem interface.</returns>
		public IVerticalCoordinateSystem CreateVerticalCoordinateSystem(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");			

            DataRow[] rows = dataSet.Tables["Coordinate Reference System"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {                
                if (row["COORD_REF_SYS_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
			
			string coordSysCode = reader["COORD_SYS_CODE"].ToString().ToLower();
			string coordSysName = reader["COORD_REF_SYS_NAME"].ToString();
			string name = reader["COORD_REF_SYS_NAME"].ToString();
			string verticalDatumCode = reader["DATUM_CODE"].ToString();
			string coordRefKind = reader["COORD_REF_SYS_KIND"].ToString();
			string remarks = reader["REMARKS"].ToString();
			string datasource = reader["DATA_SOURCE"].ToString(); // should always be EPSG??
			
			if (coordRefKind.ToLower() != "vertical")
				throw new ArgumentException(String.Format("CRS code {0} is not a projected coordinate system but a {1}.",code,coordRefKind));
			
			IVerticalDatum verticalDatum = this.CreateVerticalDatum(verticalDatumCode);
			VerticalCoordinateSystem vrs = new VerticalCoordinateSystem(coordSysName, verticalDatum,remarks,datasource,code,String.Empty,String.Empty);
			return vrs;
		}

		/// <summary>
		/// Creates a projected coordinate system using the given code.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>A IProjectedCoordinateSystem object.</returns>
		public IProjectedCoordinateSystem CreateProjectedCoordinateSystem(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

            DataRow[] rows = dataSet.Tables["Coordinate Reference System"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["COORD_REF_SYS_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
			
			string coordSysCode = reader["COORD_SYS_CODE"].ToString().ToLower();
			string coordSysName = reader["COORD_REF_SYS_NAME"].ToString();
			string name = reader["COORD_REF_SYS_NAME"].ToString();
			string horizontalDatumCode = reader["DATUM_CODE"].ToString();
			string geographicCRSCode = reader["SOURCE_GEOGCRS_CODE"].ToString();
			string projectionCode = reader["PROJECTION_CONV_CODE"].ToString();
			string coordRefKind = reader["COORD_REF_SYS_KIND"].ToString();
			string remarks = reader["REMARKS"].ToString();
			string datasource = reader["DATA_SOURCE"].ToString(); // should always be EPSG??

			if (coordRefKind.ToLower() != "projected")
				throw new ArgumentException(String.Format("CRS code {0} is not a projected coordinate system but a {1}.",code,coordRefKind));

			string primeMeridianCode = String.Empty;
			IPrimeMeridian primeMeridian = null;
			IHorizontalDatum horizontalDatum= null;
			if (horizontalDatumCode != String.Empty)
			{
				horizontalDatum = HorizontalDatum.WGS84;//this.CreateHorizontalDatum( horizontalDatumCode );
				primeMeridianCode = this.CreatePrimeMeridianCodeFromDatum(horizontalDatumCode);
				primeMeridian  = this.CreatePrimeMeridian( primeMeridianCode );
			}

			// we get the information for the axis 
			IAxisInfo[] axisInfos = GetAxisInfo(coordSysCode);
			
			ICoordinateTransformationAuthorityFactory factory = new CoordinateTransformationEPSGFactory(dataSet);

			ICoordinateTransformation mathtransform = factory.CreateFromCoordinateSystemCodes(geographicCRSCode,String.Empty);
			string methodOperation = this.GetMethodOperationCodeFromProjectionCode( projectionCode );
			IProjection projection = this.CreateProjection(methodOperation, projectionCode);
			IGeographicCoordinateSystem geographicCoordSystem = this.CreateGeographicCoordinateSystem( geographicCRSCode );
			ILinearUnit linearUnit = LinearUnit.Meters;
			IProjectedCoordinateSystem projectedCoordSys = new ProjectedCoordinateSystem(horizontalDatum, axisInfos,geographicCoordSystem, linearUnit, projection, remarks,datasource,code,coordSysName,String.Empty,String.Empty);
													
			return projectedCoordSys;
		}
		
		/// <summary>
		/// Returns an AngularUnit object from a code.
		/// </summary>
		/// <remarks>
		/// Some common angular units and their codes are described in the table below.
		/// <list type="table">
		/// <listheader><term>EPSG Code</term><description>Descriptions</description></listheader>
		/// <item><term>9101</term><description>Radian</description></item>
		/// <item><term>9102</term><description>Degree</description></item>
		/// <item><term>9103</term><description>Arc-minute</description></item>
		/// <item><term>9104</term><description>Arc-second</description></item>
		/// </list>
		/// </remarks>
		/// <param name="code">The EPSG code.</param>
		/// <returns>An object that implements the IAngularUnit interface.</returns>
		public IAngularUnit CreateAngularUnit(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");			

            DataRow[] rows = dataSet.Tables["Unit of Measure"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["UOM_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);            
            
			string unitOfMeasureType = reader["UNIT_OF_MEAS_TYPE"].ToString();
			if (unitOfMeasureType.ToLower() != "angle")
				throw new ArgumentException(String.Format("Requested unit ({0}) is not a angular unit.",unitOfMeasureType));
			string remarks = reader["REMARKS"].ToString();
			string name = reader["UNIT_OF_MEAS_NAME"].ToString();
			string targetUOMcode = reader["TARGET_UOM_CODE"].ToString();

            IAngularUnit angularUnit = null;			
			if (!(reader["FACTOR_B"].ToString() == String.Empty))   
			{
                double factorB = Convert.ToDouble(reader["FACTOR_B"].ToString(), CultureInfo.InvariantCulture);
				double factorC = Convert.ToDouble(reader["FACTOR_C"].ToString(), CultureInfo.InvariantCulture);
				double radiansPerUnit = factorB/ factorC;				
				angularUnit = new AngularUnit(radiansPerUnit, remarks, "EPSG",code,name,String.Empty,String.Empty);
			}
			else
				// some units have a null for the Factor B - so must then try using the other UOM code.				
				angularUnit = this.CreateAngularUnit(targetUOMcode);
			return angularUnit;
		}

		/// <summary>
		/// Creates a horizontal datum from a code.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>An object that implements the IHorizontalDatum interface.</returns>
		public IHorizontalDatum CreateHorizontalDatum(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

            DataRow[] rows = dataSet.Tables["Datum"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["DATUM_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
			
            string datumtype = reader["DATUM_TYPE"].ToString();			
			string ellipsoidCode = reader["ELLIPSOID_CODE"].ToString();
			string primeMeridianCode = reader["PRIME_MERIDIAN_CODE"].ToString();
			string name = reader["DATUM_NAME"].ToString();
			string remarks = reader["REMARKS"].ToString();
			string datasource = reader["DATA_SOURCE"].ToString();
			
			WGS84ConversionInfo wgsConversionInfo = new WGS84ConversionInfo();
			IEllipsoid ellipsoid = this.CreateEllipsoid( ellipsoidCode );
			IHorizontalDatum horizontalDatum = new HorizontalDatum(name, DatumType.IHD_Geocentric, ellipsoid, wgsConversionInfo,remarks,datasource,code,String.Empty,String.Empty);
			return horizontalDatum;
		}

		/// <summary>
		/// Returns descriptive text about this factory.
		/// </summary>
		/// <remarks>This method has not been implemented.</remarks>
		/// <param name="code"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public string DescriptionText(string code)
		{			
			throw new NotImplementedException();
		}


		/// <summary>
		/// Create a vertical datum given a code.
		/// </summary>
		/// <param name="code">The EPSG code of the datum to create.</param>
		/// <returns>An object that implements the IVerticalDatum interface.</returns>
		public IVerticalDatum CreateVerticalDatum(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

            DataRow[] rows = dataSet.Tables["Datum"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["DATUM_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);

			string datumtype= reader["DATUM_TYPE"].ToString();			
			string name = reader["DATUM_NAME"].ToString();
			string remarks = reader["REMARKS"].ToString();
			string datasource = reader["DATA_SOURCE"].ToString();// should always be EPSG?

			if (datumtype.ToLower() != "vertical")
				throw new ArgumentException(String.Format("Requested datum ({0}) is not a vertical datum.", code));
			IVerticalDatum verticalDatum = new VerticalDatum(DatumType.IVD_GeoidModelDerived, remarks, code,"EPSG",name,String.Empty,String.Empty);
			return verticalDatum;
		}

		/// <summary>
		///	Gets the Geoid code from a WKT name. 	
		/// </summary>
		/// <remarks>In the OGC definition of WKT horizontal datums, the geoid is referenced by a quoted string, which is used as a key value.  This method converts the key value string into a code recognized by this authority.
		/// </remarks>
		/// <param name="wkt">The WKT name.</param>
		/// <returns>String with Geoid code.</returns>
		public string WktGeoidName(string wkt)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Creates a prime meridian given a code.
		/// </summary>
		/// <param name="code">The EPSG code of the prime meridian.</param>
		/// <returns>An object that implements the IPrimeMeridian interface.</returns>
		public IPrimeMeridian CreatePrimeMeridian(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

            DataRow[] rows = dataSet.Tables["Prime Meridian"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["PRIME_MERIDIAN_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
			
			double degreesFromGreenwich = Convert.ToDouble(reader["GREENWICH_LONGITUDE"].ToString(), CultureInfo.InvariantCulture);
			string remarks = reader["REMARKS"].ToString();
			string name = reader["PRIME_MERIDIAN_NAME"].ToString();
			string datasource = reader["DATA_SOURCE"].ToString();
			string unitsOfMeasure = reader["UOM_CODE"].ToString();

			IAngularUnit degrees = this.CreateAngularUnit(unitsOfMeasure);
			return new PrimeMeridian(name, degrees, degreesFromGreenwich,remarks,datasource,code,String.Empty,String.Empty);			
		}


		/// <summary>
		/// Creates an ellipsoid given a code.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>An object that implements the IEllipsoid interface.</returns>
		public IEllipsoid CreateEllipsoid(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");			

            DataRow[] rows = dataSet.Tables["Ellipsoid"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["ELLIPSOID_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
						
            int semiMinorAxisIndex = reader.Table.Columns["SEMI_MINOR_AXIS"].Ordinal;       // reader.GetOrdinal("SEMI_MINOR_AXIS")
            int inverseFlatteningIndex = reader.Table.Columns["INV_FLATTENING"].Ordinal;    // reader.GetOrdinal("INV_FLATTENING") 
			string ellipsoidName = reader["ELLIPSOID_NAME"].ToString();
			double semiMajorAxis = Convert.ToDouble(reader["SEMI_MAJOR_AXIS"].ToString(), CultureInfo.InvariantCulture);
			string unitsOfMearureCode = reader["UOM_CODE"].ToString();
			string datasource = reader["DATA_SOURCE"].ToString();
            bool ellipsoidShape = Convert.ToBoolean(reader["ELLIPSOID_SHAPE"].ToString()); ;
			string remarks = reader["REMARKS"].ToString();

            IEllipsoid ellipsoid = null;
			if (reader[semiMinorAxisIndex].ToString() == String.Empty)
			{
                double inverseFlattening = Convert.ToDouble(reader["INV_FLATTENING"].ToString(), CultureInfo.InvariantCulture);				
				ILinearUnit linearUnit = CreateLinearUnit(unitsOfMearureCode);
				ellipsoid = new Ellipsoid(semiMajorAxis,0.0,inverseFlattening,true,linearUnit, remarks,datasource,code,ellipsoidName,String.Empty,String.Empty);
			} 
			else if(reader[inverseFlatteningIndex].ToString() == String.Empty)
			{
				double semiMinorAxis = Convert.ToDouble(reader["SEMI_MINOR_AXIS"].ToString(), CultureInfo.InvariantCulture);
				ILinearUnit linearUnit = CreateLinearUnit(unitsOfMearureCode);
				ellipsoid = new Ellipsoid(semiMajorAxis,semiMinorAxis, 0.0,false, linearUnit, remarks,datasource,code,ellipsoidName,String.Empty,String.Empty);
			}
			return ellipsoid;
		}

		/// <summary>
		/// Creates a horizontal coordinate system given a code.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>An object that implements the IHorizontalCoordinateSystem interface.</returns>
		public IHorizontalCoordinateSystem CreateHorizontalCoordinateSystem(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

            DataRow[] rows = dataSet.Tables["Coordinate Reference System"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["COORD_REF_SYS_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
			
			string coordSysCode = reader["COORD_SYS_CODE"].ToString().ToLower();
			string coordSysName = reader["COORD_REF_SYS_NAME"].ToString();
			string name = reader["COORD_REF_SYS_NAME"].ToString();
			string datumCode = reader["DATUM_CODE"].ToString();
			string coordRefKind = reader["COORD_REF_SYS_KIND"].ToString();
			string remarks = reader["REMARKS"].ToString();
			string datasource = reader["DATA_SOURCE"].ToString(); // should always be EPSG??

			if (coordRefKind.ToLower() != "horizontal")
				throw new ArgumentException(String.Format("CRS code {0} is not a horizontal coordinate system but a {1}.",code,coordRefKind));
			IAxisInfo[] axisInfos = GetAxisInfo(coordSysCode);
			IHorizontalDatum horizontalDatum = this.CreateHorizontalDatum(datumCode);
			HorizontalCoordinateSystem vrs = new HorizontalCoordinateSystem(horizontalDatum, axisInfos,remarks,datasource,code,name,String.Empty,String.Empty);
			return vrs;
		}

		/// <summary>
		/// Gets the authorith which is ESPG.
		/// </summary>
		public string Authority
		{
			get
			{
				return "EPSG";
			}
		}		
	
		/// <summary>
		/// Creates a coordinate system.
		/// </summary>
		/// <remarks>
		/// Creates the right kind of coordinate system for the given code. The available coordinate systems are
		/// <list type="bullet">
		/// <item><term>geographic 2d</term><description>Your Description</description></item>
		/// <item><term>projected</term><description>Your Description</description></item>
		/// <item><term>vertical</term><description>Your Description</description></item>
		/// <item><term>horizontal</term><description>Your Description</description></item>
		/// </list>
		/// </remarks>
		/// <param name="code">The EPSG code of the coordinate system.</param>
		/// <returns>An object that implements the ICoordinateSystem interface. </returns>
		public ICoordinateSystem CreateCoordinateSystem(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

            DataRow[] rows = dataSet.Tables["Coordinate Reference System"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["COORD_REF_SYS_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);

			string coordRefKind = reader["COORD_REF_SYS_KIND"].ToString().ToLower();			

			ICoordinateSystem coordinateSystem = null;
			switch (coordRefKind)
			{
				case "geographic 2d":
					coordinateSystem = this.CreateGeographicCoordinateSystem(code);
					break;
				case "projected":
					coordinateSystem = this.CreateProjectedCoordinateSystem(code);
					break;
				case "vertical":
					coordinateSystem = this.CreateVerticalCoordinateSystem(code);
					break;
				case "horizontal":
					coordinateSystem = this.CreateHorizontalCoordinateSystem(code);
					break;
				default:
					throw new ArgumentException(String.Format("Coordinate system '{0}' (code='{1}') is not supported.",coordRefKind,code)); 
			}
			return coordinateSystem;
		}
		
		/// <summary>
		/// Returns the coordinate system type.
		/// </summary>
		/// <param name="code">The EPSG code for the coordinate system.</param>
		/// <returns>String with the coordinate system type.</returns>
		public string GetCoordinateSystemType(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

            DataRow[] rows = dataSet.Tables["Coordinate Reference System"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["COORD_REF_SYS_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);

			string coordSysType = reader["COORD_REF_SYS_KIND"].ToString();
			return coordSysType;
		}
		
        /// <summary>
		/// Returns an array containing axis information.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>IAxisInfo[] containing axis information.</returns>
		public IAxisInfo[] GetAxisInfo(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");
            
            DataRow[] rows = dataSet.Tables["Coordinate Axis"].Select();
            ArrayList selection = new ArrayList();
            foreach (DataRow row in rows)
                if (row["COORD_SYS_CODE"].ToString() == code)
                    selection.Add(row);
            rows = (DataRow[])selection.ToArray(typeof(DataRow));

            if (rows.Length == 0)
                throw new ArgumentException(String.Format("Could not find axis with a code of {0}", code));
                
            DataRow[] nameRows = dataSet.Tables["Coordinate Axis Name"].Select();
            ArrayList axisList = new ArrayList();
            foreach (DataRow reader in rows)
            {                            
                string searchCode = reader["COORD_AXIS_NAME_CODE"].ToString();                 
                foreach (DataRow nameReader in nameRows)
                {
                    if (nameReader["COORD_AXIS_NAME_CODE"].ToString() == searchCode)
                    {
                        string name = nameReader["COORD_AXIS_NAME"].ToString();
                        string orientation = reader["COORD_AXIS_ORIENTATION"].ToString();
                        IAxisInfo axisInfo = new AxisInfo(name, GetOrientation(orientation));
                        axisList.Add(axisInfo);
                        break;
                    }
                }
            }       
    
            return (IAxisInfo[])axisList.ToArray(typeof(IAxisInfo));
		}            

		/// <summary>
		/// Helper function to turn an string into an enumeration.
		/// </summary>
		/// <param name="code">The string representation of the axis orientation.</param>
		/// <returns>IAxisOrientationEnum enumation.</returns>
		/// <exception cref="NotSupportedException">If the code is not recognized.</exception>
		public AxisOrientation GetOrientation(string code)
		{
			if (code == null)
				throw new ArgumentNullException("code");

			AxisOrientation orientation = AxisOrientation.Other;
			switch (code.ToLower())
			{
				case "north": 
					orientation = AxisOrientation.North;
					break;
				case "south":
					orientation = AxisOrientation.South;
					break;
				case "east":
					orientation = AxisOrientation.East;
					break;
				case "west":
					orientation = AxisOrientation.West;
					break;
				case "up":
					orientation = AxisOrientation.Up;
					break;
				case "down":
					orientation = AxisOrientation.Down;
					break;
				default:
					throw new NotSupportedException(String.Format("The axis orientation '{0}' is not supported.",code));
			}
			return orientation;
		}

		/// <summary>
		/// Gets information about the parameters for a projection.
		/// </summary>
		/// <param name="projectionConversionCode">The projection code.</param>
		/// <param name="coordOperationMethod">The coordniate operation code.</param>
		/// <returns>IProjectionParameter[] containing information about the parameters.</returns>
		private ProjectionParameter[] GetProjectionParameterInfo(string projectionConversionCode, string coordOperationMethod)
		{
			ParameterList paramsList = GetParameters(projectionConversionCode);
			ProjectionParameter[] projectionParams = new ProjectionParameter[paramsList.Count];
			int i=0;
			foreach(string key in paramsList.Keys)
			{
				ProjectionParameter param = new ProjectionParameter(key,paramsList.GetDouble(key));
				projectionParams[i] = param;
				i++;
			}
			return projectionParams;
		}
		/// <summary>
		/// Gets projection parameters information.
		/// </summary>
		/// <param name="projectionConversionCode">The projeciton conversion code.</param>
		/// <returns>ParameterList with details about the projection parameters.</returns>
		public ParameterList GetParameters(string projectionConversionCode)
		{            
            DataRow[] paramUsages = dataSet.Tables["Coordinate_Operation Parameter Usage"].Select();            
            DataRow[] parametrs = dataSet.Tables["Coordinate_Operation Parameter"].Select();

            DataRow[] paramValues = dataSet.Tables["Coordinate_Operation Parameter Value"].Select();
            ArrayList selection = new ArrayList();
            foreach (DataRow paramValue in paramValues)
                if (paramValue["COORD_OP_CODE"].ToString() == projectionConversionCode)
                    selection.Add(paramValue);
            paramValues = (DataRow[])selection.ToArray(typeof(DataRow));


            selection = new ArrayList();
            foreach (DataRow paramValue in paramValues)            
            {
                foreach (DataRow paramUsage in paramUsages)
                {
                    foreach (DataRow parm in parametrs)
                    {
                        if ( (paramUsage["COORD_OP_METHOD_CODE"].ToString() == paramValue["COORD_OP_METHOD_CODE"].ToString())
                             && (paramUsage["PARAMETER_CODE"].ToString() == paramValue["PARAMETER_CODE"].ToString())
                             && (paramValue["PARAMETER_CODE"].ToString() == parm["PARAMETER_CODE"].ToString()) )
                                {
                                    selection.Add(parm);
                                    selection.Add(paramValue);
                                    selection.Add(paramUsage);
                                }
                    }
                }
            }

            /* 
             * We have a triple of rows: each triple is:
             *  0:  param.PARAMETER_NAME
             *  1:  paramValue.PARAMETER_VALUE
             *  2:  paramUsage.Param_Sign_Reversal
             */
            DataRow[] results = (DataRow[])selection.ToArray(typeof(DataRow));           
			ParameterList parameters = new ParameterList();
            for (int i = 0; i < results.Length; i = i + 3)
			{
                string paramNameLong = (results[i] as DataRow)["PARAMETER_NAME"].ToString();
				string paramNameShort = TranslateParamName(paramNameLong);
                string paramValueString = (results[i + 1] as DataRow)["PARAMETER_VALUE"].ToString();
				double paramValue = 0.0;
				if (paramValueString != String.Empty && paramValueString != null)
					paramValue = Convert.ToDouble(paramValueString, CultureInfo.InvariantCulture);
                string reverse = (results[i + 2] as DataRow)["Param_Sign_Reversal"].ToString();

				// for some reason params, all params are held positive, and there is a flag to determine if params are negative.
				if (reverse.ToLower() == "yes")
					paramValue = paramValue * -1.0;
				parameters.Add(paramNameShort, paramValue);
			}			
			return parameters;
		}

		private static string TranslateParamName(string paramLongName)
		{
			return paramLongName.ToLower().Replace(" ","_");			
		}

		/// <summary>
		/// Gets the prime meridian code for the specified datum.
		/// </summary>
		/// <param name="code">The ESP code.</param>
		/// <returns>String with the EPSG code for the prime meridian.</returns>
		private string CreatePrimeMeridianCodeFromDatum(string code)
		{
            DataRow[] rows = dataSet.Tables["Datum"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["DATUM_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
		
			string primeMeridianCode = reader["PRIME_MERIDIAN_CODE"].ToString();
			return primeMeridianCode;
		}
		
		private string GetMethodOperationCodeFromProjectionCode(string code)
		{
            DataRow[] rows = dataSet.Tables["Coordinate_Operation Parameter Value"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["COORD_OP_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);			

			string methodOp = reader["COORD_OP_METHOD_CODE"].ToString();			
			return methodOp;
		}

		private IProjection CreateProjection(string code, string projectionCode)
		{
            DataRow[] rows = dataSet.Tables["Coordinate_Operation Method"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["COORD_OP_METHOD_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);
			
			string name = reader["COORD_OP_METHOD_NAME"].ToString();
			string remarks = reader["REMARKS"].ToString();

			ProjectionParameter[] projectionParameters = this.GetProjectionParameterInfo(projectionCode,"ignore");
			Projection projection = new Projection(name,projectionParameters,name,remarks,"EPSG",code);
			return projection;
		}
	}
}
