using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Reprojection
{
    public class SpatialReference : IEquatable<SpatialReference>
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="definition"></param>
        public SpatialReference(string definition, GeometryFactory factory)
        {
            Definition = definition;
            Factory = factory;
        }

        public GeometryFactory Factory { get; }

        public virtual string Definition { get; private set; }

        

        bool IEquatable<SpatialReference>.Equals(SpatialReference other)
        {
            if (other == null)
                return false;

            return GetType() == other.GetType() &&
                   string.Equals(Definition, other.Definition);
        }
    }
}