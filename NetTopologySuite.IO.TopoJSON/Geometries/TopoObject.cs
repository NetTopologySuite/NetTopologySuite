using NetTopologySuite.Features;

namespace NetTopologySuite.IO.TopoJSON.Geometries
{
    public abstract class TopoObject
    {
        private readonly string _type;

        protected TopoObject(string type)
        {
            _type = type;
        }

        public string Type
        {
            get { return _type; }
        }

        public IAttributesTable Properties { get; set; }

        public override string ToString()
        {
            return string.Format("Type: {0}", Type);
        }
    }
}
