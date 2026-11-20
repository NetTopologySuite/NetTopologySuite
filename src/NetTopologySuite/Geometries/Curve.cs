using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Abstract base class for geometries that have a <c>Dimension</c> of
    /// <see cref="Geometries.Dimension.Curve"/> and consist of <b>only</b> <c>1</c> component.
    /// </summary>
    [Serializable]
    public abstract class Curve : Geometry, ILineal
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="factory">The factory creating this <c>Curve</c></param>
        protected Curve(GeometryFactory factory) : base(factory)
        {
        }

        /// <inheritdoc cref="Geometry.Dimension"/>
        public sealed override Dimension Dimension => Dimension.Curve;


        /// <inheritdoc cref="Geometry.BoundaryDimension"/>
        public override Dimension BoundaryDimension
        {
            get
            {
                if (IsClosed)
                {
                    return Dimension.False;
                }
                return Dimension.Point;
            }
        }

        /// <summary>
        /// Gets a value indicating if this <c>Curve</c> forms a ring.
        /// </summary>
        public bool IsRing { get => IsClosed & IsSimple; }

        /// <summary>
        /// Gets a value indicating that this <c>Curve</c> is closed.<br/>
        /// A curve is closed if <c><see cref="StartPoint"/> == <see cref="EndPoint"/></c>.
        /// </summary>
        public abstract bool IsClosed { get; }

        /// <summary>
        /// Gets a value indicating the start point of this <c>CURVE</c>
        /// </summary>
        public abstract Point StartPoint { get; }

        /// <summary>
        /// Gets a value indicating the end point of this <c>CURVE</c>
        /// </summary>
        public abstract Point EndPoint { get; }
    }
}
