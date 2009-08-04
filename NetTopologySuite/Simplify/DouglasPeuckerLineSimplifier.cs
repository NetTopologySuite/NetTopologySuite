using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a line (sequence of points) using
    /// the standard Douglas-Peucker algorithm.
    /// </summary>
    public class DouglasPeuckerLineSimplifier<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ICoordinateSequence<TCoordinate> _coordinates;
        private readonly List<BitVector32> _useCoordinate = new List<BitVector32>();
        private Double _distanceTolerance;
        private Int32 _outputCoordinateCount;
        private LineSegment<TCoordinate> _segment;

        public DouglasPeuckerLineSimplifier(ICoordinateSequence<TCoordinate> coordinates)
        {
            _coordinates = coordinates;
        }

        public Double DistanceTolerance
        {
            get { return _distanceTolerance; }
            set { _distanceTolerance = value; }
        }

        public static ICoordinateSequence<TCoordinate> Simplify(ICoordinateSequence<TCoordinate> coordinates,
                                                                Double distanceTolerance)
        {
            DouglasPeuckerLineSimplifier<TCoordinate> simp = new DouglasPeuckerLineSimplifier<TCoordinate>(coordinates);
            simp.DistanceTolerance = distanceTolerance;
            return coordinates.CoordinateSequenceFactory.Create(simp.Simplify());
        }

        public IEnumerable<TCoordinate> Simplify()
        {
            simplifySection(0, _coordinates.Count - 1);

            Int32 index = 0;

            foreach (TCoordinate coordinate in _coordinates)
            {
                if (getUseCoordinate(index))
                {
                    yield return coordinate;
                }

                index += 1;
            }
        }

        private void simplifySection(Int32 i, Int32 j)
        {
            if ((i + 1) == j)
            {
                return;
            }

            _segment = new LineSegment<TCoordinate>(_coordinates[i], _coordinates[j]);

            Double maxDistance = -1.0;
            Int32 maxIndex = i;

            for (Int32 k = i + 1; k < j; k++)
            {
                Double distance = _segment.Distance(_coordinates[k]);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = k;
                }
            }

            if (maxDistance <= DistanceTolerance)
            {
                for (Int32 k = i + 1; k < j; k++)
                {
                    setUseCoordinate(k, false);
                }
            }
            else
            {
                simplifySection(i, maxIndex);
                simplifySection(maxIndex, j);
            }
        }

        private void setUseCoordinate(Int32 coordinateIndex, Boolean use)
        {
            Int32 index = coordinateIndex >> 5; // divide by 32
            BitVector32 bits = _useCoordinate[index];
            bits[coordinateIndex%32] = use;
            _useCoordinate[index] = bits;
        }

        private Boolean getUseCoordinate(Int32 coordinateIndex)
        {
            return _useCoordinate[coordinateIndex >> 5][coordinateIndex%32];
        }
    }
}