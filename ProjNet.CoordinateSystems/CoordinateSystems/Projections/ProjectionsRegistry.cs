using System;
using System.Collections.Generic;
using System.Globalization;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;

namespace ProjNet.CoordinateSystems.Projections
{
    /// <summary>
    /// Registry class for all known <see cref="MapProjection"/>s.
    /// </summary>
    public class ProjectionsRegistry
    {
        private static readonly Dictionary<string, Type> TypeRegistry = new Dictionary<string, Type>();
        private static readonly Dictionary<string, int> ConstructorRegistry = new Dictionary<string, int>();

        private static readonly object RegistryLock = new object();

        /// <summary>
        /// Static constructor
        /// </summary>
        static ProjectionsRegistry()
        {
            Register("mercator", typeof(Mercator));
            Register("mercator_1sp", typeof (Mercator));
            Register("mercator_2sp", typeof (Mercator));
            Register("pseudo-mercator", typeof(PseudoMercator));
            Register("popular visualisation pseudo-mercator", typeof(PseudoMercator));
            Register("google_mercator", typeof(PseudoMercator));
			
            Register("transverse_mercator", typeof(TransverseMercator));

            Register("albers", typeof(AlbersProjection));
			Register("albers_conic_equal_area", typeof(AlbersProjection));

			Register("krovak", typeof(KrovakProjection));

			Register("polyconic", typeof(PolyconicProjection));
			
            Register("lambert_conformal_conic", typeof(LambertConformalConic2SP));
			Register("lambert_conformal_conic_2sp", typeof(LambertConformalConic2SP));
			Register("lambert_conic_conformal_(2sp)", typeof(LambertConformalConic2SP));

            Register("cassini_soldner", typeof(CassiniSoldnerProjection));
        }

        /// <summary>
        /// Method to register a new Map
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public static void Register(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (type == null)
                throw new ArgumentNullException("type");

            if (!typeof(IMathTransform).IsAssignableFrom(type))
                throw new ArgumentException("The provided type does not implement 'GeoAPI.CoordinateSystems.Transformations.IMathTransform'!", "type");

            var ci = CheckConstructor(type);
            if (ci == 0)
                throw new ArgumentException("The provided type is lacking a suitable constructor", "type");

            var key = name.ToLower(CultureInfo.InvariantCulture).Replace(' ', '_');
            lock (RegistryLock)
            {
                if (TypeRegistry.ContainsKey(key))
                {
                    var rt = TypeRegistry[key];
                    if (ReferenceEquals(type, rt))
                        return;
                    throw new ArgumentException("A different projection type has been registered with this name", "name");
                }

                TypeRegistry.Add(key, type);
                ConstructorRegistry.Add(key, ci);
            }
        }

        private static int CheckConstructor(Type type)
        {
            var c = type.GetConstructor(new[] { typeof(IEnumerable<ProjectionParameter>) });
            if (c != null)
                return 1;

            c = type.GetConstructor(new[] { typeof(List<ProjectionParameter>) });
            if (c != null)
                return 2;

            c = type.GetConstructor(new[] { typeof(IList<ProjectionParameter>) });
            if (c != null)
                return 3;
            
            c = type.GetConstructor(new[] { typeof(ICollection<ProjectionParameter>) });
            return c != null ? 4 : 0;
        }

        internal static IMathTransform CreateProjection(string className, IEnumerable<ProjectionParameter> parameters)
        {
            var key = className.ToLower(CultureInfo.InvariantCulture).Replace(' ', '_');

            Type projectionType;
            int ci;

            lock (RegistryLock)
            {
                if (!TypeRegistry.TryGetValue(key, out projectionType))
                    throw new NotSupportedException(String.Format("Projection {0} is not supported.", className));
                ci = ConstructorRegistry[key];
            }

            switch (ci)
            {
                case 1:
                    return (IMathTransform) Activator.CreateInstance(projectionType, parameters);
                case 2:
                    var l = parameters as List<ProjectionParameter> ?? new List<ProjectionParameter>(parameters);
                    return (IMathTransform)Activator.CreateInstance(projectionType, l);
                case 3:
                    var il = parameters as IList<ProjectionParameter> ?? new List<ProjectionParameter>(parameters);
                    return (IMathTransform)Activator.CreateInstance(projectionType, il);
                case 4:
                    var ic = parameters as ICollection<ProjectionParameter> ?? new List<ProjectionParameter>(parameters);
                    return (IMathTransform)Activator.CreateInstance(projectionType, ic);
            }

            throw new NotSupportedException("Should never reach here!");
        }
    }
}