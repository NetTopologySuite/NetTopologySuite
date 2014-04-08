namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    public class Transform
    {
        private readonly double[] _scale;
        private readonly double[] _translate;

        public Transform(double[] scale, double[] translate)
        {
            this._scale = scale;
            this._translate = translate;
        }

        public double[] Scale
        {
            get { return _scale; }
        }

        public double[] Translate
        {
            get { return _translate; }
        }

        public override string ToString()
        {
            return string.Format("Scale: {0}, Translate: {1}", Scale, Translate);
        }
    }
}