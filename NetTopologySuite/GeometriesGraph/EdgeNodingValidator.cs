using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Noding;
using NPack.Interfaces;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// Validates that a collection of SegmentStrings is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// </summary>
    public class EdgeNodingValidator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        public static void CheckValid(IGeometryFactory<TCoordinate> geoFactory, IEnumerable<Edge<TCoordinate>> edges)
        {
            EdgeNodingValidator<TCoordinate> env = new EdgeNodingValidator<TCoordinate>(geoFactory,edges);
            env.CheckValid();
        }

        private readonly FastNodingValidator<TCoordinate> _nodingValidator;

        public EdgeNodingValidator(IGeometryFactory<TCoordinate> geoFactory, IEnumerable<Edge<TCoordinate>> edges)
        {
            _nodingValidator = new FastNodingValidator<TCoordinate>(geoFactory, toSegmentStrings(edges));
        }

        private static IEnumerable<ISegmentString<TCoordinate>> toSegmentStrings(
            IEnumerable<Edge<TCoordinate>> edges)
        {
            foreach (Edge<TCoordinate> e in edges)
            {
                yield return new BasicSegmentString<TCoordinate>(e.Coordinates, e);
            }
        }

        public void CheckValid()
        {
            _nodingValidator.CheckValid();
        }
    }
}