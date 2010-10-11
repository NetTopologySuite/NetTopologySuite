using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NPack;
using NPack.Interfaces;

#if DOTNET40
//using Processor = Transform
#else
#endif

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Allow computing and removing common significand bits from one or more 
    /// <see cref="IGeometry{TCoordinate}"/> instances.
    /// </summary>
    public class CommonBitsRemover<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private TCoordinate _commonCoordinate;
        private readonly IGeometryFactory<TCoordinate> _geomFactory;
        private readonly CommonCoordinateFilter _filter;

        public CommonBitsRemover(IGeometryFactory<TCoordinate> coordFactory)
        {
            _geomFactory = coordFactory;
            _filter = new CommonCoordinateFilter(_geomFactory.CoordinateFactory);
        }

        /// <summary>
        /// Add a point to the set of geometries whose common bits are
        /// being computed.  After this method has executed the
        /// common coordinate reflects the common bits of all added
        /// geometries.
        /// </summary>
        /// <param name="geom">A Geometry to test for common bits.</param>
        public void Add(IGeometry<TCoordinate> geom)
        {
            _filter.ApplyTo(geom);
            _commonCoordinate = _filter.CommonCoordinate;
        }

        /// <summary>
        /// The common bits of the Coordinates in the supplied Geometries.
        /// </summary>
        public TCoordinate CommonCoordinate
        {
            get { return _commonCoordinate; }
        }

        /// <summary>
        /// Removes the common coordinate bits from a Geometry.
        /// The coordinates of the Geometry are changed.
        /// </summary>
        /// <param name="geom">The Geometry from which to remove the common coordinate bits.</param>
        /// <returns>The shifted Geometry.</returns>
        public IGeometry<TCoordinate> RemoveCommonBits(IGeometry<TCoordinate> geom)
        {
            if (_commonCoordinate[Ordinates.X] == 0.0 && _commonCoordinate[Ordinates.Y] == 0.0)
            {
                return geom;
            }

            TCoordinate invCoord = _geomFactory.CoordinateFactory.Create(-_commonCoordinate[Ordinates.X], -_commonCoordinate[Ordinates.Y]);
            Translater trans = new Translater(_geomFactory, invCoord);
            return trans.ApplyTo(geom);
        }

        /// <summary>
        /// Adds the common coordinate bits back into a Geometry.
        /// The coordinates of the Geometry are changed.
        /// </summary>
        /// <param name="geom">The Geometry to which to add the common coordinate bits.</param>
        /// <returns>The shifted Geometry.</returns>
        public IGeometry<TCoordinate> AddCommonBits(IGeometry<TCoordinate> geom)
        {
            Translater trans = new Translater(_geomFactory, _commonCoordinate);
            return trans.ApplyTo(geom);
        }

        public class CommonCoordinateFilter //: ICoordinateFilter<TCoordinate>
        {
            private readonly ICoordinateFactory<TCoordinate> _coordFact;
            private readonly CommonBits _commonBitsX = new CommonBits();
            private readonly CommonBits _commonBitsY = new CommonBits();
            private readonly CommonBits _commonBitsZ = new CommonBits();
            private readonly CommonBits _commonBitsM = new CommonBits();
            private Boolean _hasZ = false;
            private Boolean _hasM = false;

            public CommonCoordinateFilter(ICoordinateFactory<TCoordinate> coordFact)
            {
                _coordFact = coordFact;
            }

            public void ApplyTo(IGeometry<TCoordinate>geom)
            {
                foreach (TCoordinate coordinate in geom.Coordinates)
                    Filter(coordinate);
            }

            private void Filter(TCoordinate coord)
            {
                _commonBitsX.Add(coord[Ordinates.X]);
                _commonBitsY.Add(coord[Ordinates.Y]);

                if (_hasZ || coord.ContainsOrdinate(Ordinates.Z))
                {
                    _hasZ = true;
                    _commonBitsZ.Add(coord[Ordinates.Z]);
                }

                if (_hasM || coord.ContainsOrdinate(Ordinates.M))
                {
                    _hasM = true;
                    _commonBitsM.Add(coord[Ordinates.M]);
                }
            }

            public TCoordinate CommonCoordinate
            {
                get { return _coordFact.Create(_commonBitsX.Common, _commonBitsY.Common); }
            }
        }

        private class Translater// : ICoordinateFilter<TCoordinate>
        {
            private readonly TCoordinate _trans;
            private readonly IGeometryFactory<TCoordinate> _geomFact;

            public Translater(IGeometryFactory<TCoordinate> geomFact, TCoordinate trans)
            {
                _geomFact = geomFact;
                _trans = trans;
            }

            public IGeometry<TCoordinate> ApplyTo(IGeometry<TCoordinate> geom)
            {
                if ( geom is IPoint<TCoordinate> )
                    return _geomFact.CreatePoint(AddTrans(geom.Coordinates[0]));

                if ( geom is IMultiPoint<TCoordinate>)
                    return _geomFact.CreateMultiPoint(Processor.Transform<TCoordinate>(geom.Coordinates, AddTrans));

                if ( geom is ILineString<TCoordinate> )
                    return _geomFact.CreateLineString(Processor.Transform<TCoordinate>(geom.Coordinates, AddTrans));

                if (geom is IMultiLineString<TCoordinate>)
                {
                    IMultiLineString<TCoordinate> ms = _geomFact.CreateMultiLineString();
                    foreach (ILineString<TCoordinate> line in (IGeometryCollection<TCoordinate>) geom)
                        ms.Add(ApplyTo(line));
                    return ms;
                }

                if (geom is IPolygon<TCoordinate>)
                {
                    IPolygon<TCoordinate> polyInput = geom as IPolygon<TCoordinate>;
                    IPolygon<TCoordinate> poly =
                        _geomFact.CreatePolygon(
                            _geomFact.CreateLinearRing(
                                Processor.Transform<TCoordinate>(polyInput.ExteriorRing.Coordinates, AddTrans)),
                                Processor.Transform(polyInput.InteriorRings, AddTrans));

                    return poly;
                }
                if (geom is IMultiPolygon<TCoordinate>)
                {
                    IMultiPolygon<TCoordinate> ms = _geomFact.CreateMultiPolygon();
                    foreach (IPolygon<TCoordinate> line in (IGeometryCollection<TCoordinate>)geom)
                        ms.Add(ApplyTo(line));
                    return ms;
                }

                if (geom is IGeometryCollection<TCoordinate>)
                {
                    IGeometryCollection<TCoordinate> gc = _geomFact.CreateGeometryCollection();
                    foreach (IGeometry<TCoordinate> geometry in (IGeometryCollection<TCoordinate>)geom)
                        gc.Add(ApplyTo(geometry));
                    return gc;
                }

                throw  new Exception("Should never reach here.");

            }

            private TCoordinate AddTrans(TCoordinate input)
            {
                return input.Add(_trans);
            }

            private ILinearRing<TCoordinate> AddTrans(ILineString<TCoordinate> input)
            {
                return _geomFact.CreateLinearRing(Processor.Transform<TCoordinate>(input.Coordinates, AddTrans));
            }

            private class AddDeltaOperation : GeometryEditor<TCoordinate>.CoordinateOperation
            {
                private readonly ICoordinateFactory<TCoordinate> _coordFact;
                private readonly TCoordinate _delta;

                public AddDeltaOperation(ICoordinateFactory<TCoordinate> coordFact, TCoordinate delta)
                {
                    _coordFact = coordFact;
                    _delta = delta;
                }

                public override IEnumerable<TCoordinate> Edit(IEnumerable<TCoordinate> coordinates, IGeometry<TCoordinate> geometry)
                {
                    foreach (TCoordinate coordinate in coordinates)
                        yield return coordinate.Add(_delta);
                }
            }
        }
    }
}
