/*  
 * 
 * 
 *	A specific authority must implement these interfaces.
 * 
 * 
 * 
using System;


namespace Geotools.CoordinateReferenceSystems
{
	/// <summary>
	/// Summary description for ICoordinateSystemAuthorityFactory.
	/// </summary>
	public class CoordinateSystemAuthorityFactory : ICoordinateSystemAuthorityFactory
	{
		public CoordinateSystemAuthorityFactory()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#region Implementation of ICoordinateSystemAuthorityFactory
		public IGeographicCoordinateSystem CreateGeographicCoordinateSystem(string Code)
		{
			return null;
		}

		public ICompoundCoordinateSystem CreateCompoundCoordinateSystem(string Code)
		{
			return null;
		}

		public ILinearUnit CreateLinearUnit(string Code)
		{
			return null;
		}

		public string GeoidFromWKTName(string WKT)
		{
			return null;
		}

		public IVerticalCoordinateSystem CreateVerticalCoordinateSystem(string Code)
		{
			return null;
		}

		public IProjectedCoordinateSystem CreateProjectedCoordinateSystem(string Code)
		{
			return null;
		}

		public IAngularUnit CreateAngularUnit(string code)
		{
			IAngularUnit angularUnit = null;
			switch (code)
			{
				case "unittest" :
					angularUnit = new AngularUnit(1,"remarks","authority","authoritycode","name","alias","abbreviation");
					break;
				default:
					throw new Geographic.OpenGISException(String.Format("Angular unit with code '{0}' is not recognised",code)); 
			}
			return angularUnit;
		}

		public IHorizontalDatum CreateHorizontalDatum(string Code)
		{
			return null;
		}

		public string DescriptionText(string Code)
		{
			return null;
		}

		public IVerticalDatum CreateVerticalDatum(string Code)
		{
			return null;
		}

		public string WktGeoidName(string Geoid)
		{
			return null;
		}

		public IPrimeMeridian CreatePrimeMeridian(string Code)
		{
			return null;
		}

		public IEllipsoid CreateEllipsoid(string Code)
		{
			return null;
		}

		public IHorizontalCoordinateSystem CreateHorizontalCoordinateSystem(string Code)
		{
			return null;
		}

		public string Authority
		{
			get
			{
				return null;
			}
		}
		#endregion
	}
}
*/
