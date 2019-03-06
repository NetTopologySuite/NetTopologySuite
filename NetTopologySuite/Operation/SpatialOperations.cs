using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.Operation.Buffer;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Overlay.Snap;
using NetTopologySuite.Operation.Predicate;
using NetTopologySuite.Operation.Relate;

namespace NetTopologySuite.Operation
{
    public class SpatialOperations : ISpatialOperations
    {
        public SpatialOperations(IGeometryFactory factory)
        {
            Factory = factory;
        }

        protected IGeometryFactory Factory { get; }

        protected virtual void PrepareInput(IGeometry g1, out IGeometry g1out)
        {
            g1out = g1;
        }

        protected virtual void PrepareInput(IGeometry g1, IGeometry g2, out IGeometry g1out, out IGeometry g2out)
        {
            g1out = g1;
            g2out = g2;
        }

        protected virtual IGeometry PrepareInput(IGeometry geometry)
        {
            return geometry;
        }

        protected virtual IGeometry CreateResult(IGeometry geometry)
        {
            return geometry;
        }

        public virtual double Area(IGeometry geometry)
        {
            PrepareInput(geometry,out var geom);

            // ToDo:
            //   consider placing this in separate operation
            double area = 0d;
            if (geom is IGeometryCollection gc)
            {
                for (int i = 0; i < gc.NumGeometries; i++)
                    area += Area(gc.GetGeometryN(i));
            }
            else if (geom is IPolygon p)
            {
                area += Algorithm.Area.OfRing(p.ExteriorRing.CoordinateSequence);
                for (int i = 0; i < p.NumInteriorRings; i++)
                    area -= Algorithm.Area.OfRing(p.GetInteriorRingN(i).CoordinateSequence);
                return area;
            }

            return area;
        }

        public virtual double Length(IGeometry geometry)
        {
            PrepareInput(geometry, out var geom);

            // ToDo:
            //   consider placing this in separate operation
            double length = 0d;
            if (geom is IGeometryCollection gc)
            {
                var so = ((IGeometryFactoryEx) geom.Factory).SpatialOperations;
                for (int i = 0; i < gc.NumGeometries; i++)
                    length += so.Length(gc.GetGeometryN(i));
            }
            else if (geom is IPolygon p)
            {
                length += Algorithm.Length.OfLine(p.ExteriorRing.CoordinateSequence);
                for (int i = 0; i < p.NumInteriorRings; i++)
                    length += Algorithm.Length.OfLine(p.GetInteriorRingN(i).CoordinateSequence);
                return length;
            }
            else if (geom is ILineString l)
            {
                length += Algorithm.Length.OfLine(l.CoordinateSequence);
                return length;
            }

            return length;
        }

