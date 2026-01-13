using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Utility to combine just the <see cref="Envelope"/>s of a list of geometries.
    /// </summary>
    public sealed class EnvelopeCombiner
    {
        /// <summary>
        /// Gets the smallest <see cref="Envelope"/> within which all input geometries fit, or a
        /// <see cref="Envelope.IsNull">null</see> envelope if no non-empty geometries were found in
        /// the input list.
        /// </summary>
        /// <param name="geoms">
        /// The list of input geometries.
        /// </param>
        /// <returns>
        /// The smallest <see cref="Envelope"/> within which all input geometries fit, or a
        /// <see cref="Envelope.IsNull">null</see> envelope if no non-empty geometries were found in
        /// the input list.
        /// </returns>
        public static Envelope Combine(params Geometry[] geoms) => new EnvelopeCombiner(geoms).Combine();

        /// <summary>
        /// Gets the smallest <see cref="Envelope"/> within which all input geometries fit, or a
        /// <see cref="Envelope.IsNull">null</see> envelope if no non-empty geometries were found in
        /// the input list.
        /// </summary>
        /// <param name="geoms">
        /// The list of input geometries.
        /// </param>
        /// <returns>
        /// The smallest <see cref="Envelope"/> within which all input geometries fit, or a
        /// <see cref="Envelope.IsNull">null</see> envelope if no non-empty geometries were found in
        /// the input list.
        /// </returns>
        public static Envelope Combine(IEnumerable<Geometry> geoms) => new EnvelopeCombiner(geoms).Combine();

        /// <summary>
        /// Gets the <see cref="Geometry"/> representation of the result of <see cref="Combine(Geometry[])"/>.
        /// </summary>
        /// <param name="geoms">
        /// The list of input geometries.
        /// </param>
        /// <returns>
        /// The <see cref="Geometry"/> representation of the result of <see cref="Combine(Geometry[])"/>.
        /// </returns>
        public static Geometry CombineAsGeometry(params Geometry[] geoms) => new EnvelopeCombiner(geoms).CombineAsGeometry();

        /// <summary>
        /// Gets the <see cref="Geometry"/> representation of the result of <see cref="Combine(IEnumerable{Geometry})"/>.
        /// </summary>
        /// <param name="geoms">
        /// The list of input geometries.
        /// </param>
        /// <returns>
        /// The <see cref="Geometry"/> representation of the result of <see cref="Combine(IEnumerable{Geometry})"/>.
        /// </returns>
        public static Geometry CombineAsGeometry(IEnumerable<Geometry> geoms) => new EnvelopeCombiner(geoms).CombineAsGeometry();

        private readonly Geometry[] _geoms;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvelopeCombiner"/> class.
        /// </summary>
        /// <param name="geoms">
        /// The <see cref="Geometry"/> instances to combine.
        /// </param>
        public EnvelopeCombiner(params Geometry[] geoms)
        {
            _geoms = geoms ?? Array.Empty<Geometry>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvelopeCombiner"/> class.
        /// </summary>
        /// <param name="geoms">
        /// The <see cref="Geometry"/> instances to combine.
        /// </param>
        public EnvelopeCombiner(IEnumerable<Geometry> geoms)
        {
            switch (geoms)
            {
                case null:
                    _geoms = Array.Empty<Geometry>();
                    break;

                case Geometry[] geomArray:
                    _geoms = geomArray;
                    break;

                default:
                    _geoms = geoms.ToArray();
                    break;
            }
        }

        /// <summary>
        /// Gets the smallest <see cref="Envelope"/> within which all input geometries fit, or a
        /// <see cref="Envelope.IsNull">null</see> envelope if no non-empty geometries were found in
        /// the input list.
        /// </summary>
        /// <returns>
        /// The smallest <see cref="Envelope"/> within which all input geometries fit, or a
        /// <see cref="Envelope.IsNull">null</see> envelope if no non-empty geometries were found in
        /// the input list.
        /// </returns>
        public Envelope Combine()
        {
            var result = new Envelope();
            foreach (var geom in _geoms)
            {
                if (geom is null)
                {
                    continue;
                }

                result.ExpandToInclude(geom.EnvelopeInternal);
            }

            return result;
        }

        /// <summary>
        /// Gets the <see cref="Geometry"/> representation of the result of <see cref="Combine()"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="Geometry"/> representation of the result of <see cref="Combine()"/>.
        /// </returns>
        public Geometry CombineAsGeometry()
        {
            var env = Combine();
            GeometryFactory factory = null;
            foreach (var geom in _geoms)
            {
                if (!(geom is null))
                {
                    factory = geom.Factory;
                    break;
                }
            }

            if (factory is null)
            {
                factory = NtsGeometryServices.Instance.CreateGeometryFactory();
            }

            return factory.ToGeometry(env);
        }
    }
}
