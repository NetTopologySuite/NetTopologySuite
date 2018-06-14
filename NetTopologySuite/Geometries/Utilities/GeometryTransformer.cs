using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A framework for processes which transform an input <c>Geometry</c> into
    /// an output <see cref="IGeometry"/>, possibly changing its structure and type(s).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is a framework for implementing subclasses
    /// which perform transformations on
    /// various different Geometry subclasses.
    /// </para>
    /// <para>
    /// It provides an easy way of applying specific transformations
    /// to given point types, while allowing unhandled types to be simply copied.
    /// Also, the framework handles ensuring that if subcomponents change type
    /// the parent geometries types change appropriately to maintain valid structure.
    /// Subclasses will override whichever <c>TransformX</c> methods
    /// they need to to handle particular Geometry types.
    /// </para>
    /// <para>
    /// A typically usage would be a transformation that may transform <c>Polygons</c> into
    /// <c>Polygons</c>, <c>LineStrings</c> or <c>Points</c>, depending on the geometry of the input
    /// (For instance, a simplification operation).
    /// This class would likely need to override the <see cref="TransformMultiPolygon(IMultiPolygon, IGeometry)"/>
    /// method to ensure that if input Polygons change type the result is a <c>GeometryCollection</c>,
    /// not a <c>MultiPolygon</c>.</para>
    /// <para>
    /// The default behaviour of this class is simply to recursively transform
    /// each Geometry component into an identical object by deep copying down
    /// to the level of, but not including, coordinates.
    /// </para>
    /// <para>
    /// Note that all <c>Transform<i>XXX</i></c> methods may return <c>null</c>,
    /// to avoid creating empty point objects. This will be handled correctly
    /// by the transformer. <c>Transform<i>XXX</i></c> methods should always return valid
    /// geometry - if they cannot do this they should return <c>null</c>
    /// (for instance, it may not be possible for a transformLineString implementation
    /// to return at least two points - in this case, it should return <c>null</c>).
    /// The <see cref="Transform(IGeometry)"/> method itself will always
    /// return a non-null Geometry object (but this may be empty).</para>
    /// </remarks>>
    public class GeometryTransformer
    {
        /*
        * Possible extensions:
        * GetParent() method to return immediate parent e.g. of LinearRings in Polygons
        */

        private IGeometry _inputGeom;

        /// <summary>
        /// The geometry factory
        /// </summary>
        protected IGeometryFactory Factory;

        // these could eventually be exposed to clients
        /// <summary>
        /// <c>true</c> if empty geometries should not be included in the result.
        /// </summary>
        private bool pruneEmptyGeometry = true;

        /// <summary>
        /// <c>true</c> if a homogenous collection result
        /// from a <c>GeometryCollection</c> should still
        /// be a general GeometryCollection.
        /// </summary>
        private bool preserveGeometryCollectionType = true;

        /// <summary>
        /// <c>true</c> if the type of the input should be preserved.
        /// </summary>
        private bool _preserveType;

        ///// <summary>
        /////
        ///// </summary>
        /*public GeometryTransformer() { }*/

        /// <summary>
        /// Makes the input geometry available
        /// </summary>
        public IGeometry InputGeometry => _inputGeom;

        /// <summary>
        ///
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <returns></returns>
        public IGeometry Transform(IGeometry inputGeom)
        {
            _inputGeom = inputGeom;
            Factory = inputGeom.Factory;
            if (inputGeom is IPoint)
                return TransformPoint((IPoint)inputGeom, null);
            if (inputGeom is IMultiPoint)
                return TransformMultiPoint((IMultiPoint)inputGeom, null);
            if (inputGeom is ILinearRing)
                return TransformLineString((ILinearRing)inputGeom, null);
            if (inputGeom is ILineString)
                return TransformLineString((ILineString)inputGeom, null);
            if (inputGeom is IMultiLineString)
                return TransformMultiLineString((IMultiLineString)inputGeom, null);
            if (inputGeom is IPolygon)
                return TransformPolygon((IPolygon)inputGeom, null);
            if (inputGeom is IMultiPolygon)
                return TransformMultiPolygon((IMultiPolygon)inputGeom, null);
            if (inputGeom is IGeometryCollection)
                return TransformGeometryCollection((IGeometryCollection)inputGeom, null);
            throw new ArgumentException("Unknown Geometry subtype: " + inputGeom.GetType().FullName);
        }

        /// <summary>
        /// Convenience method which provides standard way of
        /// creating a <c>CoordinateSequence</c>.
        /// </summary>
        /// <param name="coords">The coordinate array to copy.</param>
        /// <returns>A coordinate sequence for the array.</returns>
        protected virtual ICoordinateSequence CreateCoordinateSequence(Coordinate[] coords)
        {
            return Factory.CoordinateSequenceFactory.Create(coords);
        }

        /// <summary>
        /// Convenience method which provides a standard way of copying <see cref="ICoordinateSequence"/>s.
        /// </summary>
        /// <param name="seq">The sequence to copy.</param>
        /// <returns>A deep copy of the sequence.</returns>
        protected virtual ICoordinateSequence Copy(ICoordinateSequence seq)
        {
            return seq.Copy();
        }

        /// <summary>
        /// Transforms a <see cref="ICoordinateSequence"/>.
        /// This method should always return a valid coordinate list for
        /// the desired result type.  (E.g. a coordinate list for a LineString
        /// must have 0 or at least 2 points).
        /// If this is not possible, return an empty sequence -
        /// this will be pruned out.
        /// </summary>
        /// <param name="coords">The coordinates to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>The transformed coordinates</returns>
        protected virtual ICoordinateSequence TransformCoordinates(ICoordinateSequence coords, IGeometry parent)
        {
            return Copy(coords);
        }

        /// <summary>
        /// Transforms a <see cref="IPoint"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>Point</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>Point</c></returns>
        protected virtual IGeometry TransformPoint(IPoint geom, IGeometry parent)
        {
            return Factory.CreatePoint(TransformCoordinates(geom.CoordinateSequence, geom));
        }

        /// <summary>
        /// Transforms a <see cref="IMultiPoint"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>MultiPoint</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>MultiPoint</c></returns>
        protected virtual IGeometry TransformMultiPoint(IMultiPoint geom, IGeometry parent)
        {
            var transGeomList = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var transformGeom = TransformPoint((IPoint)geom.GetGeometryN(i), geom);
                if (transformGeom == null) continue;
                if (transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            return Factory.BuildGeometry(transGeomList);
        }

        /// <summary>
        /// Transforms a <see cref="ILinearRing"/>.
        /// <para/>
        /// The transformation of a <c>LinearRing</c> may result in a coordinate sequence
        /// which does not form a structurally valid ring (i.e. a degenerate ring of 3 or fewer points).
        /// In this case a <c>LineString</c> is returned.
        /// Subclasses may wish to override this method and check for this situation
        /// (e.g.a subclass may choose to eliminate degenerate linear rings)
        /// </summary>
        /// <param name="geom">The <c>LinearRing</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>
        /// A <c>LinearRing</c> if the transformation resulted in a structurally valid ring, otherwise,
        /// if the transformation caused the LinearRing to collapse to 3 or fewer points, a <c>LineString</c>
        /// </returns>
        protected virtual IGeometry TransformLinearRing(ILinearRing geom, IGeometry parent)
        {
            var seq = TransformCoordinates(geom.CoordinateSequence, geom);
            if (seq == null)
                return Factory.CreateLinearRing((ICoordinateSequence) null);

            int seqSize = seq.Count;
            // ensure a valid LinearRing
            if (seqSize > 0 && seqSize < 4 && !_preserveType)
                return Factory.CreateLineString(seq);
            return Factory.CreateLinearRing(seq);
        }

        /// <summary>
        /// Transforms a <see cref="ILineString"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>LineString</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>LineString</c></returns>
        protected virtual IGeometry TransformLineString(ILineString geom, IGeometry parent)
        {
            // should check for 1-point sequences and downgrade them to points
            return Factory.CreateLineString(TransformCoordinates(geom.CoordinateSequence, geom));
        }

        /// <summary>
        /// Transforms a <see cref="IMultiLineString"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>MultiLineString</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>MultiLineString</c></returns>
        protected virtual IGeometry TransformMultiLineString(IMultiLineString geom, IGeometry parent)
        {
            var transGeomList = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var transformGeom = TransformLineString((ILineString)geom.GetGeometryN(i), geom);
                if (transformGeom == null) continue;
                if (transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            return Factory.BuildGeometry(transGeomList);
        }

        /// <summary>
        /// Transforms a <see cref="IPolygon"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>Polygon</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>Polygon</c></returns>
        protected virtual IGeometry TransformPolygon(IPolygon geom, IGeometry parent)
        {
            bool isAllValidLinearRings = true;
            var shell = TransformLinearRing(geom.Shell, geom);

            if (shell == null || !(shell is ILinearRing) || shell.IsEmpty)
                isAllValidLinearRings = false;

            var holes = new List<ILineString>();
            for (int i = 0; i < geom.NumInteriorRings; i++)
            {
                var hole = TransformLinearRing(geom.Holes[i], geom);
                if (hole == null || hole.IsEmpty) continue;
                if (!(hole is ILinearRing))
                    isAllValidLinearRings = false;
                holes.Add((ILineString)hole);
            }

            if (isAllValidLinearRings)
            {
                var holesAsLinearRing = new ILinearRing[holes.Count];

                // in case your IDE whines about array covariance on this line,
                // don't worry, it's safe -- we proved above that it will work.
                holes.CopyTo(holesAsLinearRing);
                return Factory.CreatePolygon((ILinearRing)shell, holesAsLinearRing);
            }
            else
            {
                var components = new List<IGeometry>();
                if (shell != null)
                    components.Add(shell);
                foreach (var hole in holes)
                    components.Add(hole);
                return Factory.BuildGeometry(components);
            }
        }

        /// <summary>
        /// Transforms a <see cref="IMultiPolygon"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>MultiPolygon</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>MultiPolygon</c></returns>
        protected virtual IGeometry TransformMultiPolygon(IMultiPolygon geom, IGeometry parent)
        {
            var transGeomList = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var transformGeom = TransformPolygon((IPolygon)geom.GetGeometryN(i), geom);
                if (transformGeom == null) continue;
                if (transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            return Factory.BuildGeometry(transGeomList);
        }

        /// <summary>
        /// Transforms a <see cref="IGeometryCollection"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>GeometryCollection</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>GeometryCollection</c></returns>
        protected virtual IGeometry TransformGeometryCollection(IGeometryCollection geom, IGeometry parent)
        {
            var transGeomList = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var transformGeom = Transform(geom.GetGeometryN(i));
                if (transformGeom == null) continue;
                if (pruneEmptyGeometry && transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            if (preserveGeometryCollectionType)
                return Factory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(transGeomList));
            return Factory.BuildGeometry(transGeomList);
        }
    }
}
