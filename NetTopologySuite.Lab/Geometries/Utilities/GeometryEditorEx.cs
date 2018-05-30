using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A class which supports creating new <see cref="IGeometry"/>s
    /// which are modifications of existing ones,
    /// maintaining the same type structure.
    /// Geometry objects are intended to be treated as immutable.
    /// This class "modifies" Geometrys<see cref="IGeometry"/>s
    /// by traversing them, applying a user-defined
    /// <see cref="GeometryEditor.IGeometryEditorOperation"/>, <see cref="CoordinateSequenceOperation"/> or <see cref="CoordinateOperation"/>
    /// and creating new <see cref="IGeometry"/>s with the same structure but
    /// (possibly) modified components.
    /// <para>
    /// Examples of the kinds of modifications which can be made are:
    /// <list>
    /// <item>the values of the coordinates may be changed.
    /// The editor does not check whether changing coordinate values makes the result Geometry invalid.
    /// </item>
    /// <item>the coordinate lists may be changed (e.g.by adding, deleting or modifying coordinates).
    /// The modified coordinate lists must be consistent with their original parent component
    /// (e.g.a LinearRing must always have at least 4 coordinates, and the first and last coordinate must be equal).
    /// </item>
    /// <item>components of the original geometry may be deleted.
    /// (e.g.holes may be removed from a Polygon, or LineStrings removed from a MultiLineString).
    /// Deletions will be propagated up the component tree appropriately.
    /// </item>
    /// </list>
    /// All changes must be consistent with the original Geometry's structure
    /// (e.g.a Polygon cannot be collapsed into a LineString).
    /// If changing the structure is required, use a <see cref="GeometryTransformer"/>.
    /// </para>
    /// <para>
    /// This class supports creating an edited Geometry
    /// using a different <c>GeometryFactory</c> via the <see cref="GeometryEditorEx"/> constructor.
    /// Examples of situations where this is required is if the geometry is
    /// transformed to a new SRID and / or a new PrecisionModel.
    /// </para>
    /// <para>
    /// <strong> Usage Notes </strong>
    /// <list>
    /// <item>The resulting Geometry is not checked for validity.
    /// If validity needs to be enforced, the new Geometry's
    /// <see cref="IGeometry.IsValid"/> should be called.
    /// </item>
    /// <item>By default the UserData of the input geometry is not copied to the result.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class GeometryEditorEx
    {
        /**
         * The factory used to create the modified Geometry.
         * If <tt>null</tt> the GeometryFactory of the input is used.
         */
        private IGeometryFactory _targetFactory;
        private bool _isUserDataCopied;
        private readonly GeometryEditor.IGeometryEditorOperation _operation;

        /// <summary>
        /// Creates a new GeometryEditor object which will create edited
        /// <see cref="IGeometry"/>s with the same <see cref="IGeometryFactory"/> as the input Geometry.
        /// </summary>
        public GeometryEditorEx()
            : this(new NoOpGeometryOperation()) { }

        /// <summary>
        /// Creates a new GeometryEditor object which will create edited
        /// <see cref="IGeometry"/>s with the given <see cref="IGeometryFactory"/> as the input Geometry.
        /// </summary>
        /// <param name="targetFactory">
        /// The GeometryFactory to create edited Geometrys with.
        /// </param>
        public GeometryEditorEx(IGeometryFactory targetFactory)
            : this(new NoOpGeometryOperation(), targetFactory) { }

        /// <summary>
        /// Creates a GeometryEditor which edits geometries using
        /// a given <see cref="GeometryEditor.IGeometryEditorOperation"/>
        /// and the same <see cref="IGeometryFactory"/> as the input Geometry.
        /// </summary>
        /// <param name="operation">The edit operation to use.</param>
        public GeometryEditorEx(GeometryEditor.IGeometryEditorOperation operation)
        {
            _operation = operation;
        }

        /// <summary>
        /// Creates a GeometryEditor which edits geometries using
        /// a given <see cref="GeometryEditor.IGeometryEditorOperation"/>
        /// and the given <see cref="IGeometryFactory"/>.
        /// </summary>
        /// <param name="operation">The edit operation to use.</param>
        /// <param name="targetFactory">The GeometryFactory to create  edited Geometrys with.</param>
        public GeometryEditorEx(GeometryEditor.IGeometryEditorOperation operation, IGeometryFactory targetFactory)
        {
            _operation = operation;
            _targetFactory = targetFactory;
        }

        /// <summary>
        /// Gets/Sets whether the User Data is copied to the edit result.
        /// </summary>
        /// <remarks>Only the object reference is copied.</remarks>
        /// <value><c>true</c> if the input user data should be copied.</value>
        public bool CopyUserData
        {
            get => _isUserDataCopied;
            set => _isUserDataCopied = value;
        }

        /// <summary>
        /// Edit a <see cref="IGeometry"/>.
        /// </summary>
        /// <remarks>
        /// Clients can create subclasses of <see cref="GeometryEditor.IGeometryEditorOperation"/>,
        /// <see cref="CoordinateSequenceOperation"/> or <see cref="CoordinateOperation"/>
        /// to perform required modifications.
        /// </remarks>
        /// <param name="geometry">The Geometry to edit.</param>
        /// <returns>
        /// A new <see cref="IGeometry"/> which is the result
        /// of the editing (which may be empty).
        /// </returns>
        public IGeometry Edit(IGeometry geometry)
        {
            // nothing to do
            if (geometry == null)
                return null;

            var result = EditInternal(geometry);
            if (_isUserDataCopied)
                result.UserData = geometry.UserData;
            return result;
        }

        private IGeometry EditInternal(IGeometry geometry)
        {
            // if client did not supply a GeometryFactory, use the one from the input Geometry
            if (_targetFactory == null)
                _targetFactory = geometry.Factory;

            if (geometry is IGeometryCollection)
            {
                return EditGeometryCollection((IGeometryCollection)geometry);
            }
            if (geometry is IPolygon)
            {
                return EditPolygon((IPolygon)geometry);
            }
            if (geometry is IPoint)
            {
                return EditPoint((IPoint)geometry);
            }
            if (geometry is ILinearRing)
            {
                return EditLinearRing((ILinearRing)geometry);
            }
            if (geometry is ILineString)
            {
                return EditLineString((ILineString)geometry);
            }

            var err = string.Format("Unsupported Geometry class: {0}", geometry.GetType().Name);
            Assert.ShouldNeverReachHere(err);
            return null;
        }

        private IPoint EditPoint(IPoint geom)
        {
            var newGeom = (IPoint)_operation.Edit(geom, _targetFactory);
            if (newGeom == null)
            {
                // null return means create an empty one
                newGeom = _targetFactory.CreatePoint((ICoordinateSequence)null);
            }
            else if (newGeom == geom)
            {
                // If geometry was not modified, copy it
                newGeom = (IPoint)_targetFactory.CreateGeometry(geom);
            }
            return newGeom;
        }

        private ILineString EditLineString(ILineString geom)
        {
            var newGeom = (ILineString)_operation.Edit(geom, _targetFactory);
            if (newGeom == null)
            {
                // null return means create an empty one
                newGeom = _targetFactory.CreateLineString((ICoordinateSequence)null);
            }
            else if (newGeom == geom)
            {
                // If geometry was not modified, copy it
                newGeom = (ILineString)_targetFactory.CreateGeometry(geom);
            }
            return newGeom;
        }

        private ILinearRing EditLinearRing(ILinearRing geom)
        {
            var newGeom = (ILinearRing)_operation.Edit(geom, _targetFactory);
            if (newGeom == null)
            {
                // null return means create an empty one
                newGeom = _targetFactory.CreateLinearRing((ICoordinateSequence)null);
            }
            else if (newGeom == geom)
            {
                // If geometry was not modified, copy it
                newGeom = (ILinearRing)_targetFactory.CreateGeometry(geom);
            }
            return newGeom;
        }

        private IPolygon EditPolygon(IPolygon polygon)
        {
            var newPolygon = (IPolygon)_operation.Edit(polygon, _targetFactory);
            // create one if needed
            if (newPolygon == null)
            {
                newPolygon = _targetFactory.CreatePolygon();
                return newPolygon;
            }
            /**
             * If geometry was modified, return it
             */
            if (newPolygon != polygon)
            {
                return newPolygon;
            }

            var shell = (ILinearRing)Edit(newPolygon.ExteriorRing);
            if (shell == null || shell.IsEmpty)
            {
                return _targetFactory.CreatePolygon();
            }

            IList<ILinearRing> holes = new List<ILinearRing>(newPolygon.NumInteriorRings);
            for (var i = 0; i < newPolygon.NumInteriorRings; i++)
            {
                var hole = (ILinearRing)Edit(newPolygon.GetInteriorRingN(i));
                if (hole == null || hole.IsEmpty)
                {
                    continue;
                }
                holes.Add(hole);
            }

            return _targetFactory.CreatePolygon(shell, holes.ToArray());
        }

        private IGeometryCollection EditGeometryCollection(IGeometryCollection collection)
        {
            // first edit the entire collection
            // MD - not sure why this is done - could just check original collection?
            var collectionForType = (IGeometryCollection)_operation.Edit(collection, _targetFactory);
            if (collectionForType != collection)
            {
                return collectionForType;
            }

            // edit the component geometries
            var geometries = new List<IGeometry>(collectionForType.NumGeometries);
            for (var i = 0; i < collectionForType.NumGeometries; i++)
            {
                var geometry = Edit(collectionForType.GetGeometryN(i));
                if (geometry == null || geometry.IsEmpty)
                    continue;
                geometries.Add(geometry);
            }

            if (collectionForType is IMultiPoint)
            {
                return _targetFactory.CreateMultiPoint(geometries.Cast<IPoint>().ToArray());
            }
            if (collectionForType is IMultiLineString)
            {
                return _targetFactory.CreateMultiLineString(geometries.Cast<ILineString>().ToArray());
            }
            if (collectionForType is IMultiPolygon)
            {
                return _targetFactory.CreateMultiPolygon(geometries.Cast<IPolygon>().ToArray());
            }
            return _targetFactory.CreateGeometryCollection(geometries.ToArray());
        }

        /// <summary>
        /// A GeometryEditorOperation which does not modify the input geometry.
        /// This can be used for simple changes of GeometryFactory (including PrecisionModel and SRID).
        /// </summary>
        public class NoOpGeometryOperation : GeometryEditor.IGeometryEditorOperation
        {
            public IGeometry Edit(IGeometry geometry, IGeometryFactory targetFactory)
            {
                return geometry;
            }
        }

        /// <summary>
        /// A <see cref="GeometryEditor.IGeometryEditorOperation"/> which edits
        /// the coordinate list of a <see cref="IGeometry"/>.
        /// Operates on <see cref="IGeometry"/> subclasses
        /// which contains a single coordinate list.
        /// </summary>
        public abstract class CoordinateOperation : GeometryEditor.IGeometryEditorOperation
        {
            public IGeometry Edit(IGeometry geometry, IGeometryFactory targetFactory)
            {
                var coordinates = geometry.Coordinates;
                if (geometry is LinearRing)
                {
                    var edit = Edit(coordinates, geometry);
                    return targetFactory.CreateLinearRing(edit);
                }

                if (geometry is LineString)
                {
                    var edit = Edit(coordinates, geometry);
                    return targetFactory.CreateLineString(edit);
                }

                if (geometry is Point)
                {
                    var edit = Edit(coordinates, geometry);
                    var coordinate = edit.Length > 0 ? edit[0] : null;
                    return targetFactory.CreatePoint(coordinate);
                }

                return geometry;
            }

            /// <summary>
            /// Edits the array of <paramref name="coordinates"/>s from a <see cref="IGeometry"/>.
            /// <remarks>
            /// If it is desired to preserve the immutability of Geometries,
            /// if the coordinates are changed a new array should be created
            /// and returned.
            /// </remarks>
            /// </summary>
            /// <param name="coordinates">The coordinate array to operate on.</param>
            /// <param name="geometry">The geometry containing the coordinate list.</param>
            /// <returns>An edited coordinate array (which may be the same as the input).</returns>
            public abstract Coordinate[] Edit(Coordinate[] coordinates, IGeometry geometry);
        }

        /// <summary>
        /// A <see cref="GeometryEditor.IGeometryEditorOperation"/> which edits
        /// the <see cref="ICoordinateSequence"/>.
        /// Operates on <see cref="IGeometry"/> subclasses
        /// which contains a single coordinate list.
        /// </summary>
        public abstract class CoordinateSequenceOperation : GeometryEditor.IGeometryEditorOperation
        {
            public IGeometry Edit(IGeometry geometry, IGeometryFactory targetFactory)
            {
                if (geometry is ILinearRing)
                {
                    var cs = ((ILinearRing)geometry).CoordinateSequence;
                    var edit = Edit(cs, geometry, targetFactory);
                    return targetFactory.CreateLinearRing(edit);
                }

                if (geometry is ILineString)
                {
                    var cs = ((ILineString)geometry).CoordinateSequence;
                    var edit = Edit(cs, geometry, targetFactory);
                    return targetFactory.CreateLineString(edit);
                }

                if (geometry is IPoint)
                {
                    var cs = ((IPoint)geometry).CoordinateSequence;
                    var edit = Edit(cs, geometry, targetFactory);
                    return targetFactory.CreatePoint(edit);
                }

                return geometry;
            }

            // <summary>
            /// Edits a <see cref="ICoordinateSequence"/> from a <see cref="IGeometry"/>.
            /// <param name="coordinateSequence">The coordinate array to operate on.</param>
            /// <param name="geometry">The geometry containing the coordinate list.</param>
            /// <param name="targetFactory"></param>
            /// <returns>An edited coordinate sequence (which may be the same as the input).</returns>
            public abstract ICoordinateSequence Edit(ICoordinateSequence coordinateSequence,
                IGeometry geometry, IGeometryFactory targetFactory);
        }
    }
}