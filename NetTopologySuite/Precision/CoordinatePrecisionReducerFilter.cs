using GeoAPI.Geometries;

namespace NetTopologySuite.Precision
{
    ///<summary>
    /// Reduces the precision of the <see cref="Coordinate"/>s in a
    /// <see cref="ICoordinateSequence"/> to match the supplied <see cref="IPrecisionModel"/>.
    ///</summary>
    /// <remarks>
    /// Uses <see cref="IPrecisionModel.MakePrecise(double)"/>.
    /// The input is modified in-place, so
    /// it should be cloned beforehand if the
    /// original should not be modified.
    /// </remarks>
    /// <author>mbdavis</author>
    public class CoordinatePrecisionReducerFilter : ICoordinateSequenceFilter
    {
        private readonly IPrecisionModel _precModel;

        ///<summary>
        /// Creates a new precision reducer filter.
        ///</summary>
        /// <param name="precModel">The PrecisionModel to use</param>
        public CoordinatePrecisionReducerFilter(IPrecisionModel precModel)
        {
            _precModel = precModel;
        }

        ///<summary>
        /// Rounds the Coordinates in the sequence to match the PrecisionModel
        ///</summary>
        public void Filter(ICoordinateSequence seq, int i)
        {
            seq.SetOrdinate(i, Ordinate.X, _precModel.MakePrecise(seq.GetOrdinate(i, Ordinate.X)));
            seq.SetOrdinate(i, Ordinate.Y, _precModel.MakePrecise(seq.GetOrdinate(i, Ordinate.Y)));
        }

        ///<summary>
        /// Always runs over all geometry components.
        ///</summary>
        public bool Done => false;

        ///<summary>
        /// Always reports that the geometry has changed
        ///</summary>
        public bool GeometryChanged => true;
    }
}