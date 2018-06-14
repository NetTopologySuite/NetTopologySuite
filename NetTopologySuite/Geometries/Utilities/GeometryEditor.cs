using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A class which supports creating new <see cref="IGeometry"/>s
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
    /// <list type="Bullet">
    /// <item>
    /// The values of the coordinates may be changed.
    /// The editor does not check whether changing coordinate values makes the result Geometry invalid
    /// </item>
    /// <item>
    /// The coordinate lists may be changed (e.g. by adding, deleting or modifying coordinates).
    /// The modified coordinate lists must be consistent with their original parent component
    /// (e.g. a <tt>LinearRing</tt> must always have at least 4 coordinates, and the first and last
    /// coordinate must be equal).
    /// </item>
    /// <item>
    /// Components of the original point may be deleted
    /// (e.g. holes may be removed from a Polygon, or LineStrings removed from a MultiLineString).
    /// Deletions will be propagated up the component tree appropriately.
    /// </item></list>
    /// </para>
    /// <para>
    /// All changes must be consistent with the original Geometry's structure
    /// (e.g. a <code>Polygon</code> cannot be collapsed into a <code>LineString</code>).
    /// If changing the structure is required, use a <see cref="GeometryTransformer"/>.
    /// </para>
    /// <para>
    /// This class supports creating an edited Geometry
    /// using a different <see cref="IGeometryFactory"/> via the <see cref="GeometryEditor(IGeometryFactory)"/>
    /// constructor.
    /// Examples of situations where this is required is if the geometry is
    /// transformed to a new SRID and/or a new PrecisionModel.</para>
    /// <para>
    /// Usage notes
    /// <list type="Bullet">
    /// <item>The resulting Geometry is not checked for validity.
    /// If validity needs to be enforced, the new Geometry's
    /// <see cref="IGeometry.IsValid"/> method should be called.</item>
    /// <item>By default the UserData of the input geometry is not copied to the result. </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="GeometryTransformer"/>
    /// <seealso cref="IGeometry.IsValid"/>
    public class GeometryEditor
    {
        /// <summary>
        /// The factory used to create the modified Geometry.
        /// </summary>
        /// <remarks>
        /// If <tt>null</tt> the GeometryFactory of the input is used.
        /// </remarks>
        private IGeometryFactory _factory;

        private bool _isUserDataCopied;

        /// <summary>
        /// Creates a new GeometryEditor object which will create
        /// edited <see cref="IGeometry"/> with the same <see cref="IGeometryFactory"/> as the input Geometry.
        /// </summary>
        public GeometryEditor() { }

        /// <summary>
        /// Creates a new GeometryEditor object which will create
        /// edited <see cref="IGeometry"/>s with the given <see cref="IGeometryFactory"/>.
        /// </summary>
        /// <param name="factory">The GeometryFactory to create the edited Geometry with.</param>
        public GeometryEditor(IGeometryFactory factory)
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
        public IGeometry Edit(IGeometry geometry, IGeometryEditorOperation operation)
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

        private IGeometry EditInternal(IGeometry geometry, IGeometryEditorOperation operation)
        {
            if (geometry is IGeometryCollection)
                return EditGeometryCollection((IGeometryCollection)geometry, operation);
            if (geometry is IPolygon)
                return EditPolygon((IPolygon)geometry, operation);
            if (geometry is IPoint)
                return operation.Edit(geometry, _factory);
            if (geometry is ILineString)
                return operation.Edit(geometry, _factory);
            NetTopologySuite.Utilities.Assert.ShouldNeverReachHere("Unsupported Geometry classes should be caught in the GeometryEditorOperation.");
            return null;
        }

        private IPolygon EditPolygon(IPolygon polygon, IGeometryEditorOperation operation)
        {
            var newPolygon = (IPolygon)operation.Edit(polygon, _factory);
            // create one if needed
            if (newPolygon == null)
                newPolygon = _factory.CreatePolygon();
            if (newPolygon.IsEmpty)
                //RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
                return newPolygon;

            var shell = (ILinearRing)Edit(newPolygon.ExteriorRing, operation);
            if (shell == null || shell.IsEmpty)
                //RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
                return _factory.CreatePolygon();

            var holes = new List<ILinearRing>();
            for (int i = 0; i < newPolygon.NumInteriorRings; i++)
            {
                var hole = (ILinearRing)Edit(newPolygon.GetInteriorRingN(i), operation);
                if (hole == null || hole.IsEmpty) continue;
                holes.Add(hole);
            }

            return _factory.CreatePolygon(shell, holes.ToArray());
        }

        private IGeometryCollection EditGeometryCollection(IGeometryCollection collection, IGeometryEditorOperation operation)
        {
            // first edit the entire collection
            // MD - not sure why this is done - could just check original collection?
            var collectionForType = (IGeometryCollection)operation.Edit(collection, _factory);

            // edit the component geometries
            var geometries = new List<IGeometry>();
            for (int i = 0; i < collectionForType.NumGeometries; i++)
            {
                var geometry = Edit(collectionForType.GetGeometryN(i), operation);
                if (geometry == null || geometry.IsEmpty) continue;
                geometries.Add(geometry);
            }

            if (collectionForType is IMultiPoint)
                return _factory.CreateMultiPoint(geometries.Cast<IPoint>().ToArray());

            if (collectionForType is IMultiLineString)
                return _factory.CreateMultiLineString(geometries.Cast<ILineString>().ToArray());

            if (collectionForType is IMultiPolygon)
                return _factory.CreateMultiPolygon(geometries.Cast<IPolygon>().ToArray());

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
            /// It may be <code>null</code> if the geometry is to be deleted.
            /// </summary>
            /// <param name="geometry">The Geometry to modify.</param>
            /// <param name="factory">
            /// The factory with which to construct the modified Geometry
            /// (may be different to the factory of the input point).
            /// </param>
            /// <returns>A new Geometry which is a modification of the input Geometry.</returns>
            /// <returns><value>null</value> if the Geometry is to be deleted completely</returns>
            IGeometry Edit(IGeometry geometry, IGeometryFactory factory);
        }

        /// <summary>
        /// A GeometryEditorOperation which does not modify
        /// the input geometry.
        /// This can be used for simple changes of
        /// <see cref="IGeometryFactory"/> (including PrecisionModel and SRID).
        /// </summary>
        /// <author>mbdavis</author>
        public class NoOpGeometryOperation : IGeometryEditorOperation
        {
            internal NoOpGeometryOperation()
            {
            }

            public IGeometry Edit(IGeometry geometry, IGeometryFactory factory)
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
            public IGeometry Edit(IGeometry geometry, IGeometryFactory factory)
            {
                if (geometry is ILinearRing)
                    return factory.CreateLinearRing(Edit(geometry.Coordinates, geometry));

                if (geometry is ILineString)
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
            public abstract Coordinate[] Edit(Coordinate[] coordinates, IGeometry geometry);
        }

        /// <summary>
        /// A <see cref="IGeometryEditorOperation"/> which edits the <see cref="ICoordinateSequence"/>
        /// of a <see cref="IGeometry"/>.
        /// <para/>
        /// Operates on Geometry subclasses which contains a single coordinate list.
        /// </summary>
        public class CoordinateSequenceOperation : IGeometryEditorOperation
        {
            public CoordinateSequenceOperation()
                :this((s, g) => s)
            {
            }

            public CoordinateSequenceOperation(Func<ICoordinateSequence, IGeometry, ICoordinateSequence> editSequence)
            {
                EditSequence = editSequence;
            }

            public IGeometry Edit(IGeometry geometry, IGeometryFactory factory)
            {
                var linearRing = geometry as ILinearRing;
                if (linearRing != null)
                {
                    return factory.CreateLinearRing(EditSequence(
                        (linearRing).CoordinateSequence, geometry));
                }

                var lineString = geometry as ILineString;
                if (lineString != null)
                {
                    return factory.CreateLineString(EditSequence(
                        (lineString).CoordinateSequence,
                        geometry));
                }

                var point = geometry as IPoint;
                if (point != null)
                {
                    return factory.CreatePoint(EditSequence(
                        (point).CoordinateSequence, geometry));
                }

                return geometry;
            }

            /// <summary>
            /// Edits a <see cref="ICoordinateSequence"/> from a <see cref="IGeometry"/>.
            /// </summary>
            ///// <param name="coordSeq">The coordinate array to operate on</param>
            ///// <param name="geometry">The geometry containing the coordinate list</param>
            /// <returns>An edited coordinate sequence (which may be the same as the input)</returns>
            protected Func<ICoordinateSequence, IGeometry, ICoordinateSequence> EditSequence { get; set; }
        }

    }
}