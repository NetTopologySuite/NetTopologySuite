using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// An implementation of <see cref="ICoordinateFilter"/> that delegates the
    /// filter action to a provided method.
    /// </summary>
    public class CoordinateFilter : ICoordinateFilter
    {
        private readonly Action<Coordinate> _coordFilterAction;

        /// <summary>
        /// Creates an instance of this class providing the action
        /// to perform on any coordinate.
        /// </summary>
        /// <param name="coordFilterAction">The action</param>
        /// <exception cref="ArgumentNullException"> thrown if <paramref name="coordFilterAction"/> is <c>null</c>.</exception>
        public CoordinateFilter(Action<Coordinate> coordFilterAction)
        {
            if (coordFilterAction == null)
                throw new ArgumentNullException(nameof(coordFilterAction));
            _coordFilterAction = coordFilterAction; 
        }

        /// <inheritdoc/>
        public void Filter(Coordinate coord)
        {
            _coordFilterAction(coord);
        }
    }
}
