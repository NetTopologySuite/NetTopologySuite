using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Reprojection
{
    /// <summary>
    /// Abstract base class for lookup of spatial reference definition strings
    /// </summary>
    public abstract class SpatialReferenceFactory
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="coordinateSequenceFactory"></param>
        /// <param name="precisionModel"></param>
        protected SpatialReferenceFactory(CoordinateSequenceFactory coordinateSequenceFactory, PrecisionModel precisionModel)
        {
            CoordinateSequenceFactory = coordinateSequenceFactory;
            PrecisionModel = precisionModel;
        }

        /// <summary>
        /// A string defining what kind of definition is being retrieved.
        /// </summary>
        /// <remarks>The default value is <c>wkt</c>.</remarks>
        public string DefinitionKind { get; protected set; } = "wkt";

        /// <summary>
        /// Gets a cache of spatial references already used
        /// </summary>
        protected Dictionary<int, SpatialReference> SpatialReferences { get; } = new Dictionary<int, SpatialReference>();

        /// <summary>
        /// Gets the factory for <see cref="CoordinateSequence"/>s to use when creating geometry factories for the <see cref="SpatialReference"/> objects
        /// </summary>
        protected CoordinateSequenceFactory CoordinateSequenceFactory { get; }

        /// <summary>
        /// Gets the precision model to use when creating factories for the <see cref="SpatialReference"/> objects.
        /// Reprojected coordinates are made precised with this factory as well.
        /// </summary>
        protected PrecisionModel PrecisionModel { get; }

        /// <summary>
        /// Function to get the spatial reference definition string for the given <paramref name="srid"/> value.
        /// </summary>
        /// <param name="srid">The spatial reference id.</param>
        /// <returns>A</returns>
        public SpatialReference GetSpatialReference(int srid)
        {
            if (SpatialReferences.TryGetValue(srid, out var sr))
                return sr;

            sr = CreateSpatialReference(srid);
            SpatialReferences[srid] = sr;

            return sr;
        }

        /// <summary>
        /// Function to get the spatial reference definition string for the given <paramref name="srid"/> value.
        /// </summary>
        /// <param name="srid">The spatial reference id.</param>
        /// <returns>A</returns>
        public async Task<SpatialReference> GetSpatialReferenceAsync(int srid)
        {
            if (SpatialReferences.TryGetValue(srid, out var sr))
                return sr;

            sr = await CreateSpatialReferenceAsync(srid);
            SpatialReferences[srid] = sr;

            return sr;
        }

        protected abstract SpatialReference CreateSpatialReference(int srid);

        protected abstract Task<SpatialReference> CreateSpatialReferenceAsync(int srid);
    }
}
