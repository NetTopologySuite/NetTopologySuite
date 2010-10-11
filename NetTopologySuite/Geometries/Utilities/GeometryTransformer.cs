using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NPack.Interfaces;

#if DOTNET35
using sl = System.Linq;
#endif

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A framework for processes which transform an input <see cref="Geometry{TCoordinate}"/> into
    /// an output <see cref="Geometry{TCoordinate}"/>, possibly changing its structure and type(s).
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
    /// The default behavior of this class is to simply recursively transform
    /// each Geometry component into an identical object by copying.
    /// Note that all <c>TransformX</c> methods may return <see langword="null" />,
    /// to avoid creating empty point objects. This will be handled correctly
    /// by the transformer.
    /// The Transform method itself will always
    /// return a point object.
    /// </summary>    
    public class GeometryTransformer<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /*
        * Possible extensions:
        * GetParent() method to return immediate parent e.g. of LinearRings in Polygons
        */

        private IGeometryFactory<TCoordinate> _factory;
        private IGeometry<TCoordinate> _inputGeometry;

        // these could eventually be exposed to clients

        /// <summary> 
        /// <see langword="true"/> if a homogenous collection result
        /// from a <see cref="GeometryCollection{TCoordinate}" /> should still
        /// be a general GeometryCollection.
        /// </summary>
        private Boolean _preserveGeometryCollectionType = true;

        /// <summary> 
        /// <see langword="true"/> if the type of the input should be preserved.
        /// </summary>
        private Boolean _preserveType;

        /// <summary>
        /// <see langword="true"/> if empty geometries should not be included in the result.
        /// </summary>
        private Boolean _pruneEmptyGeometry = true;

        public IGeometry<TCoordinate> InputGeometry
        {
            get { return _inputGeometry; }
        }

        public IGeometry<TCoordinate> Transform(IGeometry<TCoordinate> inputGeom)
        {
            _inputGeometry = inputGeom;
            _factory = inputGeom.Factory;

            if (inputGeom is IPoint<TCoordinate>)
            {
                return TransformPoint((IPoint<TCoordinate>) inputGeom, null);
            }

            if (inputGeom is IMultiPoint<TCoordinate>)
            {
                return TransformMultiPoint((IMultiPoint<TCoordinate>) inputGeom, null);
            }

            if (inputGeom is ILinearRing<TCoordinate>)
            {
                return TransformLinearRing((ILinearRing<TCoordinate>) inputGeom, null);
            }

            if (inputGeom is ILineString<TCoordinate>)
            {
                return TransformLineString((ILineString<TCoordinate>) inputGeom, null);
            }

            if (inputGeom is IMultiLineString<TCoordinate>)
            {
                return TransformMultiLineString((IMultiLineString<TCoordinate>) inputGeom, null);
            }

            if (inputGeom is IPolygon<TCoordinate>)
            {
                return TransformPolygon((IPolygon<TCoordinate>) inputGeom, null);
            }

            if (inputGeom is IMultiPolygon<TCoordinate>)
            {
                return TransformMultiPolygon((IMultiPolygon<TCoordinate>) inputGeom, null);
            }

            if (inputGeom is IGeometryCollection<TCoordinate>)
            {
                return TransformGeometryCollection((IGeometryCollection<TCoordinate>) inputGeom, null);
            }

            throw new ArgumentException(String.Format("Unknown Geometry subtype: {0}",
                                                      inputGeom.GetType()));
        }

        /// <summary> 
        /// Convenience method which provides standard way of
        /// creating a <c>CoordinateSequence</c>.
        /// </summary>
        /// <param name="coords">The coordinate array to copy.</param>
        /// <returns>A coordinate sequence for the array.</returns>
        protected virtual ICoordinateSequence<TCoordinate> CreateCoordinateSequence(IEnumerable<TCoordinate> coords)
        {
            return _factory.CoordinateSequenceFactory.Create(coords);
        }

        protected virtual ICoordinateSequence<TCoordinate> TransformCoordinates(ICoordinateSequence<TCoordinate> coords,
                                                                                IGeometry<TCoordinate> parent)
        {
            return coords.CoordinateSequenceFactory.Create(coords);
        }

        protected virtual IGeometry<TCoordinate> TransformPoint(IPoint<TCoordinate> point, IGeometry<TCoordinate> parent)
        {
            return _factory.CreatePoint(TransformCoordinates(point.Coordinates, point));
        }

        protected virtual IGeometry<TCoordinate> TransformMultiPoint(IMultiPoint<TCoordinate> multipoint,
                                                                     IGeometry<TCoordinate> parent)
        {
            List<IGeometry<TCoordinate>> transGeomList = new List<IGeometry<TCoordinate>>();

            IEnumerable<IPoint<TCoordinate>> points = multipoint;

            foreach (IPoint<TCoordinate> point in points)
            {
                IGeometry<TCoordinate> transformed = TransformPoint(point, multipoint);

                if (transformed == null)
                {
                    continue;
                }

                if (transformed.IsEmpty)
                {
                    continue;
                }

                transGeomList.Add(transformed);
            }

            return _factory.BuildGeometry(transGeomList);
        }

        protected virtual IGeometry<TCoordinate> TransformLinearRing(ILinearRing<TCoordinate> ring,
                                                                     IGeometry<TCoordinate> parent)
        {
            IEnumerable<TCoordinate> transformed = TransformCoordinates(ring.Coordinates, ring);

            LineStringCoordinateEnumerator<TCoordinate> lineEnumerator =
                new LineStringCoordinateEnumerator<TCoordinate>(transformed);

            // ensure a valid LinearRing
            if (!_preserveType && !lineEnumerator.IsValidRing)
            {
                return _factory.CreateLineString(lineEnumerator);
            }

            return _factory.CreateLinearRing(lineEnumerator);
        }

        protected virtual IGeometry<TCoordinate> TransformLineString(ILineString<TCoordinate> line,
                                                                     IGeometry<TCoordinate> parent)
        {
            // should check for 1-point sequences and downgrade them to points
            return _factory.CreateLineString(TransformCoordinates(line.Coordinates, line));
        }

        protected virtual IGeometry<TCoordinate> TransformMultiLineString(IMultiLineString<TCoordinate> multiLine,
                                                                          Geometry<TCoordinate> parent)
        {
            List<IGeometry<TCoordinate>> transGeomList = new List<IGeometry<TCoordinate>>();

            IEnumerable<ILineString<TCoordinate>> lines = multiLine;

            foreach (ILineString<TCoordinate> line in lines)
            {
                IGeometry<TCoordinate> transformed = TransformLineString(line, multiLine);

                if (transformed == null)
                {
                    continue;
                }

                if (transformed.IsEmpty)
                {
                    continue;
                }

                transGeomList.Add(transformed);
            }

            return _factory.BuildGeometry(transGeomList);
        }

        protected virtual IGeometry<TCoordinate> TransformPolygon(IPolygon<TCoordinate> polygon,
                                                                  IGeometry<TCoordinate> parent)
        {
            Boolean areAllValidLinearRings = true;
            IGeometry<TCoordinate> shell = TransformLinearRing(
                polygon.ExteriorRing as ILinearRing<TCoordinate>, polygon);

            if (shell == null || !(shell is ILinearRing) || shell.IsEmpty)
            {
                areAllValidLinearRings = false;
            }

            List<IGeometry<TCoordinate>> holes = new List<IGeometry<TCoordinate>>();

            foreach (ILinearRing<TCoordinate> hole in polygon.InteriorRings)
            {
                Debug.Assert(hole != null);

                IGeometry<TCoordinate> transformed = TransformLinearRing(hole, polygon);

                if (transformed == null || transformed.IsEmpty)
                {
                    continue;
                }

                if (!(transformed is ILinearRing<TCoordinate>))
                {
                    areAllValidLinearRings = false;
                }

                holes.Add(transformed);
            }

            if (areAllValidLinearRings)
            {
                return _factory.CreatePolygon(shell as ILinearRing<TCoordinate>,
                                              Caster.Downcast<ILinearRing<TCoordinate>, IGeometry<TCoordinate>>(holes));
            }
            else
            {
                List<IGeometry<TCoordinate>> components = new List<IGeometry<TCoordinate>>();

                if (shell != null)
                {
                    components.Add(shell);
                }

                foreach (IGeometry<TCoordinate> hole in holes)
                {
                    components.Add(hole);
                }

                return _factory.BuildGeometry(components);
            }
        }

        protected virtual IGeometry<TCoordinate> TransformMultiPolygon(IMultiPolygon<TCoordinate> multiPolygon,
                                                                       IGeometry<TCoordinate> parent)
        {
            List<IGeometry<TCoordinate>> transGeomList = new List<IGeometry<TCoordinate>>();

            foreach (IPolygon<TCoordinate> polygon in multiPolygon)
            {
                IGeometry<TCoordinate> transformed = TransformPolygon(polygon, multiPolygon);

                if (transformed == null)
                {
                    continue;
                }

                if (transformed.IsEmpty)
                {
                    continue;
                }

                transGeomList.Add(transformed);
            }

            return _factory.BuildGeometry(transGeomList);
        }

        protected virtual IGeometry<TCoordinate> TransformGeometryCollection(
            IGeometryCollection<TCoordinate> geometryCollection, IGeometry<TCoordinate> parent)
        {
            List<IGeometry<TCoordinate>> transGeomList = new List<IGeometry<TCoordinate>>();

            foreach (IGeometry<TCoordinate> geometry in geometryCollection)
            {
                IGeometry<TCoordinate> transformGeom = Transform(geometry);

                if (transformGeom == null)
                {
                    continue;
                }

                if (_pruneEmptyGeometry && transformGeom.IsEmpty)
                {
                    continue;
                }

                transGeomList.Add(transformGeom);
            }

            if (_preserveGeometryCollectionType)
            {
                return _factory.CreateGeometryCollection(transGeomList);
            }

            return _factory.BuildGeometry(transGeomList);
        }

        #region Nested type: LineStringCoordinateEnumerator

        private class LineStringCoordinateEnumerator<TCoordinate> : IEnumerable<TCoordinate>
        {
            private readonly IEnumerable<TCoordinate> _coordinates;
            private readonly List<TCoordinate> _ringTestList = new List<TCoordinate>();

            public LineStringCoordinateEnumerator(IEnumerable<TCoordinate> coordinates)
            {
                _coordinates = coordinates;

                foreach (TCoordinate coordinate in coordinates)
                {
                    _ringTestList.Add(coordinate);

                    if (_ringTestList.Count >= 4)
                    {
                        break;
                    }
                }
            }

            public Boolean IsValidRing
            {
                get { return _ringTestList.Count >= 4; }
            }

            #region IEnumerable<TCoordinate> Members

            public IEnumerator<TCoordinate> GetEnumerator()
            {
                foreach (TCoordinate coordinate in _coordinates)
                {
                    yield return coordinate;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #endregion
    }
}