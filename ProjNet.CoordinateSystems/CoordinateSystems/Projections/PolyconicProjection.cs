/*
 * http://svn.osgeo.org/geotools/tags/2.6.2/modules/library/referencing/src/main/java/org/geotools/referencing/operation/projection/Polyconic.java
 * http://svn.osgeo.org/geotools/tags/2.6.2/modules/library/referencing/src/main/java/org/geotools/referencing/operation/projection/MapProjection.java
 */
using System;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems.Transformations;

namespace ProjNet.CoordinateSystems.Projections
{
    /// <summary>
    /// 
    /// </summary>
    internal class PolyconicProjection : MapProjection
    {
        /// <summary>
        /// Maximum difference allowed when comparing real numbers.
        /// </summary>
        private const double Epsilon = 1E-10;

        /// <summary>
        /// Maximum number of iterations for iterative computations.
        /// </summary>
        private const int MaximumIterations = 20;

        /// <summary>
        /// Difference allowed in iterative computations.
        /// </summary>
        private const double IterationTolerance = 1E-12;

        ///<summary>
        /// Meridian distance at the latitude of origin.
        /// Used for calculations for the ellipsoid.
        /// </summary>
        private readonly double _ml0;

        private readonly double _e0, _e1, _e2, _e3; //, _e4;

        ///<summary>
        /// Constructs a new map projection from the supplied parameters.
        ///</summary>
        /// <param name="parameters">The parameter values in standard units</param>
        public PolyconicProjection(List<ProjectionParameter> parameters)
            : this(parameters, false)
        { }

        /// <summary>
        /// Constructs a new map projection from the supplied parameters.
        /// </summary>
        /// <param name="parameters">The parameter values in standard units</param>
        /// <param name="isInverse">Defines if Projection is inverse</param>
        public PolyconicProjection(List<ProjectionParameter> parameters, bool isInverse)
            : base(parameters, isInverse)
        {
            ProjectionParameter latitude_of_origin = GetParameter("latitude_of_origin");
            if (latitude_of_origin == null)
                throw new ArgumentException("Missing projection parameter 'latitude_of_origin'");

            double latitudeOfOrigin = Degrees2Radians(latitude_of_origin.Value);

            //Compute constants
            _e0 = e0fn(_es);
            _e1 = e1fn(_es);
            _e2 = e1fn(_es);
            _e3 = e1fn(_es);
            //_e4 = e1fn(_es);

            _ml0 = mlfn(_e0, _e1, _e2, _e3, latitudeOfOrigin);
        }

        public override double[] DegreesToMeters(double[] lonlat)
        {

            double lam = Degrees2Radians(lonlat[0]);
            double phi = Degrees2Radians(lonlat[1]);

            double x, y;

            if (Math.Abs(phi) <= Epsilon)
            {
                x = lam;
                y = -_ml0;
            }
            else
            {
                double sp = Math.Sin(phi);
                double cp;
                double ms = Math.Abs(cp = Math.Cos(phi)) > Epsilon ? msfn(sp, cp) / sp : 0.0;
                lam *= sp;
                x = ms * Math.Sin(lam);
                y = (mlfn(_e0, _e1, _e2, _e3, phi) - _ml0) + ms * (1.0 - Math.Cos(lam));
            }

            return new double[] { x, y };
        }

        public override double[] MetersToDegrees(double[] p)
        {
            double x = p[0];
            double y = p[1];

            double lam, phi;

            y += _ml0;
            if (Math.Abs(y) <= Epsilon)
            {
                lam = x;
                phi = 0.0;
            }
            else
            {
                double r = y * y + x * x;
                phi = y;
                int i = 0;
                for (; i <= MaximumIterations; i++)
                {
                    double sp = Math.Sin(phi);
                    double cp = Math.Cos(phi);
                    if (Math.Abs(cp) < IterationTolerance)
                        throw new Exception("No Convergence");

                    double s2ph = sp * cp;
                    double mlp = Math.Sqrt(1.0 - _es * sp * sp);
                    double c = sp * mlp / cp;
                    double ml = mlfn(_e0, _e1, _e2, _e3, phi);
                    double mlb = ml * ml + r;
                    mlp = (1.0 - _es) / (mlp * mlp * mlp);
                    double dPhi = (ml + ml + c * mlb - 2.0 * y * (c * ml + 1.0)) / (
                                _es * s2ph * (mlb - 2.0 * y * ml) / c +
                                2.0 * (y - ml) * (c * mlp - 1.0 / s2ph) - mlp - mlp);
                    if (Math.Abs(dPhi) <= IterationTolerance)
                        break;

                    phi += dPhi;
                }
                if (i > MaximumIterations)
                    throw new Exception("No Convergence");
                double c2 = Math.Sin(phi);
                lam = Math.Asin(x * Math.Tan(phi) * Math.Sqrt(1.0 - _es * c2 * c2)) / Math.Sin(phi);
            }

            return new double[] { Radians2Degrees(lam), Radians2Degrees(phi) };
        }
        /// <summary>
        /// Returns the inverse of this projection.
        /// </summary>
        /// <returns>IMathTransform that is the reverse of the current projection.</returns>
        public override IMathTransform Inverse()
        {
            if (_inverse == null)
                _inverse = new PolyconicProjection(_Parameters, !_isInverse);
            return _inverse;
        }

        #region Private helpers
        ///<summary>
         /// Computes function <code>f(s,c,e²) = c/sqrt(1 - s²*e²)</code> needed for the true scale
         /// latitude (Snyder 14-15), where <var>s</var> and <var>c</var> are the sine and cosine of
         /// the true scale latitude, and <var>e²</var> is the eccentricity squared.
        ///</summary>
        double msfn(double s, double c)
        {
            return c / Math.Sqrt(1.0 - (s * s) * _es);
        }

        #endregion

    }
}