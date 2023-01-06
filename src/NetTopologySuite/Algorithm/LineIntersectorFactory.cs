using System;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// A factory that is to be used to create <see cref="LineIntersector"/> instances.
    /// The distinguation is some sort <see cref="Elevation.ElevationModel"/>
    /// </summary>
    /// 
    public static class LineIntersectorFactory
    {
        private static Elevation.ElevationModel _defaultEm;

        /// <summary>
        /// Gets or sets a value indicating the default elevation model to use when none is supplied.
        /// <para/>
        /// If this property is not <c>null</c>
        /// </summary>
        public static Elevation.ElevationModel DefaultElevationModel
        {
            get => _defaultEm;
            set
            {
                if (value is Operation.OverlayNG.ElevationModel)
                    throw new ArgumentException("Elevation model must not be of type NetTopologySuite.Operation.OverlayNG.ElevationModel");

                _defaultEm = value;

            }
        }

        /// <summary>
        /// Creates a <see cref="LineIntersector"/> for the provided elevation model
        /// </summary>
        /// <param name="elevationModel">An elevation model</param>
        /// <returns>A line intersector</returns>
        public static LineIntersector CreateFor(Elevation.ElevationModel elevationModel)
        {
            // See if we have an elevation model that forces us to use the RobustLineIntersectorWithElevationModel
            if (elevationModel != null && !(elevationModel is Operation.OverlayNG.ElevationModel))
                return new RobustLineIntersectorWithElevationModel(elevationModel);

            // Is a default ElevationModel defined, which forces us to use the RobustLineIntersectorWithElevationModel
            var dem = DefaultElevationModel;
            if (dem != null)
                return new RobustLineIntersectorWithElevationModel(dem);

            // Return plain old RobustLineIntersector
            return new RobustLineIntersector();
        }
    }
}
