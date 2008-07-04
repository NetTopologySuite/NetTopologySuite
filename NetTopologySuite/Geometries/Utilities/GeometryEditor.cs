using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary> 
    /// Supports creating a new <c>Geometry</c> which is a modification of an existing one.
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
    public class GeometryEditor
    {
        /// <summary> 
        /// The factory used to create the modified Geometry.
        /// </summary>
        private IGeometryFactory factory = null;

        /// <summary> 
        /// Creates a new GeometryEditor object which will create
        /// an edited <c>Geometry</c> with the same {GeometryFactory} as the input Geometry.
        /// </summary>
        public GeometryEditor() { }

        /// <summary> 
        /// Creates a new GeometryEditor object which will create
        /// the edited Geometry with the given GeometryFactory.
        /// </summary>
        /// <param name="factory">The GeometryFactory to create the edited Geometry with.</param>
        public GeometryEditor(IGeometryFactory factory)
        {
            this.factory = factory;
        }

        /// <summary> 
        /// Edit the input <c>Geometry</c> with the given edit operation.
        /// Clients will create subclasses of GeometryEditorOperation or
        /// CoordinateOperation to perform required modifications.
        /// </summary>
        /// <param name="geometry">The Geometry to edit.</param>
        /// <param name="operation">The edit operation to carry out.</param>
        /// <returns>A new <c>Geometry</c> which is the result of the editing.</returns>
        public IGeometry Edit(IGeometry geometry, GeometryEditorOperation operation)
        {
            // if client did not supply a GeometryFactory, use the one from the input Geometry
            if (factory == null)
                factory = geometry.Factory;
            if (geometry is IGeometryCollection) 
                return EditGeometryCollection((IGeometryCollection) geometry, operation);
            if (geometry is IPolygon)
                return EditPolygon((IPolygon) geometry, operation);
            if (geometry is IPoint) 
                return operation.Edit(geometry, factory);
            if (geometry is ILineString) 
                return operation.Edit(geometry, factory);
            Assert.ShouldNeverReachHere("Unsupported Geometry classes should be caught in the GeometryEditorOperation.");
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private IPolygon EditPolygon(IPolygon polygon, GeometryEditorOperation operation) 
        {
            IPolygon newPolygon = (IPolygon) operation.Edit(polygon, factory);
            if (newPolygon.IsEmpty) 
                //RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
                return newPolygon;

            ILinearRing shell = (ILinearRing) Edit(newPolygon.ExteriorRing, operation);
            if (shell.IsEmpty) 
                //RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
                return factory.CreatePolygon(null, null);

            ArrayList holes = new ArrayList();
            for (int i = 0; i < newPolygon.NumInteriorRings; i++) 
            {
                ILinearRing hole = (ILinearRing) Edit(newPolygon.GetInteriorRingN(i), operation);
                if (hole.IsEmpty) continue;
                holes.Add(hole);
            }

            return factory.CreatePolygon(shell, (ILinearRing[]) holes.ToArray(typeof(ILinearRing)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private IGeometryCollection EditGeometryCollection(IGeometryCollection collection, GeometryEditorOperation operation)
        {
            IGeometryCollection newCollection = (IGeometryCollection) operation.Edit(collection, factory);
            ArrayList geometries = new ArrayList();
            for (int i = 0; i < newCollection.NumGeometries; i++) 
            {
                IGeometry geometry = Edit(newCollection.GetGeometryN(i), operation);
                if (geometry.IsEmpty)  continue;
                geometries.Add(geometry);
            }

            if (newCollection is IMultiPoint) 
                return factory.CreateMultiPoint((IPoint[]) geometries.ToArray(typeof(IPoint)));

            if (newCollection is IMultiLineString) 
                return factory.CreateMultiLineString((ILineString[]) geometries.ToArray(typeof(ILineString)));

            if (newCollection is IMultiPolygon)
                return factory.CreateMultiPolygon((IPolygon[]) geometries.ToArray(typeof(IPolygon)));

            return factory.CreateGeometryCollection((IGeometry[]) geometries.ToArray(typeof(IGeometry)));
        }

        /// <summary> 
        /// A interface which specifies an edit operation for Geometries.
        /// </summary>
        public interface GeometryEditorOperation
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
        /// A GeometryEditorOperation which modifies the coordinate list of a <c>Geometry</c>.
        /// Operates on Geometry subclasses which contains a single coordinate list.
        /// </summary>      
        public abstract class CoordinateOperation : GeometryEditorOperation
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
                    ICoordinate[] newCoordinates = Edit(geometry.Coordinates, geometry);
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
            public abstract ICoordinate[] Edit(ICoordinate[] coordinates, IGeometry geometry);
        }
    }
}
