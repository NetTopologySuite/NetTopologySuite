using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Operation.Overlay.Validate;
using System.Collections.Generic;

namespace NetTopologySuite.Algorithm.Construct
{
    /// <summary>
    /// Determines the location of a point in the polygonal elements of a geometry.
    /// Uses spatial indexing to provide efficient performance.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class IndexedPointInPolygonsLocator : IPointOnGeometryLocator
    {
        private readonly Geometry _geom;
        private STRtree<IndexedPointInAreaLocator> _index;

        public IndexedPointInPolygonsLocator(Geometry geom)
        {
            _geom = geom;
        }

        private void Init()
        {
            if (_index != null)
                return;
            var polys = PolygonalExtracter.GetPolygonals<List<Geometry>>(_geom);
            _index = new STRtree<IndexedPointInAreaLocator>();
            foreach (var poly in polys)
            {
                _index.Insert(poly.EnvelopeInternal, new IndexedPointInAreaLocator(poly));
            }
        }

        /// <inheritdoc/>
        public Location Locate(Coordinate p)
        {
            Init();

            var results = _index.Query(new Envelope(p));
            for (int i = 0; i < results.Count; i++)
            {
                var ptLocater = results[i];
                var loc = ptLocater.Locate(p);
                if (loc != Location.Exterior)
                    return loc;
            }
            return Location.Exterior;
        }
    }
}
