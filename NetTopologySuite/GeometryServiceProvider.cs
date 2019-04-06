using System;
using System.Threading;

namespace GeoAPI
{
    /// <summary>
    /// Static class that provides access to a  <see cref="IGeometryServices"/> class.
    /// </summary>
    public static class GeometryServiceProvider
    {
        private static volatile IGeometryServices s_instance;

        /// <summary>
        /// Gets or sets the <see cref="IGeometryServices"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when trying to set the value to <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to get the value before it has been set.
        /// </exception>
        public static IGeometryServices Instance
        {
            get => s_instance ?? throw new InvalidOperationException("Cannot use GeometryServiceProvider without an assigned IGeometryServices class");
            set => s_instance = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Sets <see cref="Instance"/> to the given value, unless it has already been set.
        /// </summary>
        /// <param name="instance">
        /// The new value to put into <see cref="Instance"/> if it hasn't already been set directly.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <see cref="Instance"/> was set, <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instance"/> is <see langword="null"/>.
        /// </exception>
        public static bool SetInstanceIfNotAlreadySetDirectly(IGeometryServices instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return Interlocked.CompareExchange(ref s_instance, null, instance) == null;
        }
    }
}
