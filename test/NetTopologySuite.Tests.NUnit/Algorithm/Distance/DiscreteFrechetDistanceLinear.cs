using NetTopologySuite.Geometries;
using System;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Distance
{
    /**
     * Linear Discrete Fréchet Distance computation
     */
    public class DiscreteFrechetDistanceLinear
    {

        /**
         * Computes the Discrete Fréchet Distance between two {@link Geometry}s
         * using a {@code cartesian} distance computation function.
         *
         * @param g0 the 1st geometry
         * @param g1 the 2nd geometry
         * @return the cartesian distance between {#g0} and {#g1}
         */
        public static double Distance(Geometry g0, Geometry g1)
        {
            var dist = new DiscreteFrechetDistanceLinear(g0, g1, false);
            return dist.Distance();
        }

        /**
         * Computes the Discrete Fréchet Distance between two {@link Geometry}s
         * using a {@code cartesian} distance computation function.
         *
         * @param g0 the 1st geometry
         * @param g1 the 2nd geometry
         * @return the cartesian distance between {#g0} and {#g1}
         */
        public static double distance(Geometry g0, Geometry g1, bool getCoordinates)
        {
            var dist = new DiscreteFrechetDistanceLinear(g0, g1, getCoordinates);
            return dist.Distance();
        }
        private readonly Geometry g0;
        private readonly Geometry g1;
        private readonly bool getCoordinates;

        private DiscreteFrechetDistanceLinear(Geometry g0, Geometry g1, bool getCoordinates)
        {
            this.g0 = g0;
            this.g1 = g1;
            this.getCoordinates = getCoordinates;
        }

        public double Distance()
        {

            var coords0 = this.g0.Coordinates;
            var coords1 = this.g1.Coordinates;
            double[][] distances = new double[coords0.Length][];
            for (int i = 0; i < coords0.Length; i++)
                distances[i] = new double[coords1.Length];

            for (int i = 0; i < coords0.Length; i++)
            {
                for (int j = 0; j < coords1.Length; j++)
                {
                    double distance = coords0[i].Distance(coords1[j]);
                    if (i > 0 && j > 0)
                    {
                        distances[i][j] = Math.Max(Math.Min(Math.Min(distances[i - 1][j], distances[i - 1][j - 1]), distances[i][j - 1]), distance);
                    }
                    else if (i > 0)
                    {
                        distances[i][j] = Math.Max(distances[i - 1][0], distance);
                    }
                    else if (j > 0)
                    {
                        distances[i][j] = Math.Max(distances[0][j - 1], distance);
                    }
                    else
                    {
                        distances[i][j] = distance;
                    }
                }
            }

            //System.out.println(toString(coords0.length, coords1.length, distances));
            //System.out.println();
            return distances[coords0.Length - 1][coords1.Length - 1];
        }

        /*
        // For debugging purposes only
        private static String toString(int numRows, int numCols,
                                       double[][] sparse) {

          StringBuilder sb = new StringBuilder("[");
          for (int i = 0; i < numRows; i++)
          {
            sb.append('[');
            for(int j = 0; j < numCols; j++)
            {
              if (j > 0)
                sb.append(", ");
              sb.append(String.format("%8.4f", sparse[i][j]));
            }
            sb.append(']');
            if (i < numRows - 1) sb.append(",\n");
          }
          sb.append(']');
          return sb.toString();
        }
         */

    }

}
