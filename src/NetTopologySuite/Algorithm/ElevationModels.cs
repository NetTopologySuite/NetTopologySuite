using NetTopologySuite.Geometries;
using System;
using System.Buffers;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Extension methods to work with <see cref="ElevationModel"/>s.
    /// </summary>
    public static class ElevationModels
    {
        /// <summary>
        /// Creates a copy of <paramref name="c"/> that has the z-ordinate value at <paramref name="c"/>.
        /// If the elevation model can't retrieve a z-ordinate value, a copy of <paramref name="c"/> is
        /// returned.
        /// </summary>
        /// <param name="self">The elevation model to use</param>
        /// <param name="c">A coordinate</param>
        /// <returns>A copy of <paramref name="c"/> with the z-ordinate value.</returns>
        public static Coordinate CopyWithZ(this ElevationModel self, Coordinate c)
        {
            // If it already has a z-ordinate value, return a copy
            if (!double.IsNaN(c.Z)) return c.Copy();

            // Get the z-ordinate value for c. If no z-ordinate value was supplied, return a copy of c
            double z = self.GetZ(c);
            if (double.IsNaN(z)) return c.Copy();

            int dim = Coordinates.Dimension(c);
            int measures = Coordinates.Measures(c);
            int spatial = Math.Max(3, dim - measures);
            dim = spatial + measures;
            var copy = Coordinates.Create(dim, measures);
            copy.CoordinateValue = c;
            copy.Z = z;

            return copy;
        }

        /// <summary>
        /// Function to get z-ordinate values for an array of xy-ordinate values
        /// </summary>
        /// <param name="self">The elevation model to use</param>
        /// <param name="xy">An array of xy-ordinate values</param>
        /// <param name="z">The array of z-ordinate values</param>
        public static void GetZ(this ElevationModel self, double[] xy, double[] z)
        {
            self.GetZ(new ReadOnlySpan<double>(xy), new Span<double>(z));
        }

        /// <summary>
        /// Method signature to extract xy- and z-ordinate values from a coordinate sequence
        /// </summary>
        /// <param name="sequence">The sequence to extract xy- and z ordinates from</param>
        /// <param name="xy">The xy-ordinate values</param>
        /// <param name="z">The z-ordinate values</param>
        public delegate void CoordinateSequenceToXYAndZ(CoordinateSequence sequence, Span<double> xy, Span<double> z);

        /// <summary>
        /// Method to add missing z-ordinate values to a geometry. The geometry must be
        /// built of <see cref="CoordinateSequence"/>s that are able to carry z-ordinate values.
        /// </summary>
        /// <param name="self">The elevation model providing missing z-ordinate values</param>
        /// <param name="g">The geometry to add the missing z-ordinate values to</param>
        public static void AddMissingZ(this ElevationModel self, Geometry g)
            => AddMissingZ(self, g, CsToXYAndZ);


        /// <summary>
        /// Method to add missing z-ordinate values to a geometry. The geometry must be
        /// built of <see cref="CoordinateSequence"/>s that are able to carry z-ordinate values.
        /// </summary>
        /// <param name="self">The elevation model providing missing z-ordinate values</param>
        /// <param name="g">The geometry to add the missing z-ordinate values to</param>
        /// <param name="seqToXYAndZ">A method to convert a coordinate sequence into arrays of xy- and z-ordinate values.</param>
        public static void AddMissingZ(this ElevationModel self, Geometry g, CoordinateSequenceToXYAndZ seqToXYAndZ)
        {
            if (seqToXYAndZ == null)
                throw new ArgumentNullException(nameof(seqToXYAndZ));

            var flt = new AddMissingZFilter(self, seqToXYAndZ);
            g.Apply(flt);
        }

        /// <summary>
        /// A filter class to add z-ordinate values where they are missing
        /// </summary>
        private class AddMissingZFilter : IEntireCoordinateSequenceFilter
        {
            private readonly ElevationModel _elevationModel;
            private readonly CoordinateSequenceToXYAndZ _seqToXYAndZ;

            public AddMissingZFilter(ElevationModel elevationModel, CoordinateSequenceToXYAndZ seqToXYAndZ)
            {
                _elevationModel = elevationModel;
                _seqToXYAndZ = seqToXYAndZ;
            }

            // We need to handle all sequences
            public bool Done => false;

            // Only the z-ordiante values are changed so nothing to update
            // in the geometry itself
            public bool GeometryChanged => false;

            public void Filter(CoordinateSequence seq)
            {
                // If the sequence can't deal with z-ordinate values
                // don't even attempt to use the elevation model
                if (!seq.HasZ)
                    return;

                // Get buffer for ordinate values
                double[] xyz = ArrayPool<double>.Shared.Rent(3 * seq.Count);
                try
                {
                    var xys = new Span<double>(xyz, 0, 2 * seq.Count);
                    var zs = new Span<double>(xyz, 2 * seq.Count, seq.Count);
                    // Perform conversion to arrays
                    _seqToXYAndZ(seq, xys, zs);
                    // Get z-ordinate values
                    _elevationModel.GetZ(xys, zs);
                    // Assign z-ordinate values
                    for (int i = 0; i < zs.Length; i++)
                        seq.SetZ(i, zs[i]);
                }
                finally
                {
                    // Return buffer
                    ArrayPool<double>.Shared.Return(xyz);
                }
            }
        }

        /// <summary>
        /// Default -naive- implementation to convert sequence to arrays of xy- and z-ordinate values
        /// </summary>
        /// <param name="seq">A coordinate sequence</param>
        /// <param name="xy">An array with the xy-ordinates of <paramref name="seq"/>. It has to provide space for 2x <see cref="CoordinateSequence.Count"/> elements</param>
        /// <param name="z">An array with the z-ordinate values of <paramref name="seq"/>. It has to provide space for <see cref="CoordinateSequence.Count"/> elements</param>
        private static void CsToXYAndZ(CoordinateSequence seq, Span<double> xy, Span<double> z)
        {
            if (seq.Count == 0) return;

            if (xy == null || xy.Length < 2 * seq.Count)
                throw new ArgumentException("Null or not sized properly", nameof(xy));
            if (z == null || z.Length < seq.Count)
                throw new ArgumentException("Null or not sized properly", nameof(z));

            for (int i = 0, j = 0; i < seq.Count; i++)
            {
                xy[j++] = seq.GetX(i);
                xy[j++] = seq.GetY(i);
                z[i] = seq.GetZ(i);
            }
        }
    }
}
