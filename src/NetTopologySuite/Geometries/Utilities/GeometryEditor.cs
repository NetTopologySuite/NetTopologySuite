using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A class which supports creating new <see cref="Geometry"/>s
    /// which are modifications of existing ones,
    /// maintaining the same type structure.
    /// </summary>
    /// <remarks>
    /// Geometry objects are intended to be treated as immutable.
    /// This class allows you to "modifies" a Geometrys
    /// by traversing them, applying a user-defined
    /// <see cref="IGeometryEditorOperation"/>, <see cref="CoordinateSequenceOperation"/> or <see cref="CoordinateOperation"/>
    /// and creating a new Geometrys with the same structure but
    /// (possibly) modified components.
    /// <para>
    /// Examples of the kinds of modifications which can be made are:
    /// <list type="bullet">
    /// <item><description>
    /// The values of the coordinates may be changed.
    /// The editor does not check whether changing coordinate values makes the result Geometry invalid
    /// </description></item>
    /// <item><description>
    /// The coordinate lists may be changed (e.g. by adding, deleting or modifying coordinates).
    /// The modified coordinate lists must be consistent with their original parent component
    /// (e.g. a <tt>LinearRing</tt> must always have at least 4 coordinates, and the first and last
    /// coordinate must be equal).
    /// </description></item>
    /// <item><description>
    /// Components of the original point may be deleted
    /// (e.g. holes may be removed from a Polygon, or LineStrings removed from a MultiLineString).
    /// Deletions will be propagated up the component tree appropriately.
    /// </description></item></list>
    /// </para>
    /// <para>
    /// All changes must be consistent with the original Geometry's structure
    /// (e.g. a <c>Polygon</c> cannot be collapsed into a <c>LineString</c>).
    /// If changing the structure is required, use a <see cref="GeometryTransformer"/>.
    /// </para>
    /// <para>
    /// This class supports creating an edited Geometry
    /// using a different <see cref="GeometryFactory"/> via the <see cref="GeometryEditor(GeometryFactory)"/>
    /// constructor.
    /// Examples of situations where this is required is if the geometry is
    /// transformed to a new SRID and/or a new PrecisionModel.</para>
    /// <para>
    /// Usage notes
    /// <list type="bullet">
    /// <item><description>The resulting Geometry is not checked for validity.
    /// If validity needs to be enforced, the new Geometry's
    /// <see cref="Geometry.IsValid"/> method should be called.</description></item>
    /// <item><description>By default the UserData of the input geometry is not copied to the result.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="GeometryTransformer"/>
    /// <seealso cref="Geometry.IsValid"/>
    public class GeometryEditor
    {
        /// <summary>
        /// The factory used to create the modified Geometry.
        /// </summary>
        /// <remarks>
        /// If <tt>null</tt> the GeometryFactory of the input is used.
        /// </remarks>
        private GeometryFactory _factory;

        private bool _isUserDataCopied;

        /// <summary>
        /// Creates a new GeometryEditor object which will create
        /// edited <see cref="Geometry"/> with the same <see cref="GeometryFactory"/> as the input Geometry.
        /// </summary>
        public GeometryEditor() { }

        /// <summary>
        /// Creates a new GeometryEditor object which will create
        /// edited <see cref="Geometry"/>s with the given <see cref="GeometryFactory"/>.
        /// </summary>
        /// <param name="factory">The GeometryFactory to create the edited Geometry with.</param>
        public GeometryEditor(GeometryFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Gets or sets a value indicating if the User Data is copied to the edit result.
        /// If so, only the object reference is copied.
        /// </summary>
        public bool CopyUserData
        {
            get => _isUserDataCopied;
            set => _isUserDataCopied = value;
        }

        /// <summary>
        /// Edit the input <c>Geometry</c> with the given edit operation.
        /// Clients can create subclasses of GeometryEditorOperation or
        /// CoordinateOperation to perform required modifications.
        /// </summary>
        /// <param name="geometry">The Geometry to edit.</param>
        /// <param name="operation">The edit operation to carry out.</param>
        /// <returns>A new <c>Geometry</c> which is the result of the editing (which may be empty).</returns>
        public Geometry Edit(Geometry geometry, IGeometryEditorOperation operation)
        {
            // if client did not supply a GeometryFactory, use the one from the input Geometry
            if (_factory == null)
                _factory = geometry.Factory;

            var result = EditInternal(geometry, operation);
            if (_isUserDataCopied)
            {
                result.UserData = geometry.UserData;
            }
            return result;
        }

        private Geometry EditInternal(Geometry geometry, IGeometryEditorOperation operation)
        {
            if (geometry is GeometryCollection)
                return EditGeometryCollection((GeometryCollection)geometry, operation);
            if (geometry is Polygon)
                return EditPolygon((Polygon)geometry, operation);
            if (geometry is Point)
                return operation.Edit(geometry, _factory);
            if (geometry is LineString)
                return operation.Edit(geometry, _factory);
            NetTopologySuite.Utilities.Assert.ShouldNeverReachHere("Unsupported Geometry classes should be caught in the GeometryEditorOperation.");
            return null;
        }

        private Polygon EditPolygon(Polygon polygon, IGeometryEditorOperation operation)
        {
            var newPolygon = (Polygon)operation.Edit(polygon, _factory);
            // create one if needed
            if (newPolygon == null)
                newPolygon = _factory.CreatePolygon();
            if (newPolygon.IsEmpty)
                //RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
                return newPolygon;

            var shell = (LinearRing)Edit(newPolygon.ExteriorRing, operation);
            if (shell == null || shell.IsEmpty)
                //RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
                return _factory.CreatePolygon();

            var holes = new List<LinearRing>();
            for (int i = 0; i < newPolygon.NumInteriorRings; i++)
            {
                var hole = (LinearRing)Edit(newPolygon.GetInteriorRingN(i), operation);
                if (hole == null || hole.IsEmpty) continue;
                holes.Add(hole);
            }

            return _factory.CreatePolygon(shell, holes.ToArray());
        }

        private GeometryCollection EditGeometryCollection(GeometryCollection collection, IGeometryEditorOperation operation)
        {
            // first edit the entire collection
            // MD - not sure why this is done - could just check original collection?
            var collectionForType = (GeometryCollection)operation.Edit(collection, _factory);

            // edit the component geometries
            var geometries = new List<Geometry>();
            for (int i = 0; i < collectionForType.NumGeometries; i++)
            {
                var geometry = Edit(collectionForType.GetGeometryN(i), operation);
                if (geometry == null || geometry.IsEmpty) continue;
                geometries.Add(geometry);
            }

            if (collectionForType is MultiPoint)
                return _factory.CreateMultiPoint(geometries.Cast<Point>().ToArray());

            if (collectionForType is MultiLineString)
                return _factory.CreateMultiLineString(geometries.Cast<LineString>().ToArray());

            if (collectionForType is MultiPolygon)
                return _factory.CreateMultiPolygon(geometries.Cast<Polygon>().ToArray());

            return _factory.CreateGeometryCollection(geometries.ToArray());
        }

        /// <summary>
        /// A interface which specifies an edit operation for Geometries.
        /// </summary>
        public interface IGeometryEditorOperation
        {
            /// <summary>
            /// Edits a Geometry by returning a new Geometry with a modification.
            /// The returned Geometry may be the input geometry itself.
            /// It may be <c>null</c> if the geometry is to be deleted.
            /// </summary>
            /// <param name="geometry">The Geometry to modify.</param>
            /// <param name="factory">
            /// The factory with which to construct the modified Geometry
            /// (may be different to the factory of the input point).
            /// </param>
            /// <returns>A new Geometry which is a modification of the input Geometry.</returns>
            /// <returns><c>null</c> if the Geometry is to be deleted completely</returns>
            Geometry Edit(Geometry geometry, GeometryFactory factory);
        }

        /// <summary>
        /// A GeometryEditorOperation which does not modify
        /// the input geometry.
        /// This can be used for simple changes of
        /// <see cref="GeometryFactory"/> (including PrecisionModel and SRID).
        /// </summary>
        /// <author>mbdavis</author>
        public class NoOpGeometryOperation : IGeometryEditorOperation
        {
            internal NoOpGeometryOperation()
            {
            }

            public Geometry Edit(Geometry geometry, GeometryFactory factory)
            {
                return geometry;
            }
        }

        /// <summary>
        /// A GeometryEditorOperation which edits the coordinate list of a <c>Geometry</c>.
        /// Operates on Geometry subclasses which contains a single coordinate list.
        /// </summary>
        public abstract class CoordinateOperation : IGeometryEditorOperation
        {
            /// <summary>
            ///
            /// </summary>
            /// <param name="geometry"></param>
            /// <param name="factory"></param>
            /// <returns></returns>
            public Geometry Edit(Geometry geometry, GeometryFactory factory)
            {
                if (geometry is LinearRing)
                    return factory.CreateLinearRing(Edit(geometry.Coordinates, geometry));

                if (geometry is LineString)
                    return factory.CreateLineString(Edit(geometry.Coordinates, geometry));

                if (geometry is Point)
                {
                    var newCoordinates = Edit(geometry.Coordinates, geometry);
                    return factory.CreatePoint((newCoordinates.Length > 0) ? newCoordinates[0] : null);
                }

                return geometry;
            }

            /// <summary>
            /// Edits the array of <c>Coordinate</c>s from a <c>Geometry</c>.
            /// </summary>
            /// <param name="coordinates">The coordinate array to operate on.</param>
            /// <param name="geometry">The point containing the coordinate list.</param>
            /// <returns>An edited coordinate array (which may be the same as the input).</returns>
            public abstract Coordinate[] Edit(Coordinate[] coordinates, Geometry geometry);
        }

        /// <summary>
        /// A <see cref="IGeometryEditorOperation"/> which edits the <see cref="CoordinateSequence"/>
        /// of a <see cref="Geometry"/>.
        /// <para/>
        /// Operates on Geometry subclasses which contains a single coordinate list.
        /// </summary>
        public class CoordinateSequenceOperation : IGeometryEditorOperation
        {
            public CoordinateSequenceOperation()
                :this((s, g) => s)
            {
            }

            public CoordinateSequenceOperation(Func<CoordinateSequence, Geometry, CoordinateSequence> editSequence)
            {
                EditSequence = editSequence;
            }

            public Geometry Edit(Geometry geometry, GeometryFactory factory)
            {
                var linearRing = geometry as LinearRing;
                if (linearRing != null)
                {
                    return factory.CreateLinearRing(EditSequence(
                        (linearRing).CoordinateSequence, geometry));
                }

                var lineString = geometry as LineString;
                if (lineString != null)
                {
                    return factory.CreateLineString(EditSequence(
                        (lineString).CoordinateSequence,
                        geometry));
                }

                var point = geometry as Point;
                if (point != null)
                {
                    return factory.CreatePoint(EditSequence(
                        (point).CoordinateSequence, geometry));
                }

                return geometry;
            }

            /// <summary>
            /// Edits a <see cref="CoordinateSequence"/> from a <see cref="Geometry"/>.
            /// </summary>
            ///// <param name="coordSeq">The coordinate array to operate on</param>
            ///// <param name="geometry">The geometry containing the coordinate list</param>
            /// <returns>An edited coordinate sequence (which may be the same as the input)</returns>
            protected Func<CoordinateSequence, Geometry, CoordinateSequence> EditSequence { get; set; }
        }

    }
}
