using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    public abstract class MultiCoordinateGeometry<TCoordinate> : Geometry<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private ICoordinateSequence<TCoordinate> _coordinates;

        public MultiCoordinateGeometry(IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
        }

        public override ICoordinateSequence<TCoordinate> Coordinates
        {
            get { return CoordinatesInternal; }
        }

        protected ICoordinateSequence<TCoordinate> CoordinatesInternal
        {
            get { return _coordinates; }
            set
            {
                if (_coordinates != value)
                {
                    if (_coordinates != null)
                    {
                        _coordinates.SequenceChanged -= handleCoordinatesChanged;
                    }

                    _coordinates = value;

                    if (_coordinates != null)
                    {
                        _coordinates.SequenceChanged += handleCoordinatesChanged;
                    }
                }
            }
        }

        public override IEnumerable<TCoordinate> GetVertexes(ITransformMatrix<NPack.DoubleComponent> transform)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<TCoordinate> GetVertexes()
        {
            return _coordinates;
        }

        public override Boolean EqualsExact(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            if (!IsEquivalentClass(g))
            {
                return false;
            }

            MultiCoordinateGeometry<TCoordinate> other = g as MultiCoordinateGeometry<TCoordinate>;
            Debug.Assert(other != null);

            ICoordinateSequence<TCoordinate> otherCoords = other.CoordinatesInternal;

            return CoordinatesInternal.Equals(otherCoords, tolerance);
        }

        public override IGeometry<TCoordinate> Clone()
        {
            MultiCoordinateGeometry<TCoordinate> geom
                = Clone() as MultiCoordinateGeometry<TCoordinate>;

            geom._coordinates = _coordinates.Clone();

            return geom;
        }

        protected virtual void OnCoordinatesChanged()
        {
            ExtentsInternal = null;
        }

        private void handleCoordinatesChanged(Object sender, EventArgs e)
        {
            OnCoordinatesChanged();
        }
    }
}