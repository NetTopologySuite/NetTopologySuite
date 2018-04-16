using NetTopologySuite.Features;

namespace NetTopologySuite.IO.Geometries
{
    public abstract class TopoObject
    {
        protected TopoObject(string type)
        {
            Type = type;
        }

        public string Type { get; }

        public long Id { get; set; }

        public IAttributesTable Properties { get; set; }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return string.Format("Type: {0}", Type);
        }
    }
}
