using System;

using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.CoordinateTransformations
{
	/// <summary>
	/// Summary description for MathTransform.
	/// </summary>
	/// <remarks>
	/// <para>Universal (UTM) and Modified (MTM) Transverses Mercator projections. This
	/// is a cylindrical projection, in which the cylinder has been rotated 90°.
	/// Instead of being tangent to the equator (or to an other standard latitude),
	/// it is tangent to a central meridian. Deformation are more important as we
	/// are going futher from the central meridian. The Transverse Mercator
	/// projection is appropriate for region wich have a greater extent north-south
	/// than east-west.</para>
	/// 
	/// <para>Reference: John P. Snyder (Map Projections - A Working Manual,
	///            U.S. Geological Survey Professional Paper 1395, 1987)</para>
	/// </remarks>
	internal class TransverseMercatorProjection : MapProjection
	{
	
        // Diego guidi say's:
        // Removed static in those variables!

		/* Variables common to all subroutines in this code file
           -----------------------------------------------------*/
		double RMajor;		        /* major axis 				*/
		double RMinor;		        /* smaller axis 				*/
		double ScaleFactor;	        /* scale factor				*/
		double LonCenter;	        /* Center longitude (projection center) */
		double LatOrigin;	        /* center latitude			*/
		double E0, E1, E2, E3;	    /* eccentricity constants		*/
		double E, ES, ESP;		    /* eccentricity constants		*/
		double ML0;		            /* small value m			*/
		double FalseNorthing;	    /* y offset in meters			*/
		double FalseEasting;	        /* x offset in meters			*/

		/// <summary>
		/// Creates an instance of an TransverseMercatorProjection projection object.
		/// </summary>
		/// <param name="parameters">List of parameters to initialize the projection.</param>
		public TransverseMercatorProjection(ParameterList parameters): this(parameters, false) { }

		/// <summary>
		/// Creates an instance of an TransverseMercatorProjection projection object.
		/// </summary>
		/// <param name="parameters">List of parameters to initialize the projection.</param>
		/// <param name="inverse">Flag indicating wether is a forward/projection (false) or an inverse projection (true).</param>
		/// <remarks>
		/// <list type="bullet">
		/// <listheader><term>Items</term><description>Descriptions</description></listheader>
		/// <item><term>semi_major</term><description>Your Description</description></item>
		/// <item><term>semi_minor</term><description>Your Description</description></item>
		/// <item><term>scale_factor_at_natural_origin</term><description>Your Description</description></item>
		/// <item><term>longitude_of_natural_origin</term><description>Your Description</description></item>
		/// <item><term>latitude_of_natural_origin</term><description>Your Description</description></item>
		/// <item><term>false_easting</term><description>Your Description</description></item>
		/// <item><term>false_northing</term><description>Your Description</description></item>
		/// </list>
		/// </remarks>
		public TransverseMercatorProjection(ParameterList parameters, bool inverse): base(parameters, inverse)
		{
			double r_maj = parameters.GetDouble("semi_major");			/* major axis			*/
			double r_min = parameters.GetDouble("semi_minor");			/* smaller axis			*/
			double scale_fact = parameters.GetDouble("scale_factor_at_natural_origin");		/* scale factor			*/
			double center_lon = Degrees.ToRadians(parameters.GetDouble("longitude_of_natural_origin"));		/* center longitude		*/
			double center_lat = Degrees.ToRadians(parameters.GetDouble("latitude_of_natural_origin"));		/* center latitude		*/
			double false_east = parameters.GetDouble("false_easting");	/* x offset in meters		*/
			double false_north = parameters.GetDouble("false_northing");		/* y offset in meters		*/

			double temp;			/* temporary variable		*/

			/* Place parameters in static storage for common use
			   -------------------------------------------------*/
			RMajor = r_maj;
			RMinor = r_min;
			ScaleFactor = scale_fact;
			LonCenter = center_lon;
			LatOrigin = center_lat;
			FalseNorthing = false_north;
			FalseEasting = false_east;

			temp = RMinor / RMajor;
			ES = 1.0 - SQUARE(temp);
			E = Math.Sqrt(ES);
			E0 = e0fn(ES);
			E1 = e1fn(ES);
			E2 = e2fn(ES);
			E3 = e3fn(ES);
			ML0 = RMajor * mlfn(E0, E1, E2, E3, LatOrigin);
			ESP = ES / (1.0 - ES);

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lon"></param>
		/// <param name="lat"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public override void DegreesToMeters(double lon, double lat,out double x, out double y)
		{
			lon = Degrees.ToRadians(lon);
			lat = Degrees.ToRadians(lat);

			double delta_lon=0.0;	/* Delta longitude (Given longitude - center 	*/
			double sin_phi, cos_phi;/* sin and cos value				*/
			double al, als;		/* temporary values				*/
			double c, t, tq;	/* temporary values				*/
			double con, n, ml;	/* cone constant, small m			*/
				
			delta_lon = adjust_lon(lon - LonCenter);
			sincos(lat, out sin_phi, out cos_phi);


			al  = cos_phi * delta_lon;
			als = SQUARE(al);
			c   = ESP * SQUARE(cos_phi);
			tq  = Math.Tan(lat);
			t   = SQUARE(tq);
			con = 1.0 - ES * SQUARE(sin_phi);
			n   = RMajor / Math.Sqrt(con);
			ml  = RMajor * mlfn(E0, E1, E2, E3, lat);

			x  = ScaleFactor * n * al * (1.0 + als / 6.0 * (1.0 - t + c + als / 20.0 *
				(5.0 - 18.0 * t + SQUARE(t) + 72.0 * c - 58.0 * ESP))) + FalseEasting;

			y  = ScaleFactor * (ml - ML0 + n * tq * (als * (0.5 + als / 24.0 *
				(5.0 - t + 9.0 * c + 4.0 * SQUARE(c) + als / 30.0 * (61.0 - 58.0 * t
				+ SQUARE(t) + 600.0 * c - 330.0 * ESP))))) + FalseNorthing;

			return;//(OK);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
		public override void MetersToDegrees(double x, double y,out double lon, out double lat)
		{
			double con,phi;		/* temporary angles				*/
			double delta_phi;	/* difference between longitudes		*/
			long i;			/* counter variable				*/
			double sin_phi, cos_phi, tan_phi;	/* sin cos and tangent values	*/
			double c, cs, t, ts, n, r, d, ds;	/* temporary variables		*/
			//double f, h, g, temp;			/* temporary variables		*/
			long max_iter = 6;			/* maximun number of iterations	*/

			x = x - FalseEasting;
			y = y - FalseNorthing;

			con = (ML0 + y / ScaleFactor) / RMajor;
			phi = con;
			for (i=0;;i++)
			{
				delta_phi = ((con + E1 * Math.Sin(2.0*phi) - E2 * Math.Sin(4.0*phi) + E3 * Math.Sin(6.0*phi))
					/ E0) - phi;
				/*
				   delta_phi = ((con + e1 * sin(2.0*phi) - e2 * sin(4.0*phi)) / e0) - phi;
				*/
				phi += delta_phi;
				if (Math.Abs(delta_phi) <= EPSLN) break;
				if (i >= max_iter) 
				{ 
					throw new TransformException("Latitude failed to converge"); 
					//return(95);
				}
			}
			if (Math.Abs(phi) < HALF_PI)
			{
				sincos(phi, out sin_phi, out cos_phi);
				tan_phi = Math.Tan(phi);
				c    = ESP * SQUARE(cos_phi);
				cs   = SQUARE(c);
				t    = SQUARE(tan_phi);
				ts   = SQUARE(t);
				con  = 1.0 - ES * SQUARE(sin_phi); 
				n    = RMajor / Math.Sqrt(con);
				r    = n * (1.0 - ES) / con;
				d    = x / (n * ScaleFactor);
				ds   = SQUARE(d);
				lat = phi - (n * tan_phi * ds / r) * (0.5 - ds / 24.0 * (5.0 + 3.0 * t + 
					10.0 * c - 4.0 * cs - 9.0 * ESP - ds / 30.0 * (61.0 + 90.0 * t +
					298.0 * c + 45.0 * ts - 252.0 * ESP - 3.0 * cs)));
				lon = adjust_lon(LonCenter + (d * (1.0 - ds / 6.0 * (1.0 + 2.0 * t +
					c - ds / 20.0 * (5.0 - 2.0 * c + 28.0 * t - 3.0 * cs + 8.0 * ESP +
					24.0 * ts))) / cos_phi));
			}
			else
			{
				lat = HALF_PI * sign(y);
				lon = LonCenter;
			}
			lon = Radians.ToDegrees(lon);
			lat = Radians.ToDegrees(lat);
		}
					
		/// <summary>
		/// Returns the inverse of this projection.
		/// </summary>
		/// <returns>IMathTransform that is the reverse of the current projection.</returns>
		public override IMathTransform GetInverse()
		{
			if (_inverse==null)
				_inverse = new TransverseMercatorProjection(this._parameters, ! _isInverse);
			return _inverse;
		}
	}
}
