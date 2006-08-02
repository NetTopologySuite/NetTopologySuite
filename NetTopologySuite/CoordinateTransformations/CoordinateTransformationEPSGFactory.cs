using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{
	/// <summary>
	/// Creates coordinate transformation objects from codes. 
    /// The codes are maintained by an external authority. 
    /// A commonly used authority is EPSG, which is also used in the GeoTIFF standard.
	/// </summary>
	public class CoordinateTransformationEPSGFactory : ICoordinateTransformationAuthorityFactory
	{
		CoordinateSystemEPSGFactory coordSystemFactory = null;

		private DataSet dataSet;

		/// <summary>
		/// Initializes a new instance of the CoordinateTransformationEPSGFactory class.
		/// </summary>
		public CoordinateTransformationEPSGFactory(DataSet dataSet)
		{
			if (dataSet == null)
				throw new ArgumentNullException("dataSet");
			
			this.dataSet = dataSet;
			coordSystemFactory = new CoordinateSystemEPSGFactory(dataSet);
		}				

		/// <summary>
		/// Creates a transformation from a single transformation code.
		/// </summary>
		/// <remarks>
		/// The ‘Authority’ and ‘AuthorityCode’ values of the created object will be set to the authority of this object, and the code supplied by the client, respectively.  The other metadata values may or may not be set.
		/// </remarks>
		/// <param name="code">The EPSG code of the transformation to create.</param>
		/// <returns>An object that implements the ICoordinateTransformation interface.</returns>
		public ICoordinateTransformation CreateFromTransformationCode(string code)
		{
            DataRow[] rows = dataSet.Tables["Coordinate_Operation"].Select();
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

			string sourceCSCode = reader["SOURCE_CRS_CODE"].ToString();
			string targetCSCode = reader["TARGET_CRS_CODE"].ToString();
			string coordOpMethod = reader["COORD_OP_METHOD_CODE"].ToString();
			string areaOfUseCode = reader["AREA_OF_USE_CODE"].ToString();
			string authority = reader["DATA_SOURCE"].ToString();
			string name = reader["COORD_OP_NAME"].ToString();
			string remarks = reader["REMARKS"].ToString();
			string coordOpType = reader["COORD_OP_TYPE"].ToString().ToLower();			

			string areaOfUseDescription = this.GetAreaOfUse(areaOfUseCode);

			ICoordinateSystem sourceCS = null;
			ICoordinateSystem targetCS  = null;
			IMathTransform mathTransform = null;
			if (coordOpType == "transformation")
			{
				if (sourceCSCode == String.Empty || targetCSCode == String.Empty)
					throw new InvalidOperationException(String.Format("Coordinate operation {0} which is a transformation does not have a source or target coordinate system.",code));
				
				// create the coordinate systems. Use this helper method. The 
				// helper first determines if the coordinate system is a projected or geographic coordinate system
				// and then creates the right one. 
				sourceCS = coordSystemFactory.CreateCoordinateSystem( sourceCSCode );
				targetCS = coordSystemFactory.CreateCoordinateSystem( targetCSCode ); 
			
				// use the WGS84 ellipsoid if an ellipsoid is not defined.
				IEllipsoid ellipsoid = Ellipsoid.WGS84Test;
				if (sourceCS is IProjectedCoordinateSystem)
				{
					IProjectedCoordinateSystem projectedCS = (IProjectedCoordinateSystem)sourceCS;
					ellipsoid = projectedCS.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid;
					IProjection projection = projectedCS.Projection;
			
					mathTransform = CreateCoordinateOperation( projection, ellipsoid);
				}				
			}
			else if (coordOpType == "conversion")
			{
				// not sure what to do here.
				throw new NotImplementedException("Coordinate operation 'conversion' has not been implemented.");
				// use the the WGS84 ellipsoid as a default.
				//mathTransform = CreateCoordinateOperation(code, coordOpMethod, Ellipsoid.WGS84Test);
			}
			else if (coordOpType == "concatenated operation")
				throw new NotImplementedException("concatenated operation have not been implemented.");

			ICoordinateTransformation coordinateTransformation = new CoordinateTransformation(
							TransformType.Transformation,
							targetCS,
							sourceCS,
							mathTransform,
							code,
							authority,
							name,
							areaOfUseDescription,
							remarks,String.Empty);
	
			return coordinateTransformation;
		}

		/// <summary>
		/// Creates a transformation from coordinate system codes.
		/// </summary>
		/// <param name="sourceCRSCode">EPSG code of the first coordinate reference system.</param>
		/// <param name="targetCRSCode">EPSG code of the second coordinate reference system.</param>
		/// <returns>An object that implements the ICoordinateTransformation interface.</returns>
		public ICoordinateTransformation CreateFromCoordinateSystemCodes(string sourceCRSCode, string targetCRSCode)
		{
			ICoordinateSystem sourceCRS = null;
			ICoordinateSystem targetCRS  = null;
			if (sourceCRSCode != String.Empty)
				sourceCRS = coordSystemFactory.CreateCoordinateSystem(sourceCRSCode);

			if (targetCRSCode != String.Empty)
				targetCRS = coordSystemFactory.CreateCoordinateSystem(targetCRSCode);
		
			IEllipsoid ellipsoid = Ellipsoid.WGS84Test;
			IMathTransform mathTransform = null;
			if (sourceCRS is IProjectedCoordinateSystem)
			{
				IProjectedCoordinateSystem projectedCS = (IProjectedCoordinateSystem)sourceCRS;
				ellipsoid = projectedCS.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid;
				mathTransform = CreateCoordinateOperation(projectedCS.Projection,ellipsoid);
			}

			string areaOfUseDescription=String.Empty;
			string authority=String.Empty;
			string code=String.Empty;
			string remarks=String.Empty;
			string name=String.Empty;
		
			ICoordinateTransformation coordinateTransformation = new CoordinateTransformation(
				TransformType.Transformation,
				targetCRS,
				sourceCRS,
				mathTransform,
				code,
				authority,
				name,
				areaOfUseDescription,
				remarks,String.Empty);
	
			return coordinateTransformation;
		}


		/// <summary>
		/// Gets the authority.
		/// </summary>
		public string Authority
		{
			get
			{
				return "EPSG";
			}
		}

		/// <summary>
		/// Looks up the textual description given an EPSG code for the area of use.
		/// </summary>
		/// <param name="code">The EPSG code.</param>
		/// <returns>String describing the area of use.</returns>
		private string GetAreaOfUse(string code)
		{
            DataRow[] rows = dataSet.Tables["AREA"].Select();
            DataRow reader = null;
            foreach (DataRow row in rows)
            {
                if (row["AREA_CODE"].ToString() == code)
                {
                    reader = row;
                    break;
                }
            }
            Assert.IsTrue(reader != null);				
			
            string area = reader["AREA_OF_USE"].ToString();			
			return area;
		}

		/// <summary>
		/// Given a IProjection and a IEllipsoid, createa a IMathTransform with the required parameters.
		/// </summary>
		/// <param name="projection">The projection information.</param>
		/// <param name="ellipsoid">The ellipsoid to use.</param>
		/// <returns>An object that implements the IMathTransform interface.</returns>
		private IMathTransform CreateCoordinateOperation(IProjection projection, IEllipsoid ellipsoid)
		{
			ParameterList parameterList = new ParameterList();
			for(int i = 0; i < projection.NumParameters; i++)
			{
				ProjectionParameter param = projection.GetParameter(i);
				parameterList.Add(param.Name,param.Value);
			}
			parameterList.Add("semi_major", ellipsoid.SemiMajorAxis);
			parameterList.Add("semi_minor", ellipsoid.SemiMinorAxis);

			IMathTransform transform = null;
			switch(projection.AuthorityCode)
			{
				case "9804":
					//1SP
					transform = new MercatorProjection(parameterList);
					break;
				case "9805":
					//2SP
					transform = new MercatorProjection(parameterList);
					break;
				case "9807": 
					transform = new TransverseMercatorProjection(parameterList);
					break;
				case "9633": 
					// we should get these parameters from the file - but since we know them....
					ParameterList param = new ParameterList();
					parameterList.Add("latitude_of_natural_origin",49.0);
					parameterList.Add("longitude_of_natural_origin",-2.0);
					parameterList.Add("scale_factor_at_natural_origin",0.999601272);
					parameterList.Add("false_easting",400000.0);
					parameterList.Add("false_northing",-100000.0);
					transform = new MercatorProjection(parameterList);
					break;
				default:
					throw new NotSupportedException(String.Format("Projection {0} is not supported.",projection.AuthorityCode));
			}
			return transform;
		}
	}
}