        public virtual double Distance(IGeometry g1, IGeometry g2)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);
            return DistanceOp.Distance(geom1, geom2);
        }

        public IGeometry Intersection(IGeometry g1, IGeometry g2)
        {
            // Special case: if one input is empty ==> empty
            if (g1.IsEmpty || g2.IsEmpty)
                return OverlayOp.CreateEmptyResult(SpatialFunction.Intersection, g1, g2, Factory);

            PrepareInput(g1, g2, out var geom1, out var geom2);

            // compute for GCs
            // (An inefficient algorithm, but will work)
            // TODO: improve efficiency of computation for GCs
            if (geom1.OgcGeometryType == OgcGeometryType.GeometryCollection)
            {
                // avoid 2nd preparation of input
                var so = ((IGeometryFactoryEx)geom1.Factory).SpatialOperations;
                var tmpGeom = geom2;
                return GeometryCollectionMapper.Map(
                    (IGeometryCollection)geom1, g => so.Intersection(g, tmpGeom));
            }

            // No longer needed since GCs are handled by previous code
            //CheckNotGeometryCollection(this);
            //CheckNotGeometryCollection(other);
            var res = SnapIfNeededOverlayOp.Overlay(geom1, geom2, SpatialFunction.Intersection);
            return CreateResult(res);
        }

        public IGeometry Union(IGeometry g1, IGeometry g2)
        {
            // handle empty geometry cases
            if (g1.IsEmpty || (g2 == null || g2.IsEmpty))
            {
                if (g1.IsEmpty && (g2 == null || g2.IsEmpty))
                    return OverlayOp.CreateEmptyResult(SpatialFunction.Union, g1, g2, Factory);

                // Special case: if either input is empty ==> other input
                if (g2 == null || g2.IsEmpty) return g1.Copy();
                if (g1.IsEmpty) return g2.Copy();
            }

            if (g1.OgcGeometryType == OgcGeometryType.GeometryCollection ||
                g2.OgcGeometryType == OgcGeometryType.GeometryCollection)
                throw new ArgumentException("Operation does not support GeometryCollection arguments");

            PrepareInput(g1, g2, out var geom1, out var geom2);
            var res = SnapIfNeededOverlayOp.Overlay(geom1, geom2, SpatialFunction.Union);
            return CreateResult(res);
        }

        public IGeometry Difference(IGeometry g1, IGeometry g2)
        {
            // special case: if A.isEmpty ==> empty; if B.isEmpty ==> A
            if (g1.IsEmpty)
                return OverlayOp.CreateEmptyResult(SpatialFunction.Difference, g1, g2, Factory);
            if (g2 == null || g2.IsEmpty)
                return g2.Copy();

            if (g1.OgcGeometryType == OgcGeometryType.GeometryCollection ||
                g2.OgcGeometryType == OgcGeometryType.GeometryCollection)
                throw new ArgumentException("Operation does not support GeometryCollection arguments");

            PrepareInput(g1, g2, out var geom1, out var geom2);
            var res = SnapIfNeededOverlayOp.Overlay(geom1, geom2, SpatialFunction.Difference);
            return CreateResult(res);
        }

        public IGeometry SymDifference(IGeometry g1, IGeometry g2)
        {
            // handle empty geometry cases
            if (g1.IsEmpty || (g2 == null || g2.IsEmpty))
            {
                // both empty - check dimensions
                if (g1.IsEmpty && (g2 == null || g2.IsEmpty))
                    return OverlayOp.CreateEmptyResult(SpatialFunction.SymDifference, g1, g2, Factory);

                // special case: if either input is empty ==> result = other arg
                if (g2 == null || g2.IsEmpty) return g1.Copy();
                if (g1.IsEmpty) return (IGeometry)g2.Copy();
            }

            if (g1.OgcGeometryType == OgcGeometryType.GeometryCollection ||
                g2.OgcGeometryType == OgcGeometryType.GeometryCollection)
                throw new ArgumentException("Operation does not support GeometryCollection arguments");

            PrepareInput(g1, g2, out var geom1, out var geom2);
            var res = SnapIfNeededOverlayOp.Overlay(geom1, geom2, SpatialFunction.SymDifference);
            return CreateResult(res);
        }

        public virtual IPoint Centroid(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return geometry.Factory.CreatePoint();

            PrepareInput(geometry, out var geom);
            var centPt = Algorithm.Centroid.GetCentroid(geom);
            Factory.PrecisionModel.MakePrecise(centPt);
            return (IPoint)CreateResult(Factory.CreatePoint(centPt));
        }

        public virtual IPoint InteriorPoint(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return geometry.Factory.CreatePoint();

            PrepareInput(geometry, out var geom);
            Coordinate interiorPt = null;
            var dim = geom.Dimension;
            if (dim == Dimension.Point)
            {
                var intPt = new InteriorPointPoint(geom);
                interiorPt = intPt.InteriorPoint;
            }
            else if (dim == Dimension.Curve)
            {
                var intPt = new InteriorPointLine(geom);
                interiorPt = intPt.InteriorPoint;
            }
            else
            {
                var intPt = new InteriorPointArea(geom);
                interiorPt = intPt.InteriorPoint;
            }

            Factory.PrecisionModel.MakePrecise(interiorPt);
            return (IPoint)CreateResult(geometry.Factory.CreatePoint(interiorPt));

        }

        public IGeometry Buffer(IGeometry g, double distance, IBufferParameters parameters)
        {
            PrepareInput(g, out var geom1);
            return CreateResult(BufferOp.Buffer(geom1, distance, parameters));
        }

        public IGeometry ConvexHull(IGeometry g)
        {
            PrepareInput(g, out var geom1);
            return CreateResult(new ConvexHull(geom1).GetConvexHull());
        }

        #region Predicates

        public virtual bool Equals(IGeometry g1, IGeometry g2)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            // short-circuit test
            if (!geom1.EnvelopeInternal.Equals(geom2.EnvelopeInternal))
                return true;

            return Relate(geom1, geom2).IsEquals(geom1.Dimension, geom2.Dimension);
        }

        public virtual bool IsWithinDistance(IGeometry g1, IGeometry g2, double distance)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            double envDist = geom1.EnvelopeInternal.Distance(geom2.EnvelopeInternal);
            if (envDist > distance)
                return false;

            return DistanceOp.IsWithinDistance(geom1, geom2, distance);
        }

        public bool Disjoint(IGeometry g1, IGeometry g2)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            // short-circuit test
            if (!geom1.EnvelopeInternal.Intersects(geom2.EnvelopeInternal))
                return true;

            return Relate(geom1, geom2).IsDisjoint();
        }

        public bool Touches(IGeometry g1, IGeometry g2)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            // short-circuit test
            if (!geom1.EnvelopeInternal.Intersects(geom2.EnvelopeInternal))
                return false;

            return Relate(geom1, geom2).IsTouches(geom1.Dimension, geom2.Dimension);
        }

        public bool Intersects(IGeometry g1, IGeometry g2)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            // short-circuit test
            if (!geom1.EnvelopeInternal.Intersects(geom2.EnvelopeInternal))
                return false;
            /*
             * TODO: (MD) Add optimizations:
             *
             * - for P-A case:
             * If P is in env(A), test for point-in-poly
             *
             * - for A-A case:
             * If env(A1).overlaps(env(A2))
             * test for overlaps via point-in-poly first (both ways)
             * Possibly optimize selection of point to test by finding point of A1
             * closest to centre of env(A2).
             * (Is there a test where we shouldn't bother - e.g. if env A
             * is much smaller than env B, maybe there's no point in testing
             * pt(B) in env(A)?
             */

            // optimizations for rectangle arguments
            if (geom1.IsRectangle)
                return RectangleIntersects.Intersects((IPolygon)geom1, geom2);
            if (geom2.IsRectangle)
                return RectangleIntersects.Intersects((IPolygon)geom2, geom1);

            if (geom1.OgcGeometryType == OgcGeometryType.GeometryCollection ||
                geom2.OgcGeometryType == OgcGeometryType.GeometryCollection)
            {
                // to avoid duplicate input preparation
                var so = ((IGeometryFactoryEx) geom1.Factory).SpatialOperations;
                for (int i = 0; i < geom1.NumGeometries; i++)
                {
                    for (int j = 0; j < geom2.NumGeometries; j++)
                    {
                        if (so.Intersects(geom1.GetGeometryN(i), geom2.GetGeometryN(j)))
                            return true;
                    }
                }
                return false;
            }

            return Relate(geom1, geom2).IsIntersects();
        }

        public bool Crosses(IGeometry g1, IGeometry g2)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            // short-circuit test
            if (!geom1.EnvelopeInternal.Intersects(geom2.EnvelopeInternal))
                return false;

            return Relate(geom1, geom2).IsCrosses( geom1.Dimension, geom2.Dimension);
        }

        public bool Contains(IGeometry g1, IGeometry g2)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            // optimization - lower dimension cannot contain areas
            if (geom2.Dimension == Dimension.Surface && geom1.Dimension < Dimension.Surface)
                return false;

            // optimization - P cannot contain a non-zero-length L
            // Note that a point can contain a zero-length lineal geometry,
            // since the line has no boundary due to Mod-2 Boundary Rule
            if (geom2.Dimension == Dimension.Curve && geom1.Dimension < Dimension.Curve && geom2.Length > 0.0)
                return false;

            // optimization - envelope test
            if (!geom1.EnvelopeInternal.Contains(geom2.EnvelopeInternal))
                return false;

            // optimizations for rectangle arguments
            if (geom1.IsRectangle)
                return RectangleContains.Contains((IPolygon)geom1, geom2);

            return Relate(geom1, geom2).IsContains();
        }

        public bool Overlaps(IGeometry g1, IGeometry g2)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            // short-circuit test
            if (!geom1.EnvelopeInternal.Intersects(geom2.EnvelopeInternal))
                return false;

            return Relate(geom1, geom2).IsOverlaps(geom1.Dimension, geom2.Dimension);
        }

        public bool Covers(IGeometry g1, IGeometry g2)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            // optimization - lower dimension cannot cover areas
            if (geom2.Dimension == Dimension.Surface && geom1.Dimension < Dimension.Surface)
                return false;

            // optimization - P cannot cover a non-zero-length L
            // Note that a point can cover a zero-length lineal geometry
            if (geom2.Dimension == Dimension.Curve && geom1.Dimension < Dimension.Curve && geom2.Length > 0.0)
                return false;

            // optimization - envelope test
            if (!geom1.EnvelopeInternal.Covers(geom2.EnvelopeInternal))
                return false;

            // optimization for rectangle arguments
            if (geom1.IsRectangle)
                // since we have already tested that the test envelope is covered
                return true;

            return Relate(geom1, geom2).IsCovers();
        }

        public bool Relate(IGeometry g1, IGeometry g2, string intersectionPattern)
        {
            PrepareInput(g1, g2, out var geom1, out var geom2);

            return Relate(geom1, geom2).Matches(intersectionPattern);
        }

        public IntersectionMatrix Relate(IGeometry g1, IGeometry g2)
        {

            if (g1.OgcGeometryType == OgcGeometryType.GeometryCollection ||
                g2.OgcGeometryType == OgcGeometryType.GeometryCollection)
                throw new ArgumentException("Operation does not support GeometryCollection arguments");

            PrepareInput(g1, g2, out var geom1, out var geom2);

            return RelateOp.Relate(geom1, geom2);
        }

        #endregion
    }
}
