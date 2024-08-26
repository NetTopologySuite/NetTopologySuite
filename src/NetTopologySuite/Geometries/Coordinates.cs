namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Useful utility functions for handling Coordinate objects.
    /// </summary>
    public static class Coordinates
    {
        /// <summary>
        /// Factory method providing access to common Coordinate implementations.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns>created coordinate</returns>
        public static Coordinate Create(int dimension)
        {
            return Create(dimension, 0);
        }

        /// <summary>
        /// Factory method providing access to common Coordinate implementations.
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="measures"></param>
        /// <returns>created coordinate</returns>
        public static Coordinate Create(int dimension, int measures)
        {
            if (dimension == 2)
            {
                return new Coordinate();
            }
            else if (dimension == 3 && measures == 0)
            {
                return new CoordinateZ();
            }
            else if (dimension == 3 && measures == 1)
            {
                return new CoordinateM();
            }
            else if (dimension == 4 && measures == 1)
            {
                return new CoordinateZM();
            }

            // JTS deviation: we can do better.
            return new ExtraDimensionalCoordinate(dimension, measures);
        }

        /// <summary>
        /// Determine dimension based on subclass of <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="coordinate">supplied coordinate</param>
        /// <returns>number of ordinates recorded</returns>
        public static int Dimension(Coordinate coordinate)
        {
            // NTS-specific note: be VERY CAREFUL with methods that rely on checking the types of
            // Coordinate objects when compared to JTS: NTS offers the same four types (with
            // slightly different names), but with a substantially different hierarchy relationship.
            var type = coordinate?.GetType();
            if (type == typeof(Coordinate))
            {
                return 2;
            }

            if (type == typeof(CoordinateZ))
            {
                return 3;
            }

            if (type == typeof(CoordinateM))
            {
                return 3;
            }

            if (type == typeof(CoordinateZM))
            {
                return 4;
            }

            if (coordinate is ExtraDimensionalCoordinate extraDimensionalCoordinate)
            {
                return extraDimensionalCoordinate.Dimension;
            }

            // JTS deviation: JTS's default is 3, but that's because its base Coordinate class has Z
            // stored on it.  our base class doesn't.
            return 2;
        }

        /// <summary>
        /// Evaluates the number of spatial dimensions (x, y, z) that a coordinate can carry.
        /// </summary>
        /// <param name="c">A coordinate</param>
        /// <returns>The number of spatial dimensions</returns>
        public static int SpatialDimension(Coordinate c)
        {
            if (c is CoordinateZ) // includes CoordinateZM
                return 3;
            if (c is ExtraDimensionalCoordinate edc)
                return edc.Dimension - edc.Measures;

            return c != null ? 2 : 0;
        }


        /// <summary>
        /// Check if coordinate can store a z-ordinate value, based on subclass of <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="coordinate">A coordinate</param>
        /// <returns><c>true</c> if <paramref name="coordinate"/> can carry a z-ordinate value</returns>
        public static bool HasZ(Coordinate coordinate)
        {
            return SpatialDimension(coordinate) > 2;
        }

        /// <summary>
        /// Determine number of measures based on subclass of <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="coordinate">supplied coordinate</param>
        /// <returns>number of measures recorded </returns>
        public static int Measures(Coordinate coordinate)
        {
            // NTS-specific note: be VERY CAREFUL with methods that rely on checking the types of
            // Coordinate objects when compared to JTS: NTS offers the same four types (with
            // slightly different names), but with a substantially different hierarchy relationship.
            var type = coordinate?.GetType();
            if (type == typeof(CoordinateM) || type == typeof(CoordinateZM))
            {
                return 1;
            }

            if (coordinate is ExtraDimensionalCoordinate extraDimensionalCoordinate)
            {
                return extraDimensionalCoordinate.Measures;
            }

            return 0;
        }
    }
}
