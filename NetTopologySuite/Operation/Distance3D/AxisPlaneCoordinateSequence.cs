using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Distance3D
{
    /// <summary>
    /// A <see cref="CoordinateSequence"/> wrapper which
    /// projects 3D coordinates into one of the
    /// three Cartesian axis planes,
    /// using the standard orthonormal projection
    /// (i.e. simply selecting the appropriate ordinates into the XY ordinates).
    /// The projected data is represented as 2D coordinates.
    /// </summary>
    /// <author>Martin Davis</author>
    public class AxisPlaneCoordinateSequence : CoordinateSequence
    {

        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Creates a wrapper projecting to the XY plane.
        /// </summary>
        /// <param name="seq">The sequence to be projected</param>
        /// <returns>A sequence which projects coordinates</returns>
        public static CoordinateSequence ProjectToXY(CoordinateSequence seq)
        {
            /**
         * This is just a no-op, but return a wrapper
         * to allow better testing
         */
            return new AxisPlaneCoordinateSequence(seq, XYIndex);
        }

        /// <summary>
        /// Creates a wrapper projecting to the XZ plane.
        /// </summary>
        /// <param name="seq">The sequence to be projected</param>
        /// <returns>A sequence which projects coordinates</returns>
        public static CoordinateSequence ProjectToXZ(CoordinateSequence seq)
        {
            return new AxisPlaneCoordinateSequence(seq, XZIndex);
        }

        /// <summary>
        /// Creates a wrapper projecting to the YZ plane.
        /// </summary>
        /// <param name="seq">The sequence to be projected</param>
        /// <returns>A sequence which projects coordinates</returns>
        public static CoordinateSequence ProjectToYZ(CoordinateSequence seq)
        {
            return new AxisPlaneCoordinateSequence(seq, YZIndex);
        }

        private static readonly int[] XYIndex = new[] {0, 1};
        private static readonly int[] XZIndex = new[] {0, 2};
        private static readonly int[] YZIndex = new[] {1, 2};
        // ReSharper restore InconsistentNaming

        private readonly CoordinateSequence _seq;
        private readonly int[] _indexMap;

        private AxisPlaneCoordinateSequence(CoordinateSequence seq, int[] indexMap)
        {
            _seq = seq;
            _indexMap = indexMap;
        }

        public int Dimension => 2;

        /// <inheritdoc />
        public int Measures => 0;

        public bool HasZ => false;

        public bool HasM => false;

        public Ordinates Ordinates => _seq.Ordinates;

        /// <inheritdoc />
        public Coordinate CreateCoordinate() => new CoordinateZ();

        public Coordinate GetCoordinate(int i)
        {
            return GetCoordinateCopy(i);
        }

        public Coordinate GetCoordinateCopy(int i)
        {
            return new CoordinateZ(GetX(i), GetY(i), GetZ(i));
        }

        public void GetCoordinate(int index, Coordinate coord)
        {
            coord.X = GetOrdinate(index, 0);
            coord.Y = GetOrdinate(index, 1);
            coord.Z = GetOrdinate(index, 2);
        }

        public double GetX(int index)
        {
            return GetOrdinate(index, 0);
        }

        public double GetY(int index)
        {
            return GetOrdinate(index, 1);
        }

        public double GetZ(int index)
        {
            return GetOrdinate(index, 2);
        }

        public double GetM(int index)
        {
            return double.NaN;
        }

        public double GetOrdinate(int index, int ordinateIndex)
        {
            // Z ord is always 0
            if (ordinateIndex > 1) return 0;
            return _seq.GetOrdinate(index, _indexMap[ordinateIndex]);
        }

        public int Count => _seq.Count;

        public void SetOrdinate(int index, int ordinateIndex, double value)
        {
            throw new NotSupportedException();
        }

        public Coordinate[] ToCoordinateArray()
        {
            throw new NotSupportedException();
        }

        public Envelope ExpandEnvelope(Envelope env)
        {
            throw new NotSupportedException();
        }

        public CoordinateSequence Copy()
        {
            throw new NotSupportedException();
        }

        public CoordinateSequence Reversed()
        {
            throw new NotSupportedException();
        }

    }
}
