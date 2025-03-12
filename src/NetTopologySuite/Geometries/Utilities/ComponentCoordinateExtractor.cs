using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts a representative <see cref="Coordinate"/>
    /// from each connected component of a <see cref="Geometry"/>.
    /// </summary>
    /// <version>1.9</version>
    public class ComponentCoordinateExtracter : IGeometryComponentFilter
    {
        /// <summary>
        /// Extracts a representative <see cref="Coordinate"/>
        /// from each connected component in a geometry.
        /// <para/>
        /// If more than one geometry is to be processed, it is more
        /// efficient to create a single <see cref="ComponentCoordinateExtracter"/>
        /// instance and pass it to each geometry.
        /// </summary>
        /// <param name="geom">The Geometry from which to extract</param>
        /// <returns>A list of representative Coordinates</returns>
        public static List<Coordinate> GetCoordinates(Geometry geom)
        {
            var coords = new List<Coordinate>();
            geom.Apply(new ComponentCoordinateExtracter(coords));
            return coords;
        }

        private readonly List<Coordinate> _coords;

        /// <summary>
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        /// </summary>
        public ComponentCoordinateExtracter(List<Coordinate> coords)
        {
            _coords = coords;
        }

        public void Filter(Geometry geom)
        {
            if (geom.IsEmpty)
                return;

            // add coordinates from connected components
            if (geom is LineString
                || geom is Point)
                _coords.Add(geom.Coordinate);
        }
    }
}
