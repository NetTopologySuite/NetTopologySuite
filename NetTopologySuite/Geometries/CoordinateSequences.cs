using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Utility functions for manipulating 
    /// <see cref="ICoordinateSequence" />s.
    /// </summary>
    public static class CoordinateSequences
    {
        public static ICoordinateSequence<TCoordinate> CreateEmpty<TCoordinate>()
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<TCoordinate>, IConvertible
        {
            throw new NotImplementedException();
        }

        public static ICoordinateSequence<TCoordinate> Create<TCoordinate>(Int32 initialCapacity)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<TCoordinate>, IConvertible
        {
            throw new NotImplementedException();
        }

        public static ICoordinateSequence<TCoordinate> Create<TCoordinate>(params TCoordinate[] coordinates)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<TCoordinate>, IConvertible
        {
            throw new NotImplementedException();
        }

        public static ICoordinateSequence<TCoordinate> Create<TCoordinate>(IEnumerable<TCoordinate> coordinates, Boolean allowRepeatedPoints)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<TCoordinate>, IConvertible
        {
            throw new NotImplementedException();
        }
    }
}