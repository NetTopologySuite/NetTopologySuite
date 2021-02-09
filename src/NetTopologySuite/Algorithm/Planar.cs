using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    public abstract class Metric
    {
        public static Metric Planar => new PlanarMetric();

        public static Metric Sphere => new SphereMetric();


        public abstract double Length(CoordinateSequence sequence);

        public abstract double Area(CoordinateSequence sequence);

        private sealed class PlanarMetric : Metric
        {
            public override double Length(CoordinateSequence sequence)
            {
                if (sequence == null)
                    throw new ArgumentNullException(nameof(sequence));

                return Algorithm.Length.OfLine(sequence);
            }

            public override double Area(CoordinateSequence sequence)
            {
                if (sequence == null)
                    throw new ArgumentNullException(nameof(sequence));

                return Algorithm.Area.OfRing(sequence);
            }
        }

        private sealed class SphereMetric : Metric
        {
            private readonly int latIdx = 0;
            private readonly int lonIdx = 1;

#if AVIATION_FORMULARY

            public override double Length(CoordinateSequence sequence)
            {
                // d = 2 * asin(sqrt((sin((lat1 - lat2) / 2)) ^ 2 +
                //    cos(lat1) * cos(lat2) * (sin((lon1 - lon2) / 2)) ^ 2))
                if (sequence == null)
                    throw new ArgumentNullException(nameof(sequence));

                if (sequence.Count <= 1)
                    return 0d;

                var p = sequence.GetCoordinateCopy(0);
                double lat1 = AngleUtility.ToRadians(p[latIdx]);
                double lon1 = AngleUtility.ToRadians(p[lonIdx]);

                double res = 0d;
                for (int i = 0; i < sequence.Count; i++)
                {
                    sequence.GetCoordinate(i, p);
                    double lat2 = AngleUtility.ToRadians(p[latIdx]);
                    double lon2 = AngleUtility.ToRadians(p[lonIdx]);

                    double sindlat = Math.Sin((lat1 - lat2) * 0.5d);
                    double sindlon = Math.Sin((lon1 - lon2) * 0.5d);

                    res += 2 * Math.Asin(Math.Sqrt(sindlat * sindlat +
                                                   Math.Cos(lat1) * Math.Cos(lat2) * sindlon * sindlon));
                }

                return res;
            }
#else
            private readonly double _rf, _a, _b;

            public override double Length(CoordinateSequence sequence)
            {
                if (sequence == null)
                    throw new ArgumentNullException(nameof(sequence));

                if (sequence.Count <= 1)
                    return 0d;

                var p = sequence.GetCoordinateCopy(0);
                double lat1 = AngleUtility.ToRadians(p[latIdx]);
                double lon1 = AngleUtility.ToRadians(p[lonIdx]);

                double res = 0d;
                for (int i = 0; i < sequence.Count; i++)
                {
                    sequence.GetCoordinate(i, p);
                    double lat2 = AngleUtility.ToRadians(p[latIdx]);
                    double lon2 = AngleUtility.ToRadians(p[lonIdx]);

                    res += Length(lat1, lon1, lat2, lon2);
                    lat1 = lat2;
                    lon1 = lon2;
                }

                return res;
            }

            public double Length(double lat1, double lon1, double lat2, double lon2)
            {
                /*
                 * using Vincenty inverse formula for ellipsoids
                 * based on original JavaScript by (c) Chris Veness 2002-2008 
                 * http://www.movable-type.co.uk/scripts/latlong-vincenty.html
                 */
                double f = 1.0 / _rf;
                double L = (lon2 - lon1);
                double U1 = Math.Atan((1.0 - f) * Math.Tan(lat1));
                double U2 = Math.Atan((1.0 - f) * Math.Tan(lat2));
                double sinU1 = Math.Sin(U1);
                double cosU1 = Math.Cos(U1);
                double sinU2 = Math.Sin(U2);
                double cosU2 = Math.Cos(U2);
                double lambda = L;
                double lambdaP;
                double sinLambda;
                double cosLambda;
                double sinSigma;
                double cosSigma;
                double sigma;
                double sinAlpha;
                double cosSqAlpha;
                double cos2SigmaM;
                double C;
                double uSq;
                double A;
                double B;
                double deltaSigma;
                double s;
                int iterLimit = 100;
                do
                {
                    sinLambda = Math.Sin(lambda);
                    cosLambda = Math.Cos(lambda);
                    sinSigma =
                        Math.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) +

                             (cosU1 * sinU2 -

                              sinU1 * cosU2 * cosLambda) * (cosU1 * sinU2 -
                                                            sinU1 * cosU2 * cosLambda));
                    if (sinSigma == 0.0)
                        return 0.0; /* co-incident points */
                    cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                    sigma = Math.Atan2(sinSigma, cosSigma);
                    sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                    cosSqAlpha = 1.0 - sinAlpha * sinAlpha;
                    cos2SigmaM = cosSigma - 2.0 * sinU1 * sinU2 / cosSqAlpha;
                    if (double.IsNaN(cos2SigmaM))
                        cos2SigmaM = 0; /* equatorial line */
                    C = f / 16.0 * cosSqAlpha * (4.0 + f * (4.0 - 3.0 * cosSqAlpha));
                    lambdaP = lambda;
                    lambda =
                        L + (1.0 - C) * f * sinAlpha * (sigma +
                                                        C * sinSigma * (cos2SigmaM +
                                                                        C * cosSigma *

                                                                        (-1.0 +
                                                                         2.0 *
                                                                         cos2SigmaM *
                                                                         cos2SigmaM)));
                } while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

                if (iterLimit == 0)
                    return -1.0; /* formula failed to converge */
                uSq = cosSqAlpha * (_a * _a - _b * _b) / (_b * _b);
                A = 1.0 + uSq / 16384.0 * (4096.0 +
                                           uSq * (-768.0 + uSq * (320.0 - 175.0 * uSq)));
                B = uSq / 1024.0 * (256.0 + uSq * (-128.0 + uSq * (74.0 - 47.0 * uSq)));
                deltaSigma =
                    B * sinSigma * (cos2SigmaM +
                                    B / 4.0 * (cosSigma *

                                               (-1.0 + 2.0 * cos2SigmaM * cos2SigmaM) -

                                               B / 6.0 * cos2SigmaM * (-3.0 +
                                                                       4.0 * sinSigma *
                                                                       sinSigma) * (-3.0 +
                                                   4.0 *
                                                   cos2SigmaM
                                                   *
                                                   cos2SigmaM)));
                s = _b * A * (sigma - deltaSigma);
                return s;
            }
#endif

            public override double Area(CoordinateSequence sequence)
            {
                throw new NotImplementedException();
            }
        }
    }
}
