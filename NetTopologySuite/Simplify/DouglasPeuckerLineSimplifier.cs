using System;
using System.Collections;
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
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public static IEnumerable<TCoordinate> Simplify(IEnumerable<TCoordinate> coordinates, Double distanceTolerance)
        {
            DouglasPeuckerLineSimplifier<TCoordinate> simp = new DouglasPeuckerLineSimplifier<TCoordinate>(coordinates);
            simp.DistanceTolerance = distanceTolerance;
            return simp.Simplify();
        }

        private readonly IEnumerable<TCoordinate> _coordinates;
        private Double _distanceTolerance;
        private Int32 _outputCoordinateCount;
        private LineSegment<TCoordinate> _segment = new LineSegment<TCoordinate>();
        private readonly List<BitVector32> _useCoordinate = new List<BitVector32>();

        public DouglasPeuckerLineSimplifier(IEnumerable<TCoordinate> coordinates)
        {
            _coordinates = coordinates;
        }

        public Double DistanceTolerance
        {
            get { return _distanceTolerance; }
            set { _distanceTolerance = value; }
        }

        public IEnumerable<TCoordinate> Simplify()
        {
            simplifySection(0, _coordinates.Length - 1);
            List<TCoordinate> coordList = new List<TCoordinate>();

            for (Int32 i = 0; i < _coordinates.Length; i++)
            {
                if (getUseCoordinate(i))
                {
                    coordList.Add(new TCoordinate(_coordinates[i]));
                }
            }
        }

        private void simplifySection(Int32 i, Int32 j)
        {
            if ((i + 1) == j)
            {
                return;
            }

            seg.P0 = _coordinates[i];
            seg.P1 = _coordinates[j];
            Double maxDistance = -1.0;
            Int32 maxIndex = i;

            for (Int32 k = i + 1; k < j; k++)
            {
                Double distance = seg.Distance(_coordinates[k]);

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
            _useCoordinate[coordinateIndex / 32][coordinateIndex % 32] = use;
        }

        private Boolean getUseCoordinate(Int32 coordinateIndex)
        {
            return _useCoordinate[coordinateIndex / 32][coordinateIndex % 32];
        }
    }
}