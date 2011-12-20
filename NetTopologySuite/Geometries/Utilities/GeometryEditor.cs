using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A class which supports creating a new <c>Geometry</c> which are modifications of existing ones.
    /// </summary>
    /// <remarks>
    /// Geometry objects are intended to be treated as immutable.
    /// This class allows you to "modifies" a Geometrys
    /// by traversing them, applying a user-defined
    /// <see cref="IGeometryEditorOperation"/> or <see cref="CoordinateOperation"/>
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
    /// The coordinate lists may be changed (e.g. by adding or deleting coordinates).
    /// The modifed coordinate lists must be consistent with their original parent component
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
    /// This class supports the case where an edited Geometry needs to
    /// be created under a new GeometryFactory, via the <see cref="GeometryEditor(IGeometryFactory)"/>
    /// constructor.
    /// Examples of situations where this is required is if the geometry is
    /// transformed to a new SRID and/or a new PrecisionModel.</para>
    /// <para>
    /// The resulting Geometry is not checked for validity.
    /// If validity needs to be enforced, the new Geometry's <see cref="IGeometry.IsValid"/> method should be called.</para>
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
        /// Edit the input <c>Geometry</c> with the given edit operation.
        /// Clients can create subclasses of GeometryEditorOperation or
        /// CoordinateOperation to perform required modifications.
        /// </summary>
        /// <param name="geometry">The Geometry to edit.</param>
        /// <param name="operation">The edit operation to carry out.</param>
        /// <returns>A new <c>Geometry</c> which is the result of the editing.</returns>
        public IGeometry Edit(IGeometry geometry, IGeometryEditorOperation operation)
        {
            // if client did not supply a GeometryFactory, use the one from the input Geometry
            if (_factory == null)
                _factory = geometry.Factory;
            if (geometry is IGeometryCollection)
                return EditGeometryCollection((IGeometryCollection)geometry, operation);
            if (geometry is IPolygon)
                return EditPolygon((IPolygon)geometry, operation);
            if (geometry is IPoint)
                return operation.Edit(geometry, _factory);
            if (geometry is ILineString)
                return operation.Edit(geometry, _factory);
            Assert.ShouldNeverReachHere("Unsupported Geometry classes should be caught in the GeometryEditorOperation.");
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private IPolygon EditPolygon(IPolygon polygon, IGeometryEditorOperation operation)
        {
            IPolygon newPolygon = (IPolygon)operation.Edit(polygon, _factory);
            if (newPolygon.IsEmpty)
                //RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
                return newPolygon;

            ILinearRing shell = (ILinearRing)Edit(newPolygon.ExteriorRing, operation);
            if (shell.IsEmpty)
                //RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
                return _factory.CreatePolygon(null, null);

            List<ILinearRing> holes = new List<ILinearRing>();
            for (int i = 0; i < newPolygon.NumInteriorRings; i++)
            {
                ILinearRing hole = (ILinearRing)Edit(newPolygon.GetInteriorRingN(i), operation);
                if (hole.IsEmpty) continue;
                holes.Add(hole);
            }

            return _factory.CreatePolygon(shell, holes.ToArray());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private IGeometryCollection EditGeometryCollection(IGeometryCollection collection, IGeometryEditorOperation operation)
        {
            IGeometryCollection newCollection = (IGeometryCollection)operation.Edit(collection, _factory);
            IList<IGeometry> geometries = new List<IGeometry>();
            for (int i = 0; i < newCollection.NumGeometries; i++)
            {
                IGeometry geometry = Edit(newCollection.GetGeometryN(i), operation);
                if (geometry.IsEmpty) continue;
                geometries.Add(geometry);
            }

            if (newCollection is IMultiPoint)
                return _factory.CreateMultiPoint(geometries.Cast<IPoint>().ToArray());

            if (newCollection is IMultiLineString)
                return _factory.CreateMultiLineString(geometries.Cast<ILineString>().ToArray());

            if (newCollection is IMultiPolygon)
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
            /// The returned Geometry might be the same as the Geometry passed in.
            /// </summary>
            /// <param name="geometry">The Geometry to modify.</param>
            /// <param name="factory">
            /// The factory with which to construct the modified Geometry
            /// (may be different to the factory of the input point).
            /// </param>
            /// <returns>A new Geometry which is a modification of the input Geometry.</returns>
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
        /// A GeometryEditorOperation which modifies the coordinate list of a <c>Geometry</c>.
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
                    Coordinate[] newCoordinates = Edit(geometry.Coordinates, geometry);
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
    }
}