using System;
using System.Collections.Concurrent;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite
{
    /// <summary>
    /// A geometry service provider class
    /// </summary>
    public class NtsGeometryServices
    {
        private static volatile NtsGeometryServices s_instance = new NtsGeometryServices();

        private readonly ConcurrentDictionary<GeometryFactoryKey, GeometryFactory> m_factories = new ConcurrentDictionary<GeometryFactoryKey, GeometryFactory>();

        /// <summary>
        /// Creates an instance of this class, using the <see cref="CoordinateArraySequenceFactory"/> as default and a <see cref="PrecisionModels.Floating"/> precision model. No <see cref="DefaultSRID"/> is specified
        /// </summary>
        public NtsGeometryServices()
            : this(CoordinateArraySequenceFactory.Instance,
                   new PrecisionModel(PrecisionModels.Floating),
                   -1)
        {
        }

        /// <summary>
        /// Creates an instance of this class, using the provided <see cref="CoordinateSequenceFactory"/>, <see cref="PrecisionModel"/> and spatial reference Id (<paramref name="srid"/>.
        /// </summary>
        /// <param name="coordinateSequenceFactory">The coordinate sequence factory to use.</param>
        /// <param name="precisionModel">The precision model.</param>
        /// <param name="srid">The default spatial reference ID</param>
        public NtsGeometryServices(CoordinateSequenceFactory coordinateSequenceFactory, PrecisionModel precisionModel, int srid)
        {
            DefaultCoordinateSequenceFactory = coordinateSequenceFactory ?? throw new ArgumentNullException(nameof(coordinateSequenceFactory));
            DefaultPrecisionModel = precisionModel ?? throw new ArgumentNullException(nameof(precisionModel));
            DefaultSRID = srid;
        }

        /// <summary>
        /// Gets or sets the default instance of <see cref="NtsGeometryServices"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when trying to set the value to <see langword="null"/>.
        /// </exception>
        public static NtsGeometryServices Instance
        {
            get => s_instance;
            set => s_instance = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets the default spatial reference id
        /// </summary>
        public int DefaultSRID { get; }

        /// <summary>
        /// Gets or sets the coordiate sequence factory to use
        /// </summary>
        public CoordinateSequenceFactory DefaultCoordinateSequenceFactory { get; }

        /// <summary>
        /// Gets or sets the default precision model
        /// </summary>
        public PrecisionModel DefaultPrecisionModel { get; }

        internal int NumFactories => m_factories.Count;

        /// <summary>
        /// Creates a precision model based on given precision model type
        /// </summary>
        /// <returns>The precision model type</returns>
        public PrecisionModel CreatePrecisionModel(PrecisionModels modelType) => new PrecisionModel(modelType);

        /// <summary>
        /// Creates a precision model based on given precision model.
        /// </summary>
        /// <returns>The precision model</returns>
        public PrecisionModel CreatePrecisionModel(PrecisionModel precisionModel) => new PrecisionModel(precisionModel);

        /// <summary>
        /// Creates a precision model based on the given scale factor.
        /// </summary>
        /// <param name="scale">The scale factor</param>
        /// <returns>The precision model.</returns>
        public PrecisionModel CreatePrecisionModel(double scale) => new PrecisionModel(scale);

        public GeometryFactory CreateGeometryFactory() => CreateGeometryFactory(DefaultSRID);

        public GeometryFactory CreateGeometryFactory(int srid) => CreateGeometryFactory(DefaultPrecisionModel, srid, DefaultCoordinateSequenceFactory);

        public GeometryFactory CreateGeometryFactory(CoordinateSequenceFactory coordinateSequenceFactory) => CreateGeometryFactory(DefaultPrecisionModel, DefaultSRID, coordinateSequenceFactory);

        public GeometryFactory CreateGeometryFactory(PrecisionModel precisionModel) => CreateGeometryFactory(precisionModel, DefaultSRID, DefaultCoordinateSequenceFactory);

        public GeometryFactory CreateGeometryFactory(PrecisionModel precisionModel, int srid) => CreateGeometryFactory(precisionModel, srid, DefaultCoordinateSequenceFactory);

        public GeometryFactory CreateGeometryFactory(PrecisionModel precisionModel, int srid, CoordinateSequenceFactory coordinateSequenceFactory)
        {
            if (precisionModel is null)
            {
                throw new ArgumentNullException(nameof(precisionModel));
            }

            if (coordinateSequenceFactory is null)
            {
                throw new ArgumentNullException(nameof(coordinateSequenceFactory));
            }

            return m_factories.GetOrAdd(new GeometryFactoryKey(precisionModel, srid, coordinateSequenceFactory),
                                        key => CreateGeometryFactoryCore(key.PrecisionModel, key.SRID, key.CoordinateSequenceFactory));
        }

        /// <summary>
        /// Creates a <see cref="GeometryFactory"/> based on the given parameters.
        /// </summary>
        /// <param name="precisionModel">
        /// The value for <see cref="GeometryFactory.PrecisionModel"/>.
        /// </param>
        /// <param name="srid">
        /// The value for <see cref="GeometryFactory.SRID"/>.
        /// </param>
        /// <param name="coordinateSequenceFactory">
        /// The value for <see cref="GeometryFactory.CoordinateSequenceFactory"/>.
        /// </param>
        /// <returns>
        /// A <see cref="GeometryFactory"/> that has the given values.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method is expected to be safe to call from any number of threads at once.
        /// </para>
        /// <para>
        /// Although the result for a given set of parameters is cached, there is no guarantee that,
        /// once this method is called with some set of parameters, it will never be called again
        /// with an exactly equal set of parameters.  When this does happen, an arbitrary result is
        /// chosen as the winner (not necessarily the first one to start or finish), and all other
        /// results are discarded.
        /// </para>
        /// </remarks>
        protected virtual GeometryFactory CreateGeometryFactoryCore(PrecisionModel precisionModel, int srid, CoordinateSequenceFactory coordinateSequenceFactory)
        {
            return new GeometryFactory(precisionModel, srid, coordinateSequenceFactory);
        }

        private readonly struct GeometryFactoryKey : IEquatable<GeometryFactoryKey>
        {
            public GeometryFactoryKey(PrecisionModel precisionModel, int srid, CoordinateSequenceFactory factory)
            {
                PrecisionModel = precisionModel;
                CoordinateSequenceFactory = factory;
                SRID = srid;
            }

            public PrecisionModel PrecisionModel { get; }

            public CoordinateSequenceFactory CoordinateSequenceFactory { get; }

            public int SRID { get; }

            public override int GetHashCode() => (PrecisionModel, CoordinateSequenceFactory, SRID).GetHashCode();

            public override bool Equals(object obj) => obj is GeometryFactoryKey other && Equals(other);

            public bool Equals(GeometryFactoryKey other)
            {
                return SRID == other.SRID &&
                       Equals(CoordinateSequenceFactory, other.CoordinateSequenceFactory) &&
                       Equals(PrecisionModel, other.PrecisionModel);
            }
        }
    }
}
