using NetTopologySuite.Geometries;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Reduces the precision of the <see cref="Coordinate"/>s in a
    /// <see cref="CoordinateSequence"/> to match the supplied <see cref="PrecisionModel"/>.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="PrecisionModel.MakePrecise(double)"/>.
    /// The input is modified in-place, so
    /// it should be cloned beforehand if the
    /// original should not be modified.
    /// </remarks>
    /// <author>mbdavis</author>
    public class CoordinatePrecisionReducerFilter : ICoordinateSequenceFilter
    {
        private readonly PrecisionModel _precModel;

        /// <summary>
        /// Creates a new precision reducer filter.
        /// </summary>
        /// <param name="precModel">The PrecisionModel to use</param>
        public CoordinatePrecisionReducerFilter(PrecisionModel precModel)
        {
            _precModel = precModel;
        }

        /// <summary>
        /// Rounds the Coordinates in the sequence to match the PrecisionModel
        /// </summary>
        public void Filter(CoordinateSequence seq, int i)
        {
            seq.SetOrdinate(i, 0, _precModel.MakePrecise(seq.GetOrdinate(i, 0)));
            seq.SetOrdinate(i, 1, _precModel.MakePrecise(seq.GetOrdinate(i, 1)));
        }

        /// <summary>
        /// Always runs over all geometry components.
        /// </summary>
        public bool Done => false;

        /// <summary>
        /// Always reports that the geometry has changed
        /// </summary>
        public bool GeometryChanged => true;
    }
}
