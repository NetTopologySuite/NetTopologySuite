using System;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Overlay.Snap;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Operation.Union;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A class that encapsulates geometry overlay functionality
    /// </summary>
    public abstract class GeometryOverlay
    {
        /// <summary>
        /// Gets a value indicating a geometry overlay operation class that uses old NTS overlay operation set.
        /// </summary>
        public static GeometryOverlay Legacy
        {
            get { return OverlayV1.Instance; }
        }

        /// <summary>
        /// Gets a value indicating a geometry overlay operation class that uses next-generation NTS overlay operation set.
        /// </summary>
        public static GeometryOverlay NG
        {
            get { return OverlayV2.Instance; }
        }

        /// <summary>
        /// Computes a <c>Geometry</c> representing the overlay of geometries <c>a</c> and <c>b</c>
        /// using the spatial function defined by <c>opCode</c>.
        /// </summary>
        /// <param name="a">The 1st geometry</param>
        /// <param name="b">The 2nd geometry</param>
        /// <param name="opCode">The spatial function for the overlay operation</param>
        /// <returns>The computed geometry</returns>
        protected abstract Geometry Overlay(Geometry a, Geometry b, SpatialFunction opCode);

        /// <summary>
        /// Computes a <c>Geometry</c> representing the point-set which is
        /// common to both <c>a</c> and <c>b</c> Geometry.
        /// </summary>
        /// <param name="a">The 1st <c>Geometry</c></param>
        /// <param name="b">The 2nd <c>Geometry</c></param>
        /// <returns>A geometry representing the point-set common to the two <c>Geometry</c>s.</returns>
        /// <seealso cref="Geometry.Intersection"/>
        public virtual Geometry Intersection(Geometry a, Geometry b)
        {
            /*
             * TODO: MD - add optimization for P-A case using Point-In-Polygon
             */
            // special case: if one input is empty ==> empty
            if (a.IsEmpty || b.IsEmpty)
                return OverlayOp.CreateEmptyResult(SpatialFunction.Intersection, a, b, a.Factory);

            // compute for GCs
            // (An inefficient algorithm, but will work)
            // TODO: improve efficiency of computation for GCs
            if (a.OgcGeometryType == OgcGeometryType.GeometryCollection)
            {
                var g2 = b;
                return GeometryCollectionMapper.Map(
                    (GeometryCollection) a, g => g.Intersection(g2));
            }

            // No longer needed since GCs are handled by previous code
            //CheckNotGeometryCollection(a, b);

            return Overlay(a, b, SpatialFunction.Intersection);
        }

        /// <summary>
        /// Computes a <c>Geometry</c> representing the point-set
        /// which is contained in both input <c>Geometry</c>s .
        /// </summary>
        /// <param name="a">The 1st <c>Geometry</c></param>
        /// <param name="b">The 2nd <c>Geometry</c></param>
        /// <returns>A point-set combining the points of
        /// <c>Geometry</c>'s <c>a</c> and <c>b</c>.
        /// </returns>
        /// <seealso cref="Geometry.Union(Geometry)"/>
        public virtual Geometry Union(Geometry a, Geometry b)
        {
            // handle empty geometry cases
            if (a.IsEmpty || (b == null || b.IsEmpty))
            {
                // both empty - check dimensions
                if (a.IsEmpty && (b == null || b.IsEmpty))
                    return OverlayOp.CreateEmptyResult(SpatialFunction.Union, a, b, a.Factory);

                // special case: if either input is empty ==> result = other arg
                if (a.IsEmpty) return b.Copy();
                if (b == null || b.IsEmpty) return a.Copy();
            }

            // TODO: optimize if envelopes of geometries do not intersect

            CheckNotGeometryCollection(a, b);

            return Overlay(a, b, SpatialFunction.Union);
        }


        /// <summary>
        /// Computes a <c>Geometry</c> representing the closure of the point-set
        /// of the points contained in this <c>Geometry</c> that are not contained in
        /// the <c>other</c> Geometry.
        /// </summary>
        /// <param name="a">The 1st <c>Geometry</c></param>
        /// <param name="b">The 2nd <c>Geometry</c></param>
        /// <returns>
        /// A Geometry representing the point-set difference
        /// of <c>Geometry</c>'s <c>a</c> and <c>b</c>.
        /// </returns>
        /// <seealso cref="Geometry.Difference(Geometry)"/>
        public virtual Geometry Difference(Geometry a, Geometry b)
        {
            // special case: if A.isEmpty ==> empty; if B.isEmpty ==> A
            if (a.IsEmpty) return OverlayOp.CreateEmptyResult(SpatialFunction.Difference, a, b, a.Factory);
            if (b == null || b.IsEmpty) return a.Copy();

            CheckNotGeometryCollection(a, b);

            return Overlay(a, b, SpatialFunction.Difference);
        }


        /// <summary>
        /// Computes a <c>Geometry</c> representing the closure of the point-set
        /// which is the union of the points in <c>Geometry</c> <c>a</c> which are not
        /// contained in the Geometry  <c>b</c>,
        /// with the points in the <c>b</c> Geometry not contained in the <c>Geometry</c> <c>a</c>.
        /// </summary>
        /// <param name="a">The 1st <c>Geometry</c></param>
        /// <param name="b">The 2nd <c>Geometry</c></param>
        /// <returns>
        /// A Geometry representing the point-set symmetric difference
        /// of <c>Geometry</c>'s <c>a</c> and <c>b</c>.
        /// </returns>
        /// <seealso cref="Geometry.SymmetricDifference(Geometry)"/>
        public virtual Geometry SymmetricDifference(Geometry a, Geometry b)
        {
            // handle empty geometry cases
            if (a.IsEmpty || (b == null || b.IsEmpty))
            {
                // both empty - check dimensions
                if (a.IsEmpty && (b == null || b.IsEmpty))
                    return OverlayOp.CreateEmptyResult(SpatialFunction.SymDifference, a, b, a.Factory);

                // special case: if either input is empty ==> result = other arg
                if (a.IsEmpty) return b.Copy();
                if (b == null || b.IsEmpty) return a.Copy();
            }

            CheckNotGeometryCollection(a, b);

            return Overlay(a, b, SpatialFunction.SymDifference);
        }

        /// <summary>
        /// Computes the union of all the elements in the <c>Geometry</c> <c>a</c>.
        /// </summary>
        /// <param name="a">The <c>Geometry</c></param>
        /// <returns>The union of <c>a</c></returns>
        /// <seealso cref="Geometry.Union()"/>
        public abstract Geometry Union(Geometry a);


        private static void CheckNotGeometryCollection(Geometry a, Geometry b)
        {
            if (a.OgcGeometryType == OgcGeometryType.GeometryCollection)
                throw new ArgumentException("Operation does not support GeometryCollection arguments", nameof(a));
            if (b.OgcGeometryType == OgcGeometryType.GeometryCollection)
                throw new ArgumentException("Operation does not support GeometryCollection arguments", nameof(b));
        }

        #region Standard implementations

        private sealed class OverlayV1 : GeometryOverlay
        {
            public static GeometryOverlay Instance { get; } = new OverlayV1();

            private OverlayV1()
            {
            }

            protected override Geometry Overlay(Geometry geom0, Geometry geom1, SpatialFunction opCode)
            {
                return SnapIfNeededOverlayOp.Overlay(geom0, geom1, opCode);
            }

            public override Geometry Union(Geometry a)
            {
                return UnaryUnionOp.Union(a);
            }

            public override string ToString()
            {
                return "Legacy";
            }
        }

        private sealed class OverlayV2 : GeometryOverlay
        {
            public static GeometryOverlay Instance { get; } = new OverlayV2();

            private OverlayV2()
            {
            }

            protected override Geometry Overlay(Geometry geom0, Geometry geom1, SpatialFunction opCode)
            {
                return OverlayNGRobust.Overlay(geom0, geom1, opCode);
            }

            public override Geometry Union(Geometry a)
            {
                return OverlayNGRobust.Union(a);
            }

            public override string ToString()
            {
                return "NG";
            }
        }
        #endregion
    }
}
