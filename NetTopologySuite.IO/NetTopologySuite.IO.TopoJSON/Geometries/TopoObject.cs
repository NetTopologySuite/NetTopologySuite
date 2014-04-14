using NetTopologySuite.Features;

namespace NetTopologySuite.IO.Geometries
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

        public long Id { get; set; }

        public IAttributesTable Properties { get; set; }        

        public override string ToString()
        {
            return string.Format("Type: {0}", Type);
        }
    }
}
