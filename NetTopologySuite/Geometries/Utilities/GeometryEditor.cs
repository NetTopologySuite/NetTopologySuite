using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary> 
    /// Supports creating a new <see cref="Geometry{TCoordinate}"/> which is a modification of an existing one.
    /// Geometry objects are intended to be treated as immutable.
    /// This class allows you to "modify" a Geometry
    /// by traversing it and creating a new Geometry with the same overall structure but
    /// possibly modified components.
    /// The following kinds of modifications can be made:
    /// <para>
    /// The values of the coordinates may be changed.
    /// Changing coordinate values may make the result Geometry invalid;
    /// this is not checked by the GeometryEditor.
    /// </para>
    /// <para>The coordinate lists may be changed
    /// (e.g. by adding or deleting coordinates).
    /// The modifed coordinate lists must be consistent with their original parent component
    /// (e.g. a LinearRing must always have at least 4 coordinates, and the first and last
    /// coordinate must be equal).
    /// </para>
    /// <para>Components of the original point may be deleted
    /// (e.g. holes may be removed from a Polygon, or LineStrings removed from a MultiLineString).
    /// Deletions will be propagated up the component tree appropriately.
    /// </para>
    /// Note that all changes must be consistent with the original Geometry's structure
    /// (e.g. a Polygon cannot be collapsed into a LineString).
    /// The resulting Geometry is not checked for validity.
    /// If validity needs to be enforced, the new Geometry's IsValid should be checked.
    /// </summary>    
    public class GeometryEditor<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary> 
        /// The factory used to create the modified Geometry.
        /// </summary>
        private IGeometryFactory<TCoordinate> _factory = null;

        /// <summary> 
        /// Creates a new GeometryEditor object which will create
        /// an edited <see cref="Geometry{TCoordinate}"/> with the same {GeometryFactory} as the input Geometry.
        /// </summary>
        public GeometryEditor() { }

        /// <summary> 
        /// Creates a new GeometryEditor object which will create
        /// the edited Geometry with the given GeometryFactory.
        /// </summary>
        /// <param name="factory">The GeometryFactory to create the edited Geometry with.</param>
        public GeometryEditor(IGeometryFactory<TCoordinate> factory)
        {
            _factory = factory;
        }

        /// <summary> 
        /// Edit the input <see cref="Geometry{TCoordinate}"/> with the given edit 
        /// operation. Clients will create subclasses of GeometryEditorOperation or
        /// CoordinateOperation to perform required modifications.
        /// </summary>
        /// <param name="geometry">The <see cref="Geometry{TCoordinate}"/> to edit.</param>
        /// <param name="operation">The edit operation to carry out.</param>
        /// <returns>
        /// A new <see cref="Geometry{TCoordinate}"/> which is the result of the editing.
        /// </returns>
        public IGeometry<TCoordinate> Edit(IGeometry<TCoordinate> geometry, IGeometryEditorOperation operation)
        {
            // if client did not supply a GeometryFactory, use the one from the input Geometry
            if (_factory == null)
            {
                _factory = geometry.Factory;
            }

            if (geometry is IGeometryCollection<TCoordinate>)
            {
                return editGeometryCollection((IGeometryCollection<TCoordinate>)geometry, operation);
            }

            if (geometry is IPolygon<TCoordinate>)
            {
                return editPolygon((IPolygon<TCoordinate>)geometry, operation);
            }

            if (geometry is IPoint<TCoordinate>)
            {
                return operation.Edit(geometry, _factory);
            }

            if (geometry is ILineString<TCoordinate>)
            {
                return operation.Edit(geometry, _factory);
            }

            Assert.ShouldNeverReachHere("Unsupported Geometry classes should be caught in the GeometryEditorOperation.");
            return null;
        }

        #region Private helper methods
        private IPolygon<TCoordinate> editPolygon(IPolygon<TCoordinate> polygon, IGeometryEditorOperation operation)
        {
            IPolygon<TCoordinate> newPolygon = (IPolygon<TCoordinate>)operation.Edit(polygon, _factory);

            if (newPolygon.IsEmpty)
            {
                //RemoveSelectedPlugIn relies on this behavior. [Jon Aquino]
                return newPolygon;
            }

            ILinearRing<TCoordinate> shell = (ILinearRing<TCoordinate>)Edit(newPolygon.ExteriorRing, operation);

            if (shell.IsEmpty)
            {
                // RemoveSelectedPlugIn relies on this behavior. [Jon Aquino]
                return _factory.CreatePolygon(null, null);
            }

            List<ILinearRing<TCoordinate>> holes 
                = new List<ILinearRing<TCoordinate>>(newPolygon.InteriorRingsCount);

            foreach (ILineString<TCoordinate> ring in newPolygon.InteriorRings)
            {
                ILinearRing<TCoordinate> hole = (ILinearRing<TCoordinate>)Edit(ring, operation);

                if (hole.IsEmpty)
                {
                    continue;
                }

                holes.Add(hole);
            }

            return _factory.CreatePolygon(shell, holes);
        }

        private IGeometryCollection<TCoordinate> editGeometryCollection(IGeometryCollection<TCoordinate> collection,
                                                           IGeometryEditorOperation operation)
        {
            IGeometryCollection<TCoordinate> newCollection = (IGeometryCollection<TCoordinate>)operation.Edit(collection, _factory);
            List<IGeometry<TCoordinate>> editedGeometries = new List<IGeometry<TCoordinate>>();

            foreach (IGeometry<TCoordinate> geometry in editedGeometries)
            {
                IGeometry<TCoordinate> newGeometry = Edit(geometry, operation);

                if (newGeometry.IsEmpty)
                {
                    continue;
                }

                editedGeometries.Add(newGeometry);
            }

            if (newCollection is IMultiPoint)
            {
                IEnumerable<IPoint<TCoordinate>> points =
                    Enumerable.Downcast<IPoint<TCoordinate>, IGeometry<TCoordinate>>(editedGeometries);

                return _factory.CreateMultiPoint(points);
            }

            if (newCollection is IMultiLineString)
            {
                IEnumerable<ILineString<TCoordinate>> lines =
                    Enumerable.Downcast<ILineString<TCoordinate>, IGeometry<TCoordinate>>(editedGeometries);

                return _factory.CreateMultiLineString(lines);
            }

            if (newCollection is IMultiPolygon)
            {
                IEnumerable<IPolygon<TCoordinate>> polygons =
                    Enumerable.Downcast<IPolygon<TCoordinate>, IGeometry<TCoordinate>>(editedGeometries);

                return _factory.CreateMultiPolygon(polygons);
            }

            return _factory.CreateGeometryCollection(editedGeometries);
        }
        #endregion

        /// <summary> 
        /// A interface which specifies an edit operation for Geometries.
        /// </summary>
        public interface IGeometryEditorOperation
        {
            /// <summary>
            /// Edits a Geometry by returning a new Geometry with a modification.
            /// The returned Geometry might be the same as the Geometry passed in.
            /// </summary>
            /// <param name="geometry">The Geometry to modify.</param>
            /// <param name="factory">
            /// The factory with which to construct the modified Geometry
            /// (may be different to the factory of the input point).
            /// </param>
            /// <returns>A new Geometry which is a modification of the input Geometry.</returns>
            IGeometry<TCoordinate> Edit(IGeometry<TCoordinate> geometry, IGeometryFactory<TCoordinate> factory);
        }

        /// <summary>
        /// A GeometryEditorOperation which modifies the coordinate list of a <see cref="Geometry{TCoordinate}"/>.
        /// Operates on Geometry subclasses which contains a single coordinate list.
        /// </summary>      
        public abstract class CoordinateOperation : IGeometryEditorOperation
        {
            public IGeometry<TCoordinate> Edit(IGeometry<TCoordinate> geometry, IGeometryFactory<TCoordinate> factory)
            {
                if (geometry is ILinearRing)
                {
                    return factory.CreateLinearRing(Edit(geometry.Coordinates, geometry));
                }

                if (geometry is ILineString)
                {
                    return factory.CreateLineString(Edit(geometry.Coordinates, geometry));
                }

                if (geometry is IPoint)
                {
                    IEnumerable<TCoordinate> newCoordinates = Edit(geometry.Coordinates, geometry);
                    return factory.CreatePoint(newCoordinates);
                }

                return geometry;
            }

            /// <summary> 
            /// Edits the array of <c>Coordinate</c>s from a <see cref="Geometry{TCoordinate}"/>.
            /// </summary>
            /// <param name="coordinates">The coordinate array to operate on.</param>
            /// <param name="geometry">The point containing the coordinate list.</param>
            /// <returns>An edited coordinate array (which may be the same as the input).</returns>
            public abstract IEnumerable<TCoordinate> Edit(IEnumerable<TCoordinate> coordinates, IGeometry<TCoordinate> geometry);
        }
    }
}