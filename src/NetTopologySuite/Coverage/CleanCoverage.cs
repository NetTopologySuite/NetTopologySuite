using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Operation.RelateNG;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// Internal helper for <see cref="CoverageCleaner"/>.
    /// Holds the cleaned coverage areas and provides operations to merge
    /// overlap and gap polygons into them according to a chosen strategy.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class CleanCoverage
    {
        private readonly CleanArea[] _cov;
        private Quadtree<CleanArea> _covIndex;

        public CleanCoverage(int size)
        {
            _cov = new CleanArea[size];
        }

        public void Add(int i, Polygon poly)
        {
            if (_cov[i] == null)
            {
                _cov[i] = new CleanArea();
            }
            _cov[i].Add(poly);
        }

        public void MergeOverlap(Polygon overlap, IMergeStrategy mergeStrategy, IList<int> parentIndexes)
        {
            int mergeTarget = FindMergeTarget(overlap, mergeStrategy, parentIndexes, _cov);
            Add(mergeTarget, overlap);
        }

        public static int FindMergeTarget(Polygon poly, IMergeStrategy strat, IList<int> parentIndexes, CleanArea[] cov)
        {
            //-- sort parent indexes ascending so overlaps merge to first parent by default
            var indexesAsc = new int[parentIndexes.Count];
            for (int i = 0; i < parentIndexes.Count; i++) indexesAsc[i] = parentIndexes[i];
            System.Array.Sort(indexesAsc);
            for (int i = 0; i < indexesAsc.Length; i++)
            {
                int index = indexesAsc[i];
                strat.CheckMergeTarget(index, cov[index], poly);
            }
            return strat.GetTarget();
        }

        public void MergeGaps(IList<Polygon> gaps)
        {
            CreateIndex();
            foreach (var gap in gaps)
            {
                MergeGap(gap);
            }
        }

        private void MergeGap(Polygon gap)
        {
            var adjacents = FindAdjacentAreas(gap);
            //-- No adjacent area means this gap is likely an artifact
            //-- of an invalid input polygon. Discard it.
            if (adjacents.Count == 0)
                return;

            var mergeTarget = FindMaxBorderLength(gap, adjacents);
            _covIndex.Remove(mergeTarget.Envelope, mergeTarget);
            mergeTarget.Add(gap);
            _covIndex.Insert(mergeTarget.Envelope, mergeTarget);
        }

        private static CleanArea FindMaxBorderLength(Polygon poly, IList<CleanArea> areas)
        {
            double maxLen = 0;
            CleanArea maxLenArea = null;
            foreach (var a in areas)
            {
                double len = a.GetBorderLength(poly);
                if (maxLenArea == null || len > maxLen)
                {
                    maxLen = len;
                    maxLenArea = a;
                }
            }
            return maxLenArea;
        }

        private IList<CleanArea> FindAdjacentAreas(Geometry poly)
        {
            var adjacents = new List<CleanArea>();
            var rel = RelateNG.Prepare(poly);
            var queryEnv = poly.EnvelopeInternal;
            var candidateAdjIndex = _covIndex.Query(queryEnv);
            foreach (var area in candidateAdjIndex)
            {
                if (area != null && area.IsAdjacent(rel))
                {
                    adjacents.Add(area);
                }
            }
            return adjacents;
        }

        private void CreateIndex()
        {
            _covIndex = new Quadtree<CleanArea>();
            for (int i = 0; i < _cov.Length; i++)
            {
                //-- null areas are never merged to
                if (_cov[i] != null)
                {
                    _covIndex.Insert(_cov[i].Envelope, _cov[i]);
                }
            }
        }

        public Geometry[] ToCoverage(GeometryFactory geomFactory)
        {
            var cleanCov = new Geometry[_cov.Length];
            for (int i = 0; i < _cov.Length; i++)
            {
                Geometry merged;
                if (_cov[i] == null)
                {
                    merged = geomFactory.CreateEmpty(Dimension.Surface);
                }
                else
                {
                    merged = _cov[i].Union();
                }
                cleanCov[i] = merged;
            }
            return cleanCov;
        }

        internal class CleanArea
        {
            private readonly List<Polygon> _polys = new List<Polygon>();

            public void Add(Polygon poly)
            {
                _polys.Add(poly);
            }

            public Envelope Envelope
            {
                get
                {
                    var env = new Envelope();
                    foreach (var poly in _polys)
                    {
                        env.ExpandToInclude(poly.EnvelopeInternal);
                    }
                    return env;
                }
            }

            public double GetBorderLength(Polygon adjPoly)
            {
                double len = 0;
                foreach (var poly in _polys)
                {
                    var border = OverlayNGRobust.Overlay(poly, adjPoly, SpatialFunction.Intersection);
                    len += border.Length;
                }
                return len;
            }

            public double Area
            {
                get
                {
                    double area = 0;
                    foreach (var poly in _polys)
                    {
                        area += poly.Area;
                    }
                    return area;
                }
            }

            public bool IsAdjacent(RelateNG rel)
            {
                foreach (var geom in _polys)
                {
                    if (rel.Evaluate(geom, IntersectionMatrixPattern.Adjacent))
                        return true;
                }
                return false;
            }

            public Geometry Union()
            {
                var geoms = GeometryFactory.ToGeometryArray(_polys);
                return CoverageUnion.Union(geoms);
            }
        }

        public interface IMergeStrategy
        {
            int GetTarget();
            void CheckMergeTarget(int areaIndex, CleanArea cleanArea, Polygon poly);
        }

        public class BorderMergeStrategy : IMergeStrategy
        {
            private int _targetIndex = -1;
            private double _targetBorderLen;

            public int GetTarget() => _targetIndex;

            public void CheckMergeTarget(int areaIndex, CleanArea area, Polygon poly)
            {
                double borderLen = area == null ? 0 : area.GetBorderLength(poly);
                if (_targetIndex < 0 || borderLen > _targetBorderLen)
                {
                    _targetIndex = areaIndex;
                    _targetBorderLen = borderLen;
                }
            }
        }

        public class AreaMergeStrategy : IMergeStrategy
        {
            private int _targetIndex = -1;
            private double _targetArea;
            private readonly bool _isMax;

            public AreaMergeStrategy(bool isMax)
            {
                _isMax = isMax;
            }

            public int GetTarget() => _targetIndex;

            public void CheckMergeTarget(int areaIndex, CleanArea area, Polygon poly)
            {
                double areaVal = area == null ? 0.0 : area.Area;
                bool isBetter = _isMax
                    ? areaVal > _targetArea
                    : areaVal < _targetArea;
                if (_targetIndex < 0 || isBetter)
                {
                    _targetIndex = areaIndex;
                    _targetArea = areaVal;
                }
            }
        }

        public class IndexMergeStrategy : IMergeStrategy
        {
            private int _targetIndex = -1;
            private readonly bool _isMax;

            public IndexMergeStrategy(bool isMax)
            {
                _isMax = isMax;
            }

            public int GetTarget() => _targetIndex;

            public void CheckMergeTarget(int areaIndex, CleanArea area, Polygon poly)
            {
                bool isBetter = _isMax
                    ? areaIndex > _targetIndex
                    : areaIndex < _targetIndex;
                if (_targetIndex < 0 || isBetter)
                {
                    _targetIndex = areaIndex;
                }
            }
        }
    }
}
