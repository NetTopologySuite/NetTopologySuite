using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Precision
{
    ///<summary>
    /// Reduces the precision of the {@link Coordinate}s in a
    /// <see cref="ICoordinateSequence"/> to match the supplied <see cref="PrecisionModel"/>.
    ///</summary>
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

        ///<summary>
        /// Creates a new precision reducer filter.
        ///</summary>
        /// <param name="precModel">The PrecisionModel to use</param>
        public CoordinatePrecisionReducerFilter(PrecisionModel precModel)
        {
            _precModel = precModel;
        }

        ///<summary>
        /// Rounds the Coordinates in the sequence to match the PrecisionModel
        ///</summary>
        public void Filter(ICoordinateSequence seq, int i)
        {
            seq.SetOrdinate(i, Ordinates.X, _precModel.MakePrecise(seq.GetOrdinate(i, Ordinates.X)));
            seq.SetOrdinate(i, Ordinates.Y, _precModel.MakePrecise(seq.GetOrdinate(i, Ordinates.Y)));
        }

        ///<summary>
        /// Always runs over all geometry components.
        ///</summary>
        public bool Done { get { return false; } }

        ///<summary>
        /// Always reports that the geometry has changed
        ///</summary>
        public bool GeometryChanged { get { return true; } }
    }
}