using GeoAPI.Geometries;

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

            return new CoordinateZ();
        }

        /// <summary>
        /// Determine dimension based on subclass of <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="coordiante"></param>
        /// <returns>dimension</returns>
        public static int GetDimension(Coordinate coordiante)
        {
            // NTS-specific note: be VERY CAREFUL with methods that rely on checking the types of
            // Coordinate objects when compared to JTS: GeoAPI offers the same four types (with
            // slightly different names), but with a substantially different hierarchy relationship.
            var type = coordiante.GetType();
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

            // JTS deviation: JTS's default is 3, but that's because its base Coordinate class has Z
            // stored on it.  our base class doesn't.
            return 2;
        }

        /// <summary>
        /// Determine dimension based on subclass of <see cref="Coordinate"/>.
        /// </summary>
        /// <param name="coordiante"></param>
        /// <returns>dimension</returns>
        public static int GetMeasures(Coordinate coordiante)
        {
            // NTS-specific note: be VERY CAREFUL with methods that rely on checking the types of
            // Coordinate objects when compared to JTS: GeoAPI offers the same four types (with
            // slightly different names), but with a substantially different hierarchy relationship.
            var type = coordiante.GetType();
            if (type == typeof(CoordinateM) || type == typeof(CoordinateZM))
            {
                return 1;
            }

            return 0;
        }
    }
}
