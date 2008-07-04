using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Computes the distance and
    /// closest points between two <c>Geometry</c>s.
    /// The distance computation finds a pair of points in the input geometries
    /// which have minimum distance between them.  These points may
    /// not be vertices of the geometries, but may lie in the interior of
    /// a line segment. In this case the coordinate computed is a close
    /// approximation to the exact point.
    /// The algorithms used are straightforward O(n^2)
    /// comparisons.  This worst-case performance could be improved on
    /// by using Voronoi techniques.
    /// </summary>
    public class DistanceOp
    {
        /// <summary>
        /// Compute the distance between the closest points of two geometries.
        /// </summary>
        /// <param name="g0">A <c>Geometry</c>.</param>
        /// <param name="g1">Another <c>Geometry</c>.</param>
        /// <returns>The distance between the geometries.</returns>
        public static double Distance(IGeometry g0, IGeometry g1)
        {
            DistanceOp distOp = new DistanceOp(g0, g1);
            return distOp.Distance();
        }

        /// <summary>
        /// Test whether two geometries lie within a given distance of each other.
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool IsWithinDistance(IGeometry g0, IGeometry g1, double distance)
        {
            DistanceOp distOp = new DistanceOp(g0, g1, distance);
            return distOp.Distance() <= distance;
        }

        /// <summary>
        /// Compute the the closest points of two geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <param name="g0">A <c>Geometry</c>.</param>
        /// <param name="g1">Another <c>Geometry</c>.</param>
        /// <returns>The closest points in the geometries.</returns>
        public static ICoordinate[] ClosestPoints(IGeometry g0, IGeometry g1)
        {
            DistanceOp distOp = new DistanceOp(g0, g1);
            return distOp.ClosestPoints();
        }

        private PointLocator ptLocator = new PointLocator();
        private IGeometry[] geom;
        private GeometryLocation[] minDistanceLocation;
        private double minDistance = Double.MaxValue;
        private double terminateDistance = 0.0;

        /// <summary>
        /// Constructs a <see cref="DistanceOp" />  that computes the distance and closest points between
        /// the two specified geometries.
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        public DistanceOp(IGeometry g0, IGeometry g1) 
        : this(g0, g1, 0) { }

        /// <summary>
        /// Constructs a <see cref="DistanceOp" /> that computes the distance and closest points between
        /// the two specified geometries.
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <param name="terminateDistance">The distance on which to terminate the search.</param>
        public DistanceOp(IGeometry g0, IGeometry g1, double terminateDistance)
        {
            this.geom = new IGeometry[] { g0, g1, };            
            this.terminateDistance = terminateDistance;
        }

        /// <summary>
        /// Report the distance between the closest points on the input geometries.
        /// </summary>
        /// <returns>The distance between the geometries.</returns>
        public double Distance()
        {
            ComputeMinDistance();
            return minDistance;
        }

        /// <summary>
        /// Report the coordinates of the closest points in the input geometries.
        /// The points are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of <c>Coordinate</c>s of the closest points.</returns>
        public ICoordinate[] ClosestPoints()
        {
            ComputeMinDistance();
            ICoordinate[] closestPts = new ICoordinate[] { minDistanceLocation[0].Coordinate, 
                                                           minDistanceLocation[1].Coordinate };
            return closestPts;
        }

        /// <summary>
        /// Report the locations of the closest points in the input geometries.
        /// The locations are presented in the same order as the input Geometries.
        /// </summary>
        /// <returns>A pair of {GeometryLocation}s for the closest points.</returns>
        public GeometryLocation[] ClosestLocations()
        {
            ComputeMinDistance();
            return minDistanceLocation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dist"></param>
        private void UpdateMinDistance(double dist)
        {
            if (dist < minDistance)
                minDistance = dist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="locGeom"></param>
        /// <param name="flip"></param>
        private void UpdateMinDistance(GeometryLocation[] locGeom, bool flip)
        {
            // if not set then don't update
            if (locGeom[0] == null) 
                return;
            if (flip)
            {
                minDistanceLocation[0] = locGeom[1];
                minDistanceLocation[1] = locGeom[0];
            }
            else
            {
                minDistanceLocation[0] = locGeom[0];
                minDistanceLocation[1] = locGeom[1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComputeMinDistance()
        {
            if (minDistanceLocation != null)
                return;
            minDistanceLocation = new GeometryLocation[2];
            ComputeContainmentDistance();
            if (minDistance <= terminateDistance)
                return;
            ComputeLineDistance();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComputeContainmentDistance()
        {
            IList polys0 = PolygonExtracter.GetPolygons(geom[0]);
            IList polys1 = PolygonExtracter.GetPolygons(geom[1]);

            GeometryLocation[] locPtPoly = new GeometryLocation[2];
            // test if either point is wholely inside the other
            if (polys1.Count > 0)
            {
                IList insideLocs0 = ConnectedElementLocationFilter.GetLocations(geom[0]);
                ComputeInside(insideLocs0, polys1, locPtPoly);
                if (minDistance <= terminateDistance)
                {
                    minDistanceLocation[0] = locPtPoly[0];
                    minDistanceLocation[1] = locPtPoly[1];
                    return;
                }
            }
            if (polys0.Count > 0)
            {
                IList insideLocs1 = ConnectedElementLocationFilter.GetLocations(geom[1]);
                ComputeInside(insideLocs1, polys0, locPtPoly);
                if (minDistance <= terminateDistance)
                {
                    // flip locations, since we are testing geom 1 VS geom 0
                    minDistanceLocation[0] = locPtPoly[1];
                    minDistanceLocation[1] = locPtPoly[0];
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="locs"></param>
        /// <param name="polys"></param>
        /// <param name="locPtPoly"></param>
        private void ComputeInside(IList locs, IList polys, GeometryLocation[] locPtPoly)
        {
            for (int i = 0; i < locs.Count; i++)
            {
                GeometryLocation loc = (GeometryLocation)locs[i];
                for (int j = 0; j < polys.Count; j++)
                {
                    IPolygon poly = (IPolygon) polys[j];
                    ComputeInside(loc, poly, locPtPoly);
                    if (minDistance <= terminateDistance)                    
                        return;                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ptLoc"></param>
        /// <param name="poly"></param>
        /// <param name="locPtPoly"></param>
        private void ComputeInside(GeometryLocation ptLoc, IPolygon poly, GeometryLocation[] locPtPoly)
        {
            ICoordinate pt = ptLoc.Coordinate;
            if (Locations.Exterior != ptLocator.Locate(pt, poly))
            {
                minDistance = 0.0;
                locPtPoly[0] = ptLoc;
                GeometryLocation locPoly = new GeometryLocation(poly, pt);
                locPtPoly[1] = locPoly;
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComputeLineDistance()
        {
            GeometryLocation[] locGeom = new GeometryLocation[2];

            /*
             * Geometries are not wholely inside, so compute distance from lines and points
             * of one to lines and points of the other
             */
            IList lines0 = LinearComponentExtracter.GetLines(geom[0]);
            IList lines1 = LinearComponentExtracter.GetLines(geom[1]);

            IList pts0 = PointExtracter.GetPoints(geom[0]);
            IList pts1 = PointExtracter.GetPoints(geom[1]);

            // bail whenever minDistance goes to zero, since it can't get any less
            ComputeMinDistanceLines(lines0, lines1, locGeom);
            UpdateMinDistance(locGeom, false);
            if (minDistance <= terminateDistance) return;

            locGeom[0] = null;
            locGeom[1] = null;
            ComputeMinDistanceLinesPoints(lines0, pts1, locGeom);
            UpdateMinDistance(locGeom, false);
            if (minDistance <= terminateDistance) return;

            locGeom[0] = null;
            locGeom[1] = null;
            ComputeMinDistanceLinesPoints(lines1, pts0, locGeom);
            UpdateMinDistance(locGeom, true);
            if (minDistance <= terminateDistance) return;

            locGeom[0] = null;
            locGeom[1] = null;
            ComputeMinDistancePoints(pts0, pts1, locGeom);
            UpdateMinDistance(locGeom, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines0"></param>
        /// <param name="lines1"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistanceLines(IList lines0, IList lines1, GeometryLocation[] locGeom)
        {
            for (int i = 0; i < lines0.Count; i++)
            {
                ILineString line0 = (ILineString) lines0[i];
                for (int j = 0; j < lines1.Count; j++)
                {
                    ILineString line1 = (ILineString) lines1[j];
                    ComputeMinDistance(line0, line1, locGeom);
                    if (minDistance <= terminateDistance) return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points0"></param>
        /// <param name="points1"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistancePoints(IList points0, IList points1, GeometryLocation[] locGeom)
        {
            for (int i = 0; i < points0.Count; i++)
            {
                IPoint pt0 = (IPoint) points0[i];
                for (int j = 0; j < points1.Count; j++)
                {
                    IPoint pt1 = (IPoint) points1[j];
                    double dist = pt0.Coordinate.Distance(pt1.Coordinate);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        // this is wrong - need to determine closest points on both segments!!!
                        locGeom[0] = new GeometryLocation(pt0, 0, pt0.Coordinate);
                        locGeom[1] = new GeometryLocation(pt1, 0, pt1.Coordinate);
                    }
                    if (minDistance <= terminateDistance) return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="points"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistanceLinesPoints(IList lines, IList points, GeometryLocation[] locGeom)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                ILineString line = (ILineString) lines[i];
                for (int j = 0; j < points.Count; j++)
                {
                    IPoint pt = (IPoint) points[j];
                    ComputeMinDistance(line, pt, locGeom);
                    if (minDistance <= terminateDistance) return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line0"></param>
        /// <param name="line1"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistance(ILineString line0, ILineString line1, GeometryLocation[] locGeom)
        {
            if (line0.EnvelopeInternal.Distance(line1.EnvelopeInternal) > minDistance) 
                return;
            ICoordinate[] coord0 = line0.Coordinates;
            ICoordinate[] coord1 = line1.Coordinates;
            // brute force approach!
            for (int i = 0; i < coord0.Length - 1; i++)
            {
                for (int j = 0; j < coord1.Length - 1; j++)
                {
                    double dist = CGAlgorithms.DistanceLineLine(
                                                    coord0[i], coord0[i + 1],
                                                    coord1[j], coord1[j + 1]);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        LineSegment seg0 = new LineSegment(coord0[i], coord0[i + 1]);
                        LineSegment seg1 = new LineSegment(coord1[j], coord1[j + 1]);
                        ICoordinate[] closestPt = seg0.ClosestPoints(seg1);
                        locGeom[0] = new GeometryLocation(line0, i, closestPt[0]);
                        locGeom[1] = new GeometryLocation(line1, j, closestPt[1]);
                    }
                    if (minDistance <= terminateDistance) return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="pt"></param>
        /// <param name="locGeom"></param>
        private void ComputeMinDistance(ILineString line, IPoint pt, GeometryLocation[] locGeom)
        {
            if (line.EnvelopeInternal.Distance(pt.EnvelopeInternal) > minDistance) return;
            ICoordinate[] coord0 = line.Coordinates;
            ICoordinate coord = pt.Coordinate;
            // brute force approach!
            for (int i = 0; i < coord0.Length - 1; i++)
            {
                double dist = CGAlgorithms.DistancePointLine(coord, coord0[i], coord0[i + 1]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    LineSegment seg = new LineSegment(coord0[i], coord0[i + 1]);
                    ICoordinate segClosestPoint = seg.ClosestPoint(coord);
                    locGeom[0] = new GeometryLocation(line, i, segClosestPoint);
                    locGeom[1] = new GeometryLocation(pt, 0, coord);
                }
                if (minDistance <= terminateDistance) 
                    return;
            }
        }
    }
}
