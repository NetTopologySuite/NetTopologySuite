using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems
{
	/// <summary>
	/// The figure formed by the rotation of an ellipse about an axis.
	/// </summary>
	/// <remarks>
	/// In this context, the axis of rotation is always the smaller axis. It is named geodetic
	/// ellipsoid if the parameters are derived by the measurement of the shape and the size
	/// of the Earth to approximate the geoid as close as possible.
	/// </remarks>
	public class Ellipsoid : AbstractInformation, IEllipsoid
	{	
		bool _isIvfDefinitive = false;
		double _semiMajorAxis = 0.0;
		double _semiMinorAxis = 0.0;
		double _inverseFlattening = -1;
		ILinearUnit _linearUnit;

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the Ellipsoid class with the specified parameters.
		/// </summary>
		/// <param name="semiMajorAxis">Double representing semi major axis.</param>
		/// <param name="semiMinorAxis">Double representing semi smaller axis.</param>
		/// <param name="inverseFlattening">The inverse flattening.</param>
		/// <param name="isIvfDefinitive">Flag indicating wether the inverse flattening shoul be used to determine the semi-smaller axis.</param>
		/// <param name="linearUnit">The linar units to measure the ellipsoid.</param>
		internal Ellipsoid(	double semiMajorAxis, double semiMinorAxis, double inverseFlattening, bool isIvfDefinitive, ILinearUnit linearUnit) : 
            this(semiMajorAxis, semiMinorAxis, inverseFlattening,  isIvfDefinitive, linearUnit,
			    String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty) { }
	
		/// <summary>
		/// Initializes a new instance of the Ellipsoid class with the specified parameters.
		/// </summary>
		/// <param name="semiMajorAxis">Double representing semi major axis.</param>
		/// <param name="semiMinorAxis">Double representing semi smaller axis.</param>
		/// <param name="inverseFlattening">The inverse flattening.</param>
		/// <param name="isIvfDefinitive">Flag indicating wether the inverse flattening shoul be used to determine the semi-smaller axis.</param>
		/// <param name="linearUnit">The linar units to measure the ellipsoid.</param>
		/// <param name="remarks">Remarks about this object.</param>
		/// <param name="authority">The name of the authority.</param>
		/// <param name="authorityCode">The code the authority uses to identidy this object.</param>
		/// <param name="name">The name of the object.</param>
		/// <param name="alias">The alias of the object.</param>
		/// <param name="abbreviation">The abbreviated name of this object.</param>
		internal Ellipsoid(double semiMajorAxis, double semiMinorAxis, double inverseFlattening, bool isIvfDefinitive, ILinearUnit linearUnit,
			string remarks, string authority, string authorityCode, string name, string alias, string abbreviation) : 
                base(remarks, authority, authorityCode, name, alias, abbreviation)
		{
			if (linearUnit == null)
				throw new ArgumentNullException("linearUnit");
			
			_name= name;
			_linearUnit = linearUnit;
			_semiMajorAxis = semiMajorAxis;
			_inverseFlattening = inverseFlattening;
			_isIvfDefinitive = isIvfDefinitive;
			
            if (_isIvfDefinitive)
				 _semiMinorAxis = (1.0-(1.0/_inverseFlattening))* semiMajorAxis;			
			else _semiMinorAxis  = semiMinorAxis;
			
			_semiMajorAxis = semiMajorAxis;
			_inverseFlattening = inverseFlattening;

		}

		#endregion
	
		#region Static 

		/// <summary>
		/// WGS 1984 ellipsoid. This ellipsoid is used in GPS system.
		/// </summary>
		public static Ellipsoid WGS84Test
		{
			get
			{
				ILinearUnit meters = new LinearUnit(1.0);
				Ellipsoid ellipsoid = new Ellipsoid(6378137.0, -1.0, 298.257223563, true, meters,
                    String.Empty, String.Empty, String.Empty, "WGS84(default)", String.Empty, String.Empty);
				return ellipsoid;
			}
		}

		#endregion

		#region Implementation of IEllipsoid

		/// <summary>
		/// Is the Inverse Flattening definitive for this ellipsoid?
		/// </summary>
		/// <remarks>
		/// Is the Inverse Flattening definitive for this ellipsoid? Some ellipsoids use the IVF as the defining
		/// value, and calculate the polar radius whenever asked. Other ellipsoids use the polar radius to
		/// calculate the IVF whenever asked. This distinction can be important to avoid floating-point
		/// rounding errors.
		/// </remarks>
		/// <returns></returns>
		public bool IsIvfDefinitive()
		{
			return _isIvfDefinitive;
		}

		/// <summary>
		/// The equatorial radius. The returned length is expressed in this object's axis units.
		/// </summary>
		public double SemiMajorAxis
		{
			get
			{
				return _semiMajorAxis * _linearUnit.MetersPerUnit;
			}
		}		

		/// <summary>
		/// The inverse of the flattening value, or Double.PositiveInfinity  if the
		/// ellipsoid is a sphere.
		/// </summary>
		/// <remarks>
		/// The ratio of the distance between the center and a focus of the ellipse
		/// to the length of its semimajor axis. The eccentricity can alternately be
		/// computed from the equation: <code>e=sqrt(2f-f²)</code>.
		/// </remarks>
		public double InverseFlattening
		{
			get
			{
				return _inverseFlattening;
			}
		}

		/// <summary>
		/// The units the axis are defined in.
		/// </summary>
		public ILinearUnit AxisUnit
		{
			get
			{
				return _linearUnit;
			}
		}	

		/// <summary>
		/// The polar radius. The returned length is expressed in this object's axis units.
		/// </summary>
		public double SemiMinorAxis
		{
			get
			{
				return _semiMinorAxis * _linearUnit.MetersPerUnit;
			}
		}

		#endregion
	
    }
}
