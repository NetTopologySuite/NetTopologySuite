using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Diagnostics;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Determines the location for a point which is known to lie
    /// on at least one edge of a set of polygons.
    /// This provides the union-semantics for determining
    /// point location in a GeometryCollection, which may
    /// have polygons with adjacent edges which are effectively
    /// in the interior of the geometry.
    /// Note that it is also possible to have adjacent edges which
    /// lie on the boundary of the geometry
    /// (e.g. a polygon contained within another polygon with adjacent edges). 
    /// </summary>
    /// <author>Martin Davis</author>
    internal class AdjacentEdgeLocator
    {

        private List<Coordinate[]> _ringList;

        public AdjacentEdgeLocator(Geometry geom)
        {
            Init(geom);
        }

        public Location Locate(Coordinate p)
        {
            var sections = new NodeSections(p);
            foreach (var ring in _ringList)
            {
                AddSections(p, ring, sections);
            }
            var node = sections.CreateNode();
            //node.finish(false, false);
            return node.HasExteriorEdge(true) ? Location.Boundary : Location.Interior;
        }

        private void AddSections(Coordinate p, Coordinate[] ring, NodeSections sections)
        {
            for (int i = 0; i < ring.Length - 1; i++)
            {
                var p0 = ring[i];
                var pnext = ring[i + 1];

                if (p.Equals2D(pnext))
                {
                    //-- segment final point is assigned to next segment
                    continue;
                }
                else if (p.Equals2D(p0))
                {
                    int iprev = i > 0 ? i - 1 : ring.Length - 2;
                    var pprev = ring[iprev];
                    sections.AddNodeSection(CreateSection(p, pprev, pnext));
                }
                else if (PointLocation.IsOnSegment(p, p0, pnext))
                {
                    sections.AddNodeSection(CreateSection(p, p0, pnext));
                }
            }
        }

        private NodeSection CreateSection(Coordinate p, Coordinate prev, Coordinate next)
        {
            if (prev.Distance(p) == 0 || next.Distance(p) == 0)
            {
                Trace.WriteLine("Found zero-length section segment");
            };
            var ns = new NodeSection(true, Dimension.A, 1, 0, null, false, prev, p, next);
            return ns;
        }

        private void Init(Geometry geom)
        {
            if (geom.IsEmpty)
                return;
            _ringList = new List<Coordinate[]>();
            AddRings(geom, _ringList);
        }

        private static void AddRings(Geometry geom, List<Coordinate[]> ringList)
        {
            if (geom is Polygon poly) {
                var shell = (LinearRing)poly.ExteriorRing;
                AddRing(shell, true, ringList);
                for (int i = 0; i < poly.NumInteriorRings; i++)
                {
                    var hole = (LinearRing)poly.GetInteriorRingN(i);
                    AddRing(hole, false, ringList);
                }
            }
            else if (geom is GeometryCollection) {
                //-- recurse through collections
                for (int i = 0; i < geom.NumGeometries; i++)
                {
                    AddRings(geom.GetGeometryN(i), ringList);
                }
            }
        }

        private static void AddRing(LinearRing ring, bool requireCW, List<Coordinate[]> ringList)
        {
            //TODO: remove repeated points?
            var pts = RelateGeometry.Orient(ring.Coordinates, requireCW);
            ringList.Add(pts);
        }

    }
}
