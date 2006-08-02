using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A framework for processes which transform an input <c>Geometry</c> into
    /// an output <c>Geometry</c>, possibly changing its structure and type(s).
    /// This class is a framework for implementing subclasses
    /// which perform transformations on
    /// various different Geometry subclasses.
    /// It provides an easy way of applying specific transformations
    /// to given point types, while allowing unhandled types to be simply copied.
    /// Also, the framework handles ensuring that if subcomponents change type
    /// the parent geometries types change appropriately to maintain valid structure.
    /// Subclasses will override whichever <c>TransformX</c> methods
    /// they need to to handle particular Geometry types.
    /// A typically usage would be a transformation that may transform Polygons into
    /// Polygons, LineStrings
    /// or Points.  This class would likely need to override the TransformMultiPolygon
    /// method to ensure that if input Polygons change type the result is a GeometryCollection,
    /// not a MultiPolygon.
    /// The default behaviour of this class is to simply recursively transform
    /// each Geometry component into an identical object by copying.
    /// Note that all <c>TransformX</c> methods may return <c>null</c>,
    /// to avoid creating empty point objects. This will be handled correctly
    /// by the transformer.
    /// The Transform method itself will always
    /// return a point object.
    /// </summary>    
    public class GeometryTransformer 
    {
        /*
        * Possible extensions:
        * GetParent() method to return immediate parent e.g. of LinearRings in Polygons
        */

        private Geometry inputGeom = null;

        /// <summary>
        /// 
        /// </summary>
        protected GeometryFactory factory = null;

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
        private bool preserveType = false;

        /// <summary>
        /// 
        /// </summary>
        public GeometryTransformer() { }

        /// <summary>
        /// 
        /// </summary>
        public virtual Geometry InputGeometry
        {
            get
            {
                return inputGeom; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputGeom"></param>
        /// <returns></returns>
        public virtual Geometry Transform(Geometry inputGeom)
        {
            this.inputGeom = inputGeom;
            this.factory = inputGeom.Factory;
            if (inputGeom is Point)
                return TransformPoint((Point) inputGeom, null);
            if (inputGeom is MultiPoint)
                return TransformMultiPoint((MultiPoint) inputGeom, null);
            if (inputGeom is LinearRing)
                return TransformLineString((LinearRing) inputGeom, null);
            if (inputGeom is LineString)
                return TransformLineString((LineString) inputGeom, null);
            if (inputGeom is MultiLineString)
                return TransformMultiLineString((MultiLineString) inputGeom, null);
            if (inputGeom is Polygon)
                return TransformPolygon((Polygon) inputGeom, null);
            if (inputGeom is MultiPolygon)
                return TransformMultiPolygon((MultiPolygon) inputGeom, null);
            if (inputGeom is GeometryCollection)
                return TransformGeometryCollection((GeometryCollection) inputGeom, null);
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
            return factory.CoordinateSequenceFactory.Create(coords);
        }

        /// <summary> 
        /// Convenience method which provides statndard way of copying {CoordinateSequence}s
        /// </summary>
        /// <param name="seq">The sequence to copy.</param>
        /// <returns>A deep copy of the sequence.</returns>
        protected virtual ICoordinateSequence Copy(ICoordinateSequence seq)
        {
            return (ICoordinateSequence) seq.Clone();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual ICoordinateSequence TransformCoordinates(ICoordinateSequence coords, Geometry parent)
        {
            return Copy(coords);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual Geometry TransformPoint(Point geom, Geometry parent) 
        {
            return factory.CreatePoint(TransformCoordinates(geom.CoordinateSequence, geom));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual Geometry TransformMultiPoint(MultiPoint geom, Geometry parent) 
        {
            ArrayList transGeomList = new ArrayList();
            for (int i = 0; i < geom.NumGeometries; i++) 
            {
                Geometry transformGeom = TransformPoint((Point) geom.GetGeometryN(i), geom);
                if (transformGeom == null) continue;
                if (transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            return factory.BuildGeometry(transGeomList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual Geometry TransformLinearRing(LinearRing geom, Geometry parent) 
        {
            ICoordinateSequence seq = TransformCoordinates(geom.CoordinateSequence, geom);
            int seqSize = seq.Count;
            // ensure a valid LinearRing
            if (seqSize > 0 && seqSize < 4 && ! preserveType)
                return factory.CreateLineString(seq);
            return factory.CreateLinearRing(seq);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual Geometry TransformLineString(LineString geom, Geometry parent) 
        {
            // should check for 1-point sequences and downgrade them to points
            return factory.CreateLineString(TransformCoordinates(geom.CoordinateSequence, geom));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual Geometry TransformMultiLineString(MultiLineString geom, Geometry parent) 
        {
            ArrayList transGeomList = new ArrayList();
            for (int i = 0; i < geom.NumGeometries; i++) 
            {
                Geometry transformGeom = TransformLineString((LineString) geom.GetGeometryN(i), geom);
                if (transformGeom == null) continue;
                if (transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            return factory.BuildGeometry(transGeomList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual Geometry TransformPolygon(Polygon geom, Geometry parent) 
        {
            bool isAllValidLinearRings = true;
            Geometry shell = TransformLinearRing((LinearRing) geom.ExteriorRing, geom);

            if (shell == null || ! (shell is LinearRing) || shell.IsEmpty)
                isAllValidLinearRings = false;

            ArrayList holes = new ArrayList();
            for (int i = 0; i < geom.NumInteriorRings; i++) 
            {
                Geometry hole = TransformLinearRing((LinearRing) geom.GetInteriorRingN(i), geom);
                if (hole == null || hole.IsEmpty) continue;            
                if (!(hole is LinearRing))
                    isAllValidLinearRings = false;
                holes.Add(hole);
            }

            if (isAllValidLinearRings)
                return factory.CreatePolygon((LinearRing)   shell, 
                                             (LinearRing[]) holes.ToArray(typeof(LinearRing)));
            else 
            {
                ArrayList components = new ArrayList();
                if (shell != null) 
                    components.Add(shell);                
                foreach (Object hole in holes)
                    components.Add(hole);
                return factory.BuildGeometry(components);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual Geometry TransformMultiPolygon(MultiPolygon geom, Geometry parent) 
        {
            ArrayList transGeomList = new ArrayList();
            for (int i = 0; i < geom.NumGeometries; i++) 
            {
                Geometry transformGeom = TransformPolygon((Polygon) geom.GetGeometryN(i), geom);
                if (transformGeom == null) continue;
                if (transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            return factory.BuildGeometry(transGeomList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual Geometry TransformGeometryCollection(GeometryCollection geom, Geometry parent) 
        {
            ArrayList transGeomList = new ArrayList();
            for (int i = 0; i < geom.NumGeometries; i++) 
            {
                Geometry transformGeom = Transform(geom.GetGeometryN(i));
                if (transformGeom == null) continue;
                if (pruneEmptyGeometry && transformGeom.IsEmpty) continue;
                transGeomList.Add(transformGeom);
            }
            if (preserveGeometryCollectionType)
                return factory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(transGeomList));
            return factory.BuildGeometry(transGeomList);
        }
    }
}
