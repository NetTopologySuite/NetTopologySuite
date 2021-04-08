using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A framework for processes which transform an input <c>Geometry</c> into
    /// an output <see cref="Geometry"/>, possibly changing its structure and type(s).
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
    /// This class would likely need to override the <see cref="TransformMultiPolygon(MultiPolygon, Geometry)"/>
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
    /// The <see cref="Transform(Geometry)"/> method itself will always
    /// return a non-null Geometry object (but this may be empty).</para>
    /// </remarks>>
    public class GeometryTransformer
    {
        /*
        * Possible extensions:
        * GetParent() method to return immediate parent e.g. of LinearRings in Polygons
        */

        private Geometry _inputGeom;

        /// <summary>
        /// The geometry factory
        /// </summary>
        protected GeometryFactory Factory;

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
#pragma warning disable 649
        private bool _preserveType;
#pragma warning restore 649

        ///// <summary>
        /////
        ///// </summary>
        /*public GeometryTransformer() { }*/

        /// <summary>
        /// Makes the input geometry available
        /// </summary>
        public Geometry InputGeometry => _inputGeom;

        /// <summary>
        ///
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <returns></returns>
        public Geometry Transform(Geometry inputGeom)
        {
            _inputGeom = inputGeom;
            Factory = inputGeom.Factory;
            if (inputGeom is Point)
                return TransformPoint((Point)inputGeom, null);
            if (inputGeom is MultiPoint)
                return TransformMultiPoint((MultiPoint)inputGeom, null);
            if (inputGeom is LinearRing)
                return TransformLinearRing((LinearRing)inputGeom, null);
            if (inputGeom is LineString)
                return TransformLineString((LineString)inputGeom, null);
            if (inputGeom is MultiLineString)
                return TransformMultiLineString((MultiLineString)inputGeom, null);
            if (inputGeom is Polygon)
                return TransformPolygon((Polygon)inputGeom, null);
            if (inputGeom is MultiPolygon)
                return TransformMultiPolygon((MultiPolygon)inputGeom, null);
            if (inputGeom is GeometryCollection)
                return TransformGeometryCollection((GeometryCollection)inputGeom, null);
            throw new ArgumentException("Unknown Geometry subtype: " + inputGeom.GetType().FullName);
        }

        /// <summary>
        /// Convenience method which provides standard way of
        /// creating a <c>CoordinateSequence</c>.
        /// </summary>
        /// <param name="coords">The coordinate array to copy.</param>
        /// <returns>A coordinate sequence for the array.</returns>
        protected virtual CoordinateSequence CreateCoordinateSequence(Coordinate[] coords)
        {
            return Factory.CoordinateSequenceFactory.Create(coords);
        }

        /// <summary>
        /// Convenience method which provides a standard way of copying <see cref="CoordinateSequence"/>s.
        /// </summary>
        /// <param name="seq">The sequence to copy.</param>
        /// <returns>A deep copy of the sequence.</returns>
        protected virtual CoordinateSequence Copy(CoordinateSequence seq)
        {
            return seq.Copy();
        }

        /// <summary>
        /// Transforms a <see cref="CoordinateSequence"/>.
        /// This method should always return a valid coordinate list for
        /// the desired result type.  (E.g. a coordinate list for a LineString
        /// must have 0 or at least 2 points).
        /// If this is not possible, return an empty sequence -
        /// this will be pruned out.
        /// </summary>
        /// <param name="coords">The coordinates to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>The transformed coordinates</returns>
        protected virtual CoordinateSequence TransformCoordinates(CoordinateSequence coords, Geometry parent)
        {
            return Copy(coords);
        }

        /// <summary>
        /// Transforms a <see cref="Point"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>Point</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>Point</c></returns>
        protected virtual Geometry TransformPoint(Point geom, Geometry parent)
        {
            return Factory.CreatePoint(TransformCoordinates(geom.CoordinateSequence, geom));
        }

        /// <summary>
        /// Transforms a <see cref="MultiPoint"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>MultiPoint</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>MultiPoint</c></returns>
        protected virtual Geometry TransformMultiPoint(MultiPoint geom, Geometry parent)
        {
            var transGeomList = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var transformGeom = TransformPoint((Point)geom.GetGeometryN(i), geom);
                if (transformGeom == null) continue;
                if (transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            if (transGeomList.Count == 0)
            {
                return Factory.CreateMultiPoint();
            }

            return Factory.BuildGeometry(transGeomList);
        }

        /// <summary>
        /// Transforms a <see cref="LinearRing"/>.
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
        protected virtual Geometry TransformLinearRing(LinearRing geom, Geometry parent)
        {
            var seq = TransformCoordinates(geom.CoordinateSequence, geom);
            if (seq == null)
                return Factory.CreateLinearRing((CoordinateSequence) null);

            int seqSize = seq.Count;
            // ensure a valid LinearRing
            if (seqSize > 0 && seqSize < 4 && !_preserveType)
                return Factory.CreateLineString(seq);
            return Factory.CreateLinearRing(seq);
        }

        /// <summary>
        /// Transforms a <see cref="LineString"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>LineString</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>LineString</c></returns>
        protected virtual Geometry TransformLineString(LineString geom, Geometry parent)
        {
            // should check for 1-point sequences and downgrade them to points
            return Factory.CreateLineString(TransformCoordinates(geom.CoordinateSequence, geom));
        }

        /// <summary>
        /// Transforms a <see cref="MultiLineString"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>MultiLineString</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>MultiLineString</c></returns>
        protected virtual Geometry TransformMultiLineString(MultiLineString geom, Geometry parent)
        {
            var transGeomList = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var transformGeom = TransformLineString((LineString)geom.GetGeometryN(i), geom);
                if (transformGeom == null) continue;
                if (transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            if (transGeomList.Count == 0)
            {
                return Factory.CreateMultiLineString();
            }
            return Factory.BuildGeometry(transGeomList);
        }

        /// <summary>
        /// Transforms a <see cref="Polygon"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>Polygon</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>Polygon</c></returns>
        protected virtual Geometry TransformPolygon(Polygon geom, Geometry parent)
        {
            bool isAllValidLinearRings = true;
            var shell = TransformLinearRing(geom.Shell, geom);

            // handle empty inputs, or inputs which are made empty
            bool shellIsNullOrEmpty = shell == null || shell.IsEmpty;
            if (geom.IsEmpty && shellIsNullOrEmpty)
            {
                return Factory.CreatePolygon();
            }

            if (shellIsNullOrEmpty || !(shell is LinearRing))
                isAllValidLinearRings = false;

            var holes = new List<LineString>();
            for (int i = 0; i < geom.NumInteriorRings; i++)
            {
                var hole = TransformLinearRing(geom.Holes[i], geom);
                if (hole == null || hole.IsEmpty) continue;
                if (!(hole is LinearRing))
                    isAllValidLinearRings = false;
                holes.Add((LineString)hole);
            }

            if (isAllValidLinearRings)
            {
                var holesAsLinearRing = new LinearRing[holes.Count];

                // in case your IDE whines about array covariance on this line,
                // don't worry, it's safe -- we proved above that it will work.
                holes.CopyTo(holesAsLinearRing);
                return Factory.CreatePolygon((LinearRing)shell, holesAsLinearRing);
            }
            else
            {
                var components = new List<Geometry>();
                if (shell != null)
                    components.Add(shell);
                foreach (var hole in holes)
                    components.Add(hole);
                return Factory.BuildGeometry(components);
            }
        }

        /// <summary>
        /// Transforms a <see cref="MultiPolygon"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>MultiPolygon</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>MultiPolygon</c></returns>
        protected virtual Geometry TransformMultiPolygon(MultiPolygon geom, Geometry parent)
        {
            var transGeomList = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var transformGeom = TransformPolygon((Polygon)geom.GetGeometryN(i), geom);
                if (transformGeom == null) continue;
                if (transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            if (transGeomList.Count == 0)
            {
                return Factory.CreateMultiPolygon();
            }
            return Factory.BuildGeometry(transGeomList);
        }

        /// <summary>
        /// Transforms a <see cref="GeometryCollection"/> geometry.
        /// </summary>
        /// <param name="geom">The <c>GeometryCollection</c> to transform</param>
        /// <param name="parent">The parent geometry</param>
        /// <returns>A <c>GeometryCollection</c></returns>
        protected virtual Geometry TransformGeometryCollection(GeometryCollection geom, Geometry parent)
        {
            var transGeomList = new List<Geometry>();
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
