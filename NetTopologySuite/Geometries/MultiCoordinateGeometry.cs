using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    public abstract class MultiCoordinateGeometry<TCoordinate> : Geometry<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private ICoordinateSequence<TCoordinate> _coordinates;

        public MultiCoordinateGeometry(IGeometryFactory<TCoordinate> factory)
            : base(factory) { }

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
