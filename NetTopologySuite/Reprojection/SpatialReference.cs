using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Reprojection
{
    public struct SpatialReference : IEquatable<SpatialReference>
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="definitionKind">A text describing how to treat <paramref name="definition"/></param>
        /// <param name="definition">A definition text of the spatial reference</param>
        /// <param name="factory">A geometry factory that creates geometries for this spatial reference.</param>
        public SpatialReference(string definitionKind, string definition, GeometryFactory factory)
        {
            DefinitionKind = definitionKind;
            Definition = definition;
            Factory = factory;
        }

        /// <summary>
        /// Gets a string describing how to interpret <see cref="Definition"/>.
        /// </summary>
        public string DefinitionKind { get; }

        /// <summary>
        /// Gets a string defining the spatial reference.
        /// </summary>
        public string Definition { get; private set; }

        /// <summary>
        /// Gets a factory creating geometries for this spatial reference
        /// </summary>
        public GeometryFactory Factory { get; }

        bool IEquatable<SpatialReference>.Equals(SpatialReference other)
        {
            return string.Equals(Definition, other.Definition);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SpatialReference other))
                return false;
            return ((IEquatable<SpatialReference>) this).Equals(other);
        }

        public override int GetHashCode()
        {
            return 17 ^ Factory.SRID.GetHashCode();
        }

        public override string ToString()
        {
            return $"SR{Factory.SRID} ({DefinitionKind}: '{Definition}')";
        }
    }
}
