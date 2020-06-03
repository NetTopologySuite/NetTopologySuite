using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Records topological information about an 
    /// edge representing a piece of linework (lineString or polygon ring)
    /// from a single source geometry.
    /// This information is carried through the noding process
    /// (which may result in many noded edges sharing the same information object).
    /// It is then used to populate the topology info fields
    /// in <see cref="Edge"/>s (possibly via merging).
    /// That information is used to construct the topology graph <see cref="OverlayLabel"/>s.
    /// </summary>
    /// <autor>Martin Davis</autor>
    internal class EdgeSourceInfo
    {
        private int index;
        private Dimension dim;// = -999;
        private bool isHole;
        private int depthDelta;

        public EdgeSourceInfo(int index, int depthDelta, bool isHole)
        {
            this.index = index;
            this.dim = Dimension.Surface;
            this.depthDelta = depthDelta;
            this.isHole = isHole;
        }

        public EdgeSourceInfo(int index)
        {
            this.index = index;
            this.dim = Dimension.Curve;
            this.isHole = false;
            this.depthDelta = 0;
        }

        public int Index
        {
            get => index;
        }

        public Dimension Dimension
        {
            get => dim;
        }
        public int DepthDelta
        {
            get => depthDelta;
        }

        public bool IsHole
        {
            get => isHole;
        }

        public override string ToString()
        {
            return Edge.InfoString(index, dim, isHole, depthDelta);
        }
    }
}
