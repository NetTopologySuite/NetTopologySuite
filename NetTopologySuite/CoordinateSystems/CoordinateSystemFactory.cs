using System;

using GisSharpBlog.NetTopologySuite.CoordinateTransformations;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// Builds up complex objects from simpler objects or values.
	/// </summary>
	/// <remarks>
	/// <para>
	/// ICoordinateSystemFactory allows applications to make coordinate systems that cannot be
	/// created by a ICoordinateSystemAuthorityFactory. This factory is very flexible, whereas the
	/// authority factory is easier to use.
	/// </para>
	/// <para>
	/// So ICoordinateSystemAuthorityFactory can be used to make ‘standard’ coordinate systems,
	/// and ICoordinateSystemAuthorityFactory can be used to make ‘special’ coordinate systems.
	/// </para>
	/// <para>
	/// For example, the EPSG authority has codes for USA state plane coordinate systems using the
	/// NAD83 datum, but these coordinate systems always use meters. EPSG does not have codes for
	/// NAD83 state plane coordinate systems that use feet units. This factory lets an application create
	/// such a hybrid coordinate system.
	/// </para>
	/// </remarks>
	public class CoordinateSystemFactory : ICoordinateSystemFactory
	{		
		/// <summary>
		/// Initializes an instance of the CoordinateSystemFactory class.
		/// </summary>
		public CoordinateSystemFactory() { }

		#region Implementation of ICoordinateSystemFactory

		/// <summary>
		/// Creates a GCS, which could be Lat/Lon or Lon/Lat.
		/// </summary>
		/// <param name="name">The name of the coordinae system.</param>
		/// <param name="angularUnit">The angular units to use.</param>
		/// <param name="horizontalDatum">Ghe horizontal datum to use.</param>
		/// <param name="primeMeridian">The prime meridian to use.</param>
		/// <param name="axis0">Information about the x axis.</param>
		/// <param name="axis1">Information about the y axis.</param>
		/// <returns>an object that implements the IGeographicCoordinateSystem interface.</returns>
		public IGeographicCoordinateSystem CreateGeographicCoordinateSystem(string name, IAngularUnit angularUnit, 
            IHorizontalDatum horizontalDatum, IPrimeMeridian primeMeridian,  IAxisInfo axis0, IAxisInfo axis1)
		{
			return new GeographicCoordinateSystem(name, angularUnit, horizontalDatum, primeMeridian, axis0, axis1);
		}

		/// <summary>
		/// Creates a coordinate system object from an XML string.
		/// </summary>
		/// <param name="xml">XML representing the coordinate system.</param>
		/// <returns>An instance of a class that implements the ICoordinateSystem.</returns>
		public ICoordinateSystem CreateFromXML(string xml)
		{
			if(xml == null)
				throw new ArgumentNullException("xml");

			return (ICoordinateSystem)CoordinateSystemXmlReader.Create(xml);
		}

		/// <summary>
		/// Combines two coorindate system.
		/// </summary>
		/// <param name="name">The name of the new coordinate system.</param>
		/// <param name="headCS">The first coordinate system.</param>
		/// <param name="tailCS">The second coordinate system.</param>
		/// <returns>An object that implements the ICompoundCoordinateSystem interface.</returns>
		public ICompoundCoordinateSystem CreateCompoundCoordinateSystem(string name, ICoordinateSystem headCS, ICoordinateSystem tailCS)
		{
			if (headCS == null)
				throw new ArgumentNullException("headCS");

            if (tailCS == null)
				throw new ArgumentNullException("tailCS");

			CompoundCoordinateSystem compoundsCS = new CompoundCoordinateSystem(headCS, tailCS, String.Empty, String.Empty, String.Empty,name, String.Empty, String.Empty);
			return compoundsCS;
		}

		/// <summary>
		/// Creates a vertical coordinate system from a datum and linear units
		/// </summary>
		/// <param name="name">The name of the vertical coordinate system.</param>
		/// <param name="verticalDatum">The vertical datum to use.</param>
		/// <param name="verticalUnit">The units to use.</param>
		/// <param name="axis">The axis to use.</param>
		/// <returns>An an object that implements the IVerticalCoordinateSystem interface.</returns>
		public IVerticalCoordinateSystem CreateVerticalCoordinateSystem(string name, IVerticalDatum verticalDatum, ILinearUnit verticalUnit, IAxisInfo axis)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			
			if (verticalDatum == null)
				throw new ArgumentNullException("verticalDatum");

			if (verticalUnit == null)
				throw new ArgumentNullException("verticalUnit");

            VerticalCoordinateSystem verticalCS = new VerticalCoordinateSystem(name, verticalDatum, axis, verticalUnit);
			return verticalCS; 
		}

		/// <summary>
		/// Creates a projected coordinate system using a projection object.
		/// </summary>
		/// <param name="name">The name of the projected coordinate system.</param>
		/// <param name="geographicCoordinateSystem">The geographic coordinate system to base this coordinate system on.</param>
		/// <param name="projection">The projection details.</param>
		/// <param name="linearUnit">The linear units to use.</param>
		/// <param name="axis0">The X axis.</param>
		/// <param name="axis1">The Y aixs.</param>
		/// <returns>An object the implements the IProjectedCoordinateSystem interface.</returns>
		public IProjectedCoordinateSystem CreateProjectedCoordinateSystem(string name, IGeographicCoordinateSystem geographicCoordinateSystem, IProjection projection, ILinearUnit linearUnit,  IAxisInfo axis0,  IAxisInfo axis1)
		{
			if (name == null)
				throw new ArgumentNullException("name");
		    
            if (geographicCoordinateSystem == null)			
                throw new ArgumentNullException("geographicCoordinateSystem");
			
            if (projection == null)
				throw new ArgumentNullException("projection");
			
            if (linearUnit == null)
				throw new ArgumentNullException("linearUnit");
			
			IAxisInfo[] axisInfo = new IAxisInfo[2];
			axisInfo[0]=axis0;
			axisInfo[1]=axis1;
			ProjectedCoordinateSystem projectedCS = new ProjectedCoordinateSystem(null,axisInfo,geographicCoordinateSystem,linearUnit, projection);
			return projectedCS;
		}

		/// <summary>
		/// Creates a fitted coordinate system (not implemented). 
		/// </summary>
		/// <remarks>
		/// The units of the axes in the fitted coordinate system will be
		/// inferred from the units of the base coordinate system. If the affine map performs a rotation, then
		/// any mixed axes must have identical units. For example, a (lat_deg,lon_deg,height_feet) system
		/// can be rotated in the (lat,lon) plane, since both affected axes are in degrees. But you should not
		/// rotate this coordinate system in any other plane.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="baseCS"></param>
		/// <param name="toBaseWKT"></param>
		/// <param name="arAxes"></param>
		/// <returns></returns>
		public IFittedCoordinateSystem CreateFittedCoordinateSystem(string name, ICoordinateSystem baseCS, string toBaseWKT, IAxisInfo[] arAxes)
		{
			throw new NotImplementedException("FittedCoordinateSystem has not been implemented.");
		}

		/// <summary>
		/// Creates a local datum.
		/// </summary>
		/// <param name="name">The name of the datum.</param>
		/// <param name="localDatumType">The type of datum.</param>
		/// <returns>An object that implements the ILocalDatum interface.</returns>
		public ILocalDatum CreateLocalDatum(string name, DatumType localDatumType)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			
			return new LocalDatum(name, localDatumType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">Name to give new object.</param>
		/// <param name="wktProjectionClass">Classification string for projection (e.g. "Transverse_Mercator").</param>
		/// <param name="parameters">Parameters to use for projection. A default set of parameters can
		/// be constructed using classification and initialized
		/// using a chain of SetParameter(...) calls.</param>
		/// <returns>A projection.</returns>
		public IProjection CreateProjection(string name, string wktProjectionClass,  ProjectionParameter[] parameters)
		{	
			if (name == null)			
				throw new ArgumentNullException("name");			

			ParameterList parameterList = new ParameterList();
			for(int i=0; i < parameters.Length; i++)
			{
				ProjectionParameter param = parameters[i];
				parameterList.Add(param.Name,param.Value);
			}

			IProjection projection= null;
			switch (wktProjectionClass.ToLower())
			{
				case "transverse_mercator":
					projection = new TransverseMercatorProjection(parameterList);
					break;

				case "mercator":
					projection = new MercatorProjection(parameterList);
					break;
			    
                case "lambert_conformal_conic_2sp":
					projection = new LambertConformalConic2SPProjection(parameterList);
					break;
				
                case "albers":
					projection = new AlbersProjection(parameterList);
					break;
				
                default:
					throw new NotImplementedException(String.Format("The {0} projection is not supported",wktProjectionClass));
			}
			return projection;
		}

		/// <summary>
		/// Creates horizontal datum from ellipsoid and Bursa-World parameters. Since this method
		/// contains a set of Bursa-Wolf parameters, the created datum will always have a relationship to
		/// WGS84. If you wish to create a horizontal datum that has no relationship with WGS84, then you
		/// can either specify a horizontalDatumType of IHD_Other, or create it via WKT.
		/// </summary>
		/// <param name="name">The name of the datum to create.</param>
		/// <param name="horizontalDatumType">The IDatumType type use when creating the datum.</param>
		/// <param name="ellipsoid">The ellipsoid to use then creating the datum.</param>
		/// <param name="toWGS84">WKGS conversion parameters.</param>
		/// <returns>An object that implements the IHorizontalDatum interface.</returns>
		public IHorizontalDatum CreateHorizontalDatum(string name, DatumType horizontalDatumType, IEllipsoid ellipsoid,  WGS84ConversionInfo toWGS84)
		{
			if (name == null)			
				throw new ArgumentNullException("name");
			
			if (ellipsoid == null)
				throw new ArgumentNullException("ellipsoid");
			
			// no need to check horizontalDatumType and toWGS84 because they are value types.
			return new HorizontalDatum(name, horizontalDatumType, ellipsoid, toWGS84);
		}

		/// <summary>
		/// Creates a vertical datum from an enumerated type value.
		/// </summary>
		/// <param name="name">The name of the datum to create.</param>
		/// <param name="verticalDatumType">The IDatumType type use when creating the datum.</param>
		/// <returns>An object that implements the IVerticalDatum interface.</returns>
		public IVerticalDatum CreateVerticalDatum(string name, DatumType verticalDatumType)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			// no need to check verticalDatumType because IDatumType is an enum and cannot null.
			return new VerticalDatum(name, verticalDatumType);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="angularUnit"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
		public IPrimeMeridian CreatePrimeMeridian(string name, IAngularUnit angularUnit, double longitude)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (angularUnit == null)
				throw new ArgumentNullException("angularUnit");

			return new PrimeMeridian(name, angularUnit, longitude);
		}

		/// <summary>
		/// Creates a coordinate system object from a Well-Known Text string.
		/// </summary>
		/// <param name="wkt"></param>
		/// <returns></returns>
		public ICoordinateSystem CreateFromWKT(string wkt)
		{
			if (wkt == null)
				throw new ArgumentNullException("wkt");

			return (ICoordinateSystem) CoordinateSystemWktReader.Create( wkt );
		}

		/// <summary>
		/// Creates an ellipsoid from an major radius, and inverse flattening.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="semiMajorAxis">Double representing the equatorial radius.</param>
		/// <param name="inverseFlattening">Double representing the inverse of the flattening constant.</param>
		/// <param name="linearUnit">The linear units the radius are specified in.</param>
		/// <returns>An ellipsoid created with the specified parameters.</returns>
		public IEllipsoid CreateFlattenedSphere(string name, double semiMajorAxis, double inverseFlattening, ILinearUnit linearUnit)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			
			if (linearUnit == null)
				throw new ArgumentNullException("linearUnit");
			
			return new Ellipsoid(semiMajorAxis, -1.0, inverseFlattening, true,
                linearUnit, String.Empty, String.Empty, String.Empty, name, String.Empty, String.Empty);
		}

		/// <summary>
		/// Creates a local coordinate system (not implemented).
		/// </summary>
		/// <param name="name"></param>
		/// <param name="Datum"></param>
		/// <param name="Unit"></param>
		/// <param name="ArAxes"></param>
		/// <returns></returns>
		public ILocalCoordinateSystem CreateLocalCoordinateSystem(string name, ILocalDatum Datum, IUnit Unit,  IAxisInfo[] ArAxes)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Creates an ellipsoid from radius values.
		/// </summary>
		/// <param name="name">The name of the ellipsoid.</param>
		/// <param name="semiMajorAxis">Double representing the equatorial radius.</param>
		/// <param name="semiMinorAxis">Double representing the polar radius.</param>
		/// <param name="linearUnit">The linear units the radius are specified in.</param>
		/// <returns>An ellipsoid created with the specified parameters.</returns>
		public IEllipsoid CreateEllipsoid(string name, double semiMajorAxis, double semiMinorAxis, ILinearUnit linearUnit)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			
			if (linearUnit == null)
				throw new ArgumentNullException("linearUnit");
			
			return new Ellipsoid(semiMajorAxis, semiMinorAxis,1.0,false,linearUnit,String.Empty,String.Empty,String.Empty,name,String.Empty,String.Empty);
		}

		#endregion
	
    }
}
