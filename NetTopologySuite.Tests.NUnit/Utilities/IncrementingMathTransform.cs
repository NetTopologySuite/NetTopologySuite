using System;

using ProjNet.CoordinateSystems.Transformations;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    internal sealed class IncrementingMathTransform : MathTransform
    {
        public override int DimSource => throw new NotImplementedException();
        public override int DimTarget => throw new NotImplementedException();
        public override string WKT => throw new NotImplementedException();
        public override string XML => throw new NotImplementedException();

        public override MathTransform Inverse() => throw new NotImplementedException();
        public override void Invert() => throw new NotImplementedException();

        public override void Transform(ref double x, ref double y, ref double z)
        {
            ++x;
            ++y;
            ++z;
        }
    }
}
