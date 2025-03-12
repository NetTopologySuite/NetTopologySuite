using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Computes the effective area of corners, 
    /// taking into account the smoothing weight.
    /// </summary>
    /// <remarks>
    /// <h3>FUTURE WORK</h3>
    /// <list type="bullet">
    /// <item><description>Support computing geodetic area</description></item>
    /// </list>
    /// </remarks>
    /// <author>Martin Davis</author>
    internal class CornerArea
    {
        public const double DEFAULT_SMOOTH_WEIGHT = 0.0;

        private double _smoothWeight;

        /// <summary>
        /// Creates a new corner area computer using the <see cref="DEFAULT_SMOOTH_WEIGHT"/>.
        /// </summary>
        public CornerArea() : this(DEFAULT_SMOOTH_WEIGHT)
        {
        }

        /// <summary>
        /// Creates a new corner area computer using the provided <paramref name="smoothWeight"/>
        /// </summary>
        /// <param name="smoothWeight">The weight for smoothing corners.  In range [0..1].</param>
        public CornerArea(double smoothWeight)
        {
            _smoothWeight = smoothWeight;
        }

        public double Area(Coordinate pp, Coordinate p, Coordinate pn)
        {

            double area = Triangle.Area(pp, p, pn);
            double ang = AngleNorm(pp, p, pn);
            //-- rescale to [-1 .. 1], with 1 being narrow and -1 being flat
            double angBias = 1.0 - 2.0 * ang;
            //-- reduce area for narrower corners, to make them more likely to be removed
            double areaWeighted = (1 - _smoothWeight * angBias) * area;
            return areaWeighted;
        }

        private static double AngleNorm(Coordinate pp, Coordinate p, Coordinate pn)
        {
            double angNorm = AngleUtility.AngleBetween(pp, p, pn) / 2 / Math.PI;
            return MathUtil.Clamp(angNorm, 0, 1);
        }
    }
}
